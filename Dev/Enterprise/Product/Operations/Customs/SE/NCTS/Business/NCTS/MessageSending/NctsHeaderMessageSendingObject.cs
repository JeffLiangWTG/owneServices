using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.SE.NCTS.Business;

public class NctsHeaderMessageSendingObject : EU.NCTS.Business.NctsHeaderMessageSendingObject
{
	public NctsHeaderMessageSendingObject(EU.NCTS.Business.NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	[ResourceStringData("E0BAED69-5AB7-4BC1-8E3F-30AE52AB00F1", Caption = "Message Type Description")]
	public ZString MessageTypeDescription => new MessageSendingConfiguration().MessageTypeList(this.NctsHeader).GetDescriptionFromCode(this.MessageType);

	public ZPropertyInfo MessageTypeDescriptionInfo => GetZPropertyInfo(nameof(MessageTypeDescription));
}
