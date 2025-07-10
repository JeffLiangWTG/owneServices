using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(StatementsStampDutyController))]
	public class StatementsStampDutyControllerTest : Customs.Module.Testing.StatementControllerTest
	{
		public override void TestDeleteForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestEditForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestNewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestViewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert("Not Implemented", true);
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.TR.StatementsStampDuty, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(CusStatementHeader), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			AssertEquals(true, controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		[TestDate(2022, 11, 05)]
		public void TestGetForm()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var controller = new StatementsStampDutyController();
			using (var form = controller.ShowNewForm())
			{
				CombineAssertions("Statements Stamp Duty Controller", () =>
				{
					AssertEquals("Warning", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The monthly statement has been created for this month"));
					AssertNull(form);
				});
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.GlobalManifest;
			Factory.Save();
			return statement;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.GlobalManifest;
			statement.B2_GC = GlbCompany.CurrentCompany.PK;
			statement.B2_IsMonthlyStatement = ZBool.True;
			Factory.Save();

			controller = new StatementsStampDutyController();
		}

		StatementsStampDutyController controller;
	}
}
