using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(CusInBondBillController))]
	sealed class CusInBondBillControllerTest : ZControllerBasherTest
	{
		public void TestSelectAndShowBill()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			Factory.Save();
			var controller = new CusInBondBillController();
			using (var form = (USAMSForm)controller.ShowEditForm(bill2))
			{
				Application.DoEvents();
				var billsUserControl = GetControl<USAMSBillsUserControl>(form, "usamsBillsUserControl");
				var grid = GetControl<USAMSBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");
				AssertEquals("should have selected bill2", bill2.PK, ((CusInBondBill)grid.ListManager.GetCurrent()).PK);
				AssertEquals(bill2.PK, form.IdentifierForPersistingForm);
			}

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			header.BH_OverrideFreightDefaults = true;
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			controller = new CusInBondBillController();
			using (var form = (ConsolForm)controller.ShowEditForm(bill2))
			{
				Application.DoEvents();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.US.AMS);
				var userControl = (USAMSConsolManifestUserControl)plugIn.UserControl;
				var billsUserControl = GetControl<USAMSConsolManifestUserControl, USAMSBillsUserControl>(userControl, "BillsDetailsUserControl");
				form.Show();
				var grid = GetControl<USAMSBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");
				AssertEquals("should have selected bill2", bill2.PK, ((CusInBondBill)grid.ListManager.GetCurrent()).PK);
				AssertEquals(bill2.PK, form.IdentifierForPersistingForm);
			}
		}

		T GetControl<T>(USAMSForm form, string name)
			where T : Control
		{
			return GetControl<USAMSForm, T>(form, name);
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.AMSBill);
			AssertEquals(ModuleIDs.Customs.US.AMSBill, controller.ModuleID);
		}

		public void TestReUseExistingAMSForms()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "OTT12432";
			Factory.Save();
			var amsController = ZControllerFactory.Create(ControllerIDs.Customs.US.AMS);
			var amsBillController = ZControllerFactory.Create(ControllerIDs.Customs.US.AMSBill);
			using (var form1 = amsController.ShowEditForm(header))
			using (var form2 = amsBillController.ShowEditForm(bill))
			{
				AssertSame("Should be re-using the same form", form1, form2);
			}
		}

		public void TestAMSFormShouldHaveNewButtonDisabled()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "OTT12432";
			Factory.Save();
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.AMSBill);
			using (var form = (ZForm)controller.ShowEditForm(bill))
			{
				AssertEquals("Form should have NEW button disabled", ODisplayMode.NewSaved, form.DisplayMode);
				AssertEquals(bill.PK, form.IdentifierForPersistingForm);
			}
		}

		public void TestShowNewFormDenied()
		{
			ZForm method(CusInBondBill bill, ZController controller) => (ZForm)controller.ShowFormForNewEntity(bill);
			GenericShowFormDeniedTest(method);
		}

		public void TestReloadNoException()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "OTT2342";
			Factory.Save();
			AssertNoExceptionThrown(() =>
			{
				ShowFormAndReload(bill);
				var consol = Factory.New<ForwardingConsol>();
				header.BH_ParentID = consol.PK;
				header.BH_ParentTableCode = consol.TablePrefix;
				Factory.Save();
				ShowFormAndReload(bill);
			});
		}

		public void TestShowTemplateCopyFormDenied()
		{
			ZForm method(CusInBondBill bill, ZController controller) => (ZForm)controller.ShowTemplateCopyForm(bill);
			GenericShowFormDeniedTest(method);
		}

		public void TestShowDeleteFormDenied()
		{
			ZForm method(CusInBondBill bill, ZController controller) => (ZForm)controller.ShowDeleteForm(bill);
			GenericShowFormDeniedTest(method);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = ZControllerFactory.Create(GetControllerID());
			var bo = GetBusinessObjectThatIsInTheDatabase();
			AssertEquals(bo.GetType(), controller.TypeOfTopLevelBusinessObject);
		}

		void ShowFormAndReload(CusInBondBill bill)
		{
			using (var form = (ZForm)Controller.ShowEditForm(bill))
			{
				form.Show();
				form.ReloadForm();
				var formCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				Application.DoEvents();
				AssertEquals("should be NewSaved mode after reload", ODisplayMode.NewSaved, formCreatedByReloading.DisplayMode);
				formCreatedByReloading.Close();
			}

			using (var form = (ZForm)Controller.ShowEditForm(bill))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.Edit;
				form.ReloadForm();
				var formCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				Application.DoEvents();
				AssertEquals("Edit and reload, should be NewSaved mode after reload", ODisplayMode.NewSaved, formCreatedByReloading.DisplayMode);
				formCreatedByReloading.Close();
			}
		}

#if !WINZOR
		public void TestWhenOverGuiResourceThresholdOnNumberOfWindowHandlesDoesNotThrow()
		{
			const int numberOfBills = 50;
			const string expectedMaxWindowsWarningMessage = "There are too many windows and/or graphical elements open by the application. Please close some unused windows and repeat this operation again.";
			var billsPKs = new ZGuid[numberOfBills];
			for (var recordNo = 0; recordNo < numberOfBills; recordNo++)
			{
				var consol = Factory.New<ForwardingConsol>();
				var header = Factory.New<CusInBondHeader>();
				header.BH_ParentID = consol.PK;
				header.BH_ParentTableCode = consol.TablePrefix;
				header.BH_ApplicationCode = "AMS";
				header.BH_TransitDirection = "N";
				var bill = header.Bills.AddNew();
				bill.B0_HouseBillNumber = "789456";
				bill.B0_MasterBillNumber = "789456";
				billsPKs[recordNo] = bill.PK;
			}

			Factory.Save();
			using (var module = new USAMSBillModule())
			{
				var iModule = (IFilterModuleInternalsForTesting)module;
				AssertEquals(0, iModule.GridCollection.Count);
				using (var form = module.ShowPopup())
				{
					var filters = (USAMSBillFilterStrip)iModule.FilterBusinessObject;
					var filter = (ModuleTextFilter)filters[USAMSBillFilterStrip.FilterConstants.HouseBillNumber];
					filter.Property = "789456";
					filter.IsActive = true;
					var factory_Initial = iModule.GridCollection.Factory;
					factory_Initial.ResetDatabaseLoadCount();
					iModule.PerformSearch();
					AssertEquals("Precondition: created records have loaded", numberOfBills, iModule.GridCollection.Count);
					var instancesOfTooManyWindowsWarning = 0;
					for (var recordNo = 0; recordNo < numberOfBills; recordNo++)
					{
						module.DisplayGrid.SelectSingleElementByPK(billsPKs[recordNo]);
						var menuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByText("Edit");
						AssertNoExceptionThrown("Should not throw exception when form is not created because too many forms are open already", () => menuItem.PerformClick());
						instancesOfTooManyWindowsWarning = UnitTestUserNotification.Instance.PreviousMessages.Count(msg => msg.Text == expectedMaxWindowsWarningMessage);
						if (instancesOfTooManyWindowsWarning >= 2)
						{
							break;
						}
					}

					AssertEquals("Should have encountered the 'too many windows open' message twice at this point", 2, instancesOfTooManyWindowsWarning);
				}
			}
		}
#endif

		delegate ZForm ShowFormDelegate(CusInBondBill bill, ZController controller);
		void GenericShowFormDeniedTest(ShowFormDelegate method)
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "OTT2342";
			Factory.Save();
			try
			{
				var form = method(bill, Controller);
				if (form != null)
				{
					form.Dispose();
				}

				Fail("Should have thrown a ModuleGuiNotSupportedException");
			}
			catch (ModuleGuiNotSupportedException)
			{
				Assert(true);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.US.AMSBill;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "OTT2342";
			Factory.Save();
			return bill;
		}

		protected override void TearDown()
		{
			CloseAllOpenConsolForms();
			base.TearDown();
		}

		void CloseAllOpenConsolForms()
		{
			foreach (var form in Application.OpenForms.OfType<ConsolForm>().ToArray())
			{
				form.Close();
			}
		}
	}
}
