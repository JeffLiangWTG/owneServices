using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusReferenceTypeDecider : TypeDecider
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
			return row != null ? GetTypeForLoad(new ZString(row[CusReferenceSchema.CFR_Type.Name]),
				new ZString(row[CusReferenceSchema.CFR_ParentTableCode.Name]),
				new ZGuid(row[CusReferenceSchema.CFR_ParentID.Name]),
				factory) : null;
		}

		public Type GetTypeForLoad(IColumnIndexer row, BusinessObjectFactory factory)
		{
			return row != null ? GetTypeForLoad(row.GetValue(CusReferenceSchema.CFR_Type),
				row.GetValue(CusReferenceSchema.CFR_ParentTableCode),
				row.GetValue(CusReferenceSchema.CFR_ParentID),
				factory) : null;
		}

		public Type GetTypeForLoad(ZString typeCode, ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory)
		{
			Type result = null;

			if (!typeCode.IsEmpty)
			{
				result = GetTypeForLoad(typeCode);

				if (result == null)
				{
					var parentObj = factory.Load(parentTableCode, parentPK);
					(parentObj as ICusReferenceTypeSupporter)?.GetCusReferenceTypes()?.TryGetValue(typeCode, out result);

					if (result == null)
					{
						if (parentObj == null)
						{
							ErrorReporter.ReportOnce("CusReference type is unknown", $"Cannot determine the CusReferenceTypeSupporter object, because parent could not be determined (Type: {typeCode}, TableCode: {parentTableCode})");
						}
						else
						{
							var parentName = parentObj.GetType().FullName;
							ErrorReporter.ReportOnce($"CusReference type is unknown for {parentName}", $"Either {parentName} has not implement Enterprise.Customs.Business.ICusReferenceTypeSupporter or is missing a support for Type '{typeCode}'");
						}
					}
				}
			}

			return result ?? typeof(CusReference);
		}

		Type GetTypeForLoad(string typeCode)
		{
			switch (typeCode?.ToUpperInvariant())
			{
				case CusReferenceTypeList.Codes.NctsAuthorization:
					return ObjectFactory.GetType<Integration.Customs.IT.INctsAuthorization>();
				case CusReferenceTypeList.Codes.ComprehensiveValuations:
					return ObjectFactory.GetType<Integration.Customs.JP.IComprehensiveValuation>();
				case CusReferenceTypeList.Codes.OtherLawReference:
					return ObjectFactory.GetType<Integration.Customs.JP.IOtherLawReference>();
				case CusReferenceTypeList.Codes.GuaranteeReference:
					return ObjectFactory.GetType<Integration.Customs.JP.IGuaranteeReference>();
				default:
					return null;
			}
		}
	}
}
