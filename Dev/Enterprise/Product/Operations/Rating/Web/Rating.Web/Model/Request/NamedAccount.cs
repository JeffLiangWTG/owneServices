
using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the CW Code or the Named Account applicable to the rates.
	/// It can be used for filtering rates when included in the RateQuery class for sending request to any of the Rates API endpoint.
	/// </summary>
	public class NamedAccount
	{
		/// <summary>
		/// Acceptable types are : 'CW', 'NAC'
		/// 'CW': Control Customer / Consignor / Consignee (Applicable to CW rates only.)
		/// 'NAC': Named Account (Applicable to CargoSphere/CargoGuide rates only.)
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// When type is 'CW': CW > Maintain > Master Data > Organisation > Codes
		/// When type is 'NAC': Named Account Code/Name applicable to the CargoSphere/CargoGuide rates.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Acceptable Types
		/// </summary>
		public static class Types
		{
			/// <summary>
			/// CargoWise
			/// </summary>
			public const string CargoWise = "CW";

			/// <summary>
			/// NamedAccount
			/// </summary>
			public const string NamedAccount = "NAC";

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { CargoWise, NamedAccount };
		}
	}
}
