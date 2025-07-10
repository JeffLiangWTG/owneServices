using System;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			using (var control = plugin.UserControl)
			{
				AssertEquals(typeof(CustomsBrokerageUserControl), control.GetType());
			}
		}

		public void TestBindingContext()
		{
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				using (BaseCustomsBrokerageUserControl control = (BaseCustomsBrokerageUserControl)plugin.UserControl)
				{
					JobDeclaration dec = Factory.New<JobDeclaration>();
					dec.JE_MessageType = JobMessageTypeList.Codes.Import;
					control.JobDeclaration = dec;
					AssertEquals(typeof(ZBindingContext), control.BindingContext.GetType());
					control.Show();
					control.MainTabControl.SelectedTab = control.PackingTabPage;
					control.LoadPackingTabPage();
					AssertEquals(typeof(ZBindingContext), ((UserControl)control.Packing).BindingContext.GetType());
				}
			}
		}

		public void TestTabPageTextIsSetBaseOnDirection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(shipment))
			{
				var department = new BusinessObjectFactory().NewWithValidTestData<GlbDepartment>();
				department.GE_Export = false;
				department.GE_Import = false;
				department.Factory.Save();

				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, department.PK.ToGuid(), factory: GlbBranch.CurrentBranch.Factory)))
				{
					shipment.JS_RL_NKOrigin = "";
					shipment.JS_RL_NKDestination = "";
					AssertEquals("shipment.IsExport", false, shipment.IsExport());
					AssertEquals("shipment.IsImport", false, shipment.IsImport());
					AssertNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.TabPage.Text", plugin.Name, plugin.TabPage.Text);

					shipment.JS_RL_NKOrigin = "USLAX";
					shipment.JS_RL_NKDestination = "AUSYD";
					AssertEquals("shipment.IsExport", true, shipment.IsExport());
					AssertEquals("shipment.IsImport", false, shipment.IsImport());
					AssertNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.TabPage.Text", BrokeragePlugIn.ExportBrokerageText, plugin.TabPage.Text);
				}

				department.GE_Export = true;
				department.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, department.PK.ToGuid(), factory: GlbBranch.CurrentBranch.Factory)))
				{
					shipment.JS_RL_NKOrigin = "";
					shipment.JS_RL_NKDestination = "";
					AssertEquals("shipment.IsExport", true, shipment.IsExport());
					AssertEquals("shipment.IsImport", false, shipment.IsImport());
					AssertNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.TabPage.Text", BrokeragePlugIn.ExportBrokerageText, plugin.TabPage.Text);

					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "USLAX";
					AssertEquals("shipment.IsExport", false, shipment.IsExport());
					AssertEquals("shipment.IsImport", true, shipment.IsImport());
					AssertNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.TabPage.Text", BrokeragePlugIn.ImportBrokerageText, plugin.TabPage.Text);
				}

				JobDeclaration declaration;

				department.GE_Import = true;
				department.GE_Export = false;
				department.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, department.PK.ToGuid(), factory: GlbBranch.CurrentBranch.Factory)))
				{
					shipment.JS_RL_NKOrigin = "";
					shipment.JS_RL_NKDestination = "";
					AssertEquals("shipment.IsExport", false, shipment.IsExport());
					AssertEquals("shipment.IsImport", true, shipment.IsImport());
					AssertNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.TabPage.Text", BrokeragePlugIn.ImportBrokerageText, plugin.TabPage.Text);

					declaration = Factory.New<JobDeclaration>();
					declaration.JE_JS = shipment.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					AssertEquals("shipment.IsExport", false, shipment.IsExport());
					AssertEquals("shipment.IsImport", true, shipment.IsImport());
					AssertNotNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.JobDeclaration.IsExport", true, plugin.JobDeclaration.IsExport);
					AssertEquals("Plugin.JobDeclaration.IsImport", false, plugin.JobDeclaration.IsImport);
					AssertEquals("Plugin.TabPage.Text", BrokeragePlugIn.ExportBrokerageText, plugin.TabPage.Text);
				}

				department.GE_Import = false;
				department.Factory.Save();
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, department.PK.ToGuid(), factory: GlbBranch.CurrentBranch.Factory)))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					AssertEquals("shipment.IsExport", false, shipment.IsExport());
					AssertEquals("shipment.IsImport", false, shipment.IsImport());
					AssertNotNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.JobDeclaration.IsExport", false, plugin.JobDeclaration.IsExport);
					AssertEquals("Plugin.JobDeclaration.IsImport", true, plugin.JobDeclaration.IsImport);
					AssertEquals("Plugin.TabPage.Text", BrokeragePlugIn.ImportBrokerageText, plugin.TabPage.Text);

					declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
					AssertEquals("shipment.IsExport", false, shipment.IsExport());
					AssertEquals("shipment.IsImport", false, shipment.IsImport());
					AssertNotNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.JobDeclaration.IsExport", false, plugin.JobDeclaration.IsExport);
					AssertEquals("Plugin.JobDeclaration.IsImport", true, plugin.JobDeclaration.IsImport);
					AssertEquals("Plugin.TabPage.Text", BrokeragePlugIn.ImportBrokerageText, plugin.TabPage.Text);

					declaration.JE_MessageType = "ZZZ";
					AssertEquals("shipment.IsExport", false, shipment.IsExport());
					AssertEquals("shipment.IsImport", false, shipment.IsImport());
					AssertNotNull("Plugin.JobDeclaration", plugin.JobDeclaration);
					AssertEquals("Plugin.JobDeclaration.IsExport", false, plugin.JobDeclaration.IsExport);
					AssertEquals("Plugin.JobDeclaration.IsImport", false, plugin.JobDeclaration.IsImport);
					AssertEquals("Plugin.TabPage.Text", plugin.Name, plugin.TabPage.Text);
				}
			}
		}

		public void TestSavingCancelled()
		{
			var oldControl = GlbStaff.CurrentUser.GS_IsController;
			bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			try
			{
				Env.Security.SupervisorOverrides.IsAllowed = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				GlbStaff.CurrentUser.GS_IsController = false;
				using (var registrySetup = new RegistrySetup(TargetInRegistry.ReconIssue))
				{
					var originalAllowed = Env.Security.USReconIssueDefault.IsAllowed;
					try
					{
						Env.Security.USReconIssueDefault.IsAllowed = false;
						var iOR = Factory.NewWithValidTestData<OrgHeader>();
						var iorWrapper = OrgHeaderWrapper.New(iOR);
						iorWrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
						Factory.Save();

						var shipment = Factory.New<ForwardingShipment>();
						using (var plugin = new BrokeragePlugIn(shipment))
						{
							var dec = Factory.New<JobDeclaration>();
							dec.JE_JS = shipment.PK;

							dec.JE_OH_Importer = iOR.PK;
							dec.JE_MessageType = JobMessageTypeList.Codes.Import;
							dec.IOROrgPK = iOR.PK;
							dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
							dec.US_EnableENS = true;
							dec.RecalculateReconIndicators();
							dec.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;

							AssertEquals(ContinueWithSave.No, plugin.ShowPreSaveDialogs());
						}
					}
					finally
					{
						Env.Security.USReconIssueDefault.IsAllowed = originalAllowed;
					}
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldControl;
				Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
			}
		}

		public void TestReconIssueCalculatedBeforeSupervisorOverride()
		{
			Env.Security.USReconIssueDefault.IsAllowed = true;

			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			var iorWrapper = OrgHeaderWrapper.New(iOR);
			iorWrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new BrokeragePlugIn(shipment))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_JS = shipment.PK;
				AssertEquals("pre-condition", "", dec.US_OtherReconIndicator);

				dec.JE_OH_Importer = iOR.PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.IOROrgPK = iOR.PK;
				dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				dec.US_EnableENS = true;

				AssertEquals(ContinueWithSave.Yes, plugin.ShowPreSaveDialogs());
			}
		}

		public void TestCheckExportEntriesToWithdraw()
		{
			var fShipment = Factory.New<ForwardingShipment>();
			fShipment.JS_TransportMode = "SEA";
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			fShipment.ConsignorPK = consignor.PK;
			fShipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			fShipment.ConsigneePK = consignee.PK;
			fShipment.JS_RL_NKDestination = "INBOM";
			fShipment.JS_GoodsDescription = "GoodsDescription";
			fShipment.JS_UniqueConsignRef = "BlahBlahBlah";
			fShipment.JS_ReleaseType = fShipment.Lookups.JS_ReleaseType_List[0].Code;

			fShipment.JS_ActualWeight = 100m;

			ForwardingConsol consol1 = fShipment.Consols.AddNew();
			consol1.JK_TotalShipmentActWeightCheck = 100m;

			var filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "123456789";
			filer.EntryFilerIDType = "D";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			using (var form = new ZForm(fShipment))
			using (var plugin = new BrokeragePlugIn(fShipment))
			{
				form.PlugIns.Add(plugin.ControllerID);
				form.Menu.MenuItems.Add(plugin.TopLevelMenu);

				var dec = Factory.New<JobDeclaration>();
				dec.JE_JS = fShipment.PK;
				AssertEquals("pre-condition", "", dec.US_OtherReconIndicator);

				dec.JE_MessageType = JobMessageTypeList.Codes.Export;

				var invoice1 = dec.Invoices.AddNew();
				invoice1.US_HazardousCargo = "Y";
				invoice1.InvoiceLines.AddNew();
				invoice1.InvoiceLines.AddNew();
				invoice1.InvoiceLines.AddNew();

				var collection = dec.CustomsEntryHeaders;
				var entry1Mock = Factory.NewMoq<CusEntryHeader>();
				entry1Mock.Setup(m => m.IsWaitingForResponse).Returns(false);
				entry1Mock.Setup(m => m.HasBeenWithdrawn).Returns(false);
				entry1Mock.Setup(m => m.HasBeenLodgedAtCustoms).Returns(true);
				var entry1 = entry1Mock.Object;
				collection.Add(entry1);
				entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
				entry1.US_IsDeactivated = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals(ContinueWithSave.Yes, plugin.ShowPreSaveDialogs());
			}
		}

		public void TestMenuIsCorrectType()
		{
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestRefreshExchangeRates()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			audCurr.ExchangeRates.DeleteAll();

			audCurr.SetUpExchangeRates(new ZDateTime(2012, 3, 1), 1.05m);

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 3);
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "CR";
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "OFT";
			invoiceCharge.J7_Amount = 100m;
			invoiceCharge.J7_RX_NKCurrency = "AUD";

			invoice.JobComInvoiceLines.AddNew();
			declaration.ResumeApportionment();

			AssertEquals("Exchange rate is refreshed", 1.05m, invoiceCharge.J7_ExchangeRate);
			AssertEquals(new ZDateTime(2012, 3, 1), declaration.US_LatestRateDate);

			audCurr.SetUpExchangeRates(new ZDateTime(2012, 3, 2), 1.06m);
			Factory.Save();

			using (BrokeragePlugIn form = new BrokeragePlugIn(shipment))
			{
				form.OnGUIShown();

				AssertEquals("Exrate should be refreshed", 1.06m, invoiceCharge.J7_ExchangeRate);
				AssertEquals("Apportionment Dirty", true, declaration.ApportionmentDirty);
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.CargoReleaseEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			Assert(declaration.ActiveEntryHeaders.CargoReleaseEntry.HasBeenLodgedAtCustoms);

			Factory.Save();

			audCurr.SetUpExchangeRates(new ZDateTime(2012, 3, 3), 1.07m);
			using (BrokeragePlugIn form = new BrokeragePlugIn(shipment))
			{
				form.OnGUIShown();

				AssertEquals("Exrate should NOT be refreshed", 1.06m, invoiceCharge.J7_ExchangeRate);
				AssertEquals("Apportionment Dirty", false, declaration.ApportionmentDirty);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

		protected override Customs.Business.BaseJobDeclaration GetDeclaration()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = base.GetDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			((JobDeclaration)declaration).US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			((JobDeclaration)declaration).US_EnableENS = true;

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_MessageText = "B01       FR                                                                    F102AL201070193999999 ALBANIA                       LEK            ALL14810     F202AL2010173110192                                                             Y         FR00014000000000000000000000000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			((JobDeclaration)declaration).Messages.Add(message);
			return declaration;
		}

		protected override void PrepareShipmentAndMergedDeclaration()
		{
			base.PrepareShipmentAndMergedDeclaration();
			JobDeclaration.CustomsEntryHeaders[0].CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
		}

		protected override void MakeThisDeclarationPackingRelevant(Customs.Business.BaseJobDeclaration declaration)
		{
			base.MakeThisDeclarationPackingRelevant(declaration);
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			((JobDeclaration)declaration).US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
		}

		protected override void SetUp()
		{
			Db.Connection.ExecuteNonQuery(@"DELETE FROM	dbo.StmNums WHERE SN_Name LIKE '%XJ5%'");

			base.SetUp();
		}
	}
}
