using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsInvoiceDataObjectWriter : TopLevelDataObjectWriter<WhsInvoice, Shipment>
	{
		internal WhsInvoiceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		#region GetEDIMessageSubType

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		#endregion

		#region GetTopLevelDataContextType

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.WarehousePeriodicInvoice;

		#endregion

		#region PopulateDataObject

		protected override void PopulateDataObject(WhsInvoice sourceBO, Shipment dataObject) { }

		#endregion
	}
}

