using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the CW Code and the corresponding Universal Service Level assigned to the Carrier Service Level applicable to the rates.
	/// It can be used for filtering rates when included in the RateQuery class for sending request to any of the Rates API endpoint.
	/// It is also included in the Rate class to specify the Carrier Service Level applicable to the rates responded by the Rates API.
	/// </summary>
	public class CarrierServiceLevel
	{
		/// <summary>
		/// Acceptable types are: 'CW', 'UC'.
		/// 'CW': Carrier Service Level (Applicable to CW rates only).
		/// 'UC': Universal Carrier Service Level (Applicable to CargoSphere/CargoGuide rates only).
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// When Type is 'CW': CW > Maintain > Master Data > Organisation > Carrier > Service Levels > Carrier Service Code.
		/// When Type is 'UC': Universal Carrier Service Level assigned to the Carrier Service Level applicable to the rate.
		/// </summary>
		public string Value { get; set; }

		[JsonIgnore]
		internal string CWCode
		{
			get
			{
				if (Types.CargoWise.Equals(Type, StringComparison.InvariantCultureIgnoreCase))
				{
					return Value;
				}

				return null;
			}
		}

		[JsonIgnore]
		internal string UniversalCode
		{
			get
			{
				if (Types.Universal.Equals(Type, StringComparison.InvariantCultureIgnoreCase))
				{
					return Value;
				}

				return null;
			}
		}

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
			/// Universal
			/// </summary>
			public const string Universal = "UC";

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { CargoWise, Universal };
		}
	}
}
