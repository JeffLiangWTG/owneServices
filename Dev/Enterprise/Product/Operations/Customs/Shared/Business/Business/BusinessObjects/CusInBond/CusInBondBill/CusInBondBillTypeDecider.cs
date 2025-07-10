using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IBillTypeProvider
	{
		Type BillType { get; }
	}

	public class CusInBondBillTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;

			ZGuid headerPK = (row != null) ? new ZGuid(row[CusInBondBill.Schema.B0_BH]) : ZGuid.Empty;
			if (!headerPK.IsEmpty)
			{
				var header = factory.Load(typeof(CusInBondHeader), headerPK) as IBillTypeProvider;
				if (header != null)
				{
					bizOType = header.BillType;
				}
			}

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CusInBondBill type is unknown", "Cannot determine the CusInBondBill object, because Application Code is unknown");
			}

			return bizOType;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
