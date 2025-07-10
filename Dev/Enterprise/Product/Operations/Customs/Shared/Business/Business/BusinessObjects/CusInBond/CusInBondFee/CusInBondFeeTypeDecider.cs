using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusInBondFeeTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;

			ZGuid parentPk = (row != null) ? new ZGuid(row[CusInBondFee.Schema.BFE_BY]) : ZGuid.Empty;
			if (!parentPk.IsEmpty)
			{
				var supporter = factory.Load(typeof(CusInBondCargoDesc), parentPk) as ICusInBondFeeTypeSupporter;
				if (supporter != null)
				{
					bizOType = supporter.FeeType;
				}
			}

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CusInBondFee parent type is unknown", "Cannot determine the CusInBondFee parent object");
			}

			return bizOType;
		}

		public override Type GetTypeForNew() => null;
	}
}
