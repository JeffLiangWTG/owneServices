using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusSealTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var parentTableCode = row[CusSeal.Schema.BK_ParentTableCode].ToString();
			var parentId = new ZGuid(row[CusSeal.Schema.BK_ParentID]);
			Type bizOType = null;

			if (parentId.IsValid)
			{
				var parentObj = factory.Load(parentTableCode, parentId);
				var supporter = parentObj as ICusSealTypeSupporter;
				if (supporter != null)
				{
					bizOType = supporter.CusSealType;
				}

				if (bizOType == null && parentObj != null)
				{
					var parentName = ((ICusSealTypeSupporter)parentObj).CusSealType.FullName;
					ErrorReporter.ReportOnce($"{parentName} does not support creation of CusSeal", $"{parentName} has not implement Enterprise.Customs.Business.ICusSealTypeSupporter");
				}
			}
			return bizOType;
		}
	}
}
