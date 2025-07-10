using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItem : StronglyTypedRegistryItem<GatewayChargeDefaultInvoiceTargetJobConfigurationCollection>
	{
		public GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new GatewayChargeDefaultInvoiceTargetjobConfigurationRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		public class GatewayChargeDefaultInvoiceTargetjobConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public GatewayChargeDefaultInvoiceTargetjobConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryDataType(), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var defaultCollection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();

				defaultCollection.SuspendValidation();
				GetDefaultValues(defaultCollection);
				defaultCollection.ResumeValidation();
				return defaultCollection;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
			static void GetDefaultValues(GatewayChargeDefaultInvoiceTargetJobConfigurationCollection defaultCollection)
			{
				// default value is blank
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.GatewayChargeDefaultInvoiceTargetJobRegistryItemEditor, Enterprise.Accounting.GUI")]
	class GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GatewayChargeDefaultInvoiceTargetJobConfigurationCollection>
	{
		public GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryDataType()
		{
		}
	}
}
