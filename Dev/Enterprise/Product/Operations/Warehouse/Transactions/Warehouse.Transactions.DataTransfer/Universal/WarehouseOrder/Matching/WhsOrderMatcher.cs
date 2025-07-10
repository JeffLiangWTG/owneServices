using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsOrderMatcher : WhsOrderAndReceiveLastResortMatcher<WhsOrder>
	{
		internal WhsOrderMatcher(BusinessObjectFactory factory, WhsOrderAndReceiveReferences referencesParent, IXmlImportLogger logger)
			: base(factory, referencesParent, logger)
		{
		}

		protected override string DocketTypeCode
		{
			get { return DocketType.Codes.Order; }
		}
	}
}
