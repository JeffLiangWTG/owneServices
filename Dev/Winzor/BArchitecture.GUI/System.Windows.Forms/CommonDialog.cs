using Microsoft.JSInterop;
using WinzorFramework;

namespace System.Windows.Forms;

public abstract class CommonDialog : Control
{
	public DialogResult ShowDialog() => ShowDialog(null);

	CancellationTokenSource? dialogCts;

	public DialogResult ShowDialog(IWin32Window? owner)
	{
		dialogCts = new CancellationTokenSource();
		var contextForm = (owner as Form ?? WinzorDispatcher.Current.CurrentContext.Form) ?? throw new InvalidOperationException("Cannot show dialog without a context form.");
		DialogResult = DialogResult.Cancel;
		contextForm.InvokeRenderDispatcher(async () =>
		{
			try
			{
				contextForm.CommonDialog = this;
				await RunDialogAsync(contextForm);
			}
			finally
			{
				await dialogCts.CancelAsync();
			}
		});
		WinzorDispatcher.Current.RunMessageLoop(dialogCts);
		dialogCts = null;
		contextForm.CommonDialog = null;
		return DialogResult;
	}

	protected abstract Task RunDialogAsync(Form contextForm);

	protected DialogResult DialogResult
	{
		get => dialogResult;
		set
		{
			dialogResult = value;
		}
	}
	DialogResult dialogResult;

	public virtual void Reset() { }

	internal void EndModalMessageLoop()
	{
		dialogCts?.Cancel();
		dialogCts?.Dispose();
		dialogCts = null;
	}
}
