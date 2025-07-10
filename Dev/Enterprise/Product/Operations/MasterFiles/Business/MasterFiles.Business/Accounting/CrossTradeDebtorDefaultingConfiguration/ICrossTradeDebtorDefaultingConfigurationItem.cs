namespace Enterprise.MasterFiles.Business
{
	public interface ICrossTradeDebtorDefaultingConfigurationItem
	{
		string JobTypeCode { get; }

		string TransportModeCode { get; }

		bool IsCollect { get; }

		bool IsPrepaid { get; }

		ChargedPartyForCrossTradeJob BillToParty { get; }
	}

	public enum ChargedPartyForCrossTradeJob
	{
		Agent = 0,
		LocalClient = 1,
		ControllingCustomerFallingBackToLocalClient = 2,
		ControllingCustomerFallingBackToAgent = 3,
		Unknown = 4
	}
}
