namespace Enterprise.Freight.Integration
{
	public interface ICO2eProvider : ICO2eParent
	{
		bool RequireTEU { get; }
		void RecordLog(CO2eEventType type, string extra, decimal previousCO2eValue = 0);
	}
}
