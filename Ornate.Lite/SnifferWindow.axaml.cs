using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ornate.Lite
{
    public class SnifferWindow : Window, INotifyPropertyChanged //TODO: move sniffer func into browser(view), dont open this window on start, instead only show data from sniffer
    {
        public BrowserView BrowserView;
        public WebView2 Browser;
        public Sniffer Sniffer;

        public ListBox RequestList;
        public TextBlock RequestText;
        public TextBlock ResponseText;

        private bool hideLocalRequests = true;
        private bool showOnlyOrnaRequests = false;
        private bool hideResourceRequests = false;
        private bool hideOptionsRequests = true;

        #region Bindings
        // Needed to notify the view that a property has changed
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool HideLocalRequests
        {
            get => hideLocalRequests;

            set
            {
                if (value != hideLocalRequests)
                {
                    hideLocalRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ShowOnlyOrnaRequests
        {
            get => showOnlyOrnaRequests;

            set
            {
                if (value != showOnlyOrnaRequests)
                {
                    showOnlyOrnaRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HideResourceRequests
        {
            get => hideResourceRequests;

            set
            {
                if (value != hideResourceRequests)
                {
                    hideResourceRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HideOptionRequests
        {
            get => hideOptionsRequests;

            set
            {
                if (value != hideOptionsRequests)
                {
                    hideOptionsRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        public SnifferWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;

            if (Design.IsDesignMode)
                return;

            RequestList = this.FindControl<ListBox>("RequestList");
            RequestText = this.FindControl<TextBlock>("RequestText");
            ResponseText = this.FindControl<TextBlock>("ResponseText");

            var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var mainWindow = lifetime.MainWindow;
            if (mainWindow == null)
                return;
            BrowserView = mainWindow.FindControl<BrowserView>("browser");
            Browser = BrowserView.browser;
            Sniffer = BrowserView.sniffer;

            // Fill message list with messages //TODO: autorefresh or refresh button 
            GenerateRequestList(); //TODO: doesnt work
        }

        public void GenerateRequestList()
        {
            var messages = Sniffer.Messages;
            var items = new List<ListBoxItem>();
            foreach (var message in messages)
            {
                var text = message.Key;
                var req = message.Value.Request;
                if (req != null)
                {
                    var uri = new Uri(req.Url);
                    // local file check
                    if (hideLocalRequests && uri.IsFile)
                        continue;

                    // host check
                    if (showOnlyOrnaRequests && !uri.Host.Equals("playorna.com"))
                        continue;

                    // option method check
                    if (hideOptionsRequests && req.Method.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    //TODO: resource check //if (hideResourceRequests &&

                    text = $"{req.Method} {req.Url}";
                }

                items.Add(new() { Content = text, Tag = message.Key });
            }
            RequestList.Items = items;
        }

        public void OnFilterCheckboxClick(object sender, RoutedEventArgs e)
        {
            // Regenerate the request list
            GenerateRequestList();
        }

        public async void OnRequestListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == null)
                return;
            var text = (string)((ListBoxItem)selected).Tag;
            if (!Sniffer.Messages.TryGetValue(text, out var message))
                return;

            var req = message.Request;
            var resp = message.Response;

            // Get and set request body
            var reqText = $"{req.Method} {req.Url}";
            reqText += "\n";
            reqText += req.Headers.ToString(); //TODO: get headers

            // post data, if it exists
            if (req.HasPostData != null)
            {
                reqText += "\n\n";
                try
                {
                    var post = await BrowserView.DevTools.Network.GetRequestPostDataAsync(text);
                    reqText += post;
                } 
                catch (Exception ex) 
                {
                    reqText += $"Exception: {ex.Message}";
                }
            }
            RequestText.Text = reqText;

            // Get and set response body
            var body = "";
            if (resp == null)
            {
                body = "No Response received";
            }
            else
            {
                try
                {
                    var result = await BrowserView.DevTools.Network.GetResponseBodyAsync(text);
                    body = result.Body;
                }
                catch (Exception ex)
                {
                    body += $"Exception: {ex.Message}";
                }
            }
            ResponseText.Text = body;
        }
    }
}
