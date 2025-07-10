using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusGoodsLocationTypeDecider : TypeDecider
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
			Type type = null;
			var message = string.Empty;
			if (row != null)
			{
				var parentTableCode = new ZString(row[AutoCusGoodsLocation.Schema.CGL_ParentTableCode]);
				var parentID = new ZGuid(row[AutoCusGoodsLocation.Schema.CGL_ParentID]);
				var parentObj = factory.Load(parentTableCode, parentID);
				if (parentObj is ICusGoodsLocationTypeSupporter supporter)
				{
					type = supporter.GoodsLocationType;
				}

				if (type == null && parentObj != null)
				{
					message = string.Format((NoResString)"{0} has not implemented Enterprise.Customs.Business.ICusGoodsLocationTypeSupporter", parentObj.GetType().FullName);
				}
			}

			if (type == null)
			{
				ErrorReporter.ReportOnce("Cannot get correct type of CusGoodsLocation", string.IsNullOrEmpty(message) ? (NoResString)"Cannot determine the CusGoodsLocation parent object" : message);
			}
			return type;
		}
	}
}

