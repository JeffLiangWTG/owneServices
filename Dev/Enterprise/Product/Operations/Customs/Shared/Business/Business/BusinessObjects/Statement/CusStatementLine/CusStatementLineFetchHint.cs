using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusStatementLineFetchHint : EnterpriseBusinessObjectFetchStrategy
	{
		public CusStatementLineFetchHint(BaseCusStatementLine statementLine)
			: base(statementLine)
		{
		}

		new BaseCusStatementLine BusinessObject
		{
			get { return (BaseCusStatementLine)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(CusStatementHeaderSchema.Constants.TableName, BusinessObject.B3_B2);
		}
	}
}
