namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the attributes and measurements of each Packing Line used for calculating charges from searched rates.
	/// </summary>
	public class JobPackLine
	{
		/// <summary>
		/// Used as Units for calculating buy and/or sell rates from CW.
		/// Value Reference: CW > Maintain > Reference Files > Package Types > Code
		/// </summary>
		public string PackageType { get; set; }

		/// <summary>
		/// Number of Packages under the Package Type.
		/// </summary>
		public int Unit { get; set; }

		/// <summary>
		/// Match against the Commodity for searching of rates.
		/// Value Reference: CW > Maintain > Reference Files > Commodities > Code
		/// </summary>
		public string Commodity { get; set; }

		/// <summary>
		/// Weight of the package.
		/// </summary>
		public decimal? Weight { get; set; }

		/// <summary>
		/// Unit of Measure for the Weight.
		/// Value Reference: 'DT' (Decitons), 'G' (Grams), 'HG' (Hectograms), 'KG' (Kilograms), 'LB' (Pounds), 'LT' (Pounds Troy), 'MC' (Metric Carat), 'MG' (Milligrams), 'OT' (Ounces Troy), 'OZ' (Ounces), 'T' (Tons), 'TL' (Long Tons - 2240 lb), 'TN' (Short Tons - 2000 lb)
		/// </summary>
		public string WeightUnit { get; set; }

		/// <summary>
		/// Volume of the Package.
		/// </summary>
		public decimal? Volume { get; set; }

		/// <summary>
		/// Unit of Measure for the Volume.
		/// Value Reference: 'CC' (Cubic Centimetres), 'CF' (Cubic Feet), 'CI' (Cubic Inches), 'CY' (Cubic Yards), 'D3' (Cubic Decimetres), 'GA' (US Gallons), 'GI' (Imperial Gallons), 'L' (Litre), 'M3' (Cubic Metres), 'ML' (Mega Litre), 'TE' (Tea Chest)
		/// </summary>
		public string VolumeUnit { get; set; }

		/// <summary>
		/// DG Substance of the packline.
		/// Value Reference: CargoWise > Forwarding > Shipment > Packing> Dangerous Goods.
		/// </summary>
		public string DGSubstance { get; set; }

		/// <summary>
		/// DG Class of the packline.
		/// Value Reference: CargoWise > Forwarding > Shipment > Packing> Dangerous Goods.
		/// </summary>
		public string DGClass { get; set; }
	}
}
