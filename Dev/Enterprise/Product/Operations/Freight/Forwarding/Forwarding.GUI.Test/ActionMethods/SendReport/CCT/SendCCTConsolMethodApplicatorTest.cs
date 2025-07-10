using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SendCCTConsolMethodApplicator))]
	public class SendCCTConsolMethodApplicatorTest : DocDataObjectSendingMessageMethodApplicatorTest
	{
		public void TestSkip()
		{
			var consolWithoutErrors = CreateConsolWithoutErrors("215-98757411", "C00000001", "AUSYD", "BRSAO", "AdvancedCargoReportBR", Core.Constants.CountryCodes.Brazil);
			var consolWithFilterErrors = Factory.New<ForwardingConsol>();
			PopulateConsol(consolWithFilterErrors, "215-98757412", "AUSYD", "GBLON", "C00000002");
			var consolWithSendErrors = Factory.New<ForwardingConsol>();
			PopulateConsol(consolWithSendErrors, "081001", "AUSYD", "BRSAO", "C00000003");
			var shipmentWithoutErrors = CreateShipmentWithoutErrors("081002", "S00000001", "C00001001", "AUSYD", "BRSAO");

			Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Skip;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";
			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			using (Factory.AddDisposableService())
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var consolModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobConsol))
			{
				var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(ModuleIDs.JobConsol, consolModule.SecurityCheckpoint);
				var menuCheckpoint = Env.Security.FindOrCreateVisualizerFormCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobConsol, parentCheckpoint);
				menuCheckpoint.IsAllowed = true;
				Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint).IsAllowed = true;

				var expectedLog =
					"INFO: [HL C00000001] processed successfully.\n" +
					"WARNING: [HL C00000002] The consol is not arriving in Brazil.\n" +
					"WARNING: [HL C00000003] " +
					"Message Error - Value: Weight is required in CCT House Manifest\n" +
					"Message Error - Mawb: The MAWB should contain 11 digits.\n" +
					"Message Error - Packs: Packs number is required in CCT House Manifest\n" +
					"Message Error - ErrorPlaceHolder: Shipment Data is required to send CCT House Manifest.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithFilterErrors, consolWithSendErrors }, expectedLog);

				var expectedReSendLog =
					"WARNING: [HL C00000001] A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedReSendLog);

				var expectedNotValidLog =
					"WARNING:  Can not load Consol correctly.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors }, expectedNotValidLog);
			}
		}

		public void TestAbort()
		{
			var consolWithoutErrors = CreateConsolWithoutErrors("215-98757411", "C00000001", "AUSYD", "BRSAO", "AdvancedCargoReportBR", Core.Constants.CountryCodes.Brazil);
			var consolWithFilterErrors = Factory.New<ForwardingConsol>();
			PopulateConsol(consolWithFilterErrors, "215-98757412", "AUSYD", "GBLON", "C00000002");
			var consolWithSendErrors = Factory.New<ForwardingConsol>();
			PopulateConsol(consolWithSendErrors, "081001", "AUSYD", "BRSAO", "C00000003");
			var shipmentWithoutErrors = CreateShipmentWithoutErrors("081002", "S00000001", "C00001001", "AUSYD", "BRSAO");

			Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Abort;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";
			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			using (Factory.AddDisposableService())
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var consolModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobConsol))
			{
				var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(ModuleIDs.JobConsol, consolModule.SecurityCheckpoint);
				var menuCheckpoint = Env.Security.FindOrCreateVisualizerFormCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobConsol, parentCheckpoint);
				menuCheckpoint.IsAllowed = true;
				Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint).IsAllowed = true;

				var expectedLog =
					"INFO: [HL C00000001] processed successfully.\n" +
					"ERROR: [HL C00000002] The consol is not arriving in Brazil.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithFilterErrors, consolWithSendErrors }, expectedLog);

				var expectedReSendLog =
					"ERROR: [HL C00000001] A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedReSendLog);

				var expectedNotValidLog =
					"ERROR:  Can not load Consol correctly.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors }, expectedNotValidLog);
			}
		}

		#region Implementation

		protected override string NoSelectedError => "ERROR: No consols selected.";

		protected override string MessageSenderContext => "CargoControlAndTransitHouseManifestMessageSender";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SendCCTConsolMethodApplicator(Settings, Factory);
		}

		protected override DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider()
		{
			return new CCTConsolReportSendingProvider(Factory);
		}

		protected IStmMenuItem MenuItem => menuItem ?? (menuItem = Factory.Load<VisualizerMenuItem>(ConsolSystemFormMenuItems.CCTHouseManifestPK));
		IStmMenuItem menuItem;

		#endregion
	}
}
