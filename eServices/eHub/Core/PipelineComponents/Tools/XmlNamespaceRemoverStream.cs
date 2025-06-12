using System.IO;
using System.Text;
using System.Xml;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public class XmlNamespaceRemoverStream : XmlTranslatorStream
	{
		protected override void TranslateStartElement(
				string prefix, string localName, string nsURI)
		{
			base.TranslateStartElement(null, localName, null);
		}

		protected override void TranslateAttribute()
		{
			if (this.m_reader.Prefix != "xmlns")
				base.TranslateAttribute();
		}

		public XmlNamespaceRemoverStream(Stream input)
			: base(new XmlTextReader(input), Encoding.Default)
		{ }
	}
}
