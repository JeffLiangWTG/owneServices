using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Interop.DataObjects;

public class ZDataObject : BaseZDataObjectWithParentLinks, IDataObject
{
	readonly DataObject inner;
	protected ZDataObject()
	{
		inner = new DataObject();
	}

	protected ZDataObject(object data)
	{
		if (data is DataObject)
		{
			inner = (DataObject)data;
		}
		else
		{
			inner = new DataObject(data);
		}
	}

	protected ZDataObject(string format, object data)
	{
		if (data is DataObject)
		{
			inner = (DataObject)data;
		}
		else
		{
			inner = new DataObject(format, data);
		}
	}

	public static ZDataObject FromData(object data) => FromData(null, data);

	public static ZDataObject FromData(string format, object data) => FromDataCore(format, data, -1, string.Empty);

	public static ZDataObject FromDataWithMaximumLimitSizeInMB(object data, int maximumLimitSizeInMB, string notificationMessage)
	{
		Argument.NotNullOrEmpty(notificationMessage, nameof(notificationMessage));

		return FromDataCore(null, data, maximumLimitSizeInMB, notificationMessage);
	}

	static ZDataObject GetValidDataObject(ZDataObject data, int maximumLimitSizeInMB, string notificationMessage)
	{
		if (maximumLimitSizeInMB > 0)
		{
			var maximumLimitSize = maximumLimitSizeInMB * 1024 * 1024;
			var files = data.FileDropNames;
			var largeFileNames = new ArrayList();
			var dataFiles = new ArrayList();

			for (int i = 0; i < files.Length; i++)
			{
				var fileName = files[i];
				var fileInfo = new FileInfo(fileName);
				if (fileInfo.Exists)
				{
					if (fileInfo.Length > maximumLimitSize)
					{
						largeFileNames.Add(fileInfo.Name);
					}
					else
					{
						dataFiles.Add(fileName);
					}
				}
			}

			if (largeFileNames.Count > 0)
			{
				return new ZDataObject(DataFormats.FileDrop, (string[])dataFiles.ToArray(typeof(string))) { InfoNeedsToBeNotified = GetLargeFileNotificationMessage(notificationMessage, largeFileNames) };
			}
		}
		return data;
	}
	protected static string GetLargeFileNotificationMessage(string notificationMessage, ArrayList largeFileNames) => string.Format("{0}\r\n{1}", notificationMessage, string.Join("\r\n", largeFileNames.ToArray()));

	static ZDataObject FromDataCore(string format, object data, int maximumLimitSizeInMB, string notificationMessage)
	{
		var result = data as ZDataObject;
		if (result == null)
		{
			var constructedDataObject = (format == null) ? new ZDataObject(data) : new ZDataObject(format, data);
			try
			{
				if (constructedDataObject.FileDropCount > 0)
				{
					result = GetValidDataObject(constructedDataObject, maximumLimitSizeInMB, notificationMessage);
				}

				if (result == null)
				{
					result = ZFormattedTextDataObject.TryNewInstance(constructedDataObject);
				}

				if (result == null)
				{
					result = constructedDataObject;
				}
			}
			finally
			{
				if (result != constructedDataObject)
				{
					constructedDataObject.Dispose();
					if (result != null)
					{
						result.NeedTempFile = true;
					}
				}
			}
		}
		return result;
	}

	public bool NeedTempFile { get; private set; }

	public virtual object GetData(string format, bool autoConvert) => Inner.GetData(format, autoConvert);

	public virtual object GetData(string format) => Inner.GetData(format);

	public object GetData(Type format) => Inner.GetData(format);

	public virtual bool GetDataPresent(string format, bool autoConvert) => Inner.GetDataPresent(format, autoConvert);

	public virtual bool GetDataPresent(string format) => Inner.GetDataPresent(format);

	public bool GetDataPresent(Type format) => Inner.GetDataPresent(format);

	public string[] GetFormats(bool autoConvert) => Inner.GetFormats(autoConvert);

	public virtual string[] GetFormats() => Inner.GetFormats();

	public void SetData(string format, bool autoConvert, object data) => Inner.SetData(format, autoConvert, data);

	public void SetData(string format, object data) => Inner.SetData(format, data);

	public void SetData(Type format, object data) => Inner.SetData(format, data);

	public void SetData(object data) => Inner.SetData(data);

	protected internal virtual DataObject Inner => inner;

	public string InfoNeedsToBeNotified { get; protected set; }

	public long Size
	{
		get
		{
			long result = -1;
			var embeddedObject = Inner.GetData(EmbeddedObjectDataFormat) as MemoryStream;
			var rtf = (string)GetData(DataFormats.Rtf);

			if (rtf != null)
			{
				result = rtf.Length;
			}
			else if (embeddedObject != null)
			{
				result = embeddedObject.Length;
			}

			return result;
		}
	}

	public string SingleFileDrop => (FileDropNames != null && FileDropNames.Length == 1) ? FileDropNames[0] : null;

	public int FileDropCount => FileDropNames == null ? 0 : FileDropNames.Length;

	protected string[] FileDropNames => fileDropNames ?? (fileDropNames = Inner.GetData(DataFormats.FileDrop) as string[]);
	string[] fileDropNames;

	public virtual bool SupportsEDocs => GetDataPresent(DataFormats.Bitmap) || GetDataPresent(DataFormats.Tiff) || GetDataPresent(DataFormats.Dib) || GetDataPresent(DataFormats.FileDrop);

	public virtual string Caption
	{
		get
		{
			var result = "";

			if (caption == null)
			{
				if (!string.IsNullOrEmpty(SingleFileDrop))
				{
					result = new FileInfo(SingleFileDrop).Name;
				}
			}
			else
			{
				result = caption;
			}

			return result;
		}
		set => caption = value;
	}
	protected string caption;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	protected const string EmbeddedObjectDataFormat = "Embedded Object";
}
