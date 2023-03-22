using System;
using System.IO;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "server=localhost;user=root;password=1234567;database=fiotec";
        string folderPath = @"C:\temp\Mensagens";
        ProcessFiles(folderPath, connectionString);
    }

    static void ProcessFiles(string folderPath, string connectionString)
    {
        string[] filePaths = Directory.GetFiles(folderPath, "*.txt");

        foreach (string filePath in filePaths)
        {
            string fileContent = File.ReadAllText(filePath);
            if (IsEmail(fileContent))
            {
                SaveEmailToDatabase(filePath, fileContent, connectionString);
            }
        }
    }

    static bool IsEmail(string fileContent)
    {
        string fromPattern = @"De:\s*(.+)";
        string toPattern = @"Para:\s*(.+)";
        string subject = @"Assunto:\s*(.+)";
        string datePattern = @"Enviado em:\s*(.+)";

        bool hasFrom = Regex.IsMatch(fileContent, fromPattern, RegexOptions.IgnoreCase);
        bool hasTo = Regex.IsMatch(fileContent, toPattern, RegexOptions.IgnoreCase);
        bool hasSubject = Regex.IsMatch(fileContent, subject, RegexOptions.IgnoreCase);
        bool hasDate = Regex.IsMatch(fileContent, datePattern, RegexOptions.IgnoreCase);

        return hasFrom && hasTo && hasDate;
    }

    static void SaveEmailToDatabase(string filePath, string fileContent, string connectionString)
    {
        string fileName = Path.GetFileName(filePath);
        string sender = ExtractEmailInfo(fileContent, "De:");
        string recipient = ExtractEmailInfo(fileContent, "Para:");
        string date = ExtractEmailInfo(fileContent, "Enviado em:");
        string subject = ExtractEmailInfo(fileContent, "Assunto:");
        string content = fileContent;

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string insertQuery = "INSERT INTO arquivos (nomedoarquivo, remetente, destinatario, datahoraemail, conteudo) VALUES (@fileName, @sender, @recipient, @date, @content)";
            MySqlCommand command = new MySqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@fileName", fileName);
            command.Parameters.AddWithValue("@sender", sender);
            command.Parameters.AddWithValue("@recipient", recipient);
            command.Parameters.AddWithValue("@date", date);
            command.Parameters.AddWithValue("@content", content);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }

    static string ExtractEmailInfo(string fileContent, string info)
    {
        string pattern = string.Empty;

        switch (info)
        {
            case "De:":
                pattern = @"De:\s*(.+)";
                break;
            case "Para:":
                pattern = @"Para:\s*(.+)";
                break;
            case "Enviado em:":
                pattern = @"Enviado em:\s*(.+)";
                break;
            case "Assunto:":
                pattern = @"Assunto:\s*(.+)";
                break;
        }

        Match match = Regex.Match(fileContent, pattern, RegexOptions.IgnoreCase);
        return match.Groups[1].Value;
    }
}