using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ACASConsolReportSendingProviderTest : DocDataObjectReportSendingProviderTest
	{
		protected override ZString ExpectedMessageSenderFullName => "Enterprise.Freight.Forwarding.Documents.DataObjects.ACASHouseChecklistMessageSender";
		protected override ZString ExpectedModuleIdentifier => ModuleIDs.JobConsol.Name;

		protected override DocDataObjectReportSendingProvider GetSendingProvider()
		{
			return new ACASConsolReportSendingProvider(Factory);
		}

		protected override BusinessObject GetValidBusinessObjectForSending()
		{
			var testHelper = new SendAirCargoReprotMessageTestHelper(Factory);
			return testHelper.CreateConsolWithoutErrors("08135025185", "C0000011", "AUSYD", "USCHI");
		}

		protected override void SetUp()
		{
			base.SetUp();

			var proxy = GlbBranch.CurrentBranch.Factory.New<OrgHeader>();
			proxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = proxy.PK;
		}
	}
}
