using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface ICommissionCreatorProvider
	{
		ICommissionCreator GetCommissionCreator(ICommissionableTransaction transaction);
		IReversalTransactionCommissionCreator GetReversalTransactionCommissionCreator(ICommissionableTransaction transaction);
		IJobClosedCommissionCreator GetJobClosedCommissionCreator(IJobHeader jobHeader, ZDateTime jobCloseTime);
		ICommissionRegenerator GetCommissionRegenerator(IJobHeader jobHeader, BusinessObjectFactory factory, ILogger logger = null);
		ICommissionRegenerator GetCommissionRegenerator(ICommissionableTransaction transaction, BusinessObjectFactory factory, ILogger logger = null);
	}
}
