using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.MasterFiles.Integration
{
	public interface IMoney
	{
		ZDecimal Amount { get; }
		ICurrency Currency { get; }
	}
}
