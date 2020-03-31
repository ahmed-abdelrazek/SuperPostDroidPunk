using System;

namespace SuperPostDroidPunk.Core
{
    public class DbConfig
    {
        // Database file path
        public static string Path { get; set; } = $"{(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))}\\";

        // Datebase file name
        public static string Name { get; set; } = $"PostDroid.litedb";

        // Database ConnectionString
        public static string ConnectionString { get; set; } = $"Filename={Path}{Name};Password=IamBatman";

        // LiteDb Collection for Response as string
        public static string ResponseCollection { get; } = "Responses";
    }
}
