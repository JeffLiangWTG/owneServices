using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class DefaultRoundingsRegistryItem : StronglyTypedRegistryItem<DefaultRoundingsCollection>
	{
		public DefaultRoundingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DefaultRoundingsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultRoundingsRegistryDataType(), storage, defaultValue))
		{
		}

		public DefaultRoundingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DefaultRoundingsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultRoundingsRegistryDataType(), storage, options, defaultValue))
		{
		}

		public DefaultRoundings GetDefaultRounding(ZString ratingType)
		{
			var defaultRoundings = Value.Cast<DefaultRoundings>();

			return defaultRoundings.FirstOrDefault(rounding => string.Equals(rounding.Code, ratingType, StringComparison.OrdinalIgnoreCase))
				?? defaultRoundings.FirstOrDefault(rounding => string.Equals(rounding.Code, Core.Constants.RateMode.ALL, StringComparison.OrdinalIgnoreCase))
				?? new DefaultRoundings
				{
					RoundingType = RatingRoundingTypes.NoRounding
				};
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.RoundingsRegistryItemEditor, Enterprise.Rating.GUI")]
	public class DefaultRoundingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultRoundingsCollection>
	{
		public DefaultRoundingsRegistryDataType()
		{
		}
	}
}

