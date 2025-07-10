using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	[ZArchitecture.ComponentModel.ModuleID("GlbCompanyCampaignItem")]
	public class GlbCompanyCampaignItemContactCollection : ActiveBusinessObjectCollection<GlbCompanyCampaignItem>, IGlbCompanyCampaignItemContactCollection
	{
		public GlbCompanyCampaignItemContactCollection(BusinessObjectFactory factory, OrgContact contact)
			: base(factory)
		{
			this.Contact = contact;
		}

		public GlbCompanyCampaignItemContactCollection(BusinessObjectFactory factory, IHRJobApplicant applicant)
			: base(factory)
		{
			this.Applicant = applicant;
		}

		public GlbCompanyCampaignItemContactCollection(BusinessObjectFactory factory, GlbStaff staff)
			: base(factory)
		{
			this.Staff = staff;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (Contact != null)
			{
				result.AddToFilter(new ZQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, Contact.PK));
				var applicant = Factory.LoadFromNaturalKey<IHRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, Contact.OC_Email);
				if (applicant != null)
				{
					var applicantQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, ((BusinessObject)applicant).PK);
					result.AddToFilter(applicantQuery, JoinCondition.Or);
				}
			}
			else if (Applicant != null)
			{
				result.AddToFilter(new ZQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, Applicant.PK));
			}
			else if (Staff != null)
			{
				result.AddToFilter(new ZQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, Staff.PK));
			}

			return result;
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		protected override void SetDefaultsForNewElementCore(GlbCompanyCampaignItem newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			newElement.G8_RecipientID = Contact.PK;
		}

		readonly OrgContact Contact;
		readonly IHRJobApplicant Applicant;
		readonly GlbStaff Staff;

		IEnumerator<IGlbCompanyCampaignItem> IEnumerable<IGlbCompanyCampaignItem>.GetEnumerator()
		{
			return base.GetEnumerator();
		}

		IGlbCompanyCampaignItem IGlbCompanyCampaignItemContactCollection.this[int index]
		{
			get { return base[index]; }
		}

		public override void Delete(GlbCompanyCampaignItem businessObject)
		{
			if ((Contact != null && businessObject.G8_RecipientID == Contact.PK) ||
				(Applicant != null && businessObject.G8_RecipientID == Applicant.PK) ||
				(Staff != null && businessObject.G8_RecipientID == Staff.PK))
			{
				base.Delete(businessObject);
			}
		}
	}
}
