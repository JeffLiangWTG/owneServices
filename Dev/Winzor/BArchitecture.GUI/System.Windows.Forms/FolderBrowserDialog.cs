namespace System.Windows.Forms;

public sealed class FolderBrowserDialog : CommonDialog
{
	public string SelectedPath { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	protected override async Task RunDialogAsync(Form contextForm)
	{
		try
		{
			var fileService = contextForm.CargoWiseClientServices?.FileService;
			if (fileService != null)
			{
				var fileName = await fileService.ShowDirectoryPickerAsync();
				if (!string.IsNullOrEmpty(fileName))
				{
					DialogResult = DialogResult.OK;
					SelectedPath = fileName;
				}
			}
		}
		catch (Exception ex)
		{
			Application.ReportDeveloperException("Error when showing FolderBrowserDialog.", ex);
		}
	}
}
