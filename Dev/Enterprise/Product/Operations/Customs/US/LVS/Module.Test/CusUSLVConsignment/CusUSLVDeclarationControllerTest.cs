using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVDeclarationController))]
	public class CusUSLVDeclarationControllerTest : ZControllerBasherTest
	{
		public void TestModuleResultsPKCollectionContainsBothLowValueBillAndJobDeclaration()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.US_EntryType = EntryTypeList.Codes.LowValue;
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Import;

			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("View Bill");
				AssertNotNull(viewBillMenuItem);
				viewBillMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				var declarationForm = openForms.Single(x => x.GetType() == typeof(JobDeclarationForm));
				var moduleResultsBusinessObject = ((IBusinessForm)declarationForm).ModuleResultsBusinessObject;
				AssertContainsExactElementsInAnyOrder("ModuleResultsPKCollection contains both Low Value Bill PK and Job Declaration PK", new ZGuid[] { jobDeclaration.PK, consignment.PK }, moduleResultsBusinessObject.PKList.Select(x => x.PK));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(JobDeclarationForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestOverrideProperties()
		{
			var controller = new CusUSLVDeclarationController();
			CombineAssertions(() =>
			{
				AssertEquals(ControllerIDs.Customs.US.USLowValueEntriesDeclaration, controller.ID);
				AssertEquals(ModuleIDs.Customs.US.USLowValueEntriesDeclaration, controller.ModuleID);
				AssertEquals(typeof(USConsignmentCombined), controller.TypeOfTopLevelBusinessObject);
				AssertEquals("URLs should only be openable for current country", true, controller.MakeUrlsOnlyOpenableForCurrentCompany);
				AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(null));
				AssertEquals(Env.Security.USLVClearanceEdit, controller.GetCheckPointForEdit(null));
				AssertEquals(Env.Security.None, controller.GetCheckPointForNew(null));
				AssertEquals(Env.Security.USLVClearanceView, controller.GetCheckPointForView(null));
			});
		}

		public override void TestDeleteForm()
		{
			Assert("Should not be able to delete a form from this controller", true);
		}

		public override void TestNewForm()
		{
			Assert("Should not be able to create a form from this controller", true);
		}

		public override void TestTemplateCopyForm()
		{
			Assert("Should not be able to template copy a form from this controller", true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USLowValueEntriesDeclaration;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.US_EntryType = EntryTypeList.Codes.LowValue;
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Import;

			Factory.Save();

			return jobDeclaration;
		}
	}
}
