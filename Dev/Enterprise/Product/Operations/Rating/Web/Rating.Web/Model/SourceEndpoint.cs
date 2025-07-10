using Enterprise.Rating.Business;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// SourceEndpoint is an enum that indicates the endpoints of Rates APIs.
	/// </summary>
	public enum SourceEndpoint
	{
		/// <summary>
		/// Costing
		/// </summary>
		Costing,

		/// <summary>
		/// ClientRates
		/// </summary>
		ClientRates,

		/// <summary>
		/// CompanyTariffs
		/// </summary>
		CompanyTariffs,

		/// <summary>
		/// IntercompanyTariffs
		/// </summary>
		IntercompanyTariffs,

		/// <summary>
		/// JobCharges
		/// </summary>
		JobCharges
	}

	internal static class SourceEndpointExtensions
	{
		public static string GetRateType(this SourceEndpoint sourceEndpoint)
		{
			switch (sourceEndpoint)
			{
				case SourceEndpoint.Costing:
					return RatingConstants.RatingHeaderTypes.Costing;
				case SourceEndpoint.ClientRates:
					return RatingConstants.RatingHeaderTypes.ClientRate;
				case SourceEndpoint.CompanyTariffs:
					return RatingConstants.RatingHeaderTypes.Tariff;
				case SourceEndpoint.IntercompanyTariffs:
					return RatingConstants.RatingHeaderTypes.IntercompanyTariff;
				default:
					return string.Empty;
			}
		}
	}
}
