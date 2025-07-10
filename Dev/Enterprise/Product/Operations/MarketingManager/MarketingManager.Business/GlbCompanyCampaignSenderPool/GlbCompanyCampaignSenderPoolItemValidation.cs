//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignSenderPoolItemValidation
//
//    This class should be used for overriding validation in AutoGlbCompanyCampaignSenderPoolItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSenderPoolItemValidation : AutoGlbCompanyCampaignSenderPoolItemValidation
	{
		public GlbCompanyCampaignSenderPoolItemValidation(AutoGlbCompanyCampaignSenderPoolItem parent)
			: base(parent)
		{
		}

		static MultilingualString InvalidStaffEmailMessage { get; } = ResString.GetMultilingualString("c584a0e4-f4a0-4521-af3d-2fd06bbd1653", "The selected campaign sender does not have a valid email address.\r\nPlease choose another campaign sender or change this staff member's email address");
		static MultilingualString MissingBranchMessage { get; } = ResString.GetMultilingualString("77BB5084-622B-43D8-A930-1F947EF0EDD1", "The staff cannot be selected as the local time cannot be determined without a home branch set. Please set a home branch for the staff or select staff member with a home branch already set.");

		protected override void CheckGCP_GS_NKSender()
		{
			base.CheckGCP_GS_NKSender();
			MandatoryValidation.CheckEntered(Parent.GCP_GS_NKSenderInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GCP_GS_NKSenderInfo, Parent.Lookups.Senders);

			if (Parent.Sender != null && !EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.Sender.GS_EmailAddress))
			{
				Parent.GCP_GS_NKSenderInfo.AddError(InvalidStaffEmailMessage);
			}

			if (Parent.Sender != null && Parent.Sender.GS_GB_HomeBranch.IsEmpty)
			{
				Parent.GCP_GS_NKSenderInfo.AddError(MissingBranchMessage);
			}
		}

		protected override void CheckGCP_SendRatio()
		{
			base.CheckGCP_SendRatio();
			MandatoryValidation.CheckValidShortGreaterOrEqualToZero(Parent.GCP_SendRatioInfo);
			MandatoryValidation.CheckNotZero(Parent.GCP_SendRatioInfo);
		}
	}
}
