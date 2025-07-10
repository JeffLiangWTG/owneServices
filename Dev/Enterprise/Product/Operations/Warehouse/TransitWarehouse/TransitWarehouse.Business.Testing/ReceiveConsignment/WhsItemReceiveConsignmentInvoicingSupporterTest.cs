using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemConsignmentInvoicingSupporter<WhsItemReceiveConsignment>))]
	public class WhsItemReceiveConsignmentInvoicingSupporterTest : TransitJobInvoicingSupporterTest<WhsItemReceiveConsignment>
	{
		protected override IJobInvoicingSupporter GetInvoicingSupporter(WhsItemReceiveConsignment consignment) => new WhsItemConsignmentInvoicingSupporter<WhsItemReceiveConsignment>(consignment);

		protected void CreateTestConsignment(int index = 1, decimal volume = 0.0M, string volumeUQ = "M3", decimal weight = 0.0M, string weightUQ = "KG")
		{
			BusinessObject.WRC_JobID = $"RCN{index}";
			BusinessObject.WRC_ConsignmentID = $"RCN{index}";
			BusinessObject.WRC_WW_IntendedWarehouse = Warehouse.PK;
			BusinessObject.WRC_RL_NKDestination = "AUSYD";
			var stageLocation = Warehouse.DefaultLocation;
			var rtu = Helper.CreateReceiveTransportationUnit($"RTU{index}", Warehouse.PK, stageLocation.PK);
			Helper.CreatePackageState(BusinessObject, 1, "PKG", $"P{index}", TransitWarehouseStatuses.Codes.Arrived, rtu, volume: volume, volumeUQ: volumeUQ, weight: weight, weightUQ: weightUQ);
			Factory.Save();
		}

		#region TestHouseBillNumber

		public void TestHouseBillNumber()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);

			CreateTestConsignment();

			AssertEquals("RCN1", invoicingSupporter.HouseBillNumber);
			BusinessObject.WRC_HouseBillNumber = "HSB Test";
			AssertEquals("HSB Test", invoicingSupporter.HouseBillNumber);
		}

		#endregion

		#region TestDestination

		public void TestDestination()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertNull(invoicingSupporter.Destination);

			CreateTestConsignment();

			AssertEquals("AUSYD", invoicingSupporter.Destination.Code);
		}

		#endregion

		#region TestActualVolumeAndUnit

		public void TestActualVolumeAndUnit()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(0m, invoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, invoicingSupporter.ActualVolumeUnit);

			CreateTestConsignment(1, 1m, Constants.Volume.CubicMetres);
			CreateTestConsignment(2, 1000m, Constants.Volume.CubicDecimetres);

			invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(2m, invoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, invoicingSupporter.ActualVolumeUnit);
		}

		#endregion

		#region TestActualWeightAndUnit

		public void TestActualWeightAndUnit()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(0m, invoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, invoicingSupporter.ActualWeightUnit);

			CreateTestConsignment(1, weight: 1m, weightUQ: Constants.Weight.Kilograms);
			CreateTestConsignment(2, weight: 1000m, weightUQ: Constants.Weight.Grams);

			invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(2m, invoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, invoicingSupporter.ActualWeightUnit);
		}

		#endregion

		#region TestOuterPackTotal

		public override void TestOuterPackTotal()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(0, invoicingSupporter.OuterPackTotal);

			CreateTestConsignment(1);
			CreateTestConsignment(2);

			invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(2, invoicingSupporter.OuterPackTotal);
		}

		#endregion

		#region Implementation

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.TransitReceive;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.WhsItemReceiveConsignmentAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.WhsItemReceiveConsignmentJobInvoicing;

		protected override bool ExpectedIncludeInConsolCosting => true;

		#endregion
	}
}
