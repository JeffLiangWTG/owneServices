using System;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IOrdersDocumentSupport
	{
		DocumentWrapper[] GetPackageLabelDocumentWrappers();
		DocumentWrapper[] GetDeliveryLabelDocumentWrappers(Action<object, WhsOrderToPrintEventArgs> onPrintEvent);
	}
}
