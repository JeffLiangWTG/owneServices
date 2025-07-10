namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the attributes supported in each corresponding CW Calculator. For attribute with True/False or Boolean type of value, such attribute is available only if the value is True.
	/// </summary>
	public class CalculatorAttribute
	{
		/// <summary>
		/// Name of the attribute used in the corresponding CW Calculator.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Type of the attribute value.
		/// It can be one of : string, boolean, decimal, integer, CalculatorBreakItem, CalculatorApplyToCharges
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Actual value of the attribute.
		/// For Type is 'Breaks', this value will be an object of CalculatorBreak
		/// For Type is 'CalculatorApplyToCharges', this value will be an array of CalculatorApplyToCharge object
		/// </summary>
		public object Value { get; set; }
	}
}
