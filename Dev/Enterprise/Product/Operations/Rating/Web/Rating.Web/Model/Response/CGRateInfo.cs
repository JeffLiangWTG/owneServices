using WiseRates.Api.Model;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the attributes and pricing information specific to Cargoguide. It is included in the Rate class applicable to the Cargoguide rates responded by the Rates API.
	/// </summary>
	public class CGRateInfo
	{
		/// <summary>
		/// Issue Date
		/// </summary>
		public string IssueDate { get; set; }

		/// <summary>
		/// Payment Terms.
		/// Value Reference: PPD(Prepaid), CCX(Collect).
		/// </summary>
		public string PaymentTerm { get; set; }

		/// <summary>
		/// Origin.
		/// </summary>
		public string Origin { get; set; }

		/// <summary>
		/// Name of Origin.
		/// </summary>
		public string OriginName { get; set; }

		/// <summary>
		/// Origin City.
		/// </summary>
		public string OriginCity { get; set; }

		/// <summary>
		/// Origin City Name.
		/// </summary>
		public string OriginCityName { get; set; }

		/// <summary>
		/// Port of Loading.
		/// </summary>
		public string POL { get; set; }

		/// <summary>
		/// Destination
		/// </summary>
		public string Destination { get; set; }

		/// <summary>
		/// Name of Destination.
		/// </summary>
		public string DestinationName { get; set; }

		/// <summary>
		/// Destination City.
		/// </summary>
		public string DestinationCity { get; set; }

		/// <summary>
		/// Destination City Name
		/// </summary>
		public string DestinationCityName { get; set; }

		/// <summary>
		/// Port of Discharge.
		/// </summary>
		public string POD { get; set; }

		/// <summary>
		/// Deck Information.
		/// </summary>
		public string Deck { get; set; }

		/// <summary>
		/// Ratio.
		/// </summary>
		public string Ratio { get; set; }

		/// <summary>
		/// Remarks.
		/// </summary>
		public string Remarks { get; set; }

		/// <summary>
		/// Rate Class.
		/// </summary>
		public string RateClass { get; set; }

		/// <summary>
		/// Reference.
		/// </summary>
		public string Reference { get; set; }

		/// <summary>
		/// Via.
		/// </summary>
		public string[] Via { get; set; }

		/// <summary>
		/// Product Id.
		/// </summary>
		public string ProductId { get; set; }

		/// <summary>
		/// Product Code.
		/// </summary>
		public string ProductCode { get; set; }

		/// <summary>
		/// Product Name.
		/// </summary>
		public string ProductName { get; set; }

		/// <summary>
		/// Product Class.
		/// </summary>
		public string ProductClass { get; set; }

		/// <summary>
		/// Deck Type of Product.
		/// </summary>
		public string ProductDeck { get; set; }

		/// <summary>
		/// GSA Name of the Service Provider issuing the Cargoguide rate.
		/// </summary>
		public string GSAName { get; set; }

		/// <summary>
		/// Named Accounts.
		/// </summary>
		public string[] NamedAccounts { get; set; }

		/// <summary>
		/// Cargo Aircraft Only.
		/// </summary>
		public bool? CargoAircraftOnly { get; set; }

		/// <summary>
		/// Tempreture Range of the Product.
		/// </summary>
		public TemperatureRange TemperatureRange { get; set; }
	}
}
