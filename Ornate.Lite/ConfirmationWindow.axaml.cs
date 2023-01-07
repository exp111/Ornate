using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ornate.Lite
{
    public class ConfirmationWindow : Window, INotifyPropertyChanged
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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConfirmationWindow()
        {
            InitializeComponents();
        }

        void InitializeComponents()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
        }
    }
}
