using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the attributes and corresponding Packing Lines of each Container to be used as filters for searching of rates or measurements for calculating charges.
	/// </summary>
	public class JobContainer
	{
		/// <summary>
		/// Match against the Container Type for searching of rates.
		/// Mandatory for jobcharges endpoint with ‘FCL’ ContainerMode.
		/// Value Reference: CargoWise > Maintain > Reference Files > Containers > Code.
		/// </summary>
		public string ContainerTypeCWCode { get; set; }

		/// <summary>
		/// Container Number.
		/// </summary>
		public string Number { get; set; }

		/// <summary>
		/// Number of Containers.
		/// Mandatory for jobcharges endpoint with ‘FCL’ ContainerMode.
		/// </summary>
		public int? Unit { get; set; }

		/// <summary>
		/// Match against the Commodity for searching of rates.
		/// Value Reference: CargoWise > Maintain > Reference Files > Commodities > Code.
		/// </summary>
		public string Commodity { get; set; }

		/// <summary>
		/// Ownership of the Container
		/// Value Reference: 'CAR' (Carrier), 'SHP' (Shipper).
		/// </summary>
		public string Ownership { get; set; }

		/// <summary>
		/// The Packing Lines with corresponding attributes and measurements packed into the current Container.
		/// </summary>
		public JobPackLine[] PackLines { get; set; }

		/// <summary>
		/// Ownership of the Container
		/// </summary>
		public static class Ownerships
		{
			/// <summary>
			/// Carrier
			/// </summary>
			public const string CAR = "CAR";

			/// <summary>
			/// Shipper
			/// </summary>
			public const string SHP = "SHP";

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { CAR, SHP };
		}
	}
}
