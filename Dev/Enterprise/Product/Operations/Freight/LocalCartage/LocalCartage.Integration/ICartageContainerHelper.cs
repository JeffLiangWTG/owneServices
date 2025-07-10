namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface ICartageContainerHelper
	{
		void DeleteAllBookedMoves(bool forDataRefresh = false);
		ICommonCartage DestinationCartage { get; }
	}
}
