using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This object describes the attributes of each Job Service used for matching and calculating charges from searched rates.
	/// </summary>
	public class JobService
	{
		/// <summary>
		/// Type of the Service.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Contractor/Service Provider who provides the Service.
		/// </summary>
		public Organisation Contractor { get; set; }

		/// <summary>
		/// Location of the Service conducted.
		/// </summary>
		public Location Location { get; set; }

		/// <summary>
		/// Date that the Service is Booked.
		/// </summary>
		public DateTimeOffset? Booked { get; set; }

		/// <summary>
		/// Date that the Service is Completed.
		/// </summary>
		public DateTimeOffset? Completed { get; set; }

		/// <summary>
		/// Service Count for Rates calculation.
		/// </summary>
		public decimal Count { get; set; }

		/// <summary>
		/// Service Duration for Rates calculation.
		/// </summary>
		public int Duration { get; set; }

		/// <summary>
		/// Service Rate for Rates calculation.
		/// </summary>
		public decimal Rate { get; set; }

		/// <summary>
		/// Rate Currency for Service Rate.
		/// </summary>
		public string RateCurrency { get; set; }

		/// <summary>
		/// Measurement Basis for Rates calculation.
		/// Value Reference: 'HR' (Hour), 'DY' (Day), 'SV' (Service Occurance), 'FR' (Flat Rate), 'CN' (Container), 'PD' (Pickup Distance), 'DD' (Delivery Distance) and 'CH' (Chargeable). 
		/// </summary>
		public string MeasurementBasis { get; set; }

		/// <summary>
		/// MeasurementBases
		/// </summary>
		public static class MeasurementBases
		{
			/// <summary>
			/// Hour
			/// </summary>
			public const string Hour = JobServiceInfo.Constants.Codes.Hour;

			/// <summary>
			/// Day
			/// </summary>
			public const string Day = JobServiceInfo.Constants.Codes.Day;

			/// <summary>
			/// Service Occurance
			/// </summary>
			public const string ServiceOccurrence = JobServiceInfo.Constants.Codes.ServiceOccurrence;

			/// <summary>
			/// Flat Rate
			/// </summary>
			public const string FlatRate = JobServiceInfo.Constants.Codes.FlatRate;

			/// <summary>
			/// Container
			/// </summary>
			public const string Container = JobServiceInfo.Constants.Codes.Container;

			/// <summary>
			/// Pickup Distance
			/// </summary>
			public const string PickUpDistance = JobServiceInfo.Constants.Codes.PickUpDistance;

			/// <summary>
			/// Delivery Distance
			/// </summary>
			public const string DeliveryDistance = JobServiceInfo.Constants.Codes.DeliveryDistance;

			/// <summary>
			/// Chargeable
			/// </summary>
			public const string Chargeable = JobServiceInfo.Constants.Codes.Chargeable;

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { Hour, Day, ServiceOccurrence, FlatRate, Container, PickUpDistance, DeliveryDistance, Chargeable };
		}
	}
}
