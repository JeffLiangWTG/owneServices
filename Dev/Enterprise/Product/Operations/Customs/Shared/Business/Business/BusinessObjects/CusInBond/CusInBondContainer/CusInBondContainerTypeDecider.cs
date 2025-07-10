using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusInBondContainerTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;

			var parentPk = (row != null) ? new ZGuid(row[CusInBondContainer.Schema.BC_ParentID]) : ZGuid.Empty;
			if (!parentPk.IsEmpty)
			{
				var parentTableCode = new ZString(row[CusInBondContainer.Schema.BC_ParentTableCode]);
				bizOType = GetBizOTypeFromParent(factory, parentPk, parentTableCode);
			}

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CusInBondContainer parent type is unknown", "Cannot determine the CusInBondContainer parent object");
			}

			return bizOType;
		}

		Type GetBizOTypeFromParent(BusinessObjectFactory factory, ZGuid parentPk, ZString parentTableCode)
		{
			Type bizOType = null;
			var parentObj = factory.Load(parentTableCode, parentPk);
			var supporter = parentObj as ICusInBondContainerTypeSupporter;
			if (supporter != null)
			{
				bizOType = supporter.ContainerType;
			}

			if (bizOType == null && parentObj != null)
			{
				var parentName = parentObj.GetType().FullName;
				ErrorReporter.ReportOnce(string.Format("{0} does not support creation of CusInBondContainer", parentName), string.Format("{0} has not implement Enterprise.Customs.Business.ICusInBondContainerTypeSupporter", parentName));
			}
			return bizOType;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
