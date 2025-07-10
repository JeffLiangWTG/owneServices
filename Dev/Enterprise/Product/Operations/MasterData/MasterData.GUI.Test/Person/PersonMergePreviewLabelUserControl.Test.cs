using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Tests
{
	public class PersonMergePreviewLabelUserControlTest : TestCaseWithFactory
	{
		public void TestHeight()
		{
			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewLabelUserControl("Full Name", "PETER");
				form.Controls.Add(control);

				AssertEquals(13, control.Height);
			}
		}

		public void TestContent_DoublesHeight()
		{
			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewLabelUserControl("Address", "120 Graham Avenue\n2170");
				form.Controls.Add(control);

				AssertEquals(26, control.Height);
			}
		}

		public void TestContent_TriplesHeight()
		{
			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewLabelUserControl("Address", "120 Graham Avenue\n2170\nAustralia");
				form.Controls.Add(control);

				AssertEquals(39, control.Height);
			}
		}

		public void TestContent()
		{
			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewLabelUserControl("Full Name", "PETER");
				form.Controls.Add(control);

				AssertEquals("Full Name :", control.HumanReadableNameLabel.Text);
				AssertEquals("PETER", control.ValueLabel.Text);
				Assert(control.HumanReadableNameLabel.IsFontBold);
			}
		}
	}
}
