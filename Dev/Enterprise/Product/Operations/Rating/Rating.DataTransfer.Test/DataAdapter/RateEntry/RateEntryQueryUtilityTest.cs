using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	public class RateEntryQueryUtilityTest : RatingTestCase
	{
		void AssertInitialisation(RateEntryQueryUtility utility)
		{
			AssertEquals("Frequency is initialised", 0, utility.Frequency);
			AssertEquals("Category is initialised", "", utility.Category);
			AssertEquals("Mode is initialised", "", utility.Mode);
			AssertEquals("Destination is initialised", "", utility.Destination);
			AssertEquals("Origin is initialised", "", utility.Origin);
			AssertEquals("CommodityCode is initialised", "", utility.CommodityCode);
			AssertEquals("TransitTime is initialised", "", utility.TransitTime);
			AssertEquals("FrequencyUnits is initialised", "", utility.FrequencyUnits);
			AssertEquals("CartageDeliveryAddressPostCode is initialised", "", utility.CartageDeliveryAddressPostCode);
			AssertEquals("CartagePickupAddressPostCode is initialised", "", utility.CartagePickupAddressPostCode);
			AssertEquals("Via is initialised", "", utility.Via);
			AssertEquals("ServiceLevel is initialised", "", utility.ServiceLevel);
			AssertEquals("CarrierServiceLevel is initialised", "", utility.CarrierServiceLevel);
			AssertEquals("TransportProviderPK is initialised", ZGuid.Empty, utility.TransportProviderPK);
			AssertEquals("ConsigneePK is initialised", ZGuid.Empty, utility.ConsigneePK);
			AssertEquals("ConsignorPK is initialised", ZGuid.Empty, utility.ConsignorPK);
			AssertEquals("CartagePickupAddressOverridePK is initialised", ZGuid.Empty, utility.CartagePickupAddressOverridePK);
			AssertEquals("CartageDeliveryAddressOverridePK is initialised", ZGuid.Empty, utility.CartageDeliveryAddressOverridePK);
			AssertEquals("ContainterTypePK is initialised", ZGuid.Empty, utility.ContainterTypePK);
			AssertEquals("SupplierPK is initialised", ZGuid.Empty, utility.TransportProviderPK);
			AssertEquals("SupplierPK is initialised", ZGuid.Empty, utility.ConsignorPK);
			AssertEquals("SupplierPK is initialised", ZGuid.Empty, utility.ConsigneePK);
			AssertEquals("SupplierPK is initialised", ZGuid.Empty, utility.SupplierPK);
			AssertEquals("StartDate is initialised", ZDateTime.Empty, utility.StartDate);
			AssertEquals("EndDate is initialised", ZDateTime.Empty, utility.EndDate);
		}

		public void TestSetAllParameters()
		{
			RatingXSDTestHelper testHelper = new RatingXSDTestHelper();
			testHelper.Consignee.Factory.Save();

			var costing = Factory.New<Costing>();

			Xsd.RateEntry rateEntry = testHelper.RateEntryXSD;

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ARTWAT"));

			rateEntry.Consignor.EDICode = "ARTWAT";
			rateEntry.ServiceProvider.EDICode = "NonExistantOrg";

			RateEntryQueryUtility utility = new RateEntryQueryUtility(costing);

			AssertInitialisation(utility);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			utility.SetAllParameters(rateEntry, context);

			AssertEquals("Frequency", 2, utility.Frequency);
			AssertEquals("Category", "AIR", utility.Category);
			AssertEquals("Mode", "LSE", utility.Mode);
			AssertEquals("Destination", "NZAKL", utility.Destination);
			AssertEquals("Origin", "AUSYD", utility.Origin);
			AssertEquals("CommodityCode", "GEN", utility.CommodityCode);
			AssertEquals("TransitTime", "", utility.TransitTime);
			AssertEquals("FrequencyUnits", "Daily", utility.FrequencyUnits);
			AssertEquals("CartageDeliveryAddressPostCode", "", utility.CartageDeliveryAddressPostCode);
			AssertEquals("CartagePickupAddressPostCode", "", utility.CartagePickupAddressPostCode);
			AssertEquals("Via", "", utility.Via);
			AssertEquals("ServiceLevel", "", utility.ServiceLevel);
			AssertEquals("CarrierServiceLevel", "", utility.CarrierServiceLevel);
			AssertEquals("TransportProviderPK", ZGuid.Empty, utility.TransportProviderPK);
			AssertEquals("ConsigneePK", testHelper.Consignee.PK, utility.ConsigneePK);
			AssertEquals("ConsignorPK", consignor.PK, utility.ConsignorPK);
			AssertEquals("SupplierPK", ZGuid.Empty, utility.SupplierPK);
			AssertEquals("CartagePickupAddressOverridePK", ZGuid.Empty, utility.CartagePickupAddressOverridePK);
			AssertEquals("CartageDeliveryAddressOverridePK", ZGuid.Empty, utility.CartageDeliveryAddressOverridePK);
			AssertEquals("ContainterTypePK", ZGuid.Empty, utility.ContainterTypePK);
			AssertEquals("StartDate", new ZDateTime(2006, 1, 1), utility.StartDate);
			AssertEquals("EndDate", ZDateTime.Empty, utility.EndDate);
			AssertEquals("IsCrossTrade", true, utility.IsCrossTrade);
			IXMLImportOrgCache iheader = costing;
			AssertEquals("All orgs are cached in header", 4, iheader.CachedOrgsForXMLImport.Count);
			AssertEquals("cached TransportProviderPK", null, iheader.CachedOrgsForXMLImport[rateEntry.TransportProvider.EDICode]);
			AssertEquals("ConsigneePK", utility.ConsigneePK, iheader.CachedOrgsForXMLImport[rateEntry.Consignee.EDICode].PK);
			AssertEquals("ConsignorPK", utility.ConsignorPK, iheader.CachedOrgsForXMLImport[rateEntry.Consignor.EDICode].PK);
			AssertEquals("SupplierPK", null, iheader.CachedOrgsForXMLImport[rateEntry.ServiceProvider.EDICode]);
		}
	}
}
