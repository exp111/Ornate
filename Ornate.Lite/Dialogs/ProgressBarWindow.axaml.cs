using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ornate.Lite.Dialogs
{
    public class ProgressBarWindow : Window, INotifyPropertyChanged
    {
        // Needed to notify the view that a property has changed
        public event PropertyChangedEventHandler PropertyChanged;

        // The prompt shown at the top
        private string text;
        public string Text
        {
            get => text;

            set
            {
                if (value != text)
                {
                    text = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // The minimum value
        private int min = 0;
        public int Min
        {
            get => min;

            set
            {
                if (value != min)
                {
                    min = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // The maximum value
        private int max = 100;
        public int Max
        {
            get => max;

            set
            {
                if (value != max)
                {
                    max = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // The current Value
        private int _value = 0;
        public int Value
        {
            get => _value;

            set
            {
                if (value != _value)
                {
                    _value = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProgressBarWindow()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;
            ProgressBar a = new();
        }
    }
}
