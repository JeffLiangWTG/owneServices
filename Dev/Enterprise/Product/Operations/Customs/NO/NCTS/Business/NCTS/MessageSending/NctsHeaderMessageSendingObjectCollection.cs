using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.NCTS.Business;

public class NctsHeaderMessageSendingObjectCollection : EU.NCTS.Business.NctsHeaderMessageSendingObjectCollection
{
	public NctsHeaderMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public new NctsHeaderMessageSendingObject this[int index] => (NctsHeaderMessageSendingObject)base[index];

	public new NctsHeaderMessageSendingObject AddNew() => (NctsHeaderMessageSendingObject)base.AddNew();
}
