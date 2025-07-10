using System.IO;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(BorderLineReleaseAllocationForm))]
	sealed class BorderLineReleaseAllocationFormTest : ZFormBasherTest
	{
		public void TestAllocationButton_Click()
		{
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			setting.AddForTesting(GlbBranch.CurrentBranch.PK, 20001, 20002);
			var wrapper = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10001, 10003);
			setting.AddForTesting(GlbBranch.CurrentBranch.PK, 30001, 30010);
			var allocator = new BorderLineReleaseAllocator(wrapper);
			using (var form = new BorderLineReleaseAllocationForm(allocator))
			{
				form.Show();
				var allocateButton = form.Controls.Find("AllocateButton", true).OfType<ZButton>().First();
				using (var tempFile = TempFile.New())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					DeleteIfExists(tempFile.Filename);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					allocateButton.PerformClick();
					AssertEquals("Error Allocating Number", "Number to allocate must be greater than 0 and less than or equal to 15.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("File " + tempFile.Filename + " shouldn't have been created", false, File.Exists(tempFile.Filename));
					allocator.NumberToAllocate = 10;
					allocateButton.PerformClick();
					string confirmationMessage = "You are about to allocate '10' Entry Number(s) for Border Line Release.\n\nAre you sure you want to continue?";
					AssertEquals(confirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ZDialogResult.Cancel, UnitTestUserNotification.Instance.LastMessage.Answer);
					AssertEquals("File " + tempFile.Filename + " shouldn't have been created as the confirmation dialog was cancelled", false, File.Exists(tempFile.Filename));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					allocateButton.PerformClick();
					AssertEquals("10 Entry Number(s) have been allocated for Border Line Release.\r\nThe following file contains a list of these entry numbers.\r\n" + tempFile.Filename + "\r\nPlease provide these entry numbers to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertASCIIFileSameAsString(tempFile.Filename, @"00200010
00200028
00100012
00100020
00100038
00300018
00300026
00300034
00300042
00300059");
					DeleteIfExists(tempFile.Filename);
				}
			}

			using (var form = new BorderLineReleaseAllocationForm(allocator))
			{
				form.Show();
				var allocateButton = form.Controls.Find("AllocateButton", true).OfType<ZButton>().First();
				using (var tempFile = TempFile.New())
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					DeleteIfExists(tempFile.Filename);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					allocator.NumberToAllocate = 2;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					allocateButton.PerformClick();
					string confirmationMessage = "You are about to allocate '2' Entry Number(s) for Border Line Release.\n\nAre you sure you want to continue?";
					AssertEquals(confirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ZDialogResult.OK, UnitTestUserNotification.Instance.LastMessage.Answer);
					AssertEquals("File " + tempFile.Filename + " shouldn't have been created as the save dialog was cancelled", false, File.Exists(tempFile.Filename));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					allocateButton.PerformClick();
					AssertASCIIFileSameAsString(tempFile.Filename, @"00300067
00300075");
				}
			}
		}

		public void TestNumberToAllocateDoesntAllowDecimalPoint()
		{
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			setting.AddForTesting(GlbBranch.CurrentBranch.PK, 20001, 20002);
			var wrapper = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10001, 10003);
			setting.AddForTesting(GlbBranch.CurrentBranch.PK, 30001, 30010);
			var allocator = new BorderLineReleaseAllocator(wrapper);
			using (var form = new BorderLineReleaseAllocationForm(allocator))
			{
				form.Show();
				var numberControl = form.Controls.Find("NumberToAllocateZCalcEdit", true).OfType<ZArchitecture.ZCalcEdit>().FirstOrDefault();
				AssertNotNull(numberControl);
				AssertEquals(0, numberControl.DecimalPlaces); // NumberToAllocateZCalcEdit bound to ZLong type 'NumberToAllocate',no need to have Decimal Places
				allocator.NumberToAllocate = 5;
				AssertEquals(5, allocator.NumberToAllocate);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			var wrapper = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000);
			return new BorderLineReleaseAllocationForm(new BorderLineReleaseAllocator(wrapper));
		}
	}
}
