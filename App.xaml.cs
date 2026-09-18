using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PersonalNotes.Data;
using System.Windows;

namespace PersonalNotes;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        using var db = new NotesDbContext();
        db.Database.Migrate();

        base.OnStartup(e);
    }
}
