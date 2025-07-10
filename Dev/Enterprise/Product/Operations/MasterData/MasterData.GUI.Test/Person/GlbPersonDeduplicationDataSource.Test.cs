using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	class PotentialDuplicatesListForGlbPersonTest : DeduplicationDataSourceTest<GlbPerson, DuplicationPersonCandidate>
	{
		public override void TestGetDuplicates()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var person = list[0];
			var person2 = list[1];

			Factory.Save();

			var glowCandidate1 = new MasterDataProvider().GetDeduplicationGlbPerson(person) as DeduplicationGlbPerson;
			var glowCandidate2 = new MasterDataProvider().GetDeduplicationGlbPerson(person2) as DeduplicationGlbPerson;
			var targetList = new List<DeduplicationGlbPerson>() { (DeduplicationGlbPerson)person2.CreateIGlbPerson() };
			var scoringResult = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			var presenter = new DeduplicationPresenter(glowCandidate1, new List<DeduplicationGlbPerson>() { glowCandidate2 }, new List<ScoringResult> { scoringResult });
			var presenterModels = presenter.GeneratePresenterModels();
			var potentialDuplicatesList = new GlbPersonDeduplicationDataSource(glowCandidate1, targetList, presenterModels);

			var duplicates1 = potentialDuplicatesList.GetPotentialDuplicates();
			var duplicates2 = potentialDuplicatesList.GetDuplicationCandidates();

			AssertEquals(1, duplicates1.Count());
			AssertEquals(1, duplicates2.Count());
		}

		public override void TestNullCurrentBizoParrameterInGetDuplication()
		{
			GetDataSource();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					(DeduplicationGlbPerson)Factory.New<GlbPerson>().CreateIGlbPerson(),
				}, new List<DeduplicationPresenterModel>());

			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();

			duplicationDataSource.GetDuplicationWhenCurrentBizOIsNull(populatedTarget);
		}

		public override void TestNullInTargetParameterInGetDuplication()
		{
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					(DeduplicationGlbPerson)Factory.New<GlbPerson>().CreateIGlbPerson(),
				}, new List<DeduplicationPresenterModel>());

			var populatedCurrentBizo = MasterBizO().CreateIGlbPerson();

			var emptyDedupModel = new DeduplicationPresenterModel();
			var emptyDedupList = new List<DeduplicationPresenterModel>();
			emptyDedupList.Add(emptyDedupModel);
			var target = new List<DeduplicationPresenterModel>(emptyDedupList);
			var unpopulatedTargets = target.GroupBy(x => x.TargetID);

			duplicationDataSource.GetDuplicationWhenTargetHasNullObject(populatedCurrentBizo, unpopulatedTargets);
		}

		public override void TestGetPotentialDuplicationModel()
		{
			CreateTargetPersons();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					(DeduplicationGlbPerson)Factory.New<GlbPerson>().CreateIGlbPerson(),
				}, new List<DeduplicationPresenterModel>());

			var person = MasterBizO();
			person.PER_IsActive = true;
			person.PER_MobilePhone = "+12015551111";
			var populatedCurrentBizo = person.CreateIGlbPerson();
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();
			var model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals(person.PER_FullName, model.Code);
			AssertEquals(person.PER_FullName, model.Name);
			AssertEquals("Yes", model.Active);
			AssertEquals(targetModel.Confidence, model.Confidence);
			AssertEquals(person.PER_FullName, model.Name);
			AssertEquals(((IDeduplicatable)person).Info, model.PersonType);
			AssertEquals(targetModel.TargetID, model.PK);
			AssertEquals(targetModel.MainScore, model.Score);
			AssertEquals("Phone should be formatted", "+1 201-555-1111", model.Phone);
			AssertEquals(populatedTarget, model.DeduplicationPresenterModels);
			AssertEquals(true, model.IsActive);
			AssertEquals(false, model.IsDummy);
			AssertEquals(false, model.IsDissolved);

			person.PER_IsActive = false;
			populatedCurrentBizo = person.CreateIGlbPerson(true);
			model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
			AssertEquals("No", model.Active);
			AssertEquals(false, model.IsActive);
			AssertEquals(true, model.IsDummy);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";
			var contact = org.Contacts.AddNew();
			contact.OC_PER = person.PK;
			person.ContactCollection.Add(contact);
			populatedCurrentBizo = person.CreateIGlbPerson();
			model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
			AssertEquals($"{person.PER_FullName}: TESTORG1", model.Code);
		}

		public override void TestGetDuplicationCandidate()
		{
			CreateTargetPersons();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					(DeduplicationGlbPerson)Factory.New<GlbPerson>().CreateIGlbPerson(),
				}, new List<DeduplicationPresenterModel>());

			var person = MasterBizO();
			person.PER_IsActive = true;
			person.PER_MobilePhone = "+12015551111";
			var populatedCurrentBizo = person.CreateIGlbPerson();
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();
			var candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals(person.PER_FullName, candidate.Code);
			AssertEquals(person.PER_FullName, candidate.Name);
			AssertEquals("Yes", candidate.Active);
			AssertEquals(targetModel.Confidence, candidate.Confidence);
			AssertEquals(person.PER_FullName, candidate.Name);
			AssertEquals(((IDeduplicatable)person).Info, candidate.Type);
			AssertEquals(targetModel.TargetID, candidate.TargetPK);
			AssertEquals(targetModel.MainScore, candidate.Score);
			AssertEquals("Phone should be formatted", "+1 201-555-1111", candidate.Phone);
			AssertEquals(populatedTarget, candidate.DeduplicationPresenterModels);
			AssertEquals(true, candidate.IsActive);
			AssertEquals(false, candidate.IsDummy);
			AssertEquals(false, candidate.IsDissolved);

			person.PER_IsActive = false;
			populatedCurrentBizo = person.CreateIGlbPerson(true);
			candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);
			AssertEquals("No", candidate.Active);
			AssertEquals(false, candidate.IsActive);
			AssertEquals(true, candidate.IsDummy);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";
			var contact = org.Contacts.AddNew();
			contact.OC_PER = person.PK;
			person.ContactCollection.Add(contact);
			populatedCurrentBizo = person.CreateIGlbPerson();
			candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);
			AssertEquals($"{person.PER_FullName}: TESTORG1", candidate.Code);
		}

		public void TestGetRelatedOrgs_SingleContact()
		{
			CreateTargetPersons();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				},
				new List<DeduplicationPresenterModel>()
			);

			var person = MasterBizO();
			person.PER_IsActive = true;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			var contact = org.Contacts.AddNew();
			contact.OC_PER = person.PK;
			person.ContactCollection.Add(contact);

			var populatedCurrentBizo = person.CreateIGlbPerson();
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();

			var model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
			var candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals("TESTORG", model.RelatedTo);
			AssertEquals("TESTORG", candidate.RelatedTo);
		}

		[ExpectNoExceptions]
		public void TestLoadDuplicateStaffShouldNotCauseThreadSentryError()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			CreateTargetPersons();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				},
				new List<DeduplicationPresenterModel>()
			);

			var person = MasterBizO();
			person.PER_IsActive = true;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = org.PK;
			staff.GS_GB_HomeBranch = branch.PK;

			var populatedCurrentBizo = person.CreateIGlbPerson();
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();

			var thread = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
					duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);
				}
			}, TaskCreationOptions.LongRunning);

			thread.Wait();
		}

		public void TestGetRelatedOrgs_SingleStaff()
		{
			CreateTargetPersons();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				},
				new List<DeduplicationPresenterModel>()
			);

			var person = MasterBizO();
			person.PER_IsActive = true;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = org.PK;
			staff.GS_GB_HomeBranch = branch.PK;

			var populatedCurrentBizo = person.CreateIGlbPerson();
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();

			var model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
			var candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals("TESTORG", model.RelatedTo);
			AssertEquals("TESTORG", candidate.RelatedTo);
		}

		public void TestGetRelatedOrgs_MaximumDisplayable()
		{
			CreateTargetPersons();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				},
				new List<DeduplicationPresenterModel>()
			);

			var person = MasterBizO();
			person.PER_IsActive = true;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "BBBORG";
			var contact = org1.Contacts.AddNew();
			contact.OC_PER = person.PK;
			person.ContactCollection.Add(contact);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AAAORG";
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = org2.PK;
			staff.GS_GB_HomeBranch = branch.PK;

			var populatedCurrentBizo = person.CreateIGlbPerson();
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();

			var model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
			var candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals("AAAORG, BBBORG", model.RelatedTo);
			AssertEquals("AAAORG, BBBORG", candidate.RelatedTo);
		}

		public void TestGetRelatedOrgs_SomeOrgsTruncated()
		{
			CreateTargetPersons();
			var duplicationDataSource = new GlbPersonDeduplicationDataSourceTest(new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					new MasterDataProvider().GetDeduplicationGlbPerson(Factory.New<GlbPerson>()) as DeduplicationGlbPerson,
				},
				new List<DeduplicationPresenterModel>()
			);

			var person = MasterBizO();
			person.PER_IsActive = true;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "DDDORG";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = person.PK;
			person.ContactCollection.Add(contact1);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AAAORG";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = person.PK;
			person.ContactCollection.Add(contact2);

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "CCCORG";
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_PER = person.PK;
			person.ContactCollection.Add(contact3);

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "BBBORG";
			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = org4.PK;
			staff.GS_GB_HomeBranch = branch.PK;

			var populatedCurrentBizo = person.CreateIGlbPerson();
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();

			var model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
			var candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals("AAAORG, BBBORG, ... (2 more)", model.RelatedTo);
			AssertEquals("AAAORG, BBBORG, ... (2 more)", candidate.RelatedTo);
		}

		protected override void AssertResultsOrdered(List<PotentialDuplicationModel> duplicates)
		{
			AssertEquals(5, duplicates.Count);

			AssertEquals(person5.PK, duplicates[0].PK);
			AssertEquals(person3.PK, duplicates[1].PK);
			AssertEquals(person2.PK, duplicates[2].PK);
			AssertEquals(person4.PK, duplicates[3].PK);
			AssertEquals(person6.PK, duplicates[4].PK);
		}

		protected override void AssertResultsOrdered(List<DuplicationCandidate> duplicates)
		{
			AssertEquals(5, duplicates.Count);

			AssertEquals(person5.PK, duplicates[0].TargetPK);
			AssertEquals(person3.PK, duplicates[1].TargetPK);
			AssertEquals(person2.PK, duplicates[2].TargetPK);
			AssertEquals(person4.PK, duplicates[3].TargetPK);
			AssertEquals(person6.PK, duplicates[4].TargetPK);
		}

		protected override GlbPerson MasterBizO()
		{
			person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_HomeAddress1 = "Some text";
			person1.PER_City = "Some City";
			person1.PER_FullName = "Test Person";
			person1.PER_MobilePhone = "0492052686";

			return person1;
		}

		GlbPerson person1;
		GlbPerson person2;
		GlbPerson person3;
		GlbPerson person4;
		GlbPerson person5;
		GlbPerson person6;

		protected override IDeduplicationDataSource GetDataSource()
		{
			CreateTargetPersons();
			return new GlbPersonDeduplicationDataSource(new MasterDataProvider().GetDeduplicationGlbPerson(MasterBizO()) as DeduplicationGlbPerson,
				new List<DeduplicationGlbPerson>()
				{
					(DeduplicationGlbPerson)person2.CreateIGlbPerson(),
					(DeduplicationGlbPerson)person3.CreateIGlbPerson(),
					(DeduplicationGlbPerson)person4.CreateIGlbPerson(),
					(DeduplicationGlbPerson)person5.CreateIGlbPerson(),
					(DeduplicationGlbPerson)person6.CreateIGlbPerson()
				}, PresenterModels());
		}

		protected override List<DeduplicationPresenterModel> PresenterModels()
		{
			return new List<DeduplicationPresenterModel>
			{
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.PersonNames,
					ChildMasterValue = person1.PER_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = person3.PER_FullName,
					Confidence = ConfidenceRating.Exact,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 0.9,
					MasterType = typeof(GlbPerson),
					Target = "TESORGSYD1",
					TargetID = person3.PK.ToGuid(),
					TargetValue = "TESORGSYD1"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 1,
					ChildConfidenceRating = ConfidenceRating.Exact,
					ChildDisplayModeForType = DeduplicationDisplayMode.Detailed,
					ChildDisplayNameForMasterColumns = "Address",
					ChildGroupNameForType = "Addresses",
					ChildMasterValue = "Some text Some City NSW AU",
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.NewGuid(),
					ChildTargetValue = "Some text Some City NSW AU",
					Confidence = ConfidenceRating.Exact,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 0.9,
					Master = "TESORGSYD",
					MasterType = typeof(GlbPerson),
					Target = "TESORGSYD1",
					TargetID = person3.PK.ToGuid(),
					TargetValue = "TESORGSYD1"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.PersonNames,
					ChildMasterValue = person1.PER_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = person5.PER_FullName,
					Confidence = ConfidenceRating.Exact,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 1,
					Master = "TESORGSYD",
					MasterType = typeof(GlbPerson),
					Target = "TESORGSYD2",
					TargetID = person5.PK.ToGuid(),
					TargetValue = "TESORGSYD2"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 1,
					ChildConfidenceRating = ConfidenceRating.Exact,
					ChildDisplayModeForType = DeduplicationDisplayMode.Detailed,
					ChildDisplayNameForMasterColumns = "Address",
					ChildGroupNameForType = "Addresses",
					ChildMasterValue = "Some text Some City NSW AU",
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.NewGuid(),
					ChildTargetValue = "Some text Some City NSW AU",
					Confidence = ConfidenceRating.Exact,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 1,
					Master = "TESORGSYD",
					MasterType = typeof(GlbPerson),
					Target = "TESORGSYD2",
					TargetID = person5.PK.ToGuid(),
					TargetValue = "TESORGSYD2"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.PersonNames,
					ChildMasterValue = person1.PER_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = person2.PER_FullName,
					Confidence = ConfidenceRating.High,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 0.93,
					Master = "TESORGSYD",
					Target = "TESORGAKL",
					TargetID = person2.PK.ToGuid(),
					TargetValue = "TESORGAKL"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.86,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.Detailed,
					ChildDisplayNameForMasterColumns = "Address",
					ChildGroupNameForType = "Addresses",
					ChildMasterValue = "Some text Some City NSW AU",
					ChildScore = 0.86,
					ChildScoringResultGroupRating = ConfidenceRating.High,
					ChildTargetID = Guid.NewGuid(),
					ChildTargetValue = "Some text Some City AUK NZ",
					Confidence = ConfidenceRating.High,
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Persons",
					MainScore = 0.93,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL",
					TargetID = person2.PK.ToGuid(),
					TargetValue = "TESORGAKL"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.PersonNames,
					ChildMasterValue = person1.PER_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = person4.PER_FullName,
					Confidence = ConfidenceRating.High,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 0.92,
					Master = "TESORGSYD",
					MasterType = typeof(GlbPerson),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL1",
					TargetID = person4.PK.ToGuid(),
					TargetValue = "TESORGAKL1"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.86,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.Detailed,
					ChildDisplayNameForMasterColumns = "Address",
					ChildGroupNameForType = "Addresses",
					ChildMasterValue = "Some text Some City NSW AU",
					ChildScore = 0.86,
					ChildScoringResultGroupRating = ConfidenceRating.High,
					ChildTargetID = Guid.NewGuid(),
					ChildTargetValue = "Some text Some City AUK NZ",
					Confidence = ConfidenceRating.High,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 0.92,
					Master = "TESORGSYD",
					MasterType = typeof(GlbPerson),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL1",
					TargetID = person4.PK.ToGuid(),
					TargetValue = "TESORGAKL1"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.None,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.PersonNames,
					ChildMasterValue = person1.PER_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.None,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = person6.PER_FullName,
					Confidence = ConfidenceRating.None,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 0.11,
					Master = "TESORGSYD",
					MasterType = typeof(GlbPerson),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL2",
					TargetID = person6.PK.ToGuid(),
					TargetValue = "TESORGAKL2"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.86,
					ChildConfidenceRating = ConfidenceRating.None,
					ChildDisplayModeForType = DeduplicationDisplayMode.Detailed,
					ChildDisplayNameForMasterColumns = "Address",
					ChildGroupNameForType = "Addresses",
					ChildMasterValue = "Some text Some City NSW AU",
					ChildScore = 0.86,
					ChildScoringResultGroupRating = ConfidenceRating.None,
					ChildTargetID = Guid.NewGuid(),
					ChildTargetValue = "Some text Some City AUK NZ",
					Confidence = ConfidenceRating.None,
					GlowTargetType = typeof(IGlbPerson),
					GroupNameForType = "Persons",
					MainScore = 0.11,
					Master = "TESORGSYD",
					MasterType = typeof(GlbPerson),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL2",
					TargetID = person6.PK.ToGuid(),
					TargetValue = "TESORGAKL2"
				}
			};
		}

		void CreateTargetPersons()
		{
			person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_HomeAddress1 = "Some text";
			person2.PER_City = "Some City";
			person2.PER_FullName = "Test Org 2";

			person3 = Factory.NewWithValidTestData<GlbPerson>();
			person3.PER_HomeAddress1 = "Some text";
			person3.PER_City = "Some City";
			person3.PER_FullName = "Test Org 3";

			person4 = Factory.NewWithValidTestData<GlbPerson>();
			person4.PER_HomeAddress1 = "Some text";
			person4.PER_City = "Some City";
			person4.PER_FullName = "Test Org 4";

			person5 = Factory.NewWithValidTestData<GlbPerson>();
			person5.PER_HomeAddress1 = "Some text";
			person5.PER_City = "Some City";
			person5.PER_FullName = "Test Org 5";

			person6 = Factory.NewWithValidTestData<GlbPerson>();
			person6.PER_HomeAddress1 = "Some text";
			person6.PER_City = "Some City";
			person6.PER_FullName = "Test Org 6";

			Factory.Save();
		}
	}

	class GlbPersonDeduplicationDataSourceTest : GlbPersonDeduplicationDataSource
	{
		public GlbPersonDeduplicationDataSourceTest(DeduplicationGlbPerson master, IEnumerable<DeduplicationGlbPerson> targets, IEnumerable<DeduplicationPresenterModel> results)
			: base(master, targets, results)
		{
		}

		public void GetDuplicationWhenCurrentBizOIsNull(IGrouping<Guid, DeduplicationPresenterModel> populatedTarget)
		{
			GetPotentialDuplicationModel(null, populatedTarget);
			GetDuplicationCandidate(null, populatedTarget);
		}

		public void GetDuplicationWhenTargetHasNullObject(IGlbPerson populatedCurrentBizo, IEnumerable<IGrouping<Guid, DeduplicationPresenterModel>> unpopulatedTarget)
		{
			var empty = unpopulatedTarget.FirstOrDefault();
			GetPotentialDuplicationModel(populatedCurrentBizo, empty);
			GetDuplicationCandidate(populatedCurrentBizo, empty);
		}

		public PotentialDuplicationModel GetPotentialDuplicationModelForTest(IGlbPerson currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			return GetPotentialDuplicationModel(currentBizo, target);
		}

		public DuplicationPersonCandidate GetDuplicationCandidateForTest(IGlbPerson currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			return GetDuplicationCandidate(currentBizo, target);
		}
	}
}
