using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public static class QuoteStatus
	{
		public static CodeDescriptionPairList GetStatuses(bool isFromOneOffQuoteModule = false)
		{
			var statuses = new CodeDescriptionPairList();
			statuses.AddPair(Quote.QuoteStatusOptions.Accepted.ToString(), Res.GetString("865a5613-557b-4e9f-9673-9f7dd879c37b", "Accepted"));
			statuses.AddPair(Quote.QuoteStatusOptions.ClientAccepted.ToString(), Res.GetString("e75b2031-70f2-4507-a550-1dcba1b991bb", "Client Accepted"));
			statuses.AddPair(Quote.QuoteStatusOptions.Active.ToString(), Res.GetString("cfbad1a7-4094-4569-9381-48cc7da2e786", "Active, Finalized and Approved"));
			statuses.AddPair(Quote.QuoteStatusOptions.Approved.ToString(), Res.GetString("f10dbd62-adb7-44d4-aef9-f85962d25ca2", "Approved"));
			statuses.AddPair(Quote.QuoteStatusOptions.Finalized.ToString(), Res.GetString("0641a0aa-8433-44b5-a032-6154200fbdea", "Finalized Only"));
			statuses.AddPair(Quote.QuoteStatusOptions.Cancelled.ToString(), Res.GetString("ec94eecd-8269-4923-8c58-59b3f6e095f3", "Canceled"));
			statuses.AddPair(Quote.QuoteStatusOptions.Expired.ToString(), Res.GetString("38bf8c9b-3713-45c2-927c-e79e8bd1fd30", "Expired"));
			if (isFromOneOffQuoteModule)
			{
				statuses.AddPair(Quote.QuoteStatusOptions.Used.ToString(), Res.GetString("706f0a3a-666c-46c7-b4c5-37d16c4bb6f6", "Used"));
			}

			return statuses;
		}

		#region Query Status Query

		public static void SetQueryStatusQuery(ZDBOnlyQuery query, ZString status)
		{
			// Try to keep the order of these ifs to be similar to Quote.cs -> QuoteStatus
			if (status == Quote.QuoteStatusOptions.Cancelled)
			{
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.True);
			}
			else if (status == Quote.QuoteStatusOptions.Accepted)
			{
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			}
			else if (status == Quote.QuoteStatusOptions.Used)
			{
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsOneOffQuoteConsumed, SQLComparisonOperator.Equal, ZBool.True);
			}
			else if (status == Quote.QuoteStatusOptions.ClientAccepted)
			{
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_ClientAccepted, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			}
			else if (status == Quote.QuoteStatusOptions.Finalized)
			{
				query.AddToFilter(QuoteEndDateGreaterThanToday, JoinCondition.And);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsLocked, SQLComparisonOperator.Equal, ZBool.True);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_ClientAccepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsOneOffQuoteConsumed, SQLComparisonOperator.Equal, ZBool.False);
			}
			else if (status == Quote.QuoteStatusOptions.Approved)
			{
				query.AddSubQuery(GetApprovedQuotations(), JoinCondition.And);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_ClientAccepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsOneOffQuoteConsumed, SQLComparisonOperator.Equal, ZBool.False);
				query.AddToFilter(QuoteEndDateGreaterThanToday, JoinCondition.And);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsLocked, SQLComparisonOperator.Equal, ZBool.False);
			}
			// It will filter all quotations with the status of Active, Finalized and Approved
			else if (status == Quote.QuoteStatusOptions.Active)
			{
				query.AddToFilter(QuoteEndDateGreaterThanToday, JoinCondition.And);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_ClientAccepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsOneOffQuoteConsumed, SQLComparisonOperator.Equal, ZBool.False);
			}
			else if (status == Quote.QuoteStatusOptions.Expired)
			{
				query.AddToFilter(QuoteEndDateLessThanToday, JoinCondition.And);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_Accepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_ClientAccepted, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			}
		}

		static ZDBOnlySubQuery GetApprovedQuotations()
		{
			ZDBOnlySubQuery result = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.QuotationInternallyApproved.Code);

			return result;
		}

		static ZQuery QuoteEndDateGreaterThanToday
		{
			get
			{
				ZQuery result = new ZQuery(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
				result.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.Equal, null);

				return result;
			}
		}

		static ZQuery QuoteEndDateLessThanToday => new ZQuery(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.LessThan, ZDateTime.Today);

		#endregion
	}
}
