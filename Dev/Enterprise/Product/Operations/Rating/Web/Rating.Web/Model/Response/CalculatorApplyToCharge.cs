namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the details and attributes used in charge codes / types the CW Calculator applies to.
	/// </summary>
	public class CalculatorApplyToCharge
	{
		/// <summary>
		/// Type of how/whether Charge Code/Type calculator is applied.
		/// Value Reference: 'COD' (Charge Code), 'ALL' (All Charge Codes), 'FRT' (Freight Charges), 'ORG' (Origin Charges), 'DST' (Destination Charges), 'LOD' (Loading Charges), 'OBR' (Origin Customs Brokerage Charges), 'BRK' (Customs Brokerage Charges), 'UNL' (Unloading Charges), 'DSB' (Disbursements), 'CUD' (Customs Disbursements), 'VAL' (Value of Goods), 'INS' (Insurance Value), 'CUV' (Customs Value), 'INV' (Invoice Value), 'SEQ' (Calculation Priority).
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Charge Code that calculator to be applied to (applicable to ApplyType = 'COD' only).
		/// Value Reference: CW > Maintain > Account > Global / (Local) Charge Codes > Codes.
		/// </summary>
		public string ApplyCharge { get; set; }

		/// <summary>
		/// Calculation Priority.
		/// </summary>
		public int? CalculationPriority { get; set; }
	}
}
