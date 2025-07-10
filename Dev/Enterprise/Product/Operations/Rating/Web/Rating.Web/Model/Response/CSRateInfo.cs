namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the attributes and pricing information specific to CargoSphere. It is included in the Rate class applicable to the CargoSphere rates responded by the Rates API.
	/// </summary>
	public class CSRateInfo
	{
		/// <summary>
		/// Tradelane.
		/// </summary>
		public string TradeLane { get; set; }

		/// <summary>
		/// Routing information, same as displaying on CargoWise Rate Selector.
		/// </summary>
		public string Routing { get; set; }

		/// <summary>
		/// Named Accounts.
		/// </summary>
		public string[] NamedAccounts { get; set; }

		/// <summary>
		/// Commodity.
		/// </summary>
		public string Commodity { get; set; }

		/// <summary>
		/// Service String.
		/// </summary>
		public string ServiceString { get; set; }

		/// <summary>
		/// Rate Type.
		/// </summary>
		public string RateType { get; set; }

		/// <summary>
		/// Rate Type 2.
		/// </summary>
		public string RateType2 { get; set; }

		/// <summary>
		/// Vessel information.
		/// </summary>
		public string Vessel { get; set; }

		/// <summary>
		/// Add-on information.
		/// </summary>
		public string AddOn { get; set; }
	}
}
