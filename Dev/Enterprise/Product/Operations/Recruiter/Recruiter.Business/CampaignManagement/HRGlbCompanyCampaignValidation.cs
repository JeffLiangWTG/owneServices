using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRGlbCompanyCampaignValidation : GlbCompanyCampaignValidation
	{
		public HRGlbCompanyCampaignValidation(HRGlbCompanyCampaign parent)
			: base(parent)
		{
		}

		public new HRGlbCompanyCampaign Parent
		{
			get { return (HRGlbCompanyCampaign)base.Parent; }
		}

		#region ContactDataSource

		string lastSenderNotAvailableMessage;
		string LastSenderNotAvailableMessage
		{
			get
			{
				if (lastSenderNotAvailableMessage == null)
				{
					lastSenderNotAvailableMessage = Res.GetString("3D614BA9-5DB5-4CF4-8184-14CE146CBA01", "Last Sender function is not available in HR Campaign Management.");
				}

				return lastSenderNotAvailableMessage;
			}
		}

		protected override void CheckContactDataSource()
		{
			if (!Parent.GetIsValidationOnNonPersistentPropertiesSuspended)
			{
				MandatoryValidation.CheckEntered(Parent.ContactDataSourceInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ContactDataSourceInfo);

				if (!Parent.IsTouchCampaign && Parent.G0_UseLastEmailSenderAddress)
				{
					Parent.ContactDataSourceInfo.AddWarning(LastSenderNotAvailableMessage);
				}
			}
		}

		#endregion

		#region G0_UseLastEmailSenderAddress

		protected override void CheckG0_UseLastEmailSenderAddress()
		{
			base.CheckG0_UseLastEmailSenderAddress();

			if (!Parent.IsTouchCampaign && Parent.G0_UseLastEmailSenderAddress)
			{
				Parent.G0_UseLastEmailSenderAddressInfo.AddWarning(LastSenderNotAvailableMessage);
			}
		}

		#endregion

		#region SimulationContactDataSource

		public void ValidateSimulationContactDataSource()
		{
			ValidateCalculatedProperty(Parent.SimulationContactDataSourceInfo);
		}

		protected virtual void CheckSimulationContactDataSource()
		{
			if (!Parent.GetIsValidationOnNonPersistentPropertiesSuspended)
			{
				MandatoryValidation.CheckEntered(Parent.SimulationContactDataSourceInfo);
				ListValidation.ErrorIfInvalidCode(Parent.SimulationContactDataSourceInfo);
			}
		}

		#endregion
	}
}
