using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(BaseCustomsDeclarationUserControl))]
	sealed class BaseCustomsDeclarationUserControlBaseOnlyTest : BaseCustomsDeclarationUserControlAbstractTest<BaseCustomsDeclarationUserControl, BaseJobDeclaration>
	{
		public void TestControls() => CombineAssertions(() =>
		{
			using var control = new BaseCustomsDeclarationUserControl();
			var detailsGroupBox = control.AssertContainsControl<ZGroupBox>("DeclarationDetailsGroupBox");
			_ = detailsGroupBox.AssertContainsControl<ZTextBox>("ExportDeclarationNumberBoundTextBox", x => x
				.WithCaption("Entry Number")
				.WithLocation(94, 16)
			);
			_ = detailsGroupBox.AssertContainsControl<ZTextBox>("StatusTextBox", x => x
				.WithCaption("Status")
				.WithLocation(296, 16)
			);
		});

		public void TestJE_MessageTypeInfo_ValueChanged_SetTransportDetailsLayout()
		{
			var declaration = Factory.New<TestJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var panelLayoutProvider = Mock.Of<IPanelLayoutProvider>(x => x.Layout == new PanelLayout());
			var provider = Mock.Of<IDeclarationFormLayoutProvider>(x => x.GetDeclarationTransportDetailsLayout(It.IsAny<BaseJobDeclaration>()) == panelLayoutProvider);
			var objectHandle = Mock.Of<ObjectHandle>(x => x.GetObject() == provider);
			var hashtable = new Hashtable { { declaration.CountryCode, objectHandle } };
			ObjectFactory.Substitute("DeclarationFormLayoutProviders", hashtable);

			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.TransportDetailsLayoutPanel = null;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertNotNull(control.TransportDetailsLayoutPanel);
			}
		}

		public void TestSeaVisibility_Import()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				CombineAssertions(() =>
				{
					AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit visible", true, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
					AssertEquals("JE_ContainerCountCalcEdit visible", false, decUserControl.JE_ContainerCountCalcEdit.Visible);
					AssertEquals("Container Mode should be visible", true, decUserControl.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Vessel should be visible", true, decUserControl.VesselFindBox.Visible);
				});
			}
		}

		public void TestAirVisibility_Import()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				CombineAssertions(() =>
				{
					AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit visible", true, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
					AssertEquals("JE_ContainerCountCalcEdit visible", false, decUserControl.JE_ContainerCountCalcEdit.Visible);
					AssertEquals("Vessel should not be visible", false, decUserControl.VesselFindBox.Visible);
					AssertEquals("Voyage/FlightNo should be visible", true, decUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("Voyage Box should be where the (not visible) Vessel Find Box was", decUserControl.JE_VoyageFlightNoBoundTextBox.Location.X, decUserControl.VesselFindBox.Location.X);
				});
			}
		}

		public void TestSeaVisibility_Export()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_ExportDate = new ZDateTime(2004, 12, 12);
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				CombineAssertions(() =>
				{
					AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit visible", false, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
					AssertEquals("JE_ContainerCountCalcEdit visible", true, decUserControl.JE_ContainerCountCalcEdit.Visible);
					AssertEquals("Voyage should be visible", true, decUserControl.VesselFindBox.Visible);
				});
				form.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "EXP";

				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit visible", false, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("JE_ContainerCountCalcEdit visible", false, decUserControl.JE_ContainerCountCalcEdit.Visible);
				AssertEquals("Container Mode should not be visible", false, decUserControl.JE_ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("Vessel should not be visible", false, decUserControl.VesselFindBox.Visible);
			}
		}

		public void TestAirVisibility_Export()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_ExportDate = new ZDateTime(2004, 12, 12);
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				CombineAssertions(() =>
				{
					AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit visible", false, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
					AssertEquals("JE_ContainerCountCalcEdit visible", false, decUserControl.JE_ContainerCountCalcEdit.Visible);
					AssertEquals("Container Mode should not be visible", false, decUserControl.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Vessel should not be visible", false, decUserControl.VesselFindBox.Visible);
				});
			}
		}

		public void TestLockingSeaShipmentData_DeclarationReadOnly()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;

				form.CustomsBrokerageUserControl.SetDeclarationReadOnly(true);

				CombineAssertions(() =>
				{
					// fields are not on decUserControl directly (new baglayout)
					AssertVisibleAndReadOnly(decUserControl.DeclarationDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(CommonDeclarationDetailsUserControl.DeclarationNumberTextBox)), true);
					AssertVisibleAndReadOnly(decUserControl.DeclarationDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(CommonDeclarationDetailsUserControl.StatusTextBox)), true);

					AssertVisibleAndReadOnly(decUserControl.ShipmentTypeLayoutPanel.FindSingleOrDefault<ZDropEdit>(nameof(ShipmentTypeUserControl.MessageTypeDropEdit)), true);
					AssertVisibleAndReadOnly(decUserControl.ShipmentTypeLayoutPanel.FindSingleOrDefault<ZDropEdit>(nameof(ShipmentTypeUserControl.TransportModeDropEdit)), true);
					AssertVisibleAndReadOnly(decUserControl.ShipmentTypeLayoutPanel.FindSingleOrDefault<ZDropEdit>(nameof(ShipmentTypeUserControl.ContainerModeDropEdit)), true);

					AssertVisibleAndReadOnly(decUserControl.TransportDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(TransportDetailsPortOfLoadingUserControl.PortOfLoadingFindBox)), true);
					AssertVisibleAndReadOnly(decUserControl.TransportDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(TransportDetailsPortOfDischargeUserControl.PortOfDischargeFindBox)), true);
					AssertVisibleAndReadOnly(decUserControl.TransportDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(TransportDetailsUserControl.OceanBillTextBox)), true);

					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(ShipmentDetailsUserControl.HouseBillParcelPostTextBox)), true);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(ShipmentDetailsOriginUserControl.OriginFindBox)), true);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(ShipmentDetailsFinalDestinationUserControl.FinalDestinationFindBox)), true);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZDateEdit>(nameof(ShipmentDetailsFinalDestinationUserControl.EstimatedArrivalDateEdit)), true);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZDateEdit>(nameof(ShipmentDetailsOriginUserControl.EstimatedDepartureDateEdit)), true);
				});
			}
		}

		public void TestLockingSeaShipmentData_DeclarationEditable()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;

				form.CustomsBrokerageUserControl.SetDeclarationReadOnly(false);

				CombineAssertions(() =>
				{
					AssertVisibleAndReadOnly(decUserControl.PortOfLoadingFindBox, false);
					AssertVisibleAndReadOnly(decUserControl.JE_VoyageFlightNoBoundTextBox, false);
					AssertVisibleAndReadOnly(decUserControl.JE_MasterBillForSeaBoundTextBox, false);

					// fields are not on decUserControl directly (new baglayout)
					AssertVisibleAndReadOnly(decUserControl.DeclarationDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(CommonDeclarationDetailsUserControl.DeclarationNumberTextBox)), true); // should stay read only
					AssertVisibleAndReadOnly(decUserControl.DeclarationDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(CommonDeclarationDetailsUserControl.StatusTextBox)), declaration.JE_EntryStatusDescriptionInfo.ReadOnly);

					AssertVisibleAndReadOnly(decUserControl.ShipmentTypeLayoutPanel.FindSingleOrDefault<ZDropEdit>(nameof(ShipmentTypeUserControl.MessageTypeDropEdit)), declaration.JE_MessageTypeInfo.ReadOnly);
					AssertVisibleAndReadOnly(decUserControl.ShipmentTypeLayoutPanel.FindSingleOrDefault<ZDropEdit>(nameof(ShipmentTypeUserControl.TransportModeDropEdit)), false);

					AssertVisibleAndReadOnly(decUserControl.TransportDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(TransportDetailsPortOfLoadingUserControl.PortOfLoadingFindBox)), false);
					AssertVisibleAndReadOnly(decUserControl.TransportDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(TransportDetailsPortOfDischargeUserControl.PortOfDischargeFindBox)), false);
					AssertVisibleAndReadOnly(decUserControl.TransportDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(TransportDetailsUserControl.OceanBillTextBox)), false);

					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZTextBox>(nameof(ShipmentDetailsUserControl.HouseBillParcelPostTextBox)), false);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(ShipmentDetailsOriginUserControl.OriginFindBox)), false);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZCodeFindBox>(nameof(ShipmentDetailsFinalDestinationUserControl.FinalDestinationFindBox)), false);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZDateEdit>(nameof(ShipmentDetailsFinalDestinationUserControl.EstimatedArrivalDateEdit)), false);
					AssertVisibleAndReadOnly(decUserControl.ShipmentDetailsLayoutPanel.FindSingleOrDefault<ZDateEdit>(nameof(ShipmentDetailsOriginUserControl.EstimatedDepartureDateEdit)), false);
				});
			}
		}

		public void TestBondedWarehouseVisibility()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.RightTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.OrganisationsTabPage;

				CombineAssertions(() =>
				{
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					AssertEquals("Import", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
					AssertEquals("Export", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
					AssertEquals("Drawback", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
				});
			}
		}

		public void TestVisibleDeniedPartyScreeningControls_NoShipment()
		{
			AssertScreenStatusVisible(true, false);
			AssertScreenStatusVisible(false, true);

			void AssertScreenStatusVisible(bool enableCompliance, bool expected)
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new BaseJobDeclarationForm(declaration))
				{
					form.Show();
					var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;

					CombineAssertions(() =>
					{
						AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit", false, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
						AssertEquals("ScreenButton", expected, decUserControl.ScreenButton.Visible);
						AssertEquals("ScreeningStatusDropEdit", expected, decUserControl.ScreeningStatusDropEdit.Visible);
					});
				}
			}
		}

		public void TestVisibleDeniedPartyScreeningControls_LinkedShipment()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;

				CombineAssertions(() =>
				{
					AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit", false, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
					AssertEquals("ScreenButton", false, decUserControl.ScreenButton.Visible);
					AssertEquals("ScreeningStatusDropEdit", false, decUserControl.ScreeningStatusDropEdit.Visible);
				});
			}
		}

		public void TestJE_ContainerModeLabel_IsNonTransportDeclarationType()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			var declaration = declarationMock.Object;
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				CombineAssertions(() =>
				{
					declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
					AssertEquals("Mail", false, control.JE_ContainerModeBoundDropDownEditVisibleForTesting);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("Sea", false, control.JE_ContainerModeBoundDropDownEditVisibleForTesting);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("Air", false, control.JE_ContainerModeBoundDropDownEditVisibleForTesting);
				});
			}
		}

		public void TestJE_ContainerModeLabel_NotIsNonTransportDeclarationType()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				CombineAssertions(() =>
				{
					control.JobDeclaration = declaration;
					declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
					AssertEquals("Mail", true, control.JE_ContainerModeBoundDropDownEditVisibleForTesting);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("Sea", true, control.JE_ContainerModeBoundDropDownEditVisibleForTesting);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("Air", false, control.JE_ContainerModeBoundDropDownEditVisibleForTesting);
				});
			}
		}

		public void TestDeclarationDetailsLayoutPanel()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("Declaration Details layout is used", control.DeclarationDetailsLayoutPanel);
					AssertEquals("Other controls removed", control.DeclarationDetailsGroupBox.Controls.Count, 1);
					AssertLessThan("RightTabControl should not extend beyond the bottom of the form", control.RightTabControl.Bottom, control.Height);
				});
			}
		}

		public void TestDeclarationDetailsUserControls()
		{
			var declaration = Factory.NewWithValidTestData<TestJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("Declaration Details layout not used", control.DeclarationDetailsLayoutPanel);
					AssertGreaterThan("Older Controls used", control.DeclarationDetailsGroupBox.Controls.Count, 1);
				});
			}
		}

		public void TestShipmentTypeLayoutPanel()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("Shipment Type layout is used", control.ShipmentTypeLayoutPanel);
					AssertEquals("Other controls removed", control.ShipmentTypeGroupBox.Controls.Count, 1);
				});
			}
		}

		public void TestShipmentTypeUserControls()
		{
			var declaration = Factory.NewWithValidTestData<TestJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("Shipment Type layout not used", control.ShipmentTypeLayoutPanel);
					AssertGreaterThan("Older Controls used", control.ShipmentTypeGroupBox.Controls.Count, 1);
				});
			}
		}

		public void TestTransportDetailsLayoutPanel()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("Transport details layout is used", control.TransportDetailsLayoutPanel);
					AssertEquals("Other controls removed", control.TransportDetailsGroupBox.Controls.Count, 1);
				});
			}
		}

		public void TestTransportDetailsUserControls()
		{
			var declaration = Factory.NewWithValidTestData<TestJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("Transport details layout not used", control.TransportDetailsLayoutPanel);
					AssertGreaterThan("Older Controls used", control.TransportDetailsGroupBox.Controls.Count, 1);
				});
			}
		}

		public void TestShipmentDetailsLayoutPanel()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					var shipmentDetailsLayoutPanel = control.ShipmentDetailsLayoutPanel;
					AssertNotNull("Shipment details layout is used", shipmentDetailsLayoutPanel);
					AssertEquals("TabStop", true, shipmentDetailsLayoutPanel.TabStop);
					AssertEquals("Other controls removed", control.ShipmentDetailsGroupBox.Controls.Count, 1);
				});
			}
		}

		public void TestShipmentDetailsUserControls()
		{
			var declaration = Factory.NewWithValidTestData<TestJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull("Shipment details layout not used", control.ShipmentDetailsLayoutPanel);
					AssertGreaterThan("Older controls Used", control.ShipmentDetailsGroupBox.Controls.Count, 1);
				});
			}
		}

		public void TestDynamicTransportControlsSea_Visibility()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				CombineAssertions(() =>
				{
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("Voyage Textbox Visibility", true, control.VesselFindBox.Visible);
					AssertEquals("Ocean Bill Textbox Visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
				});
			}
		}

		public void TestDynamicTransportControlsRoad_Visibility()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
				CombineAssertions(() =>
				{
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("Vessel Textbox Visibility", false, control.VesselFindBox.Visible);
					AssertEquals("Ocean Bill Textbox Visibility", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
				});
			}
		}

		public void TestDynamicTransportControlsAir_Visibility()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				CombineAssertions(() =>
				{
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("Vessel Textbox Visibility", false, control.VesselFindBox.Visible);
					AssertEquals("Ocean Bill Textbox Visibility", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("MasterBill Textbox Visibility", true, control.JE_MasterBillForAirBoundTextBox.Visible);
				});
			}
		}

		public void TestDynamicTransportControlsInvalid_Visibility()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				declaration.JE_TransportMode = "ZZZ";
				CombineAssertions(() =>
				{
					AssertEquals("Voyage Textbox Visibility", false, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("Vessel Textbox Visibility", false, control.VesselFindBox.Visible);
					AssertEquals("Ocean Bill Textbox Visibility", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
				});
			}
		}

		public void TestJE_MasterBillForSeaBoundTextBoxInvisibleForMail()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
				AssertEquals("JE_MasterBillForSeaBoundTextBox Visibility", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
			}
		}

		public void TestShipmentCustomFieldsControlLabel()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var decUserControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var shipmentCustomFP = decUserControl.ShipmentCustomFieldsPage;
				var shipemntControlOnCustomFP = shipmentCustomFP.Controls["ZTabPage"];

				var tabControl = (ZTabControl)decUserControl.Controls.Find("RightTabControl", true)[0];
				var customTabPage = (ZTabPage)decUserControl.Controls.Find("ShipmentCustomFieldsPage", true)[0];
				tabControl.SelectedTab = customTabPage;
				var labelText = ((ProcessTemplateCustomFieldsControl)customTabPage.Controls["shipmentCustomFieldsControl1"]).NothingSetupMessageLabelText;
				AssertEquals("ShipmentCustomFieldsControl1", "To make use of this tab, please setup customized fields against Workflow Templates.", labelText);
			}
		}

		public void TestSaveButtonsAreNotEnabledOnOpeningExistingRecord()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.Load<BaseJobDeclaration>(testDec.PK);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				AssertEquals("Save Button is currently enabled, and should be disabled", false, form.oPostingButtonsUserControl.SaveAndCloseButton.Enabled);
			}
		}

		public void TestAttachedOrdersVisible_NoShipment()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				AssertNotNull(form.CustomsBrokerageUserControl.FindSingle<ZTabPage>("OrdersTabPage"));
			}
		}

		public void TestAttachedOrdersVisible_LinkedShipment()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				AssertNull(form.CustomsBrokerageUserControl.FindSingleOrDefault<ZTabPage>("OrdersTabPage"));
			}
		}

		public void TestUseDeniedPartyScreeningStatusDropEdit()
		{
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				AssertType<DeniedPartyScreeningStatusDropEdit>(control.ScreeningStatusDropEdit);
			}
		}

		public void TestChangeCommissionedDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "GBLON";

			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				AssertEquals("AIR", declaration.JE_TransportMode);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				declaration.JE_RL_NKOrigin = "UAIEV";
				AssertEquals("UAIEV", declaration.JE_RL_NKOrigin);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				declaration.JE_RL_NKFinalDestination = "USLAX";
				AssertEquals("USLAX", declaration.JE_RL_NKFinalDestination);
			}
		}

		public void TestLoadOrganisationsTabPage()
		{
			var declaration = Factory.NewWithValidTestData<TestJobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.RightTabControl.SelectedTab = control.OrganisationsTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Top Panel should be used", true, control.OrganisationsTabPage.Controls.Contains(control.OrganisationsTopPanel));
					AssertEquals("OrganisationsUserControl should not be used", false, control.OrganisationsTabPage.Controls.Contains(control.OrganisationsUserControl));
				});
			}
		}

		public void TestLoadDynamicOrganisationsTabPage()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.RightTabControl.SelectedTab = control.OrganisationsTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Top Panel should not be used", false, control.OrganisationsTabPage.Controls.Contains(control.OrganisationsTopPanel));
					AssertEquals("OrganisationDetailsLayoutPanel should be used", true, control.OrganisationsTabPage.Controls.Contains(control.OrganisationDetailsLayoutPanel));
				});
			}
		}

		public void TestJE_MasterBillForSeaBoundTextBoxVisibleForExpressCourier_NotExpress()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();

				Env.Registry.IsExpress = false;
				CombineAssertions(() =>
				{
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					form.CustomsBrokerageUserControl.JobDeclaration = declaration;
					AssertEquals("JE_MasterBillForSeaBoundTextBox Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("JE_MasterBillForAirBoundTextBox InVisible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);

					declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
					form.CustomsBrokerageUserControl.JobDeclaration = declaration;
					AssertEquals("JE_MasterBillForSeaBoundTextBox Invisible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("JE_MasterBillForAirBoundTextBox Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				});
			}
		}

		public void TestJE_MasterBillForSeaBoundTextBoxVisibleForExpressCourier_Express()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();

				Env.Registry.IsExpress = true;
				CombineAssertions(() =>
				{
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					form.CustomsBrokerageUserControl.JobDeclaration = declaration;
					AssertEquals("JE_MasterBillForSeaBoundTextBox Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("JE_MasterBillForAirBoundTextBox Invisible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);

					declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
					form.CustomsBrokerageUserControl.JobDeclaration = declaration;
					AssertEquals("JE_MasterBillForSeaBoundTextBox Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("JE_MasterBillForAirBoundTextBox Invisible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				});
			}
		}

		public void TestNoUserNotificationShownDuringSavingTransaction()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = "EDT";
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = MasterFiles.Business.WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<_DataSource.JobDeclaration.JE_OverrideFreightDefaults>";
			action.PQ_FieldValue = "N";
			Factory.Save();
			shipment.JS_GoodsDescription = "aaaaa";
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.FireSaveButton();
			}

			AssertNull("should suppress user notification", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestNoUserNotificationShownDuringSavingTransactionFromClose()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_GoodsDescription = "Shipment description";
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = "EDT";
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = MasterFiles.Business.WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<_DataSource.JobDeclaration.JE_OverrideFreightDefaults>";
			action.PQ_FieldValue = "N";
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_OverrideFreightDefaults = true;
				declaration.JE_GoodsDescription = "Declaration description";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save? Yes/No/Cancel
				form.CancelButton.PerformClick();
			}

			CombineAssertions(() =>
			{
				AssertNotContains("User notification was surpressed", "Removing the override will reset your customs declaration data", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Data will changed by the workflow", "Shipment description", declaration.JE_GoodsDescription);
			});
		}

		void AssertVisibleAndReadOnly(Control control, bool isExpectedReadOnly)
		{
			var controlName = control.Name;
			AssertEquals($"{controlName} Visible", true, control.Visible);
			AssertEquals($"{controlName} ReadOnly", isExpectedReadOnly, control.GetReadOnly());
		}

		class TestJobDeclaration : BaseJobDeclaration
		{
			public TestJobDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
		class BaseCustomsDeclarationUserControlForTest : BaseCustomsDeclarationUserControl
		{
			protected override bool HasCommissions() => true;
		}
	}
}
