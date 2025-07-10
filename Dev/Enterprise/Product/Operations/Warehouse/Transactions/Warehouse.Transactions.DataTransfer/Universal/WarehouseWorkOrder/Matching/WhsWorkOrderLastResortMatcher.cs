using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsWorkOrderLastResortMatcher : WhsDocketLastResortMatcher<WhsWorkOrder, WhsWorkOrderReferences>
	{
		public WhsWorkOrderLastResortMatcher(
			BusinessObjectFactory factory,
			WhsWorkOrderReferences referencesParent,
			IXmlImportLogger logger)
			: base(factory, referencesParent, logger)
		{
		}

		protected override string DocketTypeCode => DocketType.Codes.WorkOrder;

		protected override void BuildMatchingQueryAndMatchDelegates(WhsWorkOrderReferences referencesParent)
		{
			var matchDelegates = new List<MatchDelegate>();
			var query = new ZQuery();

			var externalReference = referencesParent.ExternalReference;
			if (!externalReference.IsEmpty)
			{
				query.AddToFilter(WhsDocketSchema.WD_ExternalReference, externalReference);
				matchDelegates.Add(whsDocket => GetMatchCount(whsDocket.WD_ExternalReference, externalReference));

				var externalReferenceSplit = referencesParent.ExternalReferenceSplit;
				query.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, externalReferenceSplit);
				matchDelegates.Add(whsDocket => GetMatchCount(whsDocket.WD_ExternalReferenceSplit, externalReferenceSplit));
			}

			BuildFullMatchDelegate(matchDelegates, query);
		}

		protected override void BuildFallbackMatchDelegates(WhsWorkOrderReferences referencesParent)
		{
			AddAdditionalReferencesFallback(referencesParent);
		}
	}
}
