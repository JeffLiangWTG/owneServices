using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusInBondCargoDescTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(new ZString(row[CusInBondCargoDescSchema.BY_ParentTableCode.Name]), new ZGuid(row[CusInBondCargoDescSchema.BY_ParentID.Name]), factory) : null;
		}

		public Type GetTypeForLoad(IColumnIndexer row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(row.GetValue(CusInBondCargoDescSchema.BY_ParentTableCode), row.GetValue(CusInBondCargoDescSchema.BY_ParentID), factory) : null;
		}

		public Type GetTypeForLoad(ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory)
		{
			Type result = null;
			BusinessObject parentObj = null;
			if (!parentPK.IsEmpty && !parentTableCode.IsEmpty)
			{
				parentObj = factory.Load(parentTableCode, parentPK);
				result = (parentObj as ICusInBondCargoDescTypeProvider)?.CusInBondCargoDescType;
			}

			if (result == null)
			{
				if (parentObj == null)
				{
					ErrorReporter.ReportOnce("CusInBondCargoDesc type is unknown", FormattableString.Invariant($"Cannot determine the CusInBondCargoDesc object, because parent could not be determined (TableCode: {parentTableCode})"));
				}
				else
				{
					string parentName = parentObj.GetType().FullName;
					ErrorReporter.ReportOnce("CusInBondCargoDesc type is unknown for " + parentName, FormattableString.Invariant($"Cannot determine the CusInBondCargoDesc object, because {parentName} has not implement {typeof(ICusInBondCargoDescTypeProvider).FullName}"));
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
