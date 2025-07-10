namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	using CargoWise.Types;

	/// <summary>
	/// Submitter, coded an..17.
	/// </summary>
	public interface ITSWSubmitter
	{
		ZString SubmitterCode { get; }
	}
}
