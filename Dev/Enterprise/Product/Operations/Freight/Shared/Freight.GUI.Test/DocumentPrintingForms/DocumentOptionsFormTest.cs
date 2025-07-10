using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(DummyDocumentOptionsForm))]
	sealed class DocumentOptionsFormTest : ZFormBasherTest
	{
		public void TestLayoutControls()
		{
			using (DocumentOptionsForm form = new DummyDocumentOptionsForm(new TestingBusinessObject(Factory)))
			{
				form.Show();
				Application.DoEvents();

				var top = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
				var halfTop = top / 2;

				var optionsGroupBox = (ZGroupBox)typeof(DocumentOptionsForm).GetField("OptionsGroupBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				AssertEquals("Options GroupBox Top", halfTop, optionsGroupBox.Top);
				AssertEquals("Options GroupBox height", top, optionsGroupBox.Height);
				AssertEquals("Options GroupBox text", "Options", optionsGroupBox.Text);
				AssertEquals("Caption", "Document Options", form.Text);

				var printButton = (ZButton)typeof(DocumentOptionsForm).GetField("PrintButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				AssertEquals("PrintButton top", halfTop + (2 * top), printButton.Top);

				var cancelPrintButton = (ZButton)typeof(DocumentOptionsForm).GetField("CancelPrintButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				AssertEquals("CancelPrintButton top", halfTop + (2 * top), cancelPrintButton.Top);

				var mainStatusBar = (ZStatusBar)typeof(DocumentOptionsForm).GetField("MainStatusBar", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(form);
				var zButtonHeight1 = ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
				var zButtonHeight2 = ControlDpiScalingHelper.ScaleToCurrentDpiY(25);

				var formHeight1 = halfTop + (3 * top) + (zButtonHeight1 * 2) + (2 * mainStatusBar.Height) + ControlDpiScalingHelper.ScaleToCurrentDpiY(1);
				var formHeight2 = halfTop + (3 * top) + (zButtonHeight2 * 2) + (2 * mainStatusBar.Height) + ControlDpiScalingHelper.ScaleToCurrentDpiY(1);

				AssertEquals("Form Height - " + form.Height, true, form.Height >= formHeight1 && form.Height <= formHeight2);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DummyDocumentOptionsForm(new TestingBusinessObject(Factory));
		}

		protected override bool AllowFormSizeFixed => true;

		class TestingBusinessObject : NonPersistentBusinessObject
		{
			public TestingBusinessObject(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		class DummyDocumentOptionsForm : DocumentOptionsForm
		{
			public DummyDocumentOptionsForm(TestingBusinessObject bO) : base(bO, Core.Constants.DataContext.None)
			{
			}

			protected override Control[] GetControls(Core.Constants.DataContext dataContext)
			{
				return System.Array.Empty<Control>();
			}
		}

		#endregion
	}
}
