using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencySailingDefaultNumberOfDecimalsSupporterTest : AgencyAllocationItemDefaultNumberOfDecimalsSupporterTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			var jobSailing = Factory.New<JobSailing>();
			sailing = new AgencySailing(jobSailing, ZGuid.Empty);
		}

		public override BusinessObject BizObj
		{
			get
			{
				return sailing;
			}
		}

		AgencySailing sailing;
	}
}
