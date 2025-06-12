using System.IO.Compression;

namespace eServices.Dms.Core.MessagesRepository;

public class CompressingStream : Stream
{
	private readonly Stream inputStream;
	private readonly bool leaveOpen;
	private readonly MemoryStream outputStream;
	private readonly GZipStream gzipStream;
	private bool inputStreamExhausted;
	private bool outputStreamExhausted;

	public CompressingStream(Stream inputStream, bool leaveOpen = false)
	{
		this.inputStream = inputStream;
		this.leaveOpen = leaveOpen;

		outputStream = new MemoryStream();
		gzipStream = new GZipStream(outputStream, CompressionMode.Compress);
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => throw new NotImplementedException();

	public override long Position { get => outputStream.Position; set => throw new NotImplementedException(); }

	public override int Read(byte[] buffer, int offset, int count)
	{
		int written = 0;
		var copyBuffer = new byte[count];

		while (!inputStreamExhausted || !outputStreamExhausted)
		{
			var read = outputStream.ReadAsync(buffer, offset + written, count - written).Result;
			if ((written += read) == count)
				return written;

			if (!inputStreamExhausted)
			{
				var copyCount = inputStream.ReadAsync(copyBuffer, 0, copyBuffer.Length).Result;
				if (copyCount == 0)
				{
					inputStreamExhausted = true;
				}
				else
				{
					var outputPos = outputStream.Position;
					gzipStream.Write(copyBuffer, 0, copyCount);
					gzipStream.Flush();
					outputStream.Position = outputPos;
				}
			}
			else
			{
				outputStreamExhausted = true;
			}
		}

		return written;
	}

	public override void Flush() => outputStream.Flush();

	public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

	public override void SetLength(long value) => throw new NotImplementedException();

	public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			outputStream.Dispose();
			if (!leaveOpen)
			{
				gzipStream.Dispose();
				inputStream.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	public override async ValueTask DisposeAsync()
	{
		if (!leaveOpen)
		{
			await inputStream.DisposeAsync();
		}
		await outputStream.DisposeAsync();
		await gzipStream.DisposeAsync();
		await base.DisposeAsync();
	}
}
