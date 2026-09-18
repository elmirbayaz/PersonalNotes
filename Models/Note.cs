using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PersonalNotes.Models;

public class Note
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // null означает, что заметка находится в глобальной папке
    public int? FolderId { get; set; }

    public Folder? Folder { get; set; }

    public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
}
