using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalNotes.Models;

public class NoteTag
{
    public int NoteId { get; set; }

    public Note Note { get; set; } = null!;

    public int TagId { get; set; }

    public Tag Tag { get; set; } = null!;
}