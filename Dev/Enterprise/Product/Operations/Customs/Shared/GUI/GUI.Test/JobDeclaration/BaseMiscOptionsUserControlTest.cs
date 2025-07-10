using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseMiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExist()
		{
			using (var control = new BaseMiscOptionsUserControl())
			{
				TestHelper.AssertControlExists(control, "PaymentPartyDropEdit", "JE_PaymentMethod");
				TestHelper.AssertControlExists(control, "BranchGuidFindBox", "JE_GB");
				TestHelper.AssertControlExists(control, "MergeByDropEdit", "JE_MergeBy");
				TestHelper.AssertControlExists(control, "BrokerCodeFindBox", "JE_GS_NKCusAgent");
				TestHelper.AssertControlExists(control, "PaidByDropEdit", "JE_PaidBy");
			}
		}

		public void TestPaidByDropEditVisibility()
		{
			AssertPaidByDropEditVisibility(DeclarationApplicationCodeList.Codes.Interfaced, true);
			AssertPaidByDropEditVisibility(DeclarationApplicationCodeList.Codes.Builtin, false);
		}

		public void TestChangeControlVisibility()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MessageType = "EXP";
			using (var testUserControl = new TestMiscOptionsUserControl())
			{
				testUserControl.JobDeclaration = testDec;
				testDec.JE_MessageType = "IMP";
				AssertEquals("Event handler called", true, testUserControl.ControlChanged);
			}
		}

		public class TestMiscOptionsUserControl : BaseMiscOptionsUserControl
		{
			public bool ControlChanged;
			protected override void ChangeControlVisibilityOnMessageTypeChanged()
			{
				base.ChangeControlVisibilityOnMessageTypeChanged();
				ControlChanged = true;
			}
		}

		void AssertPaidByDropEditVisibility(ZString applicationCode, bool isVisible)
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_ApplicationCode = applicationCode;
			using (var testUserControl = new BaseMiscOptionsUserControl())
			{
				testUserControl.JobDeclaration = testDec;
				var paidByDropEdit = testUserControl.FindSingle<ZDropEdit>("PaidByDropEdit");
				AssertEquals(isVisible, paidByDropEdit.Visible);
			}
		}
	}
}
