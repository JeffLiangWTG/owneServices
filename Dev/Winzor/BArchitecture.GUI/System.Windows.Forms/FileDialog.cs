using System.Text.RegularExpressions;

namespace System.Windows.Forms;

public abstract partial class FileDialog : CommonDialog
{
	public string FileName
	{
		get => FileNames.Length > 0 ? FileNames[0] : string.Empty;
		set => FileNames = string.IsNullOrEmpty(value) ? Array.Empty<string>() : new string[] { value };
	}

	public string[] FileNames { get; protected set; } = Array.Empty<string>();

	/// <summary>
	///  In an inherited class, initializes a new instance of the <see cref="FileDialog"/> class.
	/// </summary>
	internal FileDialog()
	{
		Reset();
	}

	public override void Reset()
	{
		FileNames = Array.Empty<string>();
	}

	public bool AddExtension { get; set; }

	public string DefaultExt { get; set; } = string.Empty;

	public bool ValidateNames { get; set; }

	public virtual bool CheckFileExists { get; set; }

	public bool CheckPathExists { get; set; }

	public bool SupportMultiDottedExtensions { get; set; }

	public bool ShowHelp { get; set; }

	public string InitialDirectory { get; set; } = string.Empty;

	public string Filter { get; set; } = string.Empty;

	public int FilterIndex { get; set; } = 1;

	public bool DereferenceLinks { get; set; }

	public bool RestoreDirectory { get; set; }

	public string Title { get; set; } = string.Empty;

	public IList<Dictionary<string, string[]>>? ParseFilter()
	{
		if (string.IsNullOrWhiteSpace(Filter))
		{
			return null;
		}

		var filters = Filter.Split('|');
		if (filters.Length % 2 != 0)
		{
			throw BadFilterException();
		}

		var result = new List<Dictionary<string, string[]>>();
		var defaultExtAdded = false;
		var defaultExtLower = string.IsNullOrEmpty(DefaultExt) ? null : $".{DefaultExt.ToLower()}";
		for (var i = 0; i < filters.Length; i += 2)
		{
			var description = filters[i];
			if (description.Contains("All files"))
			{
				continue;
			}
			var extensions = filters[i + 1].Split(';').Select(ext => ext.Trim().Replace("*", "").ToLower()).ToArray();
			if (defaultExtLower != null && defaultExtLower.Split('|').All(i => extensions.Contains("." + i.Replace(".", "").Replace("*", ""))))
			{
				defaultExtAdded = true;
			}

			var redundantDesc = $" ?\\({string.Join(",", extensions.Select(ext => $" ?\\*?{Regex.Escape(ext)} ?"))}\\)$";
			description = Regex.Replace(description, redundantDesc, "", RegexOptions.IgnoreCase);

			result.Add(new Dictionary<string, string[]> { [description] = extensions });
		}

		if (defaultExtLower != null && !defaultExtAdded)
		{
			result.Insert(0, new Dictionary<string, string[]> { [""] = new[] { defaultExtLower } });
		}

		return result;

		Exception BadFilterException()
		{
			return new InvalidOperationException(
				"Filter string you provided is not valid. " +
				"The filter string must contain a description of the filter, followed by the vertical bar (|) and the filter pattern. " +
				"The strings for different filtering options must also be separated by the vertical bar. " +
				"Example: \"Text files (*.txt)|*.txt|All files (*.*)|*.*\"");
		}
	}
}
