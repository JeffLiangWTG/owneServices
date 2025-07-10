using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

public class CalCalculationMethodRegistryItem : StronglyTypedRegistryItem<CalCalculationMethodRegistryCollection>
{
	public CalCalculationMethodRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CalCalculationMethodRegistryCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CalCalculationMethodRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
	{
	}
}
