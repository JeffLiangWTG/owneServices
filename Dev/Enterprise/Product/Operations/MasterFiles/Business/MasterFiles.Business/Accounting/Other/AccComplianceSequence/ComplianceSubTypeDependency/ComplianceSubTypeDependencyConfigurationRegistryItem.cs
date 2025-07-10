using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeDependencyConfigurationRegistryItem : StronglyTypedRegistryItem<ComplianceSubTypeDependencyConfigurationCollection>
	{
		public ComplianceSubTypeDependencyConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceSubTypeDependencyConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class ComplianceSubTypeDependencyConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public ComplianceSubTypeDependencyConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new ComplianceSubTypeDependencyConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				ComplianceSubTypeDependencyConfigurationCollection defaultCollection = new ComplianceSubTypeDependencyConfigurationCollection();

				defaultCollection.SuspendValidation();

				GlbCompany company = null;
				if (companyPK != Guid.Empty)
				{
					company = new BusinessObjectFactory().Load<GlbCompany>(companyPK);
				}
				if (company != null)
				{
					CountryComplianceFactory.GetIComplianceSubTypeDependencyConfigurationProvider(company.Country.Code)?.GetDefaults(defaultCollection);
				}

				defaultCollection.ResumeValidation();
				return defaultCollection;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ComplianceSubTypeDependencyConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	class ComplianceSubTypeDependencyConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceSubTypeDependencyConfigurationCollection>
	{
		public ComplianceSubTypeDependencyConfigurationRegistryDataType()
		{
		}
	}
}
