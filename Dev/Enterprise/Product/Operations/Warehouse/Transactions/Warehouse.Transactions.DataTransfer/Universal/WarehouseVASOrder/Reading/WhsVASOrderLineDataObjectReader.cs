using CargoWise.Common;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsVASOrderLineDataObjectReader : DataObjectReader<OrderLine, WhsVASOrderLine>
	{
		public WhsVASOrderLineDataObjectReader(OrderLine orderLineDO, OrgHeader client, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(orderLineDO, logger, factory)
		{
			Client = Argument.NotNull(client, "client");
		}
		readonly OrgHeader Client;

		#region GetExistingBusinessObject

		protected override WhsVASOrderLine GetExistingBusinessObject()
		{
			return null;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsVASOrderLine targetBO)
		{
			var product = WhsProductLoader.GetMatchedProductOrMaybeCreateNew(Res.GetString("6f49d585-9c01-46d2-bc11-f36c0a867800", "VAS Order"), dataObject.LineNumber, dataObject.Product.GetCodeAsUpperCase(), dataObject, Client, false, factory, logger);
			var productRow = GetColumnIndexerFromRow(product);
			var vasOrderLineRow = GetColumnIndexerFromRow(targetBO);

			if (product != null)
			{
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_LineNumber, dataObject.LineNumber);
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_OP_Product, productRow.GetValue(OrgSupplierPartSchema.PK));
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_Quantity, CalculateQuantity(product, dataObject.OrderedQty, dataObject.PackageQty, dataObject.PackageQtyUnit, dataObject.LineNumber));
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_PackingDate, dataObject.PackingDate);
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_ExpiryDate, dataObject.ExpiryDate);
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_PartAttrib1, dataObject.PartAttribute1);
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_PartAttrib2, dataObject.PartAttribute2);
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_PartAttrib3, dataObject.PartAttribute3);
				SetValue(vasOrderLineRow, WhsVASOrderLineSchema.WVL_SerialNumber, dataObject.SerialNumber);
			}
			else
			{
				throw new DataObjectReadFailureException(WhsProductLoader.GetUnableToMatchProductMessage(dataObject));
			}
		}

		ZDecimal CalculateQuantity(OrgSupplierPart supplierPart, ZDecimal? orderedQty, ZDecimal? packageQty, PackageType packageQtyUnit, ZInt? lineNumber)
		{
			ZDecimal qty = 0m;

			if (orderedQty.HasValue && orderedQty.Value > 0m)
			{
				qty = orderedQty.Value;
			}
			else if (packageQty.HasValue && packageQty.Value > 0m)
			{
				var packType = packageQtyUnit.Code.Value;
				qty = Utilities.Round(supplierPart.UnitConverter.Convert(packageQty.Value, packType, supplierPart.OP_StockKeepingUnit), supplierPart.OP_CountDecimalPlaces);

				if (qty <= 0m)
				{
					throw new DataObjectReadFailureException(Res.GetString("b968afd6-8eac-480b-b0fc-81dda6be54be", "Unit conversion for package type '{0}' does not exist. Line: {1}.", packageQtyUnit.Code, lineNumber));
				}
			}

			if (qty <= 0m)
			{
				logger.Log(
					LogType.Warning,
					Res.GetString("1f7bdfd7-26ee-4bae-ba1d-fa7ff2dd4922", "Order Quantity and Package Quantity are not valid (both have value 0). Line: {0}.", lineNumber));
			}

			return qty;
		}

		#endregion
	}
}
