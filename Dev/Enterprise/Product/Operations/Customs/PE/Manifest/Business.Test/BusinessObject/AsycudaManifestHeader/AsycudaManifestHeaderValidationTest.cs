using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_Nature()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_Nature = ZString.Empty;
			AssertHasMessageErrorContaining(header.AMA_NatureInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertHasMessageErrorContaining(header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Peru, RefCusCodeListTypes.Codes.CustomsOffice, "019", "TUMBES", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.AMA_CustomsOfficeInfo, "001", "019");
		}
	}
}
