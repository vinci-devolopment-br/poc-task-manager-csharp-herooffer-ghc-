using System;
using System.IO;
using System.Text.Json;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}

public class UsuarioService
{
    This code defines two classes:

    **`Usuario`** - A simple data model with three properties:
    - `Id` - numeric identifier
    - `Nome` - user's name
    - `Email` - user's email

    **`UsuarioService`** - A service class that handles saving users to a JSON file:

    - **`FilePath`** - constant defining the file name `"usuarios.json"` where data is stored

    - **`SalvarUsuario(Usuario usuario)`** - method that:
        1. Creates an empty list of users
        2. Checks if the JSON file already exists
        3. If it exists, reads and deserializes the existing content into the list
        4. Adds the new user to the list
        5. Serializes the entire list back to formatted JSON
        6. Writes the result to the file

    This is a simple **file-based persistence** implementation, acting as a lightweight alternative to a database.

    The placeholder area is empty — no additional code is needed there. The class declaration simply opens with `{` and continues with the existing members.
    private const string FilePath = "usuarios.json";

    public void SalvarUsuario(Usuario usuario)
    {
        List<Usuario> usuarios = new List<Usuario>();

        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                usuarios = JsonSerializer.Deserialize<List<Usuario>>(json);
            }
        }

        usuarios.Add(usuario);
        string novoJson = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, novoJson);
    }
}