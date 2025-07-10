using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	class DpsOrgOrDocAddressUpdateRelatedConsolsByPhasesTest : TestCaseWithFactory
	{
		public void TestUpdateConsolStatusToBLKWithMatchedPhase()
		{
			AssertUpdateConsolStatusToBLKOrKeepCurrentStatus("ALL", "BLK");
		}

		public void TestKeepConsolStatusToNOTWithoutMatchedPhase()
		{
			AssertUpdateConsolStatusToBLKOrKeepCurrentStatus("AAA", "NOT");
		}

		public void TestUpdateConsolStatusToRELWithMatchedPhase()
		{
			AssertUpdateConsolStatusToRELOrKeepCurrentStatus("ALL", "REL");
		}

		public void TestKeepConsolStatusToCLRWithoutMatchedPhase()
		{
			AssertUpdateConsolStatusToRELOrKeepCurrentStatus("AAA", "CLR");
		}

		public void TestDoNotUpdateConsolWhenStatusIsJCLWithMatchedPhase()
		{
			AssertDoNotUpdateConsolStatusForJCL("ALL");
		}

		public void TestDoNotUpdateConsolWhenStatusIsJCLWithoutMatchedPhase()
		{
			AssertDoNotUpdateConsolStatusForJCL("AAA");
		}

		public void TestUpdateConsolStatusToBLKWithBothClearAndNotClearParties()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			creditor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			departureCTO.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var jobConsol = Factory.New<IForwardingConsol>();
			jobConsol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			jobConsol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;
			jobConsol.JK_Phase = "ALL";
			Factory.Save();

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add("ALL");
			ExecuteUpdateRelatedConsolsForOrgOrDocAddressByPhasesNew(creditor, GlbCompany.CurrentCompany, phaseTable);

			((BusinessObject)jobConsol).Reload();
			AssertEquals("JobConsol's end status is BLK", ScreeningStatusesList.Codes.Block, jobConsol.JK_ScreeningStatus);
		}

		#region Implemetation

		void AssertUpdateConsolStatusToBLKOrKeepCurrentStatus(string phase, string expectedEndStatus)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.OH_ScreeningStatus = "NOT";
			Factory.Save();

			var jobConsolObject = new OrgRelatedConsolTestObject(Factory, org, "NOT", expectedEndStatus, "ALL");
			Factory.Save();
			AssertEquals($"JobConsol's starting status is {jobConsolObject.StartingStatus}", jobConsolObject.StartingStatus, jobConsolObject.JobConsol.JK_ScreeningStatus);

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add(phase);

			ExecuteUpdateRelatedConsolsForOrgOrDocAddressByPhasesNew(org, GlbCompany.CurrentCompany, phaseTable);

			((BusinessObject)jobConsolObject.JobConsol).Reload();
			AssertEquals($"JobConsol's end status is {jobConsolObject.ExpectedEndStatus}", jobConsolObject.ExpectedEndStatus, jobConsolObject.JobConsol.JK_ScreeningStatus);
		}

		void AssertUpdateConsolStatusToRELOrKeepCurrentStatus(string phase, string expectedEndStatus)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.OH_ScreeningStatus = "CLR";
			Factory.Save();

			var jobConsolObject = new OrgRelatedConsolTestObject(Factory, org, "CLR", expectedEndStatus, "ALL");
			Factory.Save();
			AssertEquals($"JobConsol's starting status is {jobConsolObject.StartingStatus}", jobConsolObject.StartingStatus, jobConsolObject.JobConsol.JK_ScreeningStatus);

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add(phase);

			ExecuteUpdateRelatedConsolsForOrgOrDocAddressByPhasesNew(org, GlbCompany.CurrentCompany, phaseTable);

			((BusinessObject)jobConsolObject.JobConsol).Reload();
			AssertEquals($"JobConsol's end status is {jobConsolObject.ExpectedEndStatus}", jobConsolObject.ExpectedEndStatus, jobConsolObject.JobConsol.JK_ScreeningStatus);
		}

		void AssertDoNotUpdateConsolStatusForJCL(string phase)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.OH_ScreeningStatus = "CLR";
			Factory.Save();

			var jobConsolObject = new OrgRelatedConsolTestObject(Factory, org, "JCL", "JCL", "ALL");
			Factory.Save();
			AssertEquals($"JobConsol's starting status is {jobConsolObject.StartingStatus}", jobConsolObject.StartingStatus, jobConsolObject.JobConsol.JK_ScreeningStatus);

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add(phase);

			ExecuteUpdateRelatedConsolsForOrgOrDocAddressByPhasesNew(org, GlbCompany.CurrentCompany, phaseTable);

			((BusinessObject)jobConsolObject.JobConsol).Reload();
			AssertEquals($"JobConsol's end status is {jobConsolObject.ExpectedEndStatus}", jobConsolObject.ExpectedEndStatus, jobConsolObject.JobConsol.JK_ScreeningStatus);
		}

		int ExecuteUpdateRelatedConsolsForOrgOrDocAddressByPhasesNew(OrgHeader org, GlbCompany glbCompany, DataTable consolPhasesToUpdate)
		{
			var cmd = Db.Connection.Command("UpdateRelatedConsolsForOrgOrDocAddressByPhasesNew");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, glbCompany.PK.ToGuid());
			cmd.AddTableValuedParameter("@phaseList", "dbo.TVP_Char_3", consolPhasesToUpdate);
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());

			return cmd.ExecuteProcedureWithReturnValue();
		}

		class OrgRelatedConsolTestObject
		{
			readonly BusinessObjectFactory factory;
			public string StartingStatus { get; }
			public string ExpectedEndStatus { get; }
			public IForwardingConsol JobConsol { get; set; }

			public OrgRelatedConsolTestObject(BusinessObjectFactory factory, OrgHeader org, string startingStatus, string expectedEndStatus, string phase)
			{
				this.factory = factory;
				StartingStatus = startingStatus;
				ExpectedEndStatus = expectedEndStatus;

				if (JobConsol == null)
				{
					JobConsol = this.factory.New<IForwardingConsol>();
					JobConsol.JK_ScreeningStatus = StartingStatus;
					JobConsol.JK_OA_CreditorAddress = org.MainAddress.PK;
					JobConsol.JK_Phase = phase;
				}
			}
		}

		#endregion
	}
}
