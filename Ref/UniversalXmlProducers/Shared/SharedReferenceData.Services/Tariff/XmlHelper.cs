using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff
{
	public class XmlHelper
	{
		public void ValidateFile(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
				{
					throw new InvalidOperationException("File does not exist");
				}

				using (var xmlContent = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					var valid = true;
					var message = new StringBuilder();

					var settings = new XmlReaderSettings()
					{
						Schemas = SchemaSet,
						ValidationType = ValidationType.Schema,
						ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ReportValidationWarnings
					};

					settings.ValidationEventHandler += new ValidationEventHandler((o, e) =>
					{
						if (e != null)
						{
							valid = false;
							message.AppendLine(CultureInfo.InvariantCulture, $"Line: {e.Exception.LineNumber} Position: {e.Exception.LinePosition} Message: {e.Message}");
						}
					});

					using (var xmlReader = XmlReader.Create(xmlContent, settings))
					{
						while (valid && xmlReader.Read())
						{ }
					}

					if (!valid)
					{
						throw new InvalidOperationException($"Invalid schema:\r\n{message.ToString()}");
					}
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to validate file {filePath}", ex);
			}
		}

		XmlSchemaSet SchemaSet
		{
			get
			{
				if (schemaSet == null)
				{
					schemaSet = new XmlSchemaSet();
					schemaSet.Add(XmlSchema.Read(XmlSchemaReader, (o, e) => throw new InvalidOperationException("Failed to load XML Schema")));
				}
				return schemaSet;
			}
		}
		XmlSchemaSet schemaSet;

		static XmlTextReader XmlSchemaReader
		{
			get
			{
				var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Schema.DispatchDataExport-XML-Data.xsd");
				return new XmlTextReader(resourceStream);
			}
		}

		public T ReadNext<T>(Stream xmlStream, string xmlName)
		{
			T result = default;

#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				using (var reader = XmlReader.Create(xmlStream))
				{
					if (reader.ReadToFollowing(xmlName))
					{
						if (!serializers.ContainsKey(typeof(T)))
						{
							serializers.Add(typeof(T), new XmlSerializer(typeof(T)));
						}

						result = (T)serializers[typeof(T)].Deserialize(reader.ReadSubtree());
					}
				}
			}
			catch { }
#pragma warning restore CA1031 // Do not catch general exception types

			return result;
		}

		Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();
	}
}
