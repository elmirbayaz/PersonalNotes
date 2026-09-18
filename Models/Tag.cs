using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalNotes.Models;

public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
}