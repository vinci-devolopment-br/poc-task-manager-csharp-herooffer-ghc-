# 📋 TaskManager — POC

> Aplicação web de gerenciamento de tarefas construída com **ASP.NET Core 9 MVC** + **Entity Framework Core** + **SQL Server**, seguindo arquitetura em camadas com padrão Repository.

---

## 🗂️ Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Stack Tecnológica](#-stack-tecnológica)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Modelo de Dados](#-modelo-de-dados)
- [Funcionalidades](#-funcionalidades)
- [Rotas da Aplicação](#-rotas-da-aplicação)
- [Como Executar](#-como-executar)
- [Testes](#-testes)
- [CI/CD e Quality Gate](#-cicd-e-quality-gate)
- [Configurações](#-configurações)

---

## 🎯 Visão Geral

O **TaskManager** é uma aplicação CRUD completa para gerenciamento de tarefas com suporte a prioridades, categorias, tags, paginação, ordenação e estatísticas em tempo real.

```
┌─────────────────────────────────────────────────────────┐
│                     NAVEGADOR                           │
│              (Bootstrap 5 + jQuery)                     │
└─────────────────────┬───────────────────────────────────┘
                      │ HTTP
┌─────────────────────▼───────────────────────────────────┐
│               ASP.NET Core 9 MVC                        │
│         TasksController  │  HomeController              │
└──────────┬──────────────────────────┬───────────────────┘
           │ ITaskService             │
┌──────────▼──────────────────────────────────────────────┐
│                   TaskService                           │
│              (Lógica de Negócio)                        │
└──────────┬──────────────────────────────────────────────┘
           │ ITaskRepository
┌──────────▼──────────────────────────────────────────────┐
│                  TaskRepository                         │
│           (Entity Framework Core)                       │
└──────────┬──────────────────────────────────────────────┘
           │
┌──────────▼──────────────────────────────────────────────┐
│            SQL Server (LocalDB em Dev)                  │
│                  Tabela: Tasks                          │
└─────────────────────────────────────────────────────────┘
```

---

## 🏗️ Arquitetura

A aplicação segue o padrão **MVC em camadas** com separação clara de responsabilidades:

```
┌────────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER                                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────┐  │
│  │ TasksController │  │ HomeController  │  │    Views      │  │
│  └────────┬────────┘  └────────┬────────┘  └───────────────┘  │
├───────────┼────────────────────┼───────────────────────────────┤
│  BUSINESS LAYER                │                               │
│  ┌────────▼────────────────────▼────────────────────────────┐  │
│  │  ITaskService  ◄────────  TaskService                    │  │
│  └────────────────────────────┬─────────────────────────────┘  │
├───────────────────────────────┼───────────────────────────────┤
│  DATA ACCESS LAYER            │                               │
│  ┌────────────────────────────▼─────────────────────────────┐  │
│  │  ITaskRepository  ◄────── TaskRepository                 │  │
│  │                           TaskManagerDbContext           │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
```

### Princípios aplicados

| Princípio | Implementação |
|---|---|
| **Dependency Injection** | `ITaskService`, `ITaskRepository` injetados via construtor |
| **Repository Pattern** | `TaskRepository` abstrai o acesso ao EF Core |
| **Interface Segregation** | `ITaskService` e `ITaskRepository` com contratos mínimos |
| **Single Responsibility** | Controller cuida de HTTP, Service cuida de negócio, Repository cuida de dados |

---

## 🛠️ Stack Tecnológica

| Componente | Tecnologia | Versão |
|---|---|---|
| **Framework** | ASP.NET Core MVC | 9.0 |
| **ORM** | Entity Framework Core | 9.0.0 |
| **Banco de Dados** | SQL Server / LocalDB | — |
| **Frontend** | Bootstrap + jQuery | 5.x / 3.x |
| **Testes** | xUnit + Moq | 2.4.3 / 4.20.72 |
| **Cobertura** | Coverlet | 8.0.1 |
| **CI/CD** | GitHub Actions | — |
| **Linguagem** | C# | 13 / .NET 9 |

---

## 📁 Estrutura do Projeto

```
poc-task-manager/
├── .github/
│   ├── copilot-instructions.md     ← Agente Security Review Assistant
│   └── workflows/
│       └── dotnet.yml              ← Pipeline CI/CD + Quality Gate 70%
│
├── src/
│   └── TaskManager.Web/
│       ├── Controllers/
│       │   ├── TasksController.cs  ← CRUD completo de tarefas
│       │   └── HomeController.cs   ← Página inicial
│       ├── Data/
│       │   └── TaskManagerDbContext.cs  ← Contexto EF Core
│       ├── Models/
│       │   ├── TaskItem.cs         ← Entidade principal
│       │   ├── Priority.cs         ← Enum de prioridades
│       │   └── Category.cs         ← Enum de categorias
│       ├── Repositories/
│       │   ├── ITaskRepository.cs  ← Contrato do repositório
│       │   └── TaskRepository.cs   ← Implementação EF Core
│       ├── Services/
│       │   ├── ITaskService.cs     ← Contrato do serviço
│       │   └── TaskService.cs      ← Lógica de negócio
│       ├── Views/
│       │   ├── Tasks/              ← Index, Create, Edit, Delete
│       │   ├── Home/               ← Index, Privacy
│       │   └── Shared/             ← Layout, Error
│       ├── Program.cs              ← Entry point / DI Container
│       └── appsettings.json        ← Configurações
│
├── tests/
│   └── TaskManager.Web.Tests/
│       ├── TaskServiceTests.cs     ← Testes unitários do serviço
│       ├── TasksControllerTests.cs ← Testes unitários do controller
│       ├── TaskRepositoryTests.cs  ← Testes de integração do repositório
│       ├── ApiSmokeTests.cs        ← Smoke tests de integração
│       ├── HomeControllerTests.cs  ← Testes do HomeController
│       └── SecurityTests.cs        ← Testes de segurança
│
└── nuget.config                    ← Configuração NuGet local
```

---

## 📊 Modelo de Dados

### Entidade `TaskItem`

```
┌──────────────────────────────────────────────────────┐
│                     TaskItem                         │
├──────────────────┬──────────────────┬────────────────┤
│ Campo            │ Tipo             │ Restrições      │
├──────────────────┼──────────────────┼────────────────┤
│ Id               │ long             │ PK, Auto-inc    │
│ Title            │ string           │ Required, ≤200  │
│ Description      │ string?          │ Opcional, ≤2000 │
│ Priority         │ Priority (enum)  │ Required        │
│ Category         │ Category (enum)  │ Required        │
│ DueDate          │ DateTime?        │ Opcional        │
│ Tags             │ List<string>     │ Stored as CSV   │
│ AssignedTo       │ string?          │ Opcional, ≤100  │
│ UserId           │ string           │ Required, ≤100  │
│ Completed        │ bool             │ Default: false  │
│ CreatedAt        │ DateTime         │ Auto (UTC)      │
│ UpdatedAt        │ DateTime         │ Auto (UTC)      │
└──────────────────┴──────────────────┴────────────────┘
```

### Enum `Priority`

| Valor | Descrição |
|---|---|
| `Low` | Baixa prioridade |
| `Medium` | Média prioridade *(padrão)* |
| `High` | Alta prioridade |
| `Urgent` | Urgente |

### Enum `Category`

| Valor | Descrição |
|---|---|
| `Work` | Trabalho |
| `Personal` | Pessoal |
| `Study` | Estudo |
| `Health` | Saúde |
| `Bug` | Bug |
| `Improvement` | Melhoria |
| `Feature` | Feature |
| `Support` | Suporte |
| `Other` | Outro *(padrão)* |

---

## ⚙️ Funcionalidades

### CRUD de Tarefas

| Operação | Descrição |
|---|---|
| ✅ **Listar** | Lista paginada com 2 itens por página e ordenação por data |
| ✅ **Criar** | Formulário com validação de campos obrigatórios |
| ✅ **Editar** | Edição completa com validação e proteção CSRF |
| ✅ **Excluir** | Confirmação de exclusão com proteção CSRF |

### Paginação e Ordenação

```
GET /Tasks?page=1&order=desc
         └── page: número da página (padrão: 1)
         └── order: "asc" | "desc" (padrão: "desc" - mais recentes primeiro)
```

### Estatísticas em Tempo Real

```
┌─────────────────────────────────────────────────┐
│              Dashboard de Estatísticas          │
├─────────────┬─────────────┬──────────┬──────────┤
│   Total     │  Concluídas │ Pendentes│  Urgentes│
│   (todas)   │ (completed) │          │  (ativas)│
└─────────────┴─────────────┴──────────┴──────────┘
```

### Partial View com AJAX

- `GET /Tasks/TaskListPartial?page=X&order=Y` → retorna HTML parcial para atualização dinâmica da lista sem reload de página

---

## 🌐 Rotas da Aplicação

| Método | Rota | Ação | Descrição |
|---|---|---|---|
| `GET` | `/` ou `/Tasks` | `Index` | Lista de tarefas com paginação |
| `GET` | `/Tasks/Create` | `Create` | Formulário de criação |
| `POST` | `/Tasks/Create` | `Create` | Salvar nova tarefa |
| `GET` | `/Tasks/Edit/{id}` | `Edit` | Formulário de edição |
| `POST` | `/Tasks/Edit/{id}` | `Edit` | Salvar edição |
| `GET` | `/Tasks/Delete/{id}` | `Delete` | Página de confirmação |
| `POST` | `/Tasks/Delete/{id}` | `DeleteConfirmed` | Executar exclusão |
| `GET` | `/Tasks/TaskListPartial` | `TaskListPartial` | Partial View AJAX |
| `GET` | `/Home` | `Index` | Home |
| `GET` | `/Home/Privacy` | `Privacy` | Política de privacidade |

---

## 🚀 Como Executar

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (incluso no Visual Studio)
- Git

### 1. Clonar o repositório

```bash
git clone https://github.com/vinci-devolopment-br/poc-task-manager-csharp-herooffer-ghc-.git
cd poc-task-manager-csharp-herooffer-ghc-
```

### 2. Restaurar dependências

```bash
dotnet restore --configfile nuget.config
```

> ⚠️ Em redes com proxy corporativo, utilize o `nuget.config` já configurado na raiz do projeto.

### 3. Executar a aplicação

```bash
dotnet run --project ./src/TaskManager.Web/TaskManager.Web.csproj
```

A aplicação estará disponível em: `http://localhost:5000`

> O banco de dados é criado automaticamente na primeira execução em ambiente `Development` via `EnsureCreated()`.

### 4. Via Visual Studio

Abra o arquivo `.sln` ou a pasta no Visual Studio 2022+ e pressione `F5`.

### Fluxo de inicialização

```
dotnet run
    │
    ├── Cria WebApplication
    ├── Registra serviços (DI)
    ├── Configura EF Core → SQL Server LocalDB
    │
    ├── IsDevelopment()?
    │   └── SIM → EnsureCreated() → cria DB se não existir
    │
    ├── Configura middleware (routing, auth, static files)
    └── Inicia servidor na porta 5000/5001
```

---

## 🧪 Testes

### Executar todos os testes

```bash
dotnet test ./tests/TaskManager.Web.Tests/TaskManager.Web.Tests.csproj
```

### Executar com cobertura

```bash
dotnet test ./tests/TaskManager.Web.Tests/TaskManager.Web.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/local
```

### Resumo da suíte de testes

| Classe de Teste | Tipo | Testes | O que cobre |
|---|---|---|---|
| `TaskServiceTests` | Unitário | 8 | CRUD do serviço com Moq |
| `TasksControllerTests` | Unitário | 10 | Ações do controller com Moq |
| `TaskRepositoryTests` | Integração | 8 | Repositório com EF InMemory |
| `HomeControllerTests` | Unitário | 2 | Controller de home |
| `ApiSmokeTests` | Integração | 2 | Smoke tests HTTP end-to-end |
| `SecurityTests` | Unitário | 1 | Cabeçalhos de segurança |
| `UnitTest1` | Placeholder | 1 | — |
| **Total** | — | **32** | — |

### Cobertura atual

```
Line Coverage  : ~70% (262 / 376 linhas)
Branch Coverage: ~32%
Status Quality Gate: ✅ PASSA (≥ 70%)
```

---

## 🔄 CI/CD e Quality Gate

O pipeline roda automaticamente em todo `push` e `pull_request` para `main`/`master`.

```
┌──────────────────────────────────────────────────────────┐
│                GitHub Actions Pipeline                   │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  1. checkout → 2. setup .NET 9 → 3. restore             │
│                                                          │
│  4. build app → 5. build tests                          │
│                                                          │
│  6. dotnet test (com XPlat Code Coverage)               │
│                                                          │
│  7. ✅ QUALITY GATE                                      │
│     ┌──────────────────────────────────────────────┐    │
│     │  Cobertura atual >= 70%?                     │    │
│     │  SIM ──► ✅ Pipeline PASSA                   │    │
│     │  NÃO ──► ❌ Pipeline FALHA (merge bloqueado) │    │
│     └──────────────────────────────────────────────┘    │
│                                                          │
│  8. Upload artefato coverage.cobertura.xml              │
└──────────────────────────────────────────────────────────┘
```

### Configurar Branch Protection (obrigatório para bloquear merge)

```
GitHub → Settings → Branches → Add rule → Branch: main
✅ Require status checks to pass → "build"
✅ Require branches to be up to date
✅ Do not allow bypassing
```

---

## ⚙️ Configurações

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagerDb;Trusted_Connection=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Variáveis de Ambiente (Produção)

| Variável | Descrição |
|---|---|
| `ConnectionStrings__DefaultConnection` | String de conexão com SQL Server |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` / `Testing` |

---

## 🤝 Contribuição

1. Crie uma branch a partir de `master`
2. Implemente as mudanças
3. Garanta que **todos os 32 testes passam**
4. Garanta que a **cobertura ≥ 70%** (Quality Gate do CI)
5. Abra um Pull Request

> O merge só é permitido se o CI/CD passar — incluindo o Quality Gate de cobertura.
