using System.Collections.Generic;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public class JobInfoDto
	{
		/// <summary>
		/// List of Containers with information, corresponding Packing Lines and measurements for calculating job charges.
		/// </summary>
		public IEnumerable<JobContainerDto> Containers { get; set; }
	}
}
