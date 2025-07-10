namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	/// <summary>
	/// Describes the attributes and corresponding Packing Lines of each Container to be used as filters for
	/// searching of rates and measurements for calculating charges.
	/// </summary>
	public class JobContainerDto
	{
		/// <summary>
		/// Match against the Container Type for searching of rates.
		/// </summary>
		public string ContainerType { get; set; }

		public string Number { get; set; }

		/// <summary>
		/// Number of Containers.
		/// </summary>
		public int? Count { get; set; }

		/// <summary>
		/// Match against the Commodity for searching of rates.
		/// </summary>
		public string Commodity { get; set; }

		/// <summary>
		/// The Packing Lines with corresponding attributes and measurements packed into the current Container.
		/// </summary>
		public JobPackLineDto[] PackLines { get; set; }

		/// <summary>
		/// Match against the Container Quality for searching of rates.
		/// </summary>
		public string ContainerQuality { get; set; }
	}
}
