using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;

namespace Ornate.Lite
{
    public partial class OptionsWindow : Window, INotifyPropertyChanged
    {
        // Event to notify when the options have changed
        public event PropertyChangedEventHandler PropertyChanged;

        private bool autoStart;
        private bool autoMute;
        private bool autoSniffer;

        public OptionsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Save the options and close the window
            bool autoStartValue = chkAutoStart.IsChecked ?? false;
            bool autoMuteValue = chkAutoMute.IsChecked ?? false;
            bool autoSnifferValue = chkAutoSniffer.IsChecked ?? false;

            // Save the options to the configuration file
            SaveOptions(autoStartValue, autoMuteValue, autoSnifferValue);

            // Close the options window
            Close();
        }

        private void SaveOptions(bool autoStartValue, bool autoMuteValue, bool autoSnifferValue)
        {
            // Get the configuration file path
            string configFileName = "OrnateLite.config";
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);

            // Load the configuration file
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = configFilePath;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            // Set the option values in the configuration
            config.AppSettings.Settings["AutoStart"].Value = autoStartValue.ToString();
            config.AppSettings.Settings["AutoMute"].Value = autoMuteValue.ToString();
            config.AppSettings.Settings["AutoSniffer"].Value = autoSnifferValue.ToString();

            // Save the configuration changes
            config.Save(ConfigurationSaveMode.Modified);
        }


        // Options properties
        public bool AutoStart
        {
            get { return autoStart; }
            set
            {
                autoStart = value;
                OnPropertyChanged(nameof(AutoStart));
            }
        }

        public bool AutoMute
        {
            get { return autoMute; }
            set
            {
                autoMute = value;
                OnPropertyChanged(nameof(AutoMute));
            }
        }

        public bool AutoSniffer
        {
            get { return autoSniffer; }
            set
            {
                autoSniffer = value;
                OnPropertyChanged(nameof(AutoSniffer));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Event handlers for checkboxes
        private void chkAutoStart_Checked(object sender, RoutedEventArgs e)
        {
            AutoStart = true;
        }

        private void chkAutoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoStart = false;
        }

        private void chkAutoMute_Checked(object sender, RoutedEventArgs e)
        {
            AutoMute = true;
        }

        private void chkAutoMute_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoMute = false;
        }

        private void chkAutoSniffer_Checked(object sender, RoutedEventArgs e)
        {
            AutoSniffer = true;
        }

        private void chkAutoSniffer_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoSniffer = false;
        }
    }
}
