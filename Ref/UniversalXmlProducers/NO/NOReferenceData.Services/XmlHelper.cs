using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.NOReferenceData.Services
{
	public static class XmlHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5371:Use XmlReader for 'XmlSchema.Read()'", Justification = "Internal Tool")]
		public static string ValidateXml(string xmlData, string schemaLocation)
		{
			try
			{
				var byteArray = Encoding.UTF8.GetBytes(xmlData);
				var stream = new MemoryStream(byteArray);

				using (var xsdSchemaStream = EmbeddedResourceHelper.ReadManifestResourceContentAsStream(schemaLocation, Assembly.GetExecutingAssembly()))
				{
					var schemas = new XmlSchemaSet();
					schemas.Add(XmlSchema.Read(xsdSchemaStream, null));

					var settings = new XmlReaderSettings
					{
						Schemas = schemas,
						ValidationType = ValidationType.Schema,
						ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings,
					};

					using (var reader = XmlReader.Create(stream, settings))
					{
						while (reader.Read())
						{ }
					}
				}
				return string.Empty;
			}
			catch (XmlSchemaException ex)
			{
				return ex.Message;
			}
		}

		public static T DeserializeFromString<T>(string xmlData) where T : class
		{
			using (var stream = new StringReader(xmlData))
			{
				return DeserializeFromStream<T>(stream);
			}
		}

		private static T DeserializeFromStream<T>(TextReader xmlData) where T : class
		{
			var serializer = new XmlSerializer(typeof(T));
			return (T)serializer.Deserialize(xmlData);
		}

		public static T ReadDeserializedManifestResourceContent<T>(string resourceDetails) where T : class
		{
			var assembly = Assembly.GetCallingAssembly();
			using (var stream = EmbeddedResourceHelper.ReadManifestResourceContentAsStream(resourceDetails, assembly))
			{
				return DeserializeFromStream<T>(stream);
			}
		}
	}
}
