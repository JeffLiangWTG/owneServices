using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.CusPerson)]
	public class GlbPersonCollection : ActiveBusinessObjectCollection<GlbPerson>
	{
		readonly GlbPerson master;
		readonly ZGuid[] staffs;
		readonly ZGuid[] contacts;
		readonly ZGuid[] applicants;

		public GlbPersonCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbPersonCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public GlbPersonCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		public GlbPersonCollection(GlbPerson master, ZGuid[] staffs, ZGuid[] contacts, ZGuid[] applicants)
			: base(master.Factory, ZQuery.NoResultQuery)
		{
			this.master = master;
			this.staffs = staffs;
			this.contacts = contacts;
			this.applicants = applicants;
		}

		bool TransferringToNewPerson => master != null;

		protected override void SetDefaultsForNewElementCore(GlbPerson newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (!TransferringToNewPerson)
			{
				return;
			}

			newElement.IsMovingFromAnotherPerson = true;
			newElement.CopyPersistentValuesFrom(master);
			newElement.PER_WebAccessEnabled = false;
			var primaryId = master.PrimaryRelationship?.PPR_PrimaryId ?? ZGuid.Empty;
			IGlbPersonPrimarySource primary = null;

			if (staffs != null && staffs.Length > 0)
			{
				foreach (var staff in newElement.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffs) { ReLoadExistingRows = true }))
				{
					staff.GS_PER = newElement.PK;
					staff.UpdateFromPerson(newElement);
					if (staff.PK == primaryId)
					{
						primary = staff;
					}
				}
			}

			if (contacts != null && contacts.Length > 0)
			{
				foreach (var contact in newElement.Factory.Load<OrgContact>(new ZQuery(OrgContactSchema.PK, contacts) { ReLoadExistingRows = true }))
				{
					contact.OC_PER = newElement.PK;
					contact.UpdateFromPerson(newElement);
					if (contact.PK == primaryId)
					{
						primary = contact;
					}
				}
			}

			if (applicants != null && applicants.Length > 0)
			{
				foreach (var applicant in newElement.Factory.Load<IHRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, applicants) { ReLoadExistingRows = true }))
				{
					applicant.HA_PER = newElement.PK;
					applicant.UpdateFromPerson(newElement);
				}
			}

			if (primary != null)
			{
				newElement.SetPrimaryRelationship(primary);
			}
		}
	}
}
