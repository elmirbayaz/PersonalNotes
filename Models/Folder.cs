using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalNotes.Models;

public class Folder
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
