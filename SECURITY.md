# 🔒 SECURITY.md — TaskManager

> Documento técnico de segurança do projeto **TaskManager POC**. Mantido pelo **Security Review Assistant** — agente customizado do GitHub Copilot definido em `.github/copilot-instructions.md`.

---

## 🗂️ Índice

- [Postura de Segurança Atual](#-postura-de-segurança-atual)
- [Superfície de Ataque](#-superfície-de-ataque)
- [Controles Implementados](#-controles-implementados)
- [Vulnerabilidades Conhecidas](#-vulnerabilidades-conhecidas)
- [Matriz de Risco](#-matriz-de-risco)
- [Security Review Assistant](#-security-review-assistant)
- [Checklist de Segurança por Camada](#-checklist-de-segurança-por-camada)
- [Dependências e CVEs](#-dependências-e-cves)
- [Recomendações Prioritárias](#-recomendações-prioritárias)

---

## 📊 Postura de Segurança Atual

```
┌─────────────────────────────────────────────────────────────────┐
│                  SECURITY POSTURE OVERVIEW                      │
├────────────────────┬────────────────────┬───────────────────────┤
│  CAMADA            │  STATUS            │  NÍVEL DE RISCO       │
├────────────────────┼────────────────────┼───────────────────────┤
│  Transporte (TLS)  │ ⚠️ HTTPS desabili. │ 🔴 ALTO               │
│  Autenticação      │ ❌ Não implementado │ 🔴 ALTO               │
│  Autorização       │ ⚠️ Parcial          │ 🟡 MÉDIO              │
│  Validação Entrada │ ✅ Parcial          │ 🟡 MÉDIO              │
│  CSRF Protection   │ ✅ Implementado     │ 🟢 BAIXO              │
│  SQL Injection     │ ✅ EF Core (ORM)    │ 🟢 BAIXO              │
│  XSS               │ ⚠️ Parcial (Razor)  │ 🟡 MÉDIO              │
│  Logging           │ ⚠️ Sem PII guard    │ 🟡 MÉDIO              │
│  Secrets Mgmt      │ ❌ Config em repo   │ 🔴 ALTO               │
│  Deps / CVEs       │ ⚠️ Sem scanning     │ 🟡 MÉDIO              │
└────────────────────┴────────────────────┴───────────────────────┘
```

**Nível de Risco Geral: 🟡 MÉDIO — tendendo a ALTO**

---

## 🎯 Superfície de Ataque

```
                     INTERNET / USUÁRIO
                           │
                    ┌──────▼──────┐
                    │  HTTP/HTTPS │  ← ⚠️ HTTPS desabilitado
                    └──────┬──────┘
                           │
              ┌────────────▼────────────┐
              │      ASP.NET Core       │
              │  ┌─────────────────┐    │
              │  │  TasksController│ ←──┼── Entrada de dados (Title, Desc...)
              │  │  HomeController │    │   ValidateAntiForgeryToken ✅
              │  └────────┬────────┘    │
              │           │             │
              │  ┌────────▼────────┐    │
              │  │   TaskService   │    │   ← Sem validação de negócio extra
              │  └────────┬────────┘    │
              │           │             │
              │  ┌────────▼────────┐    │
              │  │ TaskRepository  │    │   ← EF Core parametrizado ✅
              │  └────────┬────────┘    │
              └───────────┼─────────────┘
                          │
                   ┌──────▼──────┐
                   │  SQL Server │  ← LocalDB (só Windows)
                   │  (LocalDB)  │    String de conexão em appsettings ⚠️
                   └─────────────┘
```

---

## ✅ Controles Implementados

### Controles Ativos

| Controle | Implementação | Arquivo | Status |
|---|---|---|---|
| **Anti-CSRF** | `[ValidateAntiForgeryToken]` em todos os POST | `TasksController.cs` | ✅ Ativo |
| **Validação de Modelo** | `[Required]`, `[StringLength]`, `ModelState.IsValid` | `TaskItem.cs`, controller | ✅ Ativo |
| **SQL Injection** | EF Core parametriza todas as queries | `TaskRepository.cs` | ✅ Ativo |
| **XSS (Razor)** | Razor escapa HTML por padrão em `@Model.X` | Views `*.cshtml` | ✅ Parcial |
| **Logging Estruturado** | `ILogger<T>` com contexto de erro | `TasksController.cs` | ✅ Ativo |
| **Timestamps Automáticos** | `CreatedAt`/`UpdatedAt` gerenciados pelo DbContext | `TaskManagerDbContext.cs` | ✅ Ativo |
| **Limite de Tamanho** | `Title ≤ 200`, `Description ≤ 2000`, `AssignedTo ≤ 100` | `TaskItem.cs` | ✅ Ativo |
| **Error Handler** | `UseExceptionHandler("/Home/Error")` em produção | `Program.cs` | ✅ Ativo |
| **EnsureCreated Guard** | Só executa em `IsDevelopment()` | `Program.cs` | ✅ Ativo |

### Fluxo de Validação CSRF

```
Usuário preenche formulário
         │
         ▼
  Form gerado com token oculto
  <input name="__RequestVerificationToken" ...>
         │
         ▼ POST
  [ValidateAntiForgeryToken] verifica token
         │
    ┌────┴────┐
    │         │
  Válido   Inválido
    │         │
    ▼         ▼
  Processa  400 Bad Request
```

---

## ⚠️ Vulnerabilidades Conhecidas

### [V-01] HTTPS Redirect Desabilitado

```
Arquivo:  src/TaskManager.Web/Program.cs
Linha:    ~38 — "// app.UseHttpsRedirection();"
Risco:    🔴 ALTO
```

**Impacto:** Dados trafegam em texto plano. Credenciais, tokens de sessão e dados de tarefas ficam expostos a ataques Man-in-the-Middle (MitM).

**Correção:**
```csharp
// Remover o comentário:
app.UseHttpsRedirection();
app.UseHsts(); // Adicionar também
```

---

### [V-02] Sem Autenticação

```
Arquivo:  src/TaskManager.Web/Program.cs
Risco:    🔴 ALTO
```

**Impacto:** Qualquer usuário na rede pode acessar, criar, editar e excluir tarefas. O campo `UserId` é hardcoded como `"default-user"` no controller.

```csharp
// TasksController.cs linha ~70 — UserId hardcoded:
task.UserId = "default-user"; // ← Risco de segurança
```

**Correção sugerida:**
```csharp
// Adicionar ASP.NET Core Identity ou Azure AD B2C
builder.Services.AddAuthentication()
    .AddCookie();
builder.Services.AddAuthorization();

// No controller:
task.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? throw new UnauthorizedAccessException();
```

---

### [V-03] String de Conexão em Repositório (appsettings.json)

```
Arquivo:  src/TaskManager.Web/appsettings.json
          src/TaskManager.Web/appsettings.Development.json
Risco:    🔴 ALTO (se repositório for público)
```

**Impacto:** Credenciais de banco de dados versionadas junto ao código. Em repositórios públicos, isso expõe o banco imediatamente.

**Correção:**
```bash
# Usar User Secrets em desenvolvimento:
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."

# Em produção, usar variável de ambiente:
export ConnectionStrings__DefaultConnection="Server=prod..."
```

---

### [V-04] Ausência de Headers de Segurança HTTP

```
Arquivo:  src/TaskManager.Web/Program.cs
Risco:    🟡 MÉDIO
```

**Impacto:** Sem `Content-Security-Policy`, `X-Frame-Options`, `X-Content-Type-Options` — a aplicação é vulnerável a Clickjacking e injeção de conteúdo.

**Correção:**
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'"
    );
    await next();
});
```

---

### [V-05] PII (Dados Pessoais) em Logs

```
Arquivo:  src/TaskManager.Web/Controllers/TasksController.cs
Risco:    🟡 MÉDIO
```

**Impacto:** Logs com dados de tarefas ou identificadores de usuário podem violar LGPD/GDPR se persistidos ou acessados por terceiros.

**Correção:**
```csharp
// ❌ Evitar: logar dados do usuário
_logger.LogError("Erro para usuário {UserId}: {Email}", userId, email);

// ✅ Correto: logar apenas contexto técnico
_logger.LogError(ex, "Erro ao processar tarefa {TaskId}", task.Id);
```

---

### [V-06] UserId Hardcoded no Controller

```
Arquivo:  src/TaskManager.Web/Controllers/TasksController.cs
Linha:    ~70
Risco:    🟡 MÉDIO
```

**Impacto:** Todas as tarefas são atribuídas ao mesmo usuário fictício `"default-user"`, impossibilitando isolamento de dados por usuário e controle de acesso.

---

### [V-07] Cobertura de Testes Insuficiente (Branch Coverage)

```
Arquivo:  tests/TaskManager.Web.Tests/
Risco:    🟡 MÉDIO
```

**Impacto:** Branch coverage de ~32% significa que a maioria dos caminhos de erro e casos extremos não são testados, podendo ocultar vulnerabilidades.

---

## 📊 Matriz de Risco

```
  IMPACTO
    │
 5  │                        [V-01]
    │                  [V-02]
 4  │            [V-03]
    │
 3  │        [V-05] [V-06]
    │    [V-04]
 2  │                 [V-07]
    │
 1  │
    └────────────────────────────── PROBABILIDADE
         1    2    3    4    5

Risco = Impacto × Probabilidade
🔴 ALTO (≥12): V-01, V-02, V-03
🟡 MÉDIO (6-11): V-04, V-05, V-06, V-07
🟢 BAIXO (<6): CSRF, SQL Injection
```

| ID | Vulnerabilidade | Impacto | Probabilidade | Risco | Prioridade |
|---|---|---|---|---|---|
| V-01 | HTTPS desabilitado | 5 | 4 | 🔴 20 | P1 |
| V-02 | Sem autenticação | 5 | 5 | 🔴 25 | P1 |
| V-03 | Secrets no repo | 4 | 3 | 🔴 12 | P1 |
| V-04 | Sem headers HTTP | 3 | 3 | 🟡 9 | P2 |
| V-05 | PII em logs | 3 | 2 | 🟡 6 | P2 |
| V-06 | UserId hardcoded | 3 | 5 | 🟡 15 | P2 |
| V-07 | Branch coverage 32% | 2 | 3 | 🟡 6 | P3 |

---

## 🤖 Security Review Assistant

O repositório possui um agente customizado do GitHub Copilot para revisão automática de segurança.

```
Localização: .github/copilot-instructions.md
Ativação:    Automática em todo workspace do repositório
```

### Como usar

No **Copilot Chat** (`Ctrl+Shift+I`):

```
Revise este código como Security Review Assistant:

[cole o código C# aqui]
```

### Formato de resposta do agente

```
┌─────────────────────────────────────────────────────┐
│  1. Resumo de Segurança                             │
│  2. Problemas Encontrados                           │
│     [PROBLEMA-N] | Localização | Descrição | Impacto│
│  3. Nível de Risco: Baixo / Médio / Alto            │
│  4. Recomendações (priorizadas)                     │
│  5. Exemplo de Correção (somente leitura)           │
└─────────────────────────────────────────────────────┘
```

---

## ✅ Checklist de Segurança por Camada

### Camada de Transporte

- [ ] `app.UseHttpsRedirection()` habilitado
- [ ] `app.UseHsts()` configurado para produção
- [ ] Certificado TLS válido em produção

### Camada de Autenticação e Autorização

- [ ] ASP.NET Core Identity ou provedor externo (Azure AD, etc.)
- [ ] `[Authorize]` nos controllers que exigem login
- [ ] `UseAuthentication()` antes de `UseAuthorization()`
- [ ] UserId obtido de `User.Claims`, não hardcoded

### Camada de Entrada

- [x] `[ValidateAntiForgeryToken]` em todos os POST
- [x] `ModelState.IsValid` verificado antes de processar
- [x] `[Required]` e `[StringLength]` no modelo
- [ ] Sanitização de tags (lista de strings livre)
- [ ] Rate limiting para prevenir abuso

### Camada de Dados

- [x] EF Core parametriza queries (prevenção SQL Injection)
- [x] Timestamps automáticos via `SaveChanges`
- [ ] String de conexão em User Secrets / Key Vault
- [ ] `appsettings.Development.json` no `.gitignore`

### Camada de Resposta

- [x] `UseExceptionHandler` em produção (sem stack trace)
- [ ] Headers de segurança HTTP (`CSP`, `X-Frame-Options`, etc.)
- [ ] Logs sem PII

### Camada de Testes

- [x] Quality Gate de cobertura ≥ 70% no CI
- [x] Testes unitários com Moq
- [x] Testes de integração com InMemory DB
- [ ] Branch coverage ≥ 70%
- [ ] Testes de segurança reais (OWASP ZAP, etc.)

---

## 📦 Dependências e CVEs

| Pacote | Versão | Escopo | Observação |
|---|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.0 | Produção | ✅ Versão atual |
| `Microsoft.EntityFrameworkCore.InMemory` | 9.0.0 | Testes | ✅ Versão atual |
| `Microsoft.AspNetCore.Mvc.Testing` | 9.0.0-preview | Testes | ⚠️ Preview — usar GA quando disponível |
| `Moq` | 4.20.72 | Testes | ✅ |
| `xunit` | 2.4.3 | Testes | ✅ |
| `coverlet.collector` | 8.0.1 | Testes | ✅ |

> ⚠️ O pipeline CI não verifica CVEs automaticamente (bloqueado pelo proxy corporativo). Executar `dotnet list package --vulnerable` periodicamente.

---

## 🚨 Recomendações Prioritárias

### 🔴 Imediato (antes de qualquer ambiente além de dev local)

```
1. Habilitar HTTPS redirect
   app.UseHttpsRedirection(); ← remover comentário

2. Implementar autenticação
   Adicionar ASP.NET Core Identity ou Azure AD B2C

3. Remover secrets do repositório
   Usar dotnet user-secrets (dev) e Key Vault (prod)
```

### 🟡 Curto Prazo (próximo sprint)

```
4. Adicionar headers de segurança HTTP
   CSP, X-Frame-Options, X-Content-Type-Options

5. Corrigir UserId hardcoded
   Obter de User.Claims após implementar autenticação

6. Aumentar branch coverage para ≥ 70%
   Adicionar testes para caminhos de erro
```

### 🟢 Médio Prazo

```
7. Implementar rate limiting
   Microsoft.AspNetCore.RateLimiting (built-in .NET 7+)

8. Adicionar scanning de CVEs no CI
   dotnet list package --vulnerable no workflow

9. Configurar SAST/DAST
   GitHub CodeQL ou SonarCloud para análise estática
```

---

## 📞 Reporte de Vulnerabilidades

Para reportar uma vulnerabilidade de segurança neste projeto:

1. **NÃO** abra uma issue pública
2. Entre em contato diretamente com a equipe de desenvolvimento
3. Inclua: descrição detalhada, passos para reproduzir, impacto estimado

---

*Documento gerado e mantido pelo **Security Review Assistant** — `.github/copilot-instructions.md`*
