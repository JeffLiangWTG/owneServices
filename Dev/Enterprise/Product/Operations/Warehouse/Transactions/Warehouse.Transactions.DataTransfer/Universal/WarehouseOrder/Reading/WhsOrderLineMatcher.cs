using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderLineMatcher : IWhsOrderLineMatcher
	{
		public WhsOrderLine FindExistingBizOByOrderLineNo(WhsOrder parent, OrderLine lineDataObject, Product product, IEnumerable<WhsOrderLine> matchedLines)
		{
			var orderLineNoFromDataObject = lineDataObject.LineNumber;
			var productCodeFromDataObject = product.GetCodeAsUpperCase(); // GetCodeAsUpperCase() is an extension method that works with nulls

			WhsOrderLine result = null;
			if (orderLineNoFromDataObject != null)
			{
				result = parent.Lines.Except(matchedLines).Cast<WhsOrderLine>().FirstOrDefault(
				line => line.WE_LineNo == orderLineNoFromDataObject
				&& line.ProductCode == productCodeFromDataObject);
			}

			return result;
		}
	}

	public interface IWhsOrderLineMatcher
	{
		WhsOrderLine FindExistingBizOByOrderLineNo(WhsOrder parent, OrderLine lineDataObject, Product product, IEnumerable<WhsOrderLine> matchedLines);
	}
}
