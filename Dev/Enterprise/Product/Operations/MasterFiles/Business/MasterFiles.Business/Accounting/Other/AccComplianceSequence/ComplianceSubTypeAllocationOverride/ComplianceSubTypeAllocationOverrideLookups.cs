using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeAllocationOverrideConfigurationLookups : ZLookups
	{
		public ComplianceSubTypeAllocationOverrideConfigurationLookups(ComplianceSubTypeAllocationOverrideConfiguration parent)
			: base(parent)
		{
			this.Parent = parent;
		}
		new readonly ComplianceSubTypeAllocationOverrideConfiguration Parent;

		public CodeDescriptionPairList SubTypeList
			=> AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(Parent.Country, LedgerTypes.AccountsReceivable);

		public CodeDescriptionPairList SubTypeInLocalLanguageList
			=> AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(Parent.Country, LedgerTypes.AccountsReceivable);

		public CodeDescriptionPairList AllocationMethodList
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList;

		public GlbBranchDependentCollection BranchList
			=> new GlbBranchDependentCollection(Parent.Company, new ReadOnlyBusinessObjectFactory());
	}
}
