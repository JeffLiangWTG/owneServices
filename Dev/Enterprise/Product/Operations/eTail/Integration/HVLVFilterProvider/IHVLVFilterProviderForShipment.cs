using Enterprise.Integration.ZArchitecture;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVFilterProviderForShipment
	{
		void AddHVLVFilters(IModuleFilterCollection filterCollection);
	}
}
