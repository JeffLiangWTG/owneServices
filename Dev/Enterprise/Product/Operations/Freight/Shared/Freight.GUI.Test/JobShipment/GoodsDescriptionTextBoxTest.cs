using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class GoodsDescriptionTextBoxTest : TestCaseWithDummy
	{
		public void TestDefaultNoteTypeDescription()
		{
			AssertEquals(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, Control.NoteTypeDescription);
		}

		public void TestLeadingComment()
		{
			string expectedComment = "This popup allows you to enter the detailed goods description. Changing the detailed goods description will not affect the short description.";

			using (ZForm form = new ZForm(Dummy))
			{
				Control.SetBindingMember(DummyBizoSchema.Constants.Z0_Description);
				form.Controls.Add(Control);
				Control.SetDataBinding(Dummy, "");
				form.Show();

				Control.PerformButtonClick();
				AssertEquals(expectedComment, Control.LastShownPopup.LeadingComment);
			}
		}

		public void TestNoteDescriptionTextBoxWorksInMultipleLanguages()
		{
			using (var textBox = new GoodsDescriptionTextBox())
			{
				AssertEquals(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, textBox.NoteTypeDescription);
			}
			using (var mock = Res.UseMockData())
			{
				mock.SetResourceGetter(key => new ResourceStringData(key, "translated"));
				using (var textBox = new GoodsDescriptionTextBox())
				{
					AssertEquals("translated", textBox.NoteTypeDescription);
				}
			}
		}

		#region MaxLengthExceededLogging

		public void TestMaxLengthExceededLogging()
		{
			using (ZForm form = new ZForm(Dummy))
			{
				Control.SetBindingMember(DummyBizoSchema.Constants.Z0_Description);
				form.Controls.Add(Control);
				Control.SetDataBinding(Dummy, "");
				form.Show();

				var innerTextBox = Control.Controls.Find("textBox", true)[0] as ZTextBox;
				innerTextBox.Text = null;
				AssertEquals("null is handled correctly", 0, ErrorReporter.TotalErrorCount);
				innerTextBox.Text = new string('a', 100);
				AssertEquals("text of max length is handled correctly", 0, ErrorReporter.TotalErrorCount);
				innerTextBox.Text = new string('a', 101);
				CombineAssertions("text exceeding max length", () =>
				{
					AssertEquals("Text does not overflow", new string('a', 100), innerTextBox.Text);
					AssertEquals("no error", 0, ErrorReporter.TotalErrorCount);
				});
			}
		}

		#endregion

		#region Implementation

		new DummyEnterpriseBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>()); }
		}
		DummyEnterpriseBusinessObject dummy;

		GoodsDescriptionTextBox Control
		{
			get { return control ?? (control = new GoodsDescriptionTextBox()); }
		}
		GoodsDescriptionTextBox control;

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
		}

		#endregion
	}
}
