using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business.Test.AutoRating.CarrierShipmentRates;

public class CarrierShipmentRatingAdapterTest : RatingTestCase
{
	public void TestOrigin_ReturnsOriginFromParentAsLocation()
	{
		var bizObj = new CarrierShipmentRateQueryBusinessObject(null, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals("Sydney", ratingAdapter.Origin.Description);
	}

	public void TestDestination_ReturnsDestinationFromParentAsLocation()
	{
		var bizObj = new CarrierShipmentRateQueryBusinessObject(null, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals("Los Angeles", ratingAdapter.Destination.Description);
	}

	public void TestRateTypeToUse_ReturnsRateTypeFromParent()
	{
		var bizObj = new CarrierShipmentRateQueryBusinessObject(null, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals(RateType.Shipping | RateType.ShippingImportDetention | RateType.ShippingExportDetention,
			ratingAdapter.RateTypeToUse);
	}

	public void TestJobDatesProvider_ReturnsCarrierShipmentJobDatesProvider()
	{
		var bizObj = new CarrierShipmentRateQueryBusinessObject(null, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertType(typeof(CarrierShipmentJobDatesProvider), ratingAdapter.JobDatesProvider);
	}

	public void TestCreditors_ReturnsNewCreditorFromParentCarrier()
	{
		Helper.NewOrgHeader("testCarrier");
		var rateQueryDto = new CarrierShipmentRateQueryDto { Shipment = new() { Carrier = "testCarrier" } };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals("testCarrier", ratingAdapter.Creditors.AllOrgsWithSource[0].Org.OH_Code);
		AssertEquals("TransportProvider", ratingAdapter.Creditors.AllOrgsWithSource[0].Source[0]);
	}

	public void TestDebtorOrgs_WhenAllPartiesMissing_ReturnsEmptyDebtorOrgCollection()
	{
		var rateQueryDto = new CarrierShipmentRateQueryDto { Shipment = new() };
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals(0, ratingAdapter.DebtorOrgs.Count);
	}

	public void TestDebtorOrgs_WhenParentConsignorIsGiven_ReturnsDebtorOrgCollectionWithConsignorAsOrgHeader()
	{
		Helper.NewOrgHeader("testCRD");
		var rateQueryDto = new CarrierShipmentRateQueryDto
		{
			Shipment = new()
			{
				RateParties =
					new CarrierShipmentRateRatePartyDto[] { new() { Code = "testCRD", Role = DocAddressTypes.Codes.ConsignorDocumentaryAddress } }
			}
		};
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals(1, ratingAdapter.DebtorOrgs.Count);
		AssertEquals("testCRD", ratingAdapter.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.CNR].OH_Code);
	}

	public void TestDebtorOrgs_WhenParentConsigneeIsGiven_ReturnsDebtorOrgCollectionWithConsigneeAsOrgHeader()
	{
		Helper.NewOrgHeader("testCED");
		var rateQueryDto = new CarrierShipmentRateQueryDto
		{
			Shipment = new()
			{
				RateParties =
					new CarrierShipmentRateRatePartyDto[] { new() { Code = "testCED", Role = DocAddressTypes.Codes.ConsigneeDocumentaryAddress } }
			}
		};
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals(1, ratingAdapter.DebtorOrgs.Count);
		AssertEquals("testCED", ratingAdapter.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.CNE].OH_Code);
	}

	public void TestDebtorOrgs_WhenParentBookingPartyIsGiven_ReturnsDebtorOrgCollectionWithBookingPartyAsOrgHeader()
	{
		Helper.NewOrgHeader("testBKD");
		var rateQueryDto = new CarrierShipmentRateQueryDto
		{
			Shipment = new()
			{
				RateParties =
					new CarrierShipmentRateRatePartyDto[] { new() { Code = "testBKD", Role = DocAddressTypes.Codes.BookingPartyDocumentaryAddress } }
			}
		};
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals(1, ratingAdapter.DebtorOrgs.Count);
		AssertEquals("testBKD", ratingAdapter.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.LCBK].OH_Code);
	}

	public void TestDebtorOrgs_WhenAllPartiesGiven_ReturnsDebtorOrgCollectionWithAllPartiesAsOrgHeaders()
	{
		Helper.NewOrgHeader("testCRD");
		Helper.NewOrgHeader("testCED");
		Helper.NewOrgHeader("testBKD");
		var rateQueryDto = new CarrierShipmentRateQueryDto
		{
			Shipment = new()
			{
				RateParties =
					new CarrierShipmentRateRatePartyDto[]
					{
						new() { Code = "testCRD", Role = DocAddressTypes.Codes.ConsignorDocumentaryAddress },
						new() { Code = "testCED", Role = DocAddressTypes.Codes.ConsigneeDocumentaryAddress },
						new() { Code = "testBKD", Role = DocAddressTypes.Codes.BookingPartyDocumentaryAddress }
					}
			}
		};
		var bizObj = new CarrierShipmentRateQueryBusinessObject(rateQueryDto, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		AssertEquals(3, ratingAdapter.DebtorOrgs.Count);
		AssertEquals("testCRD", ratingAdapter.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.CNR].OH_Code);
		AssertEquals("testCED", ratingAdapter.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.CNE].OH_Code);
		AssertEquals("testBKD", ratingAdapter.DebtorOrgs[Enterprise.Registry.Business.RatingDebtorOrgTypes.LCBK].OH_Code);
	}

	public void TestChargeCodeGroups_ReturnsChargeCodeGroupCollectionFromRegistry()
	{
		Env.Registry.Rating.SetFreightRatedCodes("TST");
		var bizObj = new CarrierShipmentRateQueryBusinessObject(null, "AUSYD", "USLAX");
		var ratingAdapter = new CarrierShipmentRatingAdapter(bizObj, CostSell.Cost, Factory);

		Assert(ratingAdapter.ChargeCodeGroups.Contains("TST"));
	}
}
