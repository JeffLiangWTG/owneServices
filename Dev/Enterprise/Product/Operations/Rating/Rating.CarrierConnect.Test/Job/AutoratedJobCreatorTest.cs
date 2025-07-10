using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Rating.CarrierConnect.Test;

public abstract class AutoratedJobCreatorTest : TestCaseWithFactory
{
	protected void AssertJobHasCharge(Job job, RateChargeDto charge)
	{
		var costCollection = job?.Charges;
		AssertEquals(1, costCollection.Count);

		var chargeInShipment = costCollection[0];

		AssertEquals(chargeInShipment.ChargeCode.AC_Code, charge.ChargeCode.ChargeCode);
		AssertEquals(chargeInShipment.JR_RX_NKCostCurrency, charge.RateCurrency);
		AssertEquals(chargeInShipment.JR_OSCostAmt, charge.RateAmount);
		AssertEquals(chargeInShipment.CostCalculationDescription, ZBlob.FromUTF8(charge.Description));
	}
}

