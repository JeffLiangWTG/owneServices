using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		const string SelectEnglishAddressMessage = "Please select an English address. Traditional Chinese address should be added as a translated address of the English address.";

		public void TestCheckABL_ShipperStreet1()
		{
			var messageText = "You have not entered a Shipper Street 1.";
			bill.ABL_ShipperLocalStreet1 = ZString.Empty;
			bill.ABL_ShipperStreet1 = ZString.Empty;
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertNoMessageError(bill.ABL_ShipperStreet1Info, messageText);

			bill.ABL_ShipperLocalStreet1 = "test local address";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertHasMessageError(bill.ABL_ShipperStreet1Info, messageText);

			bill.ABL_ShipperStreet1 = "test address";
			AssertNoMessageError(bill.ABL_ShipperStreet1Info, messageText);
		}

		public void TestCheckABL_NotifyPartyStreet1()
		{
			var messageText = "You have not entered a Notify Party Street 1.";
			bill.ABL_NotifyPartyLocalStreet1 = ZString.Empty;
			bill.ABL_NotifyPartyStreet1 = ZString.Empty;
			bill.Validation.ValidateABL_NotifyPartyStreet1();
			AssertNoMessageError(bill.ABL_NotifyPartyStreet1Info, messageText);

			bill.ABL_NotifyPartyLocalStreet1 = "test local address";
			bill.Validation.ValidateABL_NotifyPartyStreet1();
			AssertHasMessageError(bill.ABL_NotifyPartyStreet1Info, messageText);

			bill.ABL_NotifyPartyStreet1 = "test address";
			AssertNoMessageError(bill.ABL_NotifyPartyStreet1Info, messageText);
		}

		public void TestCheckABL_OA_Shipper()
		{
			AssertShouldSelectEnglishAddress(bill.ABL_OA_ShipperInfo);
		}

		void AssertShouldSelectEnglishAddress(ZPropertyInfo propertyInfo)
		{
			var orgHeader = Factory.New<OrgHeader>();
			var ctAddress = orgHeader.Addresses.AddNew();
			ctAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			var enAddress = orgHeader.Addresses.AddNew();
			enAddress.OA_Language = Core.SharedConstants.Languages.English;

			propertyInfo.Value = ctAddress.PK;
			AssertHasMessageError(propertyInfo, SelectEnglishAddressMessage);

			propertyInfo.Value = enAddress.PK;
			AssertNoMessageError(propertyInfo, SelectEnglishAddressMessage);
		}

		public void TestCheckABL_OA_Consignee()
		{
			AssertShouldSelectEnglishAddress(bill.ABL_OA_ConsigneeInfo);
		}

		public void TestCheckABL_OA_NotifyParty()
		{
			AssertShouldSelectEnglishAddress(bill.ABL_OA_NotifyPartyInfo);
		}

		public void TestCheckABL_ShipperName()
		{
			var message = "You have not entered a Shipper's English Name.";
			bill.ABL_ShipperStreet1 = "address 1";
			bill.ABL_ShipperName = ZString.Empty;
			bill.Validation.ValidateABL_ShipperName();
			AssertHasMessageError(bill.ABL_ShipperNameInfo, message);

			bill.ABL_ShipperStreet1 = ZString.Empty;
			bill.ABL_ShipperRegNoType = "VAT";
			bill.ABL_ShipperRegNo = "96944490";
			bill.Validation.ValidateABL_ShipperName();
			AssertHasMessageError(bill.ABL_ShipperNameInfo, message);

			bill.ABL_ShipperName = "test name";
			AssertNoMessageError(bill.ABL_ShipperNameInfo, message);

			bill.ABL_ShipperRegNo = ZString.Empty;
			bill.ABL_ShipperName = ZString.Empty;
			AssertNoMessageError(bill.ABL_ShipperNameInfo, message);
		}

		public void TestCheckABL_NotifyPartyName()
		{
			var message = "You have not entered a Notify Party's English Name.";
			bill.ABL_NotifyPartyStreet1 = "address 1";
			bill.ABL_NotifyPartyName = ZString.Empty;
			bill.Validation.ValidateABL_NotifyPartyName();
			AssertHasMessageError(bill.ABL_NotifyPartyNameInfo, message);

			bill.ABL_NotifyPartyStreet1 = ZString.Empty;
			bill.ABL_NotifyPartyRegNoType = "VAT";
			bill.ABL_NotifyPartyRegNo = "96944490";
			bill.Validation.ValidateABL_NotifyPartyName();
			AssertHasMessageError(bill.ABL_NotifyPartyNameInfo, message);

			bill.ABL_NotifyPartyName = "test name";
			AssertNoMessageError(bill.ABL_NotifyPartyNameInfo, message);

			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			bill.ABL_NotifyPartyName = ZString.Empty;
			AssertNoMessageError(bill.ABL_NotifyPartyNameInfo, message);
		}

		public void TestCheckABL_ConsigneeName()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_ConsigneeNameInfo, "Consignee's English Name");
		}

		public void TestCheckABL_ShipmentType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_ShipmentTypeInfo);
		}

		public void TestCheckABL_ManifestQty()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_ManifestQtyInfo);
		}

		public void TestCheckABL_ManifestUQ()
		{
			new TestTWCreator(Factory).CreateInvoiceUQ();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bill.ABL_ManifestUQInfo, "XXX", "AMP");
		}

		public void TestCheckABL_SplitQuantity()
		{
			var targetInfo = bill.ABL_SplitQuantityInfo;
			bill.ABL_ManifestQty = 12;
			bill.ABL_SplitQuantity = 13;
			AssertHasMessageErrorContaining(targetInfo, "Split Quantity must be lower than or equal to Quantity");

			bill.ABL_SplitQuantity = 12;
			AssertNoMessageErrorContaining(targetInfo, "Split Quantity must be lower than or equal to Quantity");
		}

		public void TestCheckABL_GrossWeight()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_GrossWeightInfo);
		}

		public void TestCheckABL_WeightUQ()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bill.ABL_GrossWeightUQInfo, "XX", "DT");
		}

		public void TestCheckABL_VolumeUQ()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(bill.ABL_VolumeUQInfo, "XX", "CC");
		}

		public void TestCheckABL_Tariff()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var message = ListValidation.InvalidCodeMessage.ToString();
			CombineAssertions(() =>
			{
				bill.ABL_Tariff = "01012100005";
				AssertHasMessageError(bill.ABL_TariffInfo, message);

				bill.ABL_Tariff = "01012100003";
				AssertNoMessageError(bill.ABL_TariffInfo, message);

				bill.ABL_Tariff = "01012100006";
				AssertHasMessageError(bill.ABL_TariffInfo, message);

				bill.ABL_Tariff = "01012100004";
				AssertNoMessageError(bill.ABL_TariffInfo, message);
			});
		}

		public void TestCheckABL_RL_NKPortOfLoading()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_RL_NKPortOfLoadingInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(bill.ABL_RL_NKPortOfLoadingInfo, "TWZZZ", "TWZ99");
			ValidationTestHelper.AssertInvalidCodeMessageError(bill.ABL_RL_NKPortOfLoadingInfo, "Z99", "TWTPE");
		}

		public void TestCheckABL_LocationInformation()
		{
			bill.ABL_RL_NKPortOfLoading = "TWZ99";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_LocationInformationInfo);

			bill.ABL_RL_NKPortOfLoading = "TWTPE";
			bill.ABL_LocationInformation = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_RL_NKFinalDestination()
		{
			bill.Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_RL_NKFinalDestinationInfo);

			bill.Header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			bill.ABL_RL_NKFinalDestination = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_GoodsLocation()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			newFactory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bill.ABL_GoodsLocationInfo, "XX", "ANP0060D");
		}

		public void TestCheckABL_ShipperRegNo()
		{
			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Taiwan;
			bill.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.ABL_ShipperRegNo = "NO1234";
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, PASMessage);

			bill.ABL_ShipperRegNo = "1234";
			AssertNoMessageError(bill.ABL_ShipperRegNoInfo, PASMessage);

			bill.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.VATCode;
			bill.ABL_ShipperRegNo = "NO1234";
			AssertNoMessageError(bill.ABL_ShipperRegNoInfo, PASMessage);
		}

		public void TestCheckABL_ConsigneeRegNo()
		{
			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Taiwan;
			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.ABL_ConsigneeRegNo = "NO1234";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, PASMessage);

			bill.ABL_ConsigneeRegNo = "1234";
			AssertNoMessageError(bill.ABL_ConsigneeRegNoInfo, PASMessage);

			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.VATCode;
			bill.ABL_ConsigneeRegNo = "NO1234";
			AssertNoMessageError(bill.ABL_ConsigneeRegNoInfo, PASMessage);
		}

		public void TestCheckABL_NotifyPartyRegNo()
		{
			bill.ABL_RN_NKNotifyPartyCountry = Core.Constants.CountryCodes.Taiwan;
			bill.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.ABL_NotifyPartyRegNo = "NO1234";
			AssertHasMessageError(bill.ABL_NotifyPartyRegNoInfo, PASMessage);

			bill.ABL_NotifyPartyRegNo = "1234";
			AssertNoMessageError(bill.ABL_NotifyPartyRegNoInfo, PASMessage);

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.VATCode;
			bill.ABL_NotifyPartyRegNo = "NO1234";
			AssertNoMessageError(bill.ABL_NotifyPartyRegNoInfo, PASMessage);
		}

		public void TestCheck_ABL_DG_UNNO()
		{
			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Code = "10XX";
			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Code = "20XX";

			ValidationTestHelper.AssertInvalidCodeMessageError(bill.ABL_DG_UNNOInfo, new ZString[] { "X@X" }, new ZString[] { "10XX", "20XX" });
		}

		const string PASMessage = "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here.";

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;
	}
}
