using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderInvoicingSupporter))]
	sealed class CarrierShipmentHeaderInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestJobHeaderMembers()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "DF00001";

			var plugin = new CarrierShipmentHeaderJobInvoicingPlugIn(carrierShipmentHeader);
			AssertEquals(nameof(CarrierShipmentHeaderJobInvoicingPlugIn.JobNumber), carrierShipmentHeader.CSH_CarrierShipmentReference, plugin.JobNumber);
			AssertEquals(nameof(CarrierShipmentHeaderJobInvoicingPlugIn.Factory), carrierShipmentHeader.Factory, plugin.Factory);
			AssertEquals(nameof(CarrierShipmentHeaderJobInvoicingPlugIn.TableName), carrierShipmentHeader.TableName, plugin.TableName);
			AssertEquals(nameof(CarrierShipmentHeaderJobInvoicingPlugIn.IsInDatabase), carrierShipmentHeader.IsInDatabase, plugin.IsInDatabase);
			AssertEquals(nameof(CarrierShipmentHeaderJobInvoicingPlugIn.IsDeleted), carrierShipmentHeader.IsDeleted, plugin.IsDeleted);
			Assert(nameof(CarrierShipmentHeaderJobInvoicingPlugIn.AllowInvoiceDeletion), plugin.AllowInvoiceDeletion);
		}

		public new void TestCreateFreightWrapperForIJobInvoicingPlugin()
		{
			Assert("Document Engine support will be added at a later stage", true);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "DF00001";
			return carrierShipmentHeader.InvoicingPlugIn;
		}
	}
}
