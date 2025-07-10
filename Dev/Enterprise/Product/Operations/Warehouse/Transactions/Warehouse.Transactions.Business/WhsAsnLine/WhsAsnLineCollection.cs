using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAsnLineCollection : DependentBusinessObjectCollection<WhsAsnLine, WhsReceive>
	{
		#region Constructors

		public WhsAsnLineCollection(WhsReceive master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		#endregion

		#region Methods

		public WhsAsnLine Add(WhsDocketLine docketLine)
		{
			var line = FindAsnLine(docketLine.SupplierPart, docketLine.WE_PalletID, docketLine.WE_PartAttrib1, docketLine.WE_PartAttrib2, docketLine.WE_PartAttrib3, docketLine.WE_SerialNumber, docketLine.WE_PackingDate, docketLine.WE_ExpiryDate, docketLine.WE_LineNo, docketLine.WE_SubLineNo);
			if (line == null)
			{
				line = this.AddNew();
				line.WN_OP = docketLine.WE_OP;
				line.WN_PartAttrib1 = docketLine.WE_PartAttrib1;
				line.WN_PartAttrib2 = docketLine.WE_PartAttrib2;
				line.WN_PartAttrib3 = docketLine.WE_PartAttrib3;
				line.WN_SerialNumber = docketLine.WE_SerialNumber;
				line.WN_PackingDate = docketLine.WE_PackingDate;
				line.WN_ExpiryDate = docketLine.WE_ExpiryDate;
				line.WN_PalletId = docketLine.WE_PalletID;
				line.WN_QuantityUQ = docketLine.ProductUQ;
				line.WN_LineNo = docketLine.WE_LineNo;
				line.WN_SubLineNo = docketLine.WE_SubLineNo;
			}

			line.WN_Quantity += docketLine.WE_ClientOrderedUnits;
			return line;
		}

		#endregion

		#region Parent

		public WhsReceive Parent => Master;

		#endregion

		#region Implementation

		WhsAsnLine FindAsnLine(OrgSupplierPart part, ZString palletID, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate packingDate, ZDate expiryDate, ZShort lineNo, ZShort subLineNo)
		{
			WhsAsnLine result = null;

			foreach (WhsAsnLine line in this)
			{
				if (line.WN_OP == part.PK
					&& line.WN_PartAttrib1 == partAttrib1
					&& line.WN_PartAttrib2 == partAttrib2
					&& line.WN_PartAttrib3 == partAttrib3
					&& line.WN_SerialNumber == serialNumber
					&& line.WN_PackingDate == packingDate
					&& line.WN_ExpiryDate == expiryDate
					&& line.WN_PalletId == palletID)
				{
					if (line.WN_LineNo == lineNo && line.WN_SubLineNo == subLineNo || (lineNo == 0 && subLineNo == 0))
					{
						result = line;
						break;
					}
				}
			}

			return result;
		}

		#endregion
	}
}
