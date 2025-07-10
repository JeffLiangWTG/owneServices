using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	public class TransportReferenceNumberTypesRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<TransportReferenceNumberTypeCollection, TransportReferenceNumberTypeCollection>
	{
		public TransportReferenceNumberTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, TransportReferenceNumberTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TransportReferenceNumberTypesDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}

		public TransportReferenceNumberTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, TransportReferenceNumberTypeCollection defaultValue, RegistryOptions registryOptions)
			: base(new RegistryItemImpl(name, category, caption, hint, new TransportReferenceNumberTypesDataType(), storage, registryOptions, defaultValue))
		{
		}

		protected override object ValueCore
		{
			get
			{
				var value = base.ValueCore as TransportReferenceNumberTypeCollection;

				if (value != null)
				{
					var defaults = DefaultValue;
					if (defaults != null)
					{
						FixMissingDefaultsAndSetSystemDefined(value, defaults);
					}
				}

				return value;
			}
		}

		protected override object GetValueWithoutFallbackCore(System.Guid companyPK, System.Guid branchPK, System.Guid departmentPK)
		{
			var value = base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as TransportReferenceNumberTypeCollection;

			if (value != null)
			{
				var defaults = DefaultValue;
				if (defaults != null)
				{
					FixMissingDefaultsAndSetSystemDefined(value, defaults);
				}
			}

			return value;
		}

		static void FixMissingDefaultsAndSetSystemDefined(TransportReferenceNumberTypeCollection value, TransportReferenceNumberTypeCollection defaults)
		{
			foreach (TransportReferenceNumberType defaultItem in defaults.Cast<TransportReferenceNumberType>().Where(d => d.SystemDefined))
			{
				var item = (TransportReferenceNumberType)value.FindByCode(defaultItem.Code)
					?? value.Add(defaultItem.Code, defaultItem.Description, defaultItem.IsUnique);

				item.SystemDefined = defaultItem.SystemDefined;
			}
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor(" Enterprise.TransportCommon.GUI.Registry.TransportReferenceNumberTypesRegistryItemEditor, Enterprise.TransportCommon.GUI")]
	public class TransportReferenceNumberTypesDataType : NonPersistentBusinessObjectRegistryDataType<TransportReferenceNumberTypeCollection>
	{
	}
}
