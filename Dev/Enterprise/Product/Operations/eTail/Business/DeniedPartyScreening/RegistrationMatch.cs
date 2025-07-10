using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class RegistrationMatch : NonPersistentBusinessObject
	{
		public RegistrationMatch(RegistrationCodeMatchInfo registrationCodeMatchInfo, ProfileRegistrationCodeInfo profileRegistrationCodeInfo) : base()
		{
			Argument.NotNull(profileRegistrationCodeInfo, nameof(profileRegistrationCodeInfo));

			ProfileRegistrationCodeInfo = profileRegistrationCodeInfo;
			RegistrationCodeMatchInfo = registrationCodeMatchInfo;
		}

		public RegistrationCodeMatchInfo RegistrationCodeMatchInfo { get; }

		public ProfileRegistrationCodeInfo ProfileRegistrationCodeInfo { get; }

		public ZString Name => BuildParty(ProfileRegistrationCodeInfo.IdType, ProfileRegistrationCodeInfo.IdNumber);

		public ZInt Score => RegistrationCodeMatchInfo?.MatchingRegistrationCodeScore ?? 0;

		public ZString Type => Res.GetString("95b817bf-198e-4f24-a3d4-684ad256b11e", "Registration");

		public RiskLevel Level => RegistrationCodeMatchInfo == null || RegistrationCodeMatchInfo.MatchingRegistrationCodeScore != 100 ? RiskLevel.Low : RiskLevel.High;

		string BuildParty(string type, string value)
		{
			string result = null;
			if (!string.IsNullOrWhiteSpace(type))
			{
				result += type + " : ";
			}

			return result + value;
		}
	}
}
