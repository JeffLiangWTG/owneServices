using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class InvoiceRemittanceConfigurationRegistryItem : StronglyTypedRegistryItem<InvoiceRemittanceConfigurationCollection>
	{
		public InvoiceRemittanceConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new InvoiceRemittanceConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class InvoiceRemittanceConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public InvoiceRemittanceConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new InvoiceRemittanceConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return new InvoiceRemittanceConfigurationCollection(new BusinessObjectFactory());
			}
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.InvoiceRemittanceConfigurationRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class InvoiceRemittanceConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InvoiceRemittanceConfigurationCollection>
	{
		public InvoiceRemittanceConfigurationRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, InvoiceRemittanceConfigurationCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			proposedValue.ForEach(x =>
				{
					var errorMessage = ((InvoiceRemittanceConfiguration)x).ValidateOrdersForIncludedElements();
					if (!string.IsNullOrEmpty(errorMessage))
					{
						throw new RegistryValidationException(errorMessage);
					}
				}
			);
		}
	}
}
