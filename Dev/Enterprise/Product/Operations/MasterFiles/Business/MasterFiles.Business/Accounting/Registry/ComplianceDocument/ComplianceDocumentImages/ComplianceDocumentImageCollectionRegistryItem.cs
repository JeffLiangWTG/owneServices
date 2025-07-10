using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceDocumentImageCollectionRegistryItem : ComplianceDocumentImageCollectionRegistryItem<ComplianceDocumentImageCollection>
	{
		public ComplianceDocumentImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new ComplianceDocumentImageRegistryItemImpl(name, category, caption, hint, new ComplianceDocumentImageCollectionRegistryDataType(), storage))
		{
		}

		public ComplianceDocumentImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceDocumentImageRegistryItemImpl(name, category, caption, hint, new ComplianceDocumentImageCollectionRegistryDataType(), storage, options))
		{
		}

		protected override ComplianceDocumentImageCollection GetEmptyValue()
		{
			return new ComplianceDocumentImageCollection();
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.ComplianceDocumentImageCollectionRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class ComplianceDocumentImageCollectionRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<ComplianceDocumentImageCollection>
	{
		public ComplianceDocumentImageCollectionRegistryDataType()
		{
		}

		protected override bool ValuesAreEqualCore(ComplianceDocumentImageCollection a, ComplianceDocumentImageCollection b)
		{
			return a.ContainsSameElementsInAnyOrder(b);
		}
	}

	public abstract class ComplianceDocumentImageCollectionRegistryItem<T> : StronglyTypedRegistryItem<T> where T : ComplianceDocumentImageCollection
	{
		internal ComplianceDocumentImageCollectionRegistryItem(ComplianceDocumentImageRegistryItemImpl inner)
			: base(inner)
		{
		}

		protected abstract T GetEmptyValue();

		internal class ComplianceDocumentImageRegistryItemImpl : RegistryItemImpl
		{
			public ComplianceDocumentImageRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, category, caption, hint, dataType, storage)
			{
			}

			public ComplianceDocumentImageRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, dataType, storage, options)
			{
			}

			protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
			{
				var collection = (ComplianceDocumentImageCollection)newValue;
				base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, collection);
				collection.CleanUpDeletedElements();
			}

			protected override void DeleteValueCore(Guid companyPk, Guid branchPk, Guid departmentPk)
			{
				var collection = (ComplianceDocumentImageCollection)GetValueWithoutFallback(companyPk, branchPk, departmentPk);
				base.DeleteValueCore(companyPk, branchPk, departmentPk);
				collection.RemoveAndDeleteAll();
				collection.CleanUpDeletedElements();
			}
		}
	}
}
