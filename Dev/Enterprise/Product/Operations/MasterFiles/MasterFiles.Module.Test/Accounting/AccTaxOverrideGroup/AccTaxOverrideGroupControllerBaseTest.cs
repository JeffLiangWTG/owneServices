using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class AccTaxOverrideGroupControllerBaseTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			Factory.Save();
			return taxOverrideGroup;
		}

		[RequiresSTA]
		public void TestShowCopyForm()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_Code = "Test1";
			taxOverrideGroup.AX_Description = "Test1 Desc";
			Factory.Save();

			Env.Security.TaxOverrideGroupsCopy.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZController controller = ZControllerFactory.Create(ControllerIDs.AccTaxOverrideGroup);

			using (IZForm form = controller.ShowTemplateCopyForm(taxOverrideGroup))
			{
				AssertNotNull("Should have returned a form", form);
				AssertEquals("Should have returned the correct form", typeof(AccTaxOverrideGroupForm), form.GetType());
				AccTaxOverrideGroupForm taxOverrideGroupForm = (AccTaxOverrideGroupForm)form;
				AccTaxOverrideGroup copiedAccChargeCode = (AccTaxOverrideGroup)taxOverrideGroupForm.BusinessEntity;
				AssertEquals("New Charge Code must be copy of previous Charge Code", taxOverrideGroup.AX_Description, copiedAccChargeCode.AX_Description);
				Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
			}

			Env.Security.TaxOverrideGroupsCopy.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			controller = ZControllerFactory.Create(ControllerIDs.AccTaxOverrideGroup);

			using (IZForm form = controller.ShowTemplateCopyForm(taxOverrideGroup))
			{
				AssertNull("Should not return a form", form);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new AccTaxOverrideGroupControllerForTest();
			AssertEquals("For New", Env.Security.TaxOverrideGroupsNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.TaxOverrideGroups, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.TaxOverrideGroupsModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.TaxOverrideGroupsDelete, controller.CheckPointForDelete);
		}
	}
}
