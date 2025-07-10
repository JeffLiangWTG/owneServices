//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewCampaignContactValidation
//
//    This class should be used for overriding validation in AutoViewCampaignContactValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class ViewCampaignContactValidation : AutoViewCampaignContactValidation
	{
		public ViewCampaignContactValidation(AutoViewCampaignContact parent)
			: base(parent)
		{
		}

		#region Email

		protected override void CheckVCC_Email()
		{
			base.CheckVCC_Email();

			if (!EmailAddressValidation.IsEmailAddressValid(Parent.VCC_Email))
			{
				Parent.VCC_EmailInfo.AddError(EnterCorrectEmailAddressErrorMessage);
			}
		}

		string EnterCorrectEmailAddressErrorMessage
		{
			get { return Res.GetString("b770c59a-9083-4a03-a913-8982394b87f0", "Please enter a valid email address."); }
		}

		#endregion
	}
}
