using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefCurrency
	{
		ZGuid PK { get; }
		ZString RX_Code { get; }
		ZString RX_Desc { get; }
		int Decimals { get; }
	}
}
