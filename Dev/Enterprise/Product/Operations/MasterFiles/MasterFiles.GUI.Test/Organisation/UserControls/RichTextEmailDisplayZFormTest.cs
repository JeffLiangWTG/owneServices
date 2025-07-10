using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RichTextEmailDisplayZForm))]
	sealed class RichTextEmailDisplayZFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		[RequiresSTA]
		public void TestDisplay()
		{
			RichTextEmailDisplayZForm form = null;
			AssertNoExceptionThrown(() => form = new RichTextEmailDisplayZForm("Test URL is http://testurl.com", "Test Title"));

			using (form)
			{
				form.Show();

				AssertEquals("EmailViewerTextBox should not have ToolBar", false, form.EmailViewerTextBox.IsToolBarVisible);
				#if !WINZOR
				Assert("Should contain URL", form.EmailViewerTextBox.Rtf.IndexOf("http://testurl.com") != -1);
				#else
				Assert("Should contain URL", form.EmailViewerTextBox.Html.IndexOf("http://testurl.com") != -1);
				#endif
			}
		}

		[RequiresSTA]
		public void TestCreateRichTextFile()
		{
			int initialFileCount = Directory.GetFiles(Temp.TempPath).Length;

			using (var form = new RichTextEmailDisplayZForm(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Body text}", "Title?"))
			{
				form.Show();
				int runningFileCount = Directory.GetFiles(Temp.TempPath).Length;
				AssertEquals("Form caption should be", "Title?", form.FormCaption);
				AssertEquals("One new file should have been created", initialFileCount + 1, runningFileCount);
			}

			int finalFileCount = Directory.GetFiles(Temp.TempPath).Length;
			AssertEquals("New file should have been removed", initialFileCount, finalFileCount);
		}

		[RequiresSTA]
		public void TestFromFile()
		{
			string tempFile = Temp.GetTempFileNameWithExtension("rtf");
			try
			{
				int initialFileCount = Directory.GetFiles(Temp.TempPath).Length;

				using (var form = RichTextEmailDisplayZForm.FromFile(tempFile, "meh title?"))
				{
					form.Show();
					Application.DoEvents();

					int runningFileCount = Directory.GetFiles(Temp.TempPath).Length;
					AssertEquals("Form caption should be", "meh title?", form.FormCaption);
					AssertEquals("No new files should have been created", initialFileCount, runningFileCount);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RichTextEmailDisplayZForm(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Body text}", Res.GetString("81059e79-8917-4d92-8770-fdeddc8fef30", "Title"));
		}

		#endregion
	}
}
