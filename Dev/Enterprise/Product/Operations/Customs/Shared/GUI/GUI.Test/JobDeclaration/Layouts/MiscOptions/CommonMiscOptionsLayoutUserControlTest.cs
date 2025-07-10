using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommonMiscOptionsLayoutUserControlTest : TestCase
	{
		public void TestPaidByDropEdit()
		{
			AssertType<ZDropEdit>(control.PaidByDropEdit);
		}

		public void TestPaymentPartyDropEdit()
		{
			AssertType<ZDropEdit>(control.PaymentPartyDropEdit);
		}

		public void TestBranchGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.BranchGuidFindBox);
		}

		public void TestMergeByDropEdit()
		{
			AssertType<ZDropEdit>(control.MergeByDropEdit);
		}

		public void TestBrokerCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.BrokerCodeFindBox);
		}

		public void TestRelatedDeclarationsUserControl()
		{
			AssertType<BaseRelatedDeclarationsUserControl>(control.RelatedDeclarationsUserControl);
		}

		public void TestMiscellaneousOptionsSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.MiscellaneousOptionsSeparatorUserControl);
		}

		public void TestEntryAuthorisationDateEdit()
		{
			AssertType<ZDateEdit>(control.EntryAuthorisationDateEdit);
		}

		public void TestRepresentationDropEdit()
		{
			AssertType<ZDropEdit>(control.RepresentationDropEdit);
		}

		public void TestDefermentAccountNumberTextBox()
		{
			CombineAssertions(() =>
			{
				var userControl = control.DefermentAccountNumberTextBox;
				AssertType<ZTextBox>("Type", userControl);
				AssertEquals("BindTo", nameof(BaseJobDeclaration.JE_DefermentAccountNumber), userControl.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CommonMiscOptionsLayoutUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		CommonMiscOptionsLayoutUserControl control;
	}
}
