using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicantCollection : BusinessObjectCollection<HRJobApplicant>, Enterprise.Integration.Recruiter.IHRJobApplicantCollection
	{
		public HRJobApplicantCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public HRJobApplicantCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		readonly GlbPerson master;
		public HRJobApplicantCollection(GlbPerson master, ZQuery filter)
			: base(master.Factory, filter)
		{
			this.master = master;
		}

		protected override void SetDefaultsForNewChild(BusinessObject newApplicant)
		{
			base.SetDefaultsForNewChild(newApplicant);
			var applicant = newApplicant as HRJobApplicant;

			if (master != null && applicant != null)
			{
				applicant.SetFromPerson(master);
			}
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ApplicantFindBoxListProvider(this); }
		}

		class ApplicantFindBoxListProvider : FindBoxListProvider
		{
			public ApplicantFindBoxListProvider(IBusinessObjectCollection collection) : base(collection)
			{
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				base.AddCodeEqualsFilter(query, code);
				query.IsNoResultQuery = false;
				var mainQuery = new ZDBOnlyQuery(typeof(HRJobApplicant));
				var personSub = new ZDBOnlySubQuery(typeof(GlbPerson), HRJobApplicantSchema.HA_PER);
				personSub.AddToFilter(GlbPersonSchema.PER_FullName, code);
				mainQuery.AddSubQuery(personSub, JoinCondition.And);
				query.AddToFilter(mainQuery);
			}
		}
	}
}
