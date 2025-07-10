using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.NL.ExitControl.Business;

public class ExitControlMessageSendingObjectCollection : EU.ExitControl.Business.ExitControlMessageSendingObjectCollection
{
	public ExitControlMessageSendingObjectCollection(IEnumerable<CusExitReport> messagingObjects, BusinessObjectFactory factory) : base(messagingObjects, factory)
	{
	}

	public new ExitControlMessageSendingObject this[int i] => (ExitControlMessageSendingObject)base[i];

	public new ExitControlMessageSendingObject AddNew() => (ExitControlMessageSendingObject)base.AddNew();

	protected override EU.ExitControl.Business.ExitControlMessageSendingObject GetSendingAction(CusExitReport messagingObject) => new ExitControlMessageSendingObject(messagingObject);
}
