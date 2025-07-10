using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	public interface IPickFaceCreateTransfers
	{
		void CreateAndSaveTransfers(IEnumerable<IPickFaceInfo> pickFaceInfos, IWhsDocketCreationLogger logger, bool allowMultipleTransfersToReplenish = false);
	}
}
