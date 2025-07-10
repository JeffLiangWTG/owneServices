using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class RelatedBrokerageJobMatcher : MasterFiles.Business.UniversalData.RelatedBrokerageJobMatcher
	{
		protected override BusinessObject GetMatchingJobCore(Dictionary<RelatedJobMatcherKeys, ZString> matchingCriteria)
		{
			BusinessObject result = null;
			var query = new ZQuery();
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);

			if (matchingCriteria.ContainsKey(RelatedJobMatcherKeys.ITN))
			{
				var itn = matchingCriteria[RelatedJobMatcherKeys.ITN];
				if (!itn.IsEmpty)
				{
					var declarationSubQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
					var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
					var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, itn);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN);

					entryHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
					declarationSubQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
					query.AddToFilter(declarationSubQuery);

					var jobs = new BusinessObjectFactory().Load<JobDeclaration>(query);
					if (jobs.Length == 1)
					{
						result = jobs[0];
					}
				}
			}

			if (result == null && matchingCriteria.ContainsKey(RelatedJobMatcherKeys.BookingRefNumber))
			{
				var bookingRefNumber = matchingCriteria[RelatedJobMatcherKeys.BookingRefNumber];
				if (!bookingRefNumber.IsEmpty)
				{
					query = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_TransportReference, bookingRefNumber);

					var jobs = new BusinessObjectFactory().Load<JobDeclaration>(query);
					if (jobs.Length == 1)
					{
						result = jobs[0];
					}
				}
			}

			if (result == null)
			{
				result = base.GetMatchingJobCore(matchingCriteria);
			}
			return result;
		}
	}
}
