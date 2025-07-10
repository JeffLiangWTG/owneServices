using System.Drawing;
using Enterprise.ComplianceRisk.Business;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class DeniedPartyGridColorHelper
	{
		#region DeniedPartyStatusProviders 

		interface IDeniedPartyStatusProvider
		{
			string Matched { get; }
			string Cleared { get; }
			string Unknown { get; }
			string PermanentClear { get; }
			string NotScreened { get; }
			string JobCleared { get; }
			string NeedsScreening { get; }
			string RequiresReview { get; }
			string Release { get; }
			string Block { get; }
			string JobClearedExternal { get; }
			string JobBlockedExternal { get; }
		}

		class DeniedPartyScreeningStatusProvider : IDeniedPartyStatusProvider
		{
			public string Matched => ScreeningStatusesList.Codes.Matched;
			public string Cleared => ScreeningStatusesList.Codes.Clear;
			public string Unknown => ScreeningStatusesList.Codes.Unknown;
			public string PermanentClear => ScreeningStatusesList.Codes.PermanentClear;
			public string NotScreened => ScreeningStatusesList.Codes.NotScreened;
			public string JobCleared => ScreeningStatusesList.Codes.JobCleared;
			public string NeedsScreening => ScreeningStatusesList.Codes.NeedsScreening;
			public string RequiresReview => ScreeningStatusesList.Codes.RequiresReview;
			public string Release => ScreeningStatusesList.Codes.Release;
			public string Block => DeniedPartyConstants.LogsScreeningStatus.Block;
			public string JobClearedExternal => ScreeningStatusesList.Codes.JobClearedExternal;
			public string JobBlockedExternal => ScreeningStatusesList.Codes.JobBlockedExternal;
		}

		class OrgScreeningStatusProvider : IDeniedPartyStatusProvider
		{
			public string Matched => DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty;
			public string Cleared => DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			public string Unknown => DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound;
			public string RequiresReview => DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound;
			public string PermanentClear => DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear;
			public string NotScreened => DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed;
			public string NeedsScreening => DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed;
			public string JobCleared => DeniedPartyConstants.LogsScreeningStatus.JobCleared;
			public string Release => DeniedPartyConstants.LogsScreeningStatus.Release;
			public string Block => DeniedPartyConstants.LogsScreeningStatus.Block;
			public string JobClearedExternal => ScreeningStatusesList.Codes.JobClearedExternal;
			public string JobBlockedExternal => ScreeningStatusesList.Codes.JobBlockedExternal;
		}

		DeniedPartyScreeningStatusProvider deniedPartyStatusProvider;
		DeniedPartyScreeningStatusProvider DeniedPartyStatusProvider => deniedPartyStatusProvider ?? (deniedPartyStatusProvider = new DeniedPartyScreeningStatusProvider());

		OrgScreeningStatusProvider orgStatusProvider;
		OrgScreeningStatusProvider OrgStatusProvider => orgStatusProvider ?? (orgStatusProvider = new OrgScreeningStatusProvider());

		#endregion

		#region GetColorForResultStatus

		public Color GetColorForResultStatus(string status) => GetColorForResultStatus(status, DeniedPartyStatusProvider);

		public Color GetColorForResultStatus(ScreeningPartyWrapper screeningPartyWrapper) => GetColorForResultStatus(screeningPartyWrapper.ScreeningStatus, DeniedPartyStatusProvider);

		public Color GetColorForResultStatus(ComplianceRiskPartyWrapper screeningPartyWrapper) => GetColorForResultStatus(screeningPartyWrapper.ScreeningStatus, DeniedPartyStatusProvider);

		internal Color GetColorForResultStatus(StmEntityScreeningLog orgScreeningStatus) => GetColorForResultStatus(orgScreeningStatus.PJ_Status, OrgStatusProvider);

		internal Color GetColorForResultStatus(IRelatedOrgPartyScreeningStatus relatedOrgPartyScreeningStatus) => GetColorForResultStatus(relatedOrgPartyScreeningStatus.PJ_Status, OrgStatusProvider);

		static Color GetColorForResultStatus(string status, IDeniedPartyStatusProvider statusProvider)
		{
			var chosenColor = DeniedPartyConstants.GridColor.Undefined;

			if (status.Equals(statusProvider.Matched))
			{
				chosenColor = DeniedPartyConstants.GridColor.Matched;
			}
			else if (status.Equals(statusProvider.Cleared))
			{
				chosenColor = DeniedPartyConstants.GridColor.Clear;
			}
			else if (status.Equals(statusProvider.Unknown))
			{
				chosenColor = DeniedPartyConstants.GridColor.Unknown;
			}
			else if (status.Equals(statusProvider.JobCleared))
			{
				chosenColor = DeniedPartyConstants.GridColor.JobCleared;
			}
			else if (status.Equals(statusProvider.PermanentClear))
			{
				chosenColor = DeniedPartyConstants.GridColor.PermanentClear;
			}
			else if (status.Equals(statusProvider.NotScreened))
			{
				chosenColor = DeniedPartyConstants.GridColor.NotScreened;
			}
			else if (status.Equals(statusProvider.NeedsScreening))
			{
				chosenColor = DeniedPartyConstants.GridColor.NeedsScreening;
			}
			else if (status.Equals(statusProvider.RequiresReview))
			{
				chosenColor = DeniedPartyConstants.GridColor.RequiresReview;
			}
			else if (status.Equals(statusProvider.Release))
			{
				chosenColor = DeniedPartyConstants.GridColor.Release;
			}
			else if (status.Equals(statusProvider.Block))
			{
				chosenColor = DeniedPartyConstants.GridColor.Block;
			}
			else if (status.Equals(statusProvider.JobClearedExternal))
			{
				chosenColor = DeniedPartyConstants.GridColor.JobClearedExternal;
			}
			else if (status.Equals(statusProvider.JobBlockedExternal))
			{
				chosenColor = DeniedPartyConstants.GridColor.JobBlockedExternal;
			}

			return chosenColor;
		}

		#endregion
	}
}
