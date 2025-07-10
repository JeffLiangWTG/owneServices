using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment.ForwardingShipmentInvoicingSupporter))]
	public class ForwardingShipmentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public override void TestCustomsEntryNumberType()
		{
			// Base test expects empty string while in ForwardingShipment we always have shipment.CustomsEntryNumberType
			var shipment = Factory.New<ForwardingShipment>();
			shipment.CustomsEntryNumberType = "TF";
			var supporter = new ForwardingShipment.ForwardingShipmentInvoicingSupporter(shipment);

			AssertEquals("CustomsEntryNumberType", "TF", supporter.CustomsEntryNumberType);
		}

		public void TestGetReasonNotToAllowAutoRate_WhenAutorateRevenue_ClientContractNumberRulesNotViolated_ShouldNotReturnError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var supporter = new ForwardingShipment.ForwardingShipmentInvoicingSupporter(shipment);

			var numbers = shipment.Numbers;
			numbers.AddOrSkipContractNumber("AAA", "CLC", countryCode: "AU");

			var result = supporter.GetReasonNotToAllowAutoRate(AutoRateOptions.AutorateRevenue);
			AssertEquals("There should be only 1 CLC", 1, numbers.Count);
			AssertNullOrEmpty(result);
		}

		public void TestGetReasonNotToAllowAutoRate_WhenAutorateRevenue_ClientContractNumberRulesViolated_ShouldReturnError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var supporter = new ForwardingShipment.ForwardingShipmentInvoicingSupporter(shipment);

			var numbers = shipment.Numbers;
			numbers.AddOrSkipContractNumber("AAA", "CLC", countryCode: "AU");
			var newNumber = numbers.AddNew();
			newNumber.CE_EntryType = "CLC";
			newNumber.CE_RN_NKCountryCode = "US";
			newNumber.CE_EntryNum = "BBB";

			var result = supporter.GetReasonNotToAllowAutoRate(AutoRateOptions.AutorateRevenue);
			AssertEquals("Only one Client Contract Number, regardless same or different Country/Region of Issue, can be used for Autorating Revenue.", result);
		}

		#region Implementation

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			ForwardingShipment forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			return forwardingShipment;
		}

		#endregion
	}
}
