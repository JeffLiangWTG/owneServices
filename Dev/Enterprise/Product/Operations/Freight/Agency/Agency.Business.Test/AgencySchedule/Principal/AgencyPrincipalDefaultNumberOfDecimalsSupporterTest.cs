using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyPrincipalDefaultNumberOfDecimalsSupporterTest : AgencyAllocationItemDefaultNumberOfDecimalsSupporterTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			principal = new AgencyPrincipal(Factory);
		}

		public override BusinessObject BizObj
		{
			get
			{
				return principal;
			}
		}

		AgencyPrincipal principal;
	}
}
