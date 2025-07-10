using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class TradeGroupCountry : IEquatable<TradeGroupCountry>
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public bool Equals(TradeGroupCountry other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Code == other.Code && Description == other.Description && StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((TradeGroupCountry)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Code, Description, StartDate, EndDate);
		}
	}
}
