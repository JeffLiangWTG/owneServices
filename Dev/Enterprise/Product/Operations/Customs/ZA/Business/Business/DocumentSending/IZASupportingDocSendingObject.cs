using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public interface IZASupportingDocSendingObject : Customs.Business.ISupportingDocObject
	{
		ZString AgentDualProfileCode { get; }
		ZString TradingPartyID { get; }
	}
}
