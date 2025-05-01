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
    //public static HotSheet<Item, ItemCsvMap> Items { get; private set; }
    public static List<SkillData> Skills { get; private set; } = [];
    public static List<BaseEcho> Echoes { get; private set; } = [];
    public static List<BaseEchoData> EchoesEntry { get; private set; } = [];
    public static List<Ability> Abilities { get; private set; } = [];

    public static bool Load()
    {
        try
        {
            Items = LoadCsv<Item, ItemCsvMap>("Items");
            //Items = new("Items");
            Skills = LoadCsv<SkillData, SkillDataMap>("Skills");
            Abilities = LoadCsv<Ability, AbilityMap>("Abilities");
            Echoes = LoadEchoes();

            Logger.Information("All Data Sheets have been loaded successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error reading Data Sheets. Reason:\n{ex}");
            return false;
        }
    }

    public static void Unload()
    {
        Items.Clear();
        Skills.Clear();
        Abilities.Clear();
        Echoes.Clear();
        EchoesEntry.Clear();
    }

    public static void Reload()
    {
        Unload();
        Load();
    }

    /// <summary>
    /// Reads a CSV file and transforms it into a list of <typeparamref name="T"/> using a <typeparamref name="M"/> map class.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <typeparam name="M">The <see cref="ClassMap"/> related to <typeparamref name="T"/></typeparam>
    /// <param name="sheetFile">Name of the file with or without the extension</param>
    /// <returns>A list of <typeparamref name="T"/></returns>
    private static List<T> LoadCsv<T, M>(string sheetFile) where M : ClassMap<T>
    {
        try
        {
            using var reader = new StreamReader(Path.Combine(MainWindow.PathToResources, "DataSheets", $"{Path.ChangeExtension(sheetFile, ".csv")}"));
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<M>();

            return [.. csv.GetRecords<T>()];
        }
        catch (Exception ex)
        {
            Logger.Error($"Couldn't read CSV file. Reason:\n{ex}");
            return [];
        }
    }

    public static bool SaveCsv<T, M>(string sheetFile, List<T> records) where M : ClassMap<T>
    {
        try
        {
            using var writer = new StreamWriter(Path.Combine(MainWindow.PathToResources, "DataSheets", $"{Path.ChangeExtension(sheetFile, ".csv")}"));
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<M>();

            csv.WriteRecords(records);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Couldn't save CSV file. Reason:\n{ex}");
            return false;
        }
    }

    private static List<BaseEcho> LoadEchoes()
    {
        string PathToEchoes = Path.Combine(MainWindow.PathToResources, "Echoes");
        List<BaseEchoData> echoData = LoadCsv<BaseEchoData, BaseEchoDataMap>("Echoes");
        EchoesEntry = echoData;
        List<BaseEcho> actualEchoes = [];

        foreach (BaseEchoData echo in echoData)
        {
            try
            {
                BaseEcho baseEcho = BaseEchoParser.ReadFromXml(Path.Combine(PathToEchoes, $"{Path.ChangeExtension(echo.FileName, ".xml")}"));
                baseEcho.EchoDexIndex = echo.EchoDexId;
                baseEcho.Id = echo.EchoId;
                actualEchoes.Add(baseEcho);
            }
            catch (FileNotFoundException) { Logger.Error($"File definition for echo \"{echo.FileName}\" doesn't exist."); }
            catch (Exception ex) { Logger.Error($"Couldn't read data for echo \"{echo.FileName}\". Reason:\n{ex}"); }
        }

        return actualEchoes;
    }
}
