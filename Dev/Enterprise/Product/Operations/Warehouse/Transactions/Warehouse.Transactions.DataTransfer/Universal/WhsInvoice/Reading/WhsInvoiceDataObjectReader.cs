using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsInvoiceDataObjectReader : WhsDataObjectReader<WhsInvoice>
	{
		internal WhsInvoiceDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#region DataContextType

		public override DataContextType DataContextType => DataContextType.WarehousePeriodicInvoice;

		#endregion

		#region GetNewBusinessObject

		protected override WhsInvoice GetNewBusinessObject() => throw new DataObjectReadFailureException("Import of WhsInvoice Universal Shipments is not supported, only targeting for Universal Transaction XML (XUT).");

		#endregion

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<WhsInvoice> GetCombinedReferenceMatcher() => null;

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override WhsInvoice GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsInvoice targetBO) { }

		#endregion
	}
}

