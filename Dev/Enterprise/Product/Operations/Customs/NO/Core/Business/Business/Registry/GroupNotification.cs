using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry;

[XmlSerializerAssembly("Enterprise.Customs.NO.Business.XmlSerializers")]
public sealed class GroupNotification : Enterprise.Registry.Business.Customs.GroupNotification
{
	public GroupNotification()
	{
	}

	public GroupNotification(ZString sendMode, ZGuid sendGroupPK) : base(sendMode, sendGroupPK)
	{
	}

	public GroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	protected override CodeDescriptionPairList GetSendModeList() => new(OLookUpEditType.EmailTo);

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new GroupNotification(fallbackLevel, factory);
}
