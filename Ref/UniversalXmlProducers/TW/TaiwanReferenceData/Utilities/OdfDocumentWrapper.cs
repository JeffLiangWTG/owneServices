using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class OdfDocumentWrapper
	{
		public OdfDocumentWrapper(XmlDocument document, XmlNamespaceManager nsManager)
		{
			Document = Argument.NotNull(document, nameof(document));
			NSManager = Argument.NotNull(nsManager, nameof(nsManager));
		}

		public XmlDocument Document { get; }

		public XmlNamespaceManager NSManager { get; }

		public IEnumerable<string> TextLineThroughStyleNames
		{
			get
			{
				if (textLineThroughStyleNames == null)
				{
					textLineThroughStyleNames = GetTextLineThroughStyleNames();
				}
				return textLineThroughStyleNames;
			}
		}
		IEnumerable<string> textLineThroughStyleNames;

		IEnumerable<string> GetTextLineThroughStyleNames()
		{
			var xpath = "/office:document-content/office:automatic-styles/style:style[style:text-properties[@style:text-line-through-style='solid']]";
			return Document.DocumentElement?.SelectNodes(xpath, NSManager)?.Cast<XmlNode>().Select(x => x.Attributes?["style:name"]?.Value ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? Enumerable.Empty<string>();
		}
	}
}
