using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(JobMawbForm))]
	public class JobMawbFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			return new JobMawbForm(mawb);
		}

		public void TestPlugInsz()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = consol.PK;
			using (JobMawbForm mawbForm = new JobMawbForm(mawb))
			{
				mawbForm.Show();

				bool jobInvoicingPluginExists = false;

				foreach (ZArchitecture.PlugIn.ZPlugIn plugin in mawbForm.PlugIns.Instances)
				{
					if (plugin.GetType().FullName == "Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToFreight")
					{
						jobInvoicingPluginExists = true;
						break;
					}
				}
				Assert("Invoicing should be plugged in", jobInvoicingPluginExists);
			}
		}

		public void TestMAWB_IsPrintedReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mawb = Factory.New<JobMawb>();
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = consol.PK;

			using (var mawbForm = new JobMawbForm(mawb))
			{
				mawbForm.Show();
				var isPrintedCheckBox = mawbForm.Controls.Find("IsPrintedBoundCheckBox", true)[0] as ZCheckBox;
				Assert("Is printed should be readonly", isPrintedCheckBox.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestMAWB_IsPrintedReadOnly_ShouldBeFalse_WhenParentIDIsEmptyAndIsPrintedIsTrue()
		{
			var mawb = CreateMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");
			using (var mawbForm = new JobMawbForm(mawb))
			{
				mawbForm.Show();
				var isPrintedCheckBox = mawbForm.Controls.Find("IsPrintedBoundCheckBox", true)[0] as ZCheckBox;
				Assert("Is printed should not be readonly. This is to allow MAWBs that were previously unallocated from Consols but which didn't have JM_IsPrinted reset to be returned to stock.", !isPrintedCheckBox.ReadOnly);

				var referenceTypeLabel = mawbForm.Controls.Find("ReferenceTypeLabel", true)[0] as ZLabel;
				AssertEquals("Previously printed and un-allocated.", referenceTypeLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestMAWBUnallocation_Continue()
		{
			var mawb = CreateMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			using (new JobMawbForm(mawb))
			{
				Env.Security.JobMAWBResetPrintedFlag.IsAllowed = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				mawb.MAWB_IsPrinted = false;
				AssertEquals("Unchecking this box means this MAWB Number will be returned to the MAWB Stock. Do you want to continue?",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, mawb.JM_IsPrinted);
			}
		}

		[RequiresSTA]
		public void TestMAWBDeallocation_Cancel()
		{
			var mawb = CreateMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			using (new JobMawbForm(mawb))
			{
				Env.Security.JobMAWBResetPrintedFlag.IsAllowed = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				mawb.MAWB_IsPrinted = false;
				AssertEquals("Unchecking this box means this MAWB Number will be returned to the MAWB Stock. Do you want to continue?",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, mawb.JM_IsPrinted);
			}
		}

		public void TestMAWBDeallocation_UndoDeallocation()
		{
			Env.Security.JobMAWBResetPrintedFlag.IsAllowed = true;
			var mawb = CreateMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			using (var form = new JobMawbForm(mawb))
			{
				form.Show();
				var isPrintedCheckBox = form.Controls.Find("IsPrintedBoundCheckBox", true)[0] as ZCheckBox;
				Assert("Pre-condition: Is Printed checkbox is ticked", isPrintedCheckBox.Checked);
				Assert("Pre-condition: Is Printed checkbox is not readonly", !isPrintedCheckBox.ReadOnly);
				var referenceTypeLabel = form.Controls.Find("ReferenceTypeLabel", true)[0] as ZLabel;
				AssertEquals("Pre-condition: ReferenceTypeLabel", "Previously printed and un-allocated.", referenceTypeLabel.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				isPrintedCheckBox.Checked = false;
				AssertEquals("Unchecking this box means this MAWB Number will be returned to the MAWB Stock. Do you want to continue?",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, mawb.JM_IsPrinted);
				AssertEquals("Available to allocate.", referenceTypeLabel.Text);

				isPrintedCheckBox.Checked = false;
				AssertEquals(false, mawb.JM_IsPrinted);
				AssertEquals("Available to allocate.", referenceTypeLabel.Text);

				isPrintedCheckBox.Checked = true;
				AssertEquals(true, mawb.JM_IsPrinted);

				referenceTypeLabel = form.Controls.Find("ReferenceTypeLabel", true)[0] as ZLabel;
				AssertEquals("Previously printed and un-allocated.", referenceTypeLabel.Text);
			}
		}

		JobMawb CreateMawb(string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;
			mawb.JM_ParentID = ZGuid.Empty;
			mawb.JM_ParentTableCode = ZString.Empty;
			mawb.JM_IsPrinted = true;

			return mawb;
		}

		public void TestReferenceTypeLabelSize()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = ZGuid.Empty;
			mawb.JM_IsPrinted = true;

			using (var mawbForm = new JobMawbForm(mawb))
			{
				mawbForm.Show();
				var referenceTypeLabel = mawbForm.Controls.Find("ReferenceTypeLabel", true)[0] as ZLabel;
				Assert("ReferenceTypeLabel should be bigger than oe equal to 245", referenceTypeLabel.Size.Width >= 245);
			}
		}
	}
}
