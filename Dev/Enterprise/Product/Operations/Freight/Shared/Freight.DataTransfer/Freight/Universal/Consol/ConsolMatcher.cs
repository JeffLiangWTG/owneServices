using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ConsolMatcher<T> : CombinationKeyMatcher<T, CommonConsolReferences>
		where T : CommonConsol
	{
		public ConsolMatcher(BusinessObjectFactory factory, CommonConsolReferences references, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, references, logger)
		{
			this.helper = Argument.NotNull(helper, "helper");
			this.references = Argument.NotNull(references, "references");
		}

		readonly IUniversalFreightHelper helper;
		readonly CommonConsolReferences references;

		protected override bool CheckLatestParent(T consol, T consolToCompare)
		{
			return consol.JK_SystemCreateTimeUtc > consolToCompare.JK_SystemCreateTimeUtc;
		}

		protected override void BuildMatchingQueryAndMatchDelegates(CommonConsolReferences referencesParent)
		{
			if (referencesParent.MatchMainCarrierReferencesToCoLoader)
			{
				if (referencesParent.MBOLNumber.IsEmpty
					&& referencesParent.CarriersBookingReference.IsEmpty
					&& referencesParent.CoLoadMasterBillNumber.IsEmpty
					&& referencesParent.CoLoadBookingConfirmationReference.IsEmpty)
				{
					return;
				}

				BuildVGMConsolReferencesQueryAndMatchDelegate(referencesParent);
			}
			else
			{
				if (!referencesParent.CarriersBookingReference.IsEmpty)
				{
					var bookingReferenceQuery = new ZQuery(JobConsolSchema.JK_BookingReference, referencesParent.CarriersBookingReference);
					AddPossibleMatch(bookingReferenceQuery.AddToFilter(BuildCoLoadTypeQuery(false)), consol => GetMatchCount(consol.JK_BookingReference, referencesParent.CarriersBookingReference));

					var coLoadBookingReferenceQuery = new ZQuery(JobConsolSchema.JK_CoLoadBookingReference, referencesParent.CarriersBookingReference);
					AddPossibleMatch(coLoadBookingReferenceQuery.AddToFilter(BuildCoLoadTypeQuery(true)), consol => GetMatchCount(consol.JK_CoLoadBookingReference, referencesParent.CarriersBookingReference));
				}

				AddPossibleMatch(JobConsolSchema.JK_AgentsReference, referencesParent.AgentsReference, consol => GetMatchCount(consol.JK_AgentsReference, referencesParent.AgentsReference));

				if (helper.ConsolHasAdditionalReferences)
				{
					BuildAdditionalReferencesQueryAndMatchDelegate(referencesParent);
				}
			}
		}

		ZQuery BuildCoLoadTypeQuery(bool isCoLoad)
		{
			var sqlComparisonOperator = isCoLoad ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			return new ZQuery(JobConsolSchema.JK_AgentType, sqlComparisonOperator, Constants.AgentType.CoLoad);
		}

		void BuildAdditionalReferencesQueryAndMatchDelegate(CommonConsolReferences referencesParent)
		{
			var helper = new AdditionalReferencesMatchingHelper<T>(factory);
			var consolPKsMatchingAdditionalReferences = helper.GetParentsMatchingAdditionalReferences(referencesParent, null);

			AddPossibleMatch(JobConsolSchema.PK, consolPKsMatchingAdditionalReferences,
				consol => GetMatchCount(consol.Numbers, CusEntryNumSchema.CE_EntryType, CusEntryNumSchema.CE_EntryNum, referencesParent.AdditionalReferences));
		}

		protected override void BuildFallbackMatchDelegates(CommonConsolReferences referencesParent)
		{
			if (!referencesParent.MatchMainCarrierReferencesToCoLoader)
			{
				AddFallbackMatch(referencesParent.LoadPort, consol => GetMatchCount(consol.JK_RL_NKLoadPort, referencesParent.LoadPort));
				AddFallbackMatch(referencesParent.DischargePort, consol => GetMatchCount(consol.JK_RL_NKDischargePort, referencesParent.DischargePort));
				AddFallbackMatch(referencesParent.CarrierC1CCode, consol => GetMatchCount(consol.ShippingLine?.C1CCode ?? ZString.Empty, referencesParent.CarrierC1CCode));
			}
		}

		protected override ZQuery GetFullQuery(ZQuery initialMatchingQuery, CommonConsolReferences referencesParent)
		{
			var result = base.GetFullQuery(initialMatchingQuery, referencesParent);
			helper.AddConsolParameters(result);
			return result;
		}

		protected override T GetLatestParentIfApplicable(List<T> parentsToLookThrough)
		{
			if (!references.IsConsolidationAdvice)
			{
				MatchConsolResultService.Register(factory, parentsToLookThrough, references.CarriersBookingReference, ZString.Empty);

				var service = MatchConsolResultService.GetInstance(factory);
				if (service != null && service.HasConsolsInYearRange)
				{
					var message = Res.GetString("09dff33d-8786-4ce2-a09f-edeb70ad7fac",
	@"System cannot find a Consol to link as there are multiple consols with the same Booking Number {0}.
Consolidations with the same Booking Number:
{1}", references.CarriersBookingReference, string.Join(",", service.ConsolNames));

					logger?.LogBoth(LogType.Information, message);
					MatchConsolResultService.UnRegister(factory);

					return null;
				}
			}

			return base.GetLatestParentIfApplicable(parentsToLookThrough);
		}

		#region VGM

		void BuildVGMConsolReferencesQueryAndMatchDelegate(CommonConsolReferences referencesParent)
		{
			var query = new ZQuery();

			var coloadTypeQuery = new ZQuery(JobConsolSchema.JK_AgentType, Core.Constants.AgentType.CoLoad);

			if (!referencesParent.MBOLNumber.IsEmpty)
			{
				query.AddToFilter(JobConsolSchema.JK_MasterBillNum, referencesParent.MBOLNumber);

				var subQuery = new ZQuery(JobConsolSchema.JK_CoLoadMasterBill, referencesParent.MBOLNumber);
				subQuery.AddToFilter(coloadTypeQuery);
				query.AddToFilter(subQuery, JoinCondition.Or);
			}

			if (!referencesParent.CoLoadMasterBillNumber.IsEmpty)
			{
				var subQuery = new ZQuery(JobConsolSchema.JK_CoLoadMasterBill, referencesParent.CoLoadMasterBillNumber);
				subQuery.AddToFilter(coloadTypeQuery);
				query.AddToFilter(subQuery, JoinCondition.Or);
			}

			if (!referencesParent.CarriersBookingReference.IsEmpty)
			{
				var subQuery = new ZQuery(JobConsolSchema.JK_BookingReference, referencesParent.CarriersBookingReference);
				query.AddToFilter(subQuery, JoinCondition.Or);

				subQuery = new ZQuery(JobConsolSchema.JK_CoLoadBookingReference, referencesParent.CarriersBookingReference);
				subQuery.AddToFilter(coloadTypeQuery);
				query.AddToFilter(subQuery, JoinCondition.Or);
			}

			if (!referencesParent.CoLoadBookingConfirmationReference.IsEmpty)
			{
				var subQuery = new ZQuery(JobConsolSchema.JK_CoLoadBookingReference, referencesParent.CoLoadBookingConfirmationReference);
				subQuery.AddToFilter(coloadTypeQuery);
				query.AddToFilter(subQuery, JoinCondition.Or);
			}

			AddPossibleMatch(query, CreateMatchDelegate(referencesParent));
		}

		MatchDelegate CreateMatchDelegate(CommonConsolReferences referencesParent)
		{
			return consol =>
			{
				var matchCount = 0;

				if (!referencesParent.SCAC.IsEmpty)
				{
					matchCount += consol.ShippingLine.HasSCAC(referencesParent.SCAC) ? 4 : 0;
					matchCount += consol.Creditor.HasSCAC(referencesParent.SCAC) ? 4 : 0;
				}

				if (!referencesParent.CoLoadSCAC.IsEmpty)
				{
					matchCount += consol.Creditor.HasSCAC(referencesParent.CoLoadSCAC) ? 4 : 0;
				}

				if (!referencesParent.MBOLNumber.IsEmpty)
				{
					matchCount += consol.JK_MasterBillNum == referencesParent.MBOLNumber ? 2 : 0;
					matchCount += consol.IsCoLoad && consol.JK_CoLoadMasterBill == referencesParent.MBOLNumber ? 2 : 0;
				}

				if (!referencesParent.CoLoadMasterBillNumber.IsEmpty)
				{
					matchCount += consol.IsCoLoad && consol.JK_CoLoadMasterBill == referencesParent.CoLoadMasterBillNumber ? 2 : 0;
				}

				if (!referencesParent.CarriersBookingReference.IsEmpty)
				{
					matchCount += consol.JK_BookingReference == references.CarriersBookingReference ? 1 : 0;
					matchCount += consol.IsCoLoad && consol.JK_CoLoadBookingReference == references.CarriersBookingReference ? 1 : 0;
				}

				if (!referencesParent.CoLoadBookingConfirmationReference.IsEmpty)
				{
					matchCount += consol.IsCoLoad && consol.JK_CoLoadBookingReference == references.CoLoadBookingConfirmationReference ? 1 : 0;
				}

				return matchCount;
			};
		}

		#endregion
	}
}
