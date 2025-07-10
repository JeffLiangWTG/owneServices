//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmEntityScreeningLogLookups
//
//    This class should be used for overriding collections in AutoStmEntityScreeningLogLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class StmEntityScreeningLogLookups : AutoStmEntityScreeningLogLookups
	{
		public StmEntityScreeningLogLookups(AutoStmEntityScreeningLog parent) : base(parent)
		{
		}

		#region Statuses

		public CodeDescriptionPairList Statuses
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed, Res.GetString("3b57c73e-c244-4fc8-9584-5643f651d776", "No Screening Performed"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.ScreenedClear, Res.GetString("92cb6289-b4d0-4fe7-a0c3-08ecc020500b", "Screening Status Clear"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.UpdateRelatedJobs, Res.GetString("279041b6-78c4-48da-a1e9-6028c30eef4a", "Update of Related Jobs"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound, Res.GetString("7fe6273b-7aeb-4178-af4b-49c71128dc40", "Requires Review - Potential Matches Found"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, Res.GetString("7fb9cfe7-db24-4001-a95a-ca0a3577bb88", "Matched To Denied Party"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, Res.GetString("7bd8dd5d-997b-4fed-954a-397f9ab07626", "Needs Re-screening - Invalidated by Local Data Changes"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate, Res.GetString("5f008235-413b-4681-ac19-f82aca8b1c3b", "Needs Re-screening - Invalidated by Content Update"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled, Res.GetString("675DCE60-568D-4d3b-B3A0-F2695C3B95D7", "Screening Result Not Accepted"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear, Res.GetString("775DCE60-568D-4d3b-B3A0-A2695C3B95D6", "Screening Status Permanently Clear"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.JobCleared, Res.GetString("6DCC96DC-073F-470B-B28E-EDF1BB971E15", "Mark As Job Clear"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.AddPartyScreen, Res.GetString("9DA6A5B3-8115-4ECB-9CF1-EF6C483F4ED6", "Party Added"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.RemovePartyScreen, Res.GetString("6AB23B1E-D81B-467E-B0E0-EBE1DBAEC2F1", "Party Removed"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.UserDecisionsMarkAsSanctioned, Res.GetString("0F406D85-403A-434A-A34E-B6B631164621", "Marked as Sanctioned"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions, Res.GetString("7ADFBCC0-EF34-4AD8-AF03-8C8FF85D9FA6", "Sanctions Removed"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.ScreenedMarkAsSanctioned, Res.GetString("56118482-D2CF-4517-B8DC-FDA757BE6D4B", "Screened and Marked as Sanctioned"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.ScreeningProcessError, Res.GetString("310FAC84-34DC-45F0-A278-E3B97DAE8AB9", "Screening Process Error"));
				result.AddPair(DeniedPartyConstants.LogsScreeningStatus.ComplianceRiskSnapshot, Res.GetString("E757F66B-C4C5-4D14-AE03-84C6FF54FFC7", "Compliance Risk Snapshot"));

				return result;
			}
		}

		#endregion
	}
}
