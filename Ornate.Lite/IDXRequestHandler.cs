using System;
using System.IO;
using System.Net;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace Ornate.Lite
{
    public class IDXRequestHandler : RequestHandler
    {
        protected override CefResourceRequestHandler GetResourceRequestHandler(CefBrowser browser, CefFrame frame, CefRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (isNavigation)
                disableDefaultHandling = false;

            if (request.Url.EndsWith(".idx"))
                return new ResourceRequestHandler();
            else
                return base.GetResourceRequestHandler(browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }

        class ResourceRequestHandler : CefResourceRequestHandler
        {
            protected override CefCookieAccessFilter GetCookieAccessFilter(CefBrowser browser, CefFrame frame, CefRequest request)
            {
                return null;
            }

            protected override CefResourceHandler GetResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request)
            {
                var stream = new MemoryStream();
                stream.Write(File.ReadAllBytes(new Uri(request.Url).LocalPath));
                stream.Position = 0;
                return new RequestResultStream(stream, "text/html", HttpStatusCode.OK).MakeHandler();
            }

            internal sealed class RequestResultStream
            {
                private readonly Stream _stream;
                private readonly HttpStatusCode _code;
                private readonly string _contentType;

                public RequestResultStream(Stream stream, string contentType, HttpStatusCode code)
                {
                    _stream = stream;
                    _code = code;
                    _contentType = contentType;
                }

                public CefResourceHandler MakeHandler()
                {
                    return new Handler(_stream, _contentType, _code);
                }

                private sealed class Handler : CefResourceHandler
                {
                    // TODO: async
                    // TODO: exception handling

                    private readonly Stream _stream;
                    private readonly HttpStatusCode _code;
                    private readonly string _contentType;

                    public Handler(Stream stream, string contentType, HttpStatusCode code)
                    {
                        _stream = stream;
                        _code = code;
                        _contentType = contentType;
                    }

                    protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
                    {
                        handleRequest = true;
                        return true;
                    }

                    protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
                    {
                        response.Status = (int)_code;
                        response.StatusText = _code.ToString();
                        response.MimeType = _contentType;

                        if (_stream.CanSeek)
                            responseLength = _stream.Length;
                        else
                            responseLength = -1;

                        redirectUrl = default;
                    }

                    protected override bool Skip(long bytesToSkip, out long bytesSkipped, CefResourceSkipCallback callback)
                    {
                        if (!_stream.CanSeek)
                        {
                            bytesSkipped = -2;
                            return false;
                        }

                        bytesSkipped = _stream.Seek(bytesToSkip, SeekOrigin.Begin);
                        return true;
                    }

                    protected override unsafe bool Read(Stream dataOut, int bytesToRead, out int bytesRead, CefResourceReadCallback callback)
                    {
                        Span<byte> bytes = stackalloc byte[(int)_stream.Length];
                        bytesRead = _stream.Read(bytes);
                        dataOut.Write(bytes);
                        return bytesRead != 0;
                    }

                    protected override void Cancel()
                    {
                    }

                    protected override void Dispose(bool disposing)
                    {
                        base.Dispose(disposing);

                        _stream.Dispose();
                    }
                }
            }
        }
    }
}