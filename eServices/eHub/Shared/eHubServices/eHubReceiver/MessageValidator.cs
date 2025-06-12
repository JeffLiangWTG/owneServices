using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using CargoWise.eHub.Clients.TRX.Schemas._3B18.Interchange.CodeList;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver
{
	public class MessageValidator
	{
		public static string Validate(Stream messageStream, string dateTimeNowString)
		{
			var validator = new MessageValidator(dateTimeNowString);
			return validator.ValidateMessage(messageStream);
		}

		MessageValidator(string dateTimeNowString)
		{
			DateTimeNowString = dateTimeNowString;
		}

		string ValidateMessage(Stream messageStream)
		{
			errorMessage = "";

			var xmlSettings = new XmlReaderSettings();
			xmlSettings.Schemas.Add(MessageValidator.GetSchemas());
			xmlSettings.ValidationType = ValidationType.Schema;
			xmlSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
			xmlSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
			xmlSettings.ValidationEventHandler += new ValidationEventHandler(xmlSettingsValidationEventHandler);
			var reader = new StreamReader(messageStream);

			using (var xmlReader = XmlReader.Create(reader, xmlSettings))
			{ 
				try
				{
					while (xmlReader.Read()) { }
				}
				catch
				{
					return string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>ERROR</status><timestamp>{0}</timestamp><errorcode>503</errorcode><errordescription>Not valid XML</errordescription></responseforsoap>", DateTimeNowString);
				}
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				return string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>ERROR</status><timestamp>{0}</timestamp><errorcode>502</errorcode><errordescription>{1}</errordescription></responseforsoap>", DateTimeNowString, errorMessage);
			}

			return string.Empty;
		}

		void xmlSettingsValidationEventHandler(object sender, ValidationEventArgs args)
		{
			errorMessage += args.Message + "\r\n";
		}

		string errorMessage;

		static IEnumerable<Type> FindTypes(Assembly assembly, Func<Type, bool> accept)
		{
			foreach (var type in assembly.GetTypes())
			{
				if (accept(type))
				{
					yield return type;
				}
			}
		}

		static XmlSchemaSet GetSchemas()
		{
			if (Schemas == null)
			{
				var schemas = new XmlSchemaSet();
				var assembly = Assembly.GetAssembly(typeof(RN_DeliveryType_01_00));
				// Needs to be revisited to implement lazy loading of schemas
				var schemaTypes = FindTypes(assembly, t => t.FullName.Contains("_3B18") && (t.DeclaringType == null));

				foreach (var definition in schemaTypes)
				{
					var obj = (SchemaBase)Activator.CreateInstance(definition);

					using (var reader = new StringReader(obj.XmlContent))
					{
						schemas.Add(XmlSchema.Read(reader, null));
					}
				}

				Schemas = schemas;
			}

			return Schemas;
		}

		private static XmlSchemaSet Schemas;
		string DateTimeNowString { get; set; }
	}
}