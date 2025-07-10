using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaignItem : IBusiness
	{
		ZDateTime G8_SystemCreateTimeUtc { get; }
		ZPropertyInfo G8_SystemCreateTimeUtcInfo { get; }

		ZString G8_SystemCreateUser { get; }
		ZPropertyInfo G8_SystemCreateUserInfo { get; }

		ZDateTime G8_FollowedUp { get; set; }
		ZPropertyInfo G8_FollowedUpInfo { get; }

		ZString G8_GS_NKFollowedUpBy { get; set; }
		ZPropertyInfo G8_GS_NKFollowedUpByInfo { get; }

		ZDateTime LastCommunicationDate { get; }
		ZPropertyInfo LastActualCommunicationInfo { get; }

		ZString LastCommunicationStaffCode { get; }
		ZPropertyInfo LastCommunicationStaffCodeInfo { get; }

		ZString G8_DeliveryMethod { get; }
		ZPropertyInfo G8_DeliveryMethodInfo { get; }

		ZString CampaignID { get; }
		ZPropertyInfo CampaignIDInfo { get; }

		ZString CampaignName { get; }
		ZPropertyInfo CampaignNameInfo { get; }

		ZString G8_RecipientTableCode { get; set; }
		ZPropertyInfo G8_RecipientTableCodeInfo { get; }

		ZGuid G8_RecipientID { get; set; }
		ZPropertyInfo G8_RecipientIDInfo { get; }

		ZString G8_TrackingStatus { get; }
		ZString TrackingStatusDescription { get; }
		ZPropertyInfo TrackingStatusDescriptionInfo { get; }

		ZDateTime G8_LastSentTimeUtc { get; }

		ZGuid G8_G0 { get; set; }

		ZGuid PK { get; }

		ZBool IsUnsubscribed { get; }

		void FollowUp();
		void MarkAsVerified();

		IGlbCompanyCampaign Campaign { get; }
		ISalesRelationModel SalesRelationModel { get; }
	}
}
