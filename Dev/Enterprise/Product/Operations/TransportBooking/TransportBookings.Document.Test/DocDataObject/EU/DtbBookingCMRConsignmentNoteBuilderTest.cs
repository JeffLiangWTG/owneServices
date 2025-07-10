using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Document.Testing
{
	sealed partial class DtbBookingCMRConsignmentNoteBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSupplierAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "SupplierAddress should be empty at this stage.");

			var pickupAddress = Factory.New<OrgHeader>();
			pickupAddress.OH_FullName = "MAERSK Name";
			pickupAddress.OH_RL_NKClosestPort = "AUSYD";
			pickupAddress.MainAddress.Address1 = "Address 1 Unit 13";
			pickupAddress.MainAddress.Address2 = "ADDRESS 24 Lost Lane";
			pickupAddress.MainAddress.City = "Sydney";
			pickupAddress.MainAddress.Postcode = "2229";
			pickupAddress.MainAddress.State = "NSW";
			pickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			pickup.Address.E2_OA_Address  = pickupAddress.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"MAERSK NAME{System.Environment.NewLine}ADDRESS 1 UNIT 13{System.Environment.NewLine}ADDRESS 24 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "SupplierAddress should be filled correctly.");

			SetOverrideAddress(pickup.Address);

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "SupplierAddress when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestImporterAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "ImporterAddress should be empty at this stage.");

			var deliveryAddress = Factory.New<OrgHeader>();
			deliveryAddress.OH_FullName = "MAERSK";
			deliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			deliveryAddress.MainAddress.Address1 = "Unit 13";
			deliveryAddress.MainAddress.Address2 = "4 Lost Lane";
			deliveryAddress.MainAddress.City = "Sydney";
			deliveryAddress.MainAddress.Postcode = "2229";
			deliveryAddress.MainAddress.State = "NSW";
			deliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			delivery.Address.E2_OA_Address = deliveryAddress.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"MAERSK{System.Environment.NewLine}UNIT 13{System.Environment.NewLine}4 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "ImporterAddress should be filled correctly.");

			SetOverrideAddress(delivery.Address);

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "ImporterAddress when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestInternationalConsignmentNote()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.InternationalConsignmentNote, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "InternationalConsignmentNote should be empty.");
		}

		[ExpectNoExceptions]
		public void TestPlaceOfDelivery()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "PlaceOfDelivery should be empty at this stage.");

			var deliveryAddress = Factory.New<OrgHeader>();
			deliveryAddress.OH_FullName = "MAERSK";
			deliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			deliveryAddress.MainAddress.Address1 = "Unit 13";
			deliveryAddress.MainAddress.Address2 = "4 Lost Lane";
			deliveryAddress.MainAddress.City = "Como";
			deliveryAddress.MainAddress.Postcode = "2229";
			deliveryAddress.MainAddress.State = "NSW";
			deliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			delivery.Address.E2_OA_Address = deliveryAddress.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("2229 COMO AU").Using(CustomComparers.TypeComparison), "PlaceOfDelivery should be filled with the delivery's address.");

			SetOverrideAddress(delivery.Address);

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("1234 BRISBANE HK").Using(CustomComparers.TypeComparison), "PlaceOfDelivery when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestGoodsTakingOverPlaceAndDate()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be empty when no address is provided.");

			var today = ZDate.Today;
			var tomorrow = today.AddDays(1);
			var tomorrowAsString = tomorrow.ToString("dd/MM/yyyy");

			var pickupAddress = Factory.New<OrgHeader>();
			pickupAddress.OH_FullName = "MAERSK";
			pickupAddress.OH_RL_NKClosestPort = "AUSYD";
			pickupAddress.MainAddress.Address1 = "Unit 13";
			pickupAddress.MainAddress.Address2 = "4 Lost Lane";
			pickupAddress.MainAddress.City = "Miranda";
			pickupAddress.MainAddress.Postcode = "2229";
			pickupAddress.MainAddress.State = "NSW";
			pickupAddress.MainAddress.OA_RN_NKCountryCode = "SG";

			pickup.Address.E2_OA_Address = pickupAddress.MainAddress.PK;
			var pic1 = pickup.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			pic1.KK_Estimated = today;
			var pic2 = pickup.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			pic2.KK_Estimated = tomorrow;
			var pic3 = pickup.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			pic3.KK_Estimated = tomorrow.AddDays(1);

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"MIRANDA SG {tomorrowAsString}").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be filled with the pickup address and Pic. Estimated.");

			SetOverrideAddress(pickup.Address);

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"BRISBANE HK {tomorrowAsString}").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver should be filled with the pickup address and Pic. Estimated when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestGoodsAttachedDocuments()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.GoodsAttachedDocuments, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "GoodsAttachedDocuments should be empty.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9WithOnePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_MarksAndNumbers = "MNMP1";
			package1.KP_PackageQty = 2;
			package1.KP_F3_NKPackType = PkgUnit.Pallet;
			package1.KP_GoodsDescription = "Goods description 1";
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_MarksAndNumbers = "MNMP2";
			package2.KP_PackageQty = 3;
			package2.KP_F3_NKPackType = PkgUnit.Package;
			package2.KP_GoodsDescription = "Goods description 2";
			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_MarksAndNumbers = "MNMD1";
			package3.KP_PackageQty = 4;
			package3.KP_F3_NKPackType = PkgUnit.Piece;
			package3.KP_GoodsDescription = "Goods description 3";
			var packageDivot3 = delivery.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo($"MNMP1; 2 PLT; Goods description 1{System.Environment.NewLine}MNMP2; 3 PKG; Goods description 2").Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 should be filled with the packages that assigned to pickup.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9WithOnePickupMultipleDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_MarksAndNumbers = "MNMD1";
			package1.KP_PackageQty = 2;
			package1.KP_F3_NKPackType = PkgUnit.Pallet;
			package1.KP_GoodsDescription = "Goods description 1";
			var packageDivot1 = delivery.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_MarksAndNumbers = "MNMD2";
			package2.KP_PackageQty = 3;
			package2.KP_F3_NKPackType = PkgUnit.Package;
			package2.KP_GoodsDescription = "Goods description 2";
			var packageDivot2 = delivery.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var delivery1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_MarksAndNumbers = "MNMD3";
			package3.KP_PackageQty = 5;
			package3.KP_F3_NKPackType = PkgUnit.Piece;
			package3.KP_GoodsDescription = "Goods description 3";
			var packageDivot3 = delivery1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			package4.KP_MarksAndNumbers = "MNMP1";
			package4.KP_PackageQty = 4;
			package4.KP_F3_NKPackType = PkgUnit.Piece;
			package4.KP_GoodsDescription = "Goods description 4";
			var packageDivot4 = pickup.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo($"MNMD1; 2 PLT; Goods description 1{System.Environment.NewLine}MNMD2; 3 PKG; Goods description 2").Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 should be filled with the packages that assigned to delivery.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9WithMultiplePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_MarksAndNumbers = "MNMP1";
			package1.KP_PackageQty = 2;
			package1.KP_F3_NKPackType = PkgUnit.Pallet;
			package1.KP_GoodsDescription = "Goods description 1";
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_MarksAndNumbers = "MNMP2";
			package2.KP_PackageQty = 3;
			package2.KP_F3_NKPackType = PkgUnit.Package;
			package2.KP_GoodsDescription = "Goods description 2";
			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var pickup1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_MarksAndNumbers = "MNMP3";
			package3.KP_PackageQty = 5;
			package3.KP_F3_NKPackType = PkgUnit.Piece;
			package3.KP_GoodsDescription = "Goods description 3";
			var packageDivot3 = pickup1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			package4.KP_MarksAndNumbers = "MNMD1";
			package4.KP_PackageQty = 4;
			package4.KP_F3_NKPackType = PkgUnit.Piece;
			package4.KP_GoodsDescription = "Goods description 4";
			var packageDivot4 = delivery.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo($"MNMP1; 2 PLT; Goods description 1{System.Environment.NewLine}MNMP2; 3 PKG; Goods description 2").Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 should be filled with the packages that assigned to pickup.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsTariffCodeBox10()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsTariffCodeBox10, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "LineDetailsTariffCodeBox10 should be empty.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11WithOnePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_Weight = 10;
			package1.KP_WeightUQ = Weight.Kilograms;
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_Weight = 100;
			package2.KP_WeightUQ = Weight.Grams;

			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_Weight = 0.1;
			package3.KP_WeightUQ = Weight.Kilograms;
			var packageDivot3 = delivery.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			var expectedLineDescription = $"10.00    {System.Environment.NewLine}0.1     ";
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 should provide details from the pickup instruction in kilograms (KG).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11WithOnePickupMultipleDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_Weight = 10;
			package1.KP_WeightUQ = Weight.Kilograms;
			var packageDivot1 = delivery.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_Weight = 100;
			package2.KP_WeightUQ = Weight.Grams;
			var packageDivot2 = delivery.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var delivery1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_Weight = 0.2;
			package3.KP_WeightUQ = Weight.Kilograms;
			var packageDivot3 = delivery1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			package4.KP_Weight = 0.1;
			package4.KP_WeightUQ = Weight.Kilograms;
			var packageDivot4 = pickup.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			var expectedLineDescription = $"10.00    {System.Environment.NewLine}0.1     ";
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 should provide details from the delivery instruction in kilograms (KG).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11WithMultiplePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_Weight = 10;
			package1.KP_WeightUQ = Weight.Kilograms;
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_Weight = 100;
			package2.KP_WeightUQ = Weight.Grams;
			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var pickup1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_Weight = 0.2;
			package3.KP_WeightUQ = Weight.Kilograms;
			var packageDivot3 = pickup1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			package4.KP_Weight = 0.1;
			package4.KP_WeightUQ = Weight.Kilograms;
			var packageDivot4 = delivery.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			var expectedLineDescription = $"10.00    {System.Environment.NewLine}0.1     ";
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 should provide details from the delivery instruction in kilograms (KG).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12WithOnePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_Volume = 2;
			package1.KP_VolumeUQ = Volume.CubicMetres;
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_Volume = 100;
			package2.KP_VolumeUQ = Volume.Litre;

			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_Volume = 0.1;
			package3.KP_VolumeUQ = Volume.CubicYards;
			var packageDivot3 = delivery.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			var expectedLineDescription = $"2.00    {System.Environment.NewLine}0.1     ";
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3 should provide details from the pickup instruction in cubic meters (M3).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12WithOnePickupMultipleDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_Volume = 2;
			package1.KP_VolumeUQ = Volume.CubicMetres;
			var packageDivot1 = delivery.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_Volume = 100;
			package2.KP_VolumeUQ = Volume.Litre;
			var packageDivot2 = delivery.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var delivery1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_Volume = 0.2;
			package3.KP_VolumeUQ = Volume.CubicYards;
			var packageDivot3 = delivery1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			package4.KP_Volume = 0.1;
			package4.KP_VolumeUQ = Volume.CubicYards;
			var packageDivot4 = pickup.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			var expectedLineDescription = $"2.00    {System.Environment.NewLine}0.1     ";
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 should provide details from the delivery instruction in cubic meters (M3).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12WithMultiplePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			package1.KP_Volume = 2;
			package1.KP_VolumeUQ = Volume.CubicMetres;
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			package2.KP_Volume = 100;
			package2.KP_VolumeUQ = Volume.Litre;
			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var pickup1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_Volume = 0.2;
			package3.KP_VolumeUQ = Volume.CubicYards;
			var packageDivot3 = pickup1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			package4.KP_Volume = 0.1;
			package4.KP_VolumeUQ = Volume.CubicYards;
			var packageDivot4 = delivery.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			var expectedLineDescription = $"2.00    {System.Environment.NewLine}0.1     ";
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 should provide details from the delivery instruction in cubic meters (M3).");
		}

		[ExpectNoExceptions]
		public void TestSendersInstructions()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(ZString.Empty), "When no HandlingInstructions or DangerousGoodsAdditionalHandlingInformation is provided, an empty string is expected.");

			pickup.KN_ServiceInstruction = "PICKUP Instructions";
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "We have Handling Instructions");
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "We have Dangerous Goods Additional Handling Information");

			cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			var expectedLineDescription = $"PICKUP INSTRUCTIONS{System.Environment.NewLine}WE HAVE HANDLING INSTRUCTIONS{System.Environment.NewLine}WE HAVE DANGEROUS GOODS ADDITIONAL HANDLING INFORM";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "When Handling Instructions or Dangerous Goods Additional Handling Information is provided, it should be available.");
		}

		[ExpectNoExceptions]
		public void TestCarrierAddress()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "The Best Carrier Company";
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			carrierAddress.OA_Address1 = "109 Main St";
			carrierAddress.City = "Sydney";
			carrierAddress.OA_RN_NKCountryCode = "AU";
			booking.Address.E2_OA_Address = carrierAddress.PK;

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CarrierAddress, Is.EqualTo($"THE BEST CARRIER COMPANY{System.Environment.NewLine}109 MAIN ST{System.Environment.NewLine}SYDNEY - AUSTRALIA").Using(CustomComparers.TypeComparison), "Carrier address should be filled correctly.");
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.JobNumber, Is.EqualTo(booking.KM_JobID).Using(CustomComparers.TypeComparison), "JobNumber should be the transport booking number.");
		}

		[ExpectNoExceptions]
		[TestDate(2025, 2, 04)]
		public void TestEstablishedInDate()
		{
			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.EstablishedInDate, Is.EqualTo("04 Feb 2025").Using(CustomComparers.TypeComparison), "EstablishedInDate should be in the format dd MMM yyyy.");
		}

		[ExpectNoExceptions]
		public void TestEstablishedInPlace()
		{
			var currentBranchPort = ZString.Empty;
			try
			{
				currentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "SGSIN";
				var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);

				NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.EstablishedInPlace, Is.EqualTo("Singapore").Using(CustomComparers.TypeComparison), "EstablishedInPlace should be the current branch's home port.");
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchPort;
			}
		}

		[ExpectNoExceptions]
		public void TestDangerousGoodsWithOnePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var dg = package2.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1A";

			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot3 = delivery.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var dg1 = package3.UNDGs.AddNew();
			dg1.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg1.DI_IMOClass = "1.1B";

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo("1.1A").Using(CustomComparers.TypeComparison), "DangerousGoodsClass should be filled with the value from the pickup.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo("0113").Using(CustomComparers.TypeComparison), "DangerousGoodsNumber should be filled with the value from the pickup.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter should be empty.");
		}

		[ExpectNoExceptions]
		public void TestDangerousGoodsWithOnePickupMultipleDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot1 = delivery.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot2 = delivery.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var dg = package2.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1A";

			var delivery1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot3 = delivery1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var dg1 = package3.UNDGs.AddNew();
			dg1.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg1.DI_IMOClass = "1.1B";

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot4 = pickup.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var dg2 = package4.UNDGs.AddNew();
			dg2.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg2.DI_IMOClass = "1.1C";

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo("1.1A").Using(CustomComparers.TypeComparison), "DangerousGoodsClass should be filled with the value from the delivery.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo("0113").Using(CustomComparers.TypeComparison), "DangerousGoodsNumber should be filled with the value from the delivery.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter should be empty.");
		}

		[ExpectNoExceptions]
		public void TestDangerousGoodsWithMultiplePickupOneDelivery()
		{
			var package1 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot1 = pickup.PackageDivots.AddNew();
			packageDivot1.KD_KP_Package = package1.PK;

			var package2 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot2 = pickup.PackageDivots.AddNew();
			packageDivot2.KD_KP_Package = package2.PK;

			var dg = package2.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1A";

			var pickup1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var package3 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot3 = pickup1.PackageDivots.AddNew();
			packageDivot3.KD_KP_Package = package3.PK;

			var dg1 = package3.UNDGs.AddNew();
			dg1.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg1.DI_IMOClass = "1.1B";

			var package4 = Factory.NewWithValidTestData<PkgPackage>();
			var packageDivot4 = delivery.PackageDivots.AddNew();
			packageDivot4.KD_KP_Package = package4.PK;

			var dg2 = package4.UNDGs.AddNew();
			dg2.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg2.DI_IMOClass = "1.1C";

			var cmrConsignmentNoteDocDataObject = GetBookingCMRConsignmentNoteDocData(booking);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo("1.1A").Using(CustomComparers.TypeComparison), "DangerousGoodsClass should be filled with the value from the pickup.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo("0113").Using(CustomComparers.TypeComparison), "DangerousGoodsNumber should be filled with the value from the pickup.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter should be empty.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			booking = Factory.NewWithValidTestData<DtbBooking>();
			pickup = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			delivery = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
		}

		CMRConsignmentNoteDocDataObject GetBookingCMRConsignmentNoteDocData(DtbBooking booking)
		{
			var parameters = new DocDataObjectParameters("Test", "Test", new DtbBookingInstruction[] { pickup, delivery });
			var bookingCMRConsignmentNoteBuilder = new DtbBookingCMRConsignmentNoteBuilder(booking, parameters);
			return bookingCMRConsignmentNoteBuilder.Build().CMRConsignmentNoteDocuments.First();
		}

		void SetOverrideAddress(JobDocAddress jobDocAddress)
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "DHL";
			jobDocAddress.E2_Postcode = "1234";
			jobDocAddress.E2_City = "Brisbane";
			jobDocAddress.E2_RN_NKCountryCode = "HK";
			jobDocAddress.E2_Address1 = "Unit 13-1";
			jobDocAddress.E2_Address2 = "4 Lost Lane-1";
		}

		DtbBooking booking;
		DtbBookingInstruction pickup;
		DtbBookingInstruction delivery;
	}
}
