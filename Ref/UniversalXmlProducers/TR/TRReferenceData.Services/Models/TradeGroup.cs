using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class TradeGroup : IEquatable<TradeGroup>
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public IEnumerable<TradeGroupCountry> Countries { get; set; }

		public bool Excluded { get; set; }

		public bool Equals(TradeGroup other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Code == other.Code &&
				   Description == other.Description &&
				   StartDate.Equals(other.StartDate) &&
				   EndDate.Equals(other.EndDate) &&
				   !Countries.Except(other.Countries).Any() &&
				   !other.Countries.Except(Countries).Any();
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

			return Equals((TradeGroup)obj);
		}

		public override int GetHashCode()
		{
			int hashCode = HashCode.Combine(Code, Description, StartDate, EndDate);
			if (Countries != null)
			{
				foreach (var country in Countries)
				{
					hashCode = HashCode.Combine(hashCode, country.GetHashCode());
				}
			}
			return hashCode;
		}
	}
}
