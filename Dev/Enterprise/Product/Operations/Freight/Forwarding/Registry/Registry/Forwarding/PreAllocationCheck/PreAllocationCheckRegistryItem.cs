using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class PreAllocationCheckRegistryItem : TranslatableRegistryItem<PreAllocationCheckCollection, PreAllocationCheckCollection>
	{
		public PreAllocationCheckRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PreAllocationCheckCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PreAllocationCheckRegistryDataType(), storage, defaultValue))
		{
			defaultPreAllocationChecks = defaultValue;
		}

		readonly PreAllocationCheckCollection defaultPreAllocationChecks;

		protected override PreAllocationCheckCollection Convert(PreAllocationCheckCollection value)
		{
			foreach (PreAllocationCheck item in value)
			{
				item.MeasureMultilingualString = GetMultilingualString(item.Measure);
			}
			return value;
		}

		public override bool IsTranslatable => true;

		public override IEnumerable<ResourceString> DefaultStrings => defaultPreAllocationChecks.Cast<PreAllocationCheck>().Select(i => i.MeasureMultilingualString).OfType<ResourceString>();

		public override int MaxLength => -1;

		public override IEnumerable<string> GetCaptions(PreAllocationCheckCollection value) => value.Cast<PreAllocationCheck>().Select(i => i.Measure.ToString());
	}

	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.Registry.PreAllocationCheckRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	class PreAllocationCheckRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PreAllocationCheckCollection>
	{
		public PreAllocationCheckRegistryDataType()
		{
		}
	}
}
