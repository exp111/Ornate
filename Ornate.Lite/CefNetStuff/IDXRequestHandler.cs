using CefNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite.CefNetStuff
{
    public class IDXResourceRequestHandler : CefResourceRequestHandler
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

                protected override bool Open(CefRequest request, ref int handleRequest, CefCallback callback)
                {
                    handleRequest = 1;
                    return true;
                }

                protected override void GetResponseHeaders(CefResponse response, ref long responseLength, ref string redirectUrl)
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

                protected override bool Skip(long bytesToSkip, ref long bytesSkipped, CefResourceSkipCallback callback)
                {
                    if (!_stream.CanSeek)
                    {
                        bytesSkipped = -2;
                        return false;
                    }

                    bytesSkipped = _stream.Seek(bytesToSkip, SeekOrigin.Begin);
                    return true;
                }

                protected override unsafe bool Read(nint dataOut, int bytesToRead, ref int bytesRead, CefResourceReadCallback callback)
                {
                    var byteSpan = new Span<byte>((void*)dataOut, bytesToRead);

                    bytesRead = _stream.Read(byteSpan);

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
