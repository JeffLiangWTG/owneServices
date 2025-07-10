using System;
using System.Collections.Generic;
using System.IO;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap
{
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Template for a multipart/form-data item.
		/// </summary>
		public const string FormDataTemplate = "--uuid:{0}\r\nContent-Id: <rootpart*{0}@example.jaxws.sun.com>\r\nContent-Type: application/xop+xml; charset=utf-8;type=\"text/xml\"\r\nContent-Transfer-Encoding: binary\r\n\r\n{1}\r\n";

		public static void WriteMultipartFormData(this Dictionary<string, string> dictionary, Stream stream, string mimeBoundary)
		{
			if (dictionary == null || dictionary.Count == 0)
			{
				return;
			}
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (mimeBoundary == null)
			{
				throw new ArgumentNullException(nameof(mimeBoundary));
			}
			if (mimeBoundary.Length == 0)
			{
				throw new ArgumentException("MIME boundary may not be empty.", nameof(mimeBoundary));
			}
			foreach (string key in dictionary.Keys)
			{
				string item = String.Format(System.Globalization.CultureInfo.InvariantCulture, FormDataTemplate, mimeBoundary, dictionary[key]);
				byte[] itemBytes = System.Text.Encoding.ASCII.GetBytes(item);
				stream.Write(itemBytes, 0, itemBytes.Length);
			}
		}
	}
}
