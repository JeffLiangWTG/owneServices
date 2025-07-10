using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ElectronicBOLUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (var form = new FormForTest(shipment))
			{
				form.Show();

				var mainPanel = form.Controls.Find("MainPanel", true).FirstOrDefault();

				AssertNotNull(mainPanel);

				var leftPanel = mainPanel.Controls.Find("LeftPanel", true).FirstOrDefault();
				var rightPanel = mainPanel.Controls.Find("RightPanel", true).FirstOrDefault();

				AssertNotNull(leftPanel);
				AssertNotNull(rightPanel);

				var leftPanelControls = new[]
				{
					"BillOfLadingBillTypeDropEdit",
					"FirstHolderDocAddressControl",
					"SurrenderPartyDocAddressControl"
				};
				foreach (var controlName in leftPanelControls)
				{
					var child = leftPanel.Controls.Find(controlName, true).FirstOrDefault();
					AssertNotNull(child);
				}

				var rightPanelControls = new[]
				{
					"BillOfLadingBillTermsDropEdit",
					"ShipperDocAddressControl",
					"ConsigneeTextBoxLabel",
					"ConsigneeTextBox",
					"AmendmentRequestDetailsTextBox",
					"PublishButton",
					"HouseBillNumberTextBox",
					"VersionTextBox",
					"EBLIdentifierTextBox",
					"BillStatusDropEdit",
					"EBLDateDateEdit",
					"ViewEditBillButton"
				};
				foreach (var controlName in rightPanelControls)
				{
					var child = rightPanel.Controls.Find(controlName, true).FirstOrDefault();
					AssertNotNull(child);
				}
			}
		}

		public void TestControls_ReadOnly()
		{
			var readOnlyControls = new[]
			{
				"BillOfLadingBillTermsDropEdit",
				"ShipperDocAddressControl",
				"ConsigneeDocAddressControl",
				"ConsigneeTextBox",
				"EBLIdentifierTextBox",
				"VersionTextBox",
				"HouseBillNumberTextBox"
			};

			var shipment = Factory.New<ForwardingShipment>();

			using (var form = new FormForTest(shipment))
			{
				form.Show();

				foreach (var controlName in readOnlyControls)
				{
					var control = form.Controls.Find(controlName, true).FirstOrDefault();

					AssertNotNull(control);
					Assert(controlName + " must be read-only", control.GetReadOnly());
				}
			}
		}

		public void TestBillOfLadingBillTypeDropEditChange()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;

			using (var form = new FormForTest(shipment))
			{
				form.Show();

				var mainPanel = form.Controls.Find("MainPanel", true).FirstOrDefault();
				var leftPanel = mainPanel.Controls.Find("LeftPanel", true).FirstOrDefault();
				var rightPanel = mainPanel.Controls.Find("RightPanel", true).FirstOrDefault();

				var billOfLadingBillTypeDropEdit = leftPanel.Controls.Find("BillOfLadingBillTypeDropEdit", true).FirstOrDefault();
				var billOfLadingBillTermsDropEdit = rightPanel.Controls.Find("BillOfLadingBillTermsDropEdit", true).FirstOrDefault();
				var consigneeTextBoxLabel = (ZLabel)rightPanel.Controls.Find("ConsigneeTextBoxLabel", true).FirstOrDefault();
				var consigneeTextBox = rightPanel.Controls.Find("ConsigneeTextBox", true).FirstOrDefault();
				var consigneeDocAddressControl = rightPanel.Controls.Find("ConsigneeDocAddressControl", true).FirstOrDefault();
				var toOrderDocAddressControl = rightPanel.Controls.Find("ToOrderDocAddressControl", true).FirstOrDefault();

				AssertEquals("Consignee", consigneeTextBoxLabel.Text);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.BlankEndorse;
				AssertEquals(Constants.BillOfLadingBillTerms.Codes.Transferable, billOfLadingBillTermsDropEdit.Text);
				AssertEquals(true, consigneeTextBoxLabel.Visible);
				AssertEquals(true, consigneeTextBox.Visible);
				AssertEquals(false, consigneeDocAddressControl.Visible);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
				AssertEquals(Constants.BillOfLadingBillTerms.Codes.NonTransferable, billOfLadingBillTermsDropEdit.Text);
				AssertEquals(false, consigneeTextBoxLabel.Visible);
				AssertEquals(false, consigneeTextBox.Visible);
				AssertEquals(true, consigneeDocAddressControl.Visible);

				shipment.JS_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
				AssertEquals(Constants.BillOfLadingBillTerms.Codes.Transferable, billOfLadingBillTermsDropEdit.Text);
				AssertEquals(true, toOrderDocAddressControl.Visible);
				AssertEquals(false, consigneeTextBoxLabel.Visible);
				AssertEquals(false, consigneeTextBox.Visible);
				AssertEquals(false, consigneeDocAddressControl.Visible);
			}
		}

		public void TestShowFormBuilderBillOfLading()
		{
			var shipment = CreateShipment();
			Assert(!shipment.IsEditingElectronicBOL);
			AssertEquals(ZString.Empty, shipment.JS_ElectronicBillOfLadingHouseBill);
			AssertNotEquals(shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				using (var form = new ZForm(shipment))
				using (var control = new ElectronicBOLUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals(shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);

					shipment.JS_ElectronicBillOfLadingHouseBill = "S0000872";
					AssertNotEquals(shipment.JS_HouseBill, shipment.JS_ElectronicBillOfLadingHouseBill);
					Factory.Save();

					AssertEquals("View/Edit Bill Of Lading", control.ViewEditBillButton.Text);
					control.ViewEditBillButton.PerformClick();
				}
			}

			var lastShownFormTypeName = ZFormModaliser.LastFormShownDialogForTest?.GetType().FullName;
			AssertEquals("Form Builder HBL form has been shown.", "Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", lastShownFormTypeName);
			Assert(!shipment.IsEditingElectronicBOL);
		}

		public void TestSetPublishButtonText()
		{
			var shipment = CreateShipment();

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is enabled", true, control.PublishButton.Enabled);
			}

			shipment.JS_ElectronicBillOfLadingStatus = "OBA";

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The button text should be 'Accept/Deny Amendment Request'", "Accept/Deny Amendment Request", control.PublishButton.Text);
				AssertEquals("The publish button is enabled", true, control.PublishButton.Enabled);
			}
		}

		public void TestSetPublishButtonEnabled()
		{
			var shipment = CreateShipment();

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);

				foreach (var code in new[] {
					FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication,
					FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillPublished,
					FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred,
					FreightConstants.BillOfLadingBillStatus.Codes.Surrendered,
					FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper
				})
				{
					shipment.JS_ElectronicBillOfLadingStatus = code;
					Assert(!control.PublishButton.Enabled);
				}

				shipment.JS_ElectronicBillOfLadingStatus = ZString.Empty;
				Assert(control.PublishButton.Enabled);
			}
		}

		public void TestReturnsExpectedBindingMember()
		{
			var shipment = CreateShipment();

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("JS_ElectronicBillOfLadingAmendmentRequestDetailsForBinding", control.FindSingle<ZTextBox>("AmendmentRequestDetailsTextBox").GetBindingMember());
				AssertEquals("JS_ElectronicBillOfLadingReference", control.FindSingle<ZTextBox>("EBLIdentifierTextBox").GetBindingMember());
				AssertEquals("JS_Calc_ElectronicBillOfLadingDate", control.FindSingle<ZDateEdit>("EBLDateDateEdit").GetBindingMember());
				AssertEquals("JS_ElectronicBillOfLadingVersion", control.FindSingle<ZTextBox>("VersionTextBox").GetBindingMember());
				AssertEquals("JS_ElectronicBillOfLadingStatus", control.FindSingle<ZDropEdit>("BillStatusDropEdit").GetBindingMember());
				AssertEquals("JS_ElectronicBillOfLadingTerms", control.FindSingle<ZDropEdit>("BillOfLadingBillTermsDropEdit").GetBindingMember());
				AssertEquals("JS_ElectronicBillOfLadingHouseBill", control.FindSingle<ZTextBox>("HouseBillNumberTextBox").GetBindingMember());
			}
		}

		public void TestDisablePublishButton_BillStatusIsSurrendered()
		{
			var shipment = CreateShipment();

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is enabled", true, control.PublishButton.Enabled);
			}

			shipment.JS_ElectronicBillOfLadingStatus = "SUR";

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is not enabled", false, control.PublishButton.Enabled);
			}
		}

		public void TestDisablePublishButton_BillStatusIsSwitchToPaper()
		{
			var shipment = CreateShipment();

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is enabled", true, control.PublishButton.Enabled);
			}

			shipment.JS_ElectronicBillOfLadingStatus = "STP";

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is not enabled", false, control.PublishButton.Enabled);
			}
		}

		public void TestSetPublishButton_ElectronicBillOfLadingStatusValueChanged()
		{
			var shipment = CreateShipment();

			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is enabled", true, control.PublishButton.Enabled);

				shipment.JS_ElectronicBillOfLadingStatus = "SUR";
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is not enabled", false, control.PublishButton.Enabled);

				shipment.JS_ElectronicBillOfLadingStatus = "STP";
				AssertEquals("The default button text is 'Publish'", "Publish", control.PublishButton.Text);
				AssertEquals("The publish button is not enabled", false, control.PublishButton.Enabled);

				shipment.JS_ElectronicBillOfLadingStatus = "OBA";
				AssertEquals("The button text should be 'Accept/Deny Amendment Request'", "Accept/Deny Amendment Request", control.PublishButton.Text);
				AssertEquals("The publish button is enabled", true, control.PublishButton.Enabled);
			}
		}

		public void TestSendAmendmentDeniedEvent_ResetToPreviousBillStatus()
		{
			AssertWithLaunchHouseBillFormSetup(() =>
			{
				var shipment = CreateShipment();
				shipment.JS_ElectronicBillOfLadingStatus = "OBA";
				shipment.HolderDocAddress.OrganisationPK = shipment.ConsigneeDocumentaryAddress.OrganisationPK;
				Factory.Save();

				var failReason = "1234567890";

				using (var form = new ZForm(shipment))
				using (var control = new ElectronicBOLUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					UnitTestUserNotification.Instance.AddUserResponse(failReason);
					control.PublishButton.PerformClick();
					AssertContains("You have not received Amendment Requested. No Accept/Deny action is required.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form = new ZForm(shipment))
				using (var control = new ElectronicBOLUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var parameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(Params.Type, Core.Constants.BillStatusUpdatedTypes.OriginalBillPublished),
						new KeyValuePair<string, string>(Params.Department, ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry)
					};
					shipment.Logs.CreateOrRecreateEventLog(Events.BillStatusUpdated, EstimateActual.Actual, new ZDateTimeOffset(2024, 11, 19, 0, 0, 1), string.Empty, parameters);
					Factory.Save();
					AssertEquals("OBP", shipment.JS_ElectronicBillOfLadingStatus);

					parameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(Params.Type, Core.Constants.BillStatusUpdatedTypes.AmendmentRequested),
						new KeyValuePair<string, string>(Params.Department, ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry),
						new KeyValuePair<string, string>(Params.ReferenceNumber, "WTLDAUILA_S00001001_1"),
						new KeyValuePair<string, string>(Params.RequestNumber, "1234"),
					};
					shipment.Logs.CreateOrRecreateEventLog(Events.BillStatusUpdated, EstimateActual.Actual, new ZDateTimeOffset(2024, 11, 19, 0, 1, 1), string.Empty, parameters);
					Factory.Save();
					Thread.Sleep(10);
					AssertEquals("OBA", shipment.JS_ElectronicBillOfLadingStatus);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					UnitTestUserNotification.Instance.AddUserResponse(failReason);
					control.PublishButton.PerformClick();
					AssertContains("Sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					var eventLogs = shipment.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.IsCancelled == ZBool.False && log.SL_SE_NKEvent == "MSN"
							&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department) && department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry
							&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType) && eventType == "Amendment Denied").ToList();

					AssertEquals("The Amendment Denied event log is created.", 1, eventLogs.Count);
					AssertEquals("The Bill Status reverts to the previous Bill Status, before the Amendment Request was received.", "OBP", shipment.JS_ElectronicBillOfLadingStatus);

					var dex = shipment.Logs.MostRecentLogByEventTime(AutoEvents.DataExport);
					AssertNotNull(dex);

					var message = dex.RelatedEDIMessage;
					AssertNotNull(message);

					AssertContains("<EventType>BLU</EventType>", message.Message.EM_MessageText);
					AssertContains(ZString.Format(@"<Context>
        <Type>NotificationDetails</Type>
        <Value>{0}</Value>
      </Context>", failReason), message.Message.EM_MessageText);
					AssertContains("<ReferenceNumber>WTLDAUILA_S00001001_1</ReferenceNumber>", message.Message.EM_MessageText);
					AssertContains("<RequestNumber>1234</RequestNumber>", message.Message.EM_MessageText);
					AssertContains("<Type>Amendment Denied</Type>", message.Message.EM_MessageText);
					AssertContains("<Department>Carrier</Department>", message.Message.EM_MessageText);
				}
			});
		}

		public void TestLaunchHouseBillForm_AcceptAmendmentRequest()
		{
			AssertWithLaunchHouseBillFormSetup(() =>
			{
				var shipment = CreateShipment();
				shipment.JS_ElectronicBillOfLadingStatus = "OBA";
				shipment.HolderDocAddress.OrganisationPK = shipment.ConsigneeDocumentaryAddress.OrganisationPK;
				Factory.Save();

				using (var form = new ZForm(shipment))
				using (var control = new ElectronicBOLUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					control.PublishButton.PerformClick();

					var lastShownFormTypeName = ZFormModaliser.LastFormShownDialogForTest?.GetType().FullName;
					AssertEquals("Form Builder HBL form has been shown.", "Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", lastShownFormTypeName);
				}
			});
		}

		public void TestLaunchHouseBillForm_PublishingRejected()
		{
			AssertWithLaunchHouseBillFormSetup(() =>
			{
				var shipment = CreateShipment();
				shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.PublishingRejected;
				shipment.HolderDocAddress.OrganisationPK = shipment.ConsigneeDocumentaryAddress.OrganisationPK;
				Factory.Save();

				using (var form = new ZForm(shipment))
				using (var control = new ElectronicBOLUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.PublishButton.PerformClick();

					var lastShownFormTypeName = ZFormModaliser.LastFormShownDialogForTest?.GetType().FullName;
					AssertEquals("Form Builder HBL form has been shown.", "Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", lastShownFormTypeName);
				}
			});
		}

		public void TestPublish_CheckElectronicBOLMinimumRequirements()
		{
			var shipment = CreateShipment();
			shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
			Factory.Save();

			using (var module = ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
			using (var form = new ZForm(shipment))
			using (var control = new ElectronicBOLUserControl())
			{
				form.Controls.Add(control);

				form.Show();
				control.PublishButton.PerformClick();

				AssertEquals("Show error when the security is not allowed", "There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPublish_NotSaved()
		{
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "BRRIO";
				shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.CargowiseBill;
				shipment.JS_HouseBill = "S00021Z";

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.HolderDocAddress.OrganisationPK = org.PK;
				shipment.SurrenderPartyDocAddress.OrganisationPK = org.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingTerms = Core.Constants.BillOfLadingBillTerms.Codes.Transferable;
				shipment.JS_ElectronicBillOfLadingStatus = ZString.Empty;

				using (var module = ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
				using (var form = new ZForm(shipment))
				using (var control = new ElectronicBOLUserControl())
				{
					form.Controls.Add(control);

					var billOfLadingMenu = Factory.Load<IStmMenuItem>(ShipmentSystemFormMenuItems.BillOfLadingPK);
					var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(ModuleIDs.JobShipment, module.SecurityCheckpoint);

					form.Show();
					control.PublishButton.PerformClick();

					Assert(!shipment.IsInDatabase);
					AssertEquals("Please save all changes before publishing.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					Factory.Save();
					control.PublishButton.PerformClick();

					var lastShownFormTypeName = ZFormModaliser.LastFormShownDialogForTest?.GetType().FullName;
					AssertEquals("Form Builder HBL form has been shown.", "Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", lastShownFormTypeName);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
					control.PublishButton.PerformClick();

					Assert(shipment.HasChanges);
					AssertEquals("Please save all changes before publishing.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPublish_ViewEditBillButton()
		{
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var shipment = CreateShipment();

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "Test Organization1";
				org.OH_IsConsignor = true;
				org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CustomsRegNo = "123456";
				orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

				shipment.HolderDocAddress.OrganisationPK = org.PK;
				shipment.SurrenderPartyDocAddress.OrganisationPK = org.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
				shipment.JS_ElectronicBillOfLadingConsigneeDocAddress.OrganisationPK = org.PK;

				shipment.JS_ElectronicBillOfLadingType = Core.Constants.BillOfLadingBillType.Codes.Straight;
				shipment.JS_ElectronicBillOfLadingTerms = Core.Constants.BillOfLadingBillTerms.Codes.Transferable;
				shipment.JS_ElectronicBillOfLadingStatus = ZString.Empty;
				Factory.Save();

				using (var module = ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
				using (var form = new ZForm(shipment))
				using (var control = new ElectronicBOLUserControl())
				{
					form.Controls.Add(control);

					form.Show();
					control.PublishButton.PerformClick();

					var lastShownFormTypeName = ZFormModaliser.LastFormShownDialogForTest?.GetType().FullName;
					AssertEquals("Form Builder HBL form has been shown.", "Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", lastShownFormTypeName);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		void AssertWithLaunchHouseBillFormSetup(Action testLogic)
		{
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				testLogic();
			}
		}

		ForwardingShipment CreateShipment()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST002";
			org.OH_FullName = "Test Organization2";
			org.OH_IsConsignee = true;

			var orgCusCode = org.CustomsCodes.AddNew();
			orgCusCode.OK_CustomsRegNo = "123456";
			orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRRIO";
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.CargowiseBill;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;

			Factory.Save();

			return shipment;
		}

		class FormForTest : ZForm
		{
			public FormForTest(ForwardingShipment shipment) : base(shipment)
			{
				Control = new ElectronicBOLUserControl();
				Controls.Add(Control);
			}

			public ElectronicBOLUserControl Control;

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					Control.Dispose();
				}

				base.Dispose(disposing);
			}
		}
	}
}
