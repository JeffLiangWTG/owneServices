namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the calculated amounts or the pricing information of the job charges applicable to the rates responded by the Rate API.
	/// </summary>
	public class ChargeInfo
	{
		/// <summary>
		/// Charge Code of the charge line applicable to the rate.
		/// </summary>
		public ChargeCodeInfo ChargeCode { get; set; }

		/// <summary>
		/// Charge Group of the charge line applicable to the rate.
		/// Value Reference: CargoWise > Maintain > Account > Global / (Local) Charge Codes > Details > Rating and Quotations > Charge Group
		/// </summary>
		public string ChargeGroup { get; set; }

		/// <summary>
		/// Charge Sub-Group of the charge line applicable to the rate.
		/// Value Reference: CargoWise > Maintain > Account > Global / (Local) Charge Codes > Details > Rating and Quotations > Sub-group
		/// </summary>
		public string ChargeSubGroup { get; set; }

		/// <summary>
		/// Description of the charge line applicable to the rate.
		/// </summary>
		public string ChargeDesc { get; set; }

		/// <summary>
		/// Local Description of the charge line applicable to the rate.
		/// </summary>
		public string ChargeLocalDesc { get; set; }

		/// <summary>
		/// Overridden local description of the charge line applicable to the rate.
		/// </summary>
		public string OverriddenChargeDesc { get; set; }

		/// <summary>
		/// Unit of the charge line applicable to the rate.
		/// Value Reference: CW > Maintain > Reference Files > Package Types > Code.
		/// WeightUnit, VolumeUnit, LengthUnit
		/// </summary>
		public string Unit { get; set; }

		/// <summary>
		/// Conversion Factor of the charge line applicable to the rate.
		/// </summary>
		public ConversionFactor ConversionFactor { get; set; }

		/// <summary>
		/// Currency of the charge line applicable to the rate.
		/// Value Reference: CW > Maintain > Reference Files > Currencies > Code.
		/// </summary>
		public string Currency { get; set; }

		/// <summary>
		/// Details explaining how the charge to be calculated when Autorating of the charge line applicable to the rate.
		/// If both non-Agent and Agent Rates are setup for the same charge, there will be two CalculatorInfo object.
		/// </summary>
		public CalculatorInfo[] Calculators { get; set; }

		/// <summary>
		/// Details of the Cost calculated for the charge.
		/// Applicable to jobcharges endpoint only.
		/// </summary>
		public CalculationItem Cost { get; set; }

		/// <summary>
		/// Details of the Revenue calculated for the charge.
		/// Applicable to jobcharges endpoint only.
		/// </summary>
		public CalculationItem Revenue { get; set; }

		/// <summary>
		/// Internal Note of the charge line applicable to the rate.
		/// Applicable to CW rates only.
		/// </summary>
		public string InternalNote { get; set; }

		/// <summary>
		/// Indication as optional per the Rate Provider.
		/// </summary>
		public bool IsOptional { get; set; }

		/// <summary>
		/// Indicate if the charge description of the relevant charge line is overridden or NOT
		/// </summary>
		public bool IsChargeDescOverride { get; set; }

		/// <summary>
		/// Rounding
		/// </summary>
		public string Rounding { get; set; }

		/// <summary>
		/// Rounding Factor
		/// </summary>
		public decimal RoundingFactor { get; set; }

		/// <summary>
		/// Indication of whether Actual Weight/Volume is used for calculating applicable charge line
		/// </summary>
		public byte ActualPercentage { get; set; }

		/// <summary>
		/// Actual Weight/Volume
		/// </summary>
		public bool ActualWgtVolOnly { get; set; }

		/// <summary>
		/// Public Note of the charge line applicable to the rate
		/// </summary>
		public string PublicNote { get; set; }

		/// <summary>
		/// Ownership
		/// </summary>
		public string ContainerOwnership { get; set; }

		/// <summary>
		/// Start Date
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// Expiry
		/// </summary>
		public string EndDate { get; set; }

		/// <summary>
		/// Unit Factor
		/// </summary>
		public string UnitFactor { get; set; }

		/// <summary>
		/// Condition
		/// </summary>
		public string Condition { get; set; }

		/// <summary>
		/// Condition Expression applicable to the rate
		/// </summary>
		public string ConditionExp { get; set; }

		/// <summary>
		/// Condition Expression Description applicable to the rate
		/// </summary>
		public string ConditionExpDesc { get; internal set; }

		/// <summary>
		/// Is Warehouse Job Level Charge
		/// </summary>
		public bool IsWhsJobLevelCharge { get; set; }

		/// <summary>
		/// Is On Pallets
		/// </summary>
		public bool IsOnPallets { get; set; }

		/// <summary>
		/// Fees/Charges Level
		/// </summary>
		public string FeeChargeLevel { get; set; }

		/// <summary>
		/// Fees and Charges Type
		/// </summary>
		public string FeeChargeType { get; set; }

		/// <summary>
		/// Company Tariff Level
		/// </summary>
		public byte CompanyTariffLevel { get; set; }

		/// <summary>
		/// Unit Multiple
		/// </summary>
		public string UnitMultiple { get; set; }

		/// <summary>
		/// The role of Payer of the relevant Charge Code
		/// </summary>
		public string IncotermPayer { get; set; }
	}
}
