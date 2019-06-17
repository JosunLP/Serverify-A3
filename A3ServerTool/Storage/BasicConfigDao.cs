﻿using A3ServerTool.Models;
using A3ServerTool.Models.Config;
using Interchangeable.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace A3ServerTool.Storage
{
    /// <inheritdoc/>
    public class BasicConfigDao : IConfigDao<BasicConfig>
    {
        private static string RootFolder => AppDomain.CurrentDomain.BaseDirectory;

        /// <inheritdoc/>
        public BasicConfig Get(Profile profile)
        {
            var file = FileHelper.GetFile(Path.Combine(RootFolder, Profile.StorageFolder, profile.Id.ToString(),
                    profile.BasicConfig.FileName) + profile.BasicConfig.FileExtension);
            if (file == null) return null;

            var properties = TextManager.ReadFileLineByLine(file);
            if (!properties.Any()) return null;

            var result = TextParseHandler.Parse<BasicConfig>(properties);
            result.FileLocation = file.FullName;
            return result;
        }

        /// <inheritdoc/>
        public void Save(Profile profile)
        {
            var configDto = new SaveDataDto
            {
                Content = string.Join("\r\n", TextParseHandler.ConvertToText(profile.BasicConfig)),
                FileExtension = profile.BasicConfig.FileExtension,
                FileName = profile.BasicConfig.FileName,
                Folders = new List<string>
                    {
                        RootFolder,
                        Profile.StorageFolder,
                        profile.Id.ToString()
                    }
            };

            profile.BasicConfig.FileLocation = configDto.GetFullPath();
            FileHelper.Save(configDto);
        }
    }
}
