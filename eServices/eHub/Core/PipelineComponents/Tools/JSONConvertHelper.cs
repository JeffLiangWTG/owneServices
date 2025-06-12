using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public static class JSONConvertHelper
	{
		public static XmlDocument JSON2XML(string json)
		{
			var jsonSettings = new JsonSerializerSettings 
			{ 
				Converters = { new XmlNodeConverter { WriteArrayAttribute = true } }, 
				DateParseHandling = DateParseHandling.DateTimeOffset 
			};

			return JsonConvert.DeserializeObject<XmlDocument>(json, jsonSettings);
		}

		public static string XML2JSON(Stream stream)
		{
			var xmlDoc = XElement.Load(stream);
			return JsonConvert.SerializeXNode(RemoveAllNamespacesNotBelongToNewtonsoft(xmlDoc), Newtonsoft.Json.Formatting.Indented);
		}

		public static string Beautify(this XmlDocument doc)
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				OmitXmlDeclaration = true
			};
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				doc.Save(writer);
			}
			return sb.ToString();
		}

		static XElement RemoveAllNamespacesNotBelongToNewtonsoft(XElement xmlDocument)
		{
			var stripped = new XElement(xmlDocument.Name.LocalName);
			foreach (var attribute in xmlDocument.Attributes())
			{
				if (IsNewtonsoftNamespace(attribute))
					stripped.Add(attribute);
				else if (!IsNamespace(attribute.Name))
					stripped.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));
			}
			if (!xmlDocument.HasElements)
			{
				stripped.Value = xmlDocument.Value;
				return stripped;
			}
			stripped.Add(xmlDocument.Elements().Select(el => RemoveAllNamespacesNotBelongToNewtonsoft(el)));
			return stripped;
		}

		static bool IsNamespace(XName name)
		{
			return name.LocalName == "xmlns" || name.NamespaceName == "http://www.w3.org/2000/xmlns/";
		}

		static bool IsNewtonsoftNamespace(XAttribute attribute)
		{
            return attribute.Value == "http://james.newtonking.com/projects/json"
                || attribute.Name.NamespaceName == "http://james.newtonking.com/projects/json";
		}
	}
}
