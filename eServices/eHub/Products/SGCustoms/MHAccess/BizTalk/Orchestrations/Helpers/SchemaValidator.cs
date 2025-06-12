using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Schemas;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	public static class SchemaValidator
	{
		public static bool Validate(XLANGMessage message, string schemaName)
		{
			var xDocument = XDocument.Load((Stream) message[0].RetrieveAs(typeof(Stream)));
			var schemaSet = GetSchemaSet(schemaName);

			var errors = new List<string>();

			xDocument.Validate(schemaSet, (sender, args) =>
			{
				if (args.Severity == XmlSeverityType.Error)
				{
					if (Regex.IsMatch(args.Message, @"'\w+' element is invalid .+value '.*' is invalid"))
					{
						var element = sender as XElement;
						errors.Add($"'{element.Name}' - '{element.Value}'");
					}
					else
					{
						errors.Add(args.Message);
					}
				}
			});

			if (errors.Any())
			{
				var exceptionMessage = string.Join("\r\n", errors);
				throw new XmlSchemaValidationException($"Message validation failed\r\n{exceptionMessage}");
			}

			return true;
		}

		public static XmlSchemaSet GetSchemaSet(string schemaName)
		{
			switch (schemaName)
			{
				case "EFACT_31_AIRPCM":
					return new EFACT_31_AIRPCM().SchemaSet;
				case "EFACT_31_AIRPCU":
					return new EFACT_31_AIRPCU().SchemaSet;
				case "EFACT_31_AIRAED":
					return new EFACT_31_AIRAED().SchemaSet;
				case "EFACT_31_AIRAEU":
					return new EFACT_31_AIRAEU().SchemaSet;
				default:
					throw new InvalidOperationException($"Could not find schema {schemaName} for validation.");
			}
		}
	}
}