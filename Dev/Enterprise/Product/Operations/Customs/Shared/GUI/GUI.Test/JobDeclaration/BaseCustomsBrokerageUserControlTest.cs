using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseCustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestLockManager()
		{
			var configs = new DeclarationLockConfigCollection(null, Factory);
			var config = configs.AddNew();
			config.DeclarationType = "AAA";

			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configs))
			{
				using (var frm = new ZForm(declaration))
				{
					var ctr = new BaseCustomsBrokerageUserControl();
					frm.Controls.Add(ctr);

					frm.Show();
					Application.DoEvents();

					var lockManager = ctr.LockManager;
					AssertNull("Should be null as the control's parent is not BaseCustomsDeclarationForm and the declaration is not plugged into shipment.", lockManager);
				}

				using (var frm = new TestCustomsDeclarationForm(declaration))
				{
					frm.Show();
					Application.DoEvents();

					var lockManager = frm.CustomsBrokerageUserControl.LockManager;
					AssertNotNull("Should not null as the control's parent is BaseCustomsDeclarationForm and the from enable the control lock.", lockManager);
				}

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;

				using (var frm = new ZForm(declaration))
				{
					var ctr = new BaseCustomsBrokerageUserControl();
					frm.Controls.Add(ctr);

					frm.Show();
					Application.DoEvents();

					var lockManager = ctr.LockManager;
					AssertNotNull("Should not null as the declaration is plugged into shipment.", lockManager);
				}
			}
		}

		public void TestCustomLabelsLoadedProperly()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "org1";
			var label = org.CustomLabels.AddNew();
			label.OT_FieldName = Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
			label.OT_Caption = "MUST SHOW ME";

			CombineAssertions(() =>
			{
				var jobDeclaration = Factory.New<BaseJobDeclaration>();
				using (var form = new TestCustomsDeclarationForm(jobDeclaration))
				{
					var invheader = jobDeclaration.Invoices.AddNew();
					invheader.JZ_InvoiceNumber = "blah";
					form.Show();
					Application.DoEvents();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					Application.DoEvents();
					jobDeclaration.JE_OH_Supplier = org.PK;
					jobDeclaration.JE_MessageType = "EXP";
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					Application.DoEvents();
					AssertEquals("Normally", "MUST SHOW ME", form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
				}

				jobDeclaration = Factory.New<BaseJobDeclaration>();
				using (var form = new TestCustomsDeclarationForm(jobDeclaration))
				{
					var invheader = jobDeclaration.Invoices.AddNew();
					invheader.JZ_InvoiceNumber = "blah";
					form.Show();
					Application.DoEvents();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 1", "Attribute 1", form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					Application.DoEvents();
					jobDeclaration.JE_OH_Supplier = org.PK;
					jobDeclaration.JE_MessageType = "EXP";
					Application.DoEvents();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 1", "MUST SHOW ME", form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
				}

				jobDeclaration = Factory.New<BaseJobDeclaration>();
				using (var form = new TestCustomsDeclarationForm(jobDeclaration))
				{
					var invheader = jobDeclaration.Invoices.AddNew();
					invheader.JZ_InvoiceNumber = "blah";
					form.Show();
					Application.DoEvents();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					Application.DoEvents();
					jobDeclaration.JE_OH_Supplier = org.PK;
					jobDeclaration.JE_MessageType = "IMP";
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 2", "Attribute 1", form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					Application.DoEvents();
					jobDeclaration.JE_OH_Supplier = org.PK;
					jobDeclaration.JE_MessageType = "EXP";
					Application.DoEvents();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 2", "MUST SHOW ME", form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
				}

				jobDeclaration = Factory.New<BaseJobDeclaration>();
				using (var form = new TestCustomsDeclarationForm(jobDeclaration))
				{
					form.Show();
					Application.DoEvents();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
					Application.DoEvents();
					jobDeclaration.JE_OH_Supplier = org.PK;
					jobDeclaration.JE_MessageType = "EXP";
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 3", 0, form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.Columns.Count);

					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
					Application.DoEvents();
					var invheader = jobDeclaration.Invoices.AddNew();
					invheader.JZ_InvoiceNumber = "blah";

					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 3", "MUST SHOW ME", form.InvoiceUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
				}
			});
		}

		public void TestIDataGridLayoutIdentifierRoot()
		{
			GlbCompany usCompany = Factory.New<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			GlbBranch usBranch = usCompany.Branches.AddNew();
			usBranch.GB_RL_NKHomePort = "USCHI";
			GlbCompany.CurrentCompany.SetCountry("US");

			BaseJobDeclaration usDeclaration = Factory.New<BaseJobDeclaration>();
			usDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			usDeclaration.JE_GB = usBranch.PK;

			GlbCompany.CurrentCompany.SetCountry("AU");

			using (ZForm form = new ZForm(usDeclaration))
			using (BaseCustomsBrokerageUserControl brokerageUserControl = new BaseCustomsBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();
				AssertNoExceptionThrown(() => _ = ((IDataGridLayoutIdentifierRoot)brokerageUserControl).ID);

				brokerageUserControl.JobDeclaration = usDeclaration;

				AssertEquals("US", ((IDataGridLayoutIdentifierRoot)brokerageUserControl).ID);
			}
		}

		[ExpectNoExceptions]
		public void TestMergeDoesNotThrowNullReference()
		{
			var creator = new MergedDeclarationCreator<BaseJobDeclaration>(Factory);
			using (var form = new ZForm(creator.Declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				// don't add user control to the form - we want to emulate what happens with a plug-in when a menu is used (I23815)
				form.Show();
				creator.Declaration.JE_HouseBill = "housey";
				brokerageUserControl.JobDeclaration = creator.Declaration;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MessagesTabPage;
				creator.Declaration.DoMerge();
				brokerageUserControl.Dispose();
			}
		}

		public void TestInvoiceHeaderNotHiddenForEXW()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(declaration))
			using (BaseCustomsBrokerageUserControl brokerageUserControl = new BaseCustomsBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();
				brokerageUserControl.JobDeclaration = declaration;
				Assert("TabPages.Contains(InvoicesTabPage)", brokerageUserControl.MainTabControl.TabPages.Contains(brokerageUserControl.InvoicesTabPage));
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoicesTabPage;
				AssertNotNull("SupplierHeaderUserControl", brokerageUserControl.SupplierHeaderUserControl);

				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				Assert("TabPages.Contains(InvoicesTabPage)", brokerageUserControl.MainTabControl.TabPages.Contains(brokerageUserControl.InvoicesTabPage));
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoicesTabPage;
				AssertNotNull("SupplierHeaderUserControl", brokerageUserControl.SupplierHeaderUserControl);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			using (ZForm form = new ZForm(declaration))
			using (BaseCustomsBrokerageUserControl brokerageUserControl = new BaseCustomsBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				Assert("TabPages.Contains(InvoicesTabPage)", brokerageUserControl.MainTabControl.TabPages.Contains(brokerageUserControl.InvoicesTabPage));
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoicesTabPage;
				AssertNotNull("SupplierHeaderUserControl", brokerageUserControl.SupplierHeaderUserControl);
			}
		}

		public void TestPackingTabPageIsShown()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				BaseJobDeclaration declaration = mockDeclaration.Object;
				brokerageUserControl.JobDeclaration = declaration;
				AssertNotNull("UserControl.PackingTabPage", brokerageUserControl.PackingTabPage);
				Assert("TabPages.Contains(UserControl.PackingTabPage)", brokerageUserControl.MainTabControl.TabPages.Contains(brokerageUserControl.PackingTabPage));
			}
		}

		public void TestPickupTabPageIsShown()
		{
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{ 
				var declaration = BaseJobDeclaration.New(Factory);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				brokerageUserControl.JobDeclaration = declaration;
				AssertEquals("TabPages.Contains(PickupTabPage)", true, brokerageUserControl.MainTabControl.TabPages.Contains(brokerageUserControl.PickupTabPage));
			}
		}

		public void TestPickupTabPage_TabRelevant()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				AssertEquals(false, brokerageUserControl.PickupTabPage.TabRelevant);
			}
		}

		public void TestPickupTabPage_TabRelevant_LinkedToShipment()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				CombineAssertions(() =>
				{
					AssertEquals("Export", true, brokerageUserControl.PickupTabPage.TabRelevant);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("Import", false, brokerageUserControl.PickupTabPage.TabRelevant);
				});
			}
		}

		public void TestDeliveryTabPageIsShown()
		{
			CombineAssertions(() =>
			{
				using (var brokerageUserControl = CreateBrokerageUserControl())
				{
					var declaration = BaseJobDeclaration.New(Factory);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					brokerageUserControl.JobDeclaration = declaration;
					AssertEquals("TabPages.Contains(DeliveryTabPage)", true, brokerageUserControl.MainTabControl.TabPages.Contains(brokerageUserControl.DeliveryTabPage));
				}
			});
		}

		public void TestDeliveryTabPage_TabRelevant()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				CombineAssertions(() =>
				{
					AssertEquals("Import", true, brokerageUserControl.DeliveryTabPage.TabRelevant);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("Export", false, brokerageUserControl.DeliveryTabPage.TabRelevant);
				});
			}
		}

		public void TestDeliveryTabPage_TabRelevant_LinkedToShipment()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				AssertEquals(false, brokerageUserControl.DeliveryTabPage.TabRelevant);
			}
		}

		public void TestShowContainerTabOnLoadOfSeaDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				AssertEquals("ContainersTab visiblity should match declaration's ContainersRequired", declaration.ContainersRequired, brokerageUserControl.MainTabControl.Contains(brokerageUserControl.ContainerTabPage));
			}
		}

		public void TestNotShowContainerTabOnLoadOfAirDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				Assert("Should not show container tab for AIR job.", !brokerageUserControl.MainTabControl.Contains(brokerageUserControl.ContainerTabPage));
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("ContainersTab visiblity should match declaration's ContainersRequired", declaration.ContainersRequired, brokerageUserControl.MainTabControl.Contains(brokerageUserControl.ContainerTabPage));
			}
		}

		public void TestAlwaysShowContainerTabWithAir()
		{
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Setup(m => m.ContainersAlwaysRequired).Returns(ZBool.True);
			BaseJobDeclaration declaration = mock.Object;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert(declaration.ContainersRequired);
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				Assert("Should  show container tab for AIR job with always show container tab set.", brokerageUserControl.MainTabControl.Contains(brokerageUserControl.ContainerTabPage));
			}
		}

		public void TestAlwaysShowContainerTabWithSeaChangingToAir()
		{
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Setup(m => m.ContainersAlwaysRequired).Returns(ZBool.True);
			BaseJobDeclaration declaration = mock.Object;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				Assert("Precondition - Should show container tab", brokerageUserControl.MainTabControl.Contains(brokerageUserControl.ContainerTabPage));
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				Assert("Should show container tab", brokerageUserControl.MainTabControl.Contains(brokerageUserControl.ContainerTabPage));
			}
		}

		public void TestShowPackingTabInCorrectPositionSea()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				AssertEquals("Should show packing tab in correct position", brokerageUserControl.PackingTabPage, brokerageUserControl.MainTabControl.TabPages[2]);
			}
		}

		public void TestShowPackingTabInCorrectPositionAir()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				AssertEquals("Should show packing tab in correct position", brokerageUserControl.PackingTabPage, brokerageUserControl.MainTabControl.TabPages[1]);
			}
		}

		public void TestSelectedTabPageAfterSettingJobDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			CommonShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				AssertEquals("Selected tab page", brokerageUserControl.DeclarationTabPage, brokerageUserControl.MainTabControl.SelectedTab);
			}
		}

		public void TestMessageInitiatorAfterSettingDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			CommonShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = declaration;
				AssertNotNull(declaration.MessageInitiator);
			}
		}

		public void TestLoadInvoicesTabPage()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoicesTabPage;
				AssertNotNull("Supplier Header User Control has Loaded", brokerageUserControl.SupplierHeaderUserControl);
				AssertEquals("Supplier Header User Control has dock still of fill", DockStyle.Fill, brokerageUserControl.SupplierHeaderUserControl.Dock);
			}
		}

		public void TestLoadMiscOptionsPage()
		{
			var declaration = Factory.New<TestJobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MiscOptionsTabPage;
				CombineAssertions(() =>
				{
					AssertNull("Dynamic Misc Options User Control not Loaded", brokerageUserControl.DynamicMiscOptions);
					var miscOptionsControl = brokerageUserControl.MiscOptions;
					AssertNotNull("Base Misc Options User Control has Loaded", miscOptionsControl);
					AssertEquals("Misc Options User Control has dock still of fill", DockStyle.Fill, miscOptionsControl.Dock);
				});
			}
		}

		public void TestLoadDynamicMiscOptionsPage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var brokerageUserControl = new BaseCustomsBrokerageUserControl())
			{
				form.Controls.Add(brokerageUserControl);
				form.Show();

				brokerageUserControl.JobDeclaration = declaration;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MiscOptionsTabPage;
				CombineAssertions(() =>
				{
					AssertNull("Base Misc Options User Control has not Loaded", brokerageUserControl.MiscOptions);
					var dynamicMiscOptionsControl = brokerageUserControl.DynamicMiscOptions;
					AssertNotNull("Dynamic Misc Options User Control Loaded", dynamicMiscOptionsControl);
					AssertEquals("Dynamic Misc Options User Control has dock still of fill", DockStyle.Fill, dynamicMiscOptionsControl.Dock);
				});
			}
		}

		public void TestLoadMessagesPage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;

				using (var form = new ZForm(dec))
				using (var brokerageUserControl = CreateBrokerageUserControl())
				{
					form.Controls.Add(brokerageUserControl);
					form.Show();

					brokerageUserControl.JobDeclaration = dec;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MessagesTabPage;
					AssertEquals(typeof(EntriesWithMessagesOnDeclarationUserControl), brokerageUserControl.MessageUserControl.GetType());
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

				using (var form = new ZForm(dec))
				using (var brokerageUserControl = CreateBrokerageUserControl())
				{
					form.Controls.Add(brokerageUserControl);
					form.Show();

					brokerageUserControl.JobDeclaration = dec;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MessagesTabPage;
					AssertEquals(typeof(ExportMessageUserControl), brokerageUserControl.MessageUserControl.GetType());
				}
			}
		}

		[ExpectNoExceptions]
		public void TestJobDeclarationSetShouldGetCalledWhenDeclarationIsSet()
		{
			var mock = new Mock<BaseCustomsBrokerageUserControl>();
			mock.CallBase = true;
			mock.Protected().Setup("OnJobDeclarationSet");

			using (BaseCustomsBrokerageUserControl userControl = mock.Object)
			{
				userControl.JobDeclaration = Factory.New<BaseJobDeclaration>();
				mock.VerifyAll();
			}
		}

		public void TestOnJobDeclarationSetCalledOnce()
		{
			var dec = BaseJobDeclaration.New(Factory);

			using (var form = new BaseJobDeclarationFormTestClass(dec))
			{
				form.Show();
				AssertEquals("onDeclarationSet should only be called once", 1, ((BaseCustomsBrokerageUserControlTestClass)form.CustomsBrokerageUserControl).calledCountOfOnJobDeclarationSet);
				form.CustomsBrokerageUserControl.Dispose();
			}
		}

		public void TestLoadPackingPageForMultiHouseBills()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			BaseJobDeclaration declaration = mockDeclaration.Object;

			using (ZForm form = new ZForm(declaration))
			using (BaseCustomsBrokerageUserControl userControl = new BaseCustomsBrokerageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				userControl.JobDeclaration = declaration;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				AssertNotNull("Packing Tab Page User Control has Loaded", userControl.Packing);
				AssertEquals("Packing Tab Page is BaseCustomsPackingUserControl", typeof(BaseCustomsPackingUserControl), userControl.Packing.GetType());
			}
		}

		public void TestCorrectPluginsAreLoadedForStandAloneDec()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia); //this is to ensure the LandedCosting Document is loaded
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
			{
				BaseCustomsBrokerageUserControl userControl = new BaseCustomsBrokerageUserControl();

				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals(true, DoesPluginExist(form.PlugIns, "Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToFreight"));
				AssertEquals(true, DoesPluginExist(form.PlugIns, "Enterprise.LandedCosting.GUI.LandedCostingPlugIn"));
				AssertEquals(true, DoesPluginExist(form.PlugIns, "Enterprise.DocumentScanning.PlugIn.eDocsPlugIn"));
				AssertEquals(false, DoesPluginExist(form.PlugIns, "Enterprise.DocumentEngine.GUI.DocumentUDFPlugIn"));
				AssertEquals(false, DoesPluginExist(userControl.MainTabControl.PlugIns, "Enterprise.LandedCosting.GUI.LandedCostingPlugIn"));
				AssertEquals(false, DoesPluginExist(form.PlugIns, "Enterprise.DocumentEngine.GUI.SDF.DocumentSDFPlugIn"));
				AssertEquals(true, DoesPluginExist(form.PlugIns, "Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn.DocDataPlugIn"));
			}
		}

		public void TestCorrectPluginsAreLoadedForDecOnShipment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia); //this is to ensure the LandedCosting Plugin is loaded
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = Factory.New<ForwardingShipment>().PK;

			using (ZForm form = new ZForm(declaration))
			{
				BaseCustomsBrokerageUserControl userControl = new BaseCustomsBrokerageUserControl();

				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);

				form.Show();

				AssertEquals(true, DoesPluginExist(userControl.MainTabControl.PlugIns, "Enterprise.LandedCosting.GUI.LandedCostingPlugIn"));
				AssertEquals(false, DoesPluginExist(form.PlugIns, "Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToFreight"));
				AssertEquals(false, DoesPluginExist(form.PlugIns, "Enterprise.LandedCosting.GUI.LandedCostingPlugIn"));
				AssertEquals(false, DoesPluginExist(form.PlugIns, "Enterprise.DocumentScanning.PlugIn.eDocsPlugIn"));
				AssertEquals(false, DoesPluginExist(form.PlugIns, "Enterprise.DocumentEngine.GUI.DocumentUDFPlugIn"));
				AssertEquals(false, DoesPluginExist(userControl.MainTabControl.PlugIns, "Enterprise.DocumentEngine.GUI.SDF.DocumentSDFPlugIn"));
				AssertEquals(false, DoesPluginExist(userControl.MainTabControl.PlugIns, "Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn.DocDataPlugIn"));
			}
		}

		public void TestEndToEndForMerge()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.Transports.AddNew();
			declaration.Transports[0].JW_IsLinked = false;

			declaration.JE_VesselName = "Vessel";
			declaration.JE_VoyageFlightNo = "AA001";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			declaration.DoMerge();
			declaration.JE_GoodsDescription = "blah blah";

			using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				form.Show();
				Application.DoEvents();

				AssertEquals("Declaration.MergeManager.RequiresMerge", declaration.MergeManager.RequiresMerge, true);
				form.FireSaveButton();
				UserIdleWorker.Flush();
				AssertNotNull("Message user control has been created by bind unbound", form.CustomsBrokerageUserControl.fMessageUserControl);

				AssertEquals("Declaration.MergeManager.RequiresMerge", declaration.MergeManager.RequiresMerge, false);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("OnShown for messages not called as it is not selected", true, form.CustomsBrokerageUserControl.fMessageUserControl.OnShownCalled_ForTesting);
				AssertEquals("Declaration.MergeManager.RequiresMerge is false since has auto-merged", declaration.MergeManager.RequiresMerge, false);
				AssertEquals("Requires merge label not visible since auto-merged OK", false, ((BaseCustomsEntryUserControl)form.CustomsBrokerageUserControl.MessagesTabPage.Controls[0]).IsRequiresMergeLabelVisibleForTesting());

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_OH_Importer = ZGuid.Invalid; // error
				AssertEquals("Declaration.MergeManager.RequiresMerge", declaration.MergeManager.RequiresMerge, true);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("Declaration.MergeManager.RequiresMerge", declaration.MergeManager.RequiresMerge, false);

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_OH_Importer = ZGuid.Empty; // valid
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("Requires merge label not visible since auto-merged OK", false, ((BaseCustomsEntryUserControl)form.CustomsBrokerageUserControl.MessagesTabPage.Controls[0]).IsRequiresMergeLabelVisibleForTesting());
				AssertEquals("Declaration.MergeManager.RequiresMerge is false since has auto-merged", declaration.MergeManager.RequiresMerge, false);
			}
		}

		public void TestDisposeDoesNotCauseSelectedIndexChangedOnTabControlAsThisWillActivePlugIns()
		{
			var selectedIndexChangingCount = 0;
			using (var form = new ZForm(BaseJobDeclaration.New(Factory)))
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageUserControl.JobDeclaration = (BaseJobDeclaration)form.BusinessEntity;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				brokerageUserControl.MainTabControl.TabPages.Add(brokerageUserControl.ContainerTabPage);
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.ContainerTabPage;
				brokerageUserControl.MainTabControl.SelectedIndexChanging += new EventHandler(MainTabControl_SelectedIndexChanging);
				brokerageUserControl.Dispose();
			}
			AssertEquals("SelectedIndexChangingCount", 0, selectedIndexChangingCount);

			void MainTabControl_SelectedIndexChanging(object sender, EventArgs e)
			{
				selectedIndexChangingCount++;
			}
		}

		public void TestTrackingTabNotShownIfFormAlreadyHasTrackingTab()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var tabControl = new ZTemplateTabControl())
			using (var brokerageTabPage = new ZTabPage())
			using (var workflowTab = new ZWorkflowTabPage())
			using (var brokerageUserControl = CreateBrokerageUserControl())
			{
				brokerageTabPage.Controls.Add(brokerageUserControl);
				tabControl.TabPages.Add(brokerageTabPage);
				tabControl.TabPages.Add(workflowTab);
				form.Controls.Add(tabControl);

				form.Show();
				Application.DoEvents();
				AssertEquals("Tracking tab page not visible on this tab page if the form already has a Tracking tab page (shipment form for example)", false, brokerageUserControl.WorkflowTabPage.IsDisposed);
			}
		}

		static BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new BaseCustomsBrokerageUserControl();

		static bool DoesPluginExist(ZArchitecture.PlugIn.PlugIns plugins, string fullName)
		{
			foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in plugins.Instances)
			{
				if (plugIn.GetType().FullName == fullName)
				{
					return true;
				}
			}
			return false;
		}

		class TestCustomsDeclarationForm : BaseJobDeclarationForm
		{
			public TestCustomsDeclarationForm(BaseJobDeclaration jobDeclaration)
				: base(jobDeclaration)
			{
			}

			public BaseInvoiceLineUserControl InvoiceUserControl => CustomsBrokerageUserControl.InvoiceLinesUserControl;
		}

		class TestJobDeclaration : BaseJobDeclaration
		{
			public TestJobDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}

	public class BaseJobDeclarationFormTestClass : BaseJobDeclarationForm
	{
		public BaseJobDeclarationFormTestClass(BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new BaseCustomsBrokerageUserControlTestClass();

		public EDIMenu EDIMenu => (EDIMenu)TopLevelMenu;
	}

	class BaseCustomsBrokerageUserControlTestClass : BaseCustomsBrokerageUserControl
	{
		protected override void OnJobDeclarationSet()
		{
			calledCountOfOnJobDeclarationSet++;
		}

		public int calledCountOfOnJobDeclarationSet;
	}
}
