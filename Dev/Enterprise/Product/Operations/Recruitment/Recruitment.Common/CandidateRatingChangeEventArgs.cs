using System;
using CargoWise.Types;

namespace Enterprise.Recruitment.Common
{
	public class CandidateRatingChangeEventArgs : EventArgs
	{
		public CandidateRatingChangeEventArgs(ZInt rating)
		{
			Rating = rating;
		}

		public ZInt Rating { get; }
	}
}
