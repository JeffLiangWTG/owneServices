using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusAuthorisationHelperTest : BusinessObjectValidationTestCase
	{
		public void TestGetAuthorizationNumbersForAddressesTest()
		{
			ZString expectedAuthType = "CWP";
			var availbleAuthList = CusAuthorisationHelper.GetCachedAuthorizationNumbersForAddresses(Factory, Core.Constants.CountryCodes.Belgium, new[] { expectedAuthType }, new[] { ZGuid.Empty }, ZDate.Today);

			AssertEquals(0, availbleAuthList.Count);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "COD";
			var warehouseAddress = Factory.New<OrgAddress>();
			warehouseAddress.Address1 = "2 Street";
			warehouseAddress.OA_OH = importer.PK;

			availbleAuthList = CusAuthorisationHelper.GetCachedAuthorizationNumbersForAddresses(Factory, Core.Constants.CountryCodes.Belgium, new[] { expectedAuthType }, new[] { importer.MainAddress.PK }, ZDate.Today);

			AssertEquals(0, availbleAuthList.Count);

			var authorisation = Factory.New<CusAuthorisationHeader>();
			authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
			authorisation.CPH_EndDate = ZDate.Today.AddDays(1);
			authorisation.CPH_Number = "BECWP3456";
			authorisation.CPH_OH_PermitHolder = importer.PK;
			authorisation.CPH_Type = "CWP";
			authorisation.CPH_OA_AppliesTo = importer.MainAddress.PK;
			Factory.Save();
			availbleAuthList = CusAuthorisationHelper.GetCachedAuthorizationNumbersForAddresses(new BusinessObjectFactory(), Core.Constants.CountryCodes.Belgium, new[] { expectedAuthType }, new[] { importer.MainAddress.PK }, ZDate.Today);

			AssertEquals(1, availbleAuthList.Count);
		}
	}
}
