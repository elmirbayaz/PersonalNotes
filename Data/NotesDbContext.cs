using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PersonalNotes.Models;

namespace PersonalNotes.Data;

public class NotesDbContext : DbContext
{
    public DbSet<Note> Notes => Set<Note>();

    public DbSet<Folder> Folders => Set<Folder>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<NoteTag> NoteTags => Set<NoteTag>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=personalnotes.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>()
            .HasIndex(tag => tag.Name)
            .IsUnique();

        modelBuilder.Entity<NoteTag>()
            .HasKey(noteTag => new { noteTag.NoteId, noteTag.TagId });

        modelBuilder.Entity<NoteTag>()
            .HasOne(noteTag => noteTag.Note)
            .WithMany(note => note.NoteTags)
            .HasForeignKey(noteTag => noteTag.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NoteTag>()
            .HasOne(noteTag => noteTag.Tag)
            .WithMany(tag => tag.NoteTags)
            .HasForeignKey(noteTag => noteTag.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Note>()
            .HasOne(note => note.Folder)
            .WithMany(folder => folder.Notes)
            .HasForeignKey(note => note.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}