using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
public class MessageVersionRegistryCollection : RegistryBusinessObjectCollectionTemplate
{
	public MessageVersionRegistryCollection()
	{
	}

	public MessageVersionRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public static MessageVersionRegistryCollection DefaultCollection => new MessageVersionRegistryCollection
	{
		new MessageVersionRegistry { DomainCode = MessageVersionRegistry.DMSDomainCode, TargetSystemName = MessageVersionRegistry.DMSDefaultTarget },
		new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = MessageVersionRegistry.NCTSP5DefaultTarget },
	};

	public new MessageVersionRegistry this[int i] => (MessageVersionRegistry)Elements[i];

	public new MessageVersionRegistry AddNew() => (MessageVersionRegistry)base.AddNew();

	protected override BusinessObject CreateNonPersistentBusinessObject() => new MessageVersionRegistry(CurrentFallbackLevel, CurrentFactory);

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new MessageVersionRegistryCollection(fallbackLevel, factory);

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;
}
