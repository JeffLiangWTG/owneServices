using System;
using System.Text;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(BorderLineReleaseMessageModule))]
	sealed class BorderLineReleaseMessageModuleTest : MQEDIMessageModuleTest
	{
		public void TestFilterControl()
		{
			using (var module = new BorderLineReleaseMessageModule())
			{
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (MQEDIMessageFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					Assert(control.FilteredGrid.Columns.Contains(LineReleaseMQEDIMessage.Schema.EM_ActionStatus));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SendOrReceiveHumanReadable));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_ApplicationCode));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_ApplicationReference));
					AssertEquals("Entry Number", control.FilteredGrid.GetColumnCaption(EDIMessage.Schema.EM_ApplicationReference));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageNum));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageType));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageSubType));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageSubTypeDescription));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_User));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeSender));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeReceiver));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SendingUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemCreateUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_DateTimeInterchangeSent));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeNumber));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeStatus));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemLastEditUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemLastEditTimeUtc));
				}
			}
		}

		public void TestAddToShipment()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_IsConsignor = true;
			manufacturer.OH_FullName = "Mr Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XOHONCAN715SCA");
			var importer = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			importer.OH_IsConsignee = true;
			importer.OH_IsShippingLine = true;
			var helper = new DeclarationTestHelper(Factory);
			helper.UpdateOrAddCustomsRegNo(importer, "95-204100600", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_IsWarehouseClient = true;
			warehouse.OH_FullName = "Mr Warehouse";
			warehouse.OH_Code = "WAR" + new Random().Next(1000000).ToString();
			warehouse.MainAddress.LocalControlledPremisesID = "L84";
			var billIssuer = Factory.NewWithValidTestData<OrgHeader>();
			billIssuer.OH_IsShippingProvider = true;
			billIssuer.OH_FullName = "Mr Bill Issuer";
			billIssuer.OH_Code = "ISS" + new Random().Next(1000000).ToString();
			billIssuer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CPRS");
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 412412341117070003L84".PadRight(80));
			messageTextBuilder.Append("X20870321    870390    HON2AMED112AUUCA        00000010NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS073197847799                00000010".PadRight(80));
			messageTextBuilder.Append("X400001000100000000".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00004".PadRight(80));
			var message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "41241234";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = importer.PK;
			Factory.Save();
			using (var module = (BorderLineReleaseMessageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.SetupPullerDialogForTestingEvent += new BorderLineReleaseMessageModule.SetupPullerDialogEventHandler((Customs.Module.DeclarationFromShipmentPullerDialog pullerDialog) =>
				{
					pullerDialog.BusinessEntity.ShipmentPK = shipment.PK;
					pullerDialog.SetCreateClickedForTesting(true);
				});
				var actionsMenuItem = module.ContextMenuExposedForTesting.FindByText("Actions");
				var item = actionsMenuItem.MenuItems.FindByText(BorderLineReleaseMessageModule.AddToExistingShipmentMenuName);
				AssertNotNull("Menu action '" + BorderLineReleaseMessageModule.CreateNewDeclarationMenuName + "' should not be null.", item);
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (MQEDIMessageFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					var filterObject = (BorderLineReleaseMessageFilterStripBusinessObject)control.FilterBusinessObject;
					var entryNumberFilter = (ModuleTextFilter)filterObject["Entry Number"];
					entryNumberFilter.IsActive = true;
					entryNumberFilter.Property = "41241234";
					control.FirePerformSearch();
					var messages = module.GridCollection.ToArray();
					AssertEquals(1, messages.Length);
					AssertEquals(message.PK, messages[0].PK);
					module.GridExposedForTesting.UnSelectAll();
					var formCached = OpenedFormCache.GetInstance();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					item.PerformClick();
					AssertEquals(MQEDIMessageModule.SelectAtLeastOneMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("formCached.Count", 0, formCached.Count);
					module.GridExposedForTesting.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					item.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					var declarationForm = formCached.GetForm(shipment.PK.ToGuid(), ControllerIDs.JobShipment.Name);
					AssertEquals(typeof(Freight.Forwarding.GUI.ShipmentForm), declarationForm.GetType());
					formCached.CloseAllCachedForms();
				}
			}
		}

		public void TestCreateNewDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 412412341206051019L89".PadRight(80));
			messageTextBuilder.Append("X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS053390088346CPRS23423343343400000008CPRS634345455434".PadRight(80));
			messageTextBuilder.Append("X25UPRR106697071114UPRR72345345245600000007".PadRight(80));
			messageTextBuilder.Append("X400001000100000002".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00006".PadRight(80));
			var message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "41241234";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			Factory.Save();
			using (var module = (BorderLineReleaseMessageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var actionsMenuItem = module.ContextMenuExposedForTesting.FindByText("Actions");
				var item = actionsMenuItem.MenuItems.FindByText(BorderLineReleaseMessageModule.CreateNewDeclarationMenuName);
				AssertNotNull("Menu action '" + BorderLineReleaseMessageModule.CreateNewDeclarationMenuName + "' should not be null.", item);
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (MQEDIMessageFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					var filterObject = (BorderLineReleaseMessageFilterStripBusinessObject)control.FilterBusinessObject;
					var entryNumberFilter = (ModuleTextFilter)filterObject["Entry Number"];
					entryNumberFilter.IsActive = true;
					entryNumberFilter.Property = "41241234";
					control.FirePerformSearch();
					var messages = module.GridCollection.ToArray();
					AssertEquals(1, messages.Length);
					AssertEquals(message.PK, messages[0].PK);
					module.GridExposedForTesting.UnSelectAll();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var formCached = OpenedFormCache.GetInstance();
					item.PerformClick();
					AssertEquals(MQEDIMessageModule.SelectAtLeastOneMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("formCached.Count", 0, formCached.Count);
					module.GridExposedForTesting.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					item.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					var declarationForm = formCached.GetForm(module.LastCreatedDeclarationPKForTesting.ToGuid(), ControllerIDs.Customs.JobDeclaration.Name);
					AssertEquals(typeof(JobDeclarationForm), declarationForm.GetType());
					formCached.CloseAllCachedForms();
				}
			}
		}

		public void TestOpenLinkedDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "TEST";
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 412412341206051019L89".PadRight(80));
			messageTextBuilder.Append("X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS053390088346CPRS23423343343400000008CPRS634345455434".PadRight(80));
			messageTextBuilder.Append("X25UPRR106697071114UPRR72345345245600000007".PadRight(80));
			messageTextBuilder.Append("X400001000100000002".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00006".PadRight(80));
			var message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "41241234";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_LinkTable = "JobDeclaration";
			message.EM_LinkUniqueID = declaration.PK;
			message.EM_MessageText = messageTextBuilder.ToString();
			Factory.Save();
			using (var module = (BorderLineReleaseMessageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var actionsMenuItem = module.ContextMenuExposedForTesting.FindByText("Actions");
				var item = actionsMenuItem.MenuItems.FindByText(BorderLineReleaseMessageModule.OpenLinkedDeclarationMenuName);
				AssertNotNull("Menu action '" + BorderLineReleaseMessageModule.OpenLinkedDeclarationMenuName + "' should not be null.", item);
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (MQEDIMessageFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					var filterObject = (BorderLineReleaseMessageFilterStripBusinessObject)control.FilterBusinessObject;
					var entryNumberFilter = (ModuleTextFilter)filterObject["Entry Number"];
					entryNumberFilter.IsActive = true;
					entryNumberFilter.Property = "41241234";
					control.FirePerformSearch();
					var messages = module.GridCollection.ToArray();
					AssertEquals(1, messages.Length);
					AssertEquals(message.PK, messages[0].PK);
					module.GridExposedForTesting.UnSelectAll();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var formCached = OpenedFormCache.GetInstance();
					item.PerformClick();
					AssertEquals(MQEDIMessageModule.SelectAtLeastOneMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("formCached.Count", 0, formCached.Count);
					module.GridExposedForTesting.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					item.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					var declarationForm = formCached.GetForm(declaration.PK.ToGuid(), ControllerIDs.Customs.JobDeclaration.Name);
					AssertEquals(typeof(JobDeclarationForm), declarationForm.GetType());
					formCached.CloseAllCachedForms();
				}
			}
		}

		public override void TestSetToComplete()
		{
			AssertSetToCompleteMenuItem(ApplicationIdentifierCodeList.Codes.LineRelease);
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = new BorderLineReleaseMessageModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.BorderLineReleaseMessage, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.BorderLineReleaseMessage;

		protected override void SetUp()
		{
			base.SetUp();
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			rawEnableComplianceRisk?.Dispose();
			rawEnableComplianceRisk = null;
			rawFreightComplianceWiseRegistry?.Dispose();
			rawFreightComplianceWiseRegistry = null;
			base.TearDown();
		}
		IDisposable rawEnableComplianceRisk;
		IDisposable rawFreightComplianceWiseRegistry;
	}
}
