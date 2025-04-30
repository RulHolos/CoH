using CoH.Game;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

public struct SaveData()
{
    public string TrainerName = "Unknown";
    public uint TrainerId = (uint)Random.Shared.Next();
    public bool Gender = true; // True = Female; False = Male
    public uint Money = 0;
    public DateTime SaveStartedAt = DateTime.Now;
    public Vector2 PositionOnMap = Vector2.Zero;
    public int CurrentMapId = 0;
    public FacingDirection FacingDir = FacingDirection.Down;
    public Party PartyEchoes; // No more than 6.
    public Dictionary<string, bool> Flags { get; set; } = [];

    public override readonly string ToString()
    {
        return $"{TrainerName} ({TrainerId}) - {(Gender ? "Female" : "Male")} at {PositionOnMap} on MAP [ID: {CurrentMapId}] Facing {FacingDir}.";
    }

    public void SetFlag(string key, bool value)
    {
        Flags.TryAdd(key, value);
    }

    public bool GetFlag(string key)
    {
        if (Flags == null)
            Flags = []; // ????????
        Flags.TryGetValue(key, out bool value);
        return value;
    }
}

[InlineArray(6)]
public struct Party
{
    private AliveEcho _element0;
}

/// <summary>
/// Responsible for handling the save file.
/// </summary>
public static class SaveFile
{
    /*
     * The purpose of this key isn't to really encrypt files so that they can't be read by anyone.
     * This is an open source project after all.
     * This is only to make it less easy for players to mess with they save files in ways that would crash the game.
     * I even left a variable to set if you wish to not use encryption at all.
     */
    private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("J*!JC@3q#vHSn9$drqF9Y4s@R8Se^UCU");

    private static string SavePath => Path.Combine(MainWindow.PathToSave, "save.dat");
    public static bool SaveExists => File.Exists(SavePath);

    public static SaveData SaveData;

    // Set this to false if you do not wish to use encryption in the save files.
    private const bool UseEncryption = true;

    public static ILogger SaveLogger = Log.ForContext("Tag", "Save");

    public static void Save()
    {
        using FileStream sw = new(SavePath, FileMode.OpenOrCreate, FileAccess.Write);
        Encrypt(sw);

        SaveLogger.Debug("Game Saved");
    }

    public static void Load()
    {
        SaveData = new();

        try
        {
            if (!File.Exists(SavePath))
                Save();
            using FileStream fs = new(SavePath, FileMode.Open, FileAccess.Read);
            SaveData = Decrypt(fs);
            SaveLogger.Debug("Game Loaded");
            SaveLogger.Debug(SaveData.ToString());
        }
        catch (Exception ex)
        {
            SaveLogger.Error($"Couldn't read the save file. Reason: {ex}");
        }
    }

    public static void Encrypt(FileStream fs)
    {
        using Aes aes = Aes.Create();
        aes.Key = EncryptionKey;
        byte[] iv = aes.IV;
        fs.Write(iv, 0, iv.Length);

        using CryptoStream cs = new(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter sr = new(cs, Encoding.Unicode);
        sr.Write(JsonConvert.SerializeObject(SaveData));
    }

    public static SaveData Decrypt(FileStream fs)
    {
        using Aes aes = Aes.Create();
        byte[] iv = new byte[aes.IV.Length];
        int numBytesToRead = aes.IV.Length;
        int numBytesRead = 0;
        while (numBytesToRead > 0)
        {
            int n = fs.Read(iv, numBytesRead, numBytesToRead);
            if (n == 0) break;

            numBytesRead += n;
            numBytesToRead -= n;
        }

        byte[] key = EncryptionKey;

        using CryptoStream cs = new(fs, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);
        using StreamReader dr = new(cs, Encoding.Unicode);
        return JsonConvert.DeserializeObject<SaveData>(dr.ReadToEnd());
    }
}