using System;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsReceiveConfirmationValueObjectDataAdapter : WhsReceiveValueObjectDataAdapter
	{
		#region Constructors

		public WhsReceiveConfirmationValueObjectDataAdapter()
			: base(EventsWithSourceType.Empty)
		{
		}

		public WhsReceiveConfirmationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#endregion

		#region Overrides

		public override void ImportFromValueObject(WhsReceive bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Job Confirmation Import is not supported");
		}

		protected override void ImportFromValueObjectCore(WhsReceive bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Job Confirmation Import is not supported");
		}

		protected override void ExportAdditionalToValueObjectCore(WhsReceive bizObj, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
			base.ExportAdditionalToValueObjectCore(bizObj, value, context);
			value.Identifier.ActionType = Xsd.WhsDocketIdentifierActionType.CON;
		}

		protected override void ExportDocketLineAdditionalInfo(WhsDocketLine line, Xsd.WhsDocketLine value, INotifications notifications)
		{
			base.ExportDocketLineAdditionalInfo(line, value, notifications);
			value.ProductUQ = line.ProductUQ;
			value.QuantityActuallyOrdered = line.WE_ClientOrderedUnits;
			value.QuantityFromClientOrder = line.WE_ClientOrderedUnits;
			value.Confirmation.Quantity = line.WE_TransactionQuantity;
		}

		#endregion
	}
}
