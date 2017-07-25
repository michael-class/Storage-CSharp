using System.IO;
using Paycor.Neo.Common.Base;

namespace Paycor.Storage.Azure.Utils
{
    public class DelegatingStream : Stream
    {
        private readonly Stream _innerStream;

        public DelegatingStream(Stream innerStream)
        {
            BytesRead = 0;
            BytesWritten = 0;
            Check.NotNull(innerStream, "innerStream");

            _innerStream = innerStream;
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _innerStream.Read(buffer, offset, count);
            BytesRead += bytesRead;
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
            BytesWritten += count;
        }

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        public override long Length
        {
            get { return _innerStream.Length; }
        }

        public override long Position
        {
            get { return _innerStream.Position; } 
            set { _innerStream.Position = value; }
        }

        public long BytesRead { get; private set; }
        public long BytesWritten { get; private set; }

    }
}
