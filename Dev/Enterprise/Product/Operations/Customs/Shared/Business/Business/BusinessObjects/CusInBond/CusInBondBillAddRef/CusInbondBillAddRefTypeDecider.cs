using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusInbondBillAddRefTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;

			var parentPk = (row != null) ? new ZGuid(row[CusInbondBillAddRef.Schema.BR_B0]) : ZGuid.Empty;
			if (!parentPk.IsEmpty)
			{
				var parentObj = factory.Load(CusInBondBillSchema.Constants.Prefix, parentPk);
				var supporter = parentObj as ICusInbondBillAddRefTypeSupporter;
				if (supporter != null)
				{
					bizOType = supporter.AddRefType;
				}

				if (bizOType == null && parentObj != null)
				{
					var parentName = parentObj.GetType().FullName;
					ErrorReporter.ReportOnce(string.Format("{0} does not support creation of CusInbondBillAddRef", parentName), string.Format("{0} has not implement Enterprise.Customs.Business.ICusInbondBillAddRefTypeSupporter", parentName));
				}
			}

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CusInbondBillAddRef parent type is unknown", "Cannot determine the CusInbondBillAddRef parent object");
			}

			return bizOType;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
