using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Module.Test
{
	[TestedType(typeof(PeriodicInvoicingController))]
	public abstract class PeriodicInvoicingControllerBasherTest : WhsControllerBaseBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(GetModuleID(), GetPeriodicInvoicingController().ModuleID);
		}

		protected abstract ModuleIdentifier GetModuleID();

		protected abstract PeriodicInvoicingController GetPeriodicInvoicingController();

		#endregion

		#region TestShowForm_IsNotNull

		#region TestShowEditForm_IsNotNull

		public void TestShowEditForm_IsNotNull()
		{
			TestShowForm_IsNotNull_Core((c, i) => c.ShowEditForm(i));
		}

		#endregion

		#region TestShowViewForm_IsNotNull

		public void TestShowViewForm_IsNotNull()
		{
			TestShowForm_IsNotNull_Core((c, i) => c.ShowViewForm(i));
		}

		#endregion

		#region TestShowDeleteForm_IsNotNull

		public void TestShowDeleteForm_IsNotNull()
		{
			var controller = GetPeriodicInvoicingController();
			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = controller.ShowDeleteForm(invoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, invoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", form);
			}
		}

		#endregion

		void TestShowForm_IsNotNull_Core(Func<PeriodicInvoicingController, PeriodicInvoicing, IZForm> showForm)
		{
			var controller = GetPeriodicInvoicingController();
			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = showForm(controller, invoice))
			{
				AssertNotNull("Form should have been opened.", form);
			}
		}

		public void TestShowEditForm_Context()
		{
			TestShowForm_Context_Core((c, i) => c.ShowEditForm(i));
		}

		public void TestShowViewForm_Context()
		{
			TestShowForm_Context_Core((c, i) => c.ShowViewForm(i));
		}

		public void TestShowDeleteForm_Context()
		{
			var controller = GetPeriodicInvoicingController();
			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = controller.ShowDeleteForm(invoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, invoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", form);
			}
		}

		void TestShowForm_Context_Core(Func<PeriodicInvoicingController, PeriodicInvoicing, IZForm> showForm)
		{
			var controller = GetPeriodicInvoicingController();
			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = showForm(controller, invoice))
			{
				Assert(form.BusinessEntityForPersistingForm.Factory.HasContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb));
			}
		}

		#endregion

		#region TestShowLoadedForm_InViewModeWithNoJobHeader

		public void TestShowLoadedForm_InViewModeWithNoJobHeader()
		{
			var controller = GetPeriodicInvoicingController();
			var invoice1 = Factory.NewWithValidTestData<PeriodicInvoicing>();
			var invoice2 = Factory.NewWithValidTestData<PeriodicInvoicing>();
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader1.JH_ParentID = invoice1.PK;
			Factory.Save();

			using (var form = controller.ShowEditForm(invoice1))
			{
				AssertEquals("DisplayMode should be Browse", ODisplayMode.Browse, form.DisplayMode);
			}

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertNull("Should not open form if user answers 'No'.", controller.ShowEditForm(invoice2));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = controller.ShowEditForm(invoice2))
			{
				AssertEquals("DisplayMode should be ReadOnly", ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}

		#endregion

		#region TestShowDeleteForm_WithNoJobHeader

		public void TestShowDeleteForm_WithNoJobHeader()
		{
			var controller = GetPeriodicInvoicingController();
			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			Factory.Save();

			AssertNull("Precondition: No Job Header.", invoice.JobHeader);

			using (var form = controller.ShowDeleteForm(invoice))
			{
				AssertNotNull("Precondition: Form opened.", form);
				AssertEquals("DisplayMode should be Delete.", ODisplayMode.Delete, form.DisplayMode);
			}
		}

		#endregion

		#region TestShowLoadedForm_SetFromDateReadOnlyIfFirstBilling

		public void TestShowLoadedForm_SetFromDateReadOnlyIfFirstBilling()
		{
			var controller = GetPeriodicInvoicingController();
			using (var form = controller.ShowNewForm())
			{
				AssertEquals(false, ((PeriodicInvoicing)((ZForm)form).BusinessEntity).DefaultStorageFromDateCalculated);
			}

			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			ReloadInvoiceAndAssertFromDateReadOnlyIsSetOnShowFormIfFirstBilling(invoice.PK, (c, i) => c.ShowEditForm(i));
			ReloadInvoiceAndAssertFromDateReadOnlyIsSetOnShowFormIfFirstBilling(invoice.PK, (c, i) => c.ShowViewForm(i));

			var reloadedInvoice = new BusinessObjectFactory().Load<PeriodicInvoicing>(invoice.PK);
			AssertEquals(false, reloadedInvoice.DefaultStorageFromDateCalculated);

			using (var deleteForm = GetPeriodicInvoicingController().ShowDeleteForm(reloadedInvoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, reloadedInvoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", deleteForm);
			}
		}

		void ReloadInvoiceAndAssertFromDateReadOnlyIsSetOnShowFormIfFirstBilling(ZGuid invoicePK, Func<PeriodicInvoicingController, PeriodicInvoicing, IZForm> showForm)
		{
			var reloadedInvoice = new BusinessObjectFactory().Load<PeriodicInvoicing>(invoicePK);
			AssertEquals(false, reloadedInvoice.DefaultStorageFromDateCalculated);

			using (showForm(GetPeriodicInvoicingController(), reloadedInvoice))
			{
				AssertEquals(true, reloadedInvoice.DefaultStorageFromDateCalculated);
			}
		}

		#endregion

		public override void TestEditForm()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			base.TestEditForm();
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<PeriodicInvoicing>();
			Factory.Save();
			return bizO;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsInvoicing;
		}

		#endregion
	}
}
