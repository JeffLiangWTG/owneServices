namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the amount with audit details of the job charges calculated from the buy or sell rates applicable.
	/// </summary>
	public class CalculationItem
	{
		/// <summary>
		/// Currency of the charge line applicable to the rate.
		/// Value Reference: CW > Maintain > Reference Files > Currencies > Code
		/// </summary>
		public string Currency { get; set; }

		/// <summary>
		/// Amount in currency.
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Local amount in currency.
		/// </summary>
		public decimal LocalAmount { get; set; }

		/// <summary>
		/// Local currency of the Company per the user token.
		/// Value Reference: CW > Maintain > Reference Files > Currencies > Code
		/// </summary>
		public string LocalCurrency { get; set; }

		/// <summary>
		/// Exchange rate between Currency and LocalCurrency.
		/// </summary>
		public decimal ExchangeRate { get; set; }

		/// <summary>
		/// Calculation description
		/// </summary>
		public string Audit { get; set; }
	}
}
