namespace ConsoleApp1
{
    public class MyCorruptingStreamWrapper : Stream
    {
        private Stream _stream;
        private int? _corruptByteOffset = null;
        private int _currentByteOffset = 0;

        public MyCorruptingStreamWrapper(Stream innerStream, int? corruptByteOffset = null)
        {
            _stream = innerStream;
            _corruptByteOffset = corruptByteOffset;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public override bool CanTimeout => _stream.CanTimeout;

        public override int WriteTimeout { get => _stream.WriteTimeout; set => _stream.WriteTimeout = value; }

        public override int ReadTimeout { get => _stream.ReadTimeout; set => _stream.ReadTimeout = value; }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            _stream.Close();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            _stream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _stream.EndRead(asyncResult);
        }

        public override ValueTask DisposeAsync()
        {
            return _stream.DisposeAsync();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _stream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _stream.FlushAsync(cancellationToken);
        }

        public override bool Equals(object? obj)
        {
            return _stream.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _stream.GetHashCode();
        }

        public override int Read(Span<byte> buffer)
        {
            return _stream.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _stream.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _stream.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _stream.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_corruptByteOffset.HasValue)
            {
                CorruptIndex(buffer, offset, count, _corruptByteOffset.Value);
            }

            Tracer.Verbose($"At {_currentByteOffset}, writing {count} bytes to stream");
            _stream.Write(buffer, offset, count);
            _currentByteOffset += count;
        }

        private void CorruptIndex(byte[] buffer, int offset, int count, int corruptIndex)
        {
            if (_currentByteOffset <= corruptIndex && corruptIndex < _currentByteOffset + count)
            {
                var oldValue = buffer[corruptIndex - _currentByteOffset];
                buffer[corruptIndex - _currentByteOffset] = (byte)~buffer[corruptIndex - _currentByteOffset];
                var newValue = buffer[corruptIndex - _currentByteOffset];

                Tracer.Verbose($"Corrupted byte at offset {corruptIndex}.  Old = {oldValue}   New = {newValue}");
            }
        }
    }
}
