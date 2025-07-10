using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using WinzorFramework;

namespace System.Windows.Forms;

internal enum CursorType
{
	System,
	SystemCursor
}

public class Cursor
{
	public Cursor(Stream s)
	{
		CursorData = string.Empty;
	}

	internal Cursor(string cursor, CursorType cursorType)
	{
		CursorData = cursor;
		CursorType = cursorType;
	}

	internal string CursorData { get; }
	internal CursorType CursorType { get; }

	public override string ToString() => CursorData;

	public static Point Position { get; set; }

	public static Cursor Current
	{
		get => current ?? Cursors.Default;
		set
		{
			if (value?.CursorData == Cursors.WaitCursor.CursorData)
			{
				if (notRespondingTask == null && WinzorDispatcher.HasCurrentContext)
				{
					var form = WinzorDispatcher.Current.CurrentContext.Form;
					if (form != null)
					{
						notRespondingTask = form.SetNotRespondingAsync();
					}
				}
				current = value;
			}
			else if (value == null || value.CursorData == Cursors.Default.CursorData)
			{
				if (notRespondingTask != null)
				{
					WinzorDispatcher.Current.CurrentContext.RegisterRenderTask(DisposeNotRespondingStateAsync(notRespondingTask));
					notRespondingTask = null;
				}
				current = value;
			}
			else
			{
				Application.OnThreadException(new NotSupportedException($"Winzor does not support setting Cursor.Current to {value}, only WaitCurosr and Default are allowed"));
			}
		}
	}

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks")]
	static async Task DisposeNotRespondingStateAsync(Task<IAsyncDisposable?> notRespondingTask)
	{
		var disposable = await notRespondingTask.ConfigureAwait(false);
		if (disposable != null)
		{
			await disposable.DisposeAsync().ConfigureAwait(false);
		}
	}

	[ThreadStatic]
	static Cursor? current;

	[ThreadStatic]
	static Task<IAsyncDisposable?>? notRespondingTask;

	public static Rectangle Clip { get; set; }
}
