using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseCusStorageDocPivotTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;

			if (row != null)
			{
				var parent = factory.Load(row[BaseCusStorageDocPivot.Schema.CSD_ParentTableCode].ToString(), new ZGuid(row[BaseCusStorageDocPivot.Schema.CSD_ParentID]));
				if (parent != null && parent is ICusStorageDocPivotTypeSupporter iSupporter)
				{
					result = iSupporter.CusStorageDocPivotType;
				}
			}
			return result;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
