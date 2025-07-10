using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.DataTransfer
{
	public abstract class XmlDeclarationDataImporter : DeclarationDataImporter
	{
		public XmlDeclarationDataImporter(XmlDocument xmlDoc, BaseJobDeclaration toJobDec) : base(toJobDec)
		{
			DocReader = new XmlDocReader(xmlDoc);
		}

		protected readonly XmlDocReader DocReader;

		protected void SetPropertyInfoValue(XmlNode parentNode, string xPath, string attributeName, ZPropertyInfo propertyInfo)
		{
			XmlNode node = parentNode.SelectSingleNode(xPath);
			if (node != null)
			{
				XmlAttribute attribute = node.Attributes[attributeName];
				if (attribute != null)
				{
					SetPropertyInfoValue(propertyInfo, attribute.Value);
				}
			}
		}

		protected void SetPropertyInfoValue(XmlNode parentNode, string xPath, ZPropertyInfo propertyInfo)
		{
			XmlNode node = parentNode.SelectSingleNode(xPath);
			if (node != null)
			{
				SetPropertyInfoValue(propertyInfo, node.InnerText);
			}
		}
	}
}
