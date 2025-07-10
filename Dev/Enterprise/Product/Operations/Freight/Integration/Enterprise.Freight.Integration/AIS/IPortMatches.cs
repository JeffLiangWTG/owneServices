namespace Enterprise.Freight.Integration
{
	public interface IPortMatches
	{
		IPortMatch LastForeignPort { get; }
		IPortMatch FirstArrivalPort { get; }
	}
}
