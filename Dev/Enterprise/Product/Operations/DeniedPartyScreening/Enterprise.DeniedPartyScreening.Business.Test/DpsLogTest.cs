using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class DpsLogTest : TestCaseWithFactory
	{
		public void TestAddNewWithNullFactoryShouldThrowException()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);
			AssertExceptionThrown<ArgumentNullException>(() => DpsLog.AddNew(null, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, new DeniedPartyResultItemV4ForTest(header, null), null, false, "Test User"));
		}

		public void TestAddNewShouldNotSaveLogsIntoDB()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, new DeniedPartyResultItemV4ForTest(header, null), sourceBizOs, false, "Test User");

			AssertEquals(1, statusCollection.Count);
			AssertEquals(false, statusCollection[0].IsInDatabase);
		}

		public void TestAddNewWithDefaultValues()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Wisetech Company";

			DpsLog.AddNewWithDefaultValues(Factory, header, DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed, "Override Test Status");

			Factory.Save();

			var dpsLog = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).Single();

			CombineAssertions(() =>
			{
				AssertEquals(dpsLog.GetType(), typeof(StmEntityScreeningLog));
				AssertEquals(header.PK, dpsLog.PJ_ParentID);
				AssertEquals(header.TableCode, dpsLog.PJ_ParentTableCode);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed, dpsLog.PJ_Status);
				AssertEquals("Override Test Status", dpsLog.PJ_ClearedReason);
				AssertEquals(header.PK, dpsLog.PJ_SourceID);
				AssertEquals(header.TableCode, dpsLog.PJ_SourceTableCode);
			});
		}

		public void TestAddNewWithDefaultValuesNullCheck()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Wisetech Company";
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => DpsLog.AddNewWithDefaultValues(null, header, DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed, "Override Test Status"));
				AssertExceptionThrown<ArgumentNullException>(() => DpsLog.AddNewWithDefaultValues(Factory, null, DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed, "Override Test Status"));
				AssertExceptionThrown<ArgumentNullException>(() => DpsLog.AddNewWithDefaultValues(Factory, header, null, "Override Test Status"));
				AssertExceptionThrown<ArgumentNullException>(() => DpsLog.AddNewWithDefaultValues(Factory, header, DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed, null));
			});
		}

		public void TestCreateLogsWithCredentialOverride_Org_Matched()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, header, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, "", "ABC", "EFG", 0, false, "Test User", sourceBizOs);
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Org was set to Matched by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertContains(@"High Confidence Result:
ABC
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Medium Confidence Result:
EFG", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithCredentialOverride_Org_Clear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, header, DeniedPartyConstants.LogsScreeningStatus.ScreenedClear, "", "ABC", "EFG", 0, false, "Test User", sourceBizOs);
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Org was set to Clear by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
				AssertNotContains("High Confidence Result:", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithCredentialOverride_Org_PermanentClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, header, DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear, "", "ABC", "EFG", 0, false, "Test User", sourceBizOs);
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains("The Screening Status of Test Org was set to Permanent Clear by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertContains("The user confirmed that they understood the impact of this by entering 'I understand the Impact'.\r\n\r\nThis record will remain clear unless manually changed.", statusCollection[0].PJ_MatchingData);
				AssertNotContains("High Confidence Result:", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithCredentialOverride_Vessel_Matched()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			var statusCollection = new StmEntityScreeningLogCollection(vessel);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(vessel, new[] { new ScreeningParty(vessel, vessel.HumanReadableName, vessel) })
			};

			DpsLog.AddNew(Factory, vessel, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, "", "ABC", "EFG", 0, false, "Test User", sourceBizOs);
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Vessel was set to Matched by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertContains(@"High Confidence Result:
ABC
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Medium Confidence Result:
EFG", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithCredentialOverride_Vessel_Clear()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			var statusCollection = new StmEntityScreeningLogCollection(vessel);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(vessel, new[] { new ScreeningParty(vessel, vessel.HumanReadableName, vessel) })
			};

			DpsLog.AddNew(Factory, vessel, DeniedPartyConstants.LogsScreeningStatus.ScreenedClear, "", "ABC", "EFG", 0, false, "Test User", sourceBizOs);
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Vessel was set to Clear by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
				AssertNotContains("High Confidence Result:", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithSaveWithCredentialOverride_Org_Matched()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, new DeniedPartyResultItemV4ForTest(header, null), sourceBizOs, false, "Test User");
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Org was set to Matched by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertContains(@"High Confidence Result:
ABC
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Medium Confidence Result:
EFG", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithSaveWithCredentialOverride_Org_Clear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.ScreenedClear, new DeniedPartyResultItemV4ForTest(header, null), sourceBizOs, false, "Test User");
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Org was set to Clear by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
				AssertNotContains("High Confidence Result:", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithSaveWithCredentialOverride_Org_PermanentClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Org";
			var statusCollection = new StmEntityScreeningLogCollection(header);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, new[] { new ScreeningParty(header, header.HumanReadableName, header) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear, new DeniedPartyResultItemV4ForTest(header, null), sourceBizOs, false, "Test User");
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains("The Screening Status of Test Org was set to Permanent Clear by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertContains("The user confirmed that they understood the impact of this by entering 'I understand the Impact'.\r\n\r\nThis record will remain clear unless manually changed.", statusCollection[0].PJ_MatchingData);
				AssertNotContains("High Confidence Result:", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithSaveWithCredentialOverride_Vessel_Matched()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			var statusCollection = new StmEntityScreeningLogCollection(vessel);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(vessel, new[] { new ScreeningParty(vessel, vessel.HumanReadableName, vessel) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, new DeniedPartyResultItemV4ForTest(vessel, null), sourceBizOs, false, "Test User");
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Vessel was set to Matched by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertContains(@"High Confidence Result:
ABC
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Medium Confidence Result:
EFG", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCreateLogsWithSaveWithCredentialOverride_Vessel_Clear()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			var statusCollection = new StmEntityScreeningLogCollection(vessel);
			Factory.Save();

			AssertEquals("Precondition: ", 0, statusCollection.Count);

			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(vessel, new[] { new ScreeningParty(vessel, vessel.HumanReadableName, vessel) })
			};

			DpsLog.AddNew(Factory, DeniedPartyConstants.LogsScreeningStatus.ScreenedClear, new DeniedPartyResultItemV4ForTest(vessel, null), sourceBizOs, false, "Test User");
			CombineAssertions(() =>
			{
				AssertEquals(1, statusCollection.Count);

				AssertContains(@"The Screening Status of Test Vessel was set to Clear by user CargoWise Support using override credentials Test User on the", statusCollection[0].PJ_MatchingData);
				AssertNotContains("The user confirmed that they understood", statusCollection[0].PJ_MatchingData);
				AssertNotContains("High Confidence Result:", statusCollection[0].PJ_MatchingData);
			});
		}

		public void TestCountrySanctionsLogStatus()
		{
			AssertEquals("DMS", DpsLog.GetCountrySanctionsLogStatus(ScreeningStatusesList.Codes.Matched));
			AssertEquals("DPC", DpsLog.GetCountrySanctionsLogStatus(ScreeningStatusesList.Codes.Clear));
			AssertEquals("DPD", DpsLog.GetCountrySanctionsLogStatus(ScreeningStatusesList.Codes.Canceled));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceList");
		}
	}
}
