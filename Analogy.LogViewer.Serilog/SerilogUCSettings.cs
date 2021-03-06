﻿using Analogy.Interfaces;
using Analogy.LogViewer.Serilog.Managers;
using Analogy.LogViewer.Serilog.Regex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Linq;

namespace Analogy.LogViewer.Serilog
{
    public partial class SerilogUCSettings : UserControl
    {
        private SerilogSettings Settings => UserSettingsManager.UserSettings.Settings;
        private IReadOnlyCollection<TimeZoneInfo> _systemTimeZones;
        
        public SerilogUCSettings()
        {
            InitializeComponent();
            InitPropertyColumnMappingsTable();
            LoadSystemTimeZones();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void InitPropertyColumnMappingsTable()
        {
            foreach (var colPropPair in Constants.DefaultMappings)
            {
                propertyColumnMappingTable.Rows.Add(colPropPair.Key, colPropPair.Value);
            }
        }
        
        private void LoadSystemTimeZones()
        {
            _systemTimeZones =  TimeZoneInfo.GetSystemTimeZones();
            timeZoneComboBox.DataSource = _systemTimeZones;
        }
        
        public void SaveSettings()
        {
#if NETCOREAPP3_1
            Settings.SupportFormats = txtbSupportedFiles.Text.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
#endif
#if !NETCOREAPP3_1
            Settings.SupportFormats = txtbSupportedFiles.Text.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
#endif
            Settings.Directory = txtbDirectory.Text;
            Settings.FileOpenDialogFilters = txtbOpenFileFilters.Text;
            Settings.SupportFormats = txtbSupportedFiles.Text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            Settings.RegexPatterns = lstbRegularExpressions.Items.Count > 0 ? lstbRegularExpressions.Items.Cast<RegexPattern>().ToList() : new List<RegexPattern>();
            Settings.Format = rbtnCLEF.Checked
                ? SerilogFileFormat.CLEF
                : (rbRegexFile.Checked ? SerilogFileFormat.REGEX : SerilogFileFormat.JSON);
            Settings.PropertyColumnMappings = new Dictionary<string, string>();
            foreach (DataGridViewRow row in propertyColumnMappingTable.Rows)
            {
                var col = row.Cells[0].Value.ToString();
                var prop = row.Cells[1].Value.ToString();
                // is well-known column
                if (Constants.DefaultMappings.ContainsKey(col))
                {
                    Settings.PropertyColumnMappings.Add(col, prop);
                }
            }
            Settings.UserTimeZone = ((TimeZoneInfo)timeZoneComboBox.SelectedItem).Id;
            UserSettingsManager.UserSettings.Save();
        }

        private void btnExportSettings_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Analogy Serilog Settings (*.serilogsettings)|*.serilogsettings";
            saveFileDialog.Title = @"Export settings";

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SaveSettings();
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(Settings));
                    MessageBox.Show("File Saved", @"Export settings", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Export: " + ex.Message, @"Error Saving file", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Analogy Serilog Settings (*.Json)|*.json";
            openFileDialog1.Title = @"Import Serilog settings";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var json = File.ReadAllText(openFileDialog1.FileName);
                    SerilogSettings settings = JsonConvert.DeserializeObject<SerilogSettings>(json);
                    LoadSettings(settings);
                    MessageBox.Show("File Imported. Save settings if desired", @"Import settings", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Import: " + ex.Message, @"Error Import file", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void LoadSettings(SerilogSettings logSettings)
        {
            txtbDirectory.Text = logSettings.Directory;
            txtbOpenFileFilters.Text = logSettings.FileOpenDialogFilters;
            txtbSupportedFiles.Text = string.Join(";", logSettings.SupportFormats.ToList());
            lstbRegularExpressions.Items.Clear();
            lstbRegularExpressions.Items.AddRange(logSettings.RegexPatterns.ToArray());
            rbtnCLEF.Checked = logSettings.Format == SerilogFileFormat.CLEF;
            rbRegexFile.Checked = logSettings.Format == SerilogFileFormat.REGEX;
            rbJson.Checked = logSettings.Format == SerilogFileFormat.JSON;
            if (logSettings.PropertyColumnMappings != null)
            {
                propertyColumnMappingTable.Rows.Clear();
                // get mappings for each well-known column, ignore any other
                foreach (var x in Constants.DefaultMappings)
                {
                    var col = x.Key;
                    var propDefault = x.Value;
                    if (logSettings.PropertyColumnMappings.TryGetValue(col, out var prop))
                    {

                        var addedRow = propertyColumnMappingTable.Rows.Add(col, prop);
                        if (prop != propDefault)
                        {
                            propertyColumnMappingTable.Rows[addedRow].Cells[1].Style.ForeColor = Color.Blue;
                        }
                    }
                    else
                    {
                        propertyColumnMappingTable.Rows.Add(col, propDefault);
                    }
                }
            try
            {
                timeZoneComboBox.SelectedItem = TimeZoneInfo.FindSystemTimeZoneById(logSettings.UserTimeZone);
            }
            catch(Exception ex)
            {
                timeZoneComboBox.SelectedItem = null;
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtbDirectory.Text = fbd.SelectedPath;
                }
            }
        }

        private void NLogSettings_Load(object sender, EventArgs e)
        {
            LoadSettings(UserSettingsManager.UserSettings.Settings);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtbRegEx.Text)) return;
            var rp = new RegexPattern(txtbRegEx.Text, txtbDateTimeFormat.Text, txtbGuidFormat.Text);
            lstbRegularExpressions.Items.Add(rp);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstbRegularExpressions.SelectedItem is RegexPattern regexPattern)
            {
                lstbRegularExpressions.Items.Remove(lstbRegularExpressions.SelectedItem);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            RegexPattern p = new RegexPattern(txtbRegEx.Text, txtbDateTimeFormat.Text, "");
            bool valid = RegexParser.CheckRegex(txtbTest.Text, p, out AnalogyLogMessage m);
            if (valid)
            {
                lblResult.Text = "Valid Regular Expression";
                lblResult.BackColor = Color.GreenYellow;
                lblResultMessage.Text = m.ToString();
            }
            else
            {
                lblResult.Text = "Non Valid Regular Expression";
                lblResult.BackColor = Color.OrangeRed;
            }
        }

        private void btnTestFilter_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog
                {
                    Filter = txtbOpenFileFilters.Text,
                    Title = @"Test Open Files",
                    Multiselect = true
                };
                openFileDialog1.ShowDialog(this);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Incorrect filter: {exception.Message}", "Invalid filter text", MessageBoxButtons.OK);
            }
        }
    }
}
