using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsCarrierLabelsBatchPrintingSubscriber
	{
		IReadOnlyCollection<ITopLevelDataObject> GetCarrierLabelsBatchRequests { get; }
		ReturnResult SubscribePackages(WhsPick pick, IEnumerable<WhsOrderToPackageItemNumbers> packageItemNumbers, IReadOnlyCollection<int> labelSeparatorSegmentNumbers);
	}
}
