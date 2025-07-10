using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.IO;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ImageDialog))]
	class ImageDialogFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var imageDialog = new ImageDialog();
			using (var directory = new TempDirectory())
			{
				imageDialog.HtmlEditorTempFilePath = directory.DirectoryName;

				imageDialog.Element = new ImageElementWithMacro();
			}
			ExcludeFromMissingResourceStringTest(imageDialog, "chkBorderColor");
			ExcludeFromMissingResourceStringTest(imageDialog, "txtAlt");
			ExcludeFromMissingResourceStringTest(imageDialog, "txtBgColor");
			ExcludeFromMissingResourceStringTest(imageDialog, "txtHeight");
			ExcludeFromMissingResourceStringTest(imageDialog, "txtToolTip");
			ExcludeFromMissingResourceStringTest(imageDialog, "txtURL");
			ExcludeFromMissingResourceStringTest(imageDialog, "txtWidth");

			var matchingControls = imageDialog.Controls.Find("txtURL", true);
			if (matchingControls.Length > 0)
			{
				matchingControls[0].Enabled = false;
			}

			return imageDialog;
		}

		protected void ExcludeFromMissingResourceStringTest(Form form, string name)
		{
			var matchingControls = form.Controls.Find(name, true);
			if (matchingControls.Length > 0)
			{
				MissingResourceStringChecker.ExcludeFromTest(matchingControls[0]);
			}
		}

		public void TestBorderWidthValidation()
		{
			using (var imageDialog = new ImageDialog())
			{
				imageDialog.Element = new ImageElementWithMacro();

				imageDialog.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				var borderThicknessCheckBox = ControlTestHelper.FindControls<ZCheckBox>(imageDialog).SingleOrDefault(x => x.Name == "chkBorderThickness");
				borderThicknessCheckBox.Checked = true;

				var borderTextBox = ControlTestHelper.FindControls<ZTextBox>(imageDialog).SingleOrDefault(x => x.Name == "txtBorder");
				borderTextBox.Focus();
				borderTextBox.Text = "bad";
				borderThicknessCheckBox.Focus();

				Assert("Bad Entry", UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				borderTextBox.Focus();
				borderTextBox.Text = "THIN";
				borderThicknessCheckBox.Focus();
				Assert("Good Entry", UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Thin Value", "thin", borderTextBox.Text);

				borderTextBox.Focus();
				borderTextBox.Text = "50bad";
				borderThicknessCheckBox.Focus();
				Assert("Bad unit", UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				borderTextBox.Focus();
				borderTextBox.Text = "50PX";
				borderThicknessCheckBox.Focus();
				Assert("Good Unit", UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("px unit", "50px", borderTextBox.Text);

				borderTextBox.Focus();
				borderTextBox.Text = "70";
				borderThicknessCheckBox.Focus();
				Assert("Good Unit", UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("default px unit", "70px", borderTextBox.Text);
			}
		}

		public void TestInternetUrlValidation()
		{
			using (var imageDialog = new ImageDialog())
			using (var tempDirectory = new TempDirectory())
			{
				var fileName = new Uri(Path.Combine(tempDirectory.DirectoryName, "logo.svg")).LocalPath;
				imageDialog.Element = new ImageElementWithMacro();

				imageDialog.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				var txtURL = imageDialog.FindAll<ZTextBox>(control => control.Name.Equals("txtURL")).First();
				var rdInternetURL = imageDialog.FindAll<ZRadioButton>(control => control.Name.Equals("rdInternetURL")).First();
				rdInternetURL.Checked = true;

				txtURL.Focus();
				txtURL.Text = fileName;
				rdInternetURL.Focus();

				AssertEquals("Internet URL Invalid", "Only accept HTTP(S) image for internet URL", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				txtURL.Focus();
				txtURL.Text = "https://wallpaperscraft.com/image.jpg";
				rdInternetURL.Focus();
				Assert("No error when use HTTP(S) images", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestLocalFileValidation_SupportedFile()
		{
			AssertLocalFileValidation(fileName: @"C:\1.png", expectedTxtUrlText: @"C:\1.png", expectedErrorMessage: null);
		}
		public void TestLocalFileValidation_UnsupportedFile()
		{
			AssertLocalFileValidation(fileName: @"C:\1.svg", expectedTxtUrlText: string.Empty, expectedErrorMessage: "Only image extensions jpg, jpeg, gif, png or bmp are supported");
		}

		void AssertLocalFileValidation(string fileName, string expectedTxtUrlText, string expectedErrorMessage)
		{
			using (var imageDialog = new ImageDialog())
			{
				imageDialog.Element = new ImageElementWithMacro();
				imageDialog.HtmlEditorGuiEvent += (object sender, EventArgs e) =>
				{
					if (e is UploadLocalImageEventArgs uploadLocalImageEventArgs)
					{
						uploadLocalImageEventArgs.Dialog = new DummyZOpenFileDialog(fileName);
					}
				};

				imageDialog.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				var txtURL = imageDialog.FindAll<ZTextBox>(control => control.Name.Equals("txtURL")).First();
				var rdoLocalFile = imageDialog.FindAll<ZRadioButton>(control => control.Name.Equals("rdoLocalFile")).First();
				var rdInternetURL = imageDialog.FindAll<ZRadioButton>(control => control.Name.Equals("rdInternetURL")).First();
				var browseButton = imageDialog.FindAll<ZButton>(control => control.Name.Equals("btnBrowseFile")).First();
				rdoLocalFile.Checked = true;
				rdInternetURL.Checked = false;
				browseButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(expectedTxtUrlText, txtURL.Text);
				});
			}
		}

		public void TestClearTextWhenSwitching()
		{
			using (var imageDialog = new ImageDialog())
			{
				imageDialog.Element = new ImageElementWithMacro();

				imageDialog.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				var txtURL = imageDialog.FindAll<ZTextBox>(control => control.Name.Equals("txtURL")).First();
				var rdInternetURL = imageDialog.FindAll<ZRadioButton>(control => control.Name.Equals("rdInternetURL")).First();
				var rdoLocalFile = imageDialog.FindAll<ZRadioButton>(control => control.Name.Equals("rdoLocalFile")).First();
				rdInternetURL.Checked = true;

				txtURL.Focus();
				txtURL.Text = "https://wallpaperscraft.com/image.jpg";
				rdInternetURL.Focus();

				Assert("No error when use HTTP(S) images", UnitTestUserNotification.Instance.LastMessage.WasNone);

				rdoLocalFile.Checked = true;
				AssertEquals("Clear the URL text when switch", string.Empty, txtURL.Text);
			}
		}

		[GuiTest]
		public void TestUploadMacroSelection()
		{
			using (var imageDialog = new ImageDialog())
			using (var directory = new TempDirectory())
			{
				imageDialog.Element = new ImageElementWithMacro();
				imageDialog.HtmlEditorGuiEvent += ImageDialogTest_HtmlEditorGuiEvent;
				imageDialog.HtmlEditorTempFilePath = directory.DirectoryName;

				imageDialog.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				imageDialog.Focus();
				AssertEquals("Form has focus", true, imageDialog.ContainsFocus);

				SendKeyToImageDialog(imageDialog, Keys.Tab);
				SendKeyToImageDialog(imageDialog, Keys.Tab);
				SendKeyToImageDialog(imageDialog, Keys.Tab);
				SendKeyToImageDialog(imageDialog, Keys.Down);
				SendKeyToImageDialog(imageDialog, Keys.Down);
				SendKeyToImageDialog(imageDialog, Keys.Alt | Keys.U);
				SendKeyToImageDialog(imageDialog, Keys.Enter);
				SendKeyToImageDialog(imageDialog, Keys.Down);
				SendKeyToImageDialog(imageDialog, Keys.Right);
				SendKeyToImageDialog(imageDialog, Keys.Enter);

				AssertEquals("Image Macro", "(*Test*)", imageDialog.ImageMacro);
				AssertStartsWith("Temporary file", imageDialog.HtmlEditorTempFilePath, imageDialog.ImageLinkForMacro);
			}
		}

		protected void ImageDialogTest_HtmlEditorGuiEvent(object sender, EventArgs e)
		{
			switch (e)
			{
				case MacroImageMenuGroupInitEventArgs menuEventArgs:
					InitMacroImageUploadMenu(menuEventArgs.Menus);
					return;

				case UploadLocalImageEventArgs uploadLocalImageEventArgs:
					HandelUploadLocalImageEvent(uploadLocalImageEventArgs);
					return;
			}
		}

		void InitMacroImageUploadMenu(MacroImageMenuGroup menus)
		{
			var testMenuGroup = menus.AddMenuGroup("Test Menu Group");
			var picture = Bitmap.FromHicon(SystemIcons.Question.Handle);

			testMenuGroup.AddMenu("Test Menu", picture, "(*Test*)", "Question");
		}

		void HandelUploadLocalImageEvent(UploadLocalImageEventArgs uploadLocalImageEventArgs)
		{
		}

		void SendKeyToImageDialog(ImageDialog dialog, Keys key)
		{
			KeySender.PostKeyDown(dialog, key);
			Application.DoEvents();
			UserIdleWorker.Flush();
		}

		class DummyZOpenFileDialog : WrappedZOpenFileDialog, IOpenFileDialog
		{
			internal DummyZOpenFileDialog(string fileNameForTest) : base()
			{
				this.fileNameForTest = fileNameForTest;
			}

			readonly string fileNameForTest;

			DialogResult IOpenFileDialog.ShowDialog(IWin32Window dialog)
			{
				FileName = fileNameForTest;
				return DialogResult.OK;
			}
		}
	}
}
