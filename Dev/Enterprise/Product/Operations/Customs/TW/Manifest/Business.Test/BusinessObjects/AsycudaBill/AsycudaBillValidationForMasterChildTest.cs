using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_BillNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bill.ABL_BillNumberInfo);
		}

		public void TestCheckABL_GoodsLocation()
		{
			var targetInfo = bill.ABL_GoodsLocationInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			TestCheckABL_GoodsLocationInList(targetInfo);
		}

		void TestCheckABL_GoodsLocationInList(ZPropertyInfo targetInfo)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BA", "Taipei office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			Factory.Save();

			bill.ABL_GoodsLocation = "AA1234";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			bill.ABL_GoodsLocation = "ANP0060D";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			bill.ABL_GoodsLocation = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckABL_E_ARV()
		{
			var targetInfo = bill.ABL_E_ARVInfo;
			header.AMA_TransportMode = "SEA";
			bill.ABL_E_ARV = ZDateTime.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_TransportMode = "AIR";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_BolType = "BOL";
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
