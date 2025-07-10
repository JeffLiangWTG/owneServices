using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class FreightConsolWrapperTest : TestCaseWithFactory
	{
		public void TestReportCarrier()
		{
			if (ImportExportHelper.IsBranchCountry(consolWrapper.Consol.JK_RL_NKLoadPort))
			{
				Assert("ReportingCarrier", consolWrapper.GetReportingCarrier() == GlbCompany.CurrentCompany.OrgProxy.OH_FullName.Left(35));
			}
			else
			{
				Assert("ReportingCarriaer", consolWrapper.GetReportingCarrier() == (consolWrapper.Consol.SendingForwarder == null ? ZString.Empty : consolWrapper.Consol.SendingForwarder.OH_FullName));
			}
		}

		#region Implementation

		protected FreightConsolWrapper consolWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consolWrapper = NewFreightConsolWrapper(consol);
		}

		protected abstract FreightConsolWrapper NewFreightConsolWrapper(ForwardingConsol consol);

		#endregion
	}
}
