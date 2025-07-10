using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(InvoicingController))]
	public class InvoicingControllerBasherTest : WhsControllerBaseBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.WhsInvoicing, new InvoicingController().ModuleID);
		}

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
			var controller = new InvoicingController();
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = new InvoicingController().ShowDeleteForm(invoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, invoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", form);
			}
		}

		#endregion

		void TestShowForm_IsNotNull_Core(Func<InvoicingController, WhsInvoice, IZForm> showForm)
		{
			var controller = new InvoicingController();
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = showForm(new InvoicingController(), invoice))
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
			var controller = new InvoicingController();
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = new InvoicingController().ShowDeleteForm(invoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, invoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", form);
			}
		}

		void TestShowForm_Context_Core(Func<InvoicingController, WhsInvoice, IZForm> showForm)
		{
			var controller = new InvoicingController();
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			using (var form = showForm(new InvoicingController(), invoice))
			{
				Assert(form.BusinessEntityForPersistingForm.Factory.HasContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb));
			}
		}

		#endregion

		#region TestShowLoadedForm_InViewModeWithNoJobHeader

		public void TestShowLoadedForm_InViewModeWithNoJobHeader()
		{
			var controller = new InvoicingController();
			var invoice1 = Factory.NewWithValidTestData<WhsInvoice>();
			var invoice2 = Factory.NewWithValidTestData<WhsInvoice>();
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
			var controller = new InvoicingController();
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
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
			var controller = new InvoicingController();
			using (var form = controller.ShowNewForm())
			{
				AssertEquals(false, ((WhsInvoice)((ZForm)form).BusinessEntity).DefaultStorageFromDateCalculated);
			}

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = invoice.PK;
			Factory.Save();

			ReloadInvoiceAndAssertFromDateReadOnlyIsSetOnShowFormIfFirstBilling(invoice.PK, (c, i) => c.ShowEditForm(i));
			ReloadInvoiceAndAssertFromDateReadOnlyIsSetOnShowFormIfFirstBilling(invoice.PK, (c, i) => c.ShowViewForm(i));

			var reloadedInvoice = new BusinessObjectFactory().Load<WhsInvoice>(invoice.PK);
			AssertEquals(false, reloadedInvoice.DefaultStorageFromDateCalculated);

			using (var deleteForm = new InvoicingController().ShowDeleteForm(reloadedInvoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, reloadedInvoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", deleteForm);
			}
		}

		void ReloadInvoiceAndAssertFromDateReadOnlyIsSetOnShowFormIfFirstBilling(ZGuid invoicePK, Func<InvoicingController, WhsInvoice, IZForm> showForm)
		{
			var reloadedInvoice = new BusinessObjectFactory().Load<WhsInvoice>(invoicePK);
			AssertEquals(false, reloadedInvoice.DefaultStorageFromDateCalculated);

			using (showForm(new InvoicingController(), reloadedInvoice))
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
			var bizO = Factory.NewWithValidTestData<WhsInvoice>();
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
