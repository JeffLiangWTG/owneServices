using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;

namespace CargoWise.Interop.DataObjects;

public interface IEmbeddedRtfImageSource
{
	EmbeddedRtfImageFileInfo[] ImageFiles { get; }
	bool Valid { get; }

	void FinaliseRtf();
}

public class EmbeddedRtfImageFileInfo
{
	public EmbeddedRtfImageFileInfo(string filePath, string fileCaption)
	{
		this.filePath = filePath;
		this.fileCaption = fileCaption;
	}

	public readonly string filePath;
	public string fileCaption;
}

class ZFormattedTextDataObject : ZDataObject, IEmbeddedRtfImageSource
{
	#region RtfTextAndEmbeddedImages

	internal class RtfTextAndEmbeddedImages : IDisposable
	{
		public RtfTextAndEmbeddedImages(string[] rtfParts, EmbeddedRtfImageFileInfo[] imageFiles, bool deleteFilesOnDispose)
		{
			valid = (rtfParts != null && rtfParts.Length > 0);
			this.rtfParts = rtfParts;
			this.imageFiles = imageFiles;
			this.deleteFilesOnDispose = deleteFilesOnDispose;
		}

		public readonly string[] rtfParts;
		public readonly EmbeddedRtfImageFileInfo[] imageFiles;
		public readonly bool valid;
		readonly bool deleteFilesOnDispose;

		public string GetRtfText()
		{
			if (valid)
			{
				if (imageFiles != null)
				{
					foreach (var imageFile in imageFiles)
					{
						var rtfPartIndexForImage = GetRtfPartIndexForImage(imageFile);
						if (rtfPartIndexForImage >= 0)
						{
							rtfParts[rtfPartIndexForImage] = imageFile.fileCaption;
						}
					}
				}

				return rtfParts.Length == 1 ? rtfParts[0] : string.Concat(rtfParts);
			}
			else
			{
				return null;
			}
		}

		int GetRtfPartIndexForImage(EmbeddedRtfImageFileInfo imageFile)
		{
			Argument.NotNull(imageFile, nameof(imageFile)); // Suggested By ReviewBot 
			Argument.NotNull(this.rtfParts, nameof(this.rtfParts));
			for (var i = 0; i < rtfParts.Length; i++)
			{
				if (rtfParts[i] == imageFile.filePath)
				{
					return i;
				}
			}

			return -1;
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (deleteFilesOnDispose && imageFiles != null)
				{
					foreach (var imageFile in imageFiles)
					{
						File.Delete(imageFile.filePath);
					}
				}
			}
		}
	}

	#endregion

	ZFormattedTextDataObject(DataObject inner, RtfTextAndEmbeddedImages rtfTextAndEmbeddedImages)
		: base(inner)
	{
		Argument.NotNull(rtfTextAndEmbeddedImages, nameof(rtfTextAndEmbeddedImages));
		this.rtfTextAndEmbeddedImages = rtfTextAndEmbeddedImages;
	}

	internal readonly RtfTextAndEmbeddedImages rtfTextAndEmbeddedImages;

	#region IEmbeddedRtfImageSource Members

	EmbeddedRtfImageFileInfo[] IEmbeddedRtfImageSource.ImageFiles => rtfTextAndEmbeddedImages.imageFiles;

	bool IEmbeddedRtfImageSource.Valid => rtfTextAndEmbeddedImages.valid;

	void IEmbeddedRtfImageSource.FinaliseRtf()
	{
		if (!Inner.GetDataPresent(DataFormats.Rtf))
		{
			Inner.SetData(DataFormats.Rtf, rtfTextAndEmbeddedImages.GetRtfText());
		}
	}

	#endregion

	#region TryNewInstance

	public static ZDataObject TryNewInstance(ZDataObject dataObject)
	{
		Argument.NotNull(dataObject, nameof(dataObject)); // Suggested By ReviewBot 
		ZFormattedTextDataObject result = null;

		var dataFormats = dataObject.GetFormats();
		if (((IList)dataFormats).Contains(DataFormats.Rtf))
		{
			// This will return null when the clipboard data (ie. from MS Office App) is large
			var rtf = (string)dataObject.GetData(DataFormats.Rtf);
			var infoNeedsToBeNotified = string.Empty;
			var rtfTextAndImages = rtf != null ? ReconstructRtfText(rtf, out infoNeedsToBeNotified) : new RtfTextAndEmbeddedImages(null, null, true);
			var innerData = ConstructInnerData(dataObject, dataFormats);

			result = new ZFormattedTextDataObject(innerData, rtfTextAndImages);

			if (!string.IsNullOrEmpty(infoNeedsToBeNotified))
			{
				result.InfoNeedsToBeNotified = infoNeedsToBeNotified;
			}
		}

		return result;
	}

	static DataObject ConstructInnerData(ZDataObject originalDataObject, string[] dataFormats)
	{
		Argument.NotNull(originalDataObject, nameof(originalDataObject));
		Argument.NotNull(dataFormats, nameof(dataFormats)); // Suggested By ReviewBot 
		var result = new DataObject();

		foreach (var dataFormat in dataFormats)
		{
			var data = GetDataSafely(originalDataObject, dataFormat);
			if (data != null)
			{
				result.SetData(dataFormat, data);
			}
		}

		return result;
	}

	static object GetDataSafely(ZDataObject dataObject, string format)
	{
		Argument.NotNull(dataObject, nameof(dataObject));
		object result;

		if (format == DataFormats.Rtf)
		{
			result = null;
		}
		else
		{
			var formatsToIgnore = new string[] { DataFormats.EnhancedMetafile, DataFormats.MetafilePict };
			result = (!((IList)formatsToIgnore).Contains(format)) ? dataObject.GetData(format) : null;
		}

		return result;
	}

	#endregion

	#region IDataObject overrides

	public override bool GetDataPresent(string format)
	{
		return format == DataFormats.Rtf || base.GetDataPresent(format);
	}

	public override bool GetDataPresent(string format, bool autoConvert)
	{
		return format == DataFormats.Rtf || base.GetDataPresent(format, autoConvert);
	}

	public override object GetData(string format)
	{
		if (format == DataFormats.Rtf)
		{
			((IEmbeddedRtfImageSource)this).FinaliseRtf();
		}

		return base.GetData(format);
	}

	public override object GetData(string format, bool autoConvert)
	{
		if (format == DataFormats.Rtf)
		{
			return GetData(format) ?? base.GetData(format, autoConvert);
		}
		else
		{
			return base.GetData(format, autoConvert);
		}
	}

	public override string[] GetFormats()
	{
		var result = new List<string>(base.GetFormats());
		if (!result.Contains(DataFormats.Rtf))
		{
			result.Add(DataFormats.Rtf);
		}
		return result.ToArray();
	}

	#endregion

	#region ReconstructRtfText

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	internal const string AnyPictFormat = @"{\pict";
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	internal const string NonShpPictFormat = @"{\nonshppict";
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	internal const string ShpPictFormat = @"{\*\shppict";
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	internal const string ObjectFormat = @"{\object";

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	internal static RtfTextAndEmbeddedImages ReconstructRtfText(string rtf, out string infoNeedToBeNotified)
	{
		var imageFiles = new List<EmbeddedRtfImageFileInfo>();
		var rtfParts = new List<string>();
		var readLength = 0;
		infoNeedToBeNotified = string.Empty;

		if (rtf.IndexOf(AnyPictFormat, StringComparison.Ordinal) >= 0)
		{
			var pictureCloseBracesCount = InitializePictureFormatStrings(rtf, out var pictureFormats);
			var invalidPictureCount = 0;

			while (CheckRequireConditionsForGetNextPicture(rtf, readLength, pictureFormats) && GetNextPicture(rtf, readLength, out var nextPictureStart, out var nextPictureEnd, out var foundPictureFormatNo, pictureFormats))
			{
				var realPictureEnd = nextPictureEnd;

				if (foundPictureFormatNo == pictureFormats.Length - 1 && nextPictureEnd >= 0)
				{
					var realPictureStart = rtf.LastIndexOf(pictureFormats[0], nextPictureEnd, StringComparison.Ordinal);
					if (realPictureStart > nextPictureStart && realPictureStart + 1 < rtf.Length)
					{
						realPictureEnd = FindCloseBrace(rtf, realPictureStart + 1);
						foundPictureFormatNo = 0;
					}
				}

				rtfParts.Add(rtf.Substring(readLength, nextPictureStart - readLength));
				readLength = nextPictureEnd + 1;

				if (foundPictureFormatNo == 0)
				{
					var tempFileName = Temp.GetTempFileNameWithExtension("bmp");

					if (SavePictureToFile(tempFileName, GetPictureString(rtf, realPictureEnd, pictureCloseBracesCount)))
					{
						var fileDesc = tempFileName;
						rtfParts.Add($@"\par\f0 File {Path.GetFileName(tempFileName)} (");

						imageFiles.Add(new EmbeddedRtfImageFileInfo(tempFileName, fileDesc));
						rtfParts.Add(fileDesc);
						rtfParts.Add(@") added to eDocs tab.\par ");
					}
					else
					{
						invalidPictureCount++;
					}
				}
			}

			if (invalidPictureCount > 0)
			{
				infoNeedToBeNotified = $"{invalidPictureCount} invalid picture{(invalidPictureCount > 1 ? "s have" : " has")} been ignored.";
			}
		}

		rtfParts.Add(rtf.Substring(readLength));

		return new RtfTextAndEmbeddedImages(rtfParts.ToArray(), imageFiles.ToArray(), true);
	}

	static bool CheckRequireConditionsForGetNextPicture(string rtf, int startIndex, string[] pictureFormats)
	{
		if (rtf == null)
		{
			return false;
		}

		if (pictureFormats == null)
		{
			return false;
		}

		if (rtf.Length <= startIndex)
		{
			return false;
		}

		if (startIndex < 0)
		{
			return false;
		}

		if (pictureFormats.Length > rtf.Length)
		{
			return false;
		}

		return true;
	}

	internal static int InitializePictureFormatStrings(string rtf, out string[] pictureFormats)
	{
		Argument.NotNull(rtf, nameof(rtf));

		int pictureCloseBracesCount;

		if (rtf.IndexOf(NonShpPictFormat) >= 0)
		{
			pictureFormats = new string[] { NonShpPictFormat, ShpPictFormat, ObjectFormat };
			pictureCloseBracesCount = 2;
		}
		else if (rtf.IndexOf(ShpPictFormat) >= 0)
		{
			pictureFormats = new string[] { ShpPictFormat, ObjectFormat };
			pictureCloseBracesCount = 2;
		}
		else
		{
			pictureFormats = new string[] { AnyPictFormat, ObjectFormat };
			pictureCloseBracesCount = 1;
		}

		return pictureCloseBracesCount;
	}

	internal static bool GetNextPicture(string rtf, int startIndex, out int pictureStart, out int pictureEnd, out int foundPictureFormatNo, string[] pictureFormats)
	{
		Argument.NotNull(rtf, nameof(rtf));
		Argument.NotNull(pictureFormats, nameof(pictureFormats));

		pictureStart = GetNextPicturePosition(rtf, startIndex, out foundPictureFormatNo, pictureFormats);
		pictureEnd = FindCloseBrace(rtf, pictureStart + 1);

		return pictureStart >= 0 && pictureStart < pictureEnd;
	}

	internal static int GetNextPicturePosition(string rtf, int startIndex, out int foundPictureFormatNo, string[] pictureFormats)
	{
		Argument.NotNull(rtf, nameof(rtf));
		Argument.NotNull(pictureFormats, nameof(pictureFormats));

		var result = -1;
		foundPictureFormatNo = -1;

		for (var i = 0; i < pictureFormats.Length; i++)
		{
			var position = rtf.IndexOf(pictureFormats[i], startIndex);
			if (position >= 0 && (result < 0 || position < result))
			{
				result = position;
				foundPictureFormatNo = i;
			}
		}

		return result;
	}

	internal static int FindCloseBrace(string rtf, int startIndex)
	{
		Argument.NotNull(rtf, nameof(rtf));
		var level = 0;

		for (var i = startIndex; i < rtf.Length; i++)
		{
			if (rtf[i] == '{')
			{
				level++;
			}
			else if (rtf[i] == '}')
			{
				level--;
				if (level < 0)
				{
					return i;
				}
			}
		}

		return -1;
	}

	internal static string GetPictureString(string rtf, int pictureEnd, int pictureCloseBracesCount)
	{
		Argument.NotNull(rtf, nameof(rtf));

		pictureEnd -= pictureCloseBracesCount;

		var lastControlWordPosition = Math.Max(rtf.LastIndexOf('\\', pictureEnd), rtf.LastIndexOf('}', pictureEnd));
		if (lastControlWordPosition >= 0 && rtf[lastControlWordPosition] == '\\')
		{
			lastControlWordPosition = rtf.IndexOfAny(new char[] { '\n', '\r', ' ' }, lastControlWordPosition);
		}
		return pictureEnd < lastControlWordPosition ? string.Empty : rtf.Substring(lastControlWordPosition + 1, pictureEnd - lastControlWordPosition).
			Trim().
			Replace("\n", string.Empty).
			Replace("\r", string.Empty);
	}

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	internal static bool SavePictureToFile(string fileName, string picture)
	{
		Argument.NotNull(picture, nameof(picture)); // Suggested By ReviewBot 
		Argument.NotNull(fileName, nameof(fileName)); // Suggested By ReviewBot 

		using (var pictureBuffer = new MemoryStream(picture.Length / 2))
		{
			for (var i = 0; i + 1 < picture.Length; i += 2)
			{
				pictureBuffer.WriteByte(GetByteFromTwoHexChars(picture[i], picture[i + 1]));
			}

			try
			{
				pictureBuffer.Position = 0;
				using (var bitmap = new Bitmap(pictureBuffer))
				{
					bitmap.Save(fileName, ImageFormat.Bmp);
					return true;
				}
			}
			catch (ArgumentException ex) when (ex.Message.Contains("Parameter is not valid"))
			{
				File.Delete(fileName);
				return false;
			}
			catch (OutOfMemoryException)
			{
				File.Delete(fileName);
				return false;
			}
		}
	}

	internal static byte GetByteFromTwoHexChars(char h1, char h2) => (byte)(GetIntFromHexChar(h1) << 4 | GetIntFromHexChar(h2));

	internal static int GetIntFromHexChar(char hex)
	{
		var value = "0123456789abcdefABCDEF".IndexOf(hex);
		if (value > 15)
		{
			value -= 6;
		}

		return value;
	}

	#endregion

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			((IDisposable)rtfTextAndEmbeddedImages).Dispose();
		}

		base.Dispose(disposing);
	}
}
