using CargoWise.EntityFramework;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVFilterProviderForDeclaration
	{
		void AddHVLVFilters(IModuleFilterCollection filterCollection, BusinessObjectFactory factory);
	}
}
