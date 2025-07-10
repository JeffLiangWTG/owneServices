using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class MailBoxCredentialCollection : DependentBusinessObjectCollection<GlbExternalPassword, GlbCompany>
	{
		public MailBoxCredentialCollection(CusInBondHeader cusInBondHeader, GlbCompany company)
			: base(company)
		{
			this.cusInBondHeader = cusInBondHeader;
		}

		readonly CusInBondHeader cusInBondHeader;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (cusInBondHeader.CusAgent != null)
			{
				result.AddToFilter(GlbExternalPasswordSchema.GP_GS, SQLComparisonOperator.Equal, cusInBondHeader.CusAgent.PK);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			var passwordTypeFilter = new ZQuery();
			passwordTypeFilter.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, SQLComparisonOperator.Equal, PasswordTypesList.Codes.TVA);
			passwordTypeFilter.AddToFilter(JoinCondition.Or, GlbExternalPasswordSchema.GP_PasswordType, SQLComparisonOperator.Equal, PasswordTypesList.Codes.UVC);
			result.AddToFilter(passwordTypeFilter, JoinCondition.And);
			return result;
		}

		public override void Add(BusinessObject businessObject)
		{
			var credential = businessObject as GlbExternalPassword;
			if (cusInBondHeader.CusAgent != null)
			{
				credential.GP_GS = cusInBondHeader.CusAgent.PK;
			}
			base.Add(businessObject);
		}
	}
}
