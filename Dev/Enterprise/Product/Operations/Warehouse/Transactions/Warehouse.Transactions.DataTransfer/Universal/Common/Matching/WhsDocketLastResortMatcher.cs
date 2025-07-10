using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsDocketLastResortMatcher<TDocket, TDocketReferences> : CombinationKeyMatcher<TDocket, TDocketReferences>
		where TDocket : WhsDocket
		where TDocketReferences : IReferencesParent
	{
		protected WhsDocketLastResortMatcher(BusinessObjectFactory factory, TDocketReferences referencesParent, IXmlImportLogger logger)
			: base(factory, referencesParent, logger)
		{
		}

		protected sealed override bool CheckLatestParent(TDocket docket, TDocket docketToCompare) => docket.WD_SystemCreateTimeUtc > docketToCompare.WD_SystemCreateTimeUtc;

		protected sealed override ZQuery GetFullQuery(ZQuery initialMatchingQuery, TDocketReferences references)
		{
			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketTypeCode);
			query.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Cancelled);
			query.AddToFilter(initialMatchingQuery);
			return query;
		}

		protected abstract string DocketTypeCode { get; }

		protected void AddAdditionalReferencesFallback(IReferencesParentWithAdditionalReferences referencesParent)
		{
			AddFallbackMatch(
				referencesParent.References,
				whsDocket => GetMatchCount(
					whsDocket.References,
					WhsDocketReferenceSchema.WX_RefType,
					WhsDocketReferenceSchema.WX_Reference,
					referencesParent.References));
		}

		protected void BuildFullMatchDelegate(List<MatchDelegate> matchDelegates, ZQuery query)
		{
			if (!query.IsEmpty)
			{
				MatchDelegate fullMatch = whsDocket =>
				{
					var result = 1;

					foreach (var match in matchDelegates)
					{
						result *= match(whsDocket);
					}

					return result;
				};

				AddPossibleMatch(query, fullMatch);
			}
		}
	}
}
