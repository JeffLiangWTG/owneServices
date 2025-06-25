using System;
using System.IO;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public class VirtualStream : Stream, ICloneable
	{
		const int DefaultBufferSize = 0x1000;
		const long SwitchToFileMemoryUsageLimitInBytes = 900 * 1024 * 1024;
		const int DefaultSwitchToFileLimitInBytes = 32 * 1024;

		public VirtualStream()
			: this(DefaultSwitchToFileLimitInBytes)
		{
		}

		public VirtualStream(int switchToFileLimitInBytes)
		{
			SwitchToFileLimitInBytes = switchToFileLimitInBytes;
			mainStream = new MemoryStream();
			tempFilePath = "";
		}

		public bool IsSwitchedToFile
		{
			get { return !string.IsNullOrEmpty(tempFilePath); }
		}

		#region Stream Overrides

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void Flush()
		{
			ThrowIfDisposed();
			mainStream.Flush();
		}

		public override long Length
		{
			get
			{
				ThrowIfDisposed();
				return mainStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				ThrowIfDisposed();
				return mainStream.Position;
			}
			set
			{
				ThrowIfDisposed();
				mainStream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			ThrowIfDisposed();
			return mainStream.Read(buffer, offset, count);  // SuppressCodeSmell Reason = Simply call base Read Method
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			ThrowIfDisposed();
			return mainStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			ThrowIfDisposed();
			mainStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			ThrowIfDisposed();
			if (!IsSwitchedToFile && mainStream.Position + count > SwitchToFileLimitInBytes)
			{
				SwitchMemoryStreamToFileStream();
			}

			try
			{
				WriteToMainStream(buffer, offset, count);
			}
			catch (IOException exception)
			{
				mainStream.Close();
				throw new IOException(string.Format("VirtualMemoryStream unable to write to a file {0} on {1} machine - {2}", tempFilePath, Environment.MachineName, exception.Message), exception);
			}
			catch (Exception)
			{
				mainStream.Close();
				throw;
			}
		}

		#endregion

		#region IClonable

		public object Clone()
		{
			ThrowIfDisposed();
			var clone = new VirtualStream(switchToFileLimitInBytes);
			CopyStream(clone, this);
			return clone;
		}

		#endregion

		#region IDisposable

		protected override void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				if (mainStream != null) mainStream.Dispose();
			}
			disposed = true;
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		Stream mainStream;
		string tempFilePath;
		bool disposed;
		int switchToFileLimitInBytes;

		protected virtual long MemoryUsage
		{
			get { return GC.GetTotalMemory(false); }
		}

		static bool Is64BitProcess
		{
			get { return IntPtr.Size == 8; }
		}

		int SwitchToFileLimitInBytes
		{
			get { return Is64BitProcess || MemoryUsage < SwitchToFileMemoryUsageLimitInBytes ? switchToFileLimitInBytes : 0; }
			set { switchToFileLimitInBytes = value; }
		}

		void ThrowIfDisposed()
		{
			if (disposed) throw new ObjectDisposedException(this.GetType().Name);
		}

		protected virtual void SwitchMemoryStreamToFileStream()
		{
			Stream? fileStream = null;

			try
			{
				tempFilePath = GetTempFilePath();
				fileStream = File.Create(tempFilePath, DefaultBufferSize, FileOptions.DeleteOnClose);
				CopyStream(fileStream, mainStream);
				mainStream.Close();
				mainStream = fileStream;
			}
			catch (IOException exception)
			{
				if (fileStream != null) fileStream.Close();
				throw new IOException(string.Format("VirtualMemoryStream unable to switch to a file {0} on {1} machine - {2}", tempFilePath, Environment.MachineName, exception.Message), exception);
			}
		}

		protected virtual void CopyStream(Stream destination, Stream source)
		{
			var buffer = new byte[DefaultSwitchToFileLimitInBytes];
			var originalPosition = source.Position;

			try
			{
				source.Position = 0;

				while (true)
				{
					int readCount = source.Read(buffer, 0, buffer.Length);
					if (readCount == 0) break;
					destination.Write(buffer, 0, readCount);
				}

				destination.Flush();
				destination.Position = originalPosition;
			}
			finally
			{
				source.Position = originalPosition;
			}
		}

		protected virtual string GetTempFilePath()
		{
			return Path.GetTempFileName();
		}

		protected virtual void WriteToMainStream(byte[] buffer, int offset, int count)
		{
			mainStream.Write(buffer, offset, count);
		}

		#endregion
	}
}
