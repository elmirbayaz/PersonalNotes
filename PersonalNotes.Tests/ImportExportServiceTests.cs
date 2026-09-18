using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PersonalNotes.Services;

namespace PersonalNotes.Tests;

public class ImportExportServiceTests
{
    [Fact]
    public void ExportNotes_CreatesJsonFile()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");

        var notes = new List<NoteTransferModel>
        {
            new()
            {
                Title = "Тестовая заметка",
                Content = "Текст тестовой заметки",
                FolderName = "Учёба"
            }
        };

        try
        {
            ImportExportService.ExportNotes(notes, filePath);

            Assert.True(File.Exists(filePath));

            string json = File.ReadAllText(filePath);
            using var document = System.Text.Json.JsonDocument.Parse(json);

            var exportedNote = document.RootElement[0];

            Assert.Equal("Тестовая заметка",
                exportedNote.GetProperty("Title").GetString());

            Assert.Equal("Текст тестовой заметки",
                exportedNote.GetProperty("Content").GetString());

            Assert.Equal("Учёба",
                exportedNote.GetProperty("FolderName").GetString());
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ImportNotes_ReturnsExportedNoteData()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");

        var notes = new List<NoteTransferModel>
        {
            new()
            {
                Title = "План курсовой работы",
                Content = "Подготовить раздел тестирования.",
                FolderName = "Учёба"
            }
        };

        try
        {
            ImportExportService.ExportNotes(notes, filePath);

            List<NoteTransferModel> importedNotes =
                ImportExportService.ImportNotes(filePath);

            NoteTransferModel importedNote = Assert.Single(importedNotes);

            Assert.Equal("План курсовой работы", importedNote.Title);
            Assert.Equal("Подготовить раздел тестирования.", importedNote.Content);
            Assert.Equal("Учёба", importedNote.FolderName);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}