using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Selection;
using Avalonia.Markup.Xaml;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ornate.Lite
{
    public class SnifferWindow : Window //TODO: move sniffer func into browser(view), dont open this window on start, instead only show data from sniffer
    {
        public BrowserView BrowserView;
        public WebView2 Browser;
        public Sniffer Sniffer;

        public ListBox RequestList;
        public TextBlock RequestText;
        public TextBlock ResponseText;

        public SnifferWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

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
            var messages = Sniffer.Messages;
            var items = new List<ListBoxItem>();
            foreach (var message in messages)
            {
                items.Add(new() { Content = message.Key });
            }
            RequestList.Items = items;
        }

        public async void OnRequestListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == null)
                return;
            var text = (string)((ListBoxItem)selected).Content;
            if (!Sniffer.Messages.TryGetValue(text, out var message))
                return;

            var req = message.Request;
            var resp = message.Response;

            RequestText.Text = $"{req.Method} {req.Url}";

            //TODO: post
            // if (req.HasPostData != null)
            //var post = await BrowserView.DevTools.Network.GetRequestPostDataAsync(text);
            var body = "";
            try
            {
                var result = await BrowserView.DevTools.Network.GetResponseBodyAsync(text);
                body = result.Body;
            }
            catch (Exception ex)
            {
                body += ex.Message;
            }
            ResponseText.Text = body;
        }
    }
}
