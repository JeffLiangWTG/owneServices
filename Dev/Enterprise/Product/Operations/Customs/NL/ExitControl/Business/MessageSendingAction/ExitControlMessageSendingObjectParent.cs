using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NL.ExitControl.Business;

[CodeAlive("Will be used with WI00806555 next work flows with send message menu item")]
public class ExitControlMessageSendingObjectParent : EU.ExitControl.Business.ExitControlMessageSendingObjectParent
{
	public ExitControlMessageSendingObjectParent(CusExitHeader cusExitHeader) : base(cusExitHeader)
	{
	}

	public new ExitControlMessageSendingObjectCollection SendingObjectsCollection => (ExitControlMessageSendingObjectCollection)base.SendingObjectsCollection;

	protected override NonPersistentBusinessObjectCollection<EU.ExitControl.Business.ExitControlMessageSendingObject> GetSendingObjectsCollectionCore() => new ExitControlMessageSendingObjectCollection(reports, Factory);
}
