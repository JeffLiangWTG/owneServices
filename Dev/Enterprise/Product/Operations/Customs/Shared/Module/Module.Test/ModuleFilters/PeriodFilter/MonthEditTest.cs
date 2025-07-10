using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Module.Testing
{
	sealed class MonthEditTest : TestCaseWithFactory
	{
		public void TestMaxLength()
		{
			AssertEquals(2, monthEdit.MaxLength);
		}

		public void TestIsCalculatorEnabled()
		{
			AssertEquals(false, monthEdit.IsCalculatorEnabled);
		}

		public void TestShowGroupSeparators()
		{
			AssertEquals(false, monthEdit.ShowGroupSeparators);
		}

		public void TestText()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", ZString.Empty, monthEdit.Text);
				monthEdit.Text = ZString.Empty;
				AssertEquals("Pad with 00", "00", monthEdit.Text);
				monthEdit.Text = "5";
				AssertEquals("Pad with 0", "05", monthEdit.Text);
				monthEdit.Text = "54";
				AssertEquals("Max length no pad", "54", monthEdit.Text);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			monthEdit = new MonthEdit();
		}

		MonthEdit monthEdit;
		protected override void TearDown()
		{
			base.TearDown();
			monthEdit?.Dispose();
		}
	}
}
