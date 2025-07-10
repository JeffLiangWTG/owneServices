using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the CW Code and corresponding ISO Type assigned to the Container Type applicable to the rates.
	/// It can be used for filtering rates when included in the RateQuery class for sending request to any of the Rates API endpoint.
	/// It is also included in the Rate class to specify the Container Type applicable to the rates responded by the Rates API.
	/// </summary>
	public class ContainerType
	{
		/// <summary>
		/// Acceptable types are : 'CW', 'ISO'.
		/// 'CW': Container/ULD Type (can be used for searching both CW and CargoGuide rates).
		/// 'ISO': ISO Type of the Container (Applicable to CargoSphere rates only).
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// When Type is 'CW': CW > Maintain > Reference Files > Containers > Code.
		/// When Type is 'ISO': CW > Maintain > Reference Files > Containers > ISO Type.
		/// </summary>
		public string Value { get; set; }

		[JsonIgnore]
		internal string CWCode => Types.CargoWise.Equals(Type, StringComparison.InvariantCultureIgnoreCase) ? Value : null;

		[JsonIgnore]
		internal string ISOType => Types.ISO.Equals(Type, StringComparison.InvariantCultureIgnoreCase) ? Value : null;

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
			/// ISO
			/// </summary>
			public const string ISO = "ISO";

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { CargoWise, ISO };
		}
	}
}
