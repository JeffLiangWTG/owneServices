using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class LocalReferenceNumberHolder : AutoLocalReferenceNumberHolder
	{
		public LocalReferenceNumberHolder(VAT404DocumentInstruction parent) : base(parent.Factory)
		{
			Parent = parent;
		}

		internal VAT404DocumentInstruction Parent;
	}

	public class LocalReferenceNumberHolderValidation : AutoLocalReferenceNumberHolderValidation
	{
		public LocalReferenceNumberHolderValidation(AutoLocalReferenceNumberHolder parent) : base(parent)
		{
		}

		VAT404DocumentInstruction ParentInstruction => (Parent as LocalReferenceNumberHolder).Parent;

		protected override void CheckLocalReferenceNumber()
		{
			base.CheckLocalReferenceNumber();
			var targetInfo = Parent.LocalReferenceNumberInfo;
			MandatoryValidation.WarnIfNotEntered(targetInfo);
			ValidationHelper.ValidateLRNFormat(targetInfo);
			if (ParentInstruction.LocalReferenceNumbersForFilter.OfType<LocalReferenceNumberHolder>().Any(x => x != Parent && x.LocalReferenceNumber == Parent.LocalReferenceNumber))
			{
				targetInfo.AddWarning(Res.GetString("A91C5D1B-8BD9-4D5B-9D8B-568FC4A88DF0", "This value has already been entered."));
			}
		}
	}
}
