using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSHeaderValidation : CusInBondHeaderValidation
	{
		public SPTSHeaderValidation(SPTSHeader parent) : base(parent)
		{
		}

		new SPTSHeader Parent => (SPTSHeader)base.Parent;

		protected override void CheckBH_VoyageNumber()
		{
			base.CheckBH_VoyageNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_VoyageNumberInfo);
		}

		protected override void CheckBH_SailingDate()
		{
			base.CheckBH_SailingDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_SailingDateInfo);
		}

		public void ValidateRegistrationDate()
		{
			ValidateCalculatedProperty(Parent.RegistrationDateInfo);
		}

		protected virtual void CheckRegistrationDate()
		{
			var registrationEntryNumber = Parent.RegistrationEntryNumber;
			if (registrationEntryNumber != null)
			{
				registrationEntryNumber.Validation.ValidateCE_IssueDate();
				Parent.RegistrationDateInfo.AddAllNotificationsFrom(registrationEntryNumber.CE_IssueDateInfo);
			}
		}
	}
}
