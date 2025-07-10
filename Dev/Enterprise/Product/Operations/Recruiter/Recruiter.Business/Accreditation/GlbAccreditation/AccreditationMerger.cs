using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class AccreditationMerger : IAccreditationMerger
	{
		public AccreditationMerger(GlbPerson retainedPerson, GlbPerson dissolvedPerson)
		{
			RetainedPersonPk = retainedPerson.PK;
			AccreditationPks = new HashSet<ZGuid>(retainedPerson.AccreditationAttemptCollection.OfType<IGlbAccreditationAttempt>().Select(x => x.HAA_HAC)
				.Intersect(
					dissolvedPerson.AccreditationAttemptCollection.OfType<IGlbAccreditationAttempt>().Select(x => x.HAA_HAC)));
		}

		public void Merge(BusinessObjectFactory factory)
		{
			if (AccreditationPks.Any())
			{
				var person = factory.Load<GlbPerson>(RetainedPersonPk);

				foreach (var accreditation in factory.Load<GlbAccreditation>(new ZQuery(GlbAccreditationSchema.PK, AccreditationPks)))
				{
					var updater = new AccreditationUpdaterForPerson()
					{
						IsFirstExamType = true,
						DeleteExistingCertificates = false,
						CompletionToleranceDays = accreditation.HAC_MustCompleteInDays,
					};
					updater.SetPersons(new[] { person });
					updater.ParentAccreditationPK = accreditation.PK;
					updater.Run();
				}
			}
		}

		readonly ZGuid RetainedPersonPk;
		readonly HashSet<ZGuid> AccreditationPks;
	}
}
