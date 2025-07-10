using System.Collections.Specialized;
using System.Drawing;
using System.Net.Mime;
using CargoWise.Blazor.Client.Integration;
using Microsoft.JSInterop;
using WinzorFramework;
using WTG.RtfConverter;

namespace System.Windows.Forms;

public static class Clipboard
{
	static readonly RtfToHtmlConverter rtfToHtmlConverter = new ()
	{
		SanitizeLinks = true
	};
	const string ClipboardJS = "/_content/WinzorFramework/js/module/clipboard.js";

	static Dictionary<string, string> fetchData = new Dictionary<string, string>();

	static IDataObject? _dataObject;

	/// <summary>
	///  Places nonpersistent data on the system <see cref="Clipboard"/>.
	/// </summary>
	public static void SetDataObject(object data) =>
		SetDataObject(data, false);

	/// <summary>
	///  Overload that uses default values for retryTimes and retryDelay.
	/// </summary>
	public static void SetDataObject(object data, bool copy) =>
		SetDataObject(data, copy, retryTimes: 10, retryDelay: 100);

	/// <summary>
	///  Places data on the system <see cref="Clipboard"/> and uses copy to specify whether the data
	///  should remain on the <see cref="Clipboard"/> after the application exits.
	/// </summary>
	public static void SetDataObject(object data, bool copy, int retryTimes, int retryDelay)
	{
		if (data is IDataObject dataObject)
		{
			_dataObject = dataObject;
		}
		else if (data is string dStr)
		{
			_dataObject = new DataObject("Text", dStr);
		}
		else
		{
			throw new NotSupportedException($"Clipboard doesn't support the {data.GetType()}!");
		}

		if (_dataObject is DataObject dObj)
		{
			var clipboardItems = new Dictionary<string, object>();
			var rtf = string.Empty;

			var formats = dObj.GetFormats();
			foreach (var format in formats)
			{
				if (format == nameof(TextDataFormat.Html))
				{
					clipboardItems.TryAdd(MediaTypeNames.Text.Html, (string)dObj.GetData(format));
				}
				else if (format == nameof(TextDataFormat.Text) || format == nameof(TextDataFormat.UnicodeText))
				{
					clipboardItems.TryAdd(MediaTypeNames.Text.Plain, (string)dObj.GetData(format));
				}
				else if (format == nameof(TextDataFormat.Rtf))
				{
					rtf = (string)dObj.GetData(format);
				}
			}
			if (!clipboardItems.ContainsKey(nameof(TextDataFormat.Html)) &&
				!string.IsNullOrEmpty(rtf))
			{
				clipboardItems.TryAdd(MediaTypeNames.Text.Html, rtfToHtmlConverter.Convert(rtf));
			}

			if (dObj.GetDataPresent(DataFormats.FileDrop))
			{
				clipboardItems.Clear();
				if (dObj.GetData(DataFormats.FileDrop) is string[] filePaths && filePaths.Length > 0)
				{
					foreach (var path in filePaths)
					{
						var fileStream = File.OpenRead(path);
						var fileStreamReference = new DotNetStreamReference(fileStream);
						clipboardItems.TryAdd(CustomMimeTypes.Application.FileDrop, fileStreamReference);
						clipboardItems.TryAdd(CustomMimeTypes.Application.FileDropFileName, Path.GetFileName(path));
					}
				}
			}

			var cwcs = WinzorDispatcher.Current.CurrentContext.Form?.CargoWiseClientServices;
			if (cwcs is not null)
			{
				WinzorDispatcher.Current.CurrentContext.InvokeRenderDispatcher(async Task () =>
				{
					var dotNetReference = DotNetObjectReference.Create(cwcs);
					var module = await cwcs.JSRuntime.InvokeAsync<IJSObjectReference>("import", ClipboardJS);
					await module.InvokeVoidAsync("clipboard.setDataObject", dotNetReference, clipboardItems, copy);
				});
			}
		}
	}

	public static void Clear()
	{
		SetDataObject(new DataObject());
	}

	public static async Task FetchClipboardDataAsync(IJSRuntime jSRuntime)
	{
		var module = await jSRuntime.InvokeAsync<IJSObjectReference>("import", ClipboardJS);
		var content = await module.InvokeAsync<Dictionary<string, ClipboardContent>?>("clipboard.getDataObject") ?? new Dictionary<string, ClipboardContent>();
		fetchData = (await Task.WhenAll(content.Select(async kvp =>
		{
			var contentData = await kvp.Value.GetContentAsync();
			return new KeyValuePair<string, string>(kvp.Key, contentData);
		})))
		.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}

	public static Dictionary<string, string> GetDataObjectDict()
	{
		return fetchData;
	}

	public static IDataObject? GetServerDataObject() => _dataObject;

	public static void SetAudio(byte[] audioBytes)
	{
		if (audioBytes is null)
		{
			throw new ArgumentNullException(nameof(audioBytes));
		}

		SetAudio(new MemoryStream(audioBytes));
	}

	public static void SetAudio(Stream audioStream)
	{
		if (audioStream is null)
		{
			throw new ArgumentNullException(nameof(audioStream));
		}

		IDataObject dataObject = new DataObject();
		dataObject.SetData(DataFormats.WaveAudio, false, audioStream);
		SetDataObject(dataObject, true);
	}

	public static void SetData(string format, object data)
	{
		if (format is null)
		{
			throw new ArgumentNullException(nameof(format));
		}

		if (string.IsNullOrWhiteSpace(format))
		{
			throw new ArgumentException(SR.DataObjectWhitespaceEmptyFormatNotAllowed, nameof(format));
		}

		// Note: We delegate argument checking to IDataObject.SetData, if it wants to do so.
		IDataObject dataObject = new DataObject();
		dataObject.SetData(format, data);
		SetDataObject(dataObject, true);
	}

	public static void SetFileDropList(StringCollection filePaths)
	{
		throw new NotImplementedException();
	}

	public static void SetImage(Image image)
	{
		if (image is null)
		{
			throw new ArgumentNullException(nameof(image));
		}

		IDataObject dataObject = new DataObject();
		dataObject.SetData(DataFormats.Bitmap, true, image);
		SetDataObject(dataObject, true);
	}

	public static void SetText(string text) => SetText(text, TextDataFormat.UnicodeText);

	public static void SetText(string text, TextDataFormat format)
	{
		if (string.IsNullOrEmpty(text))
		{
			throw new ArgumentNullException(nameof(text));
		}

		IDataObject dataObject = new DataObject();
		dataObject.SetData(ConvertToDataFormats(format), false, text);
		SetDataObject(dataObject, true);
	}

	static string ConvertToDataFormats(TextDataFormat format)
	{
		switch (format)
		{
			case TextDataFormat.Text:
				return DataFormats.Text;

			case TextDataFormat.UnicodeText:
				return DataFormats.UnicodeText;

			case TextDataFormat.Rtf:
				return DataFormats.Rtf;

			case TextDataFormat.Html:
				return DataFormats.Html;

			case TextDataFormat.CommaSeparatedValue:
				return DataFormats.CommaSeparatedValue;
		}

		return DataFormats.UnicodeText;
	}
}
