using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.Customs.US.MessageDefinitions.ExportManifest;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public static class UEMMessageHelper
	{
		public static ManifestFiling DeSerializeManifestFiling(string messageText)
		{
			if (!string.IsNullOrEmpty(messageText))
			{
				using (var stringReader = new StringReader(messageText))
				{
					if (xmlSerializer == null)
					{
						xmlSerializer = new XmlSerializer(typeof(ManifestFiling));
					}
					return (ManifestFiling)xmlSerializer.Deserialize(stringReader);
				}
			}

			return null;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "<Pending>")]
		static XmlSerializer xmlSerializer;

		public static string SeparateElementName(string name)
		{
			return Regex.Replace(name, "([a-z][A-Z])|([A-Z][A-Z][a-z])", (x) => x.Value.Substring(0, 1) + " " + x.Value.Substring(1));
		}
	}
}
