using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Serilog;
using System.ComponentModel;

namespace CoH.GameData;

public struct Config()
{
    [DefaultValue(960)] public int WindowSizeX { get; set; } = 960;
    [DefaultValue(720)] public int WindowSizeY { get; set; } = 720;
    [DefaultValue(false)] public bool VSync { get; set; } = false;
    [DefaultValue(30)] public int BGMVolume { get; set; } = 30;
    [DefaultValue(20)] public int SEVolume { get; set; } = 20;
    [DefaultValue(0)] public sbyte TextSpeed { get; set; } = 0; // Maybe an enum? Since it's 3 possible values...
}

public static class Configuration
{
    private static string PathToConfig => Path.Combine(MainWindow.PathToSave, "Config.yaml");

    public static Config Default;

    public static bool Save()
    {
        try
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            if (!Directory.Exists(MainWindow.PathToSave))
                Directory.CreateDirectory(MainWindow.PathToSave);

            string yaml = serializer.Serialize(Default);
            using FileStream fs = new(PathToConfig, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs);
            sw.Write(yaml);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            return false;
        }
    }

    public static Config Load()
    {
        try
        {
            if (!File.Exists(PathToConfig))
            {
                Default = new Config();
                Save();
                return Default;
            }

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            using StreamReader sr = new(PathToConfig);
            Default = deserializer.Deserialize<Config>(sr);
            return Default;
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            return new Config();
        }
    }
}
