using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ConfigUserControl))]
	sealed class ConfigUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		public void TestExportCodeListToExcel()
		{
			AssertExportCodeListToExcelWithSecurityCheck(true);
			AssertExportCodeListToExcelWithSecurityCheck(false);

			void AssertExportCodeListToExcelWithSecurityCheck(bool hasSecurity)
			{
				var org = Factory.New<OrgHeader>();
				var rawExportSecurity = OrgHeader.ExportExcelCheckPoint.IsAllowed;

				try
				{
					using (var testForm = new ZForm(org))
					using (var testControl = new ConfigUserControl())
					{
						testForm.Controls.Add(testControl);
						testForm.Show();
						testControl.SetDataBinding(org, "");
						AssertNotNull(testControl.ExportCodeListToExcelMenuItem);
						AssertEquals("Export Code Type List", testControl.ExportCodeListToExcelMenuItem.Caption);
						testControl.CusCodesGrid.ContextMenu.DoPopup();
						AssertCollectionContains(testControl.ExportCodeListToExcelMenuItem, testControl.CusCodesGrid.ContextMenu.MenuItems);

						OrgHeader.ExportExcelCheckPoint.IsAllowed = hasSecurity;
						UnitTestUserNotification.Instance.ClearMessages();

						if (hasSecurity)
						{
							AssertNoExceptionThrown(testControl.ExportCodeListToExcelMenuItem.PerformClick);
							using (IExcelInterface excelInterface = ExcelInterfaceFactory.New())
							{
								excelInterface.LoadExcelFile(ExcelExporter.LastExportedFileNameStaticForTest);
								using (IExcelWorkSheet workSheet = excelInterface.WorkSheets[0])
								{
									CombineAssertions(delegate
									{
										AssertEquals(OrgRegistrationCodeTypeReader.CodeCollection.Count + 1, workSheet.RowCount);
										AssertEquals(6, workSheet.ColumnCount);
										AssertEquals("Type", workSheet[0, 0]);
										AssertEquals("Description", workSheet[0, 1]);
										AssertEquals("Country", workSheet[0, 2]);
										AssertEquals("Primary", workSheet[0, 3]);
										AssertEquals("Category", workSheet[0, 4]);
										AssertEquals("Local Business Number", workSheet[0, 5]);
									});

									var lBNCodeForAU = RefCountry.LoadFromCountryCode(Factory, "AU").LocalBusinessRegNoCodeType;
									var lBNCodeForUS = RefCountry.LoadFromCountryCode(Factory, "US").LocalBusinessRegNoCodeType;
									Assert("There must be lines in Excel file other than the header", workSheet.RowCount > 1);

									for (var i = 0; i < workSheet.RowCount; i++)
									{
										var isLBN = workSheet[i, 5].ToString() == "Yes";
										var type = workSheet[i, 0].ToString();
										AssertEquals(isLBN, (type.Equals(lBNCodeForAU) || type.Equals(lBNCodeForUS)));
									}
								}
							}

							AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						}
						else
						{
							AssertNoExceptionThrown(testControl.ExportCodeListToExcelMenuItem.PerformClick);
							AssertEquals(OrgHeader.ExportExcelCheckPoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
				finally
				{
					if (!ExcelExporter.LastExportedFileNameStaticForTest.IsEmpty && File.Exists(ExcelExporter.LastExportedFileNameStaticForTest))
					{
						File.Delete(ExcelExporter.LastExportedFileNameStaticForTest);
					}

					OrgHeader.ExportExcelCheckPoint.IsAllowed = rawExportSecurity;
				}
			}
		}

		public void TestCusCodesGrid_ExportToExcel()
		{
			AssertExportToExcelWithSecurityCheck(true);
			AssertExportToExcelWithSecurityCheck(false);

			void AssertExportToExcelWithSecurityCheck(bool hasSecurity)
			{
				OrgHeader.ExportExcelCheckPoint.IsAllowed = hasSecurity;
				UnitTestUserNotification.Instance.ClearMessages();

				var rawExportSecurity = OrgHeader.ExportExcelCheckPoint.IsAllowed;
				var org = Factory.New<OrgHeader>();
				org.CustomsCodes.AddNew(Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "XYZ 123");

				try
				{
					using (var testForm = new ZForm(org))
					using (var testControl = new ConfigUserControl())
					{
						testForm.Controls.Add(testControl);
						testForm.Show();
						testControl.SetDataBinding(org, "");

						testControl.CusCodesGrid.ContextMenu.DoPopup();
						if (hasSecurity)
						{
							AssertNoExceptionThrown(testControl.CusCodesGrid.ExportAllColumnsToExcelMenuItem.PerformClick);
							AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						}
						else
						{
							AssertNoExceptionThrown(testControl.CusCodesGrid.ExportAllColumnsToExcelMenuItem.PerformClick);
							AssertEquals(OrgHeader.ExportExcelCheckPoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
				finally
				{
					if (!ExcelExporter.LastExportedFileNameStaticForTest.IsEmpty && File.Exists(ExcelExporter.LastExportedFileNameStaticForTest))
					{
						File.Delete(ExcelExporter.LastExportedFileNameStaticForTest);
					}

					OrgHeader.ExportExcelCheckPoint.IsAllowed = rawExportSecurity;
				}
			}
		}

		[RequiresSTA]
		public void TestCountryOverrideSet()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCusCode code = organisation.CustomsCodes.AddNew();
			code.OK_CodeType = "CSC";

			using (ZForm testForm = new ZForm(organisation))
			{
				using (ConfigUserControl testControl = new ConfigUserControl())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
					testControl.SetDataBinding(organisation, "");

					code.OK_RN_NKCodeCountry = "NZ";
					ZMultiControlColumnStyle colStyle = (ZMultiControlColumnStyle)testControl.CusCodesGrid.Columns["SecuredCustomsRegNo"].ColumnStyle;
					colStyle.EditControl.ControlType = FieldType.TextCodeFindBox;

					AssertNull(((ZPopupFindBox)colStyle.EditControl.CurrentEditor).GetCountryCode?.Invoke());
					testControl.CusCodesGrid_FindBoxColumnModuleShowing(null, new FindBoxColumnModuleShowingEventArgs(colStyle, code, null));
					AssertEquals("NZ", ((ZPopupFindBox)colStyle.EditControl.CurrentEditor).GetCountryCode());
				}
			}
		}

		#region test readonly config tabs

		#region test US readonly config tabs

		[RequiresSTA]
		public void TestUSReadAccessWithViewTrueModifyFalse()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = true;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = false;

			AssertPermissions("USDefaultsTabPage", ControllerIDs.Customs.US.OrganisationDetailsPlugIn);
		}

		public void TestUSReadAccessWithViewTrueModifyTrue()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = true;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = true;

			AssertPermissions("USDefaultsTabPage", ControllerIDs.Customs.US.OrganisationDetailsPlugIn);
		}

		public void TestUSReadAccessWithViewFalseModifyTrue()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = false;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = true;

			AssertPermissions("USDefaultsTabPage", ControllerIDs.Customs.US.OrganisationDetailsPlugIn);
		}

		[RequiresSTA]
		public void TestUSReadAccessWithViewFalseModifyFalse()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = false;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = false;

			AssertPermissions("USDefaultsTabPage", ControllerIDs.Customs.US.OrganisationDetailsPlugIn);
		}

		#endregion

		#region test Canada readonly config tabs

		public void TestCanadaReadAccessWithViewTrueModifyFalse()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = false;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = false;

			AssertPermissions("CanadaTabPage", ControllerIDs.Customs.CA.OrganisationDetailsPlugIn);
		}

		public void TestCanadaReadAccessWithViewTrueModifyTrue()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = false;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = false;

			AssertPermissions("CanadaTabPage", ControllerIDs.Customs.CA.OrganisationDetailsPlugIn);
		}

		public void TestCanadaReadAccessWithViewFalseModifyTrue()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = false;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = false;

			AssertPermissions("CanadaTabPage", ControllerIDs.Customs.CA.OrganisationDetailsPlugIn);
		}

		public void TestCanadaReadAccessWithViewFalseModifyFalse()
		{
			Env.Security.OrgDetailsViewCountryDefaults.IsAllowed = false;
			Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = false;

			AssertPermissions("CanadaTabPage", ControllerIDs.Customs.CA.OrganisationDetailsPlugIn);
		}

		#endregion

		void AssertPermissions(string tabName, ControllerID controllerID)
		{
			var org = Factory.New<OrgHeader>();

			using (var form = new ZForm(org))
			using (var control = new ConfigUserControl())
			{
				form.Controls.Add(control);

				control.SetDataBinding(org, "");
				form.Show();
				Application.DoEvents();
				var tabControl = control.FindSingle<ZTemplateTabControl>("ConfigTabControl");
				var tab = tabControl.TabPages[tabName] as ZTabPage;
				tabControl.SelectedTab = tab;

				var controllerViewPermission = tabControl.PlugIns.GetPlugIn(controllerID).Controller.GetCheckPointForView(control.CurrentDataItem as BusinessObject);
				var controllerEditPermission = tabControl.PlugIns.GetPlugIn(controllerID).Controller.GetCheckPointForEdit(control.CurrentDataItem as BusinessObject);

				AssertEquals(Env.Security.OrgDetailsViewCountryDefaults.IsAllowed, controllerViewPermission.IsAllowed);
				AssertEquals(Env.Security.OrgConfigModifyCountryDefaults.IsAllowed, controllerEditPermission.IsAllowed);

				AssertEquals(Env.Security.OrgDetailsViewCountryDefaults.HumanReadableName, controllerViewPermission.HumanReadableName);
				AssertEquals(Env.Security.OrgConfigModifyCountryDefaults.HumanReadableName, controllerEditPermission.HumanReadableName);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestClickingOnGridAndClosingFormDoesntCausePluginsToLoadTwice()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCusCode code = organisation.CustomsCodes.AddNew();
			code.OK_CodeType = "CSC";

			var form = new ZForm(organisation);
			using (ConfigUserControl control = new ConfigUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(organisation, "");

				control.CusCodesGrid.Focus();
				form.Close();
			}
		}

		public void TestGermanyTabIsAvailableToEritrea()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			using (ZForm testForm = new ZForm(Factory.New<OrgHeader>()))
			using (ConfigUserControl testControl = new ConfigUserControl())
			{
				testForm.Controls.Add(testControl);
				testForm.Show();
				AssertNotNull("GermanyTabPage", testControl.ConfigTabControl.Find(control => control.Name == "GermanyTabPage").SingleOrDefault());
			}
		}

		public void TestUSOrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.UnitedStates, ControllerIDs.Customs.US.OrganisationDetailsPlugIn.Name);

		public void TestCAOrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.Canada, ControllerIDs.Customs.CA.OrganisationDetailsPlugIn.Name);

		public void TestDeferrmentAccountNumberOrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.Eritrea, ControllerIDs.Customs.DE.OrganisationDetailsPlugIn.Name);

		public void TestCNOrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.China, ControllerIDs.Customs.US.OrganisationDetailsPlugIn.Name);

		[RequiresSTA]
		public void TestFROrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.France, ControllerIDs.Customs.FR.OrganisationDetailsPlugIn.Name);

		[RequiresSTA]
		public void TestTWOrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.Taiwan, ControllerIDs.Customs.TW.OrganisationDetailsPlugIn.Name);

		[RequiresSTA]
		public void TestKROrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.KoreaSouth, ControllerIDs.Customs.KR.OrganisationDetailsPlugIn.Name);

		public void TestBROrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.Brazil, ControllerIDs.Customs.BR.OrganisationDetailsPlugIn.Name);

		public void TestINOrganisationPlugInAdded() => AssertPlugInAdded(Core.Constants.CountryCodes.India, ControllerIDs.Customs.IN.OrganisationDetailsPlugIn.Name);

		public void TestXmlUniversalGroupBox()
		{
			var org = Factory.New<OrgHeader>();
			using (var form = new ZForm(org))
			using (var control = new ConfigUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(org, "");
				form.Show();
				Application.DoEvents();

				var configTabControl = form.FindSingleOrDefault<ZTabControl>("ConfigTabControl");
				AssertNotNull("Pre-condition", configTabControl);
				var ediCommsTabPage = form.FindSingleOrDefault<ZTabPage>("EDICommsTabPage");
				AssertNotNull("Pre-condition", ediCommsTabPage);
				configTabControl.SelectedTab = ediCommsTabPage;

				var ediCommunicationModesGroupBox = ediCommsTabPage.FindSingleOrDefault<ZGroupBox>("EDICommunicationModesGroupBox");
				AssertNotNull("Pre-condition", ediCommunicationModesGroupBox);

				var ediCommGridPanel = ediCommsTabPage.FindSingleOrDefault<ZPanel>("EdiCommGridPanel");
				AssertNotNull("Pre-condition", ediCommGridPanel);
				var commModeDetailsPanel = ediCommGridPanel.FindSingleOrDefault<ZPanel>("CommModeDetailsPanel");
				AssertNotNull("Pre-condition", commModeDetailsPanel);
				var ediCommsGrid = ediCommGridPanel.FindSingleOrDefault<ZGrid>("EdiCommsGrid");
				AssertNotNull("Pre-condition", ediCommsGrid);

				var xmlUniversalGroupBox = commModeDetailsPanel.FindSingleOrDefault<ZGroupBox>("XmlUniversalGroupBox");
				AssertNotNull("Pre-condition", xmlUniversalGroupBox);

				AssertEquals("XmlUniversalGroupBox should be hide when EDI communications mode does not exist", false, xmlUniversalGroupBox.Visible);

				var communicationsModeForTransaction = org.EDICommunicationsModes.AddNew();
				communicationsModeForTransaction.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsModeForTransaction.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;

				var communicationsModeForSchedule = org.EDICommunicationsModes.AddNew();
				communicationsModeForSchedule.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsModeForSchedule.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule;

				var communicationsModeForShipment = org.EDICommunicationsModes.AddNew();
				communicationsModeForShipment.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsModeForShipment.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;

				ediCommsGrid.CurrentRowIndex = 0;
				AssertEquals("XmlUniversalGroupBox visibility for xml universal transaction", true, xmlUniversalGroupBox.Visible);
				AssertEquals("XmlUniversalGroupBox caption for xml universal transaction", "XML Universal Transaction", xmlUniversalGroupBox.CaptionResourceString.Caption);

				ediCommsGrid.CurrentRowIndex = 1;
				AssertEquals("XmlUniversalGroupBox visibility for xml universal schedule", false, xmlUniversalGroupBox.Visible);
				AssertEquals("XmlUniversalGroupBox caption for xml universal schedule does not need to be configured when fileFormat is not XUS nor XUT", "XML Universal Transaction", xmlUniversalGroupBox.CaptionResourceString.Caption);

				ediCommsGrid.CurrentRowIndex = 2;
				AssertEquals("xmlUniversalGroupBox visibility for xml universal shipment", true, xmlUniversalGroupBox.Visible);
				AssertEquals("XmlUniversalGroupBox caption for xml universal shipment", "XML Universal Shipment", xmlUniversalGroupBox.CaptionResourceString.Caption);

				org.EDICommunicationsModes.RemoveAndDeleteAll();
				AssertEquals("XmlUniversalGroupBox should be hide when EDI communications mode does not exist", false, xmlUniversalGroupBox.Visible);
			}
		}

		public void TestEdiClientVisibility()
		{
			AssertEdiClientVisibility(true);
		}

		void AssertEdiClientVisibility(bool shouldShow)
		{
			var org = Factory.New<OrgHeader>();
			using (var form = new ZForm(org))
			using (var control = new ConfigUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(org, "");
				form.Show();
				Application.DoEvents();

				var configTabControl = form.FindSingleOrDefault<ZTabControl>("ConfigTabControl");
				AssertNotNull("Pre-condition", configTabControl);
				var ediCommsTabPage = form.FindSingleOrDefault<ZTabPage>("EDICommsTabPage");
				AssertNotNull("Pre-condition", ediCommsTabPage);
				configTabControl.SelectedTab = ediCommsTabPage;

				var ediCommGridPanel = ediCommsTabPage.FindSingleOrDefault<ZPanel>("EdiCommGridPanel");
				AssertNotNull("Pre-condition", ediCommGridPanel);
				var commModeDetailsPanel = ediCommGridPanel.FindSingleOrDefault<ZPanel>("CommModeDetailsPanel");
				AssertNotNull("Pre-condition", commModeDetailsPanel);
				var ediCommsGrid = ediCommGridPanel.FindSingleOrDefault<ZGrid>("EdiCommsGrid");
				AssertNotNull("Pre-condition", ediCommsGrid);

				AssertNotNull("Communication Settings Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZGroupBox>("CommunicationSettingsBox"));
				AssertNotNull("Module Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("ModuleDropEdit"));
				AssertNotNull("CommTransport Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("CommTransportDropEdit"));
				AssertNotNull("CommDirection Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("CommDirectionDropEdit"));
				AssertNotNull("Transport Mode Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("TransportModeDropEdit"));

				AssertNotNull("File Information Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZGroupBox>("FileInformationBox"));
				AssertNotNull("Email Subject Text Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("EmailSubjectTextBox"));
				AssertNotNull("File Name Text Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("FileNameTextBox"));
				AssertNotNull("File Format Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("FileFormatDropEdit"));

				AssertNotNull("Receiver Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZGroupBox>("ReceiverBox"));
				AssertNotNull("EdiParty Find Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZGuidFindBox>("EdiPartyFindBox"));
				AssertNotNull("Destination Text Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("DestinationTextBox"));
				AssertNotNull("Recipient Role Drop Edit Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("RecipientRoleDropEdit"));
				AssertNotNull("SenderVANID Text Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("SenderVANIDTextBox"));
				AssertNotNull("ReceiverVANID Text Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("ReceiverVANIDTextBox"));
				AssertNotNull("MessageVAN Find Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZGuidFindBox>("MessageVANFindBox"));

				AssertNotNull("FTP Group Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZGroupBox>("FTPGroupBox"));
				AssertNotNull("FTP Port Number Calc Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZCalcEdit>("FTPPortNumberCalcEdit"));
				AssertNotNull("FTP Login Name Text Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("FTPLoginNameTextBox"));
				AssertNotNull("FTP Password Text Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("FTPPasswordTextBox"));

				AssertNotNull("Message Metadata Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZGroupBox>("MessageInfotmationBox"));
				AssertNotNull("Event Code Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("EventCodeDropEdit"));
				AssertNotNull("Event Reference Condition Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("EventReferenceConditionDropEdit"));
				AssertNotNull("Purpose Code Drop Edit Box Exists", commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("PurposeCodeDropEdit"));

				AssertEquals("EDI client box visibility", false, commModeDetailsPanel.FindSingleOrDefault<ZGuidFindBox>("EdiPartyFindBox").Visible);
				AssertNull("EDI client column visibility", ediCommsGrid.GetColumnStyle("CommunicationParty"));
			}

			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				using (var form = new ZForm(org))
				using (var control = new ConfigUserControl())
				{
					form.Controls.Add(control);
					control.SetDataBinding(org, "");
					form.Show();
					Application.DoEvents();

					var configTabControl = form.FindSingleOrDefault<ZTabControl>("ConfigTabControl");
					AssertNotNull("Pre-condition", configTabControl);
					var ediCommsTabPage = form.FindSingleOrDefault<ZTabPage>("EDICommsTabPage");
					AssertNotNull("Pre-condition", ediCommsTabPage);
					configTabControl.SelectedTab = ediCommsTabPage;

					var ediCommGridPanel = ediCommsTabPage.FindSingleOrDefault<ZPanel>("EdiCommGridPanel");
					AssertNotNull("Pre-condition", ediCommGridPanel);
					var commModeDetailsPanel = ediCommGridPanel.FindSingleOrDefault<ZPanel>("CommModeDetailsPanel");
					AssertNotNull("Pre-condition", commModeDetailsPanel);
					var ediCommsGrid = ediCommGridPanel.FindSingleOrDefault<ZGrid>("EdiCommsGrid");
					AssertNotNull("Pre-condition", ediCommsGrid);

					AssertEquals("EDI client box visibility", shouldShow, commModeDetailsPanel.FindSingleOrDefault<ZGuidFindBox>("EdiPartyFindBox").Visible);
					AssertEquals("EDI client column visibility", shouldShow, !ediCommsGrid.GetColumnStyle("CommunicationParty").IsUnavailable);
				}
			}
		}

		public void TestCusCodesGrid_OrgCusCodeValidity()
		{
			var organisation = Factory.New<OrgHeader>();
			var code = organisation.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code.OK_CodeType = "CSC";

			using (var form = new ZForm(organisation))
			using (var control = new ConfigUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(organisation, "");

				control.CusCodesGrid.Focus();
				AssertEquals(true, control.CusCodesGrid.GetColumnStyle(OrgCusCodeValidity.Schema.VerificationStatus).IsVisible);
				AssertEquals(true, control.CusCodesGrid.GetColumnStyle(OrgCusCodeValidity.Schema.LastVerifiedTime).IsVisible);
				AssertEquals(true, control.CusCodesGrid.GetColumnStyle(OrgCusCodeValidity.Schema.VerificationAuthority).IsVisible);

				control.CusCodesGrid.SelectAllElements();
				control.CusCodesGrid.ContextMenu.DoPopup();

				AssertEquals(ZString.Empty, code.VerificationStatus);
				var verifiedMenuItem = control.CusCodesGrid.ContextMenu.MenuItems.FindByText("Mark as Verified", false);
				AssertEquals("Mark as Verified menu is NOT visible for AU registration number", false, verifiedMenuItem.Visible);
				var nonVerifiedMenuItem = control.CusCodesGrid.ContextMenu.MenuItems.FindByText("Mark as Not Verified", false);
				AssertEquals("Mark as Not Verified menu is NOT visible for AU registration number", false, nonVerifiedMenuItem.Visible);

				code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
				control.CusCodesGrid.SelectAllElements();
				control.CusCodesGrid.ContextMenu.DoPopup();

				verifiedMenuItem = control.CusCodesGrid.ContextMenu.MenuItems.FindByText("Mark as Verified", false);
				AssertEquals("Mark as Verified menu is visible for US registration number which is not verified yet.", true, verifiedMenuItem.Visible);
				nonVerifiedMenuItem = control.CusCodesGrid.ContextMenu.MenuItems.FindByText("Mark as Not Verified", false);
				AssertEquals("Mark as Not Verified menu is NOT visible for US registration number which is not verified yet.", false, nonVerifiedMenuItem.Visible);

				verifiedMenuItem.PerformClick();
				AssertEquals("VERIFIED", code.VerificationStatus);

				control.CusCodesGrid.SelectAllElements();
				control.CusCodesGrid.ContextMenu.DoPopup();
				verifiedMenuItem = control.CusCodesGrid.ContextMenu.MenuItems.FindByText("Mark as Verified", false);
				AssertEquals("Mark as Verified menu is NOT visible for US registration number which is has been verified.", false, verifiedMenuItem.Visible);
				nonVerifiedMenuItem = control.CusCodesGrid.ContextMenu.MenuItems.FindByText("Mark as Not Verified", false);
				AssertEquals("Mark as Not Verified menu is visible for US registration number which has been verified.", true, nonVerifiedMenuItem.Visible);

				nonVerifiedMenuItem.PerformClick();
				AssertEquals("NOT VERIFIED", code.VerificationStatus);
			}
		}

		public void TestDestinationLabelTextForMultipleEDIComms()
		{
			var org = Factory.New<OrgHeader>();
			using (var form = new ZForm(org))
			using (var control = new ConfigUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(org, "");
				form.Show();
				Application.DoEvents();

				var configTabControl = form.FindSingleOrDefault<ZTabControl>("ConfigTabControl");
				AssertNotNull("Pre-condition", configTabControl);
				var ediCommsTabPage = form.FindSingleOrDefault<ZTabPage>("EDICommsTabPage");
				AssertNotNull("Pre-condition", ediCommsTabPage);
				configTabControl.SelectedTab = ediCommsTabPage;

				var ediCommunicationModesGroupBox = ediCommsTabPage.FindSingleOrDefault<ZGroupBox>("EDICommunicationModesGroupBox");
				AssertNotNull("Pre-condition", ediCommunicationModesGroupBox);

				var ediCommGridPanel = ediCommsTabPage.FindSingleOrDefault<ZPanel>("EdiCommGridPanel");
				AssertNotNull("Pre-condition", ediCommGridPanel);
				var commModeDetailsPanel = ediCommGridPanel.FindSingleOrDefault<ZPanel>("CommModeDetailsPanel");
				AssertNotNull("Pre-condition", commModeDetailsPanel);
				var ediCommsGrid = ediCommGridPanel.FindSingleOrDefault<ZGrid>("EdiCommsGrid");
				AssertNotNull("Pre-condition", ediCommsGrid);

				var destinationTextBox = commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("DestinationTextBox");
				var commTransportDropEdit = commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("CommTransportDropEdit");
				AssertNotNull("Pre-condition", destinationTextBox);

				AssertEquals("Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				var communicationsMode1 = org.EDICommunicationsModes.AddNew();
				communicationsMode1.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

				var communicationsMode2 = org.EDICommunicationsModes.AddNew();
				communicationsMode2.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode2.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
				communicationsMode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;

				var communicationsMode3 = org.EDICommunicationsModes.AddNew();
				communicationsMode3.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode3.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode3.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

				var communicationsMode4 = org.EDICommunicationsModes.AddNew();
				communicationsMode4.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Receive;
				communicationsMode4.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode4.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;

				ediCommsGrid.CurrentRowIndex = 0;
				AssertEquals("eHub Client ID", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				ediCommsGrid.CurrentRowIndex = 1;
				AssertEquals("Recipient ID", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				ediCommsGrid.CurrentRowIndex = 2;
				AssertEquals("xT Client ID", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				ediCommsGrid.CurrentRowIndex = 3;
				AssertEquals("Origin Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				org.EDICommunicationsModes.RemoveAndDeleteAll();
				commTransportDropEdit.Focus();
				AssertEquals("Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		[RequiresSTA]
		public void TestDestinationLabelTextForChangeDirection()
		{
			var org = Factory.New<OrgHeader>();
			using (var form = new ZForm(org))
			using (var control = new ConfigUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(org, "");
				form.Show();
				Application.DoEvents();

				var configTabControl = form.FindSingleOrDefault<ZTabControl>("ConfigTabControl");
				AssertNotNull("Pre-condition", configTabControl);
				var ediCommsTabPage = form.FindSingleOrDefault<ZTabPage>("EDICommsTabPage");
				AssertNotNull("Pre-condition", ediCommsTabPage);
				configTabControl.SelectedTab = ediCommsTabPage;

				var ediCommunicationModesGroupBox = ediCommsTabPage.FindSingleOrDefault<ZGroupBox>("EDICommunicationModesGroupBox");
				AssertNotNull("Pre-condition", ediCommunicationModesGroupBox);

				var ediCommGridPanel = ediCommsTabPage.FindSingleOrDefault<ZPanel>("EdiCommGridPanel");
				AssertNotNull("Pre-condition", ediCommGridPanel);
				var commModeDetailsPanel = ediCommGridPanel.FindSingleOrDefault<ZPanel>("CommModeDetailsPanel");
				AssertNotNull("Pre-condition", commModeDetailsPanel);
				var ediCommsGrid = ediCommGridPanel.FindSingleOrDefault<ZGrid>("EdiCommsGrid");
				AssertNotNull("Pre-condition", ediCommsGrid);

				var destinationTextBox = commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("DestinationTextBox");
				var commTransportDropEdit = commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("CommTransportDropEdit");
				AssertNotNull("Pre-condition", destinationTextBox);

				AssertEquals("Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				var communicationsMode1 = org.EDICommunicationsModes.AddNew();
				communicationsMode1.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Receive;
				communicationsMode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;

				AssertEquals("Origin Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				communicationsMode1.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				AssertEquals("Dest. Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestDestinationLabelTextForChangeTransport()
		{
			var org = Factory.New<OrgHeader>();
			using (var form = new ZForm(org))
			using (var control = new ConfigUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(org, "");
				form.Show();
				Application.DoEvents();

				var configTabControl = form.FindSingleOrDefault<ZTabControl>("ConfigTabControl");
				AssertNotNull("Pre-condition", configTabControl);
				var ediCommsTabPage = form.FindSingleOrDefault<ZTabPage>("EDICommsTabPage");
				AssertNotNull("Pre-condition", ediCommsTabPage);
				configTabControl.SelectedTab = ediCommsTabPage;

				var ediCommunicationModesGroupBox = ediCommsTabPage.FindSingleOrDefault<ZGroupBox>("EDICommunicationModesGroupBox");
				AssertNotNull("Pre-condition", ediCommunicationModesGroupBox);

				var ediCommGridPanel = ediCommsTabPage.FindSingleOrDefault<ZPanel>("EdiCommGridPanel");
				AssertNotNull("Pre-condition", ediCommGridPanel);
				var commModeDetailsPanel = ediCommGridPanel.FindSingleOrDefault<ZPanel>("CommModeDetailsPanel");
				AssertNotNull("Pre-condition", commModeDetailsPanel);
				var ediCommsGrid = ediCommGridPanel.FindSingleOrDefault<ZGrid>("EdiCommsGrid");
				AssertNotNull("Pre-condition", ediCommsGrid);

				var destinationTextBox = commModeDetailsPanel.FindSingleOrDefault<ZTextBox>("DestinationTextBox");
				var commTransportDropEdit = commModeDetailsPanel.FindSingleOrDefault<ZDropEdit>("CommTransportDropEdit");
				AssertNotNull("Pre-condition", destinationTextBox);

				AssertEquals("Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				var communicationsMode1 = org.EDICommunicationsModes.AddNew();
				communicationsMode1.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

				AssertEquals("eHub Client ID", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				AssertEquals("Recipient ID", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;
				AssertEquals("xT Client ID", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertEquals("Dest. Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				AssertEquals("Dest. Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				communicationsMode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				AssertEquals("Dest. Address", destinationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		protected override OrganisationSecurityContainerControl GetNewControlForTesting() => new ConfigUserControl();

		protected override string[] SecurityContainerPropertiesEnabledForThisControl => new string[] { "IsModifyConfig", "IsModifyConfigBrandsAndCompanyNames", "IsModifyConfigEDICodeMapping", "IsModifyConfigGeneral", "IsModifyConfigRegistrationNumbers" };

		void AssertPlugInAdded(string countryCode, string plugInName)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (var testForm = new ZForm(Factory.New<OrgHeader>()))
			using (var testControl = new ConfigUserControl())
			{
				testForm.Controls.Add(testControl);
				testForm.Show();
				AssertNotNull($@"ConfigTabControl.PlugIns.GetPlugIn(""{plugInName}"")", testControl.ConfigTabControl.PlugIns.GetPlugIn(new ClientControllerID(plugInName)));
			}
		}
	}
}
