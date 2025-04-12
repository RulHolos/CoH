using System.Xml;
using System.Globalization;
using CoH.GameData;

public static class BaseEchoReader
{
    // TODO: Rework this. It's very broken in a lot of ways. Remove the ! and ? and nullcheck everything.
    public static unsafe BaseEcho ReadFromXml(string path)
    {
        XmlDocument doc = new();
        doc.Load(path);

        XmlNode echoNode = doc.SelectSingleNode("/Echo") ?? throw new FileLoadException("Echo file is malformed.");
        BaseEcho echo = new()
        {
            Name = echoNode.Attributes!["Name"]!.Value,
            DexName = echoNode.Attributes["DexName"]!.Value,
            Cost = byte.Parse(echoNode["Cost"]!.InnerText)
        };

        // Skills
        var skillNodes = echoNode.SelectNodes("Skills/Skill")!;
        for (int i = 0; i < 5; i++)
            echo.BaseSkills[i] = ushort.Parse(skillNodes[i]!.Attributes!["Id"]!.Value);

        // Drops
        var dropNodes = echoNode.SelectNodes("Drops/Item")!;
        for (int i = 0; i < 4; i++)
            echo.ItemDropTable[i] = ushort.Parse(dropNodes[i]!.Attributes!["Id"]!.Value);

        // Styles
        var styleNodes = echoNode.SelectNodes("Styles/Style")!;
        for (int i = 0; i < styleNodes.Count && i < 4; i++)
        {
            var style = new EchoStyle();
            var styleNode = styleNodes[i];

            style.Type = Enum.Parse<StyleType>(styleNode!.Attributes!["StyleType"]!.Value);

            var elements = styleNode.SelectSingleNode("Elements")!;
            style.Element1 = ParseElement(elements.Attributes!["Element1"]!.Value);
            style.Element2 = ParseElement(elements.Attributes!["Element2"]!.Value);

            var stats = styleNode.SelectSingleNode("Stats")!;
            string[] statsName = ["HP", "FoAtk", "FoDef", "SpAtk", "SpDef", "Speed"];
            for (int j = 0; j < 6; j++)
                style.BaseStats[j] = byte.Parse(stats[statsName[j]]!.InnerText);

            var abilities = styleNode.SelectSingleNode("Abilities")!;
            if (abilities.Attributes?.Count != 2)
                throw new ArgumentException($"{echo.Name} is missing one of more abilities.");
            style.Abilities[0] = byte.Parse(abilities.Attributes["Ability1"]!.Value);
            style.Abilities[1] = byte.Parse(abilities.Attributes["Ability2"]!.Value);

            var styleSkills = styleNode.SelectNodes("Skills/Skill")!;
            for (int j = 0; j < 11; j++) // Crashes if there's not 11
                style.StyleSkills[j] = ushort.Parse(styleSkills[j]!.Attributes!["Id"]!.Value);

            style.Level100Skill = ushort.Parse(styleNode.SelectSingleNode("Level100Skill")!.Attributes!["Id"]!.Value);

            var cardBits = styleNode.SelectSingleNode("SkillCards")!.InnerText.Split(';').Select(byte.Parse).ToArray();
            for (int j = 0; j < cardBits.Length && j < 16; j++)
                style.SkillCardBitfield[j] = cardBits[j];

            var lvl70Nodes = styleNode.SelectNodes("Level70Skills/Skill")!;
            for (int j = 0; j < 8; j++) // Crashes if there's not 8
                style.Level70Skills[j] = ushort.Parse(lvl70Nodes[j]!.Attributes!["Id"]!.Value);

            var metaNodes = styleNode.SelectNodes("Meta")!;
            foreach (XmlNode metaNode in metaNodes)
            {
                Enum.TryParse(typeof(StyleMeta), metaNode.InnerText!, true, out object? result);
                style.Meta |= result != null ? (StyleMeta)result : StyleMeta.None;
            }

            echo.Styles[i] = style;
        }

        return echo;
    }

    private static Element ParseElement(string elementName)
    {
        if (Enum.TryParse(typeof(Element), elementName, out object? result))
            return (Element)result;
        return Element.Void;
    }
}
