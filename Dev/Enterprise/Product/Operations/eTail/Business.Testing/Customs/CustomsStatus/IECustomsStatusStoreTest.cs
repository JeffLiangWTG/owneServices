using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	public class IECustomsStatusStoreTest : TestCaseWithFactory
	{
		public void TestIECustomsStatusCodeAndDescriptionList()
		{
			var ieCustomsStatusStore = new IECustomsStatusStore(Factory);
			var actualList = ieCustomsStatusStore.GetAllRefCusCodeList(true);

			var aisEntryStatusList = new AISEntryStatusList();
			var statusListType = typeof(AISEntryStatusList);
			var codesClassType = statusListType.GetNestedType((NoResString)"Codes");
			var expectedList = new List<(string, string)>();
			foreach (var codeField in codesClassType.GetFields())
			{
				var codeName = codeField.Name;
				var code = (string)codeField.GetValue(null);
				var description = $"{codeName} - {aisEntryStatusList.GetDescriptionFromCode(code)}";

				expectedList.Add((code, description));
			}

			AssertCodeDescriptionPairList(actualList, expectedList.ToArray());
		}

		public void TestOverridenMembers()
		{
			var customsStatusStore = new IECustomsStatusStore(Factory);
			var customsStatusStoreType = typeof(IECustomsStatusStore);

			var exportCodeTypeProperty = customsStatusStoreType.GetProperty("ExportCodeTypes", BindingFlags.NonPublic | BindingFlags.Instance);
			var exportCodeTypes = exportCodeTypeProperty?.GetValue(customsStatusStore) as string[];
			AssertNotNull(exportCodeTypes);
			Assert(exportCodeTypes?.Length == 1);
			AssertEquals(RefCusCodeListTypes.Codes.CustomsStatus, exportCodeTypes?[0]);

			var importCodeTypeProperty = customsStatusStoreType.GetProperty("ImportCodeTypes", BindingFlags.NonPublic | BindingFlags.Instance);
			var importCodeTypes = importCodeTypeProperty?.GetValue(customsStatusStore) as string[];
			AssertNotNull(importCodeTypes);
			Assert(importCodeTypes?.Length == 1);
			AssertEquals(RefCusCodeListTypes.Codes.CustomsStatus, importCodeTypes?[0]);

			var getCodeTypeForImportProperty = customsStatusStoreType.GetMethod("GetCodeTypeForImport", BindingFlags.NonPublic | BindingFlags.Instance);
			var codeTypeForImport = getCodeTypeForImportProperty?.Invoke(customsStatusStore, new object[] { true }) as string;
			AssertNotNull(codeTypeForImport);
			AssertEquals(RefCusCodeListTypes.Codes.CustomsStatus, codeTypeForImport);
			codeTypeForImport = getCodeTypeForImportProperty?.Invoke(customsStatusStore, new object[] { false }) as string;
			AssertNotNull(codeTypeForImport);
			AssertEquals(RefCusCodeListTypes.Codes.CustomsStatus, codeTypeForImport);

			var countryCodeProperty = customsStatusStoreType.GetProperty("CountryCode", BindingFlags.NonPublic | BindingFlags.Instance);
			var countryCode = (ZString?)countryCodeProperty?.GetValue(customsStatusStore);
			AssertNotNull(countryCode);
			AssertEquals(CountryCodes.Ireland, countryCode);
		}
	}
}
