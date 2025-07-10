using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Module.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingModule))]
	public class DtbBookingModuleTest : DtbTransportModuleTest<DtbBookingModule>
	{
		public void TestConstructor()
		{
			using (var module = new DtbBookingModuleForTest())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		protected override Type ExpectedFilterBusinessObjectType
		{
			get { return typeof(DtbBookingFilterBusinessObject); }
		}

		protected override Type ExpectedFilterControlType
		{
			get { return typeof(DtbBookingFilterControl); }
		}

		protected override bool ExpectedAllowDelete
		{
			get { return false; }
		}

		protected override bool ExpectedAllowNew
		{
			get { return true; }
		}

		protected override Type ExpectedCollectionType
		{
			get { return typeof(DtbBookingCollection); }
		}

		public void TestBusinessContexts()
		{
			using (var module = new DtbBookingModuleForTest())
			{
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("The 'DtbBooking' business context should be returned", BusinessContext.DtbBooking, module.BusinessContexts[0]);
			}
		}

		public void TestGridCollection_HasFormStateOfBooking()
		{
			using (var module = new DtbBookingModule())
			{
				AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(module.GridCollection.Factory));
			}
		}

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint
		{
			get { return Env.Licence.TransportBookings; }
		}

		public void TestNewPickupMasterMenuItemMasterBookingsEnabled()
		{
			NewMenuMenuItemsTestCore(true, DtbBookingModule.PickupMasterBookingMenuItemText);
		}

		public void TestNewPickupMasterMenuItemMasterBookingsDisabled()
		{
			NewMenuMenuItemsTestCore(false, DtbBookingModule.PickupMasterBookingMenuItemText);
		}

		public void TestNewDeliveryMasterMenuItemMasterBookingsEnabled()
		{
			NewMenuMenuItemsTestCore(true, DtbBookingModule.DeliveryMasterBookingMenuItemText);
		}

		public void TestNewDeliveryMasterMenuItemMasterBookingsDisabled()
		{
			NewMenuMenuItemsTestCore(false, DtbBookingModule.DeliveryMasterBookingMenuItemText);
		}

		void NewMenuMenuItemsTestCore(bool masterBookingsEnabled, string masterMenuItemToTest)
		{
			using (var module = new DtbBookingModuleForTest())
			{
				MenuItem[] menus = null;

				using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, masterBookingsEnabled))
				{
					menus = module.GetNewStandardMenuItemsForTest();
					var newMenu = menus.FirstOrDefault(m => m.Text == "&New").MenuItems.Cast<MenuItem>();

					CheckMenuAndTransportBookingOnOpenedFormThenDispose(newMenu, DtbBookingModule.NewbookingMenuItemText + " " + DtbBookingModule.MenuItemDefaultText, shouldBeQuote: false, shouldBeMaster: false);
					CheckMenuAndTransportBookingOnOpenedFormThenDispose(newMenu, DtbBookingModule.QuotedbookingMenuItemText, shouldBeQuote: true, shouldBeMaster: false);

					if (masterBookingsEnabled)
					{
						CheckMenuAndTransportBookingOnOpenedFormThenDispose(newMenu, masterMenuItemToTest, shouldBeQuote: false, shouldBeMaster: true);
					}
					else
					{
						CheckMenuDoesNotExist(newMenu, masterMenuItemToTest);
					}
				}
			}
		}

		void CheckMenuAndTransportBookingOnOpenedFormThenDispose(IEnumerable<MenuItem> newMenu, string subMenuText, bool shouldBeQuote, bool shouldBeMaster)
		{
			var menuItem = newMenu.FirstOrDefault(m => m.Text == subMenuText);
			AssertNotNull(menuItem);
			menuItem.PerformClick();
			var openedTransportBookingForm = ZApplication.GetOpenForms().OfType<TransportBookingForm>().Single();
			try
			{
				var newTransportBooking = (DtbBooking)openedTransportBookingForm.BusinessEntity;
				CombineAssertions(() =>
				{
					AssertEquals("New " + subMenuText + " should " + (shouldBeQuote ? string.Empty : "not ") + "be quote", shouldBeQuote, newTransportBooking.IsQuote);
					AssertEquals("New " + subMenuText + " should " + (shouldBeMaster ? string.Empty : "not ") + "be master", shouldBeMaster, newTransportBooking.KM_IsMaster);
				});
			}
			finally
			{
				openedTransportBookingForm.Dispose();
			}
		}

		void CheckMenuDoesNotExist(IEnumerable<MenuItem> newMenu, string subMenuText)
		{
			var menuItem = newMenu.FirstOrDefault(m => m.Text == subMenuText);
			AssertNull("New " + subMenuText + " menu item should not exist", menuItem);
		}

		protected override SecurityCheckpoint ExpectedSecurityCheckpoint
		{
			get { return Env.Security.DtbBooking; }
		}

		public void TestOnFactorySwapped()
		{
			AssertEquals("Default", DtbChildEditableServiceState.None, DtbChildEditableService.GetState(Factory));
			AssertEquals("Default", DtbFormState.Parent, DtbFormStateService.GetState(Factory));

			using (var module = new DtbBookingModuleForTest())
			{
				module.PerformSearchForTest();
				AssertEquals("Default", DtbChildEditableServiceState.Transport, DtbChildEditableService.GetState(module.FactoryForTest));
				AssertEquals("Form State of Module should be Booking", DtbFormState.Booking, DtbFormStateService.GetState(module.FactoryForTest));
			}
		}

		public void TestIOperationalActionSupportable_OperationalActionSupporter()
		{
			using (var module = new DtbBookingModuleForTest())
			{
				AssertEquals(typeof(DtbBookingOperationalActionSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}

		public void TestIImportCollectionInfoProvider()
		{
			using (var module = new DtbBookingModuleForTest())
			{
				var provider = (IImportCollectionInfoProvider)module;
				AssertEquals("DtbBookingModuleImportWizard", provider.ContextKey);
				var info = provider.ImportCollectionInfo;
				AssertEquals(typeof(ImportCollectionInfoImplForDtbBookingFlattened), info.GetType());

				var info2 = provider.ImportCollectionInfo;
				AssertNotEquals("Must be a different instance so the import starts fresh", info, info2);
				AssertNotEquals("Must be a different instance so the import starts fresh", info.Collection, info2.Collection);
			}
		}

		public void TestTransportBooking_WorkflowExceptionReport()
		{
			var branchPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var dtbBookingConsolidationPK = new CargoWise.Database.TestFramework.ObjectModel.DtbBookingConsolidation().InsertAndReturnObject(TestConnection).PK;
			var dtbBookingPK = new CargoWise.Database.TestFramework.ObjectModel.DtbBooking(dtbBookingConsolidationPK)
			{
				KM_JobID = "Test",
				KM_GB_Branch = branchPK
			}.InsertAndReturnObject(TestConnection).PK;
			var processTasksPK = new CargoWise.Database.TestFramework.ObjectModel.ProcessTasks(dtbBookingPK, "KM")
			{
				P9_Type = "EXC",
				P9_ActualDate = new DateTime(2016, 1, 1),
			}.InsertAndReturnObject(TestConnection).PK;

			using (var module = new DtbBookingModuleForTest())
			{
				AssertReportColumns(module, "Test", "");
			}
		}

		public void TestTransportBookingInstruction_WorkflowExceptionReport()
		{
			var branchPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var dtbBookingConsolidationPK = new CargoWise.Database.TestFramework.ObjectModel.DtbBookingConsolidation().InsertAndReturnObject(TestConnection).PK;
			var dtbBookingPK = new CargoWise.Database.TestFramework.ObjectModel.DtbBooking(dtbBookingConsolidationPK)
			{
				KM_JobID = "Test",
				KM_GB_Branch = branchPK
			}.InsertAndReturnObject(TestConnection).PK;
			var dtbBookingInstructionPK = new CargoWise.Database.TestFramework.ObjectModel.DtbBookingInstruction(dtbBookingPK)
			{
				KN_Sequence = 5
			}.InsertAndReturnObject(TestConnection).PK;
			var processTasksPK = new CargoWise.Database.TestFramework.ObjectModel.ProcessTasks(dtbBookingInstructionPK, "KN")
			{
				P9_Type = "EXC",
				P9_ActualDate = new DateTime(2016, 1, 1),
			}.InsertAndReturnObject(TestConnection).PK;

			using (var module = new DtbBookingModuleForTest())
			{
				AssertReportColumns(module, "Test", "Seq- 5");
			}
		}

		void AssertReportColumns(DtbBookingModuleForTest module, string jobNumber, string additionalDetail)
		{
			var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT JobModuleID, JobNumber, AdditionalDetail FROM Report_WorkflowExceptions((SELECT TOP 1 GC_PK FROM dbo.GlbCompany),'2016-01-01 00:00:00','2016-01-02 00:00:00')");
			AssertEquals("Result should have rows", 1, report.Rows.Count);
			AssertEquals("row['JobModuleID']", module.ID.Name, report.Rows[0]["JobModuleID"]);
			AssertEquals("row['JobNumber']", jobNumber, report.Rows[0]["JobNumber"]);
			AssertEquals("row['AdditionalDetail']", additionalDetail, report.Rows[0]["AdditionalDetail"]);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbBooking;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.Factory.NewWithValidTestData<DtbBooking>(TestBusinessObjectKind.MinimumRequiredToSave);
			collection.Factory.Save();
		}
	}

	public class JobShipmentBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (DtbBookingModule)ZModuleFactory.Instance.Create(ModuleIDs.DtbBooking);
		}

		protected override JobInvoicingPostingOption[] GetAllowedPostingOptions()
		{
			return new[] { JobInvoicingPostingOption.Costs };
		}
	}
}
