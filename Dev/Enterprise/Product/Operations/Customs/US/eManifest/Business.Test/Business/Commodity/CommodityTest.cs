using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(Commodity))]
	sealed class CommodityTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadShipmentFromDatabase()
		{
			var shipment = commodity.Shipment;
			commodity.Shipment = null;
			AssertEquals(shipment.PK, commodity.Shipment.PK);
		}

		public void TestICusInBondCargoDescIsCorrectlySetup()
		{
			AssertEquals(typeof(Commodity), ObjectFactory.GetType<Integration.Customs.US.eManifest.ICusInBondCargoDesc>());
		}

		public void TestSetDefaultGoodsValueByMonetaryValue()
		{
			var shipment = commodity.Shipment;
			AssertEquals("Pre - Condition: B0_GoodsValue", 0m, shipment.B0_GoodsValue);
			commodity.BY_MonetaryValue = 125m;
			AssertEquals(shipment.B0_GoodsValue, 125m);
		}

		public void TestManifestUQDefault()
		{
			AssertEquals("Pre-Condition: BY_ManifestUnitCode", commodity.Shipment.B0_ManifestUQ, commodity.BY_ManifestUnitCode);
			commodity.BY_PieceCount = 1;
			AssertEquals("BY_ManifestUnitCode should be defaulted from shipment", Constants.PkgUnit.Piece, commodity.BY_ManifestUnitCode);
			commodity = commodity.Shipment.Commodities.AddNew();
			commodity.BY_ManifestUnitCode = Constants.PkgUnit.Box;
			commodity.BY_PieceCount = 1;
			AssertEquals("BY_ManifestUnitCode should NOT be defaulted from shipment as the value is already set", Constants.PkgUnit.Box, commodity.BY_ManifestUnitCode);
		}

		public void TestWeightUQDefault()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Pre-Condition: BY_GrossWeightUnit", commodity.Shipment.B0_WeightUQ, commodity.BY_GrossWeightUnit);
				commodity.BY_GrossWeight = 1;
				AssertEquals("BY_GrossWeightUnit should be defaulted from shipment", Constants.Weight.Kilograms, commodity.BY_GrossWeightUnit);
				commodity = commodity.Shipment.Commodities.AddNew();
				commodity.BY_GrossWeightUnit = Constants.Weight.Grams;
				commodity.BY_GrossWeight = 1000;
				AssertEquals("BY_GrossWeightUnit should NOT be defaulted from shipment as the value is already set", Constants.Weight.Grams, commodity.BY_GrossWeightUnit);
			});
		}

		public void TestDelete()
		{
			var c4Code = commodity.C4Codes.AddNew();
			var tariff = commodity.HarmonizedNumbers.AddNew();
			var undg = commodity.UNDGs.AddNew();
			var vin = commodity.VehicleIdentificationNumbers.AddNew();
			commodity.Delete();
			Assert("C4Codes should be deleted", c4Code.IsDeleted);
			Assert("HarmonizedNumbers should be deleted", tariff.IsDeleted);
			Assert("UNDGs should be deleted", undg.IsDeleted);
			Assert("VehicleIdentificationNumbers should be deleted", vin.IsDeleted);
		}

		public void TestCollections()
		{
			CusCodeDataExtensionsTest.AssertPropertyIsStringRepresentationOfCusCodeDataCollection(commodity.BY_HarmonizedNumbersInfo, commodity.HarmonizedNumbers);
			commodity.BY_HarmonizedNumbers = "0403109000, 0403.10.9010";
			AssertEquals("0403.10.90 00, 0403.10.90 10", commodity.BY_HarmonizedNumbers);
			CusCodeDataExtensionsTest.AssertPropertyIsStringRepresentationOfCusCodeDataCollection(commodity.BY_VehicleIdentificationNumbersInfo, commodity.VehicleIdentificationNumbers);
			CusCodeDataExtensionsTest.AssertPropertyIsStringRepresentationOfCusCodeDataCollection(commodity.BY_C4CodesInfo, commodity.C4Codes);
		}

		public void TestHazardousGoodsIdentifier()
		{
			commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("UNDGs.Count", 1, commodity.UNDGs.Count);
			AssertEquals("UNDGSubstance.DG_Code", "0004a", commodity.UNDGs[0].UNDGSubstance.DG_Code);
			commodity.UNDGs[0].DI_DG = ZGuid.Empty;
			AssertEquals("BY_HazardousGoodsIdentifier", ZGuid.Empty, commodity.BY_HazardousGoodsIdentifier);
			commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;
			AssertEquals("UNDGs.Count", 1, commodity.UNDGs.Count);
			AssertEquals("UNDGSubstance.DG_Code", "0004b", commodity.UNDGs[0].UNDGSubstance.DG_Code);
			commodity.UNDGs.DeleteAll();
			AssertEquals("BY_HazardousGoodsIdentifier", ZGuid.Empty, commodity.BY_HazardousGoodsIdentifier);
			var undg0006Pk = UNDGSubstanceLoader.LoadSubstances(Factory, "0006", "", "IMO").First().PK;
			commodity.UNDGs.AddNew().DI_DG = undg0006Pk;
			AssertEquals("BY_HazardousGoodsIdentifier", undg0006Pk, commodity.BY_HazardousGoodsIdentifier);
			var undg0005Pk = UNDGSubstanceLoader.LoadSubstances(Factory, "0005", "", "IMO").First().PK;
			commodity.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0005", "", "IMO").First().PK;
			AssertEquals("BY_HazardousGoodsIdentifier", ZGuid.Empty, commodity.BY_HazardousGoodsIdentifier);
			commodity.UNDGs[0].Delete();
			AssertEquals("BY_HazardousGoodsIdentifier", undg0005Pk, commodity.BY_HazardousGoodsIdentifier);
			commodity.BY_HazardousGoodsContact = ZGuid.NewZGuid();
			commodity.BY_HazardousGoodsIdentifier = ZGuid.Empty;
			AssertEquals("UNDGs.Count when one value is cleared", 1, commodity.UNDGs.Count);
			commodity.BY_HazardousGoodsContact = ZGuid.Empty;
			AssertEquals("UNDGs.Count when item is empty", 0, commodity.UNDGs.Count);
		}

		public void TestHazardousGoodsContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Big Boss";
			contact.OC_Phone = "+3 (126) 4846516";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Big Boss 2";
			contact2.OC_Phone = "+3 (126) 3335545";
			commodity.BY_HazardousGoodsContact = contact.PK;
			AssertEquals("UNDGs.Count", 1, commodity.UNDGs.Count);
			AssertEquals("DI_OC_DGContact", contact.PK, commodity.UNDGs[0].DI_OC_DGContact);
			AssertEquals("BY_HazardousGoodsContactPhone", contact.OC_Phone, commodity.BY_HazardousGoodsContactPhone);
			commodity.UNDGs[0].DI_OC_DGContact = ZGuid.Empty;
			AssertEquals("BY_HazardousGoodsContact", ZGuid.Empty, commodity.BY_HazardousGoodsContact);
			AssertEquals("BY_HazardousGoodsContactPhone", ZString.Empty, commodity.BY_HazardousGoodsContactPhone);
			commodity.BY_HazardousGoodsContact = contact.PK;
			AssertEquals("UNDGs.Count", 1, commodity.UNDGs.Count);
			AssertEquals("DI_OC_DGContact", contact.PK, commodity.UNDGs[0].DI_OC_DGContact);
			commodity.UNDGs.DeleteAll();
			AssertEquals("BY_HazardousGoodsContact", ZGuid.Empty, commodity.BY_HazardousGoodsContact);
			AssertEquals("BY_HazardousGoodsContactPhone", ZString.Empty, commodity.BY_HazardousGoodsContactPhone);
			commodity.UNDGs.AddNew().DI_OC_DGContact = contact.PK;
			AssertEquals("BY_HazardousGoodsContact", contact.PK, commodity.BY_HazardousGoodsContact);
			AssertEquals("BY_HazardousGoodsContactPhone", contact.OC_Phone, commodity.BY_HazardousGoodsContactPhone);
			commodity.UNDGs.AddNew().DI_OC_DGContact = contact2.PK;
			AssertEquals("BY_HazardousGoodsContact", ZGuid.Empty, commodity.BY_HazardousGoodsContact);
			AssertEquals("BY_HazardousGoodsContactPhone", ZString.Empty, commodity.BY_HazardousGoodsContactPhone);
			commodity.UNDGs[0].Delete();
			AssertEquals("BY_HazardousGoodsContact", contact2.PK, commodity.BY_HazardousGoodsContact);
			AssertEquals("BY_HazardousGoodsContactPhone", contact2.OC_Phone, commodity.BY_HazardousGoodsContactPhone);
			commodity.BY_HazardousGoodsContact = ZGuid.Empty;
			commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("UNDGs.Count when one value is cleared", 1, commodity.UNDGs.Count);
			commodity.BY_HazardousGoodsIdentifier = ZGuid.Empty;
			AssertEquals("UNDGs.Count when item is empty", 0, commodity.UNDGs.Count);
		}

		public void ShipmentTypeSpecificFieldsReadOnlyAndClearedOnSave()
		{
			AssertShipmentTypeSpecificFields(ShipmentTypes.Codes.PAPS, true, true, true);
			AssertShipmentTypeSpecificFields(ShipmentTypes.Codes.BRASS, false, true, true);
			AssertShipmentTypeSpecificFields(ShipmentTypes.Codes.LowValue, true, false, false);
			AssertShipmentTypeSpecificFields(ShipmentTypes.Codes.Inbond, true, false, true);
			AssertShipmentTypeSpecificFields(ShipmentTypes.Codes.GoodsAstray, true, false, true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			commodity = (Commodity)GetNewBusinessObject();
		}
		Commodity commodity;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var trip = factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ManifestQty = 2;
			shipment.B0_ManifestUQ = Constants.PkgUnit.Piece;
			shipment.B0_Weight = 2;
			shipment.B0_WeightUQ = Constants.Weight.Kilograms;
			return shipment.Commodities[0];
		}

		void AssertShipmentTypeSpecificFields(string shipmentType, bool c4Codes, bool customsValue, bool countryOfOrigin)
		{
			commodity.Shipment.B0_ShipmentType = shipmentType;
			commodity.BY_C4Codes = "123, 456";
			commodity.BY_MonetaryValue = 10;
			commodity.BY_RN_NKCountryOfOrigin = Constants.CountryCodes.UnitedStates;
			AssertEquals(string.Format("Is BY_C4Codes readonly for shipment type '{0}'", shipmentType), c4Codes, commodity.BY_C4CodesInfo.ReadOnly);
			AssertEquals(string.Format("Is BY_MonetaryValue readonly for shipment type '{0}'", shipmentType), customsValue, commodity.BY_MonetaryValueInfo.ReadOnly);
			AssertEquals(string.Format("Is BY_RN_NKCountryOfOrigin readonly for shipment type '{0}'", shipmentType), countryOfOrigin, commodity.BY_RN_NKCountryOfOriginInfo.ReadOnly);
			Factory.Save();
			AssertEquals(string.Format("Is BY_C4Codes empty for shipment type '{0}' after save", shipmentType), c4Codes, commodity.BY_C4Codes.IsEmpty);
			AssertEquals(string.Format("Is BY_MonetaryValue empty for shipment type '{0}' after save", shipmentType), customsValue, commodity.BY_MonetaryValue.IsEmpty);
			AssertEquals(string.Format("Is BY_RN_NKCountryOfOrigin empty for shipment type '{0}' after save", shipmentType), countryOfOrigin, commodity.BY_RN_NKCountryOfOrigin.IsEmpty);
		}
	}
}
