using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class CusCodeDataTypeDecider : TypeDeciderWithUnknown<CusCodeData, UnknownCusCodeData>
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		protected override Type GetTypeForLoadCore(ZString typeCode, ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory)
		{
			Type result = null;

			if (!typeCode.IsEmpty)
			{
				var parentObj = factory.Load(parentTableCode, parentPK);
				(parentObj as ICusCodeDataTypeSupporter)?.GetCusCodeDataTypes()?.TryGetValue(typeCode, out result);
#if DEBUG
				if (result == null && parentObj != null)
				{
					string parentName = parentObj.GetType().FullName;
					ErrorReporter.ReportOnce(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} does not support CY_Type '{1}'", parentName, typeCode), string.Format(System.Globalization.CultureInfo.InvariantCulture, "Either {0} has not implement Enterprise.Customs.Business.MultiLineAddInfos.ICusCodeDataTypeSupporter or is missing a support for CY_Type '{1}'", parentName, typeCode));
				}
#endif
			}
			return result;
		}

		protected override SchemaGuidColumn PKColumn => CusCodeDataSchema.PK;
		protected override SchemaGuidColumn ParentIDColumn => CusCodeDataSchema.CY_ParentID;
		protected override SchemaStringColumn ParentTableCodeColumn => CusCodeDataSchema.CY_ParentTableCode;
		protected override SchemaStringColumn TypeColumn => CusCodeDataSchema.CY_Type;
	}
}
