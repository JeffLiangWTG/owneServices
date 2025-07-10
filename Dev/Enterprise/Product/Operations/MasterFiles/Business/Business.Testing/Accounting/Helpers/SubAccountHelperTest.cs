using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SubAccountHelperTest : TestCase
	{
		public void TestAllSubAccountTypesAreConvertedBothWays()
		{
			var subAccountTypeList = new AccountingMasterFilesConstants.SubAccountTypeList();
			foreach (CodeDescriptionPair subAccountType in subAccountTypeList)
			{
				var tableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccountType.Code);
				AssertNotEquals("If all sub accuont codes are converted in ConvertSubClassCodeToSubAccountDBParentTableCode then Sub Account code should not be the same as converted table code", subAccountType.Code, tableCode);

				var convertedSubAccountCode = SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(tableCode);
				AssertEquals("If all table codes are converted in ConvertSubAccountDBParentTableCodeToSubClassCode then converted table code should be same as sub account type code", subAccountType.Code, convertedSubAccountCode);
			}
		}
	}
}
