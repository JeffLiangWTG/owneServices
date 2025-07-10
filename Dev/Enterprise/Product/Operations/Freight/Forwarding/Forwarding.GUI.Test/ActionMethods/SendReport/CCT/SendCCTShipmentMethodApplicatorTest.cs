using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
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
	[TestedType(typeof(SendCCTShipmentMethodApplicator))]
	public class SendCCTShipmentMethodApplicatorTest : DocDataObjectSendingMessageMethodApplicatorTest
	{
		public void TestCertificateValidation()
		{
			var shipment = CreateShipmentWithoutErrors("081001", "S00000001", "C00001001", "AUSYD", "BRSAO");
			Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Skip;

			var expectedLog = "WARNING: [HL S00000001] To send messages to CCT you must have a valid certificate loaded against your staff profile.";
			ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);
		}

		public void TestSkip()
		{
			var shipmentWithoutErrors = CreateShipmentWithoutErrors("081001", "S00000001", "C00001001", "AUSYD", "BRSAO");
			var newShipmentWithoutErrors = CreateShipmentWithoutErrors("081002", "S00000002", "C00001002", "AUSYD", "BRSAO");
			var shipmentWithFilterErrors = Factory.New<ForwardingShipment>();
			PopulateShipment(shipmentWithFilterErrors, "081001", "AUSYD", "GBLON", "S00000003");
			var shipmentWithSendErrors = Factory.New<ForwardingShipment>();
			PopulateShipment(shipmentWithSendErrors, "081001", "AUSYD", "BRSAO", "S00000004");
			var consolWithoutErrors = CreateConsolWithoutErrors("215-98757411", "C00000001", "AUSYD", "BRSAO", "AdvancedCargoReportBR", Core.Constants.CountryCodes.Brazil);

			Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Skip;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";
			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			using (Factory.AddDisposableService())
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var shipmentModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobShipment))
			{
				var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(ModuleIDs.JobShipment, shipmentModule.SecurityCheckpoint);
				var menuCheckpoint = Env.Security.FindOrCreateVisualizerFormCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobShipment, parentCheckpoint);
				menuCheckpoint.IsAllowed = true;
				Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobShipment, menuCheckpoint).IsAllowed = true;

				var expectedLog =
					"INFO: [HL S00000001] processed successfully.\n" +
					"WARNING: [HL S00000003] The shipment is not arriving in Brazil.\n" +
					"WARNING: [HL S00000004] " +
					"Message Error - CompanyName: Shipper party name and address information is required.\n" +
					"Message Error - AddressLine1: Shipper Address is required for CCT messaging.\n" +
					"Message Error - CompanyName: Consignee party name and address information is required.\n" +
					"Message Error - AddressLine1: Consignee Address is required for CCT messaging.\n" +
					"Message Error - CompanyName: Import Agent party name and address information is required.\n" +
					"Message Error - AddressLine1: Import Agent Address is required for CCT messaging.\n" +
					"Message Error - TaxNumber: CNPJ is required for CCT messaging.\n" +
					"Message Error - Code: Airport Of Departure Code is required for CCT messaging.\n" +
					"Message Error - Description: Airport Of Departure is required for CCT messaging.\n" +
					"Message Error - Description: Airport Of Destination is required for CCT messaging.\n" +
					"Message Error - Value: Total Gross Weight greater than zero is required for CCT messaging.\n" +
					"Message Error - AirlinePrefix: Airline Prefix is required for CCT messaging.\n" +
					"Message Error - SerialNo: MAWB number is required.\n" +
					"Message Error - TotalNoOfPieces: Total No Of Pieces greater than zero is required for CCT messaging.\n" +
					"Message Error - ShippersSignature: Shippers Signature is required for CCT messaging.\n" +
					"Message Error - IssueDate: Issue Date is required for CCT messaging.\n" +
					"Message Error - IssuePlace: Issue Place is required for CCT messaging.\n" +
					"Message Error - AgentsSignature: Agents Signature or Agent Approved Exporter Number is required for CCT messaging.\n" +
					"Message Error - AgentApprovedExporterNumber: Agents Signature or Agent Approved Exporter Number is required for CCT messaging.\n" +
					"Message Error - TotalPrepaid: Total Charges are required for CCT Shipment messages.\n" +
					"Message Error - TotalCollect: Total Charges are required for CCT Shipment messages.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors, shipmentWithFilterErrors, shipmentWithSendErrors }, expectedLog);

				var expectedNotValidLog =
					"WARNING:  Can not load Shipment correctly.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedNotValidLog);

				PopulateMultipleRUCReferenceNumber(newShipmentWithoutErrors);
				var expectedLogWithWarning =
					"INFO: [HL S00000002] processed successfully.\n" +
					"WARNING: [HL S00000002] Warning - RUCReferenceNumber: CCT supports only one RUC and first available RUC will be sent.";
				ApplyApplicator(new BusinessObject[] { newShipmentWithoutErrors }, expectedLogWithWarning);
			}
		}

		public void TestAbort()
		{
			var shipmentWithoutErrors = CreateShipmentWithoutErrors("081001", "S00000001", "C00001001", "AUSYD", "BRSAO");
			var newShipmentWithoutErrors = CreateShipmentWithoutErrors("081002", "S00000002", "C00001002", "AUSYD", "BRSAO");
			var shipmentWithFilterErrors = Factory.New<ForwardingShipment>();
			PopulateShipment(shipmentWithFilterErrors, "081001", "AUSYD", "GBLON", "S00000003");
			var shipmentWithSendErrors = Factory.New<ForwardingShipment>();
			PopulateShipment(shipmentWithSendErrors, "081001", "AUSYD", "BRSAO", "S00000004");
			var consolWithoutErrors = CreateConsolWithoutErrors("215-98757411", "C00000001", "AUSYD", "BRSAO", "AdvancedCargoReportBR", Core.Constants.CountryCodes.Brazil);

			Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Abort;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";
			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			using (Factory.AddDisposableService())
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var shipmentModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobShipment))
			{
				var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(ModuleIDs.JobShipment, shipmentModule.SecurityCheckpoint);
				var menuCheckpoint = Env.Security.FindOrCreateVisualizerFormCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobShipment, parentCheckpoint);
				menuCheckpoint.IsAllowed = true;
				Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(MenuItem.PK.ToGuid(), MenuItem.SU_MenuNameMultilingual, ModuleIDs.JobShipment, menuCheckpoint).IsAllowed = true;

				var expectedLog =
					"INFO: [HL S00000001] processed successfully.\n" +
					"ERROR: [HL S00000003] The shipment is not arriving in Brazil.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors, shipmentWithFilterErrors, shipmentWithSendErrors }, expectedLog);

				var expectedNotValidLog =
					"ERROR:  Can not load Shipment correctly.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedNotValidLog);

				PopulateMultipleRUCReferenceNumber(newShipmentWithoutErrors);
				var expectedLogWithWarning =
					"INFO: [HL S00000002] processed successfully.\n" +
					"WARNING: [HL S00000002] Warning - RUCReferenceNumber: CCT supports only one RUC and first available RUC will be sent.\n" +
					"ERROR: [HL S00000003] The shipment is not arriving in Brazil.";
				ApplyApplicator(new BusinessObject[] { newShipmentWithoutErrors, shipmentWithFilterErrors }, expectedLogWithWarning);
			}
		}

		#region Implementation
		protected override string NoSelectedError => "ERROR: No shipments selected.";

		protected override string MessageSenderContext => "CargoControlAndTransitMessageSender";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SendCCTShipmentMethodApplicator(Settings, Factory);
		}

		protected override DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider()
		{
			return new CCTShipmentReportSendingProvider(Factory);
		}

		protected IStmMenuItem MenuItem => menuItem ?? (menuItem = Factory.Load<VisualizerMenuItem>(ShipmentSystemFormMenuItems.DocumentMenuCCTShipmentReport));
		IStmMenuItem menuItem;

		void PopulateMultipleRUCReferenceNumber(ForwardingShipment shipment)
		{
			var entryNum1 = shipment.Numbers.AddNew();
			entryNum1.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			entryNum1.CE_EntryNum = "6BR123456789D0VAHK001";

			var entryNum2 = shipment.Numbers.AddNew();
			entryNum2.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			entryNum2.CE_EntryNum = "6BR987654321DOVAHK002";
		}
		#endregion
	}
}
