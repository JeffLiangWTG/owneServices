using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgCodeMappingForeignModuleFilterValidationTest : OrgCodeMappingForeignBaseFilterValidationTest<OrgCodeMappingForeignModuleFilter>
	{
		public void TestCheckForeignCode()
		{
			Filter.ForeignCode = new string('a', OrgPatternMatchOverride.Schema.OO_ForeignCodeMaxLength);
			AssertNoErrors(Filter.ForeignCodeInfo);
			Filter.ForeignCode = new string('a', OrgPatternMatchOverride.Schema.OO_ForeignCodeMaxLength + 1);
			AssertHasError(Filter.ForeignCodeInfo, $"Filter exceeds the maximum length for a Foreign Code ({OrgPatternMatchOverride.Schema.OO_ForeignCodeMaxLength}).");
			Filter.ForeignCode = string.Empty;
			AssertNoErrors(Filter.ForeignCodeInfo);
		}

		#region Implementation

		protected override OrgCodeMappingForeignModuleFilter GetFilterForTest() => new OrgCodeMappingForeignModuleFilter("Code Mapping (Foreign Code)");

		#endregion
	}
}
