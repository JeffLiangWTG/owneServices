using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.DataTransfer.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsAdjustmentValueObjectDataAdapter : WhsDocketValueObjectDataAdapter<WhsAdjustment>
	{
		#region Constructors

		public WhsAdjustmentValueObjectDataAdapter()
			: base(EventsWithSourceType.Empty)
		{
		}

		public WhsAdjustmentValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#endregion

		#region Export

		protected override void ExportAdditionalToValueObjectCore(WhsAdjustment bizObj, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
			base.ExportAdditionalToValueObjectCore(bizObj, value, context);

			value.DocketDetail.Item = new Xsd.WhsCustomerAdjustmentDetail();
			var xsdAdjustment = (Xsd.WhsCustomerAdjustmentDetail)value.DocketDetail.Item;
			if (!bizObj.WD_FinalisedDate.IsEmpty)
			{
				xsdAdjustment.AdjustmentDate = bizObj.WD_FinalisedDate.ToZDateTime();
			}
		}

		protected override void ExportDocketLineAdditionalInfo(WhsDocketLine line, Xsd.WhsDocketLine value, INotifications notifications)
		{
			base.ExportDocketLineAdditionalInfo(line, value, notifications);
			value.ProductUQ = line.ProductUQ;
			value.QuantityFromClientOrder = line.WE_TransactionQuantity;
			value.Confirmation.Quantity = line.WE_TransactionQuantity;
		}

		protected override bool IncludeeDocs => SystemDataRegistry.Instance.IncludeWhsAdjustmenteDocs.Value;

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(WhsAdjustment bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Docket Type

		protected override ZString GetXsdWhsDocketType()
		{
			return DocketTypes.Codes.WhsAdjustment;
		}

		#endregion

		#region Error Handler

		protected override IErrorHandler GetNewDocketErrorHandler()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
