using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsVASOrderLineDataObjectWriter : DataObjectWriter<WhsVASOrderLine, OrderLine>
	{
		internal WhsVASOrderLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		#region PopulateDataObject

		protected override OrderLine PopulateDataObject(WhsVASOrderLine vasOrderLineBO)
		{
			var product = vasOrderLineBO.Product;

			var orderLine = new OrderLine();
			orderLine.LineNumber = vasOrderLineBO.WVL_LineNumber;
			orderLine.Product = product != null ? new Product { Code = product.OP_PartNum, Description = product.OP_Desc } : null;
			orderLine.OrderedQty = vasOrderLineBO.WVL_Quantity;
			orderLine.PackingDate = vasOrderLineBO.WVL_PackingDate;
			orderLine.ExpiryDate = vasOrderLineBO.WVL_ExpiryDate;
			orderLine.PartAttribute1 = vasOrderLineBO.WVL_PartAttrib1;
			orderLine.PartAttribute2 = vasOrderLineBO.WVL_PartAttrib2;
			orderLine.PartAttribute3 = vasOrderLineBO.WVL_PartAttrib3;
			orderLine.SerialNumber = vasOrderLineBO.WVL_SerialNumber;

			return orderLine;
		}

		#endregion
	}
}
