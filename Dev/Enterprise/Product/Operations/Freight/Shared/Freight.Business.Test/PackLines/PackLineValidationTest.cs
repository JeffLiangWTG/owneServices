using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class PackLineValidationTest : BaseFreightTest
	{
		public void TestValidateJL_ActualVolume()
		{
			PackLine.JL_Height = 1.011m;
			PackLine.JL_Width = 1.012m;
			PackLine.JL_Length = 1.013m;
			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			PackLine.JL_PackageCount = 2;
			PackLine.JL_ActualVolume = ZArchitecture.Core.Utilities.Round(PackLine.JL_Height * PackLine.JL_Width * PackLine.JL_Length * PackLine.JL_PackageCount, JobPackLinesSchema.JL_ActualVolume.Scale);
			PackLine.Validation.ValidateJL_ActualVolume();
			AssertEquals("JL_ActualVolumeInfo shouldn't have any warnings", false, PackLine.JL_ActualVolumeInfo.HasWarnings());

			PackLine.JL_ActualVolume = -2;
			AssertHasError(PackLine.JL_ActualVolumeInfo, "Please enter a 'Volume' greater than or equal to 0.");
		}

		public void TestValidateNonNegativeJL_ActualVolume()
		{
			PackLine.JL_ActualVolume = -1;
			AssertHasError(PackLine.JL_ActualVolumeInfo, "Please enter a 'Volume' greater than or equal to 0.");

			PackLine.JL_ActualWeight = 0;
			AssertNoWarnings(PackLine.JL_ActualVolumeInfo);

			PackLine.JL_ActualWeight = 1;
			AssertNoWarnings(PackLine.JL_ActualVolumeInfo);
		}

		public void TestValidateJL_VolumeUnit()
		{
			PackLine.JL_ActualVolumeUQ = string.Empty;
			PackLine.JL_ActualVolume = 0;
			AssertNoErrors(PackLine.JL_ActualVolumeUQInfo);

			PackLine.JL_ActualVolumeUQ = "XX";
			AssertHasError(PackLine.JL_ActualVolumeUQInfo, "Enter a valid " + PackLine.JL_ActualVolumeUQInfo.Description + ".");

			PackLine.JL_ActualVolumeUQ = string.Empty;

			PackLine.JL_ActualVolume = 1;
			AssertHasError(PackLine.JL_ActualVolumeUQInfo, "Please enter an UV.");

			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			AssertNoErrors(PackLine.JL_ActualVolumeUQInfo);
		}

		public void TestValidateJL_Height()
		{
			PackLine.JL_Height = -1;
			AssertHasError(PackLine.JL_HeightInfo, "Please enter a 'Height' greater than or equal to 0.");

			PackLine.JL_Height = 0;
			AssertNoWarnings(PackLine.JL_HeightInfo);

			PackLine.JL_Height = 1;
			AssertNoWarnings(PackLine.JL_HeightInfo);
		}

		public void TestValidateJL_Width()
		{
			PackLine.JL_Width = -1;
			AssertHasError(PackLine.JL_WidthInfo, "Please enter a 'Width' greater than or equal to 0.");

			PackLine.JL_Width = 0;
			AssertNoWarnings(PackLine.JL_WidthInfo);

			PackLine.JL_Width = 1;
			AssertNoWarnings(PackLine.JL_WidthInfo);
		}

		public void TestValidateJL_Length()
		{
			PackLine.JL_Length = -1;
			AssertHasError(PackLine.JL_LengthInfo, "Please enter a 'Length' greater than or equal to 0.");

			PackLine.JL_Length = 0;
			AssertNoWarnings(PackLine.JL_LengthInfo);

			PackLine.JL_Length = 1;
			AssertNoWarnings(PackLine.JL_LengthInfo);
		}

		public void TestValidateJL_DimensionsUnit()
		{
			PackLine.JL_UnitOfDimension = string.Empty;
			PackLine.JL_Height = 0;
			PackLine.JL_Width = 0;
			PackLine.JL_Length = 0;
			AssertNoErrors(PackLine.JL_UnitOfDimensionInfo);

			PackLine.JL_UnitOfDimension = "XX";
			AssertHasError(PackLine.JL_UnitOfDimensionInfo, "Enter a valid " + PackLine.JL_UnitOfDimensionInfo.Description + ".");

			PackLine.JL_UnitOfDimension = string.Empty;

			PackLine.JL_Height = 1;
			PackLine.JL_Width = 1;
			PackLine.JL_Length = 1;
			AssertHasError(PackLine.JL_UnitOfDimensionInfo, "Please enter an UD.");

			PackLine.JL_Height = 0;
			PackLine.JL_Width = 1;
			PackLine.JL_Length = 0;
			AssertHasError(PackLine.JL_UnitOfDimensionInfo, "Please enter an UD.");

			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			AssertNoErrors(PackLine.JL_UnitOfDimensionInfo);
		}

		public void TestValidateJL_ActualWeight()
		{
			PackLine.JL_ActualWeight = -1;
			AssertHasError(PackLine.JL_ActualWeightInfo, "Please enter a 'Weight' greater than or equal to 0.");

			PackLine.JL_ActualWeight = 0;
			AssertNoWarnings(PackLine.JL_ActualWeightInfo);

			PackLine.JL_ActualWeight = 1;
			AssertNoWarnings(PackLine.JL_ActualWeightInfo);
		}

		public void TestValidateJL_WeightUnit()
		{
			PackLine.JL_ActualWeightUQ = string.Empty;
			PackLine.JL_ActualWeight = 0;
			AssertNoErrors(PackLine.JL_ActualWeightUQInfo);

			PackLine.JL_ActualWeightUQ = "XX";
			AssertHasError(PackLine.JL_ActualWeightUQInfo, "Enter a valid " + PackLine.JL_ActualWeightUQInfo.Description + ".");

			PackLine.JL_ActualWeightUQ = string.Empty;

			PackLine.JL_ActualWeight = 1;
			AssertHasError(PackLine.JL_ActualWeightUQInfo, "Please enter an UW.");

			PackLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			AssertNoErrors(PackLine.JL_ActualWeightUQInfo);
		}

		public void TestValidateJL_OutturnedVolume()
		{
			PackLine.JL_OutturnedHeight = 1.011m;
			PackLine.JL_OutturnedWidth = 1.012m;
			PackLine.JL_OutturnedLength = 1.013m;
			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			PackLine.JL_Outturn = 2;
			AssertNoWarnings(PackLine.JL_OutturnedVolumeInfo);

			PackLine.JL_OutturnedVolume = 6;
			AssertHasWarning(PackLine.JL_OutturnedVolumeInfo, "The outturned volume does not match the volume calculated by the length x width x height x Outturned packages");

			PackLine.JL_Outturn = 3;
			AssertNoWarnings(PackLine.JL_OutturnedVolumeInfo);

			PackLine.JL_OutturnedVolume = -2;
			AssertHasError(PackLine.JL_OutturnedVolumeInfo, "Please enter an 'Outturned Volume' greater than or equal to 0.");

			PackLine.JL_OutturnedVolume = 3.109m;
			AssertNoWarnings(PackLine.JL_OutturnedVolumeInfo);
		}

		public void TestValidateNonNegativeJL_OutturnedVolume()
		{
			PackLine.JL_OutturnedVolume = -1;
			AssertHasError(PackLine.JL_OutturnedVolumeInfo, "Please enter an 'Outturned Volume' greater than or equal to 0.");

			PackLine.JL_OutturnedVolume = 0;
			AssertNoWarnings(PackLine.JL_OutturnedVolumeInfo);

			PackLine.JL_OutturnedVolume = 1;
			AssertNoWarnings(PackLine.JL_OutturnedVolumeInfo);
		}

		public void TestValidateJL_OutturnedHeight()
		{
			PackLine.JL_Outturn = 2;
			AssertNoWarnings(PackLine.JL_OutturnedHeightInfo);

			PackLine.JL_OutturnedHeight = -1;
			AssertHasError(PackLine.JL_OutturnedHeightInfo, "Please enter an 'Outturn Height' greater than or equal to 0.");

			PackLine.JL_OutturnedHeight = 0;
			AssertNoWarnings(PackLine.JL_OutturnedHeightInfo);

			PackLine.JL_OutturnedHeight = 1;
			AssertNoWarnings(PackLine.JL_OutturnedHeightInfo);
		}

		public void TestValidateJL_OutturnedLength()
		{
			PackLine.JL_Outturn = 2;
			AssertNoWarnings(PackLine.JL_OutturnedLengthInfo);

			PackLine.JL_OutturnedLength = -1;
			AssertHasError(PackLine.JL_OutturnedLengthInfo, "Please enter an 'Outturn Length' greater than or equal to 0.");

			PackLine.JL_OutturnedLength = 0;
			AssertNoWarnings(PackLine.JL_OutturnedLengthInfo);

			PackLine.JL_OutturnedLength = 1;
			AssertNoWarnings(PackLine.JL_OutturnedLengthInfo);
		}

		public void TestValidateJL_OutturnedWidth()
		{
			PackLine.JL_Outturn = 2;
			AssertNoWarnings(PackLine.JL_OutturnedWidthInfo);

			PackLine.JL_OutturnedWidth = -1;
			AssertHasError(PackLine.JL_OutturnedWidthInfo, "Please enter an 'Outturn Width' greater than or equal to 0.");

			PackLine.JL_OutturnedWidth = 0;
			AssertNoWarnings(PackLine.JL_OutturnedWidthInfo);

			PackLine.JL_OutturnedWidth = 1;
			AssertNoWarnings(PackLine.JL_OutturnedWidthInfo);
		}

		public void TestValidateJL_OutturnedWeight()
		{
			PackLine.JL_Outturn = 2;
			AssertNoWarnings(PackLine.JL_OutturnedWeightInfo);

			PackLine.JL_OutturnedWeight = -1;
			AssertHasError(PackLine.JL_OutturnedWeightInfo, "Please enter an 'Outturned Weight' greater than or equal to 0.");

			PackLine.JL_OutturnedWeight = 0;
			AssertNoWarnings(PackLine.JL_OutturnedWeightInfo);

			PackLine.JL_OutturnedWeight = 1;
			AssertNoWarnings(PackLine.JL_OutturnedWeightInfo);
		}

		public void TestValidateJL_RH_NKCommodityCode()
		{
			RefCommodityCode commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			PackLine.JL_RH_NKCommodityCode = commodity.RH_Code;
			AssertNoWarnings(PackLine.JL_RH_NKCommodityCodeInfo);

			PackLine.JL_RH_NKCommodityCode = "ZZZ";
			AssertHasError(PackLine.JL_RH_NKCommodityCodeInfo, "Enter a valid " + PackLine.JL_RH_NKCommodityCodeInfo.Description + ".");
		}

		public void TestValidateJL_F3_NKPackTypeIsMandatory()
		{
			PackLine.JL_F3_NKPackType = "";
			AssertHasErrors("JL_F3_NKPackType is mandatory", PackLine.JL_F3_NKPackTypeInfo);
			PackLine.JL_F3_NKPackType = "PLT";
			AssertNoErrors("JL_F3_NKPackType must not have errors", PackLine.JL_F3_NKPackTypeInfo);
		}

		public void TestCheckJL_PackageCount()
		{
			PackLine.JL_PackageCount = 2;
			AssertNoWarnings(PackLine.JL_PackageCountInfo);

			PackLine.JL_PackageCount = -1;
			AssertHasError(PackLine.JL_PackageCountInfo, "Please enter a 'Packs' greater than or equal to 0.");

			PackLine.JL_PackageCount = 2;
			AssertNoWarnings(PackLine.JL_PackageCountInfo);
		}

		public void TestCheckJL_RefNumber()
		{
			Shipment.JS_TransportMode = "SEA";
			Shipment.JS_PackingMode = "FCL";
			PackLine.JL_PackageCount = 1;

			PackLine.JL_RefNumber = "RefNumber12345678";
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);
			PackLine.JL_RefNumber = "RefNumber123456789";
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);

			PackLine.JL_PackageCount = 2;
			PackLine.JL_RefNumber = string.Empty;
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);
			PackLine.JL_RefNumber = "RefNumber";
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);
			PackLine.JL_PackageCount = 1;
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);

			Shipment.JS_PackingMode = "ROR";
			PackLine.JL_PackageCount = 1;

			PackLine.JL_RefNumber = "RefNumber12345678";
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);
			PackLine.JL_RefNumber = "RefNumber123456789";
			AssertHasError(PackLine.JL_RefNumberInfo, "VIN number can be no more than 17 characters long.");

			PackLine.JL_PackageCount = 2;
			PackLine.JL_RefNumber = string.Empty;
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);
			PackLine.JL_RefNumber = "RefNumber";
			AssertHasError(PackLine.JL_RefNumberInfo, "You can't enter a VIN if the package count is greater than one.");
			PackLine.JL_PackageCount = 1;
			AssertNoErrors("JL_RefNumber must not have errors", PackLine.JL_RefNumberInfo);
		}

		public void TestCheckJL_VehicleTransmission()
		{
			PackLine.JL_VehicleTransmission = Constants.VehicleTransmissionType.Automatic;
			AssertNoErrors(PackLine.JL_VehicleTransmissionInfo);

			PackLine.JL_VehicleTransmission = string.Empty;
			AssertNoErrors(PackLine.JL_VehicleTransmissionInfo);

			PackLine.JL_VehicleTransmission = "XX";
			AssertHasErrors(PackLine.JL_VehicleTransmissionInfo);
		}

		public void TestCheckJL_OA_LastKnownTransitWarehouseAddress_NoShipment()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			packLine.JL_JS = ZGuid.Empty;
			var pickupCFS = Factory.NewWithValidTestData<OrgHeader>();
			packLine.JL_OA_LastKnownTransitWarehouseAddress = pickupCFS.MainAddress.PK;
			AssertNoExceptionThrown(packLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress);
		}

		public void TestJL_OA_LastKnownTransitWarehouseAddress_ZAddress()
		{
			var validOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			PackLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.IsOrgVisible = true;
			PackLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = validOrganisation.PK;
			PackLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.AddressFK = ZGuid.Empty;
			AssertHasError(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo, "Enter a valid Last Known TW Address.");
		}

		public void TestCheckJL_OA_LastKnownTransitWarehouseAddress()
		{
			var warning = "Transit Warehouse/CFS Organization selected does not match any of CFS’s on the Shipment or Consol.";

			var consol = Shipment.Consols.AddNew();
			var pickupCFS = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCFS = Factory.NewWithValidTestData<OrgHeader>();
			var departureCFS = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalCFS = Factory.NewWithValidTestData<OrgHeader>();
			var other = Factory.NewWithValidTestData<OrgHeader>();

			Shipment.JS_OA_ExportReceivingDepot = pickupCFS.MainAddress.PK;
			Shipment.JS_OA_ImportReleaseDepot = deliveryCFS.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = departureCFS.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = arrivalCFS.MainAddress.PK;

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = other.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertHasWarning(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo, warning);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = pickupCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = deliveryCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = departureCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = arrivalCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = ZGuid.Invalid;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertHasError(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo, "Enter a valid Last Known TW Address.");
		}

		public void TestCheckJL_OA_LastKnownTransitWarehouseAddressForAddress()
		{
			var warning = "This Organization's Warehouse, XYZ, has different address.";

			var consol = Shipment.Consols.AddNew();
			var pickupCFS = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFS.OH_Code = "XYZ";
			pickupCFS.Addresses.AddNew();
			var deliveryCFS = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFS.OH_Code = "XYZ";
			deliveryCFS.Addresses.AddNew();
			var departureCFS = Factory.NewWithValidTestData<OrgHeader>();
			departureCFS.OH_Code = "XYZ";
			departureCFS.Addresses.AddNew();
			var arrivalCFS = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCFS.OH_Code = "XYZ";
			arrivalCFS.Addresses.AddNew();

			Shipment.JS_OA_ExportReceivingDepot = pickupCFS.MainAddress.PK;
			Shipment.JS_OA_ImportReleaseDepot = deliveryCFS.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = departureCFS.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = arrivalCFS.MainAddress.PK;

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = pickupCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);
			PackLine.JL_OA_LastKnownTransitWarehouseAddress = pickupCFS.Addresses[1].PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertHasWarning(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo, warning);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = deliveryCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);
			PackLine.JL_OA_LastKnownTransitWarehouseAddress = deliveryCFS.Addresses[1].PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertHasWarning(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo, warning);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = departureCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);
			PackLine.JL_OA_LastKnownTransitWarehouseAddress = departureCFS.Addresses[1].PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertHasWarning(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo, warning);

			PackLine.JL_OA_LastKnownTransitWarehouseAddress = arrivalCFS.MainAddress.PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertNoNotifications(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo);
			PackLine.JL_OA_LastKnownTransitWarehouseAddress = arrivalCFS.Addresses[1].PK;
			PackLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();
			AssertHasWarning(PackLine.JL_OA_LastKnownTransitWarehouseAddressInfo, warning);
		}

		public void TestValidateJL_LastKnownTransitWarehouseStatus()
		{
			PackLine.JL_LastKnownTransitWarehouseStatus = string.Empty;
			AssertNoErrors(PackLine.JL_LastKnownTransitWarehouseStatusInfo);

			PackLine.JL_LastKnownTransitWarehouseStatus = "XXX";
			AssertHasError(PackLine.JL_LastKnownTransitWarehouseStatusInfo, "Enter a valid " + PackLine.JL_LastKnownTransitWarehouseStatusInfo.Description + ".");
		}

		[TestDate(2021, 1, 2, 3, 4, 5)]
		public void TestCheckJL_LastKnownTransitWarehouseStatusDateTime()
		{
			var warning = "Last Known TW Date should only allow current or past date.";
			PackLine.Validation.ValidateJL_LastKnownTransitWarehouseStatusDateTime();
			AssertNoNotifications(PackLine.JL_LastKnownTransitWarehouseStatusDateTimeInfo);

			PackLine.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2021, 1, 2, 3, 4, 5);
			PackLine.Validation.ValidateJL_LastKnownTransitWarehouseStatusDateTime();
			AssertNoNotifications(PackLine.JL_LastKnownTransitWarehouseStatusDateTimeInfo);

			PackLine.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2021, 1, 2, 4, 4, 5);
			PackLine.Validation.ValidateJL_LastKnownTransitWarehouseStatusDateTime();
			AssertNoNotifications(PackLine.JL_LastKnownTransitWarehouseStatusDateTimeInfo);

			PackLine.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2021, 1, 2, 2, 4, 5);
			PackLine.Validation.ValidateJL_LastKnownTransitWarehouseStatusDateTime();
			AssertNoNotifications(PackLine.JL_LastKnownTransitWarehouseStatusDateTimeInfo);

			PackLine.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(2021, 1, 3, 3, 4, 5);
			PackLine.Validation.ValidateJL_LastKnownTransitWarehouseStatusDateTime();
			AssertHasWarning(PackLine.JL_LastKnownTransitWarehouseStatusDateTimeInfo, warning);
		}

		CommonShipment Shipment;
		PackLine PackLine;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = CommonShipment.New(Factory);
			PackLine = Shipment.OuterPackLines.AddNew();

			PackLine.JL_JS = Shipment.PK;
		}
	}
}
