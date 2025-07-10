using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusSupportingInfoTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(new ZString(row[CusSupportingInfoSchema.CSI_Type.Name]),
				new ZString(row[CusSupportingInfoSchema.CSI_SubType.Name]),
				new ZString(row[CusSupportingInfoSchema.CSI_Code.Name]),
				new ZString(row[CusSupportingInfoSchema.CSI_ParentTableCode.Name]),
				new ZGuid(row[CusSupportingInfoSchema.CSI_ParentID.Name]),
				factory) : null;
		}

		public Type GetTypeForLoad(IColumnIndexer row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(row.GetValue(CusSupportingInfoSchema.CSI_Type),
				row.GetValue(CusSupportingInfoSchema.CSI_SubType),
				row.GetValue(CusSupportingInfoSchema.CSI_Code),
				row.GetValue(CusSupportingInfoSchema.CSI_ParentTableCode),
				row.GetValue(CusSupportingInfoSchema.CSI_ParentID),
				factory) : null;
		}

		public Type GetTypeForLoad(ZString type, ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory)
		{
			return GetTypeForLoad(type, null, null, parentTableCode, parentPK, factory);
		}

		public Type GetTypeForLoad(ZString type, ZString subType, ZString code, ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory)
		{
			Type result = null;

			if (!type.IsEmpty)
			{
				var parentObj = factory.Load(parentTableCode, parentPK);
				(parentObj as Integration.Customs.ICusSupportingInfoTypeSupporter)?.GetCusSupportingInfoTypes()?.TryGetValue(type, out result);
				if (result != null)
				{
					var typeDecider = GetTypeDeciderFromType(result);
					result =
						(typeDecider as ICusSupportingInfoTypeSubTypeSupporter)?.GetTypeBySubType(subType)
						?? (typeDecider as ApplicationSpecificTypeDecider)?.GetTypeForApplicationCode(code)
						?? result;
				}
#if DEBUG
				if (result == null && parentObj != null)
				{
					string parentName = parentObj.GetType().FullName;
					ErrorReporter.ReportOnce(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} does not support CSI_Type '{1}'", parentName, type), string.Format(System.Globalization.CultureInfo.InvariantCulture, "Either {0} has not implement Enterprise.Integration.Customs.ICusSupportingInfoTypeSupporter or is missing a support for CSI_Type '{1}'", parentName, type));
				}
#endif
			}
			return result;
		}
	}
}
