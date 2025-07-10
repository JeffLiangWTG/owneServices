namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the details of the Conversion Factor applicable to the charge line of the rates responded by the Rates API. 
	/// </summary>
	public class ConversionFactor
	{
		/// <summary>
		/// Ratio between numerator unit and denominator unit. For example, 166 KG/M3.
		/// </summary>
		public string Factor { get; set; }

		/// <summary>
		/// Numerator unit. 'KG' in above example.
		/// </summary>
		public string NumeratorUnit { get; set; }

		/// <summary>
		/// Denominator unit. 'M3' in above example.
		/// </summary>
		public string DenominatorUnit { get; set; }
	}
}
