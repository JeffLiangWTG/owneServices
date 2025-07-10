using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsAdjustmentMatcher : WhsDocketLastResortMatcher<WhsAdjustment, WhsAdjustmentReferences>
	{
		internal WhsAdjustmentMatcher(BusinessObjectFactory factory, WhsAdjustmentReferences references, IXmlImportLogger logger)
			: base(factory, references, logger)
		{
		}

		protected override void BuildMatchingQueryAndMatchDelegates(WhsAdjustmentReferences referencesParent)
		{
			AddPossibleMatch(WhsDocketSchema.WD_ExternalReference, referencesParent.AdjustmentReference, whsAdjustment => GetMatchCount(whsAdjustment.WD_ExternalReference, referencesParent.AdjustmentReference));
		}

		protected override void BuildFallbackMatchDelegates(WhsAdjustmentReferences referencesParent)
		{
		}

		protected override string DocketTypeCode => DocketType.Codes.Adjustment;
	}
}
