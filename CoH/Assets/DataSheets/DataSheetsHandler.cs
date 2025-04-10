using CoH.GameData;
using CsvHelper;
using CsvHelper.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Assets.DataSheets;

public static class DataSheetsHandler
{
    public static ILogger Logger = Log.ForContext("Tag", "DataSheets");

    public static List<Item> Items { get; private set; } = [];

    public static bool Load()
    {
        try
        {
            Items = LoadCsv<Item, ItemCsvMap>("Items");

            Logger.Information("All Data Sheets have been loaded successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error reading Data Sheets. Reason: {ex}");
            return false;
        }
    }

    public static void Reload()
    {
        Items.Clear();
        Load();
    }

    /// <summary>
    /// Reads a CSV file and transforms it into a list of <typeparamref name="T"/> using a <typeparamref name="M"/> map class.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <typeparam name="M">The <see cref="ClassMap"/> related to <typeparamref name="T"/></typeparam>
    /// <param name="sheetFile">Name of the file WITHOUT THE EXTENSION</param>
    /// <returns>A list of <typeparamref name="T"/></returns>
    private static List<T> LoadCsv<T, M>(string sheetFile) where M : ClassMap<T>
    {
        using var reader = new StreamReader(Path.Combine(MainWindow.PathToResources, "DataSheets", $"{sheetFile}.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<M>();

        return [.. csv.GetRecords<T>()];
    }
}
