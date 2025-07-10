using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BillTypeList = Enterprise.Customs.Business.BillTypeList;

namespace Enterprise.Customs.US.ISF.Module.Testing
{
	[TestedType(typeof(ISFModule))]
	sealed class ISFModuleTest : ZModuleBasherTest
	{
		public void TestCreateFromShipment()
		{
			using (var module = new ISFModule())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("Actions");
				var item = actionsMenuItem.MenuItems.FindByText(ISFModule.CreateFromShipmentMenuName);
				AssertNotNull("Menu action '" + ISFModule.CreateFromShipmentMenuName + "' should not be null.", item);
				item.PerformClick();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var form1 = ZFormModaliser.LastFormShownDialogForTest as ISFFromShipmentCreatorDialog;
				var creator1 = ZFormModaliser.LastIBusinessShownOnDialogForTest as ISFFromShipmentCreator;
				item.PerformClick();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var form2 = ZFormModaliser.LastFormShownDialogForTest as ISFFromShipmentCreatorDialog;
				var creator2 = ZFormModaliser.LastIBusinessShownOnDialogForTest as ISFFromShipmentCreator;
				AssertNotEquals("Should create two different forms.", form1.GetHashCode(), form2.GetHashCode());
				AssertSame("Should use a single creator as the system only allow one ForwardingModuleShipmentCollection per factory", creator1, creator2);
			}
		}

		public void TestCreateNewDeclaration()
		{
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Australia);
			var companyAu = Factory.New<GlbCompany>();
			companyAu.GC_Code = "ZAU";
			companyAu.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			companyAu.GC_RX_NKLocalCurrency = aud.RX_Code;
			var branchAu = companyAu.Branches.AddNew();
			branchAu.GB_Code = "ZAU";
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.UnitedStates);
			var companyUs = Factory.New<GlbCompany>();
			companyAu.GC_Code = "ZUS";
			companyAu.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			companyAu.GC_RX_NKLocalCurrency = usd.RX_Code;
			var branchUs = companyUs.Branches.AddNew();
			branchUs.GB_Code = "ZUS";
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISFT234322";
			header1.BF_HouseBill = "BM1231234";
			header1.BF_MasterBill = "MB5688455";
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISFT5685454";
			header2.BF_OceanBill = "OB6856465";
			var header3 = Factory.New<CusISFHeader>();
			header3.BF_JobReference = "ISFT569836";
			header3.BF_OceanBill = "OB98756456";
			for (var i = 1; i < 11; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MasterBill = (i % 2 == 0) ? "OB6856465" : "MB5688455";
				declaration.JE_HouseBill = "BM1231234";
				declaration.JE_OwnerRef = i.ToString();
				declaration.JE_GB = branchUs.PK;
			}

			var auDeclaration = Factory.New<JobDeclaration>();
			auDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			auDeclaration.JE_MasterBill = "MB5688455";
			auDeclaration.JE_HouseBill = "BM1231234";
			auDeclaration.JE_GB = branchAu.PK;
			var auBill = auDeclaration.Bills.AddNew();
			auBill.CU_BillType = BillTypeList.Codes.MasterBill;
			auBill.CU_BillNum = "OB6856465";
			Factory.Save();
			using (var module = new ISFModule())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("Actions");
				var item = actionsMenuItem.MenuItems.FindByText(ISFModule.CreateNewDeclarationMenuName);
				AssertNotNull("Menu action '" + ISFModule.CreateNewDeclarationMenuName + "' should not be null.", item);
				using (var form = new ZChildForm(module.GridCollection))
				{
					var formCached = OpenedFormCache.GetInstance();
					try
					{
						var control = (ISFFilterControl)module.EmbeddedControl;
						form.Controls.Add(control);
						form.Show();
						var filterObject = (ISFFilterBusinessObject)control.FilterBusinessObject;
						var entryNumberFilter = (ModuleTextFilter)filterObject["Job Number"];
						entryNumberFilter.IsActive = true;
						entryNumberFilter.Property = "ISFT234322";
						control.FirePerformSearch();
						var isfs = module.GridCollection.ToArray();
						AssertEquals(1, isfs.Length);
						AssertEquals(header1.PK, isfs[0].PK);
						var grid = (ZDisplayGrid)module.DisplayGrid;
						grid.UnSelectAll();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						AssertEquals(ISFModule.SelectAtLeastOneImporterSecurityFiling, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("formCached.Count", 0, formCached.Count);
						grid.Select(0);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						var expectedMessage = @"The following bill numbers are already in use on following declaration(s):

House Bill of Lading:BM1231234
B00001000, B00001001, B00001002, B00001003, B00001004, B00001005, B00001006, B00001007, B00001008, B00001009
B00001010

Master Bill of Lading:MB5688455
B00001000, B00001002, B00001004, B00001006, B00001008, B00001010";

						AssertMultilineASCIIEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						entryNumberFilter.Property = "ISFT5685454";
						control.FirePerformSearch();
						isfs = module.GridCollection.ToArray();
						AssertEquals(1, isfs.Length);
						AssertEquals(header2.PK, isfs[0].PK);
						grid = (ZDisplayGrid)module.DisplayGrid;
						grid.UnSelectAll();
						grid.Select(0);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						expectedMessage = @"The following bill number is already in use on following declaration(s):

Ocean Bill of Lading:OB6856465
B00001001, B00001003, B00001005, B00001007, B00001009, B00001010";

						AssertMultilineASCIIEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						entryNumberFilter.Property = "ISFT569836";
						control.FirePerformSearch();
						isfs = module.GridCollection.ToArray();
						AssertEquals(1, isfs.Length);
						AssertEquals(header3.PK, isfs[0].PK);
						grid = (ZDisplayGrid)module.DisplayGrid;
						grid.UnSelectAll();
						grid.Select(0);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					}
					finally
					{
						formCached.CloseAllCachedForms();
					}
				}
			}

			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				using (var module = new ISFModule())
				{
					var actionsMenuItem = module.FormActionMenu.FindByText("Actions");
					var item = actionsMenuItem.MenuItems.FindByText(ISFModule.CreateNewDeclarationMenuName);
					AssertNull("Menu action '" + ISFModule.CreateNewDeclarationMenuName + "' should not be available in Non-US company.", item);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestCreateNewDeclaration_ShouldBeThreadSafe()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.UnitedStates);
			var companyUs = Factory.New<GlbCompany>();
			var branchUs = companyUs.Branches.AddNew();
			branchUs.GB_Code = "ZUS";
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISFT569836";
			header.BF_OceanBill = "OB98756456";
			Factory.Save();
			BusinessObject ShowGridAndGetSelected(ISFModule module, ZChildForm form)
			{
				var control = (ISFFilterControl)module.EmbeddedControl;
				form.Controls.Add(control);
				form.Show();
				var filterObject = (ISFFilterBusinessObject)control.FilterBusinessObject;
				var entryNumberFilter = (ModuleTextFilter)filterObject["Job Number"];
				entryNumberFilter.IsActive = true;
				entryNumberFilter.Property = "ISFT569836";
				control.FirePerformSearch();
				var isfs = module.GridCollection.ToArray();
				AssertEquals(1, isfs.Length);
				AssertEquals(header.PK, isfs[0].PK);
				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.UnSelectAll();
				grid.Select(0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				return grid.SelectedElements[0];
			}

			using (var module = new ISFModule())
			{
				module.ShowFormsFromMainThread = true;
				var actionsMenuItem = module.FormActionMenu.FindByText("Actions");
				var createNewDeclarationMenuItem = actionsMenuItem.MenuItems.FindByText(ISFModule.CreateNewDeclarationMenuName);
				AssertNotNull("Menu action '" + ISFModule.CreateNewDeclarationMenuName + "' should not be null.", createNewDeclarationMenuItem);
				using (var form = new ZChildForm(module.GridCollection))
				{
					var selected = ShowGridAndGetSelected(module, form);
					var mainThreadBeginInvokeWasCalled = false;
					using (MainThreadRunner.OverrideInvocationStrategy(new MockThreadInvocationStrategy(() => mainThreadBeginInvokeWasCalled = true)))
					{
						var selectedItemThreadSentry = selected.Factory.ThreadSentry;
						selectedItemThreadSentry.RelinquishThreadOwnership();
						var thread = new Thread(() =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								selectedItemThreadSentry.TakeThreadOwnership();
								createNewDeclarationMenuItem.PerformClick();
							}
						});
						thread.IsBackground = true;
						thread.Start();
						thread.Join();
					}

					Assert(mainThreadBeginInvokeWasCalled);
				}
			}
		}

		public void TestCreateNewShipment()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISFT234322";
			header.BF_HouseBill = "BM1231234";
			header.BF_MasterBill = "MB5688455";
			Factory.Save();
			using (var module = new ISFModule())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("Actions");
				var item = actionsMenuItem.MenuItems.FindByText(ISFModule.CreateNewShipmentMenuName);
				AssertNotNull("Menu action '" + ISFModule.CreateNewShipmentMenuName + "' should not be null.", item);
				using (var form = new ZChildForm(module.GridCollection))
				{
					var formCached = OpenedFormCache.GetInstance();
					try
					{
						var control = (ISFFilterControl)module.EmbeddedControl;
						form.Controls.Add(control);
						form.Show();
						var filterObject = (ISFFilterBusinessObject)control.FilterBusinessObject;
						var entryNumberFilter = (ModuleTextFilter)filterObject["Job Number"];
						entryNumberFilter.IsActive = true;
						entryNumberFilter.Property = "ISFT234322";
						control.FirePerformSearch();
						var isfs = module.GridCollection.ToArray();
						AssertEquals(1, isfs.Length);
						AssertEquals(header.PK, isfs[0].PK);
						var grid = (ZDisplayGrid)module.DisplayGrid;
						grid.UnSelectAll();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						AssertEquals(ISFModule.SelectOneImporterSecurityFiling, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("formCached.Count", 0, formCached.Count);
						grid.Select(0);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						item.PerformClick();
						//ISFHeaderAndBIllSelectorForm selectorForm = (ISFHeaderAndBIllSelectorForm)ZFormModaliser.LastFormShownDialogForTest;
						//AssertEquals(header.PK, selectorForm.BusinessEntity.Header.PK);
						var bizo = (CusISFHeader)grid.SelectedElements[0];
						AssertEquals(header.PK, bizo.PK);
						//selectorForm.Dispose();
					}
					finally
					{
						formCached.CloseAllCachedForms();
					}
				}
			}

			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				using (var module = new ISFModule())
				{
					var actionsMenuItem = module.FormActionMenu.FindByText("Actions");
					var item = actionsMenuItem.MenuItems.FindByText(ISFModule.CreateNewShipmentMenuName);
					AssertNull("Menu action '" + ISFModule.CreateNewShipmentMenuName + "' should not be available in Non-US company.", item);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestImportFromXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = new ISFModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From XML"));
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		[TestDate(2015, 10, 21)]
		public void TestExportFromXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = new ISFModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Export To XML"));
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestAllowDelete()
		{
			using (var isfModule = new ISFModule())
			{
				AssertEquals("Deactivate/Activate should be allowed", true, isfModule.AllowDelete);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ImporterSecurityFiling;

		sealed class MockThreadInvocationStrategy : IMainThreadInvocationStrategy
		{
			readonly Action mockAction;
			public MockThreadInvocationStrategy(Action mockAction)
			{
				this.mockAction = mockAction;
			}

			public void BeginInvoke(Action action) => mockAction();
			public T Invoke<T>(Func<T> func) => throw new NotImplementedException();
		}
	}
}
