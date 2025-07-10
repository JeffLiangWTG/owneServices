using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyOriginDefaultNumberOfDecimalsSupporterTest : AgencyAllocationItemDefaultNumberOfDecimalsSupporterTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			var voyageOrigin = Factory.New<VoyageOrigin>();
			voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			origin = new AgencyOrigin(voyageOrigin, ZGuid.Empty);
		}

		public override BusinessObject BizObj
		{
			get
			{
				return origin;
			}
		}

		AgencyOrigin origin;
	}
}
