using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Specialized;
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
            // Read options //TODO: dont do this every time we open the option window?
            ReadOptions();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Save the options to the configuration file
            SaveOptions();

            // Close the options window
            Close();
        }

        private string ReadSetting<T>(string key, T fallback)
        {
            // get read only config
            var config = ConfigurationManager.AppSettings;
            var result = config[key] ?? fallback.ToString(); //TODO: convert to T
            return result;
        }

        private void SetSetting<T>(string key, T val)
        {
            // Open config file
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var config = configFile.AppSettings.Settings;

            if (config[key] != null)
                config[key].Value = val.ToString();
            else
                config.Add(key, val.ToString());

            // save
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        private void ReadOptions()
        {
            try
            {
                AutoStart = Boolean.Parse(ReadSetting("AutoStart", true));
                AutoMute = Boolean.Parse(ReadSetting("AutoMute", true));
                AutoSniffer = Boolean.Parse(ReadSetting("AutoSniffer", false));
            }
            catch (Exception e) 
            {
                //TODO: show to user
                throw new($"Failed reading options: {e}");
            }
        }

        private void SaveOptions()
        {
            //TODO: probably better to get config before and then set via extensions to not reopen a file x times
            try
            {
                // Set the option values in the configuration
                SetSetting("AutoStart", AutoStart);
                SetSetting("AutoMute", AutoMute);
                SetSetting("AutoSniffer", AutoSniffer);
            }
            catch (Exception e) 
            {
                //TODO: show to user
                throw new($"Failed saving options: {e}");
            }
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
