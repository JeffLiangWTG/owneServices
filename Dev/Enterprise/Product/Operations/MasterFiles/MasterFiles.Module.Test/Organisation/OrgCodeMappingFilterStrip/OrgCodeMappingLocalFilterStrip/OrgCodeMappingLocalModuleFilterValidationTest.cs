using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgCodeMappingLocalModuleFilterValidationTest : OrgCodeMappingForeignBaseFilterValidationTest<OrgCodeMappingLocalModuleFilter>
	{
		public void TestCheckLocalCode()
		{
			Filter.LocalCode = ZGuid.BrettsGuid;
			AssertNoErrors(Filter.LocalCodeInfo);
			Filter.LocalCode = ZGuid.Invalid;
			AssertHasError(Filter.LocalCodeInfo, "Enter a valid selection.");
			Filter.LocalCode = ZGuid.Empty;
			AssertNoErrors(Filter.LocalCodeInfo);
		}

		#region Implementation

		protected override OrgCodeMappingLocalModuleFilter GetFilterForTest() => new OrgCodeMappingLocalModuleFilter("Code Mapping (Local Code)");

		#endregion
	}
}
