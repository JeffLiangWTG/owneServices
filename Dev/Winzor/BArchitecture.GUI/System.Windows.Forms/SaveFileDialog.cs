using WinzorFramework;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

public partial class SaveFileDialog : FileDialog
{
	public bool CreatePrompt { get; set; }

	public bool OverwritePrompt { get; set; }

	public Stream OpenFile()
	{
		if (fileReference is not null)
		{
			// If the dialog has already been shown, then return a file stream for the JS writable file that was seleted.
			return new SaveFileStream(fileReference);
		}
		else
		{
			if (string.IsNullOrEmpty(FileName))
			{
				throw new ArgumentNullException(nameof(FileName));
			}
			// Return a file steam while will show a dialog on the client when the stream is disposed
			return new SaveFileStream(FileName);
		}
	}

	protected override async Task RunDialogAsync(Form contextForm)
	{
		var types = ParseFilter();
		try
		{
			var fileService = contextForm.CargoWiseClientServices?.FileService;
			if (fileService != null)
			{
				fileReference = await fileService.ShowSaveFileDialogAsync(FileName ?? string.Empty, types);
				if (fileReference != null)
				{
					var fileName = fileReference.FileName;
					if (!string.IsNullOrWhiteSpace(fileName))
					{
						FileName = fileName;
						FileReferenceMap[fileName] = fileReference;
						var extension = Path.GetExtension(fileName).ToLower();
						if (types != null)
						{
							for (var i = 0; i < types.Count; i++)
							{
								if (types[i].Any(k => k.Value.Contains(extension)))
								{
									FilterIndex = i + 1;
									break;
								}
							}
						}
					}
					DialogResult = DialogResult.OK;
				}
			}
		}
		catch (Exception ex)
		{
			Application.ReportDeveloperException("Error when showing SaveFileDialog.", ex);
		}
	}

	IWritableBrowserFile? fileReference;

	static readonly Dictionary<string, IWritableBrowserFile> FileReferenceMap = new ();

	internal static IWritableBrowserFile? GetFileReference(string fileName)
	{
		FileReferenceMap.TryGetValue(fileName, out var writableFile);
		return writableFile;
	}

	internal static void RemoveFileReference(string fileName)
	{
		FileReferenceMap.Remove(fileName);
	}

	public override void Reset()
	{
		if (fileReference != null)
		{
			FileReferenceMap.Remove(fileReference.FileName);
			fileReference = null;
		}
		base.Reset();
	}
}
