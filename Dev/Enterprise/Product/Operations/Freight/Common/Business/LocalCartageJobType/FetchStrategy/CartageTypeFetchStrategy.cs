
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Business
{
	public class CartageTypeFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CartageTypeFetchStrategy(CommonCartageType cartageType)
			: base(cartageType)
		{
		}

		CommonCartageType CartageType
		{
			get { return (CommonCartageType)BusinessObject; }
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(LocalCartageJobLegTypeSchema.E4_E3, CartageType.PK);
			Factory.AddFetchHint(LocalCartageJobOrgSchema.E5_E3, CartageType.PK);
		}
	}
}
