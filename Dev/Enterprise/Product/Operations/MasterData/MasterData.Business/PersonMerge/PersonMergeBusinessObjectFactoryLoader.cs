using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;

namespace Enterprise.MasterData.Business
{
	public class PersonMergeBusinessObjectFactoryLoader
	{
		public virtual void Reload(BusinessObjectFactory newFactory, GlbPerson retainedPerson, GlbPerson dissolvedPerson, BusinessObjectFactory originalFactory, PersonMergeMode mergeMode = PersonMergeMode.Single)
		{
			newFactory.ForcePublishForDataRefresh(dissolvedPerson);

			retainedPerson.ApplicantCollection.Reload(true, true);
			retainedPerson.ContactCollection.Reload(true, true);
			retainedPerson.ReloadGlbStaffCollectionFromDb();

			originalFactory.ReloadAll<HRJobApplicant>();
			originalFactory.ReloadAll<OrgContact>();
			originalFactory.ReloadAll<GlbStaff>();
			originalFactory.ReloadAll<GlbPerson>();

			if (mergeMode == PersonMergeMode.Multi)
			{
				retainedPerson.Reload();
			}
		}
	}
}

