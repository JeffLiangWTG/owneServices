using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(ZADeclarationUserControl))]
	sealed class DeclarationUserControlTests : BaseCustomsDeclarationUserControlAbstractTest<ZADeclarationUserControl, JobDeclaration>
	{
		public void TestRadioCallSignIsCodeFindBox()
		{
			using (var control = new ZADeclarationUserControl())
			{
				AssertType<RadioCallSignCodeFindBox>("Is RadioCallSignCodeFindBox", control.UZ_RadioCallSignTextBox);
			}
		}

		public void TestEntryInstructionsTabRemoved()
		{
			using (var control = new ZADeclarationUserControl())
			{
				Assert("EntryInstructions Removed", !control.RightTabControl.TabPages.OfType<ZTabPage>().Any(x => x.Text == "Entry Instructions"));
			}
		}

		public void TestMasterCargoCarrierCodeFindBox_Location()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				using (var form = new ZForm(declaration))
				{
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
					using (var control = new ZADeclarationUserControl())
					{
						control.JobDeclaration = declaration;
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 110, true), control.masterCargoCarrierCodeFindBox.Location);
					}
				}
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				using (var form = new ZForm(declaration))
				{
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
					using (var control = new ZADeclarationUserControl())
					{
						control.JobDeclaration = declaration;
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 110, true), control.masterCargoCarrierCodeFindBox.Location);
					}
				}
			}
		}

		public void TestzCodeFindBoxVesselAgent_Visibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				using (var form = new ZForm(declaration))
				{
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
					using (var control = new ZADeclarationUserControl())
					{
						control.JobDeclaration = declaration;
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						Assert(control.zCodeFindBoxVesselAgent.Visible);
					}
				}
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				using (var form = new ZForm(declaration))
				{
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
					using (var control = new ZADeclarationUserControl())
					{
						control.JobDeclaration = declaration;
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						Assert(!control.zCodeFindBoxVesselAgent.Visible);
					}
				}
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				using (var form = new ZForm(declaration))
				{
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
					using (var control = new ZADeclarationUserControl())
					{
						control.JobDeclaration = declaration;
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						Assert(!control.zCodeFindBoxVesselAgent.Visible);
					}
				}
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				using (var form = new ZForm(declaration))
				{
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
					using (var control = new ZADeclarationUserControl())
					{
						control.JobDeclaration = declaration;
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();
						Assert(!control.zCodeFindBoxVesselAgent.Visible);
					}
				}
			}
		}

		public void TestDynamicControlVisibilityOnlyForZA()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
				using (var control = new ZADeclarationUserControl())
				{
					control.JobDeclaration = declaration;
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();
					// Parties
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					AssertEquals("JE_LocationOfGoodsCodeFindBox Visibility", true, control.jE_LocationOfGoodsCodeFindBox.Visible);
					AssertEquals("JE_LocationOfGoodsCodeFindBox Description", true, control.jE_LocationOfGoodsCodeFindBox.ShowDescriptionBox);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					AssertEquals("JE_LocationOfGoodsCodeFindBox Visibility", true, control.jE_LocationOfGoodsCodeFindBox.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
					// Orgs
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
					AssertEquals("MasterCargoCarrier", false, control.masterCargoCarrierCodeFindBox.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("MasterCargoCarrier", true, control.masterCargoCarrierCodeFindBox.Visible);
					AssertEquals("BondedWareHouse Visibility", true, control.BondedWarehouseDocAddressControl.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("MasterCargoCarrier", false, control.masterCargoCarrierCodeFindBox.Visible);
					control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
					AssertEquals("BondedWareHouse Visibility", true, control.BondedWarehouseDocAddressControl.Visible);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
					control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
					AssertEquals("MasterCargoCarrier", true, control.masterCargoCarrierCodeFindBox.Visible);
				}
			}
		}

		public void TestDynamicControlVisibilityPerTransportModeInImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			{
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 680, true);
				using (ZADeclarationUserControl control = new ZADeclarationUserControl())
				{
					control.JobDeclaration = declaration;
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("JE_MasterBillForSeaBoundTextBox", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("VoyageFlightNo", true, control.VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("VoyageFlightNo(Voyage)", 88, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(control.VoyageFlightNoBoundTextBox.Location.Y));
					AssertEquals("VesselFindBox", true, control.VesselBoundFindBox.Visible);
					AssertEquals("UZ_RadioCallSignTextBox", true, control.UZ_RadioCallSignTextBox.Visible);
					AssertEquals("UZ_CarrierTextBox", true, control.uZ_CarrierCodeFindBox.Visible);
					AssertEquals("Transit Monifest", false, control.transitManifestTextBox.Visible);
					AssertEquals("UZ_Trailer1TextBox", false, control.uZ_Trailer1TextBox.Visible);
					AssertEquals("UZ_Trailer2TextBox", false, control.uZ_Trailer2TextBox.Visible);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("JE_MasterBillForSeaBoundTextBox", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("Transit Monifest", false, control.transitManifestTextBox.Visible);
					AssertEquals("VoyageFlightNo", true, control.VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("VoyageFlightNo(Flight)", 88, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(control.VoyageFlightNoBoundTextBox.Location.Y));
					AssertEquals("VesselFindBox", false, control.VesselBoundFindBox.Visible);
					AssertEquals("UZ_RadioCallSignTextBox", false, control.UZ_RadioCallSignTextBox.Visible);
					AssertEquals("UZ_CarrierTextBox", false, control.uZ_CarrierCodeFindBox.Visible);
					AssertEquals("UZ_Trailer1TextBox", false, control.uZ_Trailer1TextBox.Visible);
					AssertEquals("UZ_Trailer2TextBox", false, control.uZ_Trailer2TextBox.Visible);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
					AssertEquals("JE_MasterBillForSeaBoundTextBox", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("Transit Monifest", true, control.transitManifestTextBox.Visible);
					AssertEquals("VesselFindBox", false, control.VesselBoundFindBox.Visible);
					AssertEquals("UZ_RadioCallSignTextBox", false, control.UZ_RadioCallSignTextBox.Visible);
					AssertEquals("UZ_CarrierTextBox", false, control.uZ_CarrierCodeFindBox.Visible);
					AssertEquals("VoyageFlightNo(VechicleRegNo)", true, control.VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("VoyageFlightNo(VechicleRegNo)", 64, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(control.VoyageFlightNoBoundTextBox.Location.Y));
					AssertEquals("UZ_Trailer1TextBox", true, control.uZ_Trailer1TextBox.Visible);
					AssertEquals("UZ_Trailer2TextBox", true, control.uZ_Trailer2TextBox.Visible);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
					AssertEquals("JE_MasterBillForSeaBoundTextBox", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("Transit Manifest", true, control.transitManifestTextBox.Visible);
					AssertEquals("VesselFindBox", false, control.VesselBoundFindBox.Visible);
					AssertEquals("UZ_RadioCallSignTextBox", false, control.UZ_RadioCallSignTextBox.Visible);
					AssertEquals("UZ_CarrierTextBox", false, control.uZ_CarrierCodeFindBox.Visible);
					AssertEquals("UZ_Trailer1TextBox", false, control.uZ_Trailer1TextBox.Visible);
					AssertEquals("UZ_Trailer2TextBox", false, control.uZ_Trailer2TextBox.Visible);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
					AssertEquals("Transit Monifest", true, control.transitManifestTextBox.Visible);
					AssertEquals("UZ_RadioCallSignTextBox", false, control.UZ_RadioCallSignTextBox.Visible);
					AssertEquals("UZ_CarrierTextBox", false, control.uZ_CarrierCodeFindBox.Visible);
					AssertEquals("UZ_Trailer1TextBox", false, control.uZ_Trailer1TextBox.Visible);
					AssertEquals("UZ_Trailer2TextBox", false, control.uZ_Trailer2TextBox.Visible);
				}
			}
		}

		public void TestDynamicControlVisibilityDropEditPaidBy()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			using (ZADeclarationUserControl control = new ZADeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				Assert(control.paidByDropEdit.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				Assert(control.paidByDropEdit.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				Assert(control.paidByDropEdit.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
				Assert(control.paidByDropEdit.Visible);
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				Assert(!control.paidByDropEdit.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				Assert(!control.paidByDropEdit.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				Assert(!control.paidByDropEdit.Visible);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
				Assert(!control.paidByDropEdit.Visible);
			}
		}

		public void TestDynamicControlVisibilityJE_MasterBill()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			using (ZADeclarationUserControl control = new ZADeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				AssertJE_MasterBillVisibility(declaration, control);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				AssertJE_MasterBillVisibility(declaration, control);
			}
		}

		public void TestDynamicControlVisibilityWhenShipmentTypeIsEXW()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			using (ZADeclarationUserControl declarationControl = new ZADeclarationUserControl())
			{
				declarationControl.JobDeclaration = declaration;
				form.Controls.Add(declarationControl);
				form.Show();
				Application.DoEvents();
				var messageDeferralSettings = new AutomaticDeferredSelection()
				{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 1 };
				using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
				{
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					AssertEXWControlsVisibility(declaration, declarationControl, true, true);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					AssertEXWControlsVisibility(declaration, declarationControl, false, true);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					AssertEXWControlsVisibility(declaration, declarationControl, true, true);
				}

				messageDeferralSettings = new AutomaticDeferredSelection()
				{ AllowAutomaticDeferredSelection = false, DaysBeforeETA = 1 };
				using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
				{
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					AssertEXWControlsVisibility(declaration, declarationControl, true, true);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					AssertEXWControlsVisibility(declaration, declarationControl, false, false);
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					AssertEXWControlsVisibility(declaration, declarationControl, true, true);
				}
			}
		}

		public void TestDefaultDetails_NotZAINVDET()
		{
			using var func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false);

			var declaration = Factory.New<JobDeclaration>();
			using var form = new ZForm(declaration);
			using var declarationControl = new ZADeclarationUserControl();

			declarationControl.JobDeclaration = declaration;
			form.Controls.Add(declarationControl);
			form.Show();

			var defaultDetailsGroupBox = declarationControl.FindSingleOrDefault<ZGroupBox>("DefaultDetailsGroupBox");
			AssertNull("DefaultDetailsGroupBox should not be created when ZAINVDET not enabled", defaultDetailsGroupBox);

			var exchangeRateDateControl = declarationControl.FindSingleOrDefault<ZDateEdit>("ExchangeRateDateControl");
			AssertNull("ExchangeRateDateControl should not be created when ZAINVDET not enabled", exchangeRateDateControl);

			var shipmentDetailsGroupBox = declarationControl.FindSingleOrDefault<ZGroupBox>("ShipmentDetailsGroupBox");
			AssertNotNull("ShipmentDetailsGroupBox", shipmentDetailsGroupBox);

			var expectedShipmentDetailsControls = new[]
			{
				"HouseBillParcelPostTextEdit",
				"houseBillIssuedDateDateEdit",
				"uZ_CargoCarrierCodeFindBox",
				"OriginFindBox",
				"JE_ExportDateBoundDateEdit2",
				"FinalDestinationFindBox",
				"JE_DateOfArrivalBoundDateEdit2",
				"jE_GoodsOriginCodeFindBox",
				"rooTypeDropEdit",
				"rulesOfOriginCertificateTextBox",
				"GoodsDescriptionTextBox",
				"OwnersReferenceTextBox",
				"WeightzCalcDropEdit",
				"VolumeCalcDropEdit",
				"TotalNoOfPacksCalcDropEdit",
				"IncoTermDropEdit",
				"IncoTermExplainButton",
				"marksAndNumbersZTextbox",
				"marksAndNumbersStmNotePopupButton",
				"agentsReferenceTextBox",
			};

			CombineAssertions(() =>
			{
				foreach (var expectedShipmentDetailsControl in expectedShipmentDetailsControls)
				{
					var control = declarationControl.FindSingleOrDefault<Control>(expectedShipmentDetailsControl);
					AssertNotNull(expectedShipmentDetailsControl, control);
					if (control != null)
					{
						AssertEquals(expectedShipmentDetailsControl + ".ParentControl", shipmentDetailsGroupBox, control.Parent);
					}
				}
			});
		}

		public void TestDefaultDetails_ZAINVDET()
		{
			using var func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true);

			var declaration = Factory.New<JobDeclaration>();
			using var form = new ZForm(declaration);
			using var declarationControl = new ZADeclarationUserControl();

			declarationControl.JobDeclaration = declaration;
			form.Controls.Add(declarationControl);
			form.Show();

			var defaultDetailsGroupBox = declarationControl.FindSingleOrDefault<ZGroupBox>("DefaultDetailsGroupBox");
			AssertNotNull("DefaultDetailsGroupBox should be created when ZAINVDET is enabled", defaultDetailsGroupBox);

			var exchangeRateDateControl = declarationControl.FindSingleOrDefault<ZDateEdit>("ExchangeRateDateControl");
			AssertNotNull("ExchangeRateDateControl should be created when ZAINVDET is enabled", exchangeRateDateControl);

			var shipmentDetailsGroupBox = declarationControl.FindSingleOrDefault<ZGroupBox>("ShipmentDetailsGroupBox");
			AssertNotNull("ShipmentDetailsGroupBox", shipmentDetailsGroupBox);

			var expectedShipmentDetailsControls = new[]
			{
				"HouseBillParcelPostTextEdit",
				"houseBillIssuedDateDateEdit",
				"uZ_CargoCarrierCodeFindBox",
				"OriginFindBox",
				"JE_ExportDateBoundDateEdit2",
				"FinalDestinationFindBox",
				"JE_DateOfArrivalBoundDateEdit2",
				"WeightzCalcDropEdit",
				"VolumeCalcDropEdit",
				"TotalNoOfPacksCalcDropEdit",
				"marksAndNumbersZTextbox",
				"marksAndNumbersStmNotePopupButton",
			};

			var expectedDefaultDetailsControls = new[]
			{
				"ExchangeRateDateControl",
				"jE_GoodsOriginCodeFindBox",
				"rooTypeDropEdit",
				"rulesOfOriginCertificateTextBox",
				"GoodsDescriptionTextBox",
				"OwnersReferenceTextBox",
				"IncoTermDropEdit",
				"IncoTermExplainButton",
				"agentsReferenceTextBox",
			};

			CombineAssertions(() =>
			{
				foreach (var expectedShipmentDetailsControl in expectedShipmentDetailsControls)
				{
					var control = declarationControl.FindSingleOrDefault<Control>(expectedShipmentDetailsControl);
					AssertNotNull(expectedShipmentDetailsControl, control);
					if (control != null)
					{
						AssertEquals(expectedShipmentDetailsControl + ".ParentControl", shipmentDetailsGroupBox, control.Parent);
					}
				}

				foreach (var expectedDefaultDetailsControl in expectedDefaultDetailsControls)
				{
					var control = declarationControl.FindSingleOrDefault<Control>(expectedDefaultDetailsControl);
					AssertNotNull(expectedDefaultDetailsControl, control);
					if (control != null)
					{
						AssertEquals(expectedDefaultDetailsControl + ".ParentControl", defaultDetailsGroupBox, control.Parent);
					}
				}

				AssertEquals("ExchangeRateDateControl BindingMember", JobDeclaration.Schema.JE_ValuationDate, declarationControl.BindingSource.GetBindingMember(exchangeRateDateControl));
			});
		}

		public void TestMarksAndNumbersPopupRefreshesAndValidatesMarksAndNumbersShort()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(declaration))
			using (var control = new ZADeclarationUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				declaration.JE_MarksAndNumbers = "Line1";
				AssertNoMessageErrors(declaration.JE_MarksAndNumbersShortInfo);
				control.MarksAndNumbersStmNotePopupButton_ClickAndEditSimulation(declaration);
				AssertHasMessageErrors(declaration.JE_MarksAndNumbersShortInfo);
			}
		}

		[ExpectNoExceptions]
		public void TestNoNullReferenceExceptionWhenUZ_IsNonIATAFormatAirWayBillValueChangedAfterDisposed()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_IsNonIATAFormatAirWayBill = true;
			declaration.JE_TransportMode = "AIR";
			Factory.Save();
			using (ZForm form = new ZForm(declaration))
			using (ZADeclarationUserControl control = new ZADeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				AssertEquals(true, control.nonIATAFormatCheckBox.Visible);
				Assert(control.nonIATAFormatCheckBox.Checked);
				control.Dispose();
				declaration.JE_IsNonIATAFormatAirWayBill = false;
			}
		}

		void AssertEXWControlsVisibility(JobDeclaration declaration, ZADeclarationUserControl declarationControl, bool visibility, bool dateOfArrivalVisibility)
		{
			CombineAssertions($"Ex-Bond Controls should be {visibility} for Shipment Type {declaration.JE_MessageType}", () =>
			{
				AssertEquals(visibility, declarationControl.SupplierOrganisationControl.Visible);
				AssertEquals(visibility, declarationControl.jE_LocationOfGoodsCodeFindBox.Visible);
				AssertEquals(visibility, declarationControl.PortOfLoadingFindBox.Visible);
				AssertEquals(visibility, declarationControl.JE_ExportDateBoundDateEdit.Visible);
				AssertEquals(visibility, declarationControl.PortOfDischargeFindBox.Visible);
				AssertEquals(dateOfArrivalVisibility, declarationControl.JE_DateOfArrivalBoundDateEdit.Visible);
			});
		}

		void AssertJE_MasterBillVisibility(JobDeclaration declaration, ZADeclarationUserControl control)
		{
			declaration.JE_IsNonIATAFormatAirWayBill = false;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Assert(!control.nonIATAFormatCheckBox.Visible);
			Assert(!control.JE_MasterBillForAirBoundTextBox.Visible);
			Assert(!control.jE_MasterBillForAirBoundNonIATATextBox.Visible);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert(control.nonIATAFormatCheckBox.Visible);
			Assert(control.JE_MasterBillForAirBoundTextBox.Visible);
			Assert(!control.jE_MasterBillForAirBoundNonIATATextBox.Visible);
			declaration.JE_IsNonIATAFormatAirWayBill = true;
			Assert(control.nonIATAFormatCheckBox.Visible);
			Assert(!control.JE_MasterBillForAirBoundTextBox.Visible);
			Assert(control.jE_MasterBillForAirBoundNonIATATextBox.Visible);
		}

		sealed class ZADeclarationUserControlForTest : ZADeclarationUserControl
		{
			public void MarksAndNumbersStmNotePopupButton_ClickAndEditSimulation(JobDeclaration declaration)
			{
				var note = declaration.Notes.FindByDescription(ZArchitecture.Business.PredefinedNoteTypes.Instance.MarksAndNumbers.Description).First();
				note.ST_NoteText = "line1\n\nline3";
				MarksAndNumbersStmNotePopupButton_NoteHasChangesChanged();
			}
		}
	}
}
