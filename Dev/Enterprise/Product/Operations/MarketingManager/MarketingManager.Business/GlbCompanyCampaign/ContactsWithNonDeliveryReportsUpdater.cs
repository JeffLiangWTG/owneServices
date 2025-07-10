using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class ContactsWithNonDeliveryReportsUpdater : NonPersistentBusinessObject
	{
		public ContactsWithNonDeliveryReportsUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ContactsWithNonDeliveryReportsUpdater(BusinessObjectFactory factory, GlbCompanyCampaign campaign)
			: this(factory)
		{
			Campaign = campaign;
		}

		public readonly GlbCompanyCampaign Campaign;

		public GlbCompanyCampaignItem SelectedCampaignItem(CampaignContact campaignContact)
		{
			GlbCompanyCampaignItem campaignItem = null;
			if (Campaign != null)
			{
				campaignItem = Campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().FirstOrDefault(item => item.G8_RecipientID == campaignContact.PK);
			}
			return campaignItem;
		}

		#region Properties

		public ZString BounceBackEmail
		{
			get { return bounceBackEmail; }
			set
			{
				SetNonPersistentPropertyValue(BounceBackEmailInfo, ref bounceBackEmail, value);
			}
		}
		ZString bounceBackEmail;

		public ZPropertyInfo BounceBackEmailInfo
		{
			get { return GetZPropertyInfo(nameof(BounceBackEmail)); }
		}

		protected bool BounceBackEmail_ReadOnly
		{
			get { return true; }
		}

		#endregion

		public string ConfirmSaveMessage
		{
			get { return Res.GetString("6c22fb12-71c7-4ff7-be23-de17530ed148", "Are you sure you want to save the changes made to the following contact(s)?"); }
		}

		[ChildEditable]
		public ContactsWithNonDeliveryReportsCollection ContactsCollection
		{
			get
			{
				if (contactsCollection == null)
				{
					contactsCollection = new ContactsWithNonDeliveryReportsCollection(Factory);
					RegisterEditableChildObject(contactsCollection);
				}
				return contactsCollection;
			}
		}
		ContactsWithNonDeliveryReportsCollection contactsCollection;

		[ChildEditable]
		public GlbStaffCollection SendersCollection
		{
			get
			{
				if (sendersCollection == null)
				{
					if (Campaign != null)
					{
						var query = new ZQuery();
						var nks = new List<ZString>();
						nks.Add(Campaign.G0_GS_NKCampaignCoordinator);
						nks.AddRange(Campaign.SenderPool.Select(i => i.GCP_GS_NKSender));
						query.AddToFilter(GlbStaffSchema.GS_Code, nks);

						sendersCollection = new GlbStaffCollection(Factory, query);
					}
					else
					{
						sendersCollection = new GlbStaffCollection(Factory);
					}

					RegisterEditableChildObject(sendersCollection);
				}

				return sendersCollection;
			}
		}

		GlbStaffCollection sendersCollection;

		public void UpdateContacts()
		{
			foreach (CampaignContact campaignContact in ContactsCollection)
			{
				if (campaignContact.VCC_TableCode == OrgContactSchema.Constants.Prefix)
				{
					var contact = Factory.Load<OrgContact>(campaignContact.PK);
					if (campaignContact.IsEditing)
					{
						contact.OC_Email = campaignContact.VCC_Email;
						if (!contact.OC_Email.IsEmpty)
						{
							contact.IsNDR = campaignContact.IsNDR;
						}
					}

					if (campaignContact.IsDeactivating)
					{
						contact.OC_IsActive = ZBool.False;
					}
				}
				else if (campaignContact.VCC_TableCode == HRJobApplicantSchema.Constants.Prefix)
				{
					if (campaignContact.IsEditing)
					{
						var applicant = Factory.Load<IHRJobApplicant>(campaignContact.PK);
						applicant.UpdateEmailAddress(campaignContact.VCC_Email);
					}
				}
				else if (campaignContact.VCC_TableCode == GlbStaffSchema.Constants.Prefix)
				{
					if (campaignContact.IsEditing)
					{
						var staff = Factory.Load<GlbStaff>(campaignContact.PK);
						staff.GS_EmailAddress = campaignContact.VCC_Email;
					}
				}
				else
				{
					var inquiry = Factory.Load<SalesEnquiry>(campaignContact.PK);
					if (campaignContact.IsEditing && inquiry.LinkedContact != null)
					{
						inquiry.LinkedContact.OC_Email = campaignContact.VCC_Email;
						if (!inquiry.LinkedContact.OC_Email.IsEmpty)
						{
							inquiry.LinkedContact.IsNDR = campaignContact.IsNDR;
						}
					}
					else if (campaignContact.IsEditing && inquiry.LinkedContact == null)
					{
						inquiry.O1_Email = campaignContact.VCC_Email;
						if (!inquiry.O1_Email.IsEmpty)
						{
							inquiry.IsNDR = campaignContact.IsNDR;
						}
					}

					if (campaignContact.IsDeactivating && inquiry.LinkedContact != null)
					{
						inquiry.LinkedContact.OC_IsActive = ZBool.False;
					}
				}
			}

			Factory.Save();
		}
	}
}
