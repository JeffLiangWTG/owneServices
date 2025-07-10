using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.DocumentSending
{
	public interface IZASupportingDocumentMessageDataProvider : ISupportingDocumentMessageDataProvider
	{
		ZString DualProfileCode { get; }
		ZString TradingPartyID { get; }
	}
}
