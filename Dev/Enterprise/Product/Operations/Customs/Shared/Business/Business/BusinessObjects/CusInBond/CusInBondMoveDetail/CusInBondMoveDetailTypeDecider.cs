using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusInBondMoveDetailTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;

			ZGuid billPK = (row != null) ? new ZGuid(row[CusInBondMoveDetail.Schema.B9_B0]) : ZGuid.Empty;
			if (!billPK.IsEmpty)
			{
				var bill = factory.Load<CusInBondBill>(billPK);
				var header = bill == null ? null : bill.Header;
				if (header != null)
				{
					var moveHeader = header.MovementHeader;
					if (moveHeader != null)
					{
						bizOType = moveHeader.MovementDetailType;
					}
				}
			}

			if (bizOType == null)
			{
				ZGuid moveHeaderPK = (row != null) ? new ZGuid(row[CusInBondMoveDetail.Schema.B9_BM]) : ZGuid.Empty;
				if (!moveHeaderPK.IsEmpty)
				{
					var moveHeader = factory.Load<BaseCusInBondMoveHeader>(moveHeaderPK);
					if (moveHeader != null)
					{
						bizOType = moveHeader.MovementDetailType;
					}
				}
			}

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CusInBondMoveDetail type is unknown", "Cannot determine the CusInBondMoveDetail object, because Application Code is unknown");
			}

			return bizOType;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
