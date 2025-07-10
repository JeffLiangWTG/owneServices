using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusInvPackTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(new ZString(row[CusInvPack.Schema.B5_ParentTableCode]), new ZGuid(row[CusInvPack.Schema.B5_ParentID]), row, factory) : null;
		}

		public Type GetTypeForLoad(ZString parentTableCode, ZGuid parentPK, DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			BusinessObject parentObj = null;
			if (!parentPK.IsEmpty && !parentTableCode.IsEmpty)
			{
				parentObj = factory.Load(parentTableCode, parentPK);
				var cusInvPackTypeSupporter = parentObj as ICusInvPackTypeSupporter;
				result = cusInvPackTypeSupporter?.PackType;
				if (result == null)
				{
					var packTypeDecider = cusInvPackTypeSupporter?.PackTypeDecider;
					result = packTypeDecider?.GetTypeForLoad(row, factory);
				}
			}

			if (result == null)
			{
				if (parentObj == null)
				{
					ErrorReporter.ReportOnce("CusInvPack type is unknown", FormattableString.Invariant($"Cannot determine the CusInvPack object, because parent could not be determined (TableCode: {parentTableCode})"));
				}
				else
				{
					string parentName = parentObj.GetType().FullName;
					ErrorReporter.ReportOnce("CusInvPack type is unknown for " + parentName, FormattableString.Invariant($"Cannot determine the CusInvPack object, because {parentName} has not implement {typeof(ICusInvPackTypeSupporter).FullName}"));
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
