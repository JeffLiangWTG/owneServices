using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class EDICodeMappingRelationshipLocalCodeModuleFilterForTest : EDICodeMappingRelationshipLocalCodeModuleFilter
	{
		public EDICodeMappingRelationshipLocalCodeModuleFilterForTest(ZString description, OrgPatternMatchOverride orgPatternMatchOverride)
			: base(description, orgPatternMatchOverride)
		{ }

		public ModuleFilterValidation GetNewValidationForTest() => GetNewValidation();
	}
}
