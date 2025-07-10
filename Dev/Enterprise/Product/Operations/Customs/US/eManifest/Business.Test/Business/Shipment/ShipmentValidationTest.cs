using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.RefDbEntUS;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB0_RN_NKCountryOfExport()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_RN_NKCountryOfExportInfo, "??", Constants.CountryCodes.UnitedStates);
		}

		public void TestCheckB0_GoodsValue()
		{
			const string messageErrorFromat = "The shipment value should equal to the total commodities value.";
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(shipment.B0_BoardedQuantityInfo);

			shipment.Validation.ValidateB0_GoodsValue();
			AssertNoNotifications(shipment.B0_GoodsValueInfo);

			var commodity = shipment.Commodities.AddNew();
			commodity.BY_MonetaryValue = 123.0m;
			shipment.B0_GoodsValue = commodity.BY_MonetaryValue + 10m;

			var commodity2 = shipment.Commodities.AddNew();
			commodity2.BY_MonetaryValue = 10m;
			AssertHasMessageErrorContaining(shipment.B0_GoodsValueInfo, messageErrorFromat);

			shipment.B0_GoodsValue = commodity.BY_MonetaryValue + commodity2.BY_MonetaryValue;
			AssertNoMessageErrorContaining(shipment.B0_GoodsValueInfo, messageErrorFromat);
		}

		public void TestCheckB0_BoardedQuantity()
		{
			const string messageError = "The boarded quantity should be less than the shipment quantity.\r\nLeave zero if same as the shipment quantity.";
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(shipment.B0_BoardedQuantityInfo);

			shipment.Validation.ValidateB0_BoardedQuantity();
			AssertNoMessageError("No validation, manifest quantity not entered", shipment.B0_BoardedQuantityInfo, messageError);

			shipment.B0_ManifestQty = 2;
			shipment.Validation.ValidateB0_BoardedQuantity();
			AssertNoMessageError("No validation, boarded quantity not entered", shipment.B0_BoardedQuantityInfo, messageError);

			shipment.B0_BoardedQuantity = 2;
			AssertHasMessageError("Shoud be zero if equals to manifest quantity", shipment.B0_BoardedQuantityInfo, messageError);

			shipment.B0_BoardedQuantity = 3;
			AssertHasMessageError("Cannot be more than manifest quantity", shipment.B0_BoardedQuantityInfo, messageError);

			shipment.B0_BoardedQuantity = 1;
			AssertNoMessageError("Boarded quantity is ok", shipment.B0_BoardedQuantityInfo, messageError);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			shipment.B0_ManifestQty = 0;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(shipment.B0_BoardedQuantityInfo);

			shipment.B0_BoardedQuantity = 3;
			AssertNoMessageError("Boarded quantity for split shipment", shipment.B0_BoardedQuantityInfo, messageError);
		}

		public void TestCheckB0_DateOfExport()
		{
			const string messageError = "Export Date is required for Goods Astray.";
			ValidationTestHelper.AssertFieldIsNotMandatory(shipment.B0_DateOfExportInfo, messageError);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			ValidationTestHelper.AssertFieldIsNotMandatory(shipment.B0_DateOfExportInfo, messageError);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.GoodsAstray;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(shipment.B0_DateOfExportInfo, messageError);

			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			ValidationTestHelper.AssertFieldIsNotMandatory(shipment.B0_DateOfExportInfo, messageError);
		}

		public void TestCheckB0_Firms()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var firms = firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "code", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_FirmsInfo, "??", firms.ZZD_Code);
		}

		public void TestCheckB0_ManifestQty()
		{
			shipment.Commodities.DeleteAll();

			string messageError = @"The shipment quantity should equal to the total packages of all commodities,
but the balance between the values makes up 1 PCE.";
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(shipment.B0_ManifestQtyInfo);

			shipment.B0_ManifestQty = 4;
			shipment.B0_ManifestUQ = Constants.PkgUnit.Piece;
			shipment.Validation.ValidateB0_ManifestQty();
			AssertNoMessageErrors("No commodities entered yet", shipment.B0_ManifestQtyInfo);
			shipment.Commodities.DeleteAll();
			var commodity = shipment.Commodities.AddNew();
			commodity.BY_PieceCount = 3;
			commodity.BY_ManifestUnitCode = Constants.PkgUnit.Bag;
			shipment.Validation.ValidateB0_ManifestQty();
			AssertHasMessageError("Commodity entered but its quantity less than on shipment", shipment.B0_ManifestQtyInfo, messageError);

			commodity.BY_PieceCount = 4;
			commodity.BY_ManifestUnitCode = Constants.PkgUnit.Box;
			shipment.Validation.ValidateB0_ManifestQty();
			AssertNoMessageErrors("Quantity is valid", shipment.B0_ManifestQtyInfo);

			commodity.Delete();
			messageError = @"The shipment quantity should equal to the total packages of all commodities,
but the balance between the values makes up 4 PCE.";
			shipment.Validation.ValidateAll();
			AssertHasMessageError("Should validate as far as validate all has been run", shipment.B0_ManifestQtyInfo, messageError);
		}

		public void TestCheckB0_ManifestUQ()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_ManifestUQInfo, "??", PackageTypes.Codes.Pieces);
			shipment.B0_ManifestQty = 2;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(shipment.B0_ManifestUQInfo, "??", PackageTypes.Codes.Pieces);
		}

		public void TestCheckB0_MasterBillNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(shipment.B0_MasterBillNumberInfo);

			const string warning = "This shipment has been deleted from Customs file and can be deleted from the system.";
			shipment.Validation.ValidateB0_MasterBillNumber();
			AssertNoWarning(shipment.B0_MasterBillNumberInfo, warning);

			shipment.B0_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			shipment.Validation.ValidateB0_MasterBillNumber();
			AssertHasWarning(shipment.B0_MasterBillNumberInfo, warning);

			shipment.Trip.BH_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			shipment.Validation.ValidateB0_MasterBillNumber();
			AssertNoWarning(shipment.B0_MasterBillNumberInfo, warning);
		}

		public void TestCheckB0_MasterBillNumberUniqueIn3Years()
		{
			var trip1 = Factory.New<Trip>();
			var trip2 = Factory.New<Trip>();
			var trip3 = Factory.New<Trip>();
			var trip4 = Factory.New<Trip>();
			var cusInBondHeader = Factory.New<DummyCusInBondHeader>();

			trip1.BH_JobReference = "E00010003";
			trip2.BH_JobReference = "E00010004";
			trip3.BH_JobReference = "E00010002";
			trip4.BH_JobReference = "E00010001";

			trip1.BH_ETA = ZDateTime.Today;
			trip2.BH_ETA = ZDateTime.Today.AddYears(-2);
			trip3.BH_ETA = ZDateTime.Today.AddYears(-1);
			trip4.BH_ETA = ZDateTime.Today.AddYears(-4);
			cusInBondHeader.BH_ETA = ZDateTime.Today.AddYears(-1);

			var shipment1 = trip1.Shipments.AddNew();
			var shipment2 = trip2.Shipments.AddNew();
			var shipment3 = trip3.Shipments.AddNew();
			var shipment4 = trip4.Shipments.AddNew();
			var cusInBondBill = cusInBondHeader.Bills.AddNew() as DummyCusInBondBill;

			shipment2.B0_MasterBillNumber = "MB00001";
			shipment3.B0_MasterBillNumber = "MB00001";
			shipment4.B0_MasterBillNumber = "MB00001";
			cusInBondBill.B0_MasterBillNumber = "MB00001";
			Factory.Save();

			shipment1.B0_MasterBillNumber = "MB00001";
			AssertHasMessageError(shipment1.B0_MasterBillNumberInfo, "House bill number found on E00010002, E00010004.");

			shipment1.B0_MasterBillNumber = "MB00002";
			AssertNoMessageErrors(shipment1.B0_MasterBillNumberInfo);
		}

		public void TestNoUnnecessaryShipmentsIsLoadedForValiation()
		{
			var date = ZDateTime.Today;
			for (var i = 1; i < 6; i++)
			{
				var trip = Factory.New<Trip>();
				trip.BH_JobReference = "E0001000" + i.ToString();
				trip.BH_ETA = date;
				var shipment = trip.Shipments.AddNew();
				shipment.B0_MasterBillNumber = "MB00001";
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newTrip = newFactory.New<Trip>();
			var newShipment = newTrip.Shipments.AddNew();
			newShipment.B0_MasterBillNumber = "MB00001";
			AssertHasMessageError(newShipment.B0_MasterBillNumberInfo, "House bill number found on E00010001, E00010002, E00010003, E00010004, E00010005.");
			AssertEquals("Validation of duplicate House bill number should not load other house bill into the factory as it will result in excessive memory usage", 1, ((INeedRow)newShipment).Row.Table.Rows.Count);
		}

		public void TestCheckB0_MasterBillNumberUniqueInSameTrip()
		{
			var trip = Factory.New<Trip>();
			var cusInBondHeader = Factory.New<DummyCusInBondHeader>();

			var shipment1 = trip.Shipments.AddNew();
			var shipment2 = trip.Shipments.AddNew();
			var cusInBondBill = cusInBondHeader.Bills.AddNew() as DummyCusInBondBill;

			AssertNoMessageError(shipment1.B0_MasterBillNumberInfo, "House Bill number must be unique.");

			shipment2.B0_MasterBillNumber = "MB00001";
			cusInBondBill.B0_MasterBillNumber = "MB00001";
			shipment1.B0_MasterBillNumber = "MB00001";
			AssertHasMessageError(shipment1.B0_MasterBillNumberInfo, "House Bill number must be unique.");

			shipment1.B0_MasterBillNumber = "MB00002";
			AssertNoMessageErrors(shipment1.B0_MasterBillNumberInfo);
		}

		[TestDate(2021, 8, 24)]
		public void TestCheckB0_MasterBillNumber_Skip3YearUniquenessCheckForTripFromHVLVShipment()
		{
			var duplicatedMasterBill = "SHIPMENT001";

			var ealierTrip = Factory.NewWithValidTestData<Trip>();
			ealierTrip.BH_JobReference = "TRIP001";
			ealierTrip.BH_ETA = ZDateTime.Today.AddDays(-1);
			var ealierShipment = ealierTrip.Shipments.AddNew();
			ealierShipment.B0_MasterBillNumber = duplicatedMasterBill;

			Factory.Save();

			var trip = Factory.NewWithValidTestData<Trip>();
			trip.BH_ETA = ZDateTime.Today;
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = duplicatedMasterBill;

			AssertHasMessageError("Shipment under non hvlv trip should validate for master bill uniqueness in 3 years.", shipment1.B0_MasterBillNumberInfo, "House bill number found on TRIP001.");

			var hvlvTrip = Factory.NewWithValidTestData<Trip>();
			hvlvTrip.BH_ETA = ZDateTime.Today;
			hvlvTrip.Logs.AddNew(Events.Transferred, "|TYP=HVL");
			var shipment2 = hvlvTrip.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = duplicatedMasterBill;

			AssertNoMessageError("Shipment under hvlv trip should skip validation for master bill uniqueness in 3 years", shipment2.B0_MasterBillNumberInfo, "House bill number found on TRIP001.");
		}

		public void TestCheckB0_MasterBillNumberHitsCache()
		{
			var trip = Factory.New<Trip>();

			var shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "LTTSTORE";

			var shipmentWithDuplicateBillNumber = trip.Shipments.AddNew();
			shipmentWithDuplicateBillNumber.B0_MasterBillNumber = "LTTSTORE";

			Assert("Precondition: cache not hit as it is not in saving context", !trip.duplicateMasterBillsCacheHit);

			trip.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				Assert("Cache has been hit as pre-save validation on ShipmentCollection was run", trip.duplicateMasterBillsCacheHit);
				AssertHasMessageError(shipment.B0_MasterBillNumberInfo, "House Bill number must be unique.");
			});
		}

		public void TestCheckB0_PortOfLadingKCode()
		{
			Factory.ClearCachedValue<Universal.ZZRefCusCodeListCombinedCollection>("ScheduleKPortCodes");
			var validCode = Factory.NewWithValidTestData<Universal.ZZRefCusCodeListCombined>();
			validCode.ZZD_Code = "Z!123";
			validCode.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
			validCode.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port;
			validCode.ZZD_StartDate = ZDateTime.BrettsBirthday;
			validCode.ZZD_EndDate = ZDateTime.MaxSmallDateTime;
			var invalidCode = "????";
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(shipment.B0_PortOfLadingKCodeInfo, invalidCode, validCode.ZZD_Code);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_PortOfLadingKCodeInfo, invalidCode, validCode.ZZD_Code);

			shipment.B0_ShipmentType = ZString.Empty;
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_PortOfLadingKCodeInfo, invalidCode, validCode.ZZD_Code);
		}

		public void TestCheckB0_RL_NKPortOfLading()
		{
			var validCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).RL_Code;
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_RL_NKPortOfLadingInfo, "????", validCode);
		}

		public void TestCheckB0_ServiceType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_ServiceTypeInfo, "??", ServiceTypes.Codes.CollectOnDelivery);
		}

		public void TestCheckB0_ShipmentType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(shipment.B0_ShipmentTypeInfo, "??", ShipmentTypes.Codes.Inbond);

			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_ShipmentTypeInfo, "??", ShipmentTypes.Codes.Inbond);
		}

		public void TestCheckAtLeastOneCommodityEntered()
		{
			const string messageError = "At least one commodity must be entered.";
			shipment.Validation.ValidateB0_ShipmentType();
			AssertNoMessageError("No child collections should be validated if validate all has not been run", shipment.B0_ShipmentTypeInfo, messageError);

			shipment.Commodities.DeleteAll();
			shipment.Validation.ValidateAll();
			AssertHasMessageError("Should validate as far as validate all has been run", shipment.B0_ShipmentTypeInfo, messageError);

			shipment.Commodities.AddNew();
			shipment.Validation.ValidateB0_ShipmentType();
			AssertNoMessageError("Commodities entered", shipment.B0_ShipmentTypeInfo, messageError);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			shipment.Commodities.DeleteAll();
			shipment.Validation.ValidateB0_ShipmentType();
			AssertNoMessageError("No commodities required for split shipment", shipment.B0_ShipmentTypeInfo, messageError);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			shipment.Validation.ValidateB0_ShipmentType();
			AssertNoMessageError("No commodities required if lodged from other trip", shipment.B0_ShipmentTypeInfo, messageError);
		}

		public void TestCheckB0_Volume()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(shipment.B0_VolumeInfo);
		}

		public void TestCheckB0_VolumeUQ()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_VolumeUQInfo, "??", Constants.Volume.CubicMetres);
			shipment.B0_Volume = 2;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(shipment.B0_VolumeUQInfo, "??", Constants.Volume.CubicMetres);
		}

		public void TestCheckB0_Weight_Negative()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(shipment.B0_WeightInfo);
		}

		public void TestCheckB0_Weight_SumWithNegativeWeight()
		{
			var commodity = shipment.FirstCommodity;
			commodity.BY_GrossWeight = 1500;
			commodity.BY_GrossWeightUnit = Constants.Weight.Kilograms;

			shipment.B0_WeightUQ = Constants.Weight.Tonnes;
			shipment.B0_Weight = 1;
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Commodity Exists", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
				shipment.B0_Weight = -1;
				AssertNoMessageErrorContaining("Negative weight shipment, not check", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
			});
		}

		public void TestCheckB0_Weight_SumCommodities()
		{
			var commodity = shipment.FirstCommodity;
			commodity.BY_GrossWeight = 1500;
			commodity.BY_GrossWeightUnit = Constants.Weight.Kilograms;

			shipment.B0_WeightUQ = Constants.Weight.Tonnes;
			shipment.B0_Weight = 1;
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Commodity Exists", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
				shipment.Commodities.DeleteAll();
				shipment.Validation.ValidateB0_Weight();
				AssertNoMessageErrorContaining("No Commodities", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
			});
		}

		public void TestCheckB0_Weight_SumCommoditiesAndConvert()
		{
			const string completeMessageError = @"The gross weight should equal to the total weight of all commodities, but the balance between the values makes up -0.5 T.";

			shipment.B0_Weight = 1;
			shipment.B0_WeightUQ = Constants.Weight.Tonnes;

			var commodity = shipment.FirstCommodity;
			commodity.BY_GrossWeight = 1500;
			commodity.BY_GrossWeightUnit = Constants.Weight.Kilograms;
			shipment.Validation.ValidateB0_Weight();
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Commodity entered but its weight less than on shipment", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
				AssertEquals("Confirm Error message is correct", true, shipment.B0_WeightInfo.HasMessageError(completeMessageError));
				commodity.BY_GrossWeight = 1000;
				shipment.Validation.ValidateB0_Weight();
				AssertNoMessageErrorContaining("Weight is valid", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
			});
		}

		public void TestCheckB0_Weight_SumCommoditiesValidateAll()
		{
			shipment.B0_WeightUQ = Constants.Weight.Tonnes;
			shipment.B0_Weight = 1;
			shipment.Commodities.DeleteAll();
			CombineAssertions(() =>
			{
				shipment.Validation.ValidateB0_Weight();
				AssertNoMessageErrorContaining("No Commodities, property validation ", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
				shipment.Validation.ValidateAll();
				AssertHasMessageErrorContaining("No Commodities Validate All", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
				shipment.B0_Weight = 2;
				AssertHasMessageErrorContaining("No Commodities property validate, after validate all fired", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
			});
		}

		[ExpectNoExceptions]
		public void TestCheckB0_Weight_InvalidUnitOnShipment()
		{
			shipment.B0_WeightUQ = "XX";
			shipment.B0_Weight = 1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("Shipment with invalid UQ, not check", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);

				shipment.B0_WeightUQ = Constants.Weight.Kilograms;
				var commodity = shipment.FirstCommodity;
				commodity.BY_GrossWeight = 1500;
				commodity.BY_GrossWeightUnit = Constants.Weight.Kilograms;
				shipment.Validation.ValidateB0_Weight();
				AssertHasMessageErrorContaining("Shipment with valid UQ", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
			});
		}

		[ExpectNoExceptions]
		public void TestCheckB0_Weight_InvalidUnitOnCommodity()
		{
			shipment.B0_WeightUQ = Constants.Weight.Tonnes;
			shipment.B0_Weight = 1;

			var commodity = shipment.FirstCommodity;
			commodity.BY_GrossWeight = 1500;
			commodity.BY_GrossWeightUnit = "XX";

			CombineAssertions(() =>
			{
				shipment.Validation.ValidateB0_Weight();
				AssertHasMessageErrorContaining("Commodity with invalid UQ", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);

				shipment.B0_Weight = 0;
				commodity = shipment.FirstCommodity;
				commodity.BY_GrossWeight = 1500;
				commodity.BY_GrossWeightUnit = "XX";
				shipment.Validation.ValidateB0_Weight();
				AssertNoMessageErrorContaining("Commodity with invalid UQ, convert to 0", shipment.B0_WeightInfo, GrossWeightCommodityPartialErrorMessage);
			});
		}

		public void TestCheckB0_WeightUQ()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(shipment.B0_WeightUQInfo, "??", Constants.Weight.Kilograms);
			shipment.B0_Weight = 2;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(shipment.B0_WeightUQInfo, "??", Constants.Weight.Kilograms);
		}

		public void TestCheckB0_WasOutOfUSFor45DaysOrLess()
		{
			const string messageError = "For Goods Astray, you have to declare that the goods have not left either your or the foreign countries'" +
					" customs service control while in the foreign country and returned within 45 days or less since the date of exportation.";
			ValidationTestHelper.AssertFieldIsNotMandatory(shipment.B0_WasOutOfUSFor45DaysOrLessInfo, messageError);

			shipment.B0_ShipmentType = ShipmentTypes.Codes.GoodsAstray;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(shipment.B0_WasOutOfUSFor45DaysOrLessInfo, messageError);

			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			ValidationTestHelper.AssertFieldIsNotMandatory(shipment.B0_WasOutOfUSFor45DaysOrLessInfo, messageError);
		}

		public void TestCheckB0_IssuerSCAC()
		{
			var validCode = Factory.LoadTop1<USCCarrier>(new ZQuery()).UI_Code;
			shipment.B0_IssuerSCAC = "~!";
			shipment.Validation.ValidateB0_IssuerSCAC();
			AssertHasMessageError("Invalid SCAC code", shipment.B0_IssuerSCACInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckB0_IssuerSCAC_Required()
		{
			shipment.B0_IssuerSCAC = "";
			var expectedError = "The issuer's SCAC is required to make the message to send to customs office valid, otherwise the message will be rejected as an error with missing SCAC.";
			AssertHasMessageError("The issuer's SCAC is missing", shipment.B0_IssuerSCACInfo, expectedError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<Trip>().Shipments.AddNew();
		}
		Shipment shipment;

		const string GrossWeightCommodityPartialErrorMessage = "The gross weight should equal to the total weight of all commodities,";
	}

	sealed class DummyCusInBondHeader : CusInBondHeader
	{
		public DummyCusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override System.Type BillTypeCore => typeof(DummyCusInBondBill);

		protected override System.Type MovementHeaderTypeCore => null;

		protected override CusInBondMoveHeaderCollection GetMovementHeaders() => null;

		protected override ICusInBondBillCollection GetNewBillsCollection() => new DummyCusInBondBillCollection(this);
	}

	sealed class DummyCusInBondBill : CusInBondBill
	{
		public DummyCusInBondBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override System.Type MovementDetailType => null;
	}

	sealed class DummyCusInBondBillCollection : CusInBondBillCollection<DummyCusInBondBill>
	{
		public DummyCusInBondBillCollection(CusInBondHeader master)
			: base(master)
		{
		}
	}
}
