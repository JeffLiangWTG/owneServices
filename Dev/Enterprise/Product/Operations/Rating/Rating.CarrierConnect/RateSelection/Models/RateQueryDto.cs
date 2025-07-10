using System;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public sealed class RateQueryDto
	{
		/// <summary>
		/// Transport Mode to use for searching rates.
		/// Currently accepts SEA or AIR.
		/// </summary>
		public string TransportMode { get; set; }

		/// <summary>
		/// Container Mode to use for searching rates.
		///	SEA: FCL, LCL.
		/// AIR: ULD, LSE.
		/// </summary>
		public string ContainerMode { get; set; }

		/// <summary>
		/// Load Port of the Tradelane.
		/// Accepts locations in form of a 5 character UNLOCO.
		/// </summary>
		public string Origin { get; set; }

		/// <summary>
		/// Discharge Port of the Tradelane.
		/// Accepts locations in form of a 5 character UNLOCO.
		/// </summary>
		public string Destination { get; set; }

		/// <summary>
		/// Date to use for matching valid rates.
		/// For Rate Search scenarios, this is used to find rates that are valid on this date.
		/// For Autorating scenarios, this is used to override the Autorating Date of the job.
		/// </summary>
		public DateTime? EffectiveDate { get; set; }

		/// <summary>
		/// Rate Type to use for searching rates.
		/// Only rates of the given RateTypes will be returned.
		/// Typical values are Forwarding, Customs, Shipping, etc.
		/// </summary>
		public string[] RateTypes { get; set; }

		/// <summary>
		/// Purpose of this RateQuery session.
		/// </summary>
		public RateSearchContext Context { get; set; }

		/// <summary>
		/// Job details to use for charge calculation.
		/// </summary>
		public JobInfoDto JobInfo { get; set; }

		public enum RateSearchContext
		{
			RateSearch,
			AutoRating,
			Quoting
		}
	}
}
