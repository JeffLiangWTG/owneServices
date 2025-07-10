using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ZStmNotePopupEditWithBindableTextTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			new ZStmNotePopupEditWithBindableText().Dispose();
		}

		[ExpectNoExceptions]
		public void TestShow()
		{
			using (ZTestForm form = new ZTestForm())
			{
				form.Controls.Add(control);
				form.Show();
			}
		}

		public void TestButtonText()
		{
			AssertEquals("Default Button Text", "More...", Control.ButtonText);

			Control.ButtonText = "Blah";
			AssertEquals("New Button Text", "Blah", Control.ButtonText);
		}

		public void TestButtonTextIsTranslatable()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(new ResourceStringGetter(delegate(string key)
				{ return new ResourceStringData(key, "!@#$%^"); }));
				AssertEquals("!@#$%^", Control.ButtonText);
			}
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
		}

		ZStmNotePopupEditWithBindableText Control
		{
			get { return control ?? (control = new ZStmNotePopupEditWithBindableText()); }
		}

		ZStmNotePopupEditWithBindableText control;

		#endregion
	}
}
