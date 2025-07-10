using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsHoldOrderLineDataObjectReader : DataObjectReader<OrderLine, WhsHoldOrderLine>
	{
		public WhsHoldOrderLineDataObjectReader(OrderLine orderLineDO, OrgHeader client, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(orderLineDO, logger, factory)
		{
			Client = Argument.NotNull(client, "client");
		}

		readonly OrgHeader Client;

		#region GetNewBusinessObject

		protected override WhsHoldOrderLine GetNewBusinessObject()
		{
			return new WhsHoldOrderLine(factory.BOFactory);
		}

		#endregion

		#region GetExistingBusinessObject

		protected override WhsHoldOrderLine GetExistingBusinessObject()
		{
			return null;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsHoldOrderLine holdOrderLine)
		{
			var product = WhsProductLoader.GetMatchedProductOrMaybeCreateNew(Res.GetString("fbbaac7a-4897-4019-87d3-c7c466debaa1", "Hold Order"), dataObject.LineNumber, dataObject.Product.GetCodeAsUpperCase(), dataObject, Client, false, factory, logger);
			var toHoldCode = dataObject.CurrentHoldCode.GetCodeAsUpperCase();
			var holdReason = dataObject.CurrentHoldReason.GetValueOrDefault();

			if (product == null)
			{
				throw new DataObjectReadFailureException(WhsProductLoader.GetUnableToMatchProductMessage(dataObject));
			}
			else if (!toHoldCode.IsEmpty && factory.LoadFromNaturalKey<WhsInventoryHeldCode>(WhsInventoryHeldCodeSchema.WHC_Code, toHoldCode) == null)
			{
				throw new DataObjectReadFailureException(string.Format(CultureInfo.InvariantCulture, "Invalid Hold Code: {0}", dataObject.CurrentHoldCode.ToStringContents()));
			}
			else if (!holdReason.IsEmpty && toHoldCode.IsEmpty)
			{
				throw new DataObjectReadFailureException("Cannot import Hold Reason without a Hold Code.");
			}
			else
			{
				var productRow = GetColumnIndexerFromRow(product);
				// Not using SetValue as WhsHoldOrder is non persistent and not backed by SchemaColumns
				holdOrderLine.ProductPK = productRow.GetValue(OrgSupplierPartSchema.PK);
				holdOrderLine.FromHoldCode = dataObject.OriginalHoldCode.GetCodeAsUpperCase();
				holdOrderLine.ToHoldCode = toHoldCode;
				holdOrderLine.HoldReason = holdReason;
				holdOrderLine.Quantity = CalculateQuantity(product, dataObject.OrderedQty, dataObject.PackageQty, dataObject.PackageQtyUnit, dataObject.LineNumber);
				holdOrderLine.PackingDate = dataObject.PackingDate.GetValueOrDefault().Date;
				holdOrderLine.ExpiryDate = dataObject.ExpiryDate.GetValueOrDefault().Date;
				holdOrderLine.PartAttrib1 = dataObject.PartAttribute1.GetValueOrDefault();
				holdOrderLine.PartAttrib2 = dataObject.PartAttribute2.GetValueOrDefault();
				holdOrderLine.PartAttrib3 = dataObject.PartAttribute3.GetValueOrDefault();
				holdOrderLine.SerialNumber = dataObject.SerialNumber.GetValueOrDefault();
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

				if (qty == 0m)
				{
					throw new DataObjectReadFailureException(Res.GetString("99bfee3e-c5c4-4f2b-a253-9bc7626b0894", "Unit conversion for package type '{0}' does not exist. Line: {1}.", packageQtyUnit.Code, lineNumber));
				}
			}
			else if ((orderedQty.HasValue && orderedQty.Value < 0m) || (packageQty.HasValue && packageQty.Value < 0m))
			{
				throw new DataObjectReadFailureException(Res.GetString("2a3adb97-74fe-48f8-b287-80ed3563096e", "Order Quantity or Package Quantity is not valid. Line: {0}.", lineNumber));
			}

			if (qty == 0m)
			{
				logger.Log(
					Enterprise.Integration.LogType.Warning,
					Res.GetString("70253d17-e7ad-4ee1-9273-773f0646fb31", "Order Quantity and Package Quantity are not valid (both have value 0). Line: {0}.", lineNumber));
			}

			return qty;
		}

		#endregion
	}
}
