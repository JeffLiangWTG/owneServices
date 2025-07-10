using System;
using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Business
{
	[CodeAlive("For Merging - Consistent Experience")]
	public class MergeOrganizationUtils
	{
		const string DebuggerParticipantName = "MergeOrganization";

		public MergeOrganizationUtils()
		{
			DebuggerParticipant = new DeduplicationDebuggerParticipant(DebuggerParticipantName);
			DeduplicationUtils.DebuggerHubInstance.Register(DebuggerParticipant);
		}

		public MergeOrganizationViewSource GetViewSource(OrgHeader master, OrgHeader target)
		{
			var orgMaster = new DeduplicationOrgHeader(master);
			var orgTarget = new DeduplicationOrgHeader(target);
			var scoreResult = TargetScorerController.Score(orgMaster, orgTarget, false);
			var resultModels = new[]
			{
				new PatternMatchingResultModel
				{
					OrgPK = orgTarget.OH_PK,
					ParentID = orgTarget.OH_PK
				}
			};

			DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, new List<ScoringResult> { scoreResult }, nameof(MergeOrganizationUtils), TimeSpan.MinValue, orgMaster);

			return new MergeOrganizationViewSource(orgMaster, new[] { orgTarget }, new[] { scoreResult }, resultModels, orgTarget.OH_PK);
		}

		protected IDeduplicationDebuggerParticipant DebuggerParticipant { get; }
	}
}
