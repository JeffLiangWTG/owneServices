using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DeduplicationOrganisation))]
	sealed class DeduplicationOrganisationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoTimeOut()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgMaster = Factory.NewWithValidTestData<OrgHeader>();
				var orgTarget = Factory.NewWithValidTestData<OrgHeader>();
				((IDeduplicatable)orgMaster).ShouldRunDeduplication = true;

				Factory.Save();

				var dedupOrg = Factory.Load<DeduplicationOrganisationForTest>(orgMaster.PK);
				var orgDuplicationFinder_ThatAlwaysTimesOut = new OrgHeaderDuplicationFinder_ThatTimesOutForTest();
				Factory.Save();

				dedupOrg.DuplicationFinderForTest = orgDuplicationFinder_ThatAlwaysTimesOut;

				dedupOrg.DuplicateSearchingFinished += (_, e) =>
				{
					AssertEquals(DuplicationStatus.OK, ((ISupportDuplicationFinder)orgDuplicationFinder_ThatAlwaysTimesOut).LastRunStatus);
				};
				dedupOrg.FindPotentialDuplicates();
			}
		}

		public void TestCreatedUnderBranchFetchForViewHintWorks()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TSTORG1";
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTORG2";
			factory.Save();
			var dedupOrg1 = factory.Load<DeduplicationOrganisation>(org1.PK);
			var dedupOrg2 = factory.Load<DeduplicationOrganisation>(org2.PK);

			AssertType<DeduplicationOrganisationFetchStrategy>(dedupOrg1.FetchStrategy);

			factory.ResetDatabaseLoadCount();

			dedupOrg1.FetchStrategy.FetchForView(new[] { new TableColumn("", "DOH_CreatedUnderBranch"), new TableColumn("", "DOH_CreatedUnderCompany") });
			dedupOrg2.FetchStrategy.FetchForView(new[] { new TableColumn("", "DOH_CreatedUnderBranch"), new TableColumn("", "DOH_CreatedUnderCompany") });

			var branch = dedupOrg1.DOH_CreatedUnderBranch;
			var company = dedupOrg1.DOH_CreatedUnderCompany;
			branch = dedupOrg2.DOH_CreatedUnderBranch;
			company = dedupOrg2.DOH_CreatedUnderCompany;

			var dbHits = new Dictionary<string, int>();
			dbHits["StmALog"] = 1;
			dbHits["GlbBranch"] = 1;
			dbHits["GlbCompany"] = 1;

			AssertDbHits(dbHits, factory);
		}

		public void TestCreatedUnderProperties()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "TMC";
			company.Branches.AddNew().GB_Code = "TMB";
			Factory.Save();
			using (DisposableEnvironment.ForBranch("TMB"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var dedupOrg = Factory.Load<DeduplicationOrganisation>(org.PK);
				AssertEquals("TMB", dedupOrg.DOH_CreatedUnderBranch);
				AssertEquals("TMC", dedupOrg.DOH_CreatedUnderCompany);
			}
		}

		public void TestAssociatedConsolidationCountIsCorrect()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateConsolidationForOrg(org1);
			CreateConsolidationForOrg(org1);
			CreateConsolidationForOrg(org1);

			CreateConsolidationForOrg(org2);
			CreateConsolidationForOrg(org2);

			Factory.Save();

			var dedupOrg1 = Factory.Load<DeduplicationOrganisation>(org1.PK);
			var dedupOrg2 = Factory.Load<DeduplicationOrganisation>(org2.PK);
			var dedupOrg3 = Factory.Load<DeduplicationOrganisation>(org3.PK);

			CombineAssertions(() =>
			{
				AssertEquals(3, dedupOrg1.DOH_ConsolidationsCount);
				AssertEquals(2, dedupOrg2.DOH_ConsolidationsCount);
				AssertEquals(0, dedupOrg3.DOH_ConsolidationsCount);
			});
		}

		void CreateConsolidationForOrg(OrgHeader org)
		{
			var jobConsolidation = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			jobConsolidation[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;
		}

		public void TestAssociatedDeclarationCountIsCorrect()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateDeclarationForOrg(org1);
			CreateDeclarationForOrg(org1);
			CreateDeclarationForOrg(org1);

			CreateDeclarationForOrg(org2);
			CreateDeclarationForOrg(org2);

			Factory.Save();

			var dedupOrg1 = Factory.Load<DeduplicationOrganisation>(org1.PK);
			var dedupOrg2 = Factory.Load<DeduplicationOrganisation>(org2.PK);
			var dedupOrg3 = Factory.Load<DeduplicationOrganisation>(org3.PK);

			CombineAssertions(() =>
			{
				AssertEquals(3, dedupOrg1.DOH_DeclarationsCount);
				AssertEquals(2, dedupOrg2.DOH_DeclarationsCount);
				AssertEquals(0, dedupOrg3.DOH_DeclarationsCount);
			});
		}

		void CreateDeclarationForOrg(OrgHeader org)
		{
			var jobConsolidation = Factory.NewWithValidTestData(ObjectFactory.GetType<IBaseJobDeclaration>());
			jobConsolidation[JobDeclarationSchema.JE_OH_Supplier] = org.PK;
			jobConsolidation[JobDeclarationSchema.JE_OH_Importer] = org.PK;
			jobConsolidation[JobDeclarationSchema.JE_OH_ShippingLine] = org.PK;
			jobConsolidation[JobDeclarationSchema.JE_OH_Forwarder] = org.PK;
		}

		public void TestAssociatedShipmentCountIsCorrect()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateShipmentForOrg(org1);
			CreateShipmentForOrg(org1);
			CreateShipmentForOrg(org1);

			CreateShipmentForOrg(org2);
			CreateShipmentForOrg(org2);

			Factory.Save();

			var dedupOrg1 = Factory.Load<DeduplicationOrganisation>(org1.PK);
			var dedupOrg2 = Factory.Load<DeduplicationOrganisation>(org2.PK);
			var dedupOrg3 = Factory.Load<DeduplicationOrganisation>(org3.PK);

			CombineAssertions(() =>
			{
				AssertEquals(3, dedupOrg1.DOH_ShipmentsCount);
				AssertEquals(2, dedupOrg2.DOH_ShipmentsCount);
				AssertEquals(0, dedupOrg3.DOH_ShipmentsCount);
			});
		}

		void CreateShipmentForOrg(OrgHeader org)
		{
			var jobShipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			jobShipment[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;
			jobShipment[JobShipmentSchema.JS_OH_ImportBroker] = org.PK;
		}

		public void TestEXCStatusForDirtyOrgRecord()
		{
			var org = Factory.NewWithPrimaryKey<OrgHeader>(DeDupOrg.PK.ToGuid());
			org.OH_FullName = "WiseTester Global";
			org.MainAddress.OA_Phone = "+61 401 132 442";
			org.OH_Code = "TESTORG";
			for (int i = 0; i < 100; i++)
			{
				org.Contacts.AddNew().OC_ContactName = "Smith_" + i;
			}

			Factory.Save();

			AssertNotEquals(DeduplicationHelper.StatusConstants.Excluded, DeDupOrg.DOH_Status);

			DeDupOrg.DuplicateSearchingFinished += (_, e) =>
			{
				DeDupOrg.SaveDeduplicationResults(e.ScoringResults);
				AssertEquals(DeduplicationHelper.StatusConstants.Excluded, DeDupOrg.DOH_Status);
			};

			DeDupOrg.FindPotentialDuplicates();
		}

		public void TestMultipleCallToSavePatternMatchingResults()
		{
			Factory.NewWithPrimaryKey<OrgHeader>(DeDupOrg.PK.ToGuid()).OH_Code = "TESTORG";
			AssertNoExceptionThrown(() => DeDupOrg.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));
			AssertNoExceptionThrown(() => DeDupOrg.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));
		}

		public void TestSavingPatternMatchingResultsWithEmptyTargetPK()
		{
			Factory.NewWithPrimaryKey<OrgHeader>(DeDupOrg.PK.ToGuid()).OH_Code = "TESTORG";
			var scoringResult = new ScoringResult();
			scoringResult.TargetPK = Guid.Empty;
			scoringResult.Score = .123;
			var scoringResults = new List<ScoringResult>() { scoringResult };
			AssertNoExceptionThrown(() => DeDupOrg.SaveDeduplicationResults(scoringResults));

			AssertSaveException();
		}

		public void TestSavePatternMatchingResultWithEXCStatus()
		{
			Factory.NewWithPrimaryKey<OrgHeader>(DeDupOrg.PK.ToGuid()).OH_Code = "TESTORG";
			var pmt = Factory.New<PatternMatchingResult>();
			pmt.PMT_MasterPK = DeDupOrg.PK;
			pmt.PMT_MasterTableCode = "OH";
			pmt.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
			pmt.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			pmt.PMT_ScorePercent = 0;
			Factory.Save();
			AssertNoExceptionThrown(() => DeDupOrg.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));

			AssertEquals(DeduplicationHelper.StatusConstants.Excluded, DeDupOrg.DOH_Status);

			AssertSaveException();
		}

		public void TestSavePatternMatchingResultWithERRStatus()
		{
			Factory.NewWithPrimaryKey<OrgHeader>(DeDupOrg.PK.ToGuid()).OH_Code = "TESTORG";
			var pmt = Factory.New<PatternMatchingResult>();
			pmt.PMT_MasterPK = DeDupOrg.PK;
			pmt.PMT_MasterTableCode = "OH";
			pmt.PMT_Status = PatternMatchingResult.StatusCodes.Error;
			pmt.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			pmt.PMT_ScorePercent = 0;
			Factory.Save();
			AssertNoExceptionThrown(() => DeDupOrg.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));

			AssertEquals(DeduplicationHelper.StatusConstants.Error, DeDupOrg.DOH_Status);

			AssertSaveException();
		}

		public void TestPatternMatchingResultWithLock_DoesNotGetProcessed()
		{
			Factory.NewWithPrimaryKey<OrgHeader>(DeDupOrg.PK.ToGuid()).OH_Code = "TESTORG";
			Factory.Save();

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection.TryGetLock((PatternMatchingConstants.AppLockKey + "," + DeDupOrg.PK.ToString()).ToUpperInvariant(), out var appLock));

				using (appLock)
				{
					// Act
					DeDupOrg.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>());

					// Assert
					AssertEquals("TESTORG PK Does not exist in PatternMatchingResult Table", false, Factory.Exists(typeof(PatternMatchingResult), new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, DeDupOrg.PK)));
				}
			}
		}

		void AssertSaveException()
		{
			AssertType<ZSaveException>(ErrorReporter.LastExceptionReported);
			AssertEquals("DeduplicationHelper|SaveDeduplicationResults|ZSaveException", ErrorReporter.LastKeyReported);
			AssertEquals("Failed to save PatternMatchingResults", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCollectionDoesNotContainNDUResults()
		{
			Factory.NewWithPrimaryKey<OrgHeader>(DeDupOrg.PK.ToGuid()).OH_Code = "TESTORG";
			DeduplicationOrganisationForTest.DuplicationScoringResultForTest = new List<ScoringResult>();
			DeDupOrg.DuplicateSearchingFinished += (_, e) =>
			{
				DeDupOrg.SaveDeduplicationResults(e.ScoringResults);
				Assert(Factory.Exists(typeof(PatternMatchingResult), new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, DeDupOrg.PK).AddToFilter(PatternMatchingResultSchema.PMT_Status, "NDU")));
				AssertEquals(0, DeDupOrg.MatchingResultCollection.Count);
			};
			DeDupOrg.FindPotentialDuplicates();
		}

		public void TestFindPotentialDuplicates()
		{
			var orgMaster = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var dedupOrg = Factory.Load<DeduplicationOrganisationForTest>(orgMaster.PK);
			DeduplicationOrganisationForTest.DuplicationScoringResultForTest = new List<ScoringResult>() { new ScoringResult()
			{
				MasterPK = orgMaster.PK.ToGuid(),
				MasterType = typeof(OrgHeader),
				Score = 1,
				TargetPK = orgTarget.PK.ToGuid(),
				TargetType = typeof(OrgHeader),
			} };
			dedupOrg.DuplicateSearchingFinished += (_, e) =>
			{
				dedupOrg.SaveDeduplicationResults(e.ScoringResults);
				Assert("PDU result saved in DB", Factory.Exists(typeof(PatternMatchingResult), new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, dedupOrg.PK).AddToFilter(PatternMatchingResultSchema.PMT_Status, "PDU")));
				AssertEquals("Collection has 1 PatternMatchingResult", 1, dedupOrg.MatchingResultCollection.Count);
				var result = dedupOrg.MatchingResultCollection[0];
				AssertEquals(orgMaster.PK, result.PMT_MasterPK);
				AssertEquals(orgTarget.PK, result.PMT_TargetPK);
				AssertEquals((ZByte)100, result.PMT_ScorePercent);
				AssertEquals("PDU", result.PMT_Status);
				AssertEquals("OH", result.PMT_TargetTableCode);
				AssertEquals("OH", result.PMT_MasterTableCode);
			};
			dedupOrg.FindPotentialDuplicates();
		}

		public void TestMatchConfidenceResults()
		{
			var orgMaster = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget3 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget4 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget5 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget6 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var result1 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget1, 70);
			var result2 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget2, 85);
			var result3 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget3, 80);
			var result4 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget4, 99);
			var result5 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget5, 50);
			var result6 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget6, 40);

			Factory.Save();

			var dedupOrg = Factory.Load<DeduplicationOrganisationForTest>(orgMaster.PK);
			CombineAssertions(() =>
			{
				AssertEquals("High results (>=80%)", 3, dedupOrg.DOH_HighDuplicates);
				AssertEquals("Medium results (>=50%)", 2, dedupOrg.DOH_MediumDuplicates);
				AssertEquals("Low results (<50%)", 1, dedupOrg.DOH_LowDuplicates);
			});
		}

		public void TestMatchMaxResult()
		{
			var orgMaster = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget3 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget4 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget5 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget6 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var result1 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget1, 70);
			var result2 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget2, 85);
			var result3 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget3, 80);
			var result4 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget4, 99);
			var result5 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget5, 50);
			var result6 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget6, 40);

			Factory.Save();

			var dedupOrg = Factory.Load<DeduplicationOrganisationForTest>(orgMaster.PK);
			AssertEquals(99, dedupOrg.DOH_MaxResult);
		}

		static PatternMatchingResult CreateOrgPatternMatchingResult(BusinessObjectFactory factory, OrgHeader master, OrgHeader target, ZByte scorePercent)
		{
			var patternMatchingResult = factory.New<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterPK = master.PK;
			patternMatchingResult.PMT_ScorePercent = scorePercent;
			patternMatchingResult.PMT_TargetPK = target.PK;
			patternMatchingResult.PMT_FoundTimeUtc = DateTime.Now;
			patternMatchingResult.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
			patternMatchingResult.PMT_TargetTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingResult.PMT_MasterTableCode = OrgHeaderSchema.Constants.Prefix;
			return patternMatchingResult;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This is a view, doesn't need to save", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeDupOrg = Factory.New<DeduplicationOrganisationForTest>();
			DeDupOrg.DOH_Status = DeduplicationHelper.StatusConstants.ToBeProcessed;
			((INeedRow)DeDupOrg).Row.AcceptChanges();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return DeDupOrg;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return DeDupOrg;
		}

		protected override void TearDown()
		{
			DeduplicationOrganisationForTest.DuplicationScoringResultForTest = null;
			base.TearDown();
		}

		public void TestMatchingResultCollection()
		{
			var orgMaster = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgTarget2 = Factory.NewWithValidTestData<OrgHeader>();

			var result1 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget1, 85);
			var result2 = CreateOrgPatternMatchingResult(Factory, orgMaster, orgTarget2, 85);

			result1.PMT_MasterPK = result2.PMT_MasterPK = DeDupOrg.PK;

			Factory.Save();
			AssertCollectionContains("Contains result1", result1, DeDupOrg.MatchingResultCollection);
			AssertCollectionContains("Contains result2", result2, DeDupOrg.MatchingResultCollection);
		}

		DeduplicationOrganisationForTest DeDupOrg;
	}
}
