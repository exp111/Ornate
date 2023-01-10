using Avalonia.Controls.Shapes;
using CefNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite.CefNetStuff
{
    class CefAppImpl : CefNetApplication
    {
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            base.OnBeforeCommandLineProcessing(processType, commandLine);

            //TODO: clean this up
            Console.WriteLine("ChromiumWebBrowser_OnBeforeCommandLineProcessing");
            Console.WriteLine(commandLine.CommandLineString);

            //commandLine.AppendSwitchWithValue("proxy-server", "127.0.0.1:8888");

            commandLine.AppendSwitch("ignore-certificate-errors");
            commandLine.AppendSwitchWithValue("remote-debugging-port", "9222");

            //enable-devtools-experiments
            commandLine.AppendSwitch("enable-devtools-experiments");

            //("force-device-scale-factor", "1");

            //commandLine.AppendSwitch("disable-gpu");
            //commandLine.AppendSwitch("disable-gpu-compositing");
            //commandLine.AppendSwitch("disable-gpu-vsync");

            commandLine.AppendSwitch("enable-begin-frame-scheduling");
            commandLine.AppendSwitch("enable-media-stream");

            commandLine.AppendSwitchWithValue("enable-blink-features", "CSSPseudoHas");
            // Enable devtools log
            commandLine.AppendSwitchWithValue("devtools-protocol-log-file", System.IO.Path.GetFullPath("devtools.log"));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                commandLine.AppendSwitch("no-zygote");
                commandLine.AppendSwitch("no-sandbox");
            }
        }

        //TODO: whats this for
        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            base.OnContextCreated(browser, frame, context);
            frame.ExecuteJavaScript(@"
{
const newProto = navigator.__proto__;
delete newProto.webdriver;
navigator.__proto__ = newProto;
}", frame.Url, 0);

        }

        public Action<long> ScheduleMessagePumpWorkCallback { get; set; }

        // Used to call to the ui thread
        protected override void OnScheduleMessagePumpWork(long delayMs)
        {
            ScheduleMessagePumpWorkCallback(delayMs);
        }
    }
}
