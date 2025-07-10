using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module;

[TestedType(typeof(Candidate))]
sealed class CandidateTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => new Candidate(Factory, Factory.NewWithValidTestData<HRJobApplication>().PK);

	public void TestEConversationNull_DummyForBinding()
	{
		var collection = new CandidateBusinessObjectCollection(Factory);
		var module = new CandidateModuleBusinessObject(collection);
		AssertNull(module.DummyForBinding.EConversation);
	}

	public void TestDeleteCandidateViaDataRefreshBusFromParent()
	{
		var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
		var application = Factory.NewWithValidTestData<HRJobApplication>();
		application.HP_HA = applicant.PK;
		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var candidate = new Candidate(newFactory, application.PK);
		var collection = new CandidateBusinessObjectCollection(newFactory);
		collection.Add(candidate);
		AssertEquals(1, collection.Count);

		applicant.Delete();
		Factory.Save();

		AssertEquals(0, collection.Count);
	}

	[GuiTest]
	public void TestCandidateLogEvents_ProfileEdited()
	{
		// arrange
		var factory = new BusinessObjectFactory();
		var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");
		using (var ctrl = new CandidateDetailsControl())
		{
			ctrl.SetDataBinding(candidate, "");

			// act
			ctrl.ValidateAndSave();
		}

		// assert
		Assert(candidate.Logs.Any(log => log.Event.SE_Code == AutoEvents.EditedARecord.Code));
	}
}
