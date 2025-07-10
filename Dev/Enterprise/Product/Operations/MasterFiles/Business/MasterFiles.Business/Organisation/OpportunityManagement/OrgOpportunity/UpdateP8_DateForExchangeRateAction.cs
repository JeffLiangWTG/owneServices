using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class UpdateP8_DateForExchangeRateAction : AutoUpdateP8_DateForExchangeRateAction
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public UpdateP8_DateForExchangeRateAction(OrgOpportunity opportunity)
		{
			this.opportunity = opportunity;
			using (SuspendSettingHasChanges())
			{
				this.Date = opportunity.P8_DateForExchangeRate;
			}
		}

		readonly OrgOpportunity opportunity;

		public void Execute()
		{
			opportunity.P8_DateForExchangeRate = Date;
		}

		public override void ValidateDate()
		{
			base.ValidateDate();
			MandatoryValidation.CheckEntered(DateInfo);
		}
	}
}
