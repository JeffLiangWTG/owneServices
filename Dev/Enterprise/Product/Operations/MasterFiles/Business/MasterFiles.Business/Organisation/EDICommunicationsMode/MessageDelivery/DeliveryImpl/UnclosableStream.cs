using System.IO;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class UnclosableStream : Stream
	{
		public UnclosableStream(Stream wrappedStream)
		{
			this.wrappedStream = wrappedStream;
		}
		readonly Stream wrappedStream;

		public override void Close()
		{
			Seek(0, SeekOrigin.Begin);
		}

		#region Pass-throughs to wrappedStream

		public override bool CanRead
		{
			get { return wrappedStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return wrappedStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return wrappedStream.CanWrite; }
		}

		public override void Flush()
		{
			wrappedStream.Flush();
		}

		public override long Length
		{
			get { return wrappedStream.Length; }
		}

		public override long Position
		{
			get { return wrappedStream.Position; }
			set { wrappedStream.Position = value; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return wrappedStream.Read(buffer, offset, count); // It's a pass through only to whatever type of stream you want to use.
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return wrappedStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			wrappedStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			wrappedStream.Write(buffer, offset, count);
		}

		#endregion
	}
}
