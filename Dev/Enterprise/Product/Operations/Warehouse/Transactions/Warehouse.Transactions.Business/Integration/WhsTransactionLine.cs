using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWarehouseTransactionLine : BondedWarehouseLineProblemProvider /* temporary until problems is in its own helper object */, IWhsWarehouseTransactionLine
	{
		#region Constructor

		public WhsWarehouseTransactionLine()
		{
		}

		protected WhsWarehouseTransactionLine(WhsDocketLine docketLine)
		{
			ExtractDataFromDocketLine(docketLine);
		}

		void ExtractDataFromDocketLine(WhsDocketLine docketLine)
		{
			Product = docketLine.SupplierPart;
			Quantity = (docketLine.Docket != null && docketLine.Docket.IsFinalised) ? docketLine.SumOfUnitsMet : docketLine.WE_TransactionQuantity;
			QuantityUnit = docketLine.WE_F3_NKPackType;
			PartAttrib1 = docketLine.WE_PartAttrib1;
			PartAttrib2 = docketLine.WE_PartAttrib2;
			PartAttrib3 = docketLine.WE_PartAttrib3;
			SerialNumber = docketLine.WE_SerialNumber;

			if (docketLine.Docket != null && docketLine.Docket.Warehouse != null)
			{
				Warehouse = docketLine.Docket.Warehouse.WarehouseAddress;
			}

			if (docketLine.Docket != null && docketLine.Docket.WD_DocketType == CodeLists.DocketType.Codes.Order)
			{
				EntryLineCodeParser parser = new EntryLineCodeParser(docketLine.WE_BondedEntryKey);
				EntryKey = parser.EntryNumber;
				EntryLineNumber = parser.LineNumber;
			}
			else
			{
				if (docketLine.CustomsData != null)
				{
					EntryKey = docketLine.CustomsData.WB_EntryKey;
					EntryLineNumber = docketLine.CustomsData.WB_EntryLineNo;
				}
			}
		}

		#endregion

		#region IWhsWarehouseTransactionLine Members

		public IOrgAddress Warehouse { get; set; }

		public IOrgSupplierPart Product { get; set; }

		public ZDecimal Quantity { get; set; }

		public ZString QuantityUnit { get; set; }

		public ZString PartAttrib1 { get; set; }

		public ZString PartAttrib2 { get; set; }

		public ZString PartAttrib3 { get; set; }

		public ZString SerialNumber { get; set; }

		public ZString EntryKey { get; set; }

		public ZShort EntryLineNumber { get; set; }

		#endregion
	}
}
