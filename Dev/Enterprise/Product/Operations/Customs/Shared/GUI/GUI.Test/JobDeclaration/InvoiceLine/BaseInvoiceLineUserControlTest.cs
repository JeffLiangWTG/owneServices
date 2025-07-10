using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(BaseInvoiceLineUserControl))]
	sealed class BaseInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnStyleInfo_OwnerCountry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.InvoiceLines.AddNew();

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var columnStyleInfo = userControl.CusContainerInvoiceLineGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().FirstOrDefault(x => x.ColumnName == "OwnerCountry");
					CombineAssertions(() =>
					{
						AssertNotNull(columnStyleInfo);
						AssertEquals("Owner Country", columnStyleInfo.CaptionResourceString.Caption);
						AssertEquals(false, columnStyleInfo.IsVisible);
						AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90), columnStyleInfo.Width);
					});
				}
			}
		}

		public void TestUniversalTariffColumnStyleInfo_GetDataGrouping()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<ZString>("DefaultDataGroupingForTariffsCore").Returns("EUN");
			var declaration = declarationMock.Object;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new BaseInvoiceLineUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals("EUN", ((Universal.GUI.TariffColumnStyleInfo)userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(userControl.TariffColumnName)).GetDataGrouping());
			}
		}

		public void TestRefreshDifferentInvoiceLineDetailsLayoutWhenMessageTypeChanges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new BaseInvoiceLineUserControlForTest())
			{
				form.Controls.Add(userControl);
				form.Show();

				bool isGetNewInvoiceLineDetailsPanelLayoutCalled = false;
				userControl.DoWhenAccessingGetNewInvoiceLineDetailsPanelLayout = () => { isGetNewInvoiceLineDetailsPanelLayoutCalled = true; };

				declaration.JE_MessageType = "CHA";

				AssertEquals("GetNewInvoiceLineDetailsPanelLayout is called", true, isGetNewInvoiceLineDetailsPanelLayoutCalled);
			}
		}

		public void TestFilterMenu()
		{
			var jobDeclartion = Factory.New<BaseJobDeclaration>();
			jobDeclartion.Invoices.AddNew();
			jobDeclartion.Invoices.AddNew();
			jobDeclartion.Invoices.AddNew();
			using (var form = new CustomsDeclarationFormForTest(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var filterMenu = (ZMenuItem)form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems[4];
				AssertEquals("Show/Hide Filters", filterMenu.Caption);
				filterMenu.PerformClick();

				var invoiceSequenceNumberFilter = (ModuleNumberRangeFilter)form.InvoiceUserControl.FilterBusinessObject[InvoiceLineFilterConstants.InvoiceSequenceNumber];
				AssertEquals(1M, invoiceSequenceNumberFilter.Property1);
				invoiceSequenceNumberFilter.ClickUpArrow();
				AssertEquals(2M, invoiceSequenceNumberFilter.Property1);
				invoiceSequenceNumberFilter.ClickUpArrow();
				AssertEquals(3M, invoiceSequenceNumberFilter.Property1);
				invoiceSequenceNumberFilter.ClickUpArrow();
				AssertEquals(4M, invoiceSequenceNumberFilter.Property1);
				AssertHasError(invoiceSequenceNumberFilter.Property1Info, "Please enter a value less than or equal to 3.");
				invoiceSequenceNumberFilter.ClickDownArrow();
				AssertEquals(3M, invoiceSequenceNumberFilter.Property1);
				AssertNoError(invoiceSequenceNumberFilter.Property1Info, "Please enter a value less than or equal to 3.");
				invoiceSequenceNumberFilter.ClickDownArrow();
				AssertEquals(2M, invoiceSequenceNumberFilter.Property1);
				invoiceSequenceNumberFilter.ClickDownArrow();
				AssertEquals(1M, invoiceSequenceNumberFilter.Property1);
				invoiceSequenceNumberFilter.ClickDownArrow();
				AssertEquals(0M, invoiceSequenceNumberFilter.Property1);
				AssertHasError(invoiceSequenceNumberFilter.Property1Info, "Please enter a value greater than or equal to 1.");
				invoiceSequenceNumberFilter.ClickUpArrow();
				AssertEquals(1M, invoiceSequenceNumberFilter.Property1);
				AssertNoError(invoiceSequenceNumberFilter.Property1Info, "Please enter a value greater than or equal to 1.");

				var brandFilter = form.InvoiceUserControl.FilterBusinessObject[InvoiceLineFilterConstants.Brand];
				AssertNotNull("CustomsInvoiceLinesBoundGrid should contain Brand filter", brandFilter);

				var modelFilter = form.InvoiceUserControl.FilterBusinessObject[InvoiceLineFilterConstants.Model];
				AssertNull("CustomsInvoiceLinesBoundGrid should not contain Model filter", modelFilter);
			}
		}

		public void TestColumnCustomisationIsOnlyAffectedByOrgsWithCustomFieldsDefined()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Org1";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Org2";
			OrgCustomLabels customLabel = org2.CustomLabels.AddNew();
			customLabel.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
			customLabel.OT_Caption = "Hair Colour";

			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = "Org3";
			OrgCustomLabels customLabe2 = org3.CustomLabels.AddNew();
			customLabe2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
			customLabe2.OT_Caption = "Container Colour";

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew(); // So that Invoice Lines tab is allowed to be shown.

			ColumnCustomisationTestHelper testHelper = new ColumnCustomisationTestHelper(declaration);

			testHelper.SetColumnVisible(org1, false, false, true);
			// JI_InvoiceQuantity will be visible because its group column, JI_InvoiceUQ, is visible.
			testHelper.AssertColumnVisible(org1, false, true, true);

			testHelper.SetColumnVisible(null, false, true, false);
			testHelper.AssertColumnVisible(null, false, true, false);
			testHelper.AssertColumnVisible(org1, false, true, false);
			testHelper.AssertColumnVisible(org2, false, true, false);

			testHelper.SetColumnVisible(org2, true, false, true);
			testHelper.AssertColumnVisible(null, false, true, false);
			testHelper.AssertColumnVisible(org1, false, true, false);
			testHelper.AssertColumnVisible(org2, true, false, true);

			testHelper.SetColumnVisible(org3, true, true, false);
			testHelper.AssertColumnVisible(null, false, true, false);
			testHelper.AssertColumnVisible(org1, false, true, false);
			testHelper.AssertColumnVisible(org2, true, false, true);
			testHelper.AssertColumnVisible(org3, true, true, false);
		}

		public void TestGridLayoutSameWhetherStandAloneOrPluggedIntoShipmentAsPlugIn()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				BaseInvoiceLineUserControl invoiceLineControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				invoiceLineControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLineSchema.JI_CC.Name].IsVisible = false;
				invoiceLineControl.CustomsInvoiceLinesBoundGrid.Columns.HasLayoutChanged = true;
			}

			Freight.Forwarding.Business.ForwardingShipment shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.JE_JS = shipment.PK;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (Freight.Forwarding.GUI.ShipmentForm shipmentForm = new Freight.Forwarding.GUI.ShipmentForm(shipment))
			{
				shipmentForm.Show();

				shipmentForm.PlugIns.SelectPlugInTabPage(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				BaseBrokeragePlugIn plugin = (BaseBrokeragePlugIn)shipmentForm.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);

				plugin.OnGUIShown();
				BaseCustomsBrokerageUserControl userControl = plugin.UserControl as BaseCustomsBrokerageUserControl;

				userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;

				BaseInvoiceLineUserControl invoiceLineControl = userControl.InvoiceLinesUserControl;

				AssertNotNull("InvoiceLineControl", invoiceLineControl);
				AssertNotNull("invoice line grid", invoiceLineControl.CustomsInvoiceLinesBoundGrid);
				AssertNotNull("JI_CC column exists in a grid", invoiceLineControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLineSchema.JI_CC.Name]);
				AssertEquals("JI_CC should be invisible as it was configured in the stand-alone form", false, invoiceLineControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLineSchema.JI_CC.Name].IsVisible);
			}
		}

		public void TestTopPanelDoesnOverlapBottomPanelAfterCantCreateInvoiceLinesLabelIsShownThenHidden()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();
				BaseCustomsBrokerageUserControl brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl linesUserControl = brokerageControl.InvoiceLinesUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals("Precondition: CantCreateInvoiceLinesLabel", true, linesUserControl.CantCreateInvoiceLinesLabel.Visible);
				AssertEquals(DockStyle.None, linesUserControl.ClassificationPanel.Dock);

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				declaration.Invoices.DeleteAll();
				declaration.Invoices.AddNew().JZ_InvoiceNumber = "AAAA";
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals("Precondition: CantCreateInvoiceLinesLabel", false, linesUserControl.CantCreateInvoiceLinesLabel.Visible);

				int bottomOfTopPanel = linesUserControl.TopPanel.Top + linesUserControl.TopPanel.Height - 1; // Yeah I know, why -1 right? think about it...
				int topOfBottomPanel = linesUserControl.BottomPanel.Top;
				AssertEquals("bottomOfTopPanel < topOfBottomPanel  - (" + bottomOfTopPanel.ToString() + " < " + topOfBottomPanel.ToString() + ")", true, bottomOfTopPanel < topOfBottomPanel);
			}
		}

		public void TestSetCantCreateInvoiceLinesLabel()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.ShouldCreateDummyInvoiceLinesForMerge).Returns(true);
			BaseJobDeclaration testDec = mockDeclaration.Object;
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("CantCreateInvoiceLinesLabel", true, testUserControl.CantCreateInvoiceLinesLabel.Visible);
				AssertEquals("BottomPanel", false, testUserControl.BottomPanel.Visible);
				AssertEquals("Bottom1Panel", false, testUserControl.TopPanel.Visible);

				mockDeclaration.Setup(m => m.ShouldCreateDummyInvoiceLinesForMerge).Returns(false);
				testDec.Invoices.AddNew();
				AssertEquals("CantCreateInvoiceLinesLabel", false, testUserControl.CantCreateInvoiceLinesLabel.Visible);
				AssertEquals("BottomPanel", true, testUserControl.BottomPanel.Visible);
				AssertEquals("Bottom1Panel", true, testUserControl.TopPanel.Visible);
			}
		}

		public void TestCantCreateInvoiceLinesLabelIsInvisibleIfInvoiceLinesShowable()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.ShouldCreateDummyInvoiceLinesForMerge).Returns(false);
			BaseJobDeclaration declaration = mockDeclaration.Object;
			declaration.Invoices.AddNew();
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(mockDeclaration.Object))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("CantCreateInvoiceLinesLabel", false, testUserControl.CantCreateInvoiceLinesLabel.Visible);
			}
		}

		public void TestCantCreateInvoiceLinesLabelIsVisibleIfInvoiceLinesNotShowable()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.ShouldCreateDummyInvoiceLinesForMerge).Returns(true);
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(mockDeclaration.Object))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("CantCreateInvoiceLinesLabel", true, testUserControl.CantCreateInvoiceLinesLabel.Visible);
			}
		}

		public void TestShowContainerTabOnLoadOfSeaDeclaration()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					Assert("Precondition - Show ContainersTab", testUserControl.LineDetailTabControl.TabPages.Contains(testUserControl.ContainersTabPage));
				}
			}
		}

		public void TestNotShowContainerTabOnLoadOfAirDeclaration()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_TransportMode = testDec.TransportModeAirCodeForTesting;
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					Assert("Should not show container tab for AIR job.", !testUserControl.LineDetailTabControl.TabPages.Contains(testUserControl.ContainersTabPage));
					testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
					Assert(" Show ContainersTab", testUserControl.LineDetailTabControl.TabPages.Contains(testUserControl.ContainersTabPage));
				}
			}
		}

		public void TestAlwaysShowContainerTabWithAir()
		{
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Setup(m => m.ContainersAlwaysRequired).Returns(ZBool.True);
			BaseJobDeclaration testDec = mock.Object;
			testDec.JE_TransportMode = testDec.TransportModeAirCodeForTesting;
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					Assert("Should show container tab for AIR job with always show container tab set.", testUserControl.LineDetailTabControl.TabPages.Contains(testUserControl.ContainersTabPage));
				}
			}
			mock.VerifyAll();
		}

		public void TestAlwaysShowContainerTabWithSeaChangingToAir()
		{
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Setup(m => m.ContainersAlwaysRequired).Returns(ZBool.True);
			BaseJobDeclaration testDec = mock.Object;
			testDec.JE_TransportMode = testDec.TransportModeAirCodeForTesting;
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					Assert("Precondition - Should show container tab", testUserControl.LineDetailTabControl.TabPages.Contains(testUserControl.ContainersTabPage));
					testDec.JE_TransportMode = testDec.TransportModeAirCodeForTesting;
					Assert("Should show container tab", testUserControl.LineDetailTabControl.TabPages.Contains(testUserControl.ContainersTabPage));
				}
			}
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestChangeMessageSubTypeSetCantCreateInvoiceLines()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var mock = new Mock<BaseInvoiceLineUserControl>();
			mock.CallBase = true;
			using (BaseInvoiceLineUserControl control = mock.Object)
			{
				control.SetDataBinding(declaration, "");

				mock.Protected().Setup("SetCantCreateInvoiceLinesLabel");
				declaration.JE_MessageSubType = "AAA";
				mock.VerifyAll();
			}
		}

		public void TestCustomFieldsTabPage()
		{
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Setup(m => m.ContainersAlwaysRequired).Returns(ZBool.True);
			var testDec = mock.Object;
			testDec.JE_TransportMode = testDec.TransportModeAirCodeForTesting;

			using (var testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var tabPagesCount = testUserControl.LineDetailTabControl.TabPages.Count;
					var customFieldsTabPage = testUserControl.LineDetailTabControl.TabPages[tabPagesCount - 1];

					AssertEquals("Custom Fields tab should be the last tab", "Custom Fields", customFieldsTabPage.Text);
					AssertEquals("Custom Fields tab should have an InvoiceLineCustomFieldsControl", typeof(InvoiceLineCustomFieldsControl), customFieldsTabPage.Controls[0].GetType());
				}
			}
		}

		public void TestSelectContainerForAllInvoiceLinesMenuItem()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				MenuItem menu = null;
				foreach (MenuItem item in testUserControl.CusContainerInvoiceLineGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Attach this Container to All Invoice Lines")
					{
						menu = item;
						break;
					}
				}
				AssertNotNull("Context menu found", menu);
			}
		}

		public void TestUnselectContainerForAllInvoiceLinesMenuItem()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				MenuItem menu = null;
				foreach (MenuItem item in testUserControl.CusContainerInvoiceLineGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Detach this container from All Invoice Lines")
					{
						menu = item;
						break;
					}
				}
				AssertNotNull("Context menu found", menu);
			}
		}

		public void TestPartAttributeCaptions()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "OH1";
			org1.MiscServ.OM_IMPartAttrib1Name = "un";
			org1.MiscServ.OM_IMPartAttrib2Name = "deux";
			org1.MiscServ.OM_IMPartAttrib3Name = "trois";
			var org2 = OrgHeader.New(Factory);
			org2.OH_Code = "OH2";
			org2.MiscServ.OM_IMPartAttrib1Name = "one";
			org2.MiscServ.OM_IMPartAttrib2Name = "two";
			org2.MiscServ.OM_IMPartAttrib3Name = "three";
			Factory.Save();

			BaseJobDeclaration declaration = BaseJobDeclaration.New(new BusinessObjectFactory());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				AssertEquals("Part Attrib. 1", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("Part Attrib. 2", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("Part Attrib. 3", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				declaration.JE_OH_Importer = org1.PK;
				AssertEquals("un", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("deux", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("trois", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				declaration.JE_OH_Importer = org2.PK;
				AssertEquals("one", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("two", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("three", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				new DataRefreshManager().StartManaging(org2.MiscServ);
				org2.MiscServ.OM_IMPartAttrib1Name = "four";
				org2.MiscServ.OM_IMPartAttrib2Name = "five";
				org2.MiscServ.OM_IMPartAttrib3Name = "six";
				Factory.Save();

				AssertEquals("four", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("five", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("six", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				declaration.JE_OH_Importer = ZGuid.Empty;
				AssertEquals("Part Attrib. 1", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("Part Attrib. 2", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("Part Attrib. 3", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));
			}
		}

		public void TestNewOwnerPartColumns()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "OH1";
			org1.MiscServ.OM_IMPartAttrib1Name = "un";
			org1.MiscServ.OM_IMPartAttrib2Name = "deux";
			org1.MiscServ.OM_IMPartAttrib3Name = "trois";
			var org2 = OrgHeader.New(Factory);
			org2.OH_Code = "OH2";
			org2.MiscServ.OM_IMPartAttrib1Name = "one";
			org2.MiscServ.OM_IMPartAttrib2Name = "two";
			org2.MiscServ.OM_IMPartAttrib3Name = "three";
			Factory.Save();

			BaseJobDeclaration declaration = BaseJobDeclaration.New(new BusinessObjectFactory());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				AssertEquals("Part Attrib. 1", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("Part Attrib. 2", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("Part Attrib. 3", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				declaration.JE_OH_Importer = org1.PK;
				AssertEquals("un", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("deux", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("trois", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				declaration.JE_OH_Importer = org2.PK;
				AssertEquals("one", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("two", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("three", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				new DataRefreshManager().StartManaging(org2.MiscServ);
				org2.MiscServ.OM_IMPartAttrib1Name = "four";
				org2.MiscServ.OM_IMPartAttrib2Name = "five";
				org2.MiscServ.OM_IMPartAttrib3Name = "six";
				Factory.Save();

				AssertEquals("four", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("five", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("six", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));

				declaration.JE_OH_Importer = ZGuid.Empty;
				AssertEquals("Part Attrib. 1", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
				AssertEquals("Part Attrib. 2", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
				AssertEquals("Part Attrib. 3", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				AssertEquals("Serial #", testUserControl.CustomsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));
			}
		}

		public void TestApportionmentPendingLabelShownOnLoadIfDirty()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testDec.ApportionmentDirty = true;

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				AssertEquals("PreCondition:Apportionment is dirty", true, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is shown", true, testUserControl.PendingApportionmentLabel.Visible);
			}
		}

		public void TestApportionmentPendingLabelHiddenOnLoadIfNotDirty()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testDec.ApportionmentDirty = false;

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				AssertEquals("PreCondition:Apportionment is dirty", false, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is not shown", false, testUserControl.PendingApportionmentLabel.Visible);
			}
		}

		public void TestApportionmentPendingLabelShownWhenDirtyGetsChangedToTrue()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testDec.ApportionmentDirty = false;
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				AssertEquals("PreCondition:Apportionment is dirty", false, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is not shown", false, testUserControl.PendingApportionmentLabel.Visible);

				testDec.ApportionmentDirty = true;
				AssertEquals("Apportionment is dirty", true, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is shown", true, testUserControl.PendingApportionmentLabel.Visible);
			}
		}

		public void TestApportionmentPendingLabelHiddenWhenDirtyGetsChangedToFalse()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testDec.ApportionmentDirty = true;
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				AssertEquals("PreCondition:Apportionment is dirty", true, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is not shown", true, testUserControl.PendingApportionmentLabel.Visible);

				testDec.ApportionmentDirty = false;
				AssertEquals("Apportionment is dirty", false, testDec.ApportionmentDirty);
				AssertEquals("Pending lable is shown", false, testUserControl.PendingApportionmentLabel.Visible);
			}
		}

		public void TestInvoiceLineGridLayoutPersister()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();

				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				BaseInvoiceLineUserControl testUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull("Persister", testUserControl.fInvoiceLineGridLayoutPersister);
			}
		}

		public void TestInvoiceLineGridDeleting()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_AutoWeightApportion = true;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = declaration.Invoices[0].InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = declaration.Invoices[0].InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine4 = declaration.Invoices[0].InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine5 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10;
			invoiceLine1.JI_LineNo = 1;
			invoiceLine2.JI_LinePrice = 10;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine3.JI_LinePrice = 10;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine4.JI_LinePrice = 10;
			invoiceLine4.JI_LineNo = 4;
			invoiceLine5.JI_LinePrice = 10;
			invoiceLine5.JI_LineNo = 5;
			invoice.JZ_Weight = 200;
			AssertEquals((ZDecimal)40, invoiceLine1.JI_Weight);
			AssertEquals((ZDecimal)40, invoiceLine2.JI_Weight);

			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(declaration))
			{
				TestZGrid testGrid = new TestZGrid();
				testGrid.Parent = testForm;
				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("PK", 200));
				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Name", 200));
				testGrid.SetDataBinding(declaration.Invoices[0].InvoiceLines, "");
				testForm.Show();
				var hitInfo = testGrid.HitTest(testGrid.RowHeaderWidth - 1, testGrid.PreferredRowHeight + 10);
				AssertEquals("Hit On Row 0", 0, hitInfo.Row);
				AssertEquals("Hit On Row Header", -1, hitInfo.Column);
				AssertEquals("Hit On Row Header", DataGrid.HitTestType.RowHeader, hitInfo.Type);

				testGrid.DeleteRow(hitInfo);
				AssertEquals((ZShort)1, invoiceLine2.JI_LineNo);
				AssertEquals((ZShort)2, invoiceLine3.JI_LineNo);
				AssertEquals((ZShort)3, invoiceLine4.JI_LineNo);
				AssertEquals((ZShort)4, invoiceLine5.JI_LineNo);
				AssertEquals((ZDecimal)50, invoiceLine2.JI_Weight);
				AssertEquals((ZDecimal)50, invoiceLine3.JI_Weight);
				AssertEquals((ZDecimal)50, invoiceLine4.JI_Weight);
				AssertEquals((ZDecimal)50, invoiceLine5.JI_Weight);
			}
		}

		public void TestTariffColumn()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Guadeloupe;
			var branch = company.Branches.AddNew();
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.FR.IJobDeclaration>();
			declaration.JE_GB = branch.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new BaseInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Liechtenstein))
				{
					var pivotGrid = (ZGrid)control.Controls.Find("CustomsInvoiceLinesBoundGrid", true)[0];
					pivotGrid.ListManager.Position = 1;
					var tariffColumnStyleInfo = (Universal.GUI.TariffColumnStyleInfo)pivotGrid.GetColumnStyle("JI_Tariff");
					AssertNotNull("TariffColumnStyleInfo", tariffColumnStyleInfo);
					AssertEquals("TariffColumnStyleInfo.TariffType", "HSN", tariffColumnStyleInfo.GetTariffType());
					AssertEquals("TariffColumnStyleInfo.GetCountryCode()", Core.Constants.CountryCodes.France, tariffColumnStyleInfo.GetCountryCode());

					invoiceLine1.Delete();
					invoiceLine2.Delete();
					AssertEquals("TariffColumnStyleInfo.GetCountryCode() - Deleted", Core.Constants.CountryCodes.Liechtenstein, tariffColumnStyleInfo.GetCountryCode());

					using (var control2 = new BaseInvoiceLineUserControl())
					{
						pivotGrid = (ZGrid)control2.Controls.Find("CustomsInvoiceLinesBoundGrid", true)[0];
						tariffColumnStyleInfo = (Universal.GUI.TariffColumnStyleInfo)pivotGrid.GetColumnStyle("JI_Tariff");
						AssertEquals("TariffColumnStyleInfo.GetCountryCode() - Null", Core.Constants.CountryCodes.Liechtenstein, tariffColumnStyleInfo.GetCountryCode());
					}
				}
			}
		}

		public void TestMatchingKeyColumn()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.InvoiceLines.AddNew();

			using (var frm = new BaseJobDeclarationForm(declaration))
			{
				frm.Show();
				frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				Application.DoEvents();

				var grid = frm.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				var column = grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>()
					.First(c => c.ColumnName == JobComInvoiceLineSchema.Constants.JI_MatchingKey);

				Assert("Should default to false.", !column.IsVisible);
			}
		}

		public void TestNewLineDetailsTabPage_NotEnabled()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
			{
				form.Controls.Add(invoiceLineUserControl);
				form.Show();
				var lineDetailTabControl = invoiceLineUserControl.FindSingle<ZTabControl>(c => c.Name == "LineDetailTabControl");
				var lineDetailsTabPage = lineDetailTabControl.FindSingle<ZTabPage>(c => c.Name == "LineDetailsTabPage");
				var newLineDetailsTabPage = lineDetailTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "NewLineDetailsTabPage");

				CombineAssertions(() =>
				{
					AssertEquals("LineDetailsTabPage", true, lineDetailsTabPage.TabVisible);
					AssertNull("NewLineDetailsTabPage", newLineDetailsTabPage);
				});
			}
		}

		public void TestApportionWeightMustSelectAtLeastOneInvoiceLine()
		{
			var jobDeclartion = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			using (var form = new BaseJobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var boundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;

				var apportionWeightMenuItem = boundGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "apportionWeightMenuItem");
				apportionWeightMenuItem.PerformClick();

				AssertEquals("Information You must select at least one Invoice Line.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestApportionWeightMenuItemWhenAutoApportionWeightMenuItemIsTrue()
		{
			var jobDeclartion = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			using (var form = new BaseJobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var boundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;

				var apportionWeightSeparatorMenuItem = boundGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "apportionWeightSeparatorMenuItem");
				var apportionWeightMenuItem = boundGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "apportionWeightMenuItem");
				AssertNotNull(apportionWeightSeparatorMenuItem);
				AssertNotNull(apportionWeightMenuItem);

				jobDeclartion.JE_AutoWeightApportion = true;
				PopupMenu(boundGrid.ContextMenu, EventArgs.Empty);

				Assert(apportionWeightSeparatorMenuItem.Visible);
				Assert(apportionWeightMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				apportionWeightMenuItem.PerformClick();
				AssertEquals("'Auto Apportion Weight' will be turned off before allocating weight. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				PopupMenu(boundGrid.ContextMenu, EventArgs.Empty);
				apportionWeightMenuItem.PerformClick();
				Assert("Should be changed to false.", !jobDeclartion.JE_AutoWeightApportion);
			}
		}

		public void TestBondedWarehouseOrderControlsVisibility() => CombineAssertions(() =>
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineWithWhsOrderNumber = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLineWithWhsControlsNotVisible = invoiceHeader.InvoiceLines.AddNew();
			invoiceLineWithWhsOrderNumber.JI_BondedWHSOrderNumber = "1";

			using (var form = new ZForm(declaration))
			using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
			{
				form.Controls.Add(invoiceLineUserControl);
				form.Show();

				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceLineWithWhsOrderNumber);
				AssertEquals("BondedWHSOrderNumberTextBox visible", true, invoiceLineUserControl.BondedWHSOrderNumberTextBox.Visible);
				AssertEquals("BondedWHSOrderLineNumberCalcEdit visible", true, invoiceLineUserControl.BondedWHSOrderLineNumberCalcEdit.Visible);

				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceLineWithWhsControlsNotVisible);
				AssertEquals("BondedWHSOrderNumberTextBox not visible", false, invoiceLineUserControl.BondedWHSOrderNumberTextBox.Visible);
				AssertEquals("BondedWHSOrderLineNumberCalcEdit not visible", false, invoiceLineUserControl.BondedWHSOrderLineNumberCalcEdit.Visible);
			}
		});

		public void TestAddInvoiceLineAssessmentInitialized()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "520620";
				((ISupportInteractionWithComplianceWiseCommodities)declaration).Helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();

				using (var form = new ZForm(declaration))
				using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
				{
					form.Controls.Add(invoiceLineUserControl);
					form.Show();
					var commodityTab = invoiceLineUserControl.LineDetailTabControl.AllTabPages.Single(x => x.Text == "Compliance Assessment") as ZTabPage;
					AssertNotNull(commodityTab);
					AssertEquals(true, commodityTab.TabVisible);
					AssertEquals("CpwInvocingLineTabPage", commodityTab.Name);

					AssertEquals(invoiceLine, invoiceLine.LinkedModuleCommodity.Parent);
					AssertEquals("CLR", invoiceLine.LinkedModuleCommodity.RiskStatus);
					AssertEquals("Clear", invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
					AssertEquals(false, invoiceLine.LinkedModuleCommodity.RiskStatusDescription_ReadOnly);
					AssertEquals(false, invoiceLine.LinkedModuleCommodity.AssessmentNotes_ReadOnly);
					AssertEquals(true, invoiceLine.LinkedModuleCommodity.CommodityExists);
					AssertEquals("Test Harmonized Border Wise Textual", invoiceLine.LinkedModuleCommodity.HarmonizedBorderWiseTextual);
				}
			}
		}

		public void TestInvoiceLineUserControlRiskStatusColumnExistOrNot()
		{
			AssertInvoiceLineUserControlRiskStatusColumnExistOrNot(false);
			AssertInvoiceLineUserControlRiskStatusColumnExistOrNot(true);

			void AssertInvoiceLineUserControlRiskStatusColumnExistOrNot(bool enable)
			{
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(enable))
				{
					var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "520620";
					((ISupportInteractionWithComplianceWiseCommodities)declaration).Helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();

					using (var form = new ZForm(declaration))
					using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
					{
						form.Controls.Add(invoiceLineUserControl);
						form.Show();

						var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
						var column = grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>()
							.FirstOrDefault(c => c.ColumnName == "LinkedModuleCommodityRiskStatusDescription");
						if (enable)
						{
							AssertNotNull("Risk Status column should exist when feature is enabled", column);
							AssertEquals("Risk Status", column.CaptionResourceString.Caption);
							AssertEquals(column.ColumnStyleType, typeof(CommodityRiskStatusColumnStyle));
						}
						else
						{
							AssertNull("Risk Status column should not exist when feature is disabled", column);
						}
					}
				}
			}
		}

		public void TestInvoiceLineUserControlComplianceAlertsColumnExistOrNot()
		{
			AssertInvoiceLineUserControlComplianceAlertsColumnExistOrNot(false);
			AssertInvoiceLineUserControlComplianceAlertsColumnExistOrNot(true);

			void AssertInvoiceLineUserControlComplianceAlertsColumnExistOrNot(bool enable)
			{
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(enable))
				{
					var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "520620";
					((ISupportInteractionWithComplianceWiseCommodities)declaration).Helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();

					using (var form = new ZForm(declaration))
					using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
					{
						form.Controls.Add(invoiceLineUserControl);
						form.Show();

						var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
						var column = grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>()
							.FirstOrDefault(c => c.ColumnName == "LegalBookLink");
						if (enable)
						{
							AssertNotNull("Compliance Alerts column should exist when feature is enabled", column);
							AssertEquals("Compliance Alerts", column.CaptionResourceString.Caption);
							AssertEquals(column.ColumnStyleType, typeof(LegalBooksColumnStyle));
						}
						else
						{
							AssertNull("Compliance Alerts column should not exist when feature is disabled", column);
						}
					}
				}
			}
		}

		public void TestInvoiceLineUserControlImportAlertsColumnExistOrNot()
		{
			AssertInvoiceLineUserControlImportAlertsColumnExistOrNot(true, true, true, true);
			AssertInvoiceLineUserControlImportAlertsColumnExistOrNot(false, true, true, false);
			AssertInvoiceLineUserControlImportAlertsColumnExistOrNot(true, false, true, false);
			AssertInvoiceLineUserControlImportAlertsColumnExistOrNot(true, true, false, false);

			void AssertInvoiceLineUserControlImportAlertsColumnExistOrNot(bool enableFeature, bool enableShowImportAlerts, bool isExport, bool expect)
			{
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(enableFeature))
				using (OrganisationsDataRegistry.Instance.CustomsShowImportAlertsOnExportDeclarations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableShowImportAlerts))
				{
					var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					if (isExport)
					{
						declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					}
					else
					{
						declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					}
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "520620";
					((ISupportInteractionWithComplianceWiseCommodities)declaration).Helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();

					using (var form = new ZForm(declaration))
					using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
					{
						form.Controls.Add(invoiceLineUserControl);
						form.Show();

						var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
						var column = grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>()
							.FirstOrDefault(c => c.ColumnName == "LinkedModuleCommodityImportAlertDescription");
						if (expect)
						{
							AssertNotNull("LinkedModuleCommodityImportAlertDescription column should exist", column);
							AssertEquals("Import Risk", column.CaptionResourceString.Caption);
						}
						else
						{
							AssertNull("LinkedModuleCommodityImportAlertDescription column should not exist", column);
						}
					}
				}
			}
		}

		class DummyInteractionWithComplianceWiseCommoditiesHelper : IInteractionWithComplianceWiseCommoditiesHelper
		{
			public DummyInteractionWithComplianceWiseCommoditiesHelper()
			{
				SourceSideCommodities = new SourceSideCommodities() { GetCommoditiesStatusFromCpw = GetCommoditiesStatusFromCpw };
				CpwSideCommodities = new CpwSideCommodities();
			}

			public ISourceSideCommodities SourceSideCommodities { get; }
			public ICpwSideCommodities CpwSideCommodities { get; }

			ComplianceResultFromCpw[] GetCommoditiesStatusFromCpw()
			{
				return new ComplianceResultFromCpw[] {
					new ComplianceResultFromCpw
					{
						HarmonizedCode = "520620",
						GroupingOrCountry = "WCO",
						GoodsDescription = "",
						OriginOfGoods = "",
						LinkVisible = true,
						HarmonizedBorderWiseTextual = "Test Harmonized Border Wise Textual",
						RiskStatus = "CLR",
						RiskNotes = "",
						AssessmentInitialized = true
					}
				};
			}
		}

		public void TestAddInvoiceLineComponentsUserControl()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableInwardProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoiceline = declaration.InvoiceLines.AddNew();
				using (var form = new ZForm(declaration))
				using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
				{
					form.Controls.Add(invoiceLineUserControl);
					form.Show();
					AssertNotNull(invoiceLineUserControl.LineDetailTabControl.GetTabPage("LineComponentsTabPage"));
				}
			}
		}

		public void TestInvoiceLineComponentsUserControl_Visibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableInwardProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoicelineWithInvisibleTab = declaration.FilteredInvoiceLines.AddNew();
				var invoicelineWithVisibleTab = declaration.FilteredInvoiceLines.AddNew();
				var componentInventory = invoicelineWithVisibleTab.ComponentInventoryCollection.AddNew();
				componentInventory.JIV_QuantityToDraw = 1;
				using (var form = new ZForm(declaration))
				using (var invoiceLineUserControl = new BaseInvoiceLineUserControl())
				{
					form.Controls.Add(invoiceLineUserControl);
					form.Show();
					invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.SetDataBinding(declaration, "FilteredInvoiceLines");
					var tabPage = invoiceLineUserControl.LineDetailTabControl.AllTabPages.Single(x => x.Name == "LineComponentsTabPage") as ZTabPage;

					AssertEquals("Prereq: invoicelineWithInvisibleTab IsInvoiceLineComponentsVisible", expected: false, invoicelineWithInvisibleTab.IsInvoiceLineComponentsVisible);
					AssertEquals("Prereq: invoicelineWithVisibleTab IsInvoiceLineComponentsVisible", expected: true, invoicelineWithVisibleTab.IsInvoiceLineComponentsVisible);
					CombineAssertions(() =>
					{
						invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoicelineWithInvisibleTab);
						AssertEquals("invoiceLineWithInvisibleTab: tab visibility", expected: false, tabPage.TabVisible);

						invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoicelineWithVisibleTab);
						AssertEquals("invoicelineWithVisibleTab: tab visibility", expected: true, tabPage.TabVisible);
					});
				}
			}
		}

		public void TestCreateClassificationAssistantRequestMenu()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "Test Supplier 1";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = supplier1.PK;
			declaration.JE_GC = company.PK;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "TESTJZ001";
			invoice1.JZ_OH_Supplier = supplier1.PK;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Calc_Invoice = "TESTJZ001";

			BaseInvoiceLineUserControlForTest.DisplayCreateClassificationAssistantRequestMenuForTest = true;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HSAssistant, Core.Constants.CountryCodes.Australia, ZDateTime.Now, value: false))
			using (var form = new CustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesGrid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				var createClassificationAssistantRequestMenuItem = invoiceLinesGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "createClassificationAssistantRequestMenuItem");
				AssertNull("Menu 'Create Classification Assistant Request' should be null when EnableClassificationAssistant is disable", createClassificationAssistantRequestMenuItem);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HSAssistant, Core.Constants.CountryCodes.Australia, ZDateTime.Now, value: true))
			{
				BaseInvoiceLineUserControlForTest.DisplayCreateClassificationAssistantRequestMenuForTest = false;
				using (var form = new CustomsDeclarationFormForTest(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLinesGrid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;

					var createClassificationAssistantRequestMenuItem = invoiceLinesGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "createClassificationAssistantRequestMenuItem");
					AssertNull("Menu 'Create Classification Assistant Request' should be null when DisplayCreateClassificationAssistantRequestMenu is false", createClassificationAssistantRequestMenuItem);
				}

				BaseInvoiceLineUserControlForTest.DisplayCreateClassificationAssistantRequestMenuForTest = true;
				using (var form = new CustomsDeclarationFormForTest(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLinesGrid = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;

					var createClassificationAssistantRequestMenuItem = invoiceLinesGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "createClassificationAssistantRequestMenuItem");
					AssertNotNull("Menu 'Create Classification Assistant Request' should not be null", createClassificationAssistantRequestMenuItem);
					Assert("Menu 'Create Classification Assistant Request' menuitem should not be visible", createClassificationAssistantRequestMenuItem.Visible);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					createClassificationAssistantRequestMenuItem.PerformClick();
					AssertEquals("Please select at least one invoice line.", UnitTestUserNotification.Instance.LastMessage.Text);

					invoiceLinesGrid.Select(0);
					createClassificationAssistantRequestMenuItem.PerformClick();
					AssertType<SendProductInformationRequestForm>(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		void PopupMenu(ContextMenu menuItem, EventArgs e)
		{
			typeof(ContextMenu).InvokeMember("OnPopup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { e });
		}

		sealed class CustomsDeclarationFormForTest : BaseJobDeclarationForm
		{
			public CustomsDeclarationFormForTest(BaseJobDeclaration jobDeclaration)
				: base(jobDeclaration)
			{
			}

			public CustomsDeclarationFormForTest()
				: this(null)
			{
			}

			public BaseInvoiceLineUserControl InvoiceUserControl
			{
				get { return CustomsBrokerageUserControl.InvoiceLinesUserControl; }
			}

			protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
			{
				return new CustomsBrokerageUserControlForTest();
			}
		}

		sealed class CustomsBrokerageUserControlForTest : BaseCustomsBrokerageUserControl
		{
			protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
			{
				return new BaseInvoiceLineUserControlForTest();
			}
		}

		sealed class BaseInvoiceLineUserControlForTest : BaseInvoiceLineUserControl
		{
			public BaseInvoiceLineUserControlForTest() : base()
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = JobComInvoiceLineSchema.Constants.JI_BrandName });
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = JobComInvoiceLineSchema.Constants.JI_Model });
			}

			protected override void OnLoad(EventArgs e)
			{
				using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLineSchema.Constants.JI_Model);
					base.OnLoad(e);
				}
			}

			protected override ZBool DynamicLayoutApplied => true;

			protected override ZBool HasDifferentPanelLayout => true;

			public Action DoWhenAccessingGetNewInvoiceLineDetailsPanelLayout { get; set; }

			protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout()
			{
				DoWhenAccessingGetNewInvoiceLineDetailsPanelLayout?.Invoke();
				return base.GetNewInvoiceLineDetailsPanelLayout();
			}

			#region Create Classification Assistant Request

			public static bool DisplayCreateClassificationAssistantRequestMenuForTest { get; set; }

			protected override bool DisplayCreateClassificationAssistantRequestMenu => DisplayCreateClassificationAssistantRequestMenuForTest;

			#endregion
		}

		sealed class ColumnCustomisationTestHelper
		{
			public ColumnCustomisationTestHelper(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
			}
			readonly BaseJobDeclaration declaration;

			public void SetColumnVisible(OrgHeader organisation, bool descriptionVisibility, bool quantityVisibility, bool priceVisibility)
			{
				declaration.JE_OH_Importer = organisation == null ? ZGuid.Empty : organisation.PK;
				using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					BaseInvoiceLineUserControl userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					ZGrid linesGrid = userControl.CustomsInvoiceLinesBoundGrid;
					linesGrid.SetColumnVisible(descriptionVisibility, JobComInvoiceLineSchema.JI_Description.Name);
					linesGrid.SetColumnVisible(quantityVisibility, JobComInvoiceLineSchema.JI_InvoiceQuantity.Name);
					linesGrid.SetColumnVisible(priceVisibility, JobComInvoiceLineSchema.JI_LinePrice.Name);
					linesGrid.Columns.HasLayoutChanged = true; // Is set whenever you change the layout in the GUI.
					form.Close();
				}
			}

			public void AssertColumnVisible(OrgHeader organisation, bool descriptionVisibility, bool quantityVisibility, bool priceVisibility)
			{
				declaration.JE_OH_Importer = organisation == null ? ZGuid.Empty : organisation.PK;
				using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					BaseInvoiceLineUserControl userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					ZGrid linesGrid = userControl.CustomsInvoiceLinesBoundGrid;
					AssertEquals(OrgName(organisation) + " - descriptionColumn.IsVisible", descriptionVisibility, linesGrid.Columns[JobComInvoiceLineSchema.JI_Description.Name].IsVisible);
					AssertEquals(OrgName(organisation) + " - quantityColumn.IsVisible", quantityVisibility, linesGrid.Columns[JobComInvoiceLineSchema.JI_InvoiceQuantity.Name].IsVisible);
					AssertEquals(OrgName(organisation) + " - priceColumn.IsVisible", priceVisibility, linesGrid.Columns[JobComInvoiceLineSchema.JI_LinePrice.Name].IsVisible);
					form.Close();
				}
			}

			static string OrgName(OrgHeader organisation)
			{
				return organisation == null ? "(null org)" : organisation.OH_FullName.ToString();
			}
		}

		sealed class TestZGrid : ZGrid
		{
			public void DeleteRow(HitTestInfo hitInfo)
			{
				MouseUpInfo = hitInfo;
				DeleteMenuItem_Click(null, new EventArgs());
			}
		}
	}
}
