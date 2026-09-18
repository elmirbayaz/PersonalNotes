using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PersonalNotes.Data;
using PersonalNotes.Models;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using PersonalNotes.Services;


namespace PersonalNotes.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Note> notes = new();

    [ObservableProperty]
    private ObservableCollection<Folder> folders = new();

    [ObservableProperty]
    private Note? selectedNote;

    [ObservableProperty]
    private Folder? selectedFolder;

    [ObservableProperty]
    private string folderName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Tag> selectedNoteTags = new();

    [ObservableProperty]
    private string newTagName = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedSortOption = "По дате изменения";

    public MainViewModel()
    {
        LoadData();
    }

    partial void OnSelectedFolderChanged(Folder? value)
    {
        FolderName = value?.Name ?? string.Empty;
        LoadNotes(value?.Id);
    }

    partial void OnSelectedNoteChanged(Note? value)
    {
        LoadTags(value?.Id);
    }

    private void LoadTags(int? noteId)
    {
        if (!noteId.HasValue)
        {
            SelectedNoteTags = new ObservableCollection<Tag>();
            return;
        }

        using var db = new NotesDbContext();

        SelectedNoteTags = new ObservableCollection<Tag>(
            db.NoteTags
                .Where(noteTag => noteTag.NoteId == noteId)
                .Include(noteTag => noteTag.Tag)
                .Select(noteTag => noteTag.Tag)
                .OrderBy(tag => tag.Name)
                .ToList());
    }

    partial void OnSearchTextChanged(string value)
    {
        LoadNotes(SelectedFolder?.Id);
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        LoadNotes(SelectedFolder?.Id);
    }

    public void LoadData()
    {
        using var db = new NotesDbContext();

        Folders = new ObservableCollection<Folder>(
            db.Folders.OrderBy(folder => folder.Name).ToList());

        LoadNotes(SelectedFolder?.Id);
    }

    private void LoadNotes(int? folderId)
    {
        using var db = new NotesDbContext();

        var query = db.Notes.Include(note => note.Folder).AsQueryable();

        query = folderId.HasValue
            ? query.Where(note => note.FolderId == folderId)
            : query.Where(note => note.FolderId == null);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(note =>
                note.Title.Contains(SearchText) ||
                note.Content.Contains(SearchText));
        }

        query = SelectedSortOption switch
        {
            "По названию" => query.OrderBy(note => note.Title),
            "По дате создания" => query.OrderByDescending(note => note.CreatedAt),
            _ => query.OrderByDescending(note => note.UpdatedAt)
        };

        Notes = new ObservableCollection<Note>(query.ToList());
    }

    [RelayCommand]
    private void ShowGlobalFolder()
    {
        SelectedFolder = null;
        LoadNotes(null);
    }

    [RelayCommand]
    private void CreateNote()
    {
        var note = new Note
        {
            Title = "Новая заметка",
            Content = string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            FolderId = SelectedFolder?.Id
        };

        using var db = new NotesDbContext();
        db.Notes.Add(note);
        db.SaveChanges();

        Notes.Insert(0, note);
        SelectedNote = note;
    }

    [RelayCommand]
    private void SaveNote()
    {
        if (SelectedNote is null)
        {
            return;
        }

        using var db = new NotesDbContext();
        var note = db.Notes.Find(SelectedNote.Id);

        if (note is null)
        {
            return;
        }

        note.Title = SelectedNote.Title;
        note.Content = SelectedNote.Content;
        note.UpdatedAt = DateTime.Now;

        db.SaveChanges();

        LoadData();
        SelectedNote = Notes.FirstOrDefault(item => item.Id == note.Id);
    }

    [RelayCommand]
    private void DeleteNote()
    {
        if (SelectedNote is null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Удалить заметку «{SelectedNote.Title}»?",
            "Удаление заметки",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        using var db = new NotesDbContext();
        var note = db.Notes.Find(SelectedNote.Id);

        if (note is not null)
        {
            db.Notes.Remove(note);
            db.SaveChanges();
        }

        Notes.Remove(SelectedNote);
        SelectedNote = null;
    }

    [RelayCommand]
    private void AddTag()
    {
        if (SelectedNote is null || string.IsNullOrWhiteSpace(NewTagName))
        {
            return;
        }

        using var db = new NotesDbContext();

        var tagName = NewTagName.Trim();

        var tag = db.Tags.FirstOrDefault(item => item.Name == tagName);

        if (tag is null)
        {
            tag = new Tag { Name = tagName };
            db.Tags.Add(tag);
            db.SaveChanges();
        }

        var exists = db.NoteTags.Any(item =>
            item.NoteId == SelectedNote.Id && item.TagId == tag.Id);

        if (!exists)
        {
            db.NoteTags.Add(new NoteTag
            {
                NoteId = SelectedNote.Id,
                TagId = tag.Id
            });

            db.SaveChanges();
        }

        NewTagName = string.Empty;
        LoadTags(SelectedNote.Id);
    }

    [RelayCommand]
    private void RemoveTag(Tag? tag)
    {
        if (SelectedNote is null || tag is null)
        {
            return;
        }

        using var db = new NotesDbContext();

        var noteTag = db.NoteTags.Find(SelectedNote.Id, tag.Id);

        if (noteTag is not null)
        {
            db.NoteTags.Remove(noteTag);
            db.SaveChanges();
        }

        LoadTags(SelectedNote.Id);
    }

    [RelayCommand]
    private void CreateFolder()
    {
        if (string.IsNullOrWhiteSpace(FolderName))
        {
            MessageBox.Show(
                "Введите название папки.",
                "Создание папки",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var folder = new Folder
        {
            Name = FolderName.Trim(),
            CreatedAt = DateTime.Now
        };

        using var db = new NotesDbContext();
        db.Folders.Add(folder);
        db.SaveChanges();

        Folders.Add(folder);
        SelectedFolder = folder;
    }

    [RelayCommand]
    private void RenameFolder()
    {
        if (SelectedFolder is null || string.IsNullOrWhiteSpace(FolderName))
        {
            return;
        }

        using var db = new NotesDbContext();
        var folder = db.Folders.Find(SelectedFolder.Id);

        if (folder is null)
        {
            return;
        }

        folder.Name = FolderName.Trim();
        db.SaveChanges();

        var folderId = folder.Id;
        LoadData();
        SelectedFolder = Folders.FirstOrDefault(item => item.Id == folderId);
    }

    [RelayCommand]
    private void DeleteFolder()
    {
        if (SelectedFolder is null)
        {
            return;
        }

        using var db = new NotesDbContext();

        var notesCount = db.Notes.Count(note => note.FolderId == SelectedFolder.Id);

        var message = notesCount == 0
            ? $"Удалить папку «{SelectedFolder.Name}»?"
            : $"В папке «{SelectedFolder.Name}» находится заметок: {notesCount}.\n\nУдалить папку вместе с заметками?";

        var result = MessageBox.Show(
            message,
            "Удаление папки",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var folder = db.Folders.Find(SelectedFolder.Id);

        if (folder is not null)
        {
            db.Folders.Remove(folder);
            db.SaveChanges();
        }

        SelectedFolder = null;
        FolderName = string.Empty;
        LoadData();
    }

    [RelayCommand]
    private void ExportNote()
    {
        if (SelectedNote is null)
        {
            MessageBox.Show(
                "Выберите заметку для экспорта.",
                "Экспорт",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "JSON-файл (*.json)|*.json",
            FileName = "note.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        using var db = new NotesDbContext();

        var note = db.Notes
            .Include(item => item.Folder)
            .FirstOrDefault(item => item.Id == SelectedNote.Id);

        if (note is null)
        {
            return;
        }

        ImportExportService.ExportNotes(
            new List<NoteTransferModel>
            {
            new()
            {
                Title = note.Title,
                Content = note.Content,
                FolderName = note.Folder?.Name
            }
            },
            dialog.FileName);

        MessageBox.Show(
            "Заметка успешно экспортирована.",
            "Экспорт",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ImportNotes()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON-файл (*.json)|*.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var importedNotes = ImportExportService.ImportNotes(dialog.FileName);

            using var db = new NotesDbContext();

            foreach (var importedNote in importedNotes)
            {
                Folder? folder = null;

                if (!string.IsNullOrWhiteSpace(importedNote.FolderName))
                {
                    folder = db.Folders.FirstOrDefault(item =>
                        item.Name == importedNote.FolderName);

                    if (folder is null)
                    {
                        folder = new Folder
                        {
                            Name = importedNote.FolderName,
                            CreatedAt = DateTime.Now
                        };

                        db.Folders.Add(folder);
                        db.SaveChanges();
                    }
                }

                db.Notes.Add(new Note
                {
                    Title = importedNote.Title,
                    Content = importedNote.Content,
                    FolderId = folder?.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            db.SaveChanges();
            LoadData();

            MessageBox.Show(
                $"Импортировано заметок: {importedNotes.Count}.",
                "Импорт",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Не удалось прочитать выбранный JSON-файл.",
                "Ошибка импорта",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}