using System.IO;
using System.Xml;
using CargoWise.eHub.Common.Extensions;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	public static class StreamExtensions
	{
		internal static string GetElementAsString(this XmlReader reader, string name, string namespaceName = "")
		{
			reader.ReadToElement(name, namespaceName);

			if (!reader.EOF)
			{
				return reader.ReadElementContentAsString();
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		internal static Stream GetElementAsStream(this XmlReader reader, string name, string namespaceName = "")
		{
			MemoryStream result = null;

			reader.ReadToElement(name, namespaceName);

			if (!reader.EOF)
			{
				result = new MemoryStream();
				reader.Read();
				reader.WriteToStream(result);
			}

			return result;
		}

		internal static byte[] GetElementAsByteArray(this XmlReader reader, string name, string namespaceName = "")
		{
			MemoryStream result = null;

			reader.ReadToElement(name, namespaceName);

			if (!reader.EOF)
			{
				using (result = new MemoryStream())
				{
					reader.Read();
					reader.WriteToStream(result);
					return result.ToArray();
				}
			}

			return null;
		}

		static void ReadToElement(this XmlReader reader, string name, string namespaceName = "")
		{
			if (reader.LocalName != name || reader.NamespaceURI != namespaceName)
			{
				reader.ReadToFollowing(name, namespaceName);
			}
		}
	}
}