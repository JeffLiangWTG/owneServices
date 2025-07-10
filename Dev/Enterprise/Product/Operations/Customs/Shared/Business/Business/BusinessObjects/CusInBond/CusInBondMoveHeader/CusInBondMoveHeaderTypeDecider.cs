using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusInBondMoveHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;

			ZGuid headerPK = (row != null) ? new ZGuid(row[CusInBondMoveHeader.Schema.BM_BH]) : ZGuid.Empty;
			if (!headerPK.IsEmpty)
			{
				var header = factory.Load<Integration.Customs.ICusInBondHeader>(headerPK);
				if (header != null)
				{
					bizOType = header.MovementHeaderType;
				}
			}

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CusInBondMoveHeader type is unknown", "Cannot determine the CusInBondMoveHeader object, because Application Code is unknown");
			}

			return bizOType;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
