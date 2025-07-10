using System.Linq;
using System.Xml.Linq;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public static class XmlUtil
	{
		public static bool CompareXmlElements(string xml1, string xml2)
		{
			var elem1 = XElement.Parse(xml1);
			var elem2 = XElement.Parse(xml2);

			return CompareElements(elem1, elem2);
		}
		static bool CompareElements(XElement elem1, XElement elem2)
		{
			if (elem1.Name != elem2.Name)
			{
				return false;
			}

			if (elem1.Attributes().Count() != elem2.Attributes().Count())
			{
				return false;
			}

			foreach (var attr in elem1.Attributes())
			{
				var otherAttr = elem2.Attribute(attr.Name);
				if (otherAttr == null || otherAttr.Value != attr.Value)
				{
					return false;
				}
			}

			var children1 = elem1.Elements().ToList();
			var children2 = elem2.Elements().ToList();

			if (children1.Count != children2.Count)
			{
				return false;
			}

			for (int i = 0; i < children1.Count; i++)
			{
				if (!CompareElements(children1[i], children2[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
