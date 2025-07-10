using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class TranshipmentMessageSendingObjectValidation : AutoTranshipmentMessageSendingObjectValidation
	{
		public TranshipmentMessageSendingObjectValidation(AutoTranshipmentMessageSendingObject parent) : base(parent)
		{
		}

		public new TranshipmentMessageSendingObject Parent => base.Parent as TranshipmentMessageSendingObject;

		public void ValidateShouldSend()
		{
			((IValidationInternals)this).Validate(Parent.ShouldSendInfo, () => { CheckShouldSend(); });
		}

		protected virtual void CheckShouldSend()
		{
			if (Parent.ShouldSend)
			{
				var targetInfo = Parent.ShouldSendInfo;
				var header = Parent.Header;
				var entryNumberGenerator = header?.EntryNumberGenerator;
				if (entryNumberGenerator != null)
				{
					if (entryNumberGenerator.EntryNumberPart1.IsEmpty)
					{
						targetInfo.AddError(MandatoryValidation.YouHaveNotEnteredMessage(header.ReceiptOfficeInfo.HumanReadableName));
					}
					if (entryNumberGenerator.EntryNumberPart2.IsEmpty)
					{
						targetInfo.AddError(MandatoryValidation.YouHaveNotEnteredMessage(header.UnladingOfficeInfo.HumanReadableName));
					}
					if (entryNumberGenerator.CustomsBrokerageBoxNumber.IsEmpty)
					{
						targetInfo.AddError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("20F6E362-BD9D-495A-9591-D3B36928E728", "Customs Broker Box Number")));
					}
				}
			}
		}

		protected override void CheckAction()
		{
			base.CheckAction();
			if (Parent?.ShouldSend ?? ZBool.False)
			{
				var targetInfo = Parent.ActionInfo;
				if (Parent.Action.IsEmpty)
				{
					targetInfo.AddError(Res.GetString("E9CC26DC-7D4A-4D8F-9FEB-7D9D47A25846", "Action is required if Send is ticked."));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(targetInfo, Parent.ActionList);
				}
			}
		}
	}
}
