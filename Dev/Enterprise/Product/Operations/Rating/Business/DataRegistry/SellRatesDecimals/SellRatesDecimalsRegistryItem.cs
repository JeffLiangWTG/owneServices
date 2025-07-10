using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class SellRatesDecimalsRegistryItem : StronglyTypedRegistryItem<SellRatesDecimalsCollection>
	{
		public SellRatesDecimalsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, SellRatesDecimalsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SellRatesDecimalsRegistryDataType(), storage, defaultValue))
		{
		}

		const int defaultNumberOfDecimals = 4;

		public int GetDecimals(ZString rateCategory)
		{
			var result = GetNumberOfDecimalsAllowed(rateCategory);

			if (result == 0)
			{
				result = DefaultNumberOfDecimalsAllowed;
			}

			return result;
		}

		int DefaultNumberOfDecimalsAllowed
		{
			get
			{
				var result = GetNumberOfDecimalsAllowed(RatingConstants.RateCategory.ALL);
				return result == 0 ? defaultNumberOfDecimals : result;
			}
		}

		int GetNumberOfDecimalsAllowed(ZString rateCategory)
		{
			var setting = Value.Cast<SellRatesDecimals>().FirstOrDefault(x => x.Code == rateCategory);
			return setting != null ? Int32.Parse(setting.Decimals, CultureInfo.InvariantCulture) : 0;
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.SellRatesDecimalsRegistryItemEditor, Enterprise.Rating.GUI")]
	class SellRatesDecimalsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SellRatesDecimalsCollection>
	{
		public SellRatesDecimalsRegistryDataType()
		{
		}
	}
}

