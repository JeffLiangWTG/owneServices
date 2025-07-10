namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the details and attributes used in breaks supported in CW Calculators.
	/// </summary>
	public class CalculatorBreakItem
	{
		/// <summary>
		/// Transport Zone Set that current Breaks object is applicable.
		/// Value Reference: CW > Maintain > Locations > Transport Zone Sets > Zones > Zone Name.
		/// </summary>
		public string CWTransportZone { get; set; }

		/// <summary>
		/// Refer to the Operator used in each break of calculator supporting breaks.
		/// Value Reference: CW > Maintain > Client Rates / Company Tariffs / Costing / Intercompany Tariffs > Charge(Rate Line) > Calculator > Break > Operator (Referred as Code for HRT Calculator).
		/// </summary>
		public string Operator { get; set; }

		/// <summary>
		/// Refer to the Break value used in each break of calculator supporting breaks.
		/// Value Reference: CW > Maintain > Client Rates / Company Tariffs / Costing / Intercompany Tariffs > Charge(Rate Line) > Calculator > Break > Break.
		/// </summary>
		public decimal? Break { get; set; }

		/// <summary>
		/// Refer to the Unit Rate / Price used in each break of calculator supporting breaks.
		/// Value Reference: CW > Maintain > Client Rates / Company Tariffs / Costing / Intercompany Tariffs > Charge(Rate Line) > Calculator > Break > Rate.
		/// </summary>
		public decimal? UnitPrice { get; set; }

		/// <summary>
		/// Refer to the Flat Amount used in each break of calculator supporting breaks.
		/// Value Reference: CW > Maintain > Client Rates / Company Tariffs / Costing / Intercompany Tariffs > Charge(Rate Line) > Calculator > Break > Flat Amount.
		/// </summary>
		public decimal? FlatAmount { get; set; }

		/// <summary>
		/// Break Minimum.
		/// </summary>
		public decimal? BreakMinimum { get; set; }

		/// <summary>
		/// Unit Multiple.
		/// </summary>
		public int? UnitMultiple { get; set; }

		/// <summary>
		/// Refer to the Restricted field used in each break of calculator supporting breaks.
		/// Value Reference: CW > Maintain > Client Rates / Company Tariffs / Costing / Intercompany Tariffs > Charge(Rate Line) > Calculator > Break > Restricted.
		/// </summary>
		public bool Restricted { get; set; }

		/// <summary>
		/// Refer to the Reason field used in each break of calculator supporting breaks.
		/// Value Reference: CW > Maintain > Client Rates / Company Tariffs / Costing / Intercompany Tariffs > Charge(Rate Line) > Calculator > Break > Reason.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Refer to the Units used in each break of calculator supporting breaks.
		/// Value Reference: CW > Maintain > Client Rates / Company Tariffs / Costing / Intercompany Tariffs > Charge(Rate Line) > Calculator > Break > Units.
		/// </summary>
		public string Units { get; set; }
	}
}
