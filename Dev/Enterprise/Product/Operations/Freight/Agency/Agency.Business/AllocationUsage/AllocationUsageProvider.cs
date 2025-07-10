using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AllocationUsageProvider : IAllocationUsageProvider
	{
		public void Load(JobVoyage voyage, ZGuid principalPK)
		{
			if (voyage == null)
			{
				throw new ArgumentNullException(nameof(voyage));
			}

			usageList.Clear();
			foreach (AllocationUsage usage in AllocationUsage.LoadForSailing(voyage, principalPK))
			{
				usageList.Add(usage.ParentPK, usage);
			}
		}

		public AllocationUsage GetSailingUsage(JobSailing sailing)
		{
			if (sailing == null)
			{
				throw new ArgumentNullException(nameof(sailing));
			}

			AllocationUsage usage;

			if (!usageList.TryGetValue(sailing.PK, out usage))
			{
				usage = new AllocationUsage();
			}

			return usage;
		}

		public AllocationUsage GetOriginUsage(VoyageOrigin origin)
		{
			if (origin == null)
			{
				throw new ArgumentNullException(nameof(origin));
			}

			AllocationUsage usage;

			if (!usageList.TryGetValue(origin.PK, out usage))
			{
				usage = CalculateOriginUsage(origin);
				usageList.Add(origin.PK, usage);
			}

			return usage;
		}

		public AllocationUsage GetCountryUsage(VoyageCountry country)
		{
			if (country == null)
			{
				throw new ArgumentNullException(nameof(country));
			}

			AllocationUsage usage;

			if (!usageList.TryGetValue(country.PK, out usage))
			{
				usage = CalculateCountryUsage(country);
				usageList.Add(country.PK, usage);
			}

			return usage;
		}

		#region Implementation

		AllocationUsage CalculateOriginUsage(VoyageOrigin origin)
		{
			if (origin.Voyage != null)
			{
				List<AllocationUsage> usages = new List<AllocationUsage>(origin.Voyage.Sailings.Count);

				foreach (JobSailing sailing in origin.Voyage.Sailings)
				{
					if (sailing.JX_JA == origin.PK)
					{
						usages.Add(GetSailingUsage(sailing));
					}
				}

				return AllocationUsage.Sum(usages);
			}
			else
			{
				return new AllocationUsage();
			}
		}

		AllocationUsage CalculateOverlappedOriginUsage(VoyageOrigin origin)
		{
			if (origin.Voyage != null)
			{
				List<AllocationUsage> usages = new List<AllocationUsage>(origin.Voyage.Sailings.Count);

				foreach (JobSailing sailing in origin.Voyage.Sailings)
				{
					if (sailing.Origin != null && origin.VoyageCountry == sailing.Origin.VoyageCountry && sailing.Overlaps(origin))
					{
						usages.Add(GetSailingUsage(sailing));
					}
				}

				return AllocationUsage.Sum(usages);
			}
			else
			{
				return new AllocationUsage();
			}
		}

		AllocationUsage CalculateCountryUsage(VoyageCountry country)
		{
			if (country.Voyage != null)
			{
				List<AllocationUsage> usages = new List<AllocationUsage>(country.Voyage.Origins.Count);

				foreach (VoyageOrigin origin in country.Origins)
				{
					usages.Add(CalculateOverlappedOriginUsage(origin));
				}

				return AllocationUsage.Max(usages);
			}
			else
			{
				return new AllocationUsage();
			}
		}

		readonly Dictionary<ZGuid, AllocationUsage> usageList = new Dictionary<ZGuid, AllocationUsage>();

		#endregion
	}
}
