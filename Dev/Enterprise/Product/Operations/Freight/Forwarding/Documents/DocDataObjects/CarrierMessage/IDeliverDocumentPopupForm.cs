using System;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IDeliverDocumentPopupForm : IDisposable
	{
		DeliverDocumentPopupAction ShowDialogAndGetResult();
	}
}
