using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_ConsigneeRegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			var bill = header.Bills.AddNew();
			CombineAssertions("ABL_ConsigneeRegNo validation", () =>
			{
				header.AMA_Nature = ShipmentTypeList.Codes.Export22;
				bill.ABL_ConsigneeRegNo = ZString.Empty;
				bill.IsToOrder = ZBool.False;
				AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_Nature", bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				bill.ABL_ConsigneeRegNo = ZString.Empty;
				bill.IsToOrder = ZBool.False;
				AssertHasMessageErrorContaining("After set value, there should be message error on AMA_Nature", bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);
				bill.ABL_ConsigneeRegNo = "mycmpregno";
				bill.IsToOrder = ZBool.False;
				AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_ConsigneeRegNo", Bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);
				bill.ABL_ConsigneeRegNo = ZString.Empty;
				bill.IsToOrder = ZBool.True;
				AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_ConsigneeRegNo", Bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
				bill.ABL_ConsigneeRegNo = ZString.Empty;
				bill.IsToOrder = ZBool.False;
				AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_ManifestType/ABL_ConsigneeRegNo", bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckABL_ConsigneeName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("MVAL", "MVAL");
			var code = helper.CreateNewOrGetExistingCusCodeList("TR", "MVAL", "CONSIGNEE", "CONSIGNEE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, "MANDATORY", "");
			Factory.Save();
			Header.AMA_ManifestType = TRManifestTypes.Codes.VARONC;
			Bill.Validation.ValidateABL_ConsigneeName();
			AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_ConsigneeName", Bill.ABL_ConsigneeNameInfo, "CONSIGNEE for TR.");
			Header.AMA_ManifestType = TRManifestTypes.Codes.DIGITH;
			Bill.Validation.ValidateABL_ConsigneeName();
			AssertHasMessageErrorContaining(Bill.ABL_ConsigneeNameInfo, "CONSIGNEE for TR.");
			Bill.ABL_ConsigneeName = "my name";
			AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_ConsigneeName", Bill.ABL_ConsigneeNameInfo, "CONSIGNEE for TR.");
		}

		public void TestCheckABL_ShipperName()
		{
			Header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;
			Bill.Validation.ValidateABL_ShipperName();
			AssertHasMessageErrorContaining(Bill.ABL_ShipperNameInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_ShipperName = "my name";
			AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_ShipperName", Bill.ABL_ShipperNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPaymentType()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			CombineAssertions("PaymentType validation", () =>
			{
				bill.PaymentType = "X";
				AssertHasMessageError("Error InvalidCode", bill.PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
				bill.PaymentType = "A";
				AssertNoMessageErrorContaining(bill.PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestPaymentTypeMaxLength()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			try
			{
				bill.PaymentType = "ABCDE";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		public void TestCheckTransshipmentType()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			AssertEquals(bill.TransshipmentType, ZString.Empty);
			bill.TransshipmentType = "X";
			AssertHasMessageErrorContaining(bill.TransshipmentTypeInfo, ListValidation.InvalidCodeMessageError);
			bill.TransshipmentType = "1";
			AssertNoMessageErrorContaining(bill.TransshipmentTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestTransshipmentTypeMaxLength()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			try
			{
				bill.TransshipmentType = "ABCDE";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		public void TestCheckCustomsEntryNumberAndCustomsEntryNumberType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "CustomsEntryNumberTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "PRV", "Previous Declaration", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.EMANIF, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "01234567890";
			bill.CustomsEntryNumberType = "PRV";
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberInfo, ListValidation.InvalidCodeMessageError);
			bill.CustomsEntryNumber = "";
			bill.CustomsEntryNumberType = "PRV";
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(bill.CustomsEntryNumberInfo, "You have not entered Customs Number.");
			bill.CustomsEntryNumberType = "";
			AssertHasMessageError(bill.CustomsEntryNumberTypeInfo, "Previous Declaration is needed.");
			bill.CustomsEntryNumberType = "ABC";
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, "Previous Declaration is needed.");
			header.AMA_ManifestType = TRManifestTypes.Codes.EMANIF;
			bill.CustomsEntryNumberType = "PRV";
			bill.CustomsEntryNumber = "9901234567890";
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberInfo, ListValidation.InvalidCodeMessageError);
			bill.CustomsEntryNumber = "";
			bill.CustomsEntryNumberType = "PRV";
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(bill.CustomsEntryNumberInfo, "You have not entered Customs Number.");
			bill.CustomsEntryNumberType = "";
			AssertHasMessageError(bill.CustomsEntryNumberTypeInfo, "Previous Declaration is needed.");
			bill.CustomsEntryNumberType = "ABC";
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, "Previous Declaration is needed.");
			header.AMA_ManifestType = TRManifestTypes.Codes.ATAITH;
			bill.CustomsEntryNumberType = "PRV";
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_ManifestType and CustomsEntryNumberType", bill.CustomsEntryNumberTypeInfo, "Previous Declaration is needed.");
		}

		public void TestCheckABL_ShipperRegNo()
		{
			Header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			Bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_Nature", Bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			Header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(Bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			Header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
			Bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrorContaining(Bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_ShipperRegNo = "mycmpregno";
			AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_ShipperRegNo", Bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBillStampDutyValue()
		{
			var bill = Header.Bills.AddNew();
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40DC";
			var container = bill.Header.Containers.AddNew();
			container.ACN_ContainerNumber = "CNTR000001";
			container.ACN_RC_ContainerType = refContainer.PK;
			container.ACN_Seal1 = "Seal 1";
			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 15;
			pack.APA_LineNo = 1;
			pack.ContainerPK = container.PK;
			Factory.Save();
			CombineAssertions("Bill Stamp Duty Value", () =>
			{
				bill.BillStampDutyValue = 100;
				AssertNoMessageErrorContaining("After set value, there should not be message error on BillStampDutyValue", bill.BillStampDutyValueInfo, "Manifest transport type is 'SEA' and bill has Empty type containers, do not add this stamp duty.");
				container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.LessThanFullContainerLoad;
				bill.BillStampDutyValue = 50;
				AssertNoMessageErrorContaining("After set value, there should not be message error on BillStampDutyValue", bill.BillStampDutyValueInfo, "Manifest transport type is 'SEA' and bill has Empty type containers, do not add this stamp duty.");
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.EmptyContainer;
				bill.BillStampDutyValue = 30;
				AssertHasMessageErrorContaining("After set value, there should be message error on BillStampDutyValue", bill.BillStampDutyValueInfo, "Manifest transport type is 'SEA' and bill has Empty type containers, do not add this stamp duty.");
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				bill.BillStampDutyValue = 40;
				AssertNoMessageErrorContaining("After set value, there should not be message error on BillStampDutyValue", bill.BillStampDutyValueInfo, "Manifest transport type is 'SEA' and bill has Empty type containers, do not add this stamp duty.");
			});
		}

		public void TestCheckABL_NotifyPartyRegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			bill.IsToOrder = ZBool.True;
			AssertNoMessageErrorContaining("After set value, there should not be message error on AMA_Nature", bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			bill.IsToOrder = ZBool.True;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyRegNo = "mycmpregno";
			bill.IsToOrder = ZBool.False;
			AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_NotifyPartyRegNo", Bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			bill.IsToOrder = ZBool.False;
			AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_NotifyPartyRegNo", Bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_LocationInformation()
		{
			Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Bill.Validation.ValidateABL_LocationInformation();
			CombineAssertions(() =>
			{
				Header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				Bill.ABL_LocationInformation = ZString.Empty;
				AssertHasMessageErrorContaining(Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Bill.ABL_LocationInformation = "TEST";
				AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_LocationInformationInfo", Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				Bill.ABL_LocationInformation = ZString.Empty;
				AssertHasMessageErrorContaining(Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Bill.ABL_LocationInformation = "TEST1";
				AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_LocationInformationInfo", Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				Bill.ABL_LocationInformation = ZString.Empty;
				AssertNoMessageErrorContaining(Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckABL_SpecialCargoCode()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			CombineAssertions(() =>
			{
				bill.ABL_SpecialCargoCode = "X";
				AssertHasMessageError(bill.ABL_SpecialCargoCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.ABL_SpecialCargoCode = "Y";
				AssertNoMessageErrorContaining(bill.ABL_SpecialCargoCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.ABL_SpecialCargoCode = "N";
				AssertNoMessageErrorContaining(bill.ABL_SpecialCargoCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckABL_RL_NKOrigin()
		{
			CombineAssertions(() =>
			{
				Header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				Bill.ABL_RL_NKOrigin = "TRIST";
				AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_RL_NKOrigin", Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Bill.ABL_RL_NKOrigin = ZString.Empty;
				AssertNoMessageErrorContaining("After set value, there should not be message error on ABL_RL_NKOrigin", Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				Bill.ABL_LocationInformation = ZString.Empty;
				AssertHasMessageErrorContaining("After set value, there should be message error on ABL_RL_NKOrigin", Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Bill.ABL_RL_NKOrigin = "TRIST";
				AssertHasMessageErrorContaining("After set value, there should be message error on ABL_RL_NKOrigin", Bill.ABL_LocationInformationInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				Header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				Bill.ABL_RL_NKOrigin = "";
				AssertHasMessageErrorContaining("There should be validation", Bill.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertHasMessageErrorContaining("There should be validation", Bill.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertHasMessageErrorContaining("There should be validation", Bill.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertHasMessageErrorContaining("There should be validation", Bill.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
				Header.AMA_ManifestType = TRManifestTypes.Codes.TIRITH;
				Header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TWForwarderManifest;
				Header.AMA_TransportMode = Core.Constants.TransportModes.AirSea;
				AssertHasMessageErrorContaining("There should not be validation", Bill.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public AsycudaManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<AsycudaManifestHeader>();
					bill = header.Bills.AddNew();
				}

				return header;
			}
		}

		AsycudaManifestHeader header;
		public AsycudaBill Bill
		{
			get
			{
				if (bill == null)
				{
					bill = Header.Bills.AddNew();
				}

				return bill;
			}
		}

		AsycudaBill bill;
	}
}
