using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the CW Code and corresponding Universal Commodity Group assigned to the Commodity applicable to the rates.
	/// It is included in the Rate class to specify the Commodity applicable to the rates responded by the Rates API.
	/// </summary>
	public class CommodityInfo
	{
		/// <summary>
		/// Acceptable types are: 'CW', 'UCG'.
		/// 'CW': CargoWise Commodity Type.
		/// 'UCG': Universal Commodity Group (Applicable to CargoGuide rates only).
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// When Type is 'CW': CW > Maintain > Reference Files > Commodities > Code.
		/// When Type is 'UCG': CW > Maintain > Reference Files > Commodities > Universal Group.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Description when type is CW : CW > Maintain > Reference Files > Commodities > Commodity Description
		/// </summary>
		public string Description { get; set; }

		[JsonIgnore]
		internal string CWCode => Types.CargoWise.Equals(Type, StringComparison.InvariantCultureIgnoreCase) ? Value : null;

		[JsonIgnore]
		internal string UniversalCommodityGroup => Types.UniversalCommodityGroup.Equals(Type, StringComparison.InvariantCultureIgnoreCase) ? Value : null;

		/// <summary>
		/// Types
		/// </summary>
		public static class Types
		{
			/// <summary>
			/// CargoWise
			/// </summary>
			public const string CargoWise = "CW";

			/// <summary>
			/// UniversalCommodityGroup
			/// </summary>
			public const string UniversalCommodityGroup = "UCG";

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { CargoWise, UniversalCommodityGroup };
		}
	}
}
