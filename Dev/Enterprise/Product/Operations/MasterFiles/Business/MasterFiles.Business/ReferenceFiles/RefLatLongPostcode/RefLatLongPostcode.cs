using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefLatLongPostcode : AutoRefLatLongPostcode
	{
		public RefLatLongPostcode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Load Postcode

		/// <summary>
		/// Loads details about a specific postcode.
		/// </summary>
		/// <param name="factory">The Factory to load this object in</param>
		/// <param name="postcode">The postcode to search for</param>
		/// <param name="citySuburb">The city / suburb to find this postcode in</param>
		/// <param name="countryCode">The country in which you are searching for this postcode - cannot be null</param>
		/// <returns>Object containining information about the specified postcode</returns>
		public static RefLatLongPostcode Load(BusinessObjectFactory factory, ZString postcode, ZString citySuburb, ZString countryCode)
		{
			if ((string)countryCode == null || countryCode.IsEmpty)
			{
				throw new ArgumentNullException(nameof(countryCode), "A country must be specified when attempting to load a specific Postcode");
			}
			if ((string)postcode == null || postcode.IsEmpty)
			{
				throw new ArgumentNullException(nameof(postcode), "A postcode must be specified");
			}

			var filter = new ZQuery(RefLatLongPostcodeSchema.RJ_Postcode, postcode);
			filter.AddToFilter(RefLatLongPostcodeSchema.RJ_RN_NKCountry, countryCode);
			var filterOutLocation00 = new ZQuery(RefLatLongPostcodeSchema.RJ_Lattitude, SQLComparisonOperator.NotEqual, 0);
			filterOutLocation00.AddToFilter(JoinCondition.Or, RefLatLongPostcodeSchema.RJ_Longitude, SQLComparisonOperator.NotEqual, 0);
			filter.AddToFilter(filterOutLocation00);

			if (!citySuburb.IsEmpty)
			{
				filter.AddToFilter(RefLatLongPostcodeSchema.RJ_CitySuburb, SQLComparisonOperator.Contains, citySuburb);
			}

			return (RefLatLongPostcode)factory.LoadTop1(typeof(RefLatLongPostcode), filter);
		}

		public static RefLatLongPostcode Load(BusinessObjectFactory factory, ZString postcode, ZString citySuburb, RefCountry country)
		{
			if (country == null)
			{
				throw new ArgumentNullException(nameof(country), "A country must be specified when attempting to load a specific Postcode");
			}

			return Load(factory, postcode, citySuburb, country.RN_Code);
		}

		public static RefLatLongPostcode Load(BusinessObjectFactory factory, ZString postcode, RefCountry country)
		{
			return Load(factory, postcode, ZString.Empty, country);
		}

		public static RefLatLongPostcode Load(BusinessObjectFactory factory, ZString postcode, ZString countryCode)
		{
			return Load(factory, postcode, ZString.Empty, countryCode);
		}

		#endregion

		#region Distance Calculation

		/// <summary>
		/// Calculates the distance in KM between 2 longitude / lattitude coordinates (in degrees).
		/// 
		/// If the longitudes and lattitudes are the same for both postcodes, a distance of 0 is returned.
		/// 
		/// Calculation performed is (based on Longitudes and Lattitudes being in Radians).
		/// 
		///		InitialCalculationResult = [ SIN(Lat1) x SIN(Lat2) ] + [ COS(Lat1) x COS(Lat2) x COS(Long1 - Long2) ]
		///		
		///		if (InitialCalculationResult > 1)		Distance = EARTH_RADIUS_KM * ARCOS(1)
		///		else									Distance = EARTH_RADIUS_KM * ARCOS(InitialCalculationResult)
		/// 
		/// </summary>
		/// <param name="postcode1">The 'from' postcode object</param>
		/// <param name="postcode2">The 'to' postcode object</param>
		/// <returns>Distance between two postcodes, in KM, rounded to 3 decimal places</returns>
		public static double CalculateDistance(RefLatLongPostcode postcode1, RefLatLongPostcode postcode2)
		{
			double result = 0;

			if (!postcode1.IsIdenticalLocation(postcode2))
			{
				double calcResult = (Math.Sin(postcode1.LattitudeRadians) * Math.Sin(postcode2.LattitudeRadians)) +
									(Math.Cos(postcode1.LattitudeRadians) * Math.Cos(postcode2.LattitudeRadians) *
									 Math.Cos(postcode1.LongitudeRadians - postcode2.LongitudeRadians));

				result = EARTH_RADIUS_KM * Math.Acos(calcResult > 1 ? 1 : calcResult);
			}

			return (double)ZArchitecture.Core.Utilities.Round(new decimal(result), 3);
		}

		public static double CalculateDistance(double fromLatDegrees, double fromLongDegrees, double toLatDegrees, double toLongDegrees)
		{
			double result = 0;

			if (!IsIdenticalLocation(fromLatDegrees, fromLongDegrees, toLatDegrees, toLongDegrees))
			{
				double calcResult = (Math.Sin(ConvertDegreesToRadians(fromLatDegrees)) * Math.Sin(ConvertDegreesToRadians(toLatDegrees))) +
									(Math.Cos(ConvertDegreesToRadians(fromLatDegrees)) * Math.Cos(ConvertDegreesToRadians(toLatDegrees)) *
									 Math.Cos(ConvertDegreesToRadians(fromLongDegrees) - ConvertDegreesToRadians(toLongDegrees)));

				result = EARTH_RADIUS_KM * Math.Acos(calcResult > 1 ? 1 : calcResult);
			}

			return (double)ZArchitecture.Core.Utilities.Round(new decimal(result), 3);
		}

		static bool IsIdenticalLocation(double fromLatDegrees, double fromLongDegrees, double toLatDegrees, double toLongDegrees)
		{
			return (fromLatDegrees == toLatDegrees && fromLongDegrees == toLongDegrees);
		}
		/// <summary>
		/// Calculates the distance in KM between 2 longitude / lattitude coordinates (in degrees).
		/// 
		/// If the longitudes and lattitudes are the same for both postcodes, a distance of 0 is returned.
		/// 
		/// Calculation performed is (based on Longitudes and Lattitudes being in Radians).
		/// 
		///		InitialCalculationResult = [ SIN(Lat1) x SIN(Lat2) ] + [ COS(Lat1) x COS(Lat2) x COS(Long1 - Long2) ]
		///		
		///		if (InitialCalculationResult > 1)		Distance = EARTH_RADIUS_KM * ARCOS(1)
		///		else									Distance = EARTH_RADIUS_KM * ARCOS(InitialCalculationResult)
		/// 
		/// </summary>
		/// <param name="address1">The 'from' OrgAddress object</param>
		/// <param name="address2">The 'to' OrgAddress object</param>
		/// <returns>Distance between two OrgAddresses, in KM, rounded to 3 decimal places</returns>
		public static double CalculateDistance(OrgAddress address1, OrgAddress address2)
		{
			double result = 0;

			if (address1 != null && !address1.OA_PostCode.IsEmpty && address2 != null && !address2.OA_PostCode.IsEmpty)
			{
				RefLatLongPostcode address1PostCode = Load(address1.Factory, address1.OA_PostCode, GetCountryCodeFromAddress(address1));
				RefLatLongPostcode address2PostCode = Load(address1.Factory, address2.OA_PostCode, GetCountryCodeFromAddress(address2));

				if (address1PostCode != null && address2PostCode != null)
				{
					result = CalculateDistance(address1PostCode, address2PostCode);
				}
			}

			return result;
		}

		public static double CalculateDistance(JobDocAddress docAddress1, JobDocAddress docAddress2)
		{
			double result = 0;

			if (docAddress1 != null && !docAddress1.E2_Postcode.IsEmpty && docAddress2 != null && !docAddress2.E2_Postcode.IsEmpty && !docAddress1.E2_RN_NKCountryCode.IsEmpty && !docAddress2.E2_RN_NKCountryCode.IsEmpty)
			{
				RefLatLongPostcode address1PostCode = Load(docAddress1.Factory, docAddress1.E2_Postcode, docAddress1.E2_RN_NKCountryCode);
				RefLatLongPostcode address2PostCode = Load(docAddress1.Factory, docAddress2.E2_Postcode, docAddress2.E2_RN_NKCountryCode);

				if (address1PostCode != null && address2PostCode != null)
				{
					result = CalculateDistance(address1PostCode, address2PostCode);
				}
			}

			return result;
		}

		static ZString GetCountryCodeFromAddress(OrgAddress address)
		{
			ZString result = address.OA_RL_NKRelatedPortCode.SubstringSafe(0, RefCountrySchema.RN_Code.MaxLength);
			if (result.IsEmpty)
			{
				result = address.Header.OH_RL_NKClosestPort.SubstringSafe(0, RefCountrySchema.RN_Code.MaxLength);
			}

			return result;
		}

		/// <summary>
		/// Earth's Radius in Kilometers
		/// </summary>
		public const double EARTH_RADIUS_KM = 6340.96;

		#endregion

		#region Degrees to Radians Conversion

		public static double ConvertDegreesToRadians(double degrees)
		{
			return degrees * Math.PI / 180;
		}

		public static double ConvertDegreesToRadians(decimal degrees)
		{
			return ConvertDegreesToRadians(Convert.ToDouble(degrees));
		}

		#endregion

		#region Identical Location

		public bool IsIdenticalLocation(RefLatLongPostcode postcode)
		{
			return (RJ_Lattitude == postcode.RJ_Lattitude) && (RJ_Longitude == postcode.RJ_Longitude);
		}

		#endregion

		#region Properties

		#region Lattitude (Radians)

		public double LattitudeRadians
		{
			get { return ConvertDegreesToRadians(RJ_Lattitude); }
		}

		#endregion

		#region Longitude (Radians)

		public double LongitudeRadians
		{
			get { return ConvertDegreesToRadians(RJ_Longitude); }
		}

		#endregion

		#endregion
	}
}
