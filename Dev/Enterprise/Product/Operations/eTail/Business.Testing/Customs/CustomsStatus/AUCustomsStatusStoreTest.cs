using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	public class AUCustomsStatusStoreTest : TestCaseWithFactory
	{
		public void TestOverridenMembers()
		{
			var customsStatusStore = new AUCustomsStatusStore(Factory);
			var customsStatusStoreType = typeof(AUCustomsStatusStore);

			var exportCodeTypeProperty = customsStatusStoreType.GetProperty("ExportCodeTypes", BindingFlags.NonPublic | BindingFlags.Instance);
			var exportCodeTypes = exportCodeTypeProperty?.GetValue(customsStatusStore) as string[];
			AssertNotNull(exportCodeTypes);
			Assert(exportCodeTypes?.Length == 1);
			AssertEquals(RefCusCodeListTypes.Codes.ExportCustomsStatus, exportCodeTypes?[0]);

			var countryCodeProperty = customsStatusStoreType.GetProperty("CountryCode", BindingFlags.NonPublic | BindingFlags.Instance);
			var countryCode = (ZString?)countryCodeProperty?.GetValue(customsStatusStore);
			AssertNotNull(countryCode);
			AssertEquals(CountryCodes.Australia, countryCode);

			var getCodeTypeForExportProperty = customsStatusStoreType.GetMethod("GetCodeTypeForExport", BindingFlags.NonPublic | BindingFlags.Instance);
			var codeTypeForExport = getCodeTypeForExportProperty?.Invoke(customsStatusStore, null) as string;
			AssertNotNull(codeTypeForExport);
			AssertEquals(RefCusCodeListTypes.Codes.ExportCustomsStatus, codeTypeForExport);
		}

		public void TestGetCustomsStatusList_FormalDeclarationCustomsStatusListIsSameAsLowValueCustomsStatusListWhenImport()
		{
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateNewOrGetExistingDataGrouping("AU");

			var codeAA = testHelper.CreateCusCodeList("AU", "CSTA", "AA", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var codeBB = testHelper.CreateCusCodeList("AU", "CSTA", "BB", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			testHelper.CreateCusCodeListAttribute(codeAA.PK, "EcommerceReleaseStatus", "HLD");
			testHelper.CreateCusCodeListAttribute(codeBB.PK, "EcommerceReleaseStatus", "CLR");

			Factory.Save();

			var customsStatusStore = new AUCustomsStatusStore(Factory);
			var declaration = Factory.New<BaseJobDeclaration>();

			AssertFormalDeclarationCustomsStatusListIsSameAsLowValueCustomsStatusList("AA");
			AssertFormalDeclarationCustomsStatusListIsSameAsLowValueCustomsStatusList("BB");

			void AssertFormalDeclarationCustomsStatusListIsSameAsLowValueCustomsStatusList(string code)
			{
				var releaseStatusFromFormalDeclarationCustomsStatusList = customsStatusStore.GetReleaseStatus(code, true, declaration);
				var releaseStatusFromLowValueCustomsStatusList = customsStatusStore.GetReleaseStatus(code, true, null);
				AssertEquals(releaseStatusFromFormalDeclarationCustomsStatusList, releaseStatusFromLowValueCustomsStatusList);
			}
		}
	}
}
