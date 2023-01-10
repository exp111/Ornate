using Avalonia.Media;
using CefNet;
using CefNet.Internal;
using Ornate.Lite.CefNetStuff;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Ornate.Lite.CefNetStuff
{
	internal sealed class CustomWebViewGlue : AvaloniaWebViewGlue
	{
		public CustomWebViewGlue(CustomWebView view)
			: base(view)
		{

		}

		private new CustomWebView WebView
		{
			get { return (CustomWebView)base.WebView; }
		}

        protected override bool OnShowPermissionPrompt(CefBrowser browser, ulong promptId, string requestingOrigin, CefPermissionRequestTypes requestedPermissions, CefPermissionPromptCallback callback)
        {
			if (requestedPermissions == CefPermissionRequestTypes.Geolocation)
			{
				callback.Continue(CefPermissionRequestResult.Accept);
				return true;
			}
            return base.OnShowPermissionPrompt(browser, promptId, requestingOrigin, requestedPermissions, callback);
        }

        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source)
		{
			if (source == CefFocusSource.Navigation)
				return true;
			return false;
		}

		//FIXME: doesnt work
        protected override CefResourceRequestHandler GetResourceRequestHandler(CefBrowser browser, CefFrame frame, CefRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref int disableDefaultHandling)
        {
            if (request.Url.EndsWith(".idx"))
                return new IDXResourceRequestHandler();
            return base.GetResourceRequestHandler(browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }

		// Hide the context menu
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model)
		{
			model.Clear();
		}

		//TODO: do we need this?
		protected override void OnFullscreenModeChange(CefBrowser browser, bool fullscreen)
		{
			WebView.RaiseFullscreenModeChange(fullscreen);
		}

		protected override bool OnConsoleMessage(CefBrowser browser, CefLogSeverity level, string message, string source, int line)
		{
			Debug.Print("[{0}]: {1} ({2}, line: {3})", level, message, source, line);
			return false;
		}
	}
}
