using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseCusGoodsCatalog), "CusGoodsCatalogProductionInfos")]
	public class BaseCusGoodsCatalogProductionInfo : AutoCusGoodsCatalogProductionInfo
	{
		public BaseCusGoodsCatalogProductionInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new CusGoodsCatalogProductionInfoTypeDecider();

		[RelatedBusinessObject(nameof(GoodsCatalog))]
		public override ZGuid CGI_CGC_Catalog { get => base.CGI_CGC_Catalog; set => base.CGI_CGC_Catalog = value; }

		public BaseCusGoodsCatalog GoodsCatalog => Factory.Load<BaseCusGoodsCatalog>(CGI_CGC_Catalog);

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning() => new string[] { Schema.CGI_CGC_Catalog, Schema.CGI_CustomsStatus, Schema.CGI_MessageStatus };
	}
}
