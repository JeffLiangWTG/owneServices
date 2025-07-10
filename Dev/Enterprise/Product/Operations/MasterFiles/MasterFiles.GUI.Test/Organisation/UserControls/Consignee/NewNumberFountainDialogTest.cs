using System;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.NumberFountain.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(NewNumberFountainDialog))]
	sealed class NewNumberFountainDialogTest : ZFormBasherTest
	{
		readonly NumberFountainForTest fountain = new NumberFountainWithStartNumForTest(22);
		protected override Form GetFormToBashCore()
		{
			return new NewNumberFountainDialog(fountain.Wrap());
		}

		public void TestNumberSizeRestrictions()
		{
			var connection = ((IDbConnectionInternals)Db.Connection).InternalDbConnection;
			var transaction = ((IDbConnectionInternals)Db.Connection).InternalDbTransaction;
			NumberFountainWithStartNumForTest fountain = new NumberFountainWithStartNumForTest(22);
			using (NewNumberFountainDialog dialog = new NewNumberFountainDialog(fountain.Wrap()))
			{
				AssertEquals("Current fountain number should be 22", "22", fountain.PeekPreliminaryFormatted(connection, transaction));
				dialog.Show();
				dialog.NewSequenceNumberCalcEdit.Text = "5" + int.MaxValue;
				dialog.OKButton_Click(null, EventArgs.Empty);
				AssertEquals("INVALID", dialog.NewSequenceNumberCalcEdit.Text);

				dialog.NewSequenceNumberCalcEdit.Text = "0";
				dialog.OKButton_Click(null, EventArgs.Empty);
				AssertEquals("INVALID", dialog.NewSequenceNumberCalcEdit.Text);
			}
		}

		public void TestStartNumForTest()
		{
			var connection = ((IDbConnectionInternals)Db.Connection).InternalDbConnection;
			var transaction = ((IDbConnectionInternals)Db.Connection).InternalDbTransaction;
			NumberFountainWithStartNumForTest fountain = new NumberFountainWithStartNumForTest(22);
			using (NewNumberFountainDialog dialog = new NewNumberFountainDialog(fountain.Wrap()))
			{
				AssertEquals("Current fountain number should be 22", "22", fountain.PeekPreliminaryFormatted(connection, transaction));
				dialog.Show();
				dialog.NewSequenceNumberCalcEdit.Text = "555";
				dialog.OKButton_Click(null, EventArgs.Empty);
				AssertEquals("Current fountain number should be 555", "555", fountain.PeekPreliminaryFormatted(connection, transaction));
			}

			using (NewNumberFountainDialog dialog = new NewNumberFountainDialog(fountain.Wrap()))
			{
				AssertEquals("Current fountain number should be 555", "555", fountain.PeekPreliminaryFormatted(connection, transaction));
				dialog.Show();
				dialog.NewSequenceNumberCalcEdit.Text = "23";
				dialog.CloseButton_Click(null, EventArgs.Empty);
				AssertEquals("Current fountain number should be 555", "555", fountain.PeekPreliminaryFormatted(connection, transaction));
			}
		}

		public void TestCurrentStartNumberDisplayed()
		{
			NumberFountainWithStartNumForTest fountain = new NumberFountainWithStartNumForTest(33);
			using (NewNumberFountainDialog dialog = new NewNumberFountainDialog(fountain.Wrap()))
			{
				AssertEquals("Current Sequence Number is: 33", dialog.CurrentSequenceNumberLabel.Text);
			}
		}
	}
}
