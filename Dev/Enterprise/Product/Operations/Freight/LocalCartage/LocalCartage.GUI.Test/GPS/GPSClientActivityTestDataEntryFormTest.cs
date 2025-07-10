using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(GPSClientActivityTestDataEntryForm))]
	public class GPSClientActivityTestDataEntryFormTest : ZFormBasherTest
	{
		public void TestErrorOnSave()
		{
			var truck = Factory.New<RefEquipment>();
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.EY_RQ_Truck = truck.PK;
			var collection = new GPSSupporterActivityTestDataCollection(workSheet);
			var newActivity = collection.AddNew();
			newActivity.EN_ActivityType = "TST";
			using (var form = new GPSClientActivityTestDataEntryForm(collection))
			{
				form.Show();
				var userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				MethodInfo group_ClickMethodInfo = form.GetType().GetMethod("OnSaveButton_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				group_ClickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
				AssertEquals("LastMessage Shown", true, userNotify.LastMessage.WasError);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			return new GPSClientActivityTestDataEntryForm(new GPSSupporterActivityTestDataCollection(workSheet));
		}
	}
}
