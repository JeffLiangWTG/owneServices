using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DpsScreeningStatusResynchronizerTest : TestCaseWithFactory
	{
		public void TestResynchronizeScreeningStatus()
		{
			var consol = Factory.New<IForwardingConsol>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Address1 = "Doc Address 1";
			jobDocAddress.E2_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobDocAddress.E2_ParentID = consol.PK;
			Factory.Save();

			jobDocAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);
			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.Clear, jobDocAddress.E2_ScreeningStatus);

			DpsScreeningStatusResynchronizer.SaveWithVerbose(true, (BusinessObject)consol);

			AssertEquals("Synchronize successful", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
			AssertEquals("Saved to database", false, ((BusinessObject)consol).HasChanges);
		}

		public void TestResynchronizeScreeningStatus_ConcurrencyException_VerboseMode()
		{
			var consol = Factory.New<IForwardingConsol>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Address1 = "Doc Address 1";
			jobDocAddress.E2_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobDocAddress.E2_ParentID = consol.PK;
			Factory.Save();

			jobDocAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);
			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.Clear, jobDocAddress.E2_ScreeningStatus);

			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobConsol
SET
	JK_ScreeningStatus = 'MAT',
	JK_SystemLastEditTimeUtc = GETUTCDATE(),
	JK_SystemLastEditUser = '~BP'
WHERE
	JK_PK = '{0}'", consol.PK));

			DpsScreeningStatusResynchronizer.SaveWithVerbose(true, (BusinessObject)consol);

			AssertEquals("Synchronize successful", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
			AssertEquals("Saved to database", false, ((BusinessObject)consol).HasChanges);
		}

		public void TestResynchronizeScreeningStatus_ConcurrencyException_NonVerboseMode()
		{
			var consol = Factory.New<IForwardingConsol>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Address1 = "Doc Address 1";
			jobDocAddress.E2_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobDocAddress.E2_ParentID = consol.PK;
			Factory.Save();

			jobDocAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.NotScreened, consol.JK_ScreeningStatus);
			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.Clear, jobDocAddress.E2_ScreeningStatus);

			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobConsol
SET
	JK_ScreeningStatus = 'MAT',
	JK_SystemLastEditTimeUtc = GETUTCDATE(),
	JK_SystemLastEditUser = '~BP'
WHERE
	JK_PK = '{0}'", consol.PK));

			AssertNoExceptionThrown(() => DpsScreeningStatusResynchronizer.SaveWithVerbose(false, (BusinessObject)consol));

			AssertEquals(ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
			AssertEquals("Not saved to database", true, ((BusinessObject)consol).HasChanges);
		}

		public void TestResynchronizeScreeningStatusForShipment_ScreeningStatusUpdater()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Address1 = "Doc Address 1";
			jobDocAddress.E2_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobDocAddress.E2_ParentID = shipment.PK;
			Factory.Save();

			jobDocAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			AssertEquals("Precondition - Lost Synchronize", ScreeningStatusesList.Codes.Clear, jobDocAddress.E2_ScreeningStatus);

			DpsScreeningStatusResynchronizer.SaveWithVerbose(true, (BusinessObject)shipment);
			var latestLog = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode))
				.OrderByDescending(o => o.SL_EventTime).FirstOrDefault();

			AssertEquals("Synchronize successful", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
			AssertContains("|NEW=CLR|OLD=NOT|TYP=MAN", latestLog.SL_Reference);
			AssertEquals("New Status: Clear, Old Status: Not Screened, Type: Manual Screen", latestLog.DisplayEventReference);
			AssertEquals("Saved to database", false, ((BusinessObject)shipment).HasChanges);
		}

		public void TestUpdateShipmentWithDeclarations_IgnoreConcurrencyExceptionIfNonVerboseMode()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorPK = consignor.PK;

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = false;
			Factory.Save();

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobDeclaration
SET
	JE_ScreeningStatus = 'UNK',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{0}'", declaration.PK));

			AssertNoExceptionThrown(() => DpsScreeningStatusResynchronizer.SynchronizeScreeningStatusToDeclarationsAndSave(false, shipment));

			CombineAssertions("Postconditions", () =>
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Shipment unchanged", ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
				AssertEquals("Declaration synchronized successfully", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
				AssertEquals("not saved to DB", true, declaration.HasChanges);
			});
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}
	}
}
