﻿namespace A3ServerTool.Helpers;

/// <inheritdoc>
public class MissionDirector : IMissionDirector
{
    private readonly IDao<Mission> _missionDao;

    public MissionDirector(IDao<Mission> missionDao)
    {
        _missionDao = missionDao;
    }

    /// <inheritdoc>
    public IList<Mission> GetMissions(IEnumerable<string> configProperties, string folderPath)
    {
        var configMissions = GetMissionsFromConfig(configProperties);
        var storedMissions = GetMissionsFromStorage(folderPath);

        foreach (var mission in storedMissions)
        {
            var configMission = configMissions.FirstOrDefault(x => x.Name == mission.Name);

            if (configMission != null)
            {
                mission.IsSelected = true;
                mission.Difficulty = configMission.Difficulty;
            }
        }

        return storedMissions;
    }

    /// <inheritdoc>
    public string SaveMissions(IEnumerable<Mission> missions, string fileContent)
    {
        if (!missions.Any()) return fileContent;

        var playableMissions = ParsePlayableMissions(missions);
        var whitelistedMissions = ConvertWhitelistedMissionsToText(missions);
        fileContent = RemoveWhitelistField(fileContent);

        return fileContent + whitelistedMissions + playableMissions;
    }

    /// <summary>
    /// Gets the missions from configuration file.
    /// </summary>
    /// <param name="configProperties">The configuration properties.</param>
    /// <returns>List of missions from server.cfg.</returns>
    private List<Mission> GetMissionsFromConfig(IEnumerable<string> configProperties)
    {
        if (configProperties?.Any() != true)
        {
            return new List<Mission>();
        }

        var missionIndex = configProperties.ToList().FindIndex(x => x.Trim() == "class Missions");
        if (missionIndex == -1) return new List<Mission>();

        var result = new List<Mission>();

        var missionLines = configProperties.ToList().GetRange(missionIndex, configProperties.Count() - missionIndex);
        if (!missionLines.Any()) return result;

        var missionToSettingsDictionary = new Dictionary<string, Dictionary<string, string>>();

        for (int i = 1; i < missionLines.Count; i++)
        {
            if (missionLines[i].Contains("class Mission"))
            {
                var settingToValueDictionary = new Dictionary<string, string>();

                for (int j = i + 1; j < missionLines.Count; j++)
                {
                    var splittedProperty = missionLines[j].Split('=')
                        .Where(x => x != "=")
                        .Select(x => x.Trim())
                        .Select(x => x.Replace("\"", string.Empty))
                        .Select(x => x.Replace(";", string.Empty))
                        .ToArray();
                    if (splittedProperty.Length != 2) continue;

                    settingToValueDictionary.Add(splittedProperty[0], splittedProperty[1]);
                    if (settingToValueDictionary.Count == 2)
                    {
                        break;
                    }
                }

                if (settingToValueDictionary.Count == 2)
                {
                    missionToSettingsDictionary.Add(missionLines[i].Replace("\t", string.Empty), settingToValueDictionary);
                }
            }
        }

        foreach (var settingsHeader in missionToSettingsDictionary)
        {
            var mission = new Mission
            {
                Name = settingsHeader.Value["template"],
                IsSelected = true
            };

            Enum.TryParse(settingsHeader.Value["difficulty"].FirstLetterToUpperCase(), out DifficultyType difficulty);
            mission.Difficulty = difficulty;
            result.Add(mission);
        }

        return result;
    }

    /// <summary>
    /// Parses the playable missions.
    /// </summary>
    /// <param name="missions">Missions marked to play on server.</param>
    private string ParsePlayableMissions(IEnumerable<Mission> missions)
    {
        if (missions?.Any() == false)
        {
            return string.Empty;
        }

        missions = missions.Where(x => x.IsSelected).ToArray();
        if (!missions.Any()) return string.Empty;

        foreach (var mission in missions)
        {
            if (mission.Difficulty == DifficultyType.None)
            {
                mission.Difficulty = DifficultyType.Recruit;
            }
        }

        int counter = 1;

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("\nclass Missions")
            .AppendLine("{");

        foreach (var mission in missions.Where(x => x.IsSelected))
        {
            stringBuilder.AppendLine(new string('\t', 1) + "class Mission_" + counter)
                .Append(new string('\t', 1))
                .AppendLine("{");
            counter++;

            foreach (var property in typeof(Mission).GetProperties())
            {
                var configProperty = property.GetCustomAttributes(true).FirstOrDefault() as ConfigProperty;
                if (configProperty?.IgnoreParsing != false || configProperty == null) continue;

                var value = property.GetValue(mission, null)
                    .ToString()
                    .SurroundWithCharacters("\"");

                stringBuilder.Append(new string('\t', 2))
                    .Append(configProperty.PropertyName)
                    .Append(" = ")
                    .Append(value)
                    .AppendLine(";");
            }

            stringBuilder.Append(new string('\t', 1)).AppendLine("};");
        }

        stringBuilder.AppendLine("};");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Converts the whitelisted missions to text.
    /// </summary>
    /// <param name="missions">Missions marked to play on server.</param>
    private string ConvertWhitelistedMissionsToText(IEnumerable<Mission> missions)
    {
        missions = missions.Where(x => x.IsWhitelisted).ToArray();
        if (!missions.Any()) return string.Empty;

        var missionAsStrings = missions.Select(m => m.Name).ToArray();
        return "\nmissionWhitelist[] = " + ParseArrayProperty(missionAsStrings) + ";";
    }

    /// <summary>
    /// Removes the mission whitelist tag.
    /// </summary>
    private string RemoveWhitelistField(string fileContent)
    {
        //TODO: this is quick workaround whitelist duping problem, need to rethink that moment
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            return string.Empty;
        }

        var tagIndex = fileContent.IndexOf("missionWhitelist[]");
        if (tagIndex == -1)
        {
            return fileContent;
        }

        var endIndex = fileContent.IndexOf("};", tagIndex);
        if (endIndex == -1)
        {
            return fileContent;
        }

        return fileContent.Remove(tagIndex, endIndex - tagIndex + 2);
    }

    /// <summary>
    /// Gets the missions from storage.
    /// </summary>
    /// <param name="folderPath">Path to searchable folder.</param>
    /// <returns>List of missions stored on hard drive.</returns>
    private List<Mission> GetMissionsFromStorage(string folderPath)
    {
        return _missionDao.GetAll(folderPath).ToList();
    }

    /// <summary>
    /// Parses the array config properties.
    /// </summary>
    /// <param name="valueAsArray">The value as array.</param>
    private string ParseArrayProperty(string[] valueAsArray)
    {
        if (valueAsArray == null || (valueAsArray.Length == 1 && string.IsNullOrEmpty(valueAsArray[0]))) return string.Empty;

        //TODO: new attribute to check if property has empty lines 
        for (int i = 0; i < valueAsArray.Length; i++)
        {
            valueAsArray[i] = Regex.Replace(valueAsArray[i], @"[^\S ]+", string.Empty);
            valueAsArray[i] = valueAsArray[i].Replace("\"", string.Empty);

            if (string.IsNullOrWhiteSpace(valueAsArray[i]))
            {
                valueAsArray[i] = Regex.Replace(valueAsArray[i], @"\s+", string.Empty);
            }
            valueAsArray[i] = "\"" + valueAsArray[i] + "\"";
        }

        if (valueAsArray.Any())
        {
            valueAsArray[0] = "\t" + valueAsArray[0];
        }

        return "{\n" + string.Join("\n\t,", valueAsArray) + "\n}";
    }
}
