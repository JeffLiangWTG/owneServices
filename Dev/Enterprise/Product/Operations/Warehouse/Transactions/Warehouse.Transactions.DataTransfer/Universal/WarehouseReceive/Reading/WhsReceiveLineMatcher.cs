using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsReceiveLineMatcher : IWhsReceiveLineMatcher
	{
		public WhsReceiveLine FindExistingBizOForCustoms(WhsReceive parent, OrderLine lineDataObject, Product product, IEnumerable<WhsReceiveLine> matchedLines)
		{
			var bondedKeyFromDataObject = (lineDataObject.CustomsData?.GetFormattedCustomsEntryKeyWithLineNo() ?? ZString.Empty);
			var productCodeFromDataObject = product.GetCodeAsUpperCase(); // GetCodeAsUpperCase() is an extension method that works with nulls

			return parent.Lines.Except(matchedLines).Cast<WhsReceiveLine>().FirstOrDefault(
				line => line.IsInDatabase
				&& line.WE_BondedEntryKey == bondedKeyFromDataObject
				&& line.ProductCode == productCodeFromDataObject);
		}

		public WhsReceiveLine FindExistingBizOForNonCustoms(WhsReceive parent, OrderLine lineDataObject, IEnumerable<WhsReceiveLine> matchedLines)
		{
			var lineNoFromDataObject = lineDataObject.LineNumber;
			var subLineNoFromDataObject = lineDataObject.SubLineNumber;
			return parent.Lines.Except(matchedLines).Cast<WhsReceiveLine>().FirstOrDefault(
				line => line.IsInDatabase
				&& line.WE_LineNo == lineNoFromDataObject
				&& line.WE_SubLineNo == subLineNoFromDataObject);
		}

		public WhsReceiveLine FindExistingBizOForForwardingShipment(WhsReceive parent, string allocationKey, Product product, IEnumerable<WhsReceiveLine> matchedLines)
		{
			var productCodeFromDataObject = product.GetCodeAsUpperCase(); // GetCodeAsUpperCase() is an extension method that works with nulls

			return parent.Lines.Except(matchedLines).Cast<WhsReceiveLine>().FirstOrDefault(
				line => line.IsInDatabase
				&& line.WE_AllocationKey.EqualsIgnoringCase(allocationKey)
				&& line.ProductCode == productCodeFromDataObject);
		}
	}

	public interface IWhsReceiveLineMatcher
	{
		WhsReceiveLine FindExistingBizOForCustoms(WhsReceive parent, OrderLine lineDataObject, Product product, IEnumerable<WhsReceiveLine> matchedLines);
		WhsReceiveLine FindExistingBizOForNonCustoms(WhsReceive parent, OrderLine lineDataObject, IEnumerable<WhsReceiveLine> matchedLines);
		WhsReceiveLine FindExistingBizOForForwardingShipment(WhsReceive parent, string allocationKey, Product product, IEnumerable<WhsReceiveLine> matchedLines);
	}
}
