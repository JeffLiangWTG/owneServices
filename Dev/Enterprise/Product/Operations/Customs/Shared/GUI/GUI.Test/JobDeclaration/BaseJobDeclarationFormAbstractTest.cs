using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class BaseJobDeclarationFormAbstractTest<TBusinessObject> : BaseCustomsFormBasherTest
		where TBusinessObject : BaseJobDeclaration
	{
		public void TestDeclarationUpdatesFromRoutingTab()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			Transport transport = declaration.Transports[0];
			AssertEquals("Precondition: transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: transport.JW_ETA", new ZDateTime(2009, 1, 3), transport.JW_ETA);
			AssertEquals("Precondition: transport.JW_IsLinked", true, transport.JW_IsLinked);
			Factory.Save();

			JobSailing sailing = transport.Sailing;
			AssertNotNull("Precondition: Must have a auto-linked sailing generated from the above settings.");

			BusinessObjectFactory updateFactory = new BusinessObjectFactory();
			JobSailing sailingToUpdate = updateFactory.Load<JobSailing>(sailing.PK);
			sailingToUpdate.Origin.JA_E_DEP = new ZDateTime(2009, 1, 4);
			sailingToUpdate.Destination.JB_E_ARV = new ZDateTime(2009, 1, 5);
			updateFactory.Save();

			BusinessObjectFactory reloadFactory = new BusinessObjectFactory();
			BaseJobDeclaration declarationReloaded = reloadFactory.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals("Precondition: declarationReloaded.JE_ExportDate", new ZDateTime(2009, 1, 2), declarationReloaded.JE_ExportDate);
			AssertEquals("Precondition: declarationReloaded.JE_DateOfArrival", new ZDateTime(2009, 1, 3), declarationReloaded.JE_DateOfArrival);

			using (BaseJobDeclarationForm form = (BaseJobDeclarationForm)Activator.CreateInstance(FormToBashType, new object[] { declarationReloaded }))
			{
				form.Show();
				AssertEquals("declarationReloaded.JE_ExportDate", new ZDateTime(2009, 1, 4), declarationReloaded.JE_ExportDate);
				AssertEquals("declarationReloaded.JE_DateOfArrival", new ZDateTime(2009, 1, 5), declarationReloaded.JE_DateOfArrival);
			}
		}

		public void TestLicense()
		{
			using (BaseJobDeclarationForm testForm = GetFormToBash() as BaseJobDeclarationForm)
			{
				testForm.Show();
				BaseJobDeclaration declaration = (BaseJobDeclaration)testForm.BusinessEntity;
				LicenceCheckpoint checkpoint = new LicenceCheckpoint();
				LicenceLoginEventArgs e = new LicenceLoginEventArgs(Env.Licence.ImportBroker);
				declaration.RaiseLicenceLogin(e);
				AssertEquals(true, e.LoginHasBeenAttempted);
			}
		}

		public void TestBondedWarehouseLicense()
		{
			using (var form = GetFormToBash() as BaseJobDeclarationForm)
			{
				form.Show();
				var declaration = (BaseJobDeclaration)form.BusinessEntity;
				var e = new LicenceLoginEventArgs(Env.Licence.BondedWarehouse);
				declaration.RaiseBondedWarehouseLicenceLogin(e);
				AssertEquals(true, e.LoginHasBeenAttempted);
			}
		}

		public void TestEachTabCanBeSelectedAndShown()
		{
			using (BaseJobDeclarationForm testForm = GetFormToBash() as BaseJobDeclarationForm)
			{
				testForm.Show();

				int initialTabPageCount = testForm.fCustomsBrokerageUserControl.MainTabControl.TabPages.Count;
				for (int i = 0; i < initialTabPageCount; i++)
				{
					testForm.fCustomsBrokerageUserControl.MainTabControl.SelectedIndex = i;
					AssertEquals("Tab Page Count is changing", initialTabPageCount, testForm.fCustomsBrokerageUserControl.MainTabControl.TabPages.Count);
					Assert(testForm.fCustomsBrokerageUserControl.MainTabControl.SelectedIndex == i);
				}
			}
		}

		public void TestDuplicateColumnStyles()
		{
			using (BaseJobDeclarationForm testForm = GetFormToBash() as BaseJobDeclarationForm)
			{
				testForm.Show();

				int initialTabPageCount = testForm.fCustomsBrokerageUserControl.MainTabControl.TabPages.Count;

				CombineAssertions(() =>
				{
					for (int i = 0; i < initialTabPageCount; i++)
					{
						testForm.fCustomsBrokerageUserControl.MainTabControl.SelectedIndex = i;
						var currentTabPage = testForm.fCustomsBrokerageUserControl.MainTabControl.TabPages[i];

						AssertZGridDoesNotHaveDuplicateColumnStyles(currentTabPage);
					}
				});

				Assert(true);
			}
		}

		public void TestPlugIns()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia); //this is to ensure the LandedCosting Document is loaded
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			using (BaseJobDeclarationForm form = new BaseJobDeclarationForm(declaration))
			{
				AssertNotNull(ControllerIDs.LandedCosting.Name, form.PlugIns.GetPlugIn(ControllerIDs.LandedCosting));
				AssertNotNull(ControllerIDs.JobInvoicing.Name, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(ControllerIDs.DocAddresses.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(ControllerIDs.eDocsPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(ControllerIDs.DocDataPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull(ControllerIDs.CartagePlugin.Name, form.PlugIns.GetPlugIn(ControllerIDs.CartagePlugin));
				AssertNotNull(ControllerIDs.DtbBooking.Name, form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}

		public void TestPrintServices()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DocsAndCartage.Services.AddNew();
			declaration.DocsAndCartage.Services.AddNew();

			DocumentWrapper[] wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);

			AssertEquals(2, wrappers.Length);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);

			using (new BaseJobDeclarationForm(declaration))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);

				AssertEquals(2, wrappers.Length);
				AssertEquals("DocumentServicesForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
			}
		}

		[SnailTest]
		public void TestControlLock()
		{
			var configs = new DeclarationLockConfigCollection(null, Factory);
			var config = configs.AddNew();
			config.DeclarationType = "IMP";

			var eventInfo = config.EventInfos.AddNew();
			eventInfo.EventType = AutoEvents.ArrivalCode;
			eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;

			var controlsForLock = GetControlsForLock();
			foreach (var kv in controlsForLock)
			{
				var tabInfo = config.TabInfos.AddNew();
				tabInfo.TabPage = kv.Key;
			}

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configs))
			using (var frm = (BaseJobDeclarationForm)GetFormToBash())
			{
				if (frm.EnableControlLock)
				{
					var declaration = frm.Declaration;

					var fileParent = (ICustomsFileParent)declaration;
					fileParent.DeclarationTypeInfo.SetValueFromString("IMP");
					fileParent.LockFile("Lock For Test");

					frm.Show();
					Application.DoEvents();

					var ignoreControlForLock = GetIgnoreControlForLock();

					foreach (var kv in controlsForLock)
					{
						var key = kv.Key;
						foreach (var controlName in kv.Value)
						{
							var matchControls = frm.Controls.Find(controlName, true);
							if (matchControls.Length > 0)
							{
								var controlForLock = matchControls[0];

								if (controlForLock is ZTabPage tp)
								{
									var tabControl = tp.Parent as ZTabControl;
									tabControl.SelectedIndex = tabControl.TabPages.IndexOf(tp);
								}

								controlForLock.Show();

								string ignoreControlNames = ignoreControlForLock.TryGetValue(key, out ignoreControlNames)
									? ignoreControlNames
									: string.Empty;

								ZControlExtensionsTest.AssertEditableIncludingChildren(controlForLock, false, ignoreControlNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
							}
						}
					}
				}
			}

			Assert(true);
		}

		protected sealed override BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = GetPopulatedDeclarationForFormBashingCore();
			declaration.ResumeApportionment();
			return declaration;
		}

		protected virtual TBusinessObject GetPopulatedDeclarationForFormBashingCore()
		{
			TBusinessObject declaration = Factory.New<TBusinessObject>();
			object o = declaration.WarehouseDocAddress;
			o = declaration.ContainerYardDocAddress;
			o = declaration.ContainerTerminalOperatorDocAddress;
			o = declaration.DepotDocAddress;

			declaration.JE_MessageType = MessageTypeForFormBashing;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			BaseJobComInvoiceGroupHeader header = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice = header.JobComInvoiceHeaders.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			return declaration;
		}

		protected virtual int NumberOfInvoiceLinesForPerformanceTest => 50;

		protected virtual Dictionary<string, string[]> GetControlsForLock()
		{
			return new Dictionary<string, string[]>
			{
				{ Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationCustom, new [] { nameof(BaseCustomsDeclarationUserControl.ShipmentCustomFieldsPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers, new [] { nameof(BaseCustomsDeclarationUserControl.NumbersTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationOrders, new [] { nameof(BaseCustomsDeclarationUserControl.OrdersTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationOrganizations, new [] { nameof(BaseCustomsDeclarationUserControl.OrganisationsTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationServices, new [] { nameof(BaseCustomsDeclarationUserControl.DocsTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationPickupOrDelivery, new [] { nameof(BaseCustomsBrokerageUserControl.PickupTabPage), nameof(BaseCustomsBrokerageUserControl.DeliveryTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.Declaration, new [] { nameof(BaseCustomsBrokerageUserControl.DeclarationTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.Containers, new [] { nameof(BaseCustomsBrokerageUserControl.ContainerTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.Packing, new [] { nameof(BaseCustomsBrokerageUserControl.PackingTabPage) }  },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.InvoiceGroups, new [] { nameof(BaseCustomsBrokerageUserControl.InvoiceGroupingTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.InvoiceHeaders, new [] { nameof(BaseCustomsBrokerageUserControl.InvoicesTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.InvoiceLines, new [] { nameof(BaseCustomsBrokerageUserControl.InvoiceLinesTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.Misc, new [] { nameof(BaseCustomsBrokerageUserControl.MiscOptionsTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.MessageOrEntries, new [] { nameof(BaseCustomsBrokerageUserControl.MessagesTabPage) } },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.EntryInstructions,new [] { nameof(BaseCustomsBrokerageUserControl.EntryInstructionDetailsTabPage) } },
			};
		}

		protected virtual Dictionary<string, string> GetIgnoreControlForLock()
		{
			return new Dictionary<string, string>
			{
				{ Core.Constants.Customs.DeclarationTabPages.Codes.Declaration, "RightTabControl" },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.Containers, "edContainerNum" },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.Packing, "PackingDetailsGrid" },
				{ Core.Constants.Customs.DeclarationTabPages.Codes.InvoiceLines, "CusContainerInvoiceLineGrid,ZGridExtendedStripControl,DjcPackageInvoiceLineGrid" }
			};
		}

		protected virtual List<string> GetInvoiceChildrenToIgnoreList() => new List<string>();

		protected virtual List<string> GetInvoiceLineChildrenToIgnoreList() => new List<string>();

		protected virtual void PopulateBillForPerformanceTest(Bill bill, int index)
		{
		}

		protected virtual BaseCusContainer CreateContainerForPerformanceTest(BaseJobDeclaration declaration, int index)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT1234" + index;
			var list = container.Lookups.CO_FCL_LCL_NCT_List;
			container.CO_FCL_LCL_AIR = list[index % list.Count].Code;
			container.CO_RC = ContainerTypes[index % ContainerTypes.Length].PK;
			return container;
		}

		protected RefContainer[] ContainerTypes
		{
			get
			{
				if (containerTypes == null)
				{
					containerTypes = Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, ContainerTypeCodes));
				}
				return containerTypes;
			}
		}
		RefContainer[] containerTypes;

		protected virtual BaseJobComInvoiceHeader CreateInvoiceHeaderForPerformanceTest(BaseJobDeclaration testDec)
		{
			return testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}

		protected virtual BaseJobDeclaration CreateDeclarationForPerformanceTest(BusinessObjectFactory factory)
		{
			BaseJobDeclaration result = BaseJobDeclaration.New(factory);
			result.JE_MessageType = MessageTypeForFormBashing;
			result.JE_TransportMode = result.TransportModeAirCodeForTesting;
			return result;
		}

		protected virtual BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
		{
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.10.10 10";
			invoiceLine.JI_AddInfo = "ORG=US*GSTE=FOOD*PRF=A";
			invoiceLine.JI_InvoiceQuantity = 35;
			invoiceLine.JI_InvoiceUQ = "KG";
			return invoiceLine;
		}

		protected new virtual TBusinessObject currentFormBashingDeclaration
		{
			get { return (TBusinessObject)base.currentFormBashingDeclaration; }
			set { base.currentFormBashingDeclaration = value; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		public void TestProductCreationConfirmationFormWithConsigneeSetting()
		{
			var declarationImporter = Factory.New<OrgHeader>();
			declarationImporter.OH_Code = "Importer";
			declarationImporter.OH_RL_NKClosestPort = "AUSYD";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.OH_IsConsignor = true;
			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = "AAA";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";

			using (var form = (BaseJobDeclarationForm)GetFormToBash())
			{
				var declaration = form.Declaration;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "123";
				invoiceLine.JI_Description = "QQQ";
				invoiceLine.JI_PartNo = "AAA";
				invoiceLine.JI_InvoiceUQ = "NO";
				declaration.JE_OH_Supplier = declarationSupplier.PK;
				if (declaration.IsEntryInstructionRequired)
				{
					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
				}
				IShowPreSaveDialog showPreSaveDialog = form;

				declaration.JE_OH_Importer = ZGuid.Empty;
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}

				declaration.JE_OH_Importer = declarationImporter.PK;
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					ZFormModaliser.LastFormShownDialogForTest = null;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
			}

			using (var form = (BaseJobDeclarationForm)GetFormToBash())
			{
				var declaration = form.Declaration;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "123";
				invoiceLine.JI_Description = "QQQ";
				invoiceLine.JI_PartNo = "AAA";
				invoiceLine.JI_InvoiceUQ = "NO";
				declaration.JE_OH_Supplier = declarationSupplier.PK;
				if (declaration.IsEntryInstructionRequired)
				{
					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
				}
				IShowPreSaveDialog showPreSaveDialog = form;

				declaration.JE_OH_Importer = ZGuid.Empty;
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					ZFormModaliser.LastFormShownDialogForTest = null;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
				}

				declaration.JE_OH_Importer = declarationImporter.PK;
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.DEF;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.NO;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
					declarationImporter.MiscServ.OM_IMEnablePromptToCreateProducts = EnablePromptToCreateProductsList.Codes.YES;
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
			}
		}

		public void TestProductCreationConfirmationFormWithPromptToCreateProducts()
		{
			using (var form = (BaseJobDeclarationForm)GetFormToBash())
			{
				var declaration = form.Declaration;
				var declarationSupplier = OrgHeader.New(Factory);
				declarationSupplier.OH_Code = "AAA";
				declarationSupplier.OH_IsConsignor = true;
				declarationSupplier.MainAddress.OA_Address1 = "Add1";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Supplier = declarationSupplier.PK;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "123";
				invoiceLine.JI_Description = "QQQ";
				invoiceLine.JI_PartNo = "AAA";
				invoiceLine.JI_InvoiceUQ = "NO";
				if (declaration.IsEntryInstructionRequired)
				{
					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
				}
				IShowPreSaveDialog showPreSaveDialog = form;

				Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed = true;
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals("ProductCreationConfirmationForm", ZFormModaliser.LastFormShownDialogForTest.Text);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
				using (CustomsDataRegistry.Instance.EnablePromptToCreateProducts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					showPreSaveDialog.ShowPreSaveDialogs();
					AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		internal static string GetActualClassName(StackTrace stackTrace, string typeFullName)
		{
			var result = new ZStringBuilder(typeFullName);
			foreach (var frame in stackTrace.GetFrames().Reverse())
			{
				var method = frame?.GetMethod();
				if (method != null && method.MemberType == MemberTypes.Constructor)
				{
					var fileName = frame.GetFileName();
					if (!string.IsNullOrEmpty(fileName))
					{
						result.AppendFormat("{0}(vsnet:{1}#{2})", method.DeclaringType.FullName, fileName, frame.GetFileLineNumber().ToString());
					}
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		internal static void AddAdditionalChildDataIfNeeded(BusinessObject bizObj, int index)
		{
			var cusCodeDataTypeSupporter = bizObj as ICusCodeDataTypeSupporter;
			if (cusCodeDataTypeSupporter != null)
			{
				AddBizObjIfNeeded<CusCodeData>(bizObj, cusCodeDataTypeSupporter.GetCusCodeDataTypes(), CusCodeDataSchema.CY_ParentID, CusCodeDataSchema.CY_Type, (parentBizObj, childBizObj) =>
				{
					childBizObj.Parent = parentBizObj;
					childBizObj.CY_ParentID = parentBizObj.PK;
					childBizObj.CY_ParentTableCode = parentBizObj.TablePrefix;
					childBizObj.CY_Data = "1";
				}, index);
			}
			var cusAddInfoTypeSupporter = bizObj as ICusAddInfoTypeSupporter;
			if (cusAddInfoTypeSupporter != null)
			{
				AddBizObjIfNeeded<CusAddInfo>(bizObj, cusAddInfoTypeSupporter.GetCusAddInfoTypes(), CusAddInfoSchema.B7_ParentID, CusAddInfoSchema.B7_Type, (parentBizObj, childBizObj) =>
				{
					childBizObj.B7_ParentID = parentBizObj.PK;
					childBizObj.B7_ParentTableCode = parentBizObj.TablePrefix;
					childBizObj.B7_AddInfoData = "1";
				}, index);
			}
		}

		static void AddBizObjIfNeeded<T>(BusinessObject bizObj, IDictionary<ZString, Type> dictionary, SchemaGuidColumn foreignKeyColumn, SchemaStringColumn typeColumn, Action<BusinessObject, T> updateData, int index)
			where T : BusinessObject
		{
			if (dictionary != null)
			{
				var query = new ZQuery(foreignKeyColumn, bizObj.PK);
				query.AddToFilter(typeColumn, dictionary.Keys);
				query.FetchOnlyFromLocalCache = true;
				var factory = bizObj.Factory;
				factory.Load<T>(query).Select(x => x[typeColumn].ToString()).Distinct().ForEach(key => dictionary.Remove(key));
				foreach (var pair in dictionary)
				{
					var newChildBizObj = (T)bizObj.Factory.New(pair.Value);
					updateData(bizObj, newChildBizObj);
					AddAdditionalChildDataIfNeeded(newChildBizObj, index);
				}
			}
		}

		void AssertZGridDoesNotHaveDuplicateColumnStyles(Control control)
		{
			foreach (var grid in control.Controls.OfType<ZGrid>())
			{
				foreach (var duplicatedColumn in grid.ColumnStyles.Cast<ZGridColumnInfo>().GroupBy(x => x.ColumnName).Where(x => x.Count() > 1).Select(x => x.Key))
				{
					Assert("Column " + duplicatedColumn + " is duplicated on Grid " + grid.Name, false);
				}
			}

			control.Controls.Cast<Control>().ForEach(x => AssertZGridDoesNotHaveDuplicateColumnStyles(x));
		}

		ZString[] ContainerTypeCodes
		{
			get
			{
				return new ZString[] {
					"LD-29",
					"PM-2H",
					"CARH1",
					"DUMP",
					"LIVE3",
					"20PL",
					"20RE",
					"40HC",
					"40NO",
					"40RE"
				};
			}
		}
	}

	public abstract class BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<TBusinessObject> : BaseJobDeclarationFormAbstractTest<TBusinessObject>
		where TBusinessObject : BaseJobDeclaration
	{
		protected override TBusinessObject GetPopulatedDeclarationForFormBashingCore()
		{
			TBusinessObject declaration = base.GetPopulatedDeclarationForFormBashingCore();
			declaration.JE_IsCancelled = true;
			return declaration;
		}
	}
}
