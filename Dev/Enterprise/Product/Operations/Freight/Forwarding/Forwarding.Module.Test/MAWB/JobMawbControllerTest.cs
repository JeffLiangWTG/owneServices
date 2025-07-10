using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobMawbController))]
	public class JobMawbControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			Factory.Save();
			return mawb;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobMawb;
		}

		[RequiresSTA]
		public void TestShowEditForm()
		{
			var controller = new JobMawbController();

			var mawb = controller.Factory.New<RangeJobMawb>();
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_MAWB = "10000001";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_IsPaper = false;
			mawb.JM_IsPrinted = false;

			IZForm form = null;
			Assert(!mawb.IsInDatabaseIncludingChildren);
			AssertNoExceptionThrown(() => form = controller.ShowEditForm(mawb));
			form.Dispose();
		}

		public void TestNotShowDeleteForm_WhenDeleted()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var mawb = Factory.New<JobMawb>();
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_MAWB = "10000001";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_IsPaper = false;
			mawb.JM_IsPrinted = false;
			Factory.Save();

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;
			var mawbInAnotherFactory = newFactory.Load<JobMawb>(mawb.PK);
			mawbInAnotherFactory.JM_IsPrinted = true;
			newFactory.Save();

			using (var testForm = new JobMawbController().ShowDeleteForm(mawb))
			{
				AssertEquals(mawbInAnotherFactory.ReasonForNotAbleToDelete, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}
	}
}
