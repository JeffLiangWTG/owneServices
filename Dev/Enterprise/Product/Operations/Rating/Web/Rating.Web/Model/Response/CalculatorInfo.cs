using System.Runtime.Serialization;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the attributes and pricing details of the Calculator used in the charge line applicable to the rates responded by the Rates API.
	/// </summary>
	[KnownType(typeof(CalculatorApplyToCharge[]))]
	[KnownType(typeof(CalculatorBreakItem[]))]
	public class CalculatorInfo
	{
		/// <summary>
		/// The 3 characters code used to identify the CW Calculator, such as FLT, UNT, CMB, etc.
		/// </summary>
		public string CWCode { get; set; }

		/// <summary>
		/// Refer to the Agent Rates checkbox to identify if the CalculatorInfo is applicable to Agent Rates or NOT.
		/// Value Reference: CW > Manage > Tariffs and Rates > Costing / Client Rates / Company Tariffs / Intercompany Tariffs > Charge(Rate Line) > Agent Rates.
		/// </summary>
		public bool IsAgentRate { get; set; }

		/// <summary>
		/// Label and corresponding values of the attributes supported in the corresponding Calculator.
		/// </summary>
		public CalculatorAttribute[] Attributes { get; set; }
	}
}
