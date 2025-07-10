using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

public sealed partial class OpenFileDialog : FileDialog
{
	public override void Reset()
	{
		BrowserFiles = Array.Empty<BrowserFile>();
		base.Reset();
	}

	public bool Multiselect { get; set; }

	public bool ReadOnlyChecked { get; set; }

	public Stream? OpenFile() => FileNames.Length > 0 ? File.OpenRead(FileNames[0]) : null;

	public BrowserFile[] BrowserFiles { get; private set; } = Array.Empty<BrowserFile>();

	protected override async Task RunDialogAsync(Form contextForm)
	{
		var types = ParseFilter();
		var fileService = contextForm.CargoWiseClientServices?.FileService;
		if (fileService != null)
		{
			BrowserFiles = await fileService.ShowOpenFilePickerAsync(Multiselect, types);
			FileNames = Array.Empty<string>(); // Invalidate old cached records, wrapper class is expected to call ForceLocalFiles 

			if (BrowserFiles.Length > 0)
			{
				DialogResult = DialogResult.OK;
			}
		}
	}

	public void SetBrowserFilesAndForceLocalFiles(BrowserFile[] browserFiles, long maximumLimitSize)
	{
		BrowserFiles = browserFiles;
		ForceLocalFiles(maximumLimitSize);
	}

	public void ForceLocalFiles(long maximumLimitSize)
	{
		var cts = new CancellationTokenSource();
		var contextForm = WinzorDispatcher.Current.CurrentContext.Form ?? throw new InvalidOperationException("Cannot show dialog without a context form.");
		contextForm.InvokeRenderDispatcher(async () =>
		{
			try
			{
				var fileService = contextForm.CargoWiseClientServices?.FileService;
				if (fileService != null)
				{
					FileNames = await fileService.UploadFilesToServerAsync(BrowserFiles, maximumLimitSize, cts.Token);
				}
			}
			finally
			{
				await cts.CancelAsync();
			}
		});

		WinzorDispatcher.Current.RunMessageLoop(cts);
		cts = null;
	}
}
