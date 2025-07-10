using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(LooseBookedMoveSplitForm))]
	public class LooseBookedMoveSplitFormTest : ZFormBasherTest
	{
		public void TestNoDimensions()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonBookedCtgMove move = cartage.LooseBookedMoves[0];
			move.EW_BookedPackCount = 10;
			move.EW_BookedWeight = 20;
			move.EW_BookedVolume = 6;
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			using (LooseBookedMoveSplitForm form = new LooseBookedMoveSplitForm(master))
			{
				form.Show();
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.OK);
				MethodInfo group_ClickMethodInfo = form.GetType().GetMethod("SaveButton_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				group_ClickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
				AssertEquals("LastMessage Shown", true, userNotify.LastMessage.WasNone);
			}
		}

		public void TestHasDimensions()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonBookedCtgMove move = cartage.LooseBookedMoves[0];
			move.EW_BookedPackCount = 10;
			move.EW_BookedWeight = 20;
			move.EW_BookedVolume = 6;
			move.EW_BookedHeight = 1;
			move.EW_BookedLength = 2;
			move.EW_BookedWidth = 3;
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			using (LooseBookedMoveSplitForm form = new LooseBookedMoveSplitForm(master))
			{
				form.Show();
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.OK);
				MethodInfo group_ClickMethodInfo = form.GetType().GetMethod("SaveButton_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				group_ClickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
				AssertEquals("LastMessage Shown", "The original Loose Booked Move has dimensions that are unable to be split. Please split manually.", userNotify.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			return new LooseBookedMoveSplitForm(master);
		}
	}
}
