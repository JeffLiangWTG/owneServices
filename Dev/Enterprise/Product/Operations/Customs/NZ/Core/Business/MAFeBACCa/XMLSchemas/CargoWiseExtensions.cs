using System.Xml.Serialization;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas
{
	[XmlSerializerAssembly("Enterprise.Customs.NZ.Business.XmlSerializers")]
	public partial class MessagingRequestType
	{
		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces Namespaces = GetNamespaces();

		static XmlSerializerNamespaces GetNamespaces()
		{
			return new XmlSerializerNamespaces(new System.Xml.XmlQualifiedName[]
			{
				new System.Xml.XmlQualifiedName(Namespace.Name, Namespace.Ns),
			});
		}

		public static class Namespace
		{
			public const string Name = "maf";
			public const string Ns = "http://www.maf.govt.nz/Messaging/Request/2008/03";
		}

		// In class MessagingRequestTypeBody, changed the definition of the property "public object MetaData" to:
		// [XmlArrayItem("EBACCARequest", IsNullable = false, Namespace = BACCApplicationType.Namespace.Ns, Type = typeof(BACCApplicationType))]
		// public BACCApplicationType[] MetaData
	}

	public partial class BACCApplicationType
	{
		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces Namespaces = GetNamespaces();

		static XmlSerializerNamespaces GetNamespaces()
		{
			return new XmlSerializerNamespaces(new System.Xml.XmlQualifiedName[]
			{
				new System.Xml.XmlQualifiedName(Namespace.Name, Namespace.Ns),
			});
		}

		public static class Namespace
		{
			public const string Name = "ebacca";
			public const string Ns = "http://www.maf.govt.nz/EBACCA/Messaging/Request/2008/03/";
		}
	}

	[XmlSerializerAssembly("Enterprise.Customs.NZ.Business.XmlSerializers")]
	public partial class MessagingResponseType
	{ }

	[XmlSerializerAssembly("Enterprise.Customs.NZ.Business.XmlSerializers")]
	public partial class EBACCANotificationType
	{ }

	[XmlSerializerAssembly("Enterprise.Customs.NZ.Business.XmlSerializers")]
	public partial class EBACCAResponseType
	{ }
}
