using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Manifest.Business;

public class DMOMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<DMOMessageSendingObject>
{
	public DMOMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new NotImplementedException();
	}

	protected override bool AllowNewCore => false;
}
