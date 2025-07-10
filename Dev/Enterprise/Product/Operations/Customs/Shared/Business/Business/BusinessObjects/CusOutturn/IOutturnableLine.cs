using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Interfaces
{
	public interface IOutturnableLine : ILinkable
	{
		bool IsDeleted { get; }
		ZString UnderbondHumanReadableName { get; }
		ZString CargoStatus { get; }
		ZInt PackagesManifested { get; }
	}
}
