using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	public class CusAddInfoTypeDecider : TypeDeciderWithUnknown<CusAddInfo, UnknownCusAddInfo>
	{
		//Needed as it is abstract, but should not be hit as CusAddInfo is not exposed to users and a specific type is specified at each collection
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		protected override Type GetTypeForLoadCore(ZString type, ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory)
		{
			if (parentTableCode == WhsBondedWarehouseAttributeSchema.Constants.Prefix)
			{
				return typeof(WarehouseCustomsAttributeAddInfo);
			}

			Type result = null;
			if (!type.IsEmpty)
			{
				var parentObj = parentPK.IsEmpty || parentTableCode.IsEmpty ? null : factory.Load(parentTableCode, parentPK);
				(parentObj as ICusAddInfoTypeSupporter)?.GetCusAddInfoTypes()?.TryGetValue(type, out result);

				if (result == null && parentObj != null)
				{
					var parentType = parentObj.GetType();
					if (Globals.CanShowDialogs)
					{
						var parentName = parentType.FullName;
						ErrorReporter.ReportOnce(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CusAddInfoTypeDecider.GetTypeForLoad|ParentBO:{0}|B7_Type:{1}", parentName, type),
							string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} does not support B7_Type '{1}'. Either {0} has not implement Enterprise.Customs.Business.MultiLineAddInfos.ICusAddInfoTypeSupporter or is missing a support for B7_Type '{1}'", parentName, type));
					}
					else
					{
						var parentTypeCode = parentType.IsSubclassOf(typeof(CusAddInfo)) ? parentObj.GetValue(TypeColumn) : (ZString)parentType.ToString().Split('.').LastOrDefault();
						throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "The '{1}' node should not be nested under the '{0}' node. Please ensure that '{1}' is placed at the correct hierarchical level.", parentTypeCode, type));
					}
				}
			}

			if (result != null && result.IsSubclassOf(typeof(BaseAddInfo)))
			{
				return typeof(CusAddInfo<>).MakeGenericType(result);
			}
			else
			{
				return result;
			}
		}

		protected override SchemaGuidColumn PKColumn => CusAddInfoSchema.PK;
		protected override SchemaGuidColumn ParentIDColumn => CusAddInfoSchema.B7_ParentID;
		protected override SchemaStringColumn ParentTableCodeColumn => CusAddInfoSchema.B7_ParentTableCode;
		protected override SchemaStringColumn TypeColumn => CusAddInfoSchema.B7_Type;
	}
}
