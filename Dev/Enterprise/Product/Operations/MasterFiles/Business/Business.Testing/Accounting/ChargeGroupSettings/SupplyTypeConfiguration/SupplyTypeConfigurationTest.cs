using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SupplyTypeConfiguration))]
	sealed class SupplyTypeConfigurationTest : ChargeGroupSettingTest
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new SupplyTypeConfiguration();

			result.JobType = "SHP";
			result.DirectionCode = Core.Constants.FreightShipmentDirection.Code.All;
			result.Mode = Core.Constants.TransportModes.Air;
			result.Incoterm = "ALL";
			result.LineDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			result.SupplyType = "LOC";

			return result;
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		public void TestIncotermWithFCNJobType_ReadOnly()
		{
			var supplyTypeConfiguration = GetBusinessObjectToClone() as SupplyTypeConfiguration;
			supplyTypeConfiguration.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			supplyTypeConfiguration.Incoterm = "DDP";
			AssertEquals("Precondition: Readonly false before jobtype change", false, supplyTypeConfiguration.IncotermInfo.ReadOnly);

			supplyTypeConfiguration.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			Assert(supplyTypeConfiguration.IncotermInfo.ReadOnly);
			AssertEquals(AccountingMasterFilesConstants.INCOTermCodes.All, supplyTypeConfiguration.Incoterm);
		}

		public void TestIncotermWithFCNJobType_DefaultValue()
		{
			var supplyTypeConfiguration = GetBusinessObjectToClone() as SupplyTypeConfiguration;
			AssertEquals(AccountingMasterFilesConstants.INCOTermCodes.All, supplyTypeConfiguration.Incoterm);
			supplyTypeConfiguration.Incoterm = "DDP";
			supplyTypeConfiguration.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertEquals("Precondition: Readonly true", true, supplyTypeConfiguration.IncotermInfo.ReadOnly);
			AssertEquals(AccountingMasterFilesConstants.INCOTermCodes.All, supplyTypeConfiguration.Incoterm);
		}
	}
}
