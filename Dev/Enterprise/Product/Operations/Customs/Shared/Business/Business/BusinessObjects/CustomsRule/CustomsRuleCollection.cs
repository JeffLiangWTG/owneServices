using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CustomsRuleCollection : ActiveBusinessObjectCollection<CustomsRule>
	{
		public CustomsRuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Rule);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, CustomsRule.CustomsRuleType);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return query;
		}
	}
}
