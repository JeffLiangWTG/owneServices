using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class AccreditationUpdaterLookups : ZLookups
	{
		public AccreditationUpdaterLookups(AccreditationUpdater parent) : base(parent)
		{
		}

		public GlbAccreditationCollection AccreditationList
		{
			get
			{
				return Factory.GetCachedValue("GlbAccreditationLookups.GlbAccreditationCollection", () =>
				{
					return new GlbAccreditationCollection(Factory);
				});
			}
		}
	}
}
