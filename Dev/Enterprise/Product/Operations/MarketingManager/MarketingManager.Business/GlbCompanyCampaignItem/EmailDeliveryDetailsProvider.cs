using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class EmailDeliveryDetailsProvider : IEmailDeliveryDetailsProvider
	{
		public EmailDeliveryDetailsProvider(GlbCompanyCampaignItem campaignItem)
		{
			this.CampaignItem = campaignItem;
		}

		public EmailDeliveryDetailsProvider(OrgContact contact)
		{
			this.Contact = contact;
		}

		public readonly GlbCompanyCampaignItem CampaignItem;
		readonly OrgContact Contact;

		public ZString BounceBackEmail
		{
			get
			{
				if (CampaignItem != null)
				{
					return CampaignItem.BounceBackEmail;
				}
				else if (Contact != null)
				{
					var emailAddress = Contact.EmailAddress.GlbEmailAddress;
					if (emailAddress != null && emailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport)
					{
						var lastBounceBackNote = BounceBackEmailProcessor.GetLastBounceBackEmailNote(emailAddress);
						if (lastBounceBackNote != null)
						{
							return lastBounceBackNote.ST_NoteDataAsText;
						}
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ContactName
		{
			get
			{
				if (CampaignItem != null)
				{
					return CampaignItem.ContactName;
				}
				else if (Contact != null)
				{
					return Contact.Name;
				}

				return ZString.Empty;
			}
		}

		public ZString EmailAddress
		{
			get
			{
				if (CampaignItem != null)
				{
					return CampaignItem.EmailAddress;
				}
				else if (Contact != null)
				{
					return Contact.Email;
				}

				return ZString.Empty;
			}
		}

		public ZString TrackingStatusDescription
		{
			get
			{
				if (CampaignItem != null)
				{
					return CampaignItem.TrackingStatusDescription;
				}

				return TrackingStatusCodes.Descriptions.NDR;
			}
		}
	}
}
