using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
public class MessageVersionRegistry : AutoMessageVersionRegistry
{
	public MessageVersionRegistry()
		: base()
	{
	}

	public MessageVersionRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public const string NCTSP5DomainCode = "NCTSP5";
	public const string NCTSP5DefaultTarget = "NCTS.NL";

	public const string DMSDomainCode = "DMS";
	public const string DMSDefaultTarget = "DMS.NL";

	#region Overrides

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new MessageVersionRegistry(fallbackLevel, factory);
	}

	protected sealed override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.DomainCode, DomainCode);
		writer.WriteElementString(Schema.TargetSystemName, TargetSystemName);
	}

	protected sealed override void ReadElements(XmlReaderWrapper reader)
	{
		DomainCode = reader.ReadElementString(Schema.DomainCode);
		TargetSystemName = reader.ReadElementString(Schema.TargetSystemName);
	}

	#endregion
}
