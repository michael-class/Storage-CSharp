using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Paycor.Neo.Logging.Format;

namespace Paycor.Storage.Host.WebApi.Utils
{
    //TODO: move to Neo
    public class BinaryMediaTypeFormatter : MediaTypeFormatter
    {   
        private static readonly ILog _logger = LogManager.GetLogger<BinaryMediaTypeFormatter>();

        private static readonly Type _supportedType = typeof(Stream);

        public BinaryMediaTypeFormatter()
        {
            _logger.Info(m => m("{0}", LogEventFormatter.Format("BinaryMediaTypeFormatter").AddEvent("ctor")));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        }

        public override bool CanReadType(Type type)
        {
            return type == _supportedType;
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return ReadFromStreamAsync(type, readStream, content, formatterLogger, CancellationToken.None);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            _logger.Trace(m => m("ReadFromStreamAsync() type={0}", type));

            var taskSource = new TaskCompletionSource<object>();
            try
            {
                // just pass read/input stream thru
                taskSource.SetResult(readStream);
            }
            catch (Exception e)
            {
                taskSource.SetException(e);
            }
            return taskSource.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            return WriteToStreamAsync(type, value, writeStream, content, transportContext, CancellationToken.None);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext, CancellationToken cancellationToken)
        {
            _logger.Trace(m => m("WriteToStreamAsync() type={0}, value={1}", type, value));

            throw new NotSupportedException("BinaryMediaTypeFormatter cannot write objects");

            /*
            var taskSource = new TaskCompletionSource<object>();
            try
            {
                var inputStream = value as Stream;
                if (inputStream == null)
                {
                    throw new ArgumentException("Can only write from a Stream object");
                }
                inputStream.CopyTo(writeStream);
                taskSource.SetResult(null);
            }
            catch (Exception e)
            {
                taskSource.SetException(e);
            }
            return taskSource.Task;
             */
        }

    }
    
}