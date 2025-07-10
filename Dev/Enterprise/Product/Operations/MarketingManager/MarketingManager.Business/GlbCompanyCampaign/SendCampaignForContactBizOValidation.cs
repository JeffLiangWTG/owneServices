using System;

using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class SendCampaignForContactBizOValidation : ZValidation
	{
		public SendCampaignForContactBizOValidation(SendCampaignForContactBizO parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(SendCampaignForContactBizOValidation); }
		}

		#region CampaignPK

		public void ValidateCampaignPK()
		{
			ValidateCalculatedProperty(Parent.CampaignPKInfo);
		}

		protected void CheckCampaignPK()
		{
			MandatoryValidation.CheckEntered(Parent.CampaignPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.CampaignPKInfo);
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateCampaignPK();
		}

		#endregion

		readonly SendCampaignForContactBizO Parent;
	}
}
