using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IPhaseSecuritySupportable : IBusiness
	{
		bool IsReadOnlyDueToPhase { get; }
		bool IsPropertyReadOnlyDueToPhase(ZString propertyName);
	}
}
