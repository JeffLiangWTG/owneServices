using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Definitions.HR;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.HRM.Common.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ReviewProcessNodeCopybackServiceTask.Code,
	"Submit a Review Node Proposals",
	"HRM",
	typeof(ReviewProcessNodeCopybackServiceTask),
	MinimumPeriod = "1day",
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "1day")]

[assembly: HostedServiceBusinessObjectBinding(ReviewProcessNodeCopybackServiceTask.Code,
	ReviewProcessNodeSchema.Constants.TableName,
	new[] { ReviewProcessNodeSchema.Constants.RRN_Status + "=APP" },
	"Ready to Submit Review Node Proposals"
)]

namespace Enterprise.HRM.Common.ServiceTasks
{
	public class ReviewProcessNodeCopybackServiceTask : ServiceProviderImpl
	{
		public const string Code = "RPR";
		readonly int batchSize;

		public ReviewProcessNodeCopybackServiceTask()
		{ }

		public ReviewProcessNodeCopybackServiceTask(int batchSize = 50)
		{
			this.batchSize = batchSize;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Logger strings")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			using var hrmConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName);
			var factory = new BusinessObjectFactory(hrmConnection);

			var processNodes = factory.Load<ReviewProcessNode>(new ZQuery(ReviewProcessNodeSchema.RRN_Status, "APP"));
			var nodesToProcess = new Queue<ReviewProcessNode>(processNodes);
			var reviewProcessSet = new HashSet<ReviewProcess>(processNodes.Select(n => n.ReviewProcess));
			var nodesProcessedSinceLastSave = 0;

			if (processNodes == null)
			{
				ServiceLogger.Information("Found no submitted review process node.");
				return;
			}

			while (nodesToProcess.Count > 0)
			{
				var processNode = nodesToProcess.Dequeue();
				ServiceLogger.Information("Found Node For Review Process: " + processNode.ReviewProcess.RPR_Name + " Reviewer: " + processNode.Reviewer.GS_Code);

				switch (processNode.ReviewProcess.RPR_Type)
				{
					case "REM":
						DoRemCopyback(factory, processNode);
						break;
					case "CLA":
						DoClassificationCopyback(factory, processNode);
						break;
					case "PER":
						DoPerformanceCopyback(factory, processNode);
						break;
					case "C&P":
						DoClassificationCopyback(factory, processNode);
						DoPerformanceCopyback(factory, processNode);
						break;
					default:
						break;
				}

				processNode.RRN_Status = "FIN";

				if (++nodesProcessedSinceLastSave >= batchSize)
				{
					factory.Save();
					nodesProcessedSinceLastSave = 0;
				}
			}

			foreach (var reviewProcess in reviewProcessSet)
			{
				var query = new ZQuery(ReviewProcessNodeSchema.RRN_RPR_ReviewProcess, reviewProcess.PK);
				query.AddToFilter(ReviewProcessNodeSchema.RRN_Status, SQLComparisonOperator.NotEqual, "FIN");
				var unclosedNode = factory.LoadTop1<ReviewProcessNode>(query);

				if (unclosedNode == null)
				{
					reviewProcess.RPR_Status = ReviewProcessStatusCodes.Closed;
				}
			}

			factory.Save();
		}

		void DoClassificationCopyback(BusinessObjectFactory factory, ReviewProcessNode processNode)
		{
			var proposals = factory.Load<ReviewProposal>(new ZQuery(ReviewProposalSchema.RRP_RRN_ReviewNode, processNode.PK)).ToList();
			proposals.RemoveAll(p => p.RRP_IsExcluded);

			foreach (var proposal in proposals)
			{
				if (proposal.RRP_Classification.IsEmpty)
				{
					var message = $"Classification missing on proposal for staff {proposal.Staff.GS_Code}";
					ServiceLogger.Error(message);
					throw new InvalidOperationException(message);
				}

				var query = new ZDBOnlyQuery(typeof(GlbStaffClassification));
				_ = query.AddToFilter(GlbStaffClassificationSchema.GSL_GS_Staff, proposal.RRP_GS_Staff);

				var sql = "CAST(GSL_EffectiveDate AS Date) = @effectiveDate";
				var parameters = new ZSqlParameterCollection
				{
					{ "@effectiveDate", processNode.ReviewProcess.RPR_EffectiveDate, GlbStaffClassificationSchema.GSL_SystemCreateTimeUtc }
				};
				_ = query.AddFilterAndZSQLParameterCollection(sql, parameters);

				if (factory.LoadTop1<GlbStaffClassification>(query) != null)
				{
					var message = $"Classification already exists for staff {proposal.Staff.GS_Code} with effective date {processNode.ReviewProcess.RPR_EffectiveDate}";
					ServiceLogger.Error(message);
					throw new InvalidOperationException(message);
				}

				var classification = factory.New<GlbStaffClassification>();
				classification.GSL_Classification = proposal.RRP_Classification;
				classification.GSL_GS_Staff = proposal.RRP_GS_Staff;
				classification.GSL_EffectiveDate = ToStaffTimezone(factory, proposal.Staff, processNode.ReviewProcess.RPR_EffectiveDate.ToDateTime());
			}
		}

		void DoPerformanceCopyback(BusinessObjectFactory factory, ReviewProcessNode processNode)
		{
			var proposals = factory.Load<ReviewProposal>(new ZQuery(ReviewProposalSchema.RRP_RRN_ReviewNode, processNode.PK)).ToList();
			proposals.RemoveAll(p => p.RRP_IsExcluded);

			foreach (var proposal in proposals)
			{
				var query = new ZDBOnlyQuery(typeof(GlbStaffReview));
				_ = query.AddToFilter(GlbStaffReviewSchema.GSV_GS_Staff, proposal.RRP_GS_Staff);

				var sql = "CAST(GSV_EffectiveDate AS Date) = @effectiveDate";
				var parameters = new ZSqlParameterCollection
				{
					{ "@effectiveDate", processNode.ReviewProcess.RPR_EffectiveDate, GlbStaffReviewSchema.GSV_SystemCreateTimeUtc }
				};
				_ = query.AddFilterAndZSQLParameterCollection(sql, parameters);

				if (factory.LoadTop1<GlbStaffReview>(query) != null)
				{
					var message = $"Performance score already exists for staff {proposal.Staff.GS_Code} with effective date {processNode.ReviewProcess.RPR_EffectiveDate}";
					ServiceLogger.Error(message);
					throw new InvalidOperationException(message);
				}

				var performance = factory.New<GlbStaffReview>();
				performance.GSV_Score = (byte)proposal.RRP_PerformanceScore;
				performance.GSV_GS_Staff = proposal.RRP_GS_Staff;
				performance.GSV_GS_NKReviewer = processNode.Reviewer.GS_Code;
				performance.GSV_EffectiveDate = ToStaffTimezone(factory, proposal.Staff, processNode.ReviewProcess.RPR_EffectiveDate.ToDateTime());
				performance.GSV_Comments = proposal.RRP_Comments;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Logger strings")]
		void DoRemCopyback(BusinessObjectFactory factory, ReviewProcessNode processNode)
		{
			var proposals = factory.Load<ReviewProposal>(new ZQuery(ReviewProposalSchema.RRP_RRN_ReviewNode, processNode.PK)).ToList();
			proposals.RemoveAll(p => p.RRP_IsExcluded);

			foreach (var proposal in proposals)
			{
				var query = new ZQuery(GlbStaffRemunerationSchema.GSR_GS_Staff, proposal.RRP_GS_Staff);
				query.AddToFilter(GlbStaffRemunerationSchema.GSR_AutoEffectiveEndDate, null);
				var latestRemuneration = factory.LoadTop1<GlbStaffRemuneration>(query);

				if (latestRemuneration == null)
				{
					var message = "Not Found StaffRemuneration For Staff: " + proposal.Staff.GS_Code;
					ServiceLogger.Error(message);
					throw new InvalidOperationException(message);
				}

				if (latestRemuneration.GSR_EffectiveDate.Date == processNode.ReviewProcess.RPR_EffectiveDate)
				{
					var message = $"Remuneration already exists for staff {proposal.Staff.GS_Code} with effective date {processNode.ReviewProcess.RPR_EffectiveDate}";
					ServiceLogger.Error(message);
					throw new InvalidOperationException(message);
				}

				var remunerationBizO = factory.New<GlbStaffRemuneration>();
				remunerationBizO.GSR_GS_Staff = latestRemuneration.GSR_GS_Staff;
				remunerationBizO.GSR_EffectiveDate = ToStaffTimezone(factory, proposal.Staff, processNode.ReviewProcess.RPR_EffectiveDate.ToDateTime());
				remunerationBizO.GSR_FullTimeEquivalent = latestRemuneration.GSR_FullTimeEquivalent;
				remunerationBizO.GSR_LeaveLiabilityHourlyRate = latestRemuneration.GSR_LeaveLiabilityHourlyRate;
				remunerationBizO.GSR_RN_NKCountry = latestRemuneration.GSR_RN_NKCountry;
				remunerationBizO.GSR_RX_NKCurrency = latestRemuneration.GSR_RX_NKCurrency;

				var latestPackage = factory.Load<GlbStaffEntitlement>(new ZQuery(GlbStaffEntitlementSchema.GSI_GSR_Remuneration, latestRemuneration.PK)).ToList();
				var proposalEntitlements = factory.Load<ReviewProposalEntitlement>(new ZQuery(ReviewProposalEntitlementSchema.RRE_RRP_Proposal, proposal.PK)).ToList();

				latestPackage.RemoveAll(i => i.GSI_Value == 0);
				proposalEntitlements.RemoveAll(p => p.RRE_Value == 0);

				UpdatePackage(factory, processNode, remunerationBizO, latestPackage, proposalEntitlements);
			}
		}

		void UpdatePackage(BusinessObjectFactory factory, ReviewProcessNode processNode, GlbStaffRemuneration newRemuneration, List<GlbStaffEntitlement> latestPackage, List<ReviewProposalEntitlement> proposalPackage)
		{
			foreach (var latestPackageItem in latestPackage)
			{
				var matchingProposalItem = proposalPackage.FirstOrDefault(p => p.RRE_EntitlementCode == latestPackageItem.GSI_EntitlementCode);
				var newEntitlementItem = factory.New<GlbStaffEntitlement>();
				if (matchingProposalItem == null)
				{
					newEntitlementItem.GSI_Value = latestPackageItem.GSI_Value;
					newEntitlementItem.GSI_GrantDate = latestPackageItem.GSI_GrantDate;
				}
				else
				{
					newEntitlementItem.GSI_GrantDate =
						latestPackageItem.GSI_Value == matchingProposalItem.RRE_Value
						? latestPackageItem.GSI_GrantDate
						: processNode.ReviewProcess.RPR_EffectiveDate;

					newEntitlementItem.GSI_Value = matchingProposalItem.RRE_Value;

					proposalPackage.Remove(matchingProposalItem);
				}

				newEntitlementItem.GSI_EntitlementCode = latestPackageItem.GSI_EntitlementCode;
				newEntitlementItem.GSI_IsFTEScalable = latestPackageItem.GSI_IsFTEScalable;
				newEntitlementItem.GSI_Frequency = latestPackageItem.GSI_Frequency;
				newEntitlementItem.GSI_IsPartOfPackage = latestPackageItem.GSI_IsPartOfPackage;
				newEntitlementItem.GSI_GSR_Remuneration = newRemuneration.PK;
				newEntitlementItem.GSI_GEG_Breakdown = latestPackageItem.GSI_GEG_Breakdown;
			}

			var proposalEntitlements = new List<GlbStaffEntitlement>();
			foreach (var proposalPackageItem in proposalPackage)
			{
				var resultEntitlementItem = factory.New<GlbStaffEntitlement>();

				resultEntitlementItem.GSI_GSR_Remuneration = newRemuneration.PK;
				resultEntitlementItem.GSI_EntitlementCode = proposalPackageItem.RRE_EntitlementCode;
				resultEntitlementItem.GSI_Value = proposalPackageItem.RRE_Value;
				resultEntitlementItem.GSI_GrantDate = processNode.ReviewProcess.RPR_EffectiveDate;
				resultEntitlementItem.GSI_GEG_Breakdown = Guid.Empty;

				proposalEntitlements.Add(resultEntitlementItem);
			}
		}

		ZDateTimeOffset ToStaffTimezone(BusinessObjectFactory factory, GlbStaff staff, DateTime datetime)
		{
			var offset = StaffTimezoneOffsetProvider.GetOffset(factory, staff.PK, datetime);

			return new ZDateTimeOffset(datetime.Ticks, offset);
		}
	}
}
