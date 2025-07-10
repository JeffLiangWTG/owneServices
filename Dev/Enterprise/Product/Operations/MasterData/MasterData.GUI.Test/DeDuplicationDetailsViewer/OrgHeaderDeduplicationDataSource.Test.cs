using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI.Tests
{
	class OrgHeaderDeduplicationDataSourceTest : DeduplicationDataSourceTest<OrgHeader, DuplicationOrganisationCandidate>
	{
		public override void TestGetDuplicates()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org2 = list[1];

			Factory.Save();

			var glowCandidate1 = new DeduplicationOrgHeader(org);
			var glowCandidate2 = new DeduplicationOrgHeader(org2);
			var targetList = new List<DeduplicationOrgHeader>() { new DeduplicationOrgHeader(org2) };
			var scoringResult = TargetScorerController.Score(glowCandidate1, glowCandidate2, true);
			var presenterModels = new DeduplicationPresenter(glowCandidate1, new List<DeduplicationOrgHeader>() { glowCandidate2 }, new List<ScoringResult> { scoringResult }).GeneratePresenterModels();
			var potentialDuplicatesOrgList = new OrgHeaderDeduplicationDataSource(glowCandidate1, targetList, presenterModels);

			var duplicates1 = potentialDuplicatesOrgList.GetPotentialDuplicates();
			var duplicates2 = potentialDuplicatesOrgList.GetDuplicationCandidates();

			AssertEquals(1, duplicates1.Count());
			AssertEquals(1, duplicates2.Count());
		}

		public override void TestNullCurrentBizoParrameterInGetDuplication()
		{
			GetDataSource();
			var duplicationDataSource = new OrgHeaderDeduplicationDataSourceForTest(new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				new List<DeduplicationOrgHeader>()
				{
					new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				}, new List<DeduplicationPresenterModel>());

			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();

			duplicationDataSource.GetDuplicationWhenCurrentBizOIsNull(populatedTarget);
		}

		public override void TestNullInTargetParameterInGetDuplication()
		{
			var duplicationDataSource = new OrgHeaderDeduplicationDataSourceForTest(new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				new List<DeduplicationOrgHeader>()
				{
					new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				}, new List<DeduplicationPresenterModel>());

			var populatedCurrentBizo = new DeduplicationOrgHeader(MasterBizO());

			var emptyDedupModel = new DeduplicationPresenterModel();
			var emptyDedupList = new List<DeduplicationPresenterModel>();
			emptyDedupList.Add(emptyDedupModel);
			var target = new List<DeduplicationPresenterModel>(emptyDedupList);
			var unpopulatedTargets = target.GroupBy(x => x.TargetID);

			duplicationDataSource.GetDuplicationWhenTargetHasNullObject(populatedCurrentBizo, unpopulatedTargets);
		}

		public override void TestGetPotentialDuplicationModel()
		{
			CreateTargetOrgs();
			var duplicationDataSource = new OrgHeaderDeduplicationDataSourceForTest(new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				new List<DeduplicationOrgHeader>()
				{
					new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				}, new List<DeduplicationPresenterModel>());

			var orgHeader = MasterBizO();
			orgHeader.OH_IsActive = true;
			var populatedCurrentBizo = new DeduplicationOrgHeader(orgHeader);
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();
			var model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals("Yes", model.Active);
			AssertEquals(orgHeader.OH_Code, model.Code);
			AssertEquals(targetModel.Confidence, model.Confidence);
			AssertEquals(orgHeader.OH_FullName, model.Name);
			AssertEquals(orgHeader.OrganisationTypesAsString, model.OrganisationType);
			AssertEquals(orgHeader.DebtorCompany, model.DebtorCompany);
			AssertEquals(orgHeader.CreditorCompany, model.CreditorCompany);
			AssertEquals(targetModel.TargetID, model.PK);
			AssertEquals(targetModel.MainScore, model.Score);
			AssertEquals(orgHeader?.UNLOCO?.RL_Code, model.UNLOCO);
			AssertEquals(populatedTarget, model.DeduplicationPresenterModels);
			AssertEquals(true, model.IsActive);
			AssertEquals(false, model.IsDummy);

			orgHeader.OH_IsActive = false;
			populatedCurrentBizo = new DeduplicationOrgHeader(orgHeader, true);
			model = duplicationDataSource.GetPotentialDuplicationModelForTest(populatedCurrentBizo, populatedTarget);
			AssertEquals("No", model.Active);
			AssertEquals(false, model.IsActive);
			AssertEquals(true, model.IsDummy);
		}

		public override void TestGetDuplicationCandidate()
		{
			CreateTargetOrgs();
			var duplicationDataSource = new OrgHeaderDeduplicationDataSourceForTest(new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				new List<DeduplicationOrgHeader>()
				{
					new DeduplicationOrgHeader(Factory.New<OrgHeader>()),
				}, new List<DeduplicationPresenterModel>());

			var orgHeader = MasterBizO();
			orgHeader.OH_IsActive = true;
			var populatedCurrentBizo = new DeduplicationOrgHeader(orgHeader);
			var populatedTarget = PresenterModels().GroupBy(x => x.TargetID).FirstOrDefault();
			var targetModel = populatedTarget.First();
			var candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);

			AssertEquals("Yes", candidate.Active);
			AssertEquals(orgHeader.OH_Code, candidate.Code);
			AssertEquals(targetModel.Confidence, candidate.Confidence);
			AssertEquals(orgHeader.OH_FullName, candidate.Name);
			AssertEquals(orgHeader.OrganisationTypesAsString, candidate.Type);
			AssertEquals(orgHeader.DebtorCompany, candidate.DebtorCompany);
			AssertEquals(orgHeader.CreditorCompany, candidate.CreditorCompany);
			AssertEquals(targetModel.TargetID, candidate.TargetPK);
			AssertEquals(targetModel.MainScore, candidate.Score);
			AssertEquals(orgHeader?.UNLOCO?.RL_Code, candidate.UNLOCO);
			AssertEquals(populatedTarget, candidate.DeduplicationPresenterModels);
			AssertEquals(true, candidate.IsActive);
			AssertEquals(false, candidate.IsDummy);

			orgHeader.OH_IsActive = false;
			populatedCurrentBizo = new DeduplicationOrgHeader(orgHeader, true);
			candidate = duplicationDataSource.GetDuplicationCandidateForTest(populatedCurrentBizo, populatedTarget);
			AssertEquals("No", candidate.Active);
			AssertEquals(false, candidate.IsActive);
			AssertEquals(true, candidate.IsDummy);
		}

		public void TestLicenceValuesNotSetWhenNotEDI()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var dedupOrgHeader = new DeduplicationOrgHeader(orgHeader);

			var duplicationDataSource = new OrgHeaderDeduplicationDataSourceForTest(dedupOrgHeader,
				new List<DeduplicationOrgHeader>() { dedupOrgHeader },
				new List<DeduplicationPresenterModel>()
			);
			var populatedTarget = new List<DeduplicationPresenterModel> { new DeduplicationPresenterModel() }.GroupBy(x => x.TargetID).FirstOrDefault();
			Factory.Save();

			var model = duplicationDataSource.GetPotentialDuplicationModelForTest(dedupOrgHeader, populatedTarget);
			var candidate = duplicationDataSource.GetDuplicationCandidateForTest(dedupOrgHeader, populatedTarget);

			CombineAssertions(() =>
			{
				AssertEquals(null, model.EnterpriseCode);
				AssertEquals(null, model.CompanyCode);
				AssertEquals(ZString.Empty, model.ProductId);
			});
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, candidate.EnterpriseCode);
				AssertEquals(ZString.Empty, candidate.CompanyCode);
				AssertEquals(ZString.Empty, candidate.ProductId);
			});
		}

		public void TestGetPotentialDuplicatesShouldIncludeInvalidUNLOCO()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			var refUNLOCO1 = Factory.NewWithValidTestData<RefUNLOCO>();
			orgHeader1.OH_RL_NKClosestPort = refUNLOCO1.RL_Code;
			orgHeader2.OH_RL_NKClosestPort = "";
			orgHeader3.OH_RL_NKClosestPort = "";
			orgHeader1.OH_FullName = "testOrg";
			orgHeader2.OH_FullName = "testOrg";
			orgHeader3.OH_FullName = "testOrg 1";
			Factory.Save();

			var targetList = new List<DeduplicationOrgHeader>() { new DeduplicationOrgHeader(orgHeader2), new DeduplicationOrgHeader(orgHeader3, isDummy: true) };
			var candiateHeader1 = new DeduplicationOrgHeader(orgHeader1);
			var candiateHeader2 = new DeduplicationOrgHeader(orgHeader2);
			var candiateHeader3 = new DeduplicationOrgHeader(orgHeader3, isDummy: true);
			var scoringResultList = new List<ScoringResult>() { TargetScorerController.Score(candiateHeader1, candiateHeader2, true),
																TargetScorerController.Score(candiateHeader1, candiateHeader3, true) };
			var presenterModels = new DeduplicationPresenter(candiateHeader1, targetList, scoringResultList).GeneratePresenterModels();
			var potentialDuplicatesOrgList = new OrgHeaderDeduplicationDataSource(candiateHeader1, targetList, presenterModels);

			var duplicates1 = potentialDuplicatesOrgList.GetPotentialDuplicates();
			var duplicates2 = potentialDuplicatesOrgList.GetDuplicationCandidates();

			AssertEquals(1, duplicates1.Count());
			AssertEquals(1, duplicates2.Count());
			AssertEquals(orgHeader2.OH_FullName, duplicates1.First().Name);
			AssertEquals(orgHeader2.OH_FullName, duplicates2.First().Name);
		}

		protected override void AssertResultsOrdered(List<PotentialDuplicationModel> duplicates)
		{
			AssertEquals(5, duplicates.Count);

			AssertEquals(org5.PK, duplicates[0].PK);
			AssertEquals(org3.PK, duplicates[1].PK);
			AssertEquals(org2.PK, duplicates[2].PK);
			AssertEquals(org4.PK, duplicates[3].PK);
			AssertEquals(org6.PK, duplicates[4].PK);
		}

		protected override void AssertResultsOrdered(List<DuplicationCandidate> duplicates)
		{
			AssertEquals(5, duplicates.Count);

			AssertEquals(org5.PK, duplicates[0].TargetPK);
			AssertEquals(org3.PK, duplicates[1].TargetPK);
			AssertEquals(org2.PK, duplicates[2].TargetPK);
			AssertEquals(org4.PK, duplicates[3].TargetPK);
			AssertEquals(org6.PK, duplicates[4].TargetPK);
		}

		protected override OrgHeader MasterBizO()
		{
			org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAABBBCCC";
			org1.MainAddress.OA_Address1 = "Some text";
			org1.MainAddress.OA_City = "Some City";
			org1.OH_FullName = "Test Org";
			org1.OH_RL_NKClosestPort = "AUSYD";

			return org1;
		}

		OrgHeader org1;
		OrgHeader org2;
		OrgHeader org3;
		OrgHeader org4;
		OrgHeader org5;
		OrgHeader org6;

		protected override IDeduplicationDataSource GetDataSource()
		{
			CreateTargetOrgs();
			return new OrgHeaderDeduplicationDataSource(new DeduplicationOrgHeader(MasterBizO()),
				new List<DeduplicationOrgHeader>()
				{
					new DeduplicationOrgHeader(org2),
					new DeduplicationOrgHeader(org3),
					new DeduplicationOrgHeader(org4),
					new DeduplicationOrgHeader(org5),
					new DeduplicationOrgHeader(org6)
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
					ChildGroupNameForType = DeduplicationProvider.Constants.OrganisationNames,
					ChildMasterValue = org1.OH_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = org3.OH_FullName,
					Confidence = ConfidenceRating.Exact,
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 0.9,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					Target = "TESORGSYD1",
					TargetID = org3.PK.ToGuid(),
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
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 0.9,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					Target = "TESORGSYD1",
					TargetID = org3.PK.ToGuid(),
					TargetValue = "TESORGSYD1"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.OrganisationNames,
					ChildMasterValue = org1.OH_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = org5.OH_FullName,
					Confidence = ConfidenceRating.Exact,
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 1,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					Target = "TESORGSYD2",
					TargetID = org5.PK.ToGuid(),
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
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 1,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					Target = "TESORGSYD2",
					TargetID = org5.PK.ToGuid(),
					TargetValue = "TESORGSYD2"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.OrganisationNames,
					ChildMasterValue = org1.OH_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = org2.OH_FullName,
					Confidence = ConfidenceRating.High,
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 0.93,
					Master = "TESORGSYD",
					Target = "TESORGAKL",
					TargetID = org2.PK.ToGuid(),
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
					GroupNameForType = "Organizations",
					MainScore = 0.93,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL",
					TargetID = org2.PK.ToGuid(),
					TargetValue = "TESORGAKL"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.High,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.OrganisationNames,
					ChildMasterValue = org1.OH_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = org4.OH_FullName,
					Confidence = ConfidenceRating.High,
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 0.92,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL1",
					TargetID = org4.PK.ToGuid(),
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
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 0.92,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL1",
					TargetID = org4.PK.ToGuid(),
					TargetValue = "TESORGAKL1"
				},
				new DeduplicationPresenterModel
				{
					ChildComparisonResultScore = 0.9,
					ChildConfidenceRating = ConfidenceRating.None,
					ChildDisplayModeForType = DeduplicationDisplayMode.List,
					ChildDisplayNameForMasterColumns = "Name",
					ChildGroupNameForType = DeduplicationProvider.Constants.OrganisationNames,
					ChildMasterValue = org1.OH_FullName,
					ChildScore = 1,
					ChildScoringResultGroupRating = ConfidenceRating.None,
					ChildTargetID = Guid.Empty,
					ChildTargetValue = org6.OH_FullName,
					Confidence = ConfidenceRating.None,
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 0.11,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL2",
					TargetID = org6.PK.ToGuid(),
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
					GlowTargetType = typeof(IOrgHeader),
					GroupNameForType = "Organizations",
					MainScore = 0.11,
					Master = "TESORGSYD",
					MasterType = typeof(OrgHeader),
					MasterValue = "TESORGSYD",
					Target = "TESORGAKL2",
					TargetID = org6.PK.ToGuid(),
					TargetValue = "TESORGAKL2"
				}
			};
		}

		void CreateTargetOrgs()
		{
			org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "DDDEEEFFF";
			org2.MainAddress.OA_Address1 = "Some text";
			org2.MainAddress.OA_City = "Some City";
			org2.OH_FullName = "Test Org 2";
			org2.OH_RL_NKClosestPort = "NZAKL";

			org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "GGGHHHIII";
			org3.MainAddress.OA_Address1 = "Some text";
			org3.MainAddress.OA_City = "Some City";
			org3.OH_FullName = "Test Org 3";
			org3.OH_RL_NKClosestPort = "";

			org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_Code = "JJJKKKLLL";
			org4.MainAddress.OA_Address1 = "Some text";
			org4.MainAddress.OA_City = "Some City";
			org4.OH_FullName = "Test Org 4";
			org4.OH_RL_NKClosestPort = "ABCD1";

			org5 = Factory.NewWithValidTestData<OrgHeader>();
			org5.OH_Code = "MMMNNNOOO";
			org5.MainAddress.OA_Address1 = "Some text";
			org5.MainAddress.OA_City = "Some City";
			org5.OH_FullName = "Test Org 5";
			org5.OH_RL_NKClosestPort = "AUSYD";

			org6 = Factory.NewWithValidTestData<OrgHeader>();
			org6.OH_Code = "PPPQQQRRR";
			org6.MainAddress.OA_Address1 = "Some text";
			org6.MainAddress.OA_City = "Some City";
			org6.OH_FullName = "Test Org 6";
			org6.OH_RL_NKClosestPort = "NZAKL";

			Factory.Save();
		}
	}

	public class OrgHeaderDeduplicationDataSourceForTest : OrgHeaderDeduplicationDataSource
	{
		public OrgHeaderDeduplicationDataSourceForTest(DeduplicationOrgHeader master, IEnumerable<DeduplicationOrgHeader> targets, IEnumerable<DeduplicationPresenterModel> results)
			: base(master, targets, results)
		{
		}

		public void GetDuplicationWhenCurrentBizOIsNull(IGrouping<Guid, DeduplicationPresenterModel> populatedTarget)
		{
			GetPotentialDuplicationModel(null, populatedTarget);
			GetDuplicationCandidate(null, populatedTarget);
		}

		public void GetDuplicationWhenTargetHasNullObject(IOrgHeader populatedCurrentBizo, IEnumerable<IGrouping<Guid, DeduplicationPresenterModel>> unpopulatedTarget)
		{
			var empty = unpopulatedTarget.FirstOrDefault();
			GetPotentialDuplicationModel(populatedCurrentBizo, empty);
			GetDuplicationCandidate(populatedCurrentBizo, empty);
		}

		public PotentialDuplicationModel GetPotentialDuplicationModelForTest(IOrgHeader currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			return GetPotentialDuplicationModel(currentBizo, target);
		}

		public DuplicationOrganisationCandidate GetDuplicationCandidateForTest(IOrgHeader currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			return GetDuplicationCandidate(currentBizo, target);
		}
	}
}
