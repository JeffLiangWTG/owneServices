using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefDocSourceForm))]
	sealed class RefDocSourceFormTest : ZFormBasherTest
	{
		public void TestFormIsFixedSingle()
		{
			RefDocSource docSource = Factory.New<RefDocSource>();
			using (RefDocSourceForm form = new RefDocSourceForm(docSource))
			{
				AssertEquals("Form should be FixedSingle, because otherwise the form doesn't resize for Events and Notes tabs", FormBorderStyle.FixedSingle, form.FormBorderStyle);
			}
		}

		public void TestSaveWithReadOnlyException()
		{
			var docSource = Factory.New<RefDocSource>();
			using (var form = new TestRefDocSourceFormWithReadOnlyException(docSource))
			{
				form.Show();
				AssertNoExceptionThrown(form.ClickSaveButton);
			}
		}

		[UseSnapshotProtection]
		public void TestSaveNewDocumentType()
		{
			RefDocSource docSource = Factory.NewWithValidTestData<RefDocSource>();
			using (TestRefDocSourceForm form = new TestRefDocSourceForm(docSource))
			{
				docSource.RDS_Code = "ZZZ";
				docSource.RDS_Desc = "ZZZZ";

				form.ClickSaveButton();
				AssertEquals("There's no confirm message pop up", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Test Classes

		class TestRefDocSourceForm : RefDocSourceForm
		{
			public TestRefDocSourceForm(RefDocSource docSource)
				: base(docSource)
			{
			}

			public new ContinueWithSave ShowPreSaveDialogs()
			{
				return base.ShowPreSaveDialogs();
			}

			public void ClickSaveButton()
			{
				PostingButtonsUserControl.SaveButton.PerformClick();
			}
		}

		class TestRefDocSourceFormWithReadOnlyException : RefDocSourceForm
		{
			public TestRefDocSourceFormWithReadOnlyException(RefDocSource docSource)
				: base(docSource)
			{
			}

			public new ContinueWithSave ShowPreSaveDialogs()
			{
				return base.ShowPreSaveDialogs();
			}

			public void ClickSaveButton()
			{
				PostingButtonsUserControl.SaveButton.PerformClick();
			}

			protected override void ShowNewForm()
			{
				var error = SqlExceptionBuilder.CreateSqlError(3906, 1, 1, "", "Failed to update database \"Odyssey_SD007\" because the database is read-only.", "", 1);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);

				throw exception;
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RefDocSourceForm(Factory.New<RefDocSource>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		#endregion
	}
}
