namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	using System.Globalization;

	using CargoWise.Types;

	public class FlightRequest
	{
		public FlightRequest(ZDate date, ZString airline, int flightNumber)
		{
			if (!date.IsValid)
			{
				throw new System.ArgumentException("Invalid argument.", nameof(date)); // Hard codded exception message
			}

			if (airline.Length != 2)
			{
				throw new System.ArgumentException("Invalid argument.", nameof(airline)); // Hard codded exception message
			}

			if (!(flightNumber > 0 && flightNumber <= 9999))
			{
				throw new System.ArgumentException("Invalid argument.", nameof(flightNumber)); // Hard codded exception message
			}

			Date = date.ToDateTime().ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
			Airline = airline;
			FlightNumber = flightNumber;
		}

		public string Date { get; }
		public string Airline { get; }
		public int FlightNumber { get; }
	}
}
