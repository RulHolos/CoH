using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using Serilog.Core;
using Serilog;

namespace CoH.Game.Ext;

/// <summary>
/// Represents a data sheet that can be hot-reloaded when the original file changes.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="M"></typeparam>
[Obsolete("Not functional for some reason")]
public class HotSheet<T, TMap> : IEnumerable<T> where TMap : ClassMap<T>
{
    private readonly ILogger Logger;

    public List<T> Items { get; private set; } = [];
    private readonly string sheetPath;
    private readonly FileSystemWatcher watcher;

    public HotSheet(string path)
    {
        Logger = Log.ForContext("Tag", "HotSheet");
        sheetPath = Path.Combine(MainWindow.PathToResources, "DataSheets", $"{Path.ChangeExtension(path, ".csv")}");

        LoadCsv();

        watcher = new(Path.GetDirectoryName(sheetPath)!, Path.GetFileName(sheetPath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        watcher.Changed += (_, _) => ReloadDebounced(); // Avoid double-triggers (VScode I see you...)
        // Never triggered?
    }

    private void LoadCsv()
    {
        try
        {
            using var reader = new StreamReader(sheetPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<TMap>();

            Items = [.. csv.GetRecords<T>()];
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load data sheet \"{sheetPath}\". Reason:\n{ex}");
            Items = [];
        }
    }

    private System.Timers.Timer? debounceTimer;
    private void ReloadDebounced()
    {
        Logger.Debug("Trying to reload...");

        debounceTimer?.Stop();
        debounceTimer = new System.Timers.Timer(200); // Avoid double triggers
        debounceTimer.Elapsed += (_, _) =>
        {
            debounceTimer?.Stop();
            LoadCsv();

            Logger.Debug("Reloaded.");
        };
        debounceTimer.Start();
    }

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}