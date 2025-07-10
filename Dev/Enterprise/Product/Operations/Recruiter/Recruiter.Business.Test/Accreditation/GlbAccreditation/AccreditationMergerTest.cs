using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class AccreditationMergerTest : TestCaseWithFactory
	{
		public void TestInterface()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();

			var attemp1 = retainedPerson.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attemp1.HAA_HAC = accreditation1.PK;
			var attemp2 = retainedPerson.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attemp2.HAA_HAC = accreditation2.PK;

			var attemp3 = dissolvedPerson.AccreditationAttemptCollection.AddNew() as GlbAccreditationAttempt;
			attemp3.HAA_HAC = accreditation1.PK;

			attemp1.HAA_CommencementDate = attemp2.HAA_CommencementDate = attemp3.HAA_CommencementDate = ZDate.Today;
			attemp1.HAA_CompletionDueDate = attemp2.HAA_CompletionDueDate = attemp3.HAA_CompletionDueDate = ZDate.Today.AddDays(30);

			Factory.Save();

			var accreditationMerger = ObjectFactory.New<IAccreditationMerger>(retainedPerson, dissolvedPerson);

			attemp3.HAA_PER = retainedPerson.PK;
			Factory.Save();
			dissolvedPerson.Delete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			accreditationMerger.Merge(newFactory);
			newFactory.Save();

			var retainedPersonInNewFactory = newFactory.Load<GlbPerson>(retainedPerson.PK);
			AssertEquals(attemp2.PK, retainedPersonInNewFactory.AccreditationAttemptCollection.OfType<IGlbAccreditationAttempt>().Single().PK);
		}
	}
}
