//copilot instructions: Gere código seguindo boas práticas de segurança com logs claros e tratamentos de erros. Crie uma função para salvar um usuário
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UsuarioService
{
    private const string FilePath = "usuarios.json";
    private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private void Log(string nivel, string mensagem)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{nivel}] {mensagem}");
    }

    private bool ValidarUsuario(Usuario usuario)
    {
        if (usuario == null)
        {
            Log("ERROR", "Usuário inválido: objeto nulo.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(usuario.Nome))
        {
            Log("WARN", "Usuário inválido: nome não pode ser vazio.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(usuario.Email) || !EmailRegex.IsMatch(usuario.Email))
        {
            Log("WARN", $"Usuário inválido: e-mail '{usuario.Email}' está em formato incorreto.");
            return false;
        }
        return true;
    }

    public bool SalvarUsuario(Usuario usuario)
    {
        Log("INFO", "Iniciando processo de salvar usuário.");

        if (!ValidarUsuario(usuario))
        {
            Log("ERROR", "Operação cancelada: dados do usuário inválidos.");
            return false;
        }

        try
        {
            List<Usuario> usuarios = new List<Usuario>();

            if (File.Exists(FilePath))
            {
                Log("INFO", $"Arquivo '{FilePath}' encontrado. Lendo dados existentes.");
                string json = File.ReadAllText(FilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    usuarios = JsonSerializer.Deserialize<List<Usuario>>(json)
                               ?? new List<Usuario>();
                }
            }
            else
            {
                Log("INFO", $"Arquivo '{FilePath}' não encontrado. Um novo arquivo será criado.");
            }

            usuarios.Add(usuario);
            string novoJson = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, novoJson);

            Log("INFO", $"Usuário '{usuario.Nome}' (ID: {usuario.Id}) salvo com sucesso.");
            return true;
        }
        catch (JsonException ex)
        {
            Log("ERROR", $"Erro ao processar JSON: {ex.Message}");
            return false;
        }
        catch (IOException ex)
        {
            Log("ERROR", $"Erro de I/O ao acessar o arquivo '{FilePath}': {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Log("ERROR", $"Sem permissão para acessar o arquivo '{FilePath}': {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Log("ERROR", $"Erro inesperado ao salvar usuário: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }
}