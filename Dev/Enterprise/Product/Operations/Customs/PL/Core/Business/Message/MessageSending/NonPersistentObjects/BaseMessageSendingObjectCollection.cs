using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class BaseMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<BaseMessageSendingObject>
{
	public BaseMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotImplementedException();

	protected override bool AllowNewCore => false;
}
