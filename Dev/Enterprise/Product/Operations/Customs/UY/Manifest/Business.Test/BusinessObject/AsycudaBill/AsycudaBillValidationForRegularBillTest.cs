using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class AsycudaBillValidationForRegularBillTest : BusinessObjectLookupsTestCase
	{
		public void TestRegistrationNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = UruguayOrgCusCodeInfo.OrgCusCodes.CID;
			bill.ABL_ConsigneeRegNo = "62318879";
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);
			bill.ABL_ConsigneeRegNo = "623188";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "The length should be: 8");
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "You have not entered a value.");

			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_ShipperRegNoType = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			bill2.ABL_ShipperRegNo = "623188";
			AssertHasMessageError(bill2.ABL_ShipperRegNoInfo, "The RUT code should be: 12 digits only");
			bill2.ABL_ShipperRegNo = "216714840016";
			AssertNoNotifications(bill2.ABL_ShipperRegNoInfo);
			bill2.ABL_ShipperRegNo = ZString.Empty;
			AssertNoWarning(bill2.ABL_ShipperRegNoInfo, "You have not entered a value.");

			var bill3 = Factory.New<AsycudaBill>();
			bill3.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill3.ABL_NotifyPartyRegNo = "231462318812";
			AssertNoNotifications(bill3.ABL_NotifyPartyRegNoInfo);
			bill3.ABL_NotifyPartyRegNo = ZString.Empty;
			AssertHasWarning(bill3.ABL_NotifyPartyRegNoInfo, "You have not entered a value.");
		}

		public void TestRegistrationNoType()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "You have not entered a value.");
			bill.ABL_ConsigneeRegNoType = UruguayOrgCusCodeInfo.OrgCusCodes.CID;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoTypeInfo);
			bill.ABL_ConsigneeRegNoType = "VAT";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "The code you have selected is not in the list.");

			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_ShipperRegNoType = ZString.Empty;
			AssertNoWarning(bill2.ABL_ShipperRegNoTypeInfo, "You have not entered a value.");
			bill2.ABL_ShipperRegNoType = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			AssertNoNotifications(bill2.ABL_ShipperRegNoTypeInfo);
			bill2.ABL_ShipperRegNoType = "VAT";
			AssertHasMessageError(bill2.ABL_ShipperRegNoTypeInfo, "The code you have selected is not in the list.");

			var bill3 = Factory.New<AsycudaBill>();
			bill3.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertNoNotifications(bill3.ABL_NotifyPartyRegNoTypeInfo);
			bill3.ABL_NotifyPartyRegNoType = ZString.Empty;
			AssertNoNotifications(bill3.ABL_NotifyPartyRegNoTypeInfo);
			bill3.ABL_NotifyPartyRegNoType = "VAT";
			AssertHasMessageError(bill3.ABL_NotifyPartyRegNoTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestShipperMandatoryData()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipperName = ZString.Empty;
			AssertHasMessageError(bill.ABL_ShipperNameInfo, MandatoryValidation.YouHaveNotEntered + " a Shipper Name.");

			bill.ABL_ShipperName = "Shipper Name";
			AssertNoNotifications(bill.ABL_ShipperNameInfo);
		}

		public void TestConsigneeMandatoryData()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeName = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeNameInfo, MandatoryValidation.YouHaveNotEntered + " a Consignee Name.");

			bill.ABL_ConsigneeName = "Consignee Name";
			AssertNoNotifications(bill.ABL_ConsigneeNameInfo);

			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, MandatoryValidation.YouHaveNotEntered + " a Consignee Country/Region.");

			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Uruguay;
			AssertNoNotifications(bill.ABL_RN_NKConsigneeCountryInfo);
		}

		public void TestNotifyPartyMandatoryData()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_NotifyPartyName = ZString.Empty;
			AssertHasMessageError(bill.ABL_NotifyPartyNameInfo, MandatoryValidation.YouHaveNotEntered + " a Notify Party Name.");

			bill.ABL_NotifyPartyName = "Notify Party Name";
			AssertNoNotifications(bill.ABL_NotifyPartyNameInfo);

			bill.ABL_NotifyPartyStreet1 = ZString.Empty;
			AssertHasMessageError(bill.ABL_NotifyPartyStreet1Info, MandatoryValidation.YouHaveNotEntered + " a Notify Party Street 1.");

			bill.ABL_NotifyPartyStreet1 = "Notify Party Street";
			AssertNoNotifications(bill.ABL_NotifyPartyStreet1Info);

			bill.ABL_NotifyPartyPhone = ZString.Empty;
			AssertHasMessageError(bill.ABL_NotifyPartyPhoneInfo, MandatoryValidation.YouHaveNotEntered + " a Notify Party Phone.");

			bill.ABL_NotifyPartyPhone = "12345123";
			AssertNoNotifications(bill.ABL_NotifyPartyPhoneInfo);
		}

		public void TestRUT()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			bill.ABL_ConsigneeRegNo = "216714840016";
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);
			bill.ABL_ConsigneeRegNo = "12345678901";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "The RUT code should be: 12 digits only");
			bill.ABL_ConsigneeRegNo = "21100-420012";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "The RUT code should be: 12 digits only");
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "You have not entered a value.");
			bill.ABL_ConsigneeRegNo = "663689171275";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "The registration number entered is not valid. The last character (check digit) is incorrect.");
		}

		public void TestBillNumber()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_BillNumber = ZString.Empty;
			AssertHasMessageError(bill.ABL_BillNumberInfo, MandatoryValidation.YouHaveNotEntered + " a Bill Number.");
		}

		public void TestPrepaidCollect()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_PrepaidCollect = ZString.Empty;

			AssertHasMessageError(bill.ABL_PrepaidCollectInfo, MandatoryValidation.YouHaveNotEntered + " a Prepaid/Collect.");
		}

		public void TestVolumeUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_Volume = 5;
			bill.ABL_VolumeUQ = ZString.Empty;

			AssertHasMessageError(bill.ABL_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered + " a Volume Unit (on Bill).");
		}

		public void TestGrossWeightIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 4;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			AssertHasMessageError(bill.ABL_GrossWeightInfo, "Weight of packages (4) is not equal to manifest Gross Weight (5) in (KG)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Weight = 3000;
			pack2.APA_WeightUQ = Core.Constants.Weight.Grams;

			bill.ABL_GrossWeight = 7;

			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
		}

		public void TestVolumeIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 4;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			AssertHasMessageError(bill.ABL_VolumeInfo, "Volume of packages (4) is not equal to manifest Volume (5) in (L)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Volume = 4000;
			pack2.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			bill.ABL_Volume = 8;

			AssertNoMessageErrors(bill.ABL_VolumeInfo);
		}
	}
}
