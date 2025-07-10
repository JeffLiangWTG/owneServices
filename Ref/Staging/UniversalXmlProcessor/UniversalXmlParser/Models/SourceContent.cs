using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Models
{
	public class SourceContent
	{
		readonly XDocument xDocument;
		readonly XElement rootElement;
		readonly IUniversalXmlTransformer universalXmlTransformer;

		public SourceContent(string rootElment, IUniversalXmlTransformer universalXmlTransformer = null)
		{
			Argument.NotNullOrEmpty(rootElment, nameof(rootElment));

			xDocument = new XDocument();

			rootElement = new XElement(rootElment);
			xDocument.Add(rootElement);
			this.universalXmlTransformer = universalXmlTransformer;
		}

		public void AddContent(string key, string value)
		{
			Argument.NotNullOrEmpty(key, nameof(key));

			rootElement.Add(new XElement(key, value));
		}

		public void AddXmlContent(string value)
		{
			Argument.NotNullOrEmpty(value, nameof(value));

			rootElement.Add(XElement.Parse(value));
		}

		public string ToXml()
		{
			var xml = xDocument.ToString(SaveOptions.DisableFormatting);
			return universalXmlTransformer == null ? xml : universalXmlTransformer.Transform(xml);
		}
	}
}
