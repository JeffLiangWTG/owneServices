using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Rating.CarrierConnect.Test;

public class AutoratedShipmentCreatorTest : AutoratedJobCreatorTest
{
	public void TestCreateFCLShipment()
	{
		var effectiveOn = new ZDate(2020, 1, 1);
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var appliedCharge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = JobTestHelpers.CreateJobDto(abcOrg, effectiveOn, appliedCharge);

		var autoratedShipmentCreator = new AutoratedShipmentCreator(dto);
		// Act
		var shipment = new AutoratedShipmentCreator(dto).CreateJob() as ForwardingShipment;
		var cusNumbers = shipment.Numbers.Cast<CusEntryNumber>();

		// Assert
		CombineAssertions(() =>
		{
			AssertNotNull(shipment);
			AssertEquals(dto.RateResult.TransportMode, shipment.JS_TransportMode);
			AssertEquals(dto.RateResult.ContainerMode, shipment.JS_PackingMode);
			AssertEquals(dto.RateResult.Origin, shipment.JS_RL_NKOrigin);
			AssertEquals(dto.RateResult.Destination, shipment.JS_RL_NKDestination);
			AssertEquals("GEN", shipment.OuterPackLines[0].JL_RH_NKCommodityCode);
			AssertEquals(abcOrg.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
			AssertNotNull(shipment.Numbers);
			AssertEquals(dto.RateResult.CarrierContractNumber, cusNumbers.First().CE_EntryNum);
			AssertEquals("CON", cusNumbers.First().CE_EntryType);

			AssertJobHasCharge(shipment.Job as Job, appliedCharge);
		});
	}
}

