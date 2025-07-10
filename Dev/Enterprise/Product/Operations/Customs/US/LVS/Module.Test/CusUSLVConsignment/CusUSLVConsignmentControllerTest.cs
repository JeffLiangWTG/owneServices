using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVConsignmentController))]
	public class CusUSLVConsignmentControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USLowValueEntriesBill;

		public void TestOverrideProperties()
		{
			var controller = new CusUSLVConsignmentController();
			CombineAssertions(() =>
			{
				AssertEquals(ControllerIDs.Customs.US.USLowValueEntriesBill, controller.ID);
				AssertEquals(ModuleIDs.Customs.US.USLowValueEntriesBill, controller.ModuleID);
				AssertEquals(typeof(USConsignmentCombined), controller.TypeOfTopLevelBusinessObject);
				AssertEquals("URLs should only be openable for current country", true, controller.MakeUrlsOnlyOpenableForCurrentCompany);
				AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(null));
				AssertEquals(Env.Security.USLVClearanceEdit, controller.GetCheckPointForEdit(null));
				AssertEquals(Env.Security.None, controller.GetCheckPointForNew(null));
				AssertEquals(Env.Security.USLVClearanceView, controller.GetCheckPointForView(null));
			});
		}

		public void TestGetFormForBusinessEntity_Consignment()
		{
			var controller = new CusUSLVConsignmentController();

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			Factory.Save();

			var viewForConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);

			using (var form = controller.ShowEditForm(viewForConsignment))
			{
				AssertType<CusUSLVConsignmentForm>(form);
				var consignmentForm = form as CusUSLVConsignmentForm;
				AssertEquals(typeof(CusUSLVConsignment), consignmentForm.DataSourceType);
			}
		}

		public void TestGetFormForBusinessEntity_Declaration()
		{
			var controller = new CusUSLVConsignmentController();

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.US_EntryType = EntryTypeList.Codes.LowValue;
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Import;

			Factory.Save();

			var viewForDeclaration = Factory.Load<USConsignmentCombined>(jobDeclaration.PK);

			using (var form = controller.ShowEditForm(viewForDeclaration))
			{
				AssertType<JobDeclarationForm>(form);
				var declarationForm = form as JobDeclarationForm;
				AssertEquals(typeof(JobDeclaration), declarationForm.DataSourceType);
			}
		}

		public override void TestNewForm()
		{
			Assert("New Form not supported for View", true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			Factory.Save();

			return Factory.Load<USConsignmentCombined>(consignment.PK);
		}
	}
}
