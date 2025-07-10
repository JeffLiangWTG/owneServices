using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationAttemptCollection : BusinessObjectCollection<GlbAccreditationAttempt>, IGlbAccreditationAttemptCollection
	{
		readonly GlbAccreditation accreditation;
		readonly GlbPerson person;

		public GlbAccreditationAttemptCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbAccreditationAttemptCollection(GlbAccreditation accreditation)
			: base(accreditation.Factory, new ZQuery(GlbAccreditationAttemptSchema.HAA_HAC, accreditation.PK))
		{
			this.accreditation = accreditation;
		}

		public GlbAccreditationAttemptCollection(GlbPerson person)
			: base(person.Factory, new ZQuery(GlbAccreditationAttemptSchema.HAA_PER, person.PK))
		{
			this.person = person;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var newAttempt = child as GlbAccreditationAttempt;
			if (newAttempt != null)
			{
				if (person != null)
				{
					newAttempt.HAA_PER = person.PK;
				}

				if (accreditation != null)
				{
					newAttempt.HAA_HAC = accreditation.PK;
				}
			}
		}

		IGlbAccreditationAttempt IGlbAccreditationAttemptCollection.this[int i]
		{
			get { return base[i]; }
		}

		void IGlbAccreditationAttemptCollection.Reload()
		{
			Reload(true, true);
		}

		protected override bool AllowNewCore => false;
	}
}
