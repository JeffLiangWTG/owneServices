using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeAllocationOverrideConfigurationRegistryItem : StronglyTypedRegistryItem<ComplianceSubTypeAllocationOverrideConfigurationCollection>
	{
		public ComplianceSubTypeAllocationOverrideConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceSubTypeAllocationOverrideConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class ComplianceSubTypeAllocationOverrideConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public ComplianceSubTypeAllocationOverrideConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new ComplianceSubTypeAllocationOverrideConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var fallback = new FallbackLevel(companyPK, branchPK, departmentPK);
				var countryCode = GetCountryCodeFrom(fallback, RegistryFactory.Instance);
				var defaultCollection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(fallback, RegistryFactory.Instance, countryCode);

				if (companyPK != Guid.Empty && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK))
				{
					(ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(countryCode) as IComplianceSubTypeAllocationOverrideConfigurationProvider)?
						.GetDefaults(defaultCollection);
				}

				return defaultCollection;
			}
		}

		internal static ZString GetCountryCodeFrom(FallbackLevel fallback, BusinessObjectFactory factory)
		{
			var companyPK = fallback.CompanyPK(returnEmptyIfBranchPKIsPresent: false);

			if (companyPK != Guid.Empty)
			{
				var company = companyPK == GlbCompany.CurrentCompany.PK ? GlbCompany.CurrentCompany : factory.Load<GlbCompany>(companyPK);
				var countryCode = company?.GC_RN_NKCountryCode ?? ZString.Empty;

				return countryCode;
			}

			return ZString.Empty;
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ComplianceSubTypeAllocationOverrideConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	class ComplianceSubTypeAllocationOverrideConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceSubTypeAllocationOverrideConfigurationCollection>
	{
	}
}
