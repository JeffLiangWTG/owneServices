using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class MultiManifestBillSenderValidation : AutoMultiManifestBillSenderValidation
	{
		public MultiManifestBillSenderValidation(AutoMultiManifestBillSender parent)
			: base(parent)
		{
		}

		protected override void CheckCycleDate()
		{
			base.CheckCycleDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CycleDateInfo);
			if (Parent.CycleDate < ZDate.Today)
			{
				Parent.CycleDateInfo.AddMessageError(ResString.GetMultilingualString("{976F0A26-11BF-4464-B0F1-035C1351CB55}", "Cycle Date can not be in the past."));
			}
		}

		protected override void CheckCycleNumber()
		{
			base.CheckCycleNumber();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CycleNumberInfo);
		}

		protected new MultiManifestBillSender Parent => (MultiManifestBillSender)base.Parent;
	}
}
