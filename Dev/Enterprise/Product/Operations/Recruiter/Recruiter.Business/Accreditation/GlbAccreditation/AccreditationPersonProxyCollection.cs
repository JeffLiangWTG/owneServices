using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	class AccreditationPersonProxyCollection : NonPersistentBusinessObjectCollection<AccreditationPersonProxy>, IAccreditationPersonProxyCollection
	{
		readonly GlbAccreditation accreditation;
		readonly GlbPerson person;

		IAccreditationPersonProxy IAccreditationPersonProxyCollection.this[int i] => this[i];

		public AccreditationPersonProxyCollection(GlbAccreditation accreditation, GlbPerson person)
			: base(accreditation?.Factory ?? person?.Factory ?? new BusinessObjectFactory())
		{
			this.accreditation = accreditation;
			this.person = person;
		}

		public void PopulateCollection()
		{
			if (accreditation == null)
			{
				return;
			}

			foreach (GlbAccreditation req in accreditation.Requirements)
			{
				Add(new AccreditationPersonProxy(req, person));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AccreditationPersonProxy(accreditation, person);
		}
	}
}
