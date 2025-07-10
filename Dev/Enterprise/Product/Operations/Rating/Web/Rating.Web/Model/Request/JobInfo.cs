using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class is mandatory for successfully requesting calculated job charges or rates pricing information under certain scenarios from the endpoint.
	/// It defines job parameters and measures required for charges calculation.
	/// </summary>
	public class JobInfo
	{
		/// <summary>
		/// To be used as Value of Goods for calculating rates setup using DIN, IXC, PEB or PER Calculator with Type of 'VAL – Value of Goods' defined.
		/// </summary>
		public decimal? GoodsValue { get; set; }

		/// <summary>
		/// To be used as Currency of Value of Goods for calculating rates setup using DIN, IXC, PEB or PER Calculator with Type of 'VAL – Value of Goods' defined.
		/// </summary>
		public string GoodsValueCurrency { get; set; }

		/// <summary>
		/// To be used as Insurance Value for calculating rates setup using DIN, IXC, PEB or PER Calculator with Type of ‘INS – Insurance Value’ defined.
		/// </summary>
		public decimal? InsuranceValue { get; set; }

		/// <summary>
		/// To be used as Currency of Insurance Value for calculating rates setup using DIN, IXC, PEB or PER Calculator with Type of ‘INS – Insurance Value’ defined.
		/// </summary>
		public string InsuranceValueCurrency { get; set; }

		/// <summary>
		/// To be used as Customs Value for calculating rates setup using DIN, IXC, PEB or PER Calculator with Type of ‘CUV – Customs Value’ defined.
		/// </summary>
		public decimal? CustomsValue { get; set; }

		/// <summary>
		/// To be used for Autorating charges matching with the corresponding services.
		/// </summary>
		public JobService[] JobServices { get; set; }

		/// <summary>
		/// To be used to override chargeable calculated by system when Autorating charges with CHG – Use Chargeable from Job as Rounding.
		/// </summary>
		public decimal ChargeableOverride { get; set; }

		/// <summary>
		/// List of Containers with information, corresponding Packing Lines and measurements for calculating job charges.
		/// </summary>
		public JobContainer[] Containers { get; set; }

		/// <summary>
		/// Match against the Drop Mode used in CTZ or CTG calculators under Origin Charges for calculating rates.
		/// Value Reference: 'ANY' (Any), 'ASK' (Ask Client), 'HSL' (Haulier Supplies Lift), 'HUL' (Hand Unload/Load by Premise), 'HWL' (Hand Unload/Load by Haulier), 'LOF' (Drop Container - Premise supplies Lift), 'PSL' (Premise Supplies Lift), 'SDL' (Drop Container with Sideloader), 'TRL' (Drop Trailer), 'WUP' (Wait for Pack/Unpack)
		/// </summary>
		public string PickupDropMode { get; set; }

		/// <summary>
		/// Match against the Drop Mode used in CTZ or CTG calculators under Destination Charges for calculating rates.
		/// Value Reference: 'ANY' (Any), 'ASK' (Ask Client), 'HSL' (Haulier Supplies Lift), 'HUL' (Hand Unload/Load by Premise), 'HWL' (Hand Unload/Load by Haulier), 'LOF' (Drop Container - Premise supplies Lift), 'PSL' (Premise Supplies Lift), 'SDL' (Drop Container with Sideloader), 'TRL' (Drop Trailer), 'WUP' (Wait for Pack/Unpack)
		/// </summary>
		public string DeliveryDropMode { get; set; }

		/// <summary>
		/// To be used for Autorating charges matching with the corresponding custom field name/value pairs
		/// </summary>
		public CustomField[] CustomFields { get; set; }

		/// <summary>
		/// It can be used to override the Company Tariff Level which is used for Auto-Rating Revenue.
		/// </summary>
		public byte? CTLevelOverride { get; set; }

		internal static class DropModes
		{
			public const string Any = "ANY";
			public const string AskClient = "ASK";
			public const string HaulierSuppliesLift = "HSL";
			public const string HandUnloadLoadByPremise = "HUL";
			public const string HandUnloadLoadByHaulier = "HWL";
			public const string DropContainerPremiseSuppliesLift = "LOF";
			public const string PremiseSuppliesLift = "PSL";
			public const string DropContainerWithSideloader = "SDL";
			public const string DropTrailer = "TRL";
			public const string WaitForPackUnpack = "WUP";

			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[]
				{
					Any,
					AskClient,
					HaulierSuppliesLift,
					HandUnloadLoadByPremise,
					HandUnloadLoadByHaulier,
					DropContainerPremiseSuppliesLift,
					PremiseSuppliesLift,
					DropContainerWithSideloader,
					DropTrailer,
					WaitForPackUnpack
				};
		}
	}
}
