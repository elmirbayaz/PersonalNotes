using PersonalNotes.Models;
using System.IO;
using System.Text.Json;

namespace PersonalNotes.Services;

public class NoteTransferModel
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? FolderName { get; set; }
}

public static class ImportExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static void ExportNotes(
        IEnumerable<NoteTransferModel> notes,
        string filePath)
    {
        var json = JsonSerializer.Serialize(notes, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public static List<NoteTransferModel> ImportNotes(string filePath)
    {
        var json = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<List<NoteTransferModel>>(json)
               ?? new List<NoteTransferModel>();
    }
}