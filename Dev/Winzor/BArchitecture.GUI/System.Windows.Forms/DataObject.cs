using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable

namespace System.Windows.Forms;

public partial class DataObject : IDataObject
{
	const string CF_DEPRECATED_FILENAME = "FileName";
	const string CF_DEPRECATED_FILENAMEW = "FileNameW";

	readonly IDataObject innerData;

	public DataObject()
	{
		innerData = new DataStore();
	}

	public DataObject(object data)
	{
		innerData = new DataStore();
		SetData(data);
	}

	public DataObject(string format, object data)
	{
		innerData = new DataStore();
		SetData(format, data);
	}

	public object GetData(string format, bool autoConvert)
	{
		if (format == nameof(DataFormats.Html))
		{
			// Clean the Html
			var htmlLines = ((string)innerData.GetData(format, autoConvert)).Split(Environment.NewLine);
			var stringBuilder = new StringBuilder();

			foreach (var line in htmlLines)
			{
				var lowerLine = line.ToLower();
				if (!lowerLine.StartsWith("version:") &&
					!lowerLine.StartsWith("starthtml:") &&
					!lowerLine.StartsWith("endhtml:") &&
					!lowerLine.StartsWith("startfragment:") &&
					!lowerLine.StartsWith("endfragment:") &&
					!lowerLine.StartsWith("startselection:") &&
					!lowerLine.StartsWith("endselection:"))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(Environment.NewLine);
					}
					stringBuilder.Append(line.Trim());
				}
			}

			var ignoreElements = new string[] { "<html>", "</html>", "<body>", "</body>", "<!--StartFragment-->", "<!--EndFragment-->" };
			foreach (var element in ignoreElements)
			{
				stringBuilder.Replace(element, string.Empty);
			}
			return stringBuilder.ToString();
		}
		return innerData.GetData(format, autoConvert);
	}

	public virtual object GetData(string format) =>
		GetData(format, true);

	public virtual object GetData(Type format)
	{
		if (format is null)
		{
			return null;
		}

		return GetData(format.FullName);
	}

	public bool GetDataPresent(string format, bool autoConvert) =>
		innerData.GetDataPresent(format, autoConvert);

	public bool GetDataPresent(string format) =>
		GetDataPresent(format, true);

	public bool GetDataPresent(Type format)
	{
		if (format is null)
		{
			return false;
		}

		return GetDataPresent(format.FullName);
	}

	public string[] GetFormats(bool autoConvert) =>
		innerData.GetFormats(autoConvert);

	public string[] GetFormats() =>
		GetFormats(true);

	public void SetData(string format, bool autoConvert, object data) =>
		innerData.SetData(format, autoConvert, data);

	public void SetData(string format, object data) =>
		innerData.SetData(format, data);

	public void SetData(Type format, object data) =>
		innerData.SetData(format, data);

	public void SetData(object data) =>
		innerData.SetData(data);

	static string[] GetMappedFormats(string format)
	{
		if (format is null)
		{
			return null;
		}

		if (format.Equals(DataFormats.Text)
			|| format.Equals(DataFormats.UnicodeText)
			|| format.Equals(DataFormats.StringFormat))
		{
			return new string[]
			{
				DataFormats.StringFormat,
				DataFormats.UnicodeText,
				DataFormats.Text,
			};
		}

		if (format.Equals(DataFormats.FileDrop)
			|| format.Equals(CF_DEPRECATED_FILENAME)
			|| format.Equals(CF_DEPRECATED_FILENAMEW))
		{
			return new string[]
			{
				DataFormats.FileDrop,
				CF_DEPRECATED_FILENAMEW,
				CF_DEPRECATED_FILENAME,
			};
		}

		if (format.Equals(DataFormats.Bitmap)
			|| format.Equals((typeof(Bitmap)).FullName))
		{
			return new string[]
			{
				(typeof(Bitmap)).FullName,
				DataFormats.Bitmap,
			};
		}

		return new string[] { format };
	}
}
