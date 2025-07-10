using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class ReceiptNumberHolder : AutoReceiptNumberHolder
	{
		public ReceiptNumberHolder(VAT404DocumentInstruction parent) : base(parent.Factory)
		{
			Parent = parent;
		}

		internal VAT404DocumentInstruction Parent;
	}

	public class ReceiptNumberHolderValidation : AutoReceiptNumberHolderValidation
	{
		public ReceiptNumberHolderValidation(AutoReceiptNumberHolder parent) : base(parent)
		{
		}
		VAT404DocumentInstruction ParentInstruction => (Parent as ReceiptNumberHolder).Parent;

		protected override void CheckReceiptNumber()
		{
			base.CheckReceiptNumber();
			var targetInfo = Parent.ReceiptNumberInfo;
			MandatoryValidation.WarnIfNotEntered(targetInfo);
			if (ParentInstruction.ReceiptNumbersForFilter.OfType<ReceiptNumberHolder>().Any(x => x != Parent && x.ReceiptNumber == Parent.ReceiptNumber))
			{
				targetInfo.AddWarning(Res.GetString("A91C5D1B-8BD9-4D5B-9D8B-568FC4A88DF0", "This value has already been entered."));
			}
		}
	}
}
