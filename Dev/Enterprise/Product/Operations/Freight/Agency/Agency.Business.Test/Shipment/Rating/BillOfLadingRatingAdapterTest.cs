using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BillOfLadingRatingAdapterTest : AgencyShipmentRatingAdapterTest<BillOfLadingJobDatesProvider>
	{
		public override AgencyShipment CreateShipment()
		{
			return Factory.NewWithValidTestData<BillOfLading>();
		}

		public override string ConsumerType
		{
			get
			{
				return JobInvoicingConsumerTypes.AgencyBillOfLading.Code;
			}
		}

		public override AgencyShipmentContainer AddNewContainer(AgencyShipment shipment)
		{
			return shipment.RealContainers.AddNew();
		}

		protected override string GetExpectedSellAutoratingMode()
		{
			return Core.Constants.FreightRateAutoratingModes.Code.StandardRate;
		}

		protected override string GetExpectedCostAutoratingMode()
		{
			return Core.Constants.FreightRateAutoratingModes.Code.StandardRate;
		}

		public void TestGetContractNumbers()
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			var number = billOfLading.Numbers.AddNew();
			number.CE_EntryType = "CON";
			number.CE_EntryNum = "AA";
			var adapter = new BillOfLadingRatingAdapter(billOfLading);
			AssertEquals("Should have 1 contract number", "AA", adapter.ClientContractNumbers.Single());
		}

		public void TestConditionsSupporterType()
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			var adapter = new BillOfLadingRatingAdapter(billOfLading);
			AssertEquals("Enterprise.Freight.Agency.Business.Shipment.Rating.BillOfLadingRateLineConditionsSupporter", adapter.ConditionsSupporter.GetType().ToString());
		}
	}
}
