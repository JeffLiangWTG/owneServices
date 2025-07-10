using CargoWise.Organizations.CodeGeneration;
using CargoWise.Organizations.PatternMatching;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class OrgCodeGeneratorParameters : IOrgCodeGeneratorParameters
	{
		IOrgCodeAlgorithm defaultAlgorithm;
		IOrgCodeAlgorithm overrideAlgorithm;

		public IOrgCodeAlgorithm DefaultAlgorithm
		{
			get { return defaultAlgorithm ?? (defaultAlgorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value); }
		}

		public int MaxLength
		{
			get { return OrgHeaderSchema.OH_Code.MaxLength; }
		}

		public IOrgPatternLanguageSettingFactory OrgPatternLanguageSettingFactory
		{
			get { return OrgPatternLanguageSetting.FactoryInstance; }
		}

		public IOrgCodeAlgorithm OverrideAlgorithm
		{
			get { return overrideAlgorithm ?? (overrideAlgorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.Value); }
		}
	}
}
