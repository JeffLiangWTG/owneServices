using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Common
{
	public static class DeduplicationUtils
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly object syncLock = new object();

		public static IDeduplicationDebuggerHub DebuggerHubInstance
		{
			get
			{
				if (hubInstance == null)
				{
					lock (syncLock)
					{
						if (hubInstance == null)
						{
							hubInstance = new DeduplicationDebuggerHub();
						}
					}
				}

				return hubInstance;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IDeduplicationDebuggerHub hubInstance;

		public static double GetUXMLOrgMatchExcludeScoreThreshold()
		{
			var percentage = OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.Value;
			return (double)percentage / 100;
		}

		public static ConfidenceRating GetExcludeConfidenceRatingResult()
		{
			var registryItem = new DeDuplicationMinimumConfidenceRating().GetDescriptionFromCode(OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value);

			return Enum.TryParse(registryItem, out ConfidenceRating rating)
				? rating
				: ConfidenceRating.None;
		}

		public static ConfidenceRating GetExcludeConfidenceRatingResult(string registryValue)
		{
			var registryItem = new DeDuplicationMinimumConfidenceRating().GetDescriptionFromCode(registryValue);

			return Enum.TryParse(registryItem, out ConfidenceRating rating)
				? rating
				: ConfidenceRating.None;
		}
	}
}
