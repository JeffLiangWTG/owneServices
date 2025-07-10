using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class BOLInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestBOLIssuerCode()
		{
			bill.ABL_BillIssuer = "ISSUER";

			AssertEquals("ISSUER", provider.BOLIssuerCode.Value);
		}

		public void TestBOLNumber()
		{
			bill.ABL_BillNumber = "123";

			AssertEquals("123", provider.BOLNumber.Value);

			bill.ABL_BillNumber = "";
			bill.Header.MasterBOL = "111";

			AssertEquals("111", provider.BOLNumber.Value);
		}

		public void TestBOLTypeCode()
		{
			bill.ABL_SpecialCargoCode = "00";

			AssertEquals("00", provider.BOLTypeCode.Value);
		}

		public void TestBOLClassification()
		{
			AssertEquals("H", provider.BOLClassificationCode.Value);

			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.MasterBillOfLading);
			AssertEquals("M", provider.BOLClassificationCode.Value);

			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.SimpleBillOfLading);
			AssertEquals("S", provider.BOLClassificationCode.Value);
		}

		public void TestSplitShipmentIndicator()
		{
			bill.ABL_ManifestQty = 123;

			arrivalLine.ATL_Quantity = 12;
			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.HouseBillOfLading);
			AssertEquals("Y", provider.SplitShipmentIndicator.Value);

			arrivalLine.ATL_Quantity = 123;
			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.HouseBillOfLading);
			AssertEquals("", provider.SplitShipmentIndicator.Value);
		}

		public void TestQuantity()
		{
			bill.ABL_ManifestQty = 123;

			AssertEquals("123", provider.Quantity.Value);
		}

		public void TestQuantityUnitOfMeasure()
		{
			bill.ABL_ManifestUQ = "KG";

			AssertEquals("KG", provider.QuantityUnitOfMeasure.Value);
		}

		public void TestWeight()
		{
			bill.ABL_GrossWeight = 123;

			AssertEquals("123", provider.Weight.Value);
		}

		public void TestWeightUnitOfMeasure()
		{
			bill.ABL_GrossWeightUQ = "KG";

			AssertEquals("KG", provider.WeightUnitOfMeasure.Value);
		}

		public void TestBoardedWeight()
		{
			var line2 = arrivalHeader.ArrivalDetails.AddNew();
			line2.ATL_Weight = 123;
			arrivalLine.ATL_Weight = 123;

			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.HouseBillOfLading);

			AssertEquals("246", provider.BoardedWeight.Value);
		}

		public void TestBoardedQuantity()
		{
			var line2 = arrivalHeader.ArrivalDetails.AddNew();
			line2.ATL_Quantity = 123;
			arrivalLine.ATL_Quantity = 123;

			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.HouseBillOfLading);

			AssertEquals("246", provider.BoardedQuantity.Value);
		}

		public void TestVolume()
		{
			bill.ABL_Volume = 123;

			AssertEquals("123", provider.Volume.Value);
		}

		public void TestVolumeUnitOfMeasure()
		{
			bill.ABL_VolumeUQ = "L";

			AssertEquals("L", provider.VolumeUnitOfMeasure.Value);
		}

		public void TestModeOfTransportationCode()
		{
			bill.ABL_InlandTransportMode = "41";

			AssertEquals("41", provider.ModeOfTransportationCode.Value);
		}

		public void TestReference()
		{
			var manifestHeader1 = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill1 = manifestHeader1.Bills.AddNew();
			bill1.ABL_CustomsDischargePort = "ABC";
			var provider1 = new BOLInfoTypeProvider(bill1, "A", BillOfLadingClassificationTypeList.Codes.HouseBillOfLading);
			AssertEquals(1, provider1.BOLReferenceInfoList.Count);
			AssertEquals("ABC", provider1.BOLReferenceInfoList.First(x => x.ReferenceTypeCode.Value == Enterprise.Customs.US.Messaging.Business.ReferenceQualifierList.Codes.CSK).ReferenceData.Value);

			manifestHeader1.MasterBOL = "MST401092442";
			var masterBill = manifestHeader1.MasterBill;
			masterBill.ABL_BillIssuer = "APLUTEST";
			AssertEquals(2, provider1.BOLReferenceInfoList.Count);
			AssertEquals("APLUMST401092442", provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == BOLInfoTypeProvider.OBReferenceType).ReferenceData.Value);

			bill1.ABL_UCRNumber = "TES";
			AssertEquals(3, provider1.BOLReferenceInfoList.Count);
			AssertEquals(ZString.Empty, provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == "TES").ReferenceData.Value);

			bill1.InBondNumbers = "000000001,000000002";
			AssertEquals(5, provider1.BOLReferenceInfoList.Count);
			AssertEquals("000000001", provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == BOLInfoTypeProvider.InBondNumReferenceType).ReferenceData.Value);
			AssertEquals("000000002", provider1.BOLReferenceInfoList.LastOrDefault(x => x.ReferenceTypeCode.Value == BOLInfoTypeProvider.InBondNumReferenceType).ReferenceData.Value);
			AssertEquals(ZString.Empty, provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == "TES").ReferenceData.Value);

			bill1.ABL_UCRNumber = ZString.Empty;
			bill1.AESITNNumbers = "X20120112901245,X2012011200001";
			AssertEquals(6, provider1.BOLReferenceInfoList.Count);
			AssertEquals(false, provider1.BOLReferenceInfoList.Any(x => x.ReferenceTypeCode.Value == "TES"));
			AssertEquals("X20120112901245", provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN).ReferenceData.Value);
			AssertEquals("X2012011200001", provider1.BOLReferenceInfoList.LastOrDefault(x => x.ReferenceTypeCode.Value == Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN).ReferenceData.Value);

			masterBill.ABL_BillIssuer = ZString.Empty;
			bill1.ABL_UCRNumber = "ABC";
			manifestHeader1.AMA_ApplicationCode = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			AssertEquals(6, provider1.BOLReferenceInfoList.Count);
			AssertEquals("ABC", provider1.BOLReferenceInfoList.First(x => x.ReferenceTypeCode.Value == Enterprise.Customs.US.Messaging.Business.ReferenceQualifierList.Codes.CSK).ReferenceData.Value);
			AssertEquals("X20120112901245", provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN).ReferenceData.Value);
			AssertEquals("X2012011200001", provider1.BOLReferenceInfoList.LastOrDefault(x => x.ReferenceTypeCode.Value == Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN).ReferenceData.Value);
			AssertEquals("000000001", provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == BOLInfoTypeProvider.InBondNumReferenceType).ReferenceData.Value);
			AssertEquals("000000002", provider1.BOLReferenceInfoList.LastOrDefault(x => x.ReferenceTypeCode.Value == BOLInfoTypeProvider.InBondNumReferenceType).ReferenceData.Value);
			AssertEquals(ZString.Empty, provider1.BOLReferenceInfoList.FirstOrDefault(x => x.ReferenceTypeCode.Value == "ABC").ReferenceData.Value);
		}

		public void TestLocationInfo()
		{
			bill.ABL_CustomsLoadPort = "0001";
			bill.ABL_CustomsDischargePort = "0002";
			manifestHeader.AMA_CustomsFirstArrivalPort = "0003";
			manifestHeader.AMA_CustomsFinalDeparturePort = "0004";
			bill.ABL_CustomsOriginPort = "0005";
			bill.ABL_CustomsFinalDestinationPort = "0006";
			bill.ABL_LocationInformation = "LOC";
			bill.VisitedPorts.AddNew("VI1");
			bill.VisitedPorts.AddNew("VI2");

			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.HouseBillOfLading);

			AssertLocations("L", new List<string> { "0001" });
			AssertLocations("U", new List<string> { "0002" });
			AssertLocations("A", new List<string> { "0003" });
			AssertLocations("D", new List<string> { "0004" });
			AssertLocations("O", new List<string> { "0005" });
			AssertLocations("R", new List<string> { "LOC" });
			AssertLocations("T", new List<string> { "VI1", "VI2" });
			AssertLocations("M", new List<string> { "0006" });
		}

		void AssertLocations(string type, List<string> values)
		{
			var list = provider.BOLLocationList.Where(l => l.LocationTypeCode.Value == type).Select(e => e.Location.Value).ToList();

			AssertContainsExactElementsInAnyOrder(values, list);
		}

		public void TestParties()
		{
			bill.ABL_ConsigneeName = "CON";
			bill.ABL_ConsigneeStreet1 = "CON ADD1";
			bill.ABL_ConsigneeStreet2 = "CON ADD2";
			bill.ABL_ConsigneeCity = "CON CITY";
			bill.ABL_ConsigneeState = "CON STATE";
			bill.ABL_ConsigneePostcode = "CON POST";
			bill.ABL_RN_NKConsigneeCountry = "US";
			bill.ABL_ConsigneePhone = "CON PHN";

			bill.ABL_ShipperName = "SHP";
			bill.ABL_ShipperStreet1 = "SHP ADD1";
			bill.ABL_ShipperStreet2 = "SHP ADD2";
			bill.ABL_ShipperCity = "SHP CITY";
			bill.ABL_ShipperState = "SHP STATE";
			bill.ABL_ShipperPostcode = "SHP POST";
			bill.ABL_RN_NKShipperCountry = "CA";
			bill.ABL_ShipperPhone = "SHP PHN";

			bill.ABL_NotifyPartyName = "NTF";
			bill.ABL_NotifyPartyStreet1 = "NTF ADD1";
			bill.ABL_NotifyPartyStreet2 = "NTF ADD2";
			bill.ABL_NotifyPartyCity = "NTF CITY";
			bill.ABL_NotifyPartyState = "NTF STATE";
			bill.ABL_NotifyPartyPostcode = "NTF POST";
			bill.ABL_RN_NKNotifyPartyCountry = "FR";
			bill.ABL_NotifyPartyPhone = "NTF PHN";

			AssertParty("CN", "CON", "CON ADD1", "CON ADD2", "CON CITY", "CON STATE", "CON POST", "US", "CON PHN");
			AssertParty("SH", "SHP", "SHP ADD1", "SHP ADD2", "SHP CITY", "SHP STATE", "SHP POST", "CA", "SHP PHN");
			AssertParty("NY", "NTF", "NTF ADD1", "NTF ADD2", "NTF CITY", "NTF STATE", "NTF POST", "FR", "NTF PHN");
			Assert(true);
		}

		void AssertParty(string type, string name, string addressLine1, string addressLine2, string city, string state, string postCode, string countryCode, string phone)
		{
			foreach (var party in provider.BOLPartyInfoList)
			{
				if (type == party.PartyType.Value)
				{
					AssertEquals("Party type " + type, name, party.PartyName.Value);
					AssertEquals("Party type " + type, addressLine1, party.PartyAddressLine1.Value);
					AssertEquals("Party type " + type, addressLine2, party.PartyAddressLine2.Value);
					AssertEquals("Party type " + type, city, party.CityName.Value);
					AssertEquals("Party type " + type, state, party.StateCode.Value);
					AssertEquals("Party type " + type, postCode, party.PostalCode.Value);
					AssertEquals("Party type " + type, countryCode, party.CountryCode.Value);
					AssertEquals("Party type " + type, phone, party.ContactPhoneNumber.Value);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();

			bill = manifestHeader.Bills.AddNew();
			arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			arrivalLine = arrivalHeader.ArrivalDetails.AddNew();
			provider = new BOLInfoTypeProvider(bill, "A", BillOfLadingClassificationTypeList.Codes.HouseBillOfLading);
		}
		IBOLInfoType provider;
		USExportAsycudaBill bill;
		AsycudaArrivalHeader arrivalHeader;
		AsycudaArrivalLine arrivalLine;
		USExportAsycudaManifestHeader manifestHeader;
	}
}
