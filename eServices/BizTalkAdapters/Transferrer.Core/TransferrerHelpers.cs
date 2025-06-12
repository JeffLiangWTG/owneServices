using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	public static class TransferrerHelpers
	{
		public static string AltPathCombine(params string[] paths)
		{
			return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		public static string ResolveMarkup(string name, string markup)
		{
			if (String.IsNullOrWhiteSpace(markup))
				return name;
			else
				return markup.Replace("{f}", name)
					.Replace("{n}", Path.GetFileNameWithoutExtension(name))
					.Replace("{x}", Path.GetExtension(name));
		}

		public static string ConvertMarkupToFileMask(string markup)
		{
			return Regex.Replace(markup, @"\{[fnx]}", "*");
		}

		public static string ConvertFileMaskToRegexPattern(string filemask)
		{
			if (String.IsNullOrWhiteSpace(filemask)) filemask = "*";
			return "^" + filemask.Replace(".", "\\.").Replace("*", ".*") + "$";
		}

		public static T GetValue<T>(XmlDocument configXml, string xpath)
		{
			return GetValue(configXml, xpath, default(T), true);
		}

		public static T GetValueOrDefault<T>(XmlDocument configXml, string xpath, T defaultValue = default(T))
		{
			return GetValue(configXml, xpath, defaultValue, false);
		}

		static T GetValue<T>(XmlDocument configXml, string xpath, T defaultValue, bool mustExist)
		{
			var node = configXml.SelectSingleNode(xpath);
			if (mustExist && node == null) throw new TransferrerException("Property " + xpath + " was not found in configuration XML.");
			return node != null ? (T)Convert.ChangeType(node.InnerText, typeof(T)) : defaultValue;
		}
	}
}
