using System.IO;
using System.Xml;
using CargoWise.eHub.Common.Extensions;

namespace CargoWise.eHub.Products.NZCustoms.Common
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

		internal static byte[] GetElementDecodeAndDecompressAndReturnAsByteArray(this XmlReader reader, string name, string namespaceName = "")
		{
			MemoryStream result = null;

			reader.ReadToElement(name, namespaceName);

			if (!reader.EOF)
			{
				using (result = new MemoryStream())
				{
					reader.Read();
                    if (reader.NodeType == XmlNodeType.EndElement && name == "Content")
                    {
                        throw new NZCustomsInvalidOperationException("Attachment document file cannot be empty. Please correct and resend the message.");
                    }
					reader.WriteToStream(result);
					result.SeekBegin();
					using (var decodedStream = result.DecodeAndDecompress())
					{
						if (decodedStream != null)
						{
							using (var memoryStream = new MemoryStream())
							{
								decodedStream.CopyTo(memoryStream);
								memoryStream.SeekBegin();
								return memoryStream.ToArray();
							}
						}
					}
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