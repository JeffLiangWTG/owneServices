using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class provides the Location applicable to the rates. It can be used for filtering rates when included in the RateQuery class for sending request to any of the Rates API endpoint.
	/// </summary>
	public class Location
	{
		/// <summary>
		/// Acceptable types are: 'COUNTRY', 'IATACITY', 'UNLOCO', 'CITY', 'ZONE', 'POSTCODE'
		/// 'COUNTRY', 'CITY', 'ZONE' or 'POSTCODE' are applicable to CW rates only.
		/// 'CITY' is applicable to jobcharges endpoint only.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// COUNTRY - ISO 3166 Country (2 char)
		/// UNLOCO - UN/LOCODE (5 char)
		/// CITY - CW > Locations > Cities > International Names
		/// ZONE - CW > Locations > International Zones > Zone Code
		/// IATACITY - IATA Codes for Cities
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Acceptable types are: '1LD', 'LDC', 'MLD', 'MDC'
		/// </summary>
		public string RelatedField { get; set; }

		[JsonIgnore]
		internal bool IsPostcode
		{
			get
			{
				return Types.Postcode.Equals(Type, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		[JsonIgnore]
		internal bool IsCW1Location
		{
			get
			{
				return this.IsEmptyLocation()
					|| Types.Country.Equals(Type, StringComparison.InvariantCultureIgnoreCase)
					|| Types.UNLOCO.Equals(Type, StringComparison.InvariantCultureIgnoreCase)
					|| Types.Zone.Equals(Type, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		/// <summary>
		/// Acceptable Location Types
		/// </summary>
		public static class Types
		{
			/// <summary>
			/// COUNTRY
			/// </summary>
			public const string Country = "COUNTRY";

			/// <summary>
			/// IATACITY
			/// </summary>
			public const string IATACity = "IATACITY";

			/// <summary>
			/// UNLOCO
			/// </summary>
			public const string UNLOCO = "UNLOCO";

			/// <summary>
			/// POSTCODE
			/// </summary>
			public const string Postcode = "POSTCODE";

			/// <summary>
			/// CITY
			/// </summary>
			public const string City = "CITY";

			/// <summary>
			/// ZONE
			/// </summary>
			public const string Zone = "ZONE";

			/// <summary>
			/// All Acceptable Types
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { Country, IATACity, City, Postcode, Zone, UNLOCO };
		}
	}
}
