using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeSupplyTypeOverride))]
	sealed class AccChargeSupplyTypeOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoAuditLogsWithAttibute()
		{
			var supplyTypeOverride = GetNewSupplyTypeOverrideWithAllPropertiesPreset();
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, supplyTypeOverride.ACS_ParentID);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				supplyTypeOverride.ACS_Direction = Core.Constants.FreightShipmentDirection.Code.Export;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				supplyTypeOverride.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestIncotermWithFCNJobType_ReadOnly()
		{
			var supplyTypeOverride = GetNewSupplyTypeOverrideWithAllPropertiesPreset();
			supplyTypeOverride.ACS_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			supplyTypeOverride.ACS_IncoTerm = "DDP";
			AssertEquals("Precondition: Readonly false before FCN Job type", false, supplyTypeOverride.ACS_IncoTermInfo.ReadOnly);

			supplyTypeOverride.ACS_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			Assert(supplyTypeOverride.ACS_IncoTermInfo.ReadOnly);
			AssertEquals(AccountingMasterFilesConstants.INCOTermCodes.All, supplyTypeOverride.ACS_IncoTerm);
		}

		AccChargeSupplyTypeOverride GetNewSupplyTypeOverrideWithAllPropertiesPreset()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var supplyTypeOverride = chargeCode.SupplyTypeOverrides.AddNew();

			supplyTypeOverride.ACS_JobType = "SHP";
			supplyTypeOverride.ACS_Direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
			supplyTypeOverride.ACS_TransportMode = "AIR";
			supplyTypeOverride.ACS_IncoTerm = "DAT";
			var newDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			newDepartment.GE_Code = "DE1";
			supplyTypeOverride.ACS_GE = newDepartment.PK;
			supplyTypeOverride.ACS_SupplyType = "LOA";
			return supplyTypeOverride;
		}
	}
}
