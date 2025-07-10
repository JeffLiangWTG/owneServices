using System.Collections.Generic;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingInventoryDetailsColumnProvider : GridColumnProvider
	{
		public TrackingInventoryDetailsColumnProvider()
			: base()
		{
		}

		public TrackingInventoryDetailsColumnProvider(PartAttributeManager attributeManager)
			: base()
		{
			AttributeManager = attributeManager;
		}

		readonly PartAttributeManager AttributeManager;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			var receiptRefColumn = new ZHyperLinkColumn(Res.GetString("e974d3d1-6bf4-4d3e-b6c2-1aab04f25198", "Receipt Ref"), "ReceiptReference") { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ReceiptReference };
			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				receiptRefColumn.DataNavigateUrlFormatString = (NoResString)@"javascript: parent." + HttpContext.Current.Request.QueryString[ZIFramePage.OKFunctionQuery] + (NoResString)"('{0}','{1}');"; // non-semantic text
			}
			receiptRefColumn.DataNavigateUrlFields = new string[] { "ReceiptReference", "PK" };
			AddToDictionaryAsRequired(receiptRefColumn);

			ZBindToChecker.CheckBindTo(((ZString)((WhsInventoryView)null).WI_OP_PartNum));
			var productColumn = new ZTextEditColumn(Res.GetString("c50620a2-6c08-4811-bd31-fbece914e9cc", "Product"), WhsInventoryView.Schema.WI_OP_PartNum) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.Product };
			AddToDictionaryAsDefault(productColumn);

			ZBindToChecker.CheckBindTo(((ZDateTimeOffset)(((WhsInventoryView)(null)).WI_ArrivalDateOrETA)));
			var arrivalDateColumn = new ZDateTimeColumn(Res.GetString("7bac57e7-ac49-44c1-a8fb-eed8cd6ab9fa", "ETA/Arrival"), WhsInventoryView.Schema.WI_ArrivalDateOrETA) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ETA };
			AddToDictionaryAsDefault(arrivalDateColumn);

			ZBindToChecker.CheckBindTo(((ZString)(((WhsInventoryView)(null)).StatusDesc)));
			var statusColumn = new ZTextEditColumn(Res.GetString("71347b95-96e4-4de5-a45a-30cad28983f1", "Status"), "StatusDesc") { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.Status };
			AddToDictionary(statusColumn);

			ZBindToChecker.CheckBindTo(((ZDecimal)(((WhsInventoryView)(null)).WI_AvailableToPickQuantity)));
			var availableQtyColumn = new ZCalcEditColumn(Res.GetString("ce7d7aa7-fb5c-420e-8a4a-9d9299d3b3dc", "Available Pick Qty"), WhsInventoryView.Schema.WI_AvailableToPickQuantity) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.AvailableToPickQuantity };
			AddToDictionary(availableQtyColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((WhsInventoryView)null).InternalsProxy.CommittedToTransactionQuantity);
			var committedQtyColumn = new ZCalcEditColumn(Res.GetString("0a336e74-5016-448c-9f4d-f0824a4b2a6a", "Committed Qty"), "InternalsProxy+" + WhsInventoryView.Schema.CommittedToTransactionQuantity) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.CommittedQuantity };
			AddToDictionary(committedQtyColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((WhsInventoryView)null).WI_CrossDockQuantity);
			var allocatedQtyColumn = new ZCalcEditColumn(Res.GetString("988ff1e1-b61d-4dff-9c84-20bcbdd7835f", "Reserved Qty"), WhsInventoryView.Schema.WI_CrossDockQuantity) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ReservedQuantity };
			AddToDictionary(allocatedQtyColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((WhsInventoryView)null).WI_TotalUnits);
			var totalQtyColumn = new ZCalcEditColumn(Res.GetString("d2a54723-e1ec-451e-90eb-1c50a9e9b4bc", "Total Qty"), WhsInventoryView.Schema.WI_TotalUnits) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.TotalQuantity };
			AddToDictionary(totalQtyColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((WhsInventoryView)null).WI_TotalValue);
			var totalValueColumn = new ZCalcEditColumn(Res.GetString("01376ea0-cf18-45c1-a1fa-3ad39458db98", "Total Value"), WhsInventoryView.Schema.WI_TotalValue) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.TotalValue };
			AddToDictionary(totalValueColumn);

			ZBindToChecker.CheckBindTo((ZString)((WhsInventoryView)null).WI_Currency);
			var totalValueCurrencyColumn = new ZTextEditColumn(Res.GetString("c9b0648b-1f20-456d-9afa-1e8d255da153", "Currency"), WhsInventoryView.Schema.WI_Currency) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.Currency };
			AddToDictionary(totalValueCurrencyColumn);

			if (AttributeManager != null)
			{
				ZBindToChecker.CheckBindTo((ZString)((WhsInventoryView)null).WI_PartAttrib1);
				ZBindToChecker.CheckBindTo((ZString)((WhsInventoryView)null).WI_PartAttrib2);
				ZBindToChecker.CheckBindTo((ZString)((WhsInventoryView)null).WI_PartAttrib3);

				var partAttr1Column = new ZTextEditColumn(AttributeManager.PartAttributeName1, WhsInventoryViewSchema.WI_PartAttrib1.Name) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PartAttribute1 };
				var partAttr2Column = new ZTextEditColumn(AttributeManager.PartAttributeName2, WhsInventoryViewSchema.WI_PartAttrib2.Name) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PartAttribute2 };
				var partAttr3Column = new ZTextEditColumn(AttributeManager.PartAttributeName3, WhsInventoryViewSchema.WI_PartAttrib3.Name) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PartAttribute3 };

				if (AttributeManager.IsPartAttributeUsedByOrganisation(1))
				{
					AddToDictionary(partAttr1Column);
				}
				if (AttributeManager.IsPartAttributeUsedByOrganisation(2))
				{
					AddToDictionary(partAttr2Column);
				}
				if (AttributeManager.IsPartAttributeUsedByOrganisation(3))
				{
					AddToDictionary(partAttr3Column);
				}
				if (AttributeManager.IsSerialNumberUsedByOrganisation)
				{
					var serialNumberColumn = new ZTextEditColumn(Res.GetString("d94a3332-cb0f-46fc-8258-dc4d05aaed66", "Serial Number"), WhsInventoryViewSchema.WI_SerialNumber.Name) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.SerialNumber };
					AddToDictionary(serialNumberColumn);
				}
				ZBindToChecker.CheckBindTo(((ZDateTime)(((WhsInventoryView)(null)).WI_ExpiryDate)));
				ZBindToChecker.CheckBindTo(((ZDateTime)(((WhsInventoryView)(null)).WI_PackingDate)));

				var expiryDateColumn = new ZDateTimeColumn(Res.GetString("8d8c1e47-7248-4caa-ba3d-914fedc18a32", "Expiry Date"), WhsInventoryViewSchema.WI_ExpiryDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.ExpiryDate };
				var packingDateColumn = new ZDateTimeColumn(Res.GetString("6e4664c3-6be0-498b-a2b1-d688d2752e6e", "Packing Date"), WhsInventoryViewSchema.WI_PackingDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingInventoryDetails.PackingDate };

				if (AttributeManager.IsExpiryDateUsedByOrganisation)
				{
					AddToDictionary(expiryDateColumn);
				}
				if (AttributeManager.IsPackingDateUsedByOrganisation)
				{
					AddToDictionary(packingDateColumn);
				}
			}

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsInventory)null).SupplierPart.OP_LastCost);
			var lastCostColumn = new ZCalcEditColumn(Res.GetString("6cd8cdd5-6327-4406-a607-c0f232e65743", "Last Cost"), "SupplierPart+" + Enterprise.MasterFiles.Business.OrgSupplierPart.Schema.OP_LastCost) { Decimals = 2, ColumnKey = WebTracker.Grids.TrackingInventoryDetails.LastCost };
			AddToDictionary(lastCostColumn);
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.ReceiptReference);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.Product);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.ETA);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.Status);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.AvailableToPickQuantity);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.CommittedQuantity);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.ReservedQuantity);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.TotalQuantity);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.TotalValue);
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.Currency);
			if (AttributeManager != null)
			{
				if (AttributeManager.IsPartAttributeUsedByOrganisation(1))
				{
					result.Add((int)WebTracker.Grids.TrackingInventoryDetails.PartAttribute1);
				}
				if (AttributeManager.IsPartAttributeUsedByOrganisation(2))
				{
					result.Add((int)WebTracker.Grids.TrackingInventoryDetails.PartAttribute2);
				}
				if (AttributeManager.IsPartAttributeUsedByOrganisation(3))
				{
					result.Add((int)WebTracker.Grids.TrackingInventoryDetails.PartAttribute3);
				}
				if (AttributeManager.IsExpiryDateUsedByOrganisation)
				{
					result.Add((int)WebTracker.Grids.TrackingInventoryDetails.ExpiryDate);
				}
				if (AttributeManager.IsPackingDateUsedByOrganisation)
				{
					result.Add((int)WebTracker.Grids.TrackingInventoryDetails.PackingDate);
				}
				if (AttributeManager.IsSerialNumberUsedByOrganisation)
				{
					result.Add((int)WebTracker.Grids.TrackingInventoryDetails.SerialNumber);
				}
			}
			result.Add((int)WebTracker.Grids.TrackingInventoryDetails.LastCost);
			return result;
		}
	}
}
