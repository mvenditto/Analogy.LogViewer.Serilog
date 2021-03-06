﻿using Analogy.LogViewer.Serilog.Regex;
using System.Collections.Generic;

namespace Analogy.LogViewer.Serilog
{
    public enum SerilogFileFormat
    {
        CLEF,
        JSON,
        REGEX
    }
    public class SerilogSettings
    {
        public string FileOpenDialogFilters { get; set; }
        public string FileSaveDialogFilters { get; } = string.Empty;
        public List<string> SupportFormats { get; set; }
        public List<RegexPattern> RegexPatterns { get; set; }
        public string Directory { get; set; }
        public IDictionary<string,string> PropertyColumnMappings { get; set; }
        public SerilogFileFormat Format { get; set; }
        public string UserTimeZone { get; set; }

        public SerilogSettings()
        {
            Format = SerilogFileFormat.CLEF;
            Directory = string.Empty;
            FileOpenDialogFilters = "All Supported formats (*.Clef;*.log)|*.clef;*.log|Clef format (*.clef)|*.clef|Plain log text file (*.log)|*.log";
            SupportFormats = new List<string> { "*.Clef", "*.log" };
            RegexPatterns = new List<RegexPattern>();
            RegexPatterns.Add(new RegexPattern(@"\$(?<Date>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2},\d{3})+\|+(?<Thread>\d+)+\|(?<Level>\w+)+\|+(?<Source>.*)\|(?<Text>.*)", "yyyy-MM-dd HH:mm:ss,fff", ""));
            PropertyColumnMappings = null;
            UserTimeZone = string.Empty;
        }
    }
}
