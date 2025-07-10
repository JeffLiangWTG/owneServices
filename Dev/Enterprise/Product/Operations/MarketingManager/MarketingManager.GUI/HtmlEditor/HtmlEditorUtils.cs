using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Enterprise.MarketingManager.GUI
{
	class HtmlEditorUtils
	{
		public static string UpdateStyleValueInStyleString(string styleStr, string styleKey, string styleValue)
		{
			var dictionary = ParseStyleStrToDictionary(styleStr);
			if (dictionary.ContainsKey(styleKey))
			{
				if (string.IsNullOrEmpty(styleValue))
				{
					dictionary.Remove(styleKey);
				}
				else
				{
					dictionary[styleKey] = styleValue;
				}
			}
			else
			{
				dictionary.Add(styleKey, styleValue);
			}

			return string.Join(string.Empty, dictionary.Select(item => $"{item.Key}:{item.Value};"));
		}

		public static Dictionary<string, string> ParseStyleStrToDictionary(string styleStr)
		{
			var dictionary = new Dictionary<string, string>();
			if (string.IsNullOrEmpty(styleStr))
			{
				return dictionary;
			}

			var styles = styleStr.Trim().Split(';');
			foreach (var style in styles)
			{
				if (!string.IsNullOrEmpty(style))
				{
					var keyValue = style.Split(':');
					var key = keyValue[0].Trim().ToLower();
					var value = keyValue.Length > 1 ? keyValue[1].Trim() : string.Empty;

					dictionary.Add(key, value);
				}
			}

			return dictionary;
		}

		public static Size? GetImageDimension(string absolutePath)
		{
			if (!File.Exists(absolutePath))
			{
				return null;
			}

			try
			{
				using (var image = Image.FromFile(absolutePath))
				{
					var height = image.Height;
					var width = image.Width;
					return new Size(width, height);
				}
			}
			catch
			{
				return null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "File extension")]
		public static string GetBase64DataUrlForLocalImage(string imagePath)
		{
			var extension = Path.GetExtension(imagePath)?.ToLower()?.TrimStart('.');
			if (extension == null)
			{
				return null;
			}

			if (extension == "jpg")
			{
				extension = "jpeg";
			}

			var base64Encoding = GetBase64Encoding(imagePath);
			return $"data:image/{extension};base64,{base64Encoding}";
		}

		static string GetBase64Encoding(string imagePath)
		{
			byte[] buffer;
			using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
			{
				buffer = new byte[fileStream.Length];
				fileStream.Read(buffer, 0, (int)fileStream.Length);
			}

			return Convert.ToBase64String(buffer);
		}
	}
}
