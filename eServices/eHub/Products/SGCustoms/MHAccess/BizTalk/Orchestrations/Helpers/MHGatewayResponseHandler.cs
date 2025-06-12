using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	public static class MHGatewayResponseHandler
	{
		public static bool Handle(string submitResponseXml, string clientID, string accountId, out bool retry, out string error)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(submitResponseXml);
			var responseXmlElement = xmlDocument.FirstChild;
			var responseXml = responseXmlElement.InnerXml;

			ValidateResponse(responseXml);

			var serializer = new XmlSerializer(typeof(MHAccessResponse), "http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09");
			var response = (MHAccessResponse)serializer.Deserialize(new XmlTextReader(new StringReader(responseXml)));

			return AnalyseResponse(response, clientID, accountId, out retry, out error);
		}

		static bool AnalyseResponse(MHAccessResponse response, string clientID, string accountId, out bool retry, out string error)
		{
			retry = false;
			error = string.Empty;

			if (response.Status == "success") return true;

			error = response.ToString();
			retry = ShouldRetry(response.ErrorCode);

			return false;
		}

		static bool ShouldRetry(string errorCode)
		{
			switch (errorCode)
			{
				case "10": //Server connection error.
					return true;
				default:
					return false;
			}
		}

		static void ValidateResponse(string xml)
		{
			var xDocument = XDocument.Parse(xml);
			var errors = new List<string>();

			xDocument.Validate(SchemaSet, (sender, args) =>
			{
				if (args.Severity == XmlSeverityType.Error)
				{
					errors.Add(args.Message);
				}
			});

			if (errors.Any())
			{
				var exceptionMessage = string.Join(";", errors);
				throw new XmlSchemaValidationException(exceptionMessage);
			}
		}

		static XmlSchemaSet SchemaSet
		{
			get { return schemaSet ?? (schemaSet = CreateSchemaSet()); }
		}

		[ThreadStatic]
		static XmlSchemaSet schemaSet;

		static XmlSchemaSet CreateSchemaSet()
		{
			var result = new XmlSchemaSet();
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(MHAccessResponseXsdPath);
			using (var reader = new StreamReader(stream))
			{
				result.Add(targetNamespace, XmlReader.Create(reader));
			}
			return result;
		}

		static string MHAccessResponseXsdPath = "CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers.MHAccessResponse.xsd";
		static string targetNamespace = "http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09";
	}
}