using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.BR;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CCTConsolReportSendingProviderTest : DocDataObjectReportSendingProviderTest
	{
		protected override ZString ExpectedMessageSenderFullName => "Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR.CargoControlAndTransitHouseManifestMessageSender";
		protected override ZString ExpectedModuleIdentifier => ModuleIDs.JobConsol.Name;

		protected override DocDataObjectReportSendingProvider GetSendingProvider()
		{
			return new CCTConsolReportSendingProvider(Factory);
		}

		protected override BusinessObject GetValidBusinessObjectForSending()
		{
			var testHelper = new SendAirCargoReprotMessageTestHelper(Factory);
			return testHelper.CreateConsolWithoutErrors("08135025185", "C0000011", "AUSYD", "BRSAO");
		}

		protected override void AssertSendingMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";
			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "AGENT SIGNATURE";

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				base.AssertSendingMessage();
			}
		}
	}
}
