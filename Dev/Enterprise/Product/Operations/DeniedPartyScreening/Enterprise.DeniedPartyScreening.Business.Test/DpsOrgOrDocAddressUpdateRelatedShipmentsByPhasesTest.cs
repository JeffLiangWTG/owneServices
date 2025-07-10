using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	class DpsOrgOrDocAddressUpdateRelatedShipmentsByPhasesTest : TestCaseWithFactory
	{
		public void TestUpdateShipmentStatusToBLKWithMatchedPhase()
		{
			AssertUpdateShipmentStatusToBLKOrKeepCurrentStatus("ALL", "BLK");
		}

		public void TestKeepShipmentStatusToNOTWithoutMatchedPhase()
		{
			AssertUpdateShipmentStatusToBLKOrKeepCurrentStatus("AAA", "NOT");
		}

		public void TestUpdateShipmentStatusToRELWithMatchedPhase()
		{
			AssertUpdateShipmentStatusToRELOrKeepCurrentStatus("ALL", "REL");
		}

		public void TestKeepShipmentStatusToCLRWithoutMatchedPhase()
		{
			AssertUpdateShipmentStatusToRELOrKeepCurrentStatus("AAA", "CLR");
		}

		public void TestDoNotUpdateShipmentWhenStatusIsJCLWithMatchedPhase()
		{
			AssertDoNotUpdateShipmentStatusForJCL("ALL");
		}

		public void TestDoNotUpdateShipmentWhenStatusIsJCLWithoutMatchedPhase()
		{
			AssertDoNotUpdateShipmentStatusForJCL("AAA");
		}

		public void TestUpdateShipmentStatusToBLKWithBothClearAndNotClearParties()
		{
			var exportReceivingDepot = Factory.NewWithValidTestData<OrgHeader>();
			var importReleaseDepot = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			exportReceivingDepot.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			importReleaseDepot.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var jobShipment = Factory.New<ForwardingShipment>();
			jobShipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;
			jobShipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;
			jobShipment.JS_Phase = "ALL";
			Factory.Save();

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add("ALL");
			ExecuteUpdateRelatedShipmentsForOrgOrDocAddressByPhasesNew(exportReceivingDepot, GlbCompany.CurrentCompany, phaseTable);

			jobShipment.Reload();
			AssertEquals("JobShipment's end status is BLK", ScreeningStatusesList.Codes.Block, jobShipment.JS_ScreeningStatus);
		}

		int ExecuteUpdateRelatedShipmentsForOrgOrDocAddressByPhasesNew(OrgHeader org, GlbCompany glbCompany, DataTable shipmentPhasesToUpdate)
		{
			var cmd = Db.Connection.Command("UpdateRelatedShipmentsForOrgOrDocAddressByPhasesNew");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, glbCompany.PK.ToGuid());
			cmd.AddTableValuedParameter("@phaseList", "dbo.TVP_Char_3", shipmentPhasesToUpdate);
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());

			return cmd.ExecuteProcedureWithReturnValue();
		}

		#region Implemetation

		void AssertUpdateShipmentStatusToBLKOrKeepCurrentStatus(string phase, string expectedEndStatus)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.OH_ScreeningStatus = "NOT";
			Factory.Save();

			var jobShipmentObject = new OrgRelatedShipmentTestObject(Factory, org, "NOT", expectedEndStatus, "ALL");
			Factory.Save();
			AssertEquals($"JobShipment's starting status is {jobShipmentObject.StartingStatus}", jobShipmentObject.StartingStatus, jobShipmentObject.JobShipment.JS_ScreeningStatus);

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add(phase);

			ExecuteUpdateRelatedShipmentsForOrgOrDocAddressByPhasesNew(org, GlbCompany.CurrentCompany, phaseTable);

			jobShipmentObject.JobShipment.Reload();
			AssertEquals($"JobShipment's end status is {jobShipmentObject.ExpectedEndStatus}", jobShipmentObject.ExpectedEndStatus, jobShipmentObject.JobShipment.JS_ScreeningStatus);
		}

		void AssertUpdateShipmentStatusToRELOrKeepCurrentStatus(string phase, string expectedEndStatus)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.OH_ScreeningStatus = "CLR";
			Factory.Save();

			var jobShipmentObject = new OrgRelatedShipmentTestObject(Factory, org, "CLR", expectedEndStatus, "ALL");
			Factory.Save();
			AssertEquals($"JobShipment's starting status is {jobShipmentObject.StartingStatus}", jobShipmentObject.StartingStatus, jobShipmentObject.JobShipment.JS_ScreeningStatus);

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add(phase);

			ExecuteUpdateRelatedShipmentsForOrgOrDocAddressByPhasesNew(org, GlbCompany.CurrentCompany, phaseTable);

			jobShipmentObject.JobShipment.Reload();
			AssertEquals($"JobShipment's end status is {jobShipmentObject.ExpectedEndStatus}", jobShipmentObject.ExpectedEndStatus, jobShipmentObject.JobShipment.JS_ScreeningStatus);
		}

		void AssertDoNotUpdateShipmentStatusForJCL(string phase)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.OH_ScreeningStatus = "CLR";
			Factory.Save();

			var jobShipmentObject = new OrgRelatedShipmentTestObject(Factory, org, "JCL", "JCL", "ALL");
			Factory.Save();
			AssertEquals($"JobShipemnt's starting status is {jobShipmentObject.StartingStatus}", jobShipmentObject.StartingStatus, jobShipmentObject.JobShipment.JS_ScreeningStatus);

			var phaseTable = new DataTable();
			phaseTable.Columns.Add("Value", typeof(string));
			phaseTable.Rows.Add(phase);

			ExecuteUpdateRelatedShipmentsForOrgOrDocAddressByPhasesNew(org, GlbCompany.CurrentCompany, phaseTable);

			jobShipmentObject.JobShipment.Reload();
			AssertEquals($"JobShipment's end status is {jobShipmentObject.ExpectedEndStatus}", jobShipmentObject.ExpectedEndStatus, jobShipmentObject.JobShipment.JS_ScreeningStatus);
		}

		class OrgRelatedShipmentTestObject
		{
			readonly BusinessObjectFactory factory;
			public string StartingStatus { get; }
			public string ExpectedEndStatus { get; }
			public ForwardingShipment JobShipment { get; }

			public OrgRelatedShipmentTestObject(BusinessObjectFactory factory, OrgHeader org, string startingStatus, string expectedEndStatus, string phase)
			{
				this.factory = factory;
				StartingStatus = startingStatus;
				ExpectedEndStatus = expectedEndStatus;

				if (JobShipment == null)
				{
					JobShipment = this.factory.New<ForwardingShipment>();
					JobShipment.JS_ScreeningStatus = StartingStatus;
					JobShipment.JS_OA_ExportReceivingDepot = org.MainAddress.PK;
					JobShipment.JS_Phase = phase;
				}
			}
		}

		#endregion
	}
}
