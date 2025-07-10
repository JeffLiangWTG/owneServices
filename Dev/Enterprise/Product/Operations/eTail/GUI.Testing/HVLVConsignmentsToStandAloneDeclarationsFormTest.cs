using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVConsignmentsToStandAloneDeclarationsForm))]
	public class HVLVConsignmentsToStandAloneDeclarationsFormTest : ZFormBasherTest
	{
		public void TestBulkConvertStandAloneDeclaration_EndToEnd()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "A01";
			consignor.OH_IsConsignor = true;
			var consignorAddress = consignor.Addresses.AddNew();
			consignorAddress.OA_Address1 = "Apple";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "B02";
			consignee.OH_IsConsignee = true;
			var consigneeAddress = consignee.Addresses.AddNew();
			consigneeAddress.OA_Address1 = "Banana";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress).E2_OA_Address = consignorAddress.PK;
			shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress).E2_OA_Address = consigneeAddress.PK;
			shipment.Consols.AddNew();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			consignment.HVC_ReleaseStatus = HVLVReleaseStatus.Held;
			consignment.HVC_ShipperName = "Shipper Name";
			consignment.HVC_ShipperAddress1 = "Shipping Address";
			consignment.HVC_ShipperCity = "Sydney";
			consignment.HVC_ShipperPostcode = "2200";
			consignment.HVC_GoodsDescription = "TEC Building";

			var item = consignment.Items.AddNew();
			item.HVI_ManifestedWeight = 1;

			Factory.Save();

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				form.FireSaveButton();

				var tabControl = form.Controls.Find("MainTabControl", false).Single() as ZTabControl;
				tabControl.SelectTab("eManifestImportTabPage");

				var declarationReference = form.Controls.Find("zTextBoxStandAloneDeclaration", true).Single() as ZTextBox;
				AssertEquals("Pre-condition: declaration reference is empty", true, string.IsNullOrEmpty(declarationReference.Text));

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					if (form is HVLVConsignmentsToStandAloneDeclarationsForm convertForm)
					{
						var selectOrDeselectAllButton = convertForm.Controls.Find("ButtonSelectOrDeselectAll", true).Single() as ZButton;
						selectOrDeselectAllButton.PerformClick();

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						var convertButton = convertForm.Controls.Find("ButtonConvert", true).Single() as ZButton;
						convertButton.PerformClick();
					}
				});

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hvlvMenu = plugin.TopLevelMenu;
				hvlvMenu.PerformSelect();
				var customsMenu = hvlvMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Caption == "Declaration");
				declarationMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Convert Consignments to Stand Alone Declarations").PerformClick();

				AssertEquals("declaration reference is updated", false, string.IsNullOrEmpty(declarationReference.Text));
			}
		}

		public void TestSelectOrDeselectAll()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var consignment3 = header.Consignments.AddNew();
			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				var consignmentGrid = form.Controls.Find("GridConsignments", true)[0] as ZGrid;
				var consignmentsToDeclarations = consignmentGrid.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().ToArray();

				var consignmentToDeclaration1 = consignmentsToDeclarations[0];
				var consignmentToDeclaration2 = consignmentsToDeclarations[1];
				var consignmentToDeclaration3 = consignmentsToDeclarations[2];

				CombineAssertions("All consignments should be ticked for conversion", () =>
				{
					AssertEquals("consignment1:", true, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", true, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", true, consignmentToDeclaration3.Convert);
				});

				selectOrDeselectAllButton.PerformClick();

				CombineAssertions("All consignments should be unticked for conversion", () =>
				{
					AssertEquals("consignment1:", false, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", false, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", false, consignmentToDeclaration3.Convert);
				});

				consignmentToDeclaration1.Convert = true;
				consignmentToDeclaration2.Convert = false;
				consignmentToDeclaration3.Convert = false;
				selectOrDeselectAllButton.PerformClick();

				CombineAssertions("All consignments should be ticked for conversion", () =>
				{
					AssertEquals("consignment1:", true, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", true, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", true, consignmentToDeclaration3.Convert);
				});
			}
		}

		public void TestSelectHeld()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var consignment3 = header.Consignments.AddNew();
			var consignment4 = header.Consignments.AddNew();
			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildExportConsignment(consignment2, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.None);
			BuildExportConsignment(consignment4, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.None);

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectHeldButton = form.Controls.Find("ButtonSelectHeld", true)[0] as ZButton;
				selectHeldButton.PerformClick();

				var consignmentGrid = form.Controls.Find("GridConsignments", true)[0] as ZGrid;
				var consignmentsToDeclarations = consignmentGrid.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().ToList();

				var consignmentToDeclaration1 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment1.PK);
				var consignmentToDeclaration2 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment2.PK);
				var consignmentToDeclaration3 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment3.PK);
				var consignmentToDeclaration4 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment4.PK);

				CombineAssertions("Held consignments should be ticked for conversion", () =>
				{
					AssertEquals("consignment1:", true, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", true, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", false, consignmentToDeclaration3.Convert);
					AssertEquals("consignment4:", false, consignmentToDeclaration4.Convert);
				});

				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				CombineAssertions("Precondition: All consignments should be ticked for conversion", () =>
				{
					AssertEquals("consignment1:", true, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", true, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", true, consignmentToDeclaration3.Convert);
					AssertEquals("consignment4:", true, consignmentToDeclaration4.Convert);
				});

				selectHeldButton.PerformClick();

				CombineAssertions("consignment 3 and 4 with 'NON' status should be unticked for conversion", () =>
				{
					AssertEquals("consignment1:", true, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", true, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", false, consignmentToDeclaration3.Convert);
					AssertEquals("consignment4:", false, consignmentToDeclaration4.Convert);
				});
			}
		}

		public void TestSelectNoneReported()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var consignment3 = header.Consignments.AddNew();
			var consignment4 = header.Consignments.AddNew();
			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildExportConsignment(consignment2, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.None);
			BuildExportConsignment(consignment4, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.None);

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectNoneReportedButton = form.Controls.Find("ButtonSelectNoneReported", true)[0] as ZButton;
				selectNoneReportedButton.PerformClick();

				var consignmentGrid = form.Controls.Find("GridConsignments", true)[0] as ZGrid;
				var consignmentsToDeclarations = consignmentGrid.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().ToArray();

				var consignmentToDeclaration1 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment1.PK);
				var consignmentToDeclaration2 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment2.PK);
				var consignmentToDeclaration3 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment3.PK);
				var consignmentToDeclaration4 = consignmentsToDeclarations.FirstOrDefault(c => c.Consignment.PK == consignment4.PK);

				CombineAssertions("None reported consignments should be ticked for conversion", () =>
				{
					AssertEquals("consignment1:", false, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", false, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", true, consignmentToDeclaration3.Convert);
					AssertEquals("consignment4:", true, consignmentToDeclaration4.Convert);
				});

				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				CombineAssertions("Precondition: All consignments should be ticked for conversion", () =>
				{
					AssertEquals("consignment1:", true, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", true, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", true, consignmentToDeclaration3.Convert);
					AssertEquals("consignment4:", true, consignmentToDeclaration4.Convert);
				});

				selectNoneReportedButton.PerformClick();

				CombineAssertions("consignment 1 and 2 with 'HLD' status should be unticked for conversion", () =>
				{
					AssertEquals("consignment1:", false, consignmentToDeclaration1.Convert);
					AssertEquals("consignment2:", false, consignmentToDeclaration2.Convert);
					AssertEquals("consignment3:", true, consignmentToDeclaration3.Convert);
					AssertEquals("consignment4:", true, consignmentToDeclaration4.Convert);
				});
			}
		}

		public void TestPopulateForm()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var consignment3 = header.Consignments.AddNew();
			var consignment4 = header.Consignments.AddNew();
			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment4, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Cleared);

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var consignmentGrid = form.Controls.Find("GridConsignments", true)[0] as ZGrid;
				var consignmentsToDeclarations = consignmentGrid.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().ToArray();

				AssertEquals("Expected only three consignments available to be converted to stand alone declaration", 3, consignmentsToDeclarations.Length);

				var consignmentToDeclarationPKs = consignmentsToDeclarations.Select(c => c.Consignment.PK).ToList();

				CombineAssertions("Expected the following consignments to be in the grid:", () =>
				{
					AssertEquals("consignment1:", true, consignmentToDeclarationPKs.Contains(consignment1.PK));
					AssertEquals("consignment2:", true, consignmentToDeclarationPKs.Contains(consignment2.PK));
					AssertEquals("consignment3:", true, consignmentToDeclarationPKs.Contains(consignment3.PK));
				});
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WithProgressBar()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.Items.AddNew();

			var consignment2 = Factory.New<HVLVConsignment>();
			consignment2.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var trackLogs = new List<(string Caption, string Message, int Progress)>();

			Action<string, string, int> progressUpdateCallback = (caption, message, progress) =>
			{
				trackLogs.Add((caption, message, progress));
			};

			var helper = new ConvertToStandAloneDeclarationHelper();
			helper.ConvertToStandAloneDeclarations(new[] { consignment1, consignment2 }, progressUpdateCallback, new CancellationTokenSource());

			AssertEquals("Expected 3 progress logs", 3, trackLogs.Count);
			AssertEquals("Loading HVLV Consignment data", trackLogs[0].Caption);

			AssertEquals("[1 / 2] HVLV Consignments loaded", trackLogs[1].Message);
			AssertEquals(50, trackLogs[1].Progress);

			AssertEquals("[2 / 2] HVLV Consignments loaded", trackLogs[2].Message);
			AssertEquals(100, trackLogs[2].Progress);

			Factory.Save();

			CombineAssertions("Expected both consignments to have a job declaration reference:", () =>
			{
				AssertNotNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
				AssertNotNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
			});
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_CancelOperation()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.Items.AddNew();

			var consignment2 = Factory.New<HVLVConsignment>();
			consignment2.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var helper = new ConvertToStandAloneDeclarationHelper();
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();
			helper.ConvertToStandAloneDeclarations(new[] { consignment1, consignment2 }, (caption, message, progress) => { }, cancellationTokenSource);

			AssertEquals("The operation was canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WhenConsigneeAndShipperIsBlank_ThenDisplayPromptToConvertShipperAndConsigneeToOrgs()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew();
			BuildImportConsignment(consignment, shipment.PK, ZGuid.Empty, ZGuid.Empty, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Expected a message prompt for users to convert their shipper and consignees into organizations", "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form for converting organization addresses to organizations should be shown", typeof(FreeTextAddressConversionForm<HVLVConsignment>), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WhenShipperOrgDoesNotExist_ThenDisplayPrompt()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			BuildImportConsignment(consignment, shipment.PK, ZGuid.Empty, ZGuid.Empty, HVLVReleaseStatus.Held);

			consignment.HVC_ShipperName = "Shipper Name";
			consignment.HVC_ShipperAddress1 = "Shipping Address";
			consignment.HVC_ShipperCity = "Sydney";
			consignment.HVC_ShipperPostcode = "2200";

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Expected a message prompt for users to convert their shipper and consignees into organisations", "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form for converting organisation addresses to organisations should be shown", typeof(FreeTextAddressConversionForm<HVLVConsignment>), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WhenConsigneeOrgDoesNotExist_ThenDisplayPrompt()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			BuildImportConsignment(consignment, shipment.PK, ZGuid.Empty, ZGuid.Empty, HVLVReleaseStatus.Held);

			consignment.HVC_ConsigneeName = "Consignee Name";
			consignment.HVC_ConsigneeAddress1 = "Consignee Address";
			consignment.HVC_ConsigneeCity = "Sydney";

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Expected a message prompt for users to convert their shipper and consignees into organisations", "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form for converting organisation addresses to organisations should be shown", typeof(FreeTextAddressConversionForm<HVLVConsignment>), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WhenNewOrgsAreNotCreatedViaSimilarAddressForm_NoConsignmentsAreConvertedToDeclaration()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			BuildImportConsignment(consignment, shipment.PK, ZGuid.Empty, ZGuid.Empty, HVLVReleaseStatus.Held);

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Code = "TESCONS";
			consigneeOrg.OH_FullName = "Test Consignee Company";

			var consigneeAddress = consigneeOrg.Addresses.AddNew();
			consigneeAddress.OA_Code = "ADDR1";
			consigneeAddress.OA_City = "Consignee City";
			consigneeAddress.OA_Address1 = "Address 1";

			using (var convertToStandAloneDeclarationsForm = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				convertToStandAloneDeclarationsForm.Show();
				var selectOrDeselectAllButton = convertToStandAloneDeclarationsForm.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = convertToStandAloneDeclarationsForm.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Expected a message prompt for users to convert their shipper and consignees into organisations", "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Precondition:", typeof(FreeTextAddressConversionForm<HVLVConsignment>), ZFormModaliser.LastFormShownDialogForTest.GetType());

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is FreeTextAddressConversionForm<HVLVConsignment> similarOrgForm)
					{
						var similarOrgsGrid = similarOrgForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as ZDisplayGrid;
						AssertEquals("Precondition", consigneeAddress.PK, ((OrgPatternMatch)similarOrgsGrid.GetFirstSelectedRow()).Address.PK);
						similarOrgForm.Close();
					}
				});

				AssertEquals("Declaration reference field for the consignment should not be populated", ZString.Empty, consignment.DeclarationReferenceForDisplay);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WhenNewOrgsAreCreatedViaSimilarAddressForm_ThenPromptUserToConvertAgainAfterwards()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			NZAddress.OA_PostCode = "2345";
			NZAddress.OA_State = "AUK";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			BuildImportConsignment(consignment, shipment.PK, NZAddress.PK, ZGuid.Empty, HVLVReleaseStatus.Held);
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ConsigneeName = "Test Consignee Company";
			consignment.HVC_ConsigneeAddress1 = "4444 Consignee Avenue";
			consignment.HVC_ConsigneeCity = "Consigneeland";
			consignment.Items.AddNew();

			var selectedConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			selectedConsigneeOrg.OH_Code = "TESCONS";
			selectedConsigneeOrg.OH_FullName = "Test Consignee Company";

			var selectedConsigneeAddress = selectedConsigneeOrg.Addresses.AddNew();
			selectedConsigneeAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			selectedConsigneeAddress.OA_Code = "TESCONSADD";
			selectedConsigneeAddress.OA_Address1 = "4444 Consignee Avenue";
			selectedConsigneeAddress.OA_City = "Consigneeland";
			selectedConsigneeAddress.OA_PostCode = "1234";
			selectedConsigneeAddress.OA_State = "NSW";

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var convertToStandAloneDeclarationForm = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				convertToStandAloneDeclarationForm.Show();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is FreeTextAddressConversionForm<HVLVConsignment> freeTextConversionForm)
					{
						var similarOrgsGrid = freeTextConversionForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;
						similarOrgsGrid.Select(0);

						AssertEquals("Precondition", selectedConsigneeAddress.PK, ((OrgPatternMatch)similarOrgsGrid.GetFirstSelectedRow()).Address.PK);
						var selectButton = freeTextConversionForm.Controls.Find("SelectButton", true).Single() as ZButton;
						selectButton.PerformClick();
						var saveButton = (freeTextConversionForm.Controls.Find("PostingButtons", true).Single() as ZPostingButtonsUserControl).SaveAndCloseButton;
						saveButton.PerformClick();
					}
				});

				var selectOrDeselectAllButton = convertToStandAloneDeclarationForm.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = convertToStandAloneDeclarationForm.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Precondition:", typeof(FreeTextAddressConversionForm<HVLVConsignment>), ZFormModaliser.LastFormShownDialogForTest.GetType());
				CombineAssertions("Expected consignee org linked", () =>
				{
					AssertEquals("Consignee has been converted to an org", true, consignment.ConsigneeIsOrganisation);
					AssertEquals("Consignee org address matched selected address", selectedConsigneeAddress.PK, consignment.HVC_OA_ConsigneeAddress);
				});

				var promptConvertToOrg = "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?";
				var promptConvertToSADAgain = "New organization(s) have been linked to the selected consignments. Please click 'Convert' again to create Stand Alone Declarations.";

				CombineAssertions("Check required messages were displayed", () =>
				{
					Assert("Expected a message prompt for users to convert their shipper and consignees into organisations", UnitTestUserNotification.Instance.PreviousMessages.Any(m => string.Equals(m.Text, promptConvertToOrg)));
					Assert("Expected a message that asks the user to hit Convert again after converting and save new orgs", UnitTestUserNotification.Instance.PreviousMessages.Any(m => string.Equals(m.Text, promptConvertToSADAgain)));
				});
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WhenUserChoosesNotToConvertShipperToOrg_ThenContinueWithConvertingConsignmentToDeclarations()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			BuildImportConsignment(consignment, shipment.PK, NZAddress.PK, ZGuid.Empty, HVLVReleaseStatus.Held);

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Expected a message that the consignments have been converted to stand alone declarations", "Consignments have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Expected declaration reference field for the consignment to be populated", consignment.DeclarationReferenceForDisplay);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclaration_WhenUserChoosesNotToConvertConsigneeToOrg_ThenContinueWithConvertingConsignmentToDeclarations()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew();
			BuildImportConsignment(consignment, shipment.PK, ZGuid.Empty, AUAddress.PK, HVLVReleaseStatus.Held);

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Expected a message that the consignments have been converted to stand alone declarations", "Consignments have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Expected declaration reference field for the consignment to be populated", consignment.DeclarationReferenceForDisplay);
			}
		}

		#region Tests for when we can merge declarations for consignments that have the same consignee

		public void TestGivenImportConsignmentsWithSameConsignee_WhenConvertDeclarations_ThenDisplayConsignmentMergePrompt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
				arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

				var consignment1 = Factory.New<HVLVConsignment>();
				var consignment2 = Factory.New<HVLVConsignment>();

				consignment1.Items.AddNew();
				consignment2.Items.AddNew();

				BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
				BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);

				consignment1.HVC_RN_NKShipperCountryCode = CountryCodes.NewZealand;
				consignment2.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;

				Factory.Save();

				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
				{
					form.Show();
					var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
					selectOrDeselectAllButton.PerformClick();

					var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
					convertButton.PerformClick();

					AssertEquals("Expected a prompt to merge the consignment1 and consignment2 declarations", "Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
				}
			}
		}

		public void TestGivenImportConsignmentsWithSameConsigneeButClearedStatus_WhenConvertDeclarations_DoNotDisplayConsignmentMergePrompt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
				arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

				var consignment1 = Factory.New<HVLVConsignment>();
				var consignment2 = Factory.New<HVLVConsignment>();

				consignment1.Items.AddNew();
				consignment2.Items.AddNew();

				BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
				BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Cleared);

				Factory.Save();

				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
				{
					form.Show();
					var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
					selectOrDeselectAllButton.PerformClick();

					var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
					convertButton.PerformClick();

					AssertEquals("No prompt for merging consignments", "Consignments have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGivenExportConsignmentsWithSameConsignee_WhenConvertDeclarations_ThenDisplayConsignmentMergePrompt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD"; 

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
				arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

				var consignment1 = Factory.New<HVLVConsignment>();
				var consignment2 = Factory.New<HVLVConsignment>();

				consignment1.Items.AddNew();
				consignment2.Items.AddNew();

				BuildExportConsignment(consignment1, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.Held);
				BuildExportConsignment(consignment2, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.Held);

				Factory.Save();

				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
				{
					form.Show();
					var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
					selectOrDeselectAllButton.PerformClick();

					var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
					convertButton.PerformClick();

					AssertEquals("Expected a prompt to merge the consignment1 and consignment2 declarations", "Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
				}
			}
		}

		public void TestGivenExportConsignmentsWithSameConsigneeButClearedStatus_WhenConvertDeclarations_DoNotDisplayConsignmentMergePrompt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
				arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

				var consignment1 = Factory.New<HVLVConsignment>();
				var consignment2 = Factory.New<HVLVConsignment>();

				consignment1.Items.AddNew();
				consignment2.Items.AddNew();

				BuildExportConsignment(consignment1, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.Held);
				BuildExportConsignment(consignment2, shipment.PK, AUAddress.PK, NZAddress.PK, HVLVReleaseStatus.Cleared);

				Factory.Save();

				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
				{
					form.Show();
					var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
					selectOrDeselectAllButton.PerformClick();

					var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
					convertButton.PerformClick();

					AssertEquals("No prompt for merging consignments", "Consignments have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGivenConsignmentsWithSameConsignee_WhenConvertDeclarations_AndUserChoosesToMergesConsignment_ThenAllConsignmentsWithCommonConsigneeHaveSameDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrgB = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrgB.OH_Code = "TESCONB";
			consigneeOrgB.OH_FullName = "Consignee Testing Company B";
			var consigneeAddressB = consigneeOrgB.Addresses.AddNew();
			consigneeAddressB.OA_RN_NKCountryCode = CountryCodes.Australia;
			consigneeAddressB.OA_Code = "ADDRESSB";
			consigneeAddressB.OA_Address1 = "B Consignee Avenue";
			consigneeAddressB.OA_City = "Consignee City B";

			var consignment1 = Factory.New<HVLVConsignment>();
			var consignment2 = Factory.New<HVLVConsignment>();
			var consignment3 = Factory.New<HVLVConsignment>();
			var consignment4 = Factory.New<HVLVConsignment>();
			var consignment5 = Factory.New<HVLVConsignment>();
			var consignment6 = Factory.New<HVLVConsignment>();

			consignment1.Items.AddNew();
			consignment2.Items.AddNew();
			consignment3.Items.AddNew();
			consignment4.Items.AddNew();
			consignment5.Items.AddNew();
			consignment6.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment4, shipment.PK, NZAddress.PK, consigneeAddressB.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment5, shipment.PK, NZAddress.PK, consigneeAddressB.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment6, shipment.PK, NZAddress.PK, consigneeAddressB.PK, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				CombineAssertions("Precondition: Consignment should have no declaration created", () =>
				{
					AssertNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment3:", consignment3.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment4:", consignment4.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment5:", consignment5.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment6:", consignment6.StandAloneDeclarationForCurrentCompany);
				});

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();
				Factory.Save();

				AssertEquals("Expected a prompt to merge all the consignment declarations", "Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
				AssertEquals("Expected a prompt that the consignments have been converted to stand alone declarations", "Consignments have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);

				var declaration1 = consignment1.StandAloneDeclarationForCurrentCompany;
				var declaration2 = consignment4.StandAloneDeclarationForCurrentCompany;

				CombineAssertions("Expected declaration to be created:", () =>
				{
					AssertNotNull(declaration1);
					AssertNotNull(declaration2);
				});

				AssertNotEquals("Expected both declarations should be different", declaration1, declaration2);

				CombineAssertions("Expected consigments to have the same declaration because they have the same consignee as consignment1:", () =>
				{
					AssertEquals("consignment2:", declaration1, consignment2.StandAloneDeclarationForCurrentCompany);
					AssertEquals("consignment3:", declaration1, consignment3.StandAloneDeclarationForCurrentCompany);
				});

				CombineAssertions("Expected consigments to have the same declaration reference because they have the same consignee as consignment4:", () =>
				{
					AssertEquals("consignment5:", declaration2, consignment5.StandAloneDeclarationForCurrentCompany);
					AssertEquals("consignment6:", declaration2, consignment6.StandAloneDeclarationForCurrentCompany);
				});
			}
		}

		public void TestGivenConsignmentsWithSameConsignee_WhenConvertDeclarations_AndUserChoosesNotToMerge_AllConsignmentsHaveDifferentDeclarations()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consigneeOrgB = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrgB.OH_Code = "TESCONB";
			consigneeOrgB.OH_FullName = "Consignee Testing Company B";
			var consigneeAddressB = consigneeOrgB.Addresses.AddNew();
			consigneeAddressB.OA_RN_NKCountryCode = CountryCodes.Australia;
			consigneeAddressB.OA_Code = "ADDRESSB";
			consigneeAddressB.OA_Address1 = "B Consignee Avenue";
			consigneeAddressB.OA_City = "Consignee City B";

			var consignment1 = Factory.New<HVLVConsignment>();
			var consignment2 = Factory.New<HVLVConsignment>();
			var consignment3 = Factory.New<HVLVConsignment>();
			var consignment4 = Factory.New<HVLVConsignment>();

			consignment1.Items.AddNew();
			consignment2.Items.AddNew();
			consignment3.Items.AddNew();
			consignment4.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, NZAddress.PK, consigneeAddressB.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment4, shipment.PK, NZAddress.PK, consigneeAddressB.PK, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				CombineAssertions("Precondition: Consignment should have no declaration created", () =>
				{
					AssertNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment3:", consignment3.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment4:", consignment4.StandAloneDeclarationForCurrentCompany);
				});

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();
				Factory.Save();

				AssertEquals("Expected a prompt to merge all the consignment declarations", "Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
				AssertEquals("Expected a prompt that the consignments have been converted to stand alone declarations", "Consignments have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);

				CombineAssertions("Expected consigments to have a declaration:", () =>
				{
					AssertNotNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
					AssertNotNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
					AssertNotNull("consignment3:", consignment3.StandAloneDeclarationForCurrentCompany);
					AssertNotNull("consignment4:", consignment4.StandAloneDeclarationForCurrentCompany);
				});

				AssertEquals("Expected consigments to have different declarations from each other:", 4, header.Consignments.Select(c => c.StandAloneDeclarationForCurrentCompany).Distinct().Count());
			}
		}

		public void TestGivenConsignmentsWithNoOrgs_AndSameConsignee_WhenConvertDeclarations_ThenDisplayPromptForOrgMatchingFirst()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.Items.AddNew();

			var consignment2 = Factory.New<HVLVConsignment>();
			consignment2.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, ZGuid.Empty, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, ZGuid.Empty, AUAddress.PK, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();
				
				AssertEquals("Expected a prompt match with missing shipper org to display first", "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?", UnitTestUserNotification.Instance.PreviousMessages[3].Text);
				AssertEquals("Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
			}
		}

		public void TestGivenConsignmentsWithMissingOrgs_AndSameConsignee_WhenConvertDeclarations_AndUserCreatesNewOrgs_ThenDisplayPromptToSave()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			NZAddress.OA_PostCode = "2345";
			NZAddress.OA_State = "AUK";
			AUAddress.OA_PostCode = "1234";
			AUAddress.OA_State = "NSW";

			var consignment1 = Factory.New<HVLVConsignment>();
			var consignment2 = Factory.New<HVLVConsignment>();
			var consignment3 = Factory.New<HVLVConsignment>();
			consignment3.HVC_ShipperName = "Shipper Company";
			consignment3.HVC_ShipperAddress1 = "1234 Shipper Street";
			consignment3.HVC_ShipperCity = "Shipper City";

			consignment1.Items.AddNew();
			consignment2.Items.AddNew();
			consignment3.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, ZGuid.Empty, AUAddress.PK, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var convertToStandAloneDeclarationsForm = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				convertToStandAloneDeclarationsForm.Show();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is FreeTextAddressConversionForm<HVLVConsignment> freeTextAddressConversionForm)
					{
						var similarOrgsGrid = freeTextAddressConversionForm.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;
						similarOrgsGrid.Select(0);

						var selectButton = freeTextAddressConversionForm.Controls.Find("SelectButton", true).Single() as ZButton;
						selectButton.PerformClick();

						var saveButton = (freeTextAddressConversionForm.Controls.Find("PostingButtons", true).Single() as ZPostingButtonsUserControl).SaveAndCloseButton;
						saveButton.PerformClick();
					}
				});

				var selectOrDeselectAllButton = convertToStandAloneDeclarationsForm.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				AssertEquals("Precondition: consignment3 should not have an official consignee", false, consignment3.ShipperIsOrganisation);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = convertToStandAloneDeclarationsForm.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Precondition:", typeof(FreeTextAddressConversionForm<HVLVConsignment>), ZFormModaliser.LastFormShownDialogForTest.GetType());

				CombineAssertions("Expected consignee org linked", () =>
				{
					AssertEquals("Expected consignment3 consignee has been converted to an org", true, consignment3.ConsigneeIsOrganisation);
					AssertEquals("Expected consignment3 consignee org address matches proposed address", AUAddress.PK, consignment3.HVC_OA_ConsigneeAddress);
				});

				var promptConvertToOrg = "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?";
				var promptConvertToSADAgain = "New organization(s) have been linked to the selected consignments. Please click 'Convert' again to create Stand Alone Declarations.";

				CombineAssertions("Check required messages were displayed", () =>
				{
					Assert("Expected a message prompt for users to convert their shipper and consignees into organisations", UnitTestUserNotification.Instance.PreviousMessages.Any(m => string.Equals(m.Text, promptConvertToOrg)));
					Assert("Expected a message that asks the user to hit Convert again after converting and save new orgs", UnitTestUserNotification.Instance.PreviousMessages.Any(m => string.Equals(m.Text, promptConvertToSADAgain)));
				});
			}
		}

		public void TestGivenConsignmentsWithMissingOrgs_AndSameConsignee_WhenConvertDeclarations_AndUserDoesNotCreateNewOrgs_ThenDisplayPromptToMergeDeclarations()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.Items.AddNew();

			var consignment2 = Factory.New<HVLVConsignment>();
			consignment2.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, ZGuid.Empty, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);

			consignment1.HVC_ConsigneeName = "Consignee Testing Company A";
			consignment1.HVC_ConsigneeAddress1 = "A Consignee Avenue";
			consignment1.HVC_ConsigneeCity = "Consignee City A";

			consignment2.HVC_ConsigneeName = "Consignee Testing Company A";
			consignment2.HVC_ConsigneeAddress1 = "A Consignee Avenue";
			consignment2.HVC_ConsigneeCity = "Consignee City A";

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var selectOrDeselectAllButton = form.Controls.Find("ButtonSelectOrDeselectAll", true)[0] as ZButton;
				selectOrDeselectAllButton.PerformClick();

				AssertEquals("Precondition: consignment1 should not have an official consignee", false, consignment1.ShipperIsOrganisation);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();

				AssertEquals("Expected a message prompt for users to convert their shipper and consignees into organisations", "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?", UnitTestUserNotification.Instance.PreviousMessages[3].Text);
				AssertEquals("Expected a prompt to merge consignments with common consignees", "Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
			}
		}

		public void TestGivenConsignmentsWithSameConsignee_WhenConvertDeclarations_ThenOnlyMergeSelectedConsignments()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment1 = Factory.New<HVLVConsignment>();
			var consignment2 = Factory.New<HVLVConsignment>();
			var consignment3 = Factory.New<HVLVConsignment>();

			consignment1.Items.AddNew();
			consignment2.Items.AddNew();
			consignment3.Items.AddNew();

			BuildImportConsignment(consignment1, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment2, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignment3, shipment.PK, NZAddress.PK, AUAddress.PK, HVLVReleaseStatus.Held);

			Factory.Save();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(header))
			{
				form.Show();
				var consignmentGrid = form.Controls.Find("GridConsignments", true)[0] as ZGrid;
				var consignmentsToDeclarations = consignmentGrid.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().ToArray();

				var consignmentToDeclaration1 = consignmentsToDeclarations[0];
				var consignmentToDeclaration2 = consignmentsToDeclarations[1];
				var consignmentToDeclaration3 = consignmentsToDeclarations[2];
				consignmentToDeclaration1.Convert = true;
				consignmentToDeclaration2.Convert = true;
				consignmentToDeclaration3.Convert = false;

				CombineAssertions("Precondition: Consignment should have no declaration created", () =>
				{
					AssertNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
					AssertNull("consignment3:", consignment3.StandAloneDeclarationForCurrentCompany);
				});

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertButton = form.Controls.Find("ButtonConvert", true)[0] as ZButton;
				convertButton.PerformClick();
				Factory.Save();

				AssertEquals("Expected a prompt to merge all the consignment declarations", "Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?", UnitTestUserNotification.Instance.PreviousMessages[2].Text);
				AssertEquals("Expected a prompt that the consignments have been converted to stand alone declarations", "Consignments have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);

				CombineAssertions("Expected declaration references to be not empty or null:", () =>
				{
					AssertNotNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
					AssertNotNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
				});

				AssertEquals("Expected consignment1 and consignment2 to have the same declaration reference because they have the same consignee:", consignment1.StandAloneDeclarationForCurrentCompany, consignment2.StandAloneDeclarationForCurrentCompany);
				AssertNull("Expected consignment3 not to have a declaration reference as it wasn't selected to be converted:", consignment3.StandAloneDeclarationForCurrentCompany);
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipperOrg.OH_Code = "TESSHIP";
			shipperOrg.OH_FullName = "Shipper Company";
			NZAddress = shipperOrg.Addresses.AddNew();
			NZAddress.OA_RN_NKCountryCode = CountryCodes.NewZealand;
			NZAddress.OA_Code = "TESSHIPADD";
			NZAddress.OA_Address1 = "1234 Shipper Street";
			NZAddress.OA_City = "Shipper City";

			var consigneeOrgA = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrgA.OH_Code = "TESCONA";
			consigneeOrgA.OH_FullName = "Consignee Testing Company A";
			AUAddress = consigneeOrgA.Addresses.AddNew();
			AUAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			AUAddress.OA_Code = "ADDRESSA";
			AUAddress.OA_Address1 = "A Consignee Avenue";
			AUAddress.OA_City = "Consignee City A";
		}

		OrgAddress NZAddress;
		OrgAddress AUAddress;

		void BuildImportConsignment(HVLVConsignment consignment, ZGuid shipmentPK, ZGuid shipperAddress, ZGuid consigneeAddress, string importReleaseStatus)
		{
			BuildConsignment(consignment, shipmentPK, shipperAddress, consigneeAddress);
			consignment.HVC_ImportReleaseStatus = importReleaseStatus;
			consignment.HVC_ReleaseStatus = importReleaseStatus;
		}

		void BuildExportConsignment(HVLVConsignment consignment, ZGuid shipmentPK, ZGuid shipperAddress, ZGuid consigneeAddress, string exportReleaseStatus)
		{
			BuildConsignment(consignment, shipmentPK, shipperAddress, consigneeAddress);
			consignment.HVC_ExportReleaseStatus = exportReleaseStatus;
			consignment.HVC_ReleaseStatus = exportReleaseStatus;
		}

		void BuildConsignment(HVLVConsignment consignment, ZGuid shipmentPK, ZGuid shipperAddress, ZGuid consigneeAddress)
		{
			consignment.HVC_JS_ManifestedOnShipment = shipmentPK;
			consignment.HVC_OA_ShipperAddress = shipperAddress;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress;
		}

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			Factory.Save();

			return new HVLVConsignmentsToStandAloneDeclarationsForm(header);
		}
	}
}
