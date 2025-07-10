using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EPaymentConfigurationRegistryItem : StronglyTypedRegistryItem<EPaymentConfigurationCollection>
	{
		public EPaymentConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new EPaymentConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class EPaymentConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public EPaymentConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new EPaymentConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var configCollection = new EPaymentConfigurationCollection();
				if (companyPK != Guid.Empty)
				{
					var companyCountry = new BusinessObjectFactory().Load<GlbCompany>(companyPK)?.Country;
					if (companyCountry != null)
					{
						var countryCode = companyCountry.Code;
						var countryDescription = companyCountry.Description;
						configCollection.PopulateDefaultConfiguration(countryCode, countryDescription);
					}
				}
				return configCollection;
			}
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.EPaymentConfigurationRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class EPaymentConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EPaymentConfigurationCollection>
	{
		public EPaymentConfigurationRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, EPaymentConfigurationCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue != null && proposedValue.Count == 0)
			{
				throw new RegistryValidationException(Res.GetString("0A6BE2E3-E9E1-4151-A889-FA28F8BD00A4", "No data found to override. Kindly set the proper values."));
			}
		}
	}
}
