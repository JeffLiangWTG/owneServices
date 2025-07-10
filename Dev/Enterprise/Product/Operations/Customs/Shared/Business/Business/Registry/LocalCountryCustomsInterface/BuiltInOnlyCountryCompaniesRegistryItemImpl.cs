using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.Customs.DataRegistry.Business
{
	public class NonBuiltInOnlyCountryRegistryItemImpl : RegistryItemImpl
	{
		public NonBuiltInOnlyCountryRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
			: base(name, category, caption, hint, dataType, storage, options, defaultValue)
		{
		}
	}
}
