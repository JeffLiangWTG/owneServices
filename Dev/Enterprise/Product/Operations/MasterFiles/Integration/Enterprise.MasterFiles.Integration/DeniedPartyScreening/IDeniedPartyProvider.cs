using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDeniedPartyProvider
	{
		ZString ReferenceId { get; }
	}
}
