using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_AXMessageSendingObjectValidation : LicensingMessageSendingObjectValidation
	{
		public NX301_AXMessageSendingObjectValidation(NX301_AXMessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckAction()
		{
			var targetInfo = Parent.ActionInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}
	}
}
