using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ornate.Lite.Dialogs
{
    public partial class ConfirmationWindow : Window, INotifyPropertyChanged
    {
        // Needed to notify the view that a property has changed
        public event PropertyChangedEventHandler PropertyChanged;

        // The prompt shown at the top
        private string prompt;
        public string Prompt
        {
            get => prompt;

            set
            {
                if (value != prompt)
                {
                    prompt = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConfirmationWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e) => Close(true);

        private void OnAbortClick(object sender, RoutedEventArgs e) => Close(false);
    }
}
