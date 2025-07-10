using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusGoodsCatalogProductionInfoTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(BaseCusGoodsCatalogProductionInfo);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var cusGoodsCatalogPK = new ZGuid(row[BaseCusGoodsCatalogProductionInfo.Schema.CGI_CGC_Catalog]);
				var cusGoodsCatalog = cusGoodsCatalogPK.IsValid ? factory.Load<BaseCusGoodsCatalog>(cusGoodsCatalogPK) : null;
				if (cusGoodsCatalog != null)
				{
					result = cusGoodsCatalog.GetProductionInfoType(new ZString(row[BaseCusGoodsCatalogProductionInfo.Schema.CGI_Type]));
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForNew()
		{
			return typeof(BaseCusGoodsCatalogProductionInfo);
		}
	}
}
