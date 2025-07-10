using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

public class SaveFileStream : Stream
{
	public SaveFileStream()
	{
		inner = new MemoryStream();
	}

	public SaveFileStream(IWritableBrowserFile fileReference)
		: this()
	{
		this.fileReference = fileReference;
	}

	public SaveFileStream(string fileName)
		: this()
	{
		if ((Path.GetDirectoryName(fileName)?.IndexOfAny(Path.GetInvalidPathChars()) ?? -1) > -1 || Path.GetFileNameWithoutExtension(fileName).IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
		{
			throw new ArgumentException("Invalid file name");
		}

		this.fileName = fileName;
		this.fileReference = SaveFileDialog.GetFileReference(fileName);
	}

	IWritableBrowserFile? fileReference;

	readonly string fileName = string.Empty;

	public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

	public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

	public override void Flush() => inner.Flush();

	public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

	public override void SetLength(long value) => inner.SetLength(value);

	public override bool CanRead => inner.CanRead;

	public override bool CanSeek => inner.CanSeek;

	public override bool CanWrite => inner.CanWrite;

	public override long Length => inner.Length;

	public override long Position { get => inner.Position; set => inner.Position = value; }

	internal bool IsDisposed
	{
		get;
		private set;
	}

	protected override void Dispose(bool disposing)
	{
		if (IsDisposed)
		{
			return;
		}

		base.Dispose(disposing);
		if (disposing)
		{
			var contextForm = WinzorDispatcher.Current.CurrentContext.Form;
			if (contextForm != null)
			{
				contextForm.InvokeRenderDispatcher(async () =>
				{
					try
					{
						if (fileReference is null)
						{
							fileReference = await (contextForm?.CargoWiseClientServices?.FileService.SaveFileToDirectoryAsync(fileName) ?? Task.FromResult<IWritableBrowserFile?>(null));
							if (fileReference is null)
							{
								return;
							}
						}
						await fileReference.WriteAsync(inner);
					}
					catch (Exception ex) when (ex is JSException || ex is TimeoutException || ex is IOException || ex is NotSupportedException)
					{
						throw new IOException($"Failed to save file: {fileName}", ex);
					}
					finally
					{
						if (fileReference is not null)
						{
							await fileReference.DisposeAsync();
						}
						await inner.DisposeAsync();
						SaveFileDialog.RemoveFileReference(fileName);
					}
				});
			}
			else
			{
				inner.Dispose();
			}
		}

		IsDisposed = true;
	}

	readonly MemoryStream inner;
}
