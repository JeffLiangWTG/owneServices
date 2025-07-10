using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Registry.Business;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class NameMatch : NonPersistentBusinessObject
	{
		public NameMatch(NameMatchInfo nameMatchInfo, ProfileNameInfo profileNameInfo) : base()
		{
			Argument.NotNull(profileNameInfo, nameof(profileNameInfo));
			ProfileNameInfo = profileNameInfo;
			NameMatchInfo = nameMatchInfo;
		}

		public NameMatchInfo NameMatchInfo { get; }

		public ProfileNameInfo ProfileNameInfo { get; }

		public ZString Name => ProfileNameInfo.FullName;

		public ZInt Score => NameMatchInfo?.MatchingNameScore ?? 0;

		public ZString Type => Res.GetString("7dbc3ff7-1b40-4ee6-852c-94038830c134", "Name");

		public RiskLevel Level
		{
			get
			{
				if (NameMatchInfo == null)
				{
					return RiskLevel.Low;
				}

				var confidenceThreshold = OrganisationsDataRegistry.Instance.GetDPSMatchingConfidenceThreshold(NameMatchInfo.RequestName.NameType);

				if (Score >= confidenceThreshold.HighThreshold)
				{
					return RiskLevel.High;
				}
				else if (NameMatchInfo.MatchingNameScore >= confidenceThreshold.MediumThreshold)
				{
					return RiskLevel.Medium;
				}
				else
				{
					return RiskLevel.Low;
				}
			}
		}
	}
}
