namespace Enterprise.Services.ServiceHost
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class MultiStream : Stream
	{
		readonly IReadOnlyList<StreamWithStartPosition> _streams;

		int _index;
		long _position;

		public MultiStream(IEnumerable<Stream> streams)
			: this(streams.ToArray())
		{
		}

		public MultiStream(params Stream[] streams)
		{
			_streams = streams.Select(s => new StreamWithStartPosition(s, s.Position)).ToList();
			_index = _streams?.Count > 0 ? 0 : -1;
		}

		public override bool CanRead => _streams?.All(s => s.Stream.CanRead) ?? true;

		public override bool CanSeek => _streams?.All(s => s.Stream.CanSeek) ?? true;

		public override bool CanWrite => false;

		public override long Length => _streams?.Sum(s => s.Length) ?? 0;

		public override long Position
		{
			get => _position;

			set => Seek(value, SeekOrigin.Begin);
		}

		public override void Flush()
		{
			var current = 0;

			while (current <= _index)
			{
				_streams[current++].Stream.Flush();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_index == -1)
			{
				return 0;
			}
			else
			{
				var totalBytesRead = 0;

				var bytesRead = _streams[_index].Stream.Read(buffer, offset, count);

				totalBytesRead += bytesRead;
				_position += bytesRead;

				while (bytesRead < count)
				{
					if (_index == _streams.Count - 1)
					{
						break;
					}

					offset += bytesRead;
					count -= bytesRead;
					_index++;

					bytesRead = _streams[_index].Stream.Read(buffer, offset, count);

					totalBytesRead += bytesRead;
					_position += bytesRead;
				}

				return totalBytesRead;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			var length = Length;

			if (length == 0)
			{
				_position = 0;
				_index = -1;
			}
			else
			{
				var value = 0L;

				switch (origin)
				{
					case SeekOrigin.Begin:
						value = 0 + offset;
						break;

					case SeekOrigin.Current:
						value = _position + offset;
						break;

					case SeekOrigin.End:
						value = length + offset;
						break;
				}

				if (value < 0)
				{
					value = 0;
				}
				else if (value >= length)
				{
					value = length - 1;
				}

				var accum = 0L;

				_index = -1;
				for (var i = 0; i < _streams.Count; i++)
				{
					var current = _streams[i];
					if (value >= accum)
					{
						if (value - accum > current.Length)
						{
							current.Position = current.Length - 1;
						}
						else
						{
							current.Position = value - accum;
						}
					}
					else
					{
						current.Position = 0;
					}

					accum += current.Length;

					if (accum > value && _index == -1)
					{
						_index = i;
					}
				}
				_position = value;
			}

			return _position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		class StreamWithStartPosition
		{
			public StreamWithStartPosition(Stream stream, long startPosition)
			{
				Stream = stream;
				StartPosition = startPosition;
			}

			public Stream Stream { get; }
			public long StartPosition { get; }
			public long Length => Stream.Length - StartPosition;
			public long Position
			{
				get { return Stream.Position - StartPosition; }
				set { Stream.Position = value + StartPosition; }
			}
		}
	}
}
