using System.IO;
using System.Linq;
using System.Xml;

namespace Enterprise.Customs.PL.Business.Testing;

static class XmlReaderComparer
{
	public static bool CompareElementsOrderSensitive(string xml1, string xml2)
	{
		using var reader1 = XmlReader.Create(new StringReader(xml1), new XmlReaderSettings { CloseInput = true });
		using var reader2 = XmlReader.Create(new StringReader(xml2), new XmlReaderSettings { CloseInput = true });
		return CompareElementsOrderSensitive(reader1, reader2);
	}

	public static bool CompareElementsOrderSensitive(XmlReader reader1, XmlReader reader2)
	{
		while (MoveToContent(reader1))
		{
			if (!MoveToContent(reader2))
			{
				return false;
			}

			if (reader1.NodeType != reader2.NodeType || reader1.Name != reader2.Name || reader1.Value != reader2.Value)
			{
				return false;
			}

			if (reader1.HasAttributes || reader2.HasAttributes)
			{
				if (reader1.AttributeCount != reader2.AttributeCount)
				{
					return false;
				}

				reader1.MoveToFirstAttribute();
				reader2.MoveToFirstAttribute();
				var attributes1 = Enumerable.Range(0, reader1.AttributeCount).Select(reader1.GetAttribute);
				var attributes2 = Enumerable.Range(0, reader2.AttributeCount).Select(reader2.GetAttribute);
				if (!attributes1.SequenceEqual(attributes2))
				{
					return false;
				}
			}
		}

		if (MoveToContent(reader2))
		{
			return false;
		}

		return true;

		bool MoveToContent(XmlReader reader)
		{
			while (reader.Read())
			{
				if (reader.NodeType is XmlNodeType.Element or XmlNodeType.EndElement or XmlNodeType.Text)
				{
					return true;
				}
			}
			return false;
		}
	}
}
