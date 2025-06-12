using System.Text;

namespace eServices.Dms.Core.MessagesRepository;

public class TextReaderStream(TextReader textReader, Encoding encoding, bool leaveOpen = false) : Stream
{
	private const int bufferSize = 1024;
	private byte[] readBuffer = Array.Empty<byte>();
	private long position = 0;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => throw new NotImplementedException();

	public override long Position { get => position; set => throw new NotImplementedException(); }

	public override int Read(byte[] buffer, int offset, int count)
	{
		int written = 0;

		while (true)
		{
			if (readBuffer.Length == 0)
			{
				var charBuffer = new char[bufferSize];
				var read = textReader.ReadBlock(charBuffer, 0, charBuffer.Length);
				if (read == 0)
				{
					return written;
				}
				readBuffer = encoding.GetBytes(charBuffer[..read]);
			}
			if (readBuffer.Length > 0)
			{
				int copyCount = Math.Min(readBuffer.Length, count);
				Array.Copy(readBuffer, 0, buffer, offset, copyCount);
				written += copyCount;
				if (copyCount < readBuffer.Length)
				{
					Array.Copy(readBuffer, copyCount, readBuffer, 0, readBuffer.Length - copyCount);
					Array.Resize(ref readBuffer, readBuffer.Length - copyCount);
				}
				else
				{
					readBuffer = Array.Empty<byte>();
				}
			}
		}
	}

	public override void Flush() => throw new NotImplementedException();

	public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

	public override void SetLength(long value) => throw new NotImplementedException();

	public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (!leaveOpen)
			{
				textReader.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	public override async ValueTask DisposeAsync()
	{
		if (!leaveOpen)
		{
			await Task.Run(() => textReader.Dispose());
		}
		await base.DisposeAsync();
	}
}
