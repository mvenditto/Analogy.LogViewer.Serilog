﻿using System.Collections.Generic;

namespace Analogy.LogViewer.Serilog
{
    public static class Constants
    {
        public static string Source { get; } = global::Serilog.Core.Constants.SourceContextPropertyName;
        public static string ThreadId { get; } = "ThreadId";
        public static string ProcessId { get; } = "ProcessId";
        public static string ProcessName { get; } = "ProcessName";
        public static string MachineName { get; } = "MachineName";
        public static string EnvironmentUserName { get; } = "EnvironmentUserName";

        public static IDictionary<string, string> DefaultMappings = new Dictionary<string, string>
        {
            {Source, Source},
            {ThreadId, ThreadId},
            {ProcessId, ProcessId},
            {ProcessName, ProcessName},
            {MachineName, MachineName},
            {EnvironmentUserName, EnvironmentUserName}
        };
    }
}
