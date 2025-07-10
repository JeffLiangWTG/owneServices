using System.Xml.Serialization;

namespace Enterprise.Customs.US.DIS.Messaging.DataFileSchema
{
	partial class MessageEnvelope
	{
		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces Namespaces = GetNamespaces();

		static XmlSerializerNamespaces GetNamespaces()
		{
			return new XmlSerializerNamespaces(new System.Xml.XmlQualifiedName[]
			{
				new System.Xml.XmlQualifiedName(Namespace.XSIName, Namespace.XSINS),
				new System.Xml.XmlQualifiedName(Namespace.DISName, Namespace.DISNS),
			});
		}

		public static class Namespace
		{
			public const string XSIName = "xsi";
			public const string XSINS = "http://www.w3.org/2001/XMLSchema-instance";

			public const string DISName = "DIS";
			public const string DISNS = "http://cbp.dhs.gov/DIS";
		}
	}
}
