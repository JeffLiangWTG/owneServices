using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.ARAPDefaultTaxRecognitionRuleLookups;

namespace Enterprise.Registry.Business
{
	public class ARDefaultTaxRecognitionRuleRegistryItem : StronglyTypedRegistryItem<ARAPDefaultTaxRecognitionRuleCollection>
	{
		public ARDefaultTaxRecognitionRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ARDefaultTaxRecognitionRuleRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class ARDefaultTaxRecognitionRuleRegistryItemImpl : RegistryItemImpl
		{
			public ARDefaultTaxRecognitionRuleRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new ARAPDefaultTaxRecognitionRuleRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				ARAPDefaultTaxRecognitionRuleCollection defaultCollection = new ARAPDefaultTaxRecognitionRuleCollection();

				defaultCollection.SuspendValidation();

				var configuration = defaultCollection.AddNew();
				configuration.LoginCompanyCountryRuleCode = LoginCompanyCountryRuleCode.IEU;
				configuration.OrganizationCountryRuleCode = OrganizationCountryRuleCode.IEU;
				configuration.TaxRecognitionCode = Constants.OrganisationTaxConfiguartionTypes.Default;

				configuration = defaultCollection.AddNew();
				configuration.LoginCompanyCountryRuleCode = LoginCompanyCountryRuleCode.IEU;
				configuration.OrganizationCountryRuleCode = OrganizationCountryRuleCode.OEU;
				configuration.TaxRecognitionCode = Constants.OrganisationTaxConfiguartionTypes.Default;

				configuration = defaultCollection.AddNew();
				configuration.LoginCompanyCountryRuleCode = LoginCompanyCountryRuleCode.OEU;
				configuration.OrganizationCountryRuleCode = OrganizationCountryRuleCode.SAL;
				configuration.TaxRecognitionCode = Constants.OrganisationTaxConfiguartionTypes.Default;

				configuration = defaultCollection.AddNew();
				configuration.LoginCompanyCountryRuleCode = LoginCompanyCountryRuleCode.OEU;
				configuration.OrganizationCountryRuleCode = OrganizationCountryRuleCode.DTL;
				configuration.TaxRecognitionCode = Constants.OrganisationTaxConfiguartionTypes.Default;

				defaultCollection.ResumeValidation();
				return defaultCollection;
			}
		}
	}
}
