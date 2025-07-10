using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class BillNumberWrapper : IAssociatedTransportDocument
	{
		public BillNumberWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "Shipment cannot be null");
		}
		readonly AsycudaBill bill;

		public ZString BillNumber => bill.ABL_BillNumber;

		public ZString BillType
		{
			get
			{
				if (bill.IsAir)
				{
					return NZ.TradeSingleWindow.BillTypeList.Codes.MB;
				}
				else if (bill.IsSea)
				{
					return NZ.TradeSingleWindow.BillTypeList.Codes.BM;
				}
				return ZString.Empty;
			}
		}

		public IEnumerable<ZGuid> RelatedEquipment => Enumerable.Empty<ZGuid>();

		public IEnumerable<ZGuid> RelatedPackages => Enumerable.Empty<ZGuid>();

		ZInt IAssociatedTransportDocument.MessageSequence
		{
			get { return fMessageSequence; }
			set { fMessageSequence = value; }
		}
		ZInt fMessageSequence;

		public ZGuid PK => bill.PK;
	}
}
