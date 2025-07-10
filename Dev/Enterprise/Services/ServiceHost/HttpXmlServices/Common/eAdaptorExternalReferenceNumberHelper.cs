using System.IO;
using System.Xml;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Services.ServiceHost
{
	public static class eAdaptorExternalReferenceNumberHelper
	{
		public static string GetExternalReferenceNumber(Stream stream)
		{
			using (var reader = XmlReader.Create(stream))
			{
				try
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element && reader.Name == "MessageNumberCollection")
						{
							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "MessageNumberCollection")
								{
									break;
								}
								if (reader.NodeType == XmlNodeType.Element && reader.Name == "MessageNumber" && reader.GetAttribute("Type") == nameof(MessageNumberType.External))
								{
									return reader.ReadElementContentAsString();
								}
							}
						}
					}
				}
				catch (XmlException)
				{
					return null;
				}
				finally
				{
					stream.Seek(0, SeekOrigin.Begin);
				}
			}
			return null;
		}
	}
}
