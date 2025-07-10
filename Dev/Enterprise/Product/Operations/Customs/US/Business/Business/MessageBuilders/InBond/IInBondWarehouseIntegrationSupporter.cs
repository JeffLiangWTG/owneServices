using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	public interface IInBondWarehouseIntegrationSupporter : Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter, IControllerIDProvider
	{
		bool ShouldUpdate(bool isFailure, bool isWithdrawal);
		bool HasBeenWithdrawn { get; }
		bool IsAmendmentError { get; }
		bool IsAmendmentClear { get; }
		bool IsOriginalError { get; }
		bool IsWithdrawalError { get; }
		ZString JobReference { get; }
		ZString InBondNumber { get; }
		GlbBranch Branch { get; }
	}
}
