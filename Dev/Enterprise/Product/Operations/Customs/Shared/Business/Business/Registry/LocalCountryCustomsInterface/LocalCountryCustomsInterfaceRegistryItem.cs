using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.CustomsWare;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class LocalCountryCustomsInterfaceRegistryItem : StronglyTypedRegistryItem<LocalCountryCustomsInterface>, IRegistryItemWithOtherChangedItems
	{
		public LocalCountryCustomsInterfaceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new NonBuiltInOnlyCountryRegistryItemImpl(name, category, caption, hint, new LocalCountryCustomsInterfaceRegistryItemDataType(), storage, RegistryOptions.Default, new LocalCountryCustomsInterfaceRegistryItemDataType().DefaultValue))
		{
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }
	}

	[RegistryEditor("Enterprise.Customs.DataRegistry.GUI.LocalCountryCustomsInterfaceRegistryItemEditor, Enterprise.Customs.GUI")]
	public class LocalCountryCustomsInterfaceRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<LocalCountryCustomsInterface>
	{
		protected override void ValidateCore(IRegistryItem registryItem, LocalCountryCustomsInterface proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			var registryItemWithOtherChangedItems = registryItem as IRegistryItemWithOtherChangedItems;
			if (registryItemWithOtherChangedItems != null && proposedValue.CustomsInterfaceIsActivated())
			{
				var company = Factory.Load<GlbCompany>(companyPK);
				if (company != null)
				{
					var customsWareRegistry = ObjectFactory.Get<ICustomsWareRegistry>();
					if (registryItemWithOtherChangedItems.IsRegistryItemConfiguredInMutuallyExclusiveItems(company.PK.ToGuid(), IntegratedCountryHelper.IsABMInterfaceActivatedByCompany(company.GC_RN_NKCountryCode, company.PK.ToGuid()),
						new ZString[] { CustomsDataRegistry.Instance.CustomsWareCompany.Name,
							customsWareRegistry.CustomsWareSiteID.Name,
							customsWareRegistry.UserName.Name,
							customsWareRegistry.Password.Name }))
					{
						throw new RegistryValidationException(CannotConfigureInterfaceForCompany(company.GC_Code, "ABM"));
					}
				}
			}
		}

		BusinessObjectFactory Factory => factory = factory ?? new BusinessObjectFactory();
		BusinessObjectFactory factory;

		internal static string CannotConfigureInterfaceForCompany(ZString companyCode, ZString countryName)
		{
			return Res.GetString("e8ee4365-5af9-4ff7-bcf5-7cbce2e1298c", "Cannot configure an interface for company {0} because a configuration already exists under '{1}' for the same company.", companyCode, countryName);
		}
	}
}
