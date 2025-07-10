using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Interfaces
{
	public interface ICusUnderbondDependentCollectionParent : IBusiness, IOutturnableLine
	{
		CusUnderbondCollection Underbonds { get; }
		ZString Details { get; }
		bool CanSendWithoutDelay { get; }
		IOutturnableLine[] OutturnableLines { get; }
		bool UsesTranshipmentPortOnUnderbond { get; }
		ZString DefaultTranshipmentPort { get; }
	}
}
