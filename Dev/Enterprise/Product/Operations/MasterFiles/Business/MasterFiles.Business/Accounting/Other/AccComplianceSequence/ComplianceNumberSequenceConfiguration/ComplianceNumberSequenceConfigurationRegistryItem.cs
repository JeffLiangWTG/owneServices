using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ComplianceNumberSequenceConfigurationRegistryItem : StronglyTypedRegistryItem<ComplianceNumberSequenceConfigurationCollection>
	{
		public ComplianceNumberSequenceConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceNumberSequenceConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var collection = base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as ComplianceNumberSequenceConfigurationCollection;
			collection.AddMandatoryComplianceNumberSequenceConfiguration(new BusinessObjectFactory(), companyPK);
			return collection;
		}

		class ComplianceNumberSequenceConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public ComplianceNumberSequenceConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new ComplianceNumberSequenceConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var factory = new BusinessObjectFactory();
				var defaultCollection = new ComplianceNumberSequenceConfigurationCollection(factory);
				defaultCollection.AddMandatoryComplianceNumberSequenceConfiguration(factory, companyPK);
				return defaultCollection;
			}
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.ComplianceNumberSequenceConfigurationRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class ComplianceNumberSequenceConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceNumberSequenceConfigurationCollection>
	{
		public ComplianceNumberSequenceConfigurationRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, ComplianceNumberSequenceConfigurationCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			proposedValue.ForEach(x =>
			{
				var errorMessage = ((ComplianceNumberSequenceConfiguration)x).ValidateOrdersForIncludedElements();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					throw new RegistryValidationException(errorMessage);
				}
			}
			);
		}
	}
}
