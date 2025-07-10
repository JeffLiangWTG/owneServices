using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(ZForm))]
	public class DeduplicationPersonFilterControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var filterBizO = new DeduplicationPersonFilterBusinessObject();
			var collection = new DeduplicationPersonCollection(Factory);
			var form = new ZForm();
			form.CaptionRenderingEnabled = true;
			var control = new DeduplicationPersonFilterControl(collection, filterBizO);
			control.Dock = DockStyle.Fill;
			control.Name = "FilterControl";
			form.Controls.Add(control);
			form.ControllerID = DummyControllerIDs.Dummy;
			return form;
		}

		[RequiresSTA]
		public void TestGridHeaderCaptions()
		{
			using (Form)
			{
				Form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Number of potential duplicates with high confidence", GetResourceString("DPE_HighDuplicates").FullDescription);
					AssertEquals("Number of potential duplicates with medium confidence", GetResourceString("DPE_MediumDuplicates").FullDescription);
					AssertEquals("Number of potential duplicates with low confidence", GetResourceString("DPE_LowDuplicates").FullDescription);
					AssertEquals("Total number of potential duplicates found", GetResourceString("DPE_TotalDuplicates").FullDescription);
				});

				CombineAssertions(() =>
				{
					AssertEquals("High Confidence Results", GetResourceString("DPE_HighDuplicates").Caption);
					AssertEquals("Medium Confidence Results", GetResourceString("DPE_MediumDuplicates").Caption);
					AssertEquals("Low Confidence Results", GetResourceString("DPE_LowDuplicates").Caption);
					AssertEquals("Total Results", GetResourceString("DPE_TotalDuplicates").Caption);
				});

				CombineAssertions(() =>
				{
					AssertEquals("High Results", GetResourceString("DPE_HighDuplicates").MediumCaption);
					AssertEquals("Medium Results", GetResourceString("DPE_MediumDuplicates").MediumCaption);
					AssertEquals("Low Results", GetResourceString("DPE_LowDuplicates").MediumCaption);
					AssertEquals("", GetResourceString("DPE_TotalDuplicates").MediumCaption);
				});

				CombineAssertions(() =>
				{
					AssertEquals("High", GetResourceString("DPE_HighDuplicates").ShortCaption);
					AssertEquals("Medium", GetResourceString("DPE_MediumDuplicates").ShortCaption);
					AssertEquals("Low", GetResourceString("DPE_LowDuplicates").ShortCaption);
					AssertEquals("Total", GetResourceString("DPE_TotalDuplicates").ShortCaption);
				});

				ResourceStringData GetResourceString(string columnName) => (FilterControl.Grid.Columns[columnName].ColumnStyle as ZTextBoxColumnStyle).CaptionResourceString;
			}
		}

		[RequiresSTA]
		public void TestFilterGridColumns()
		{
			using (Form)
			{
				Form.Show();
				AssertEquals("Including shared columns", 17, FilterControl.Grid.Columns.Count);

				CombineAssertions(() =>
				{
					Assert("Contains DPE_FullName Column", FilterControl.Grid.Columns.Contains("DPE_FullName"));
					Assert("Contains DPE_TotalDuplicates Column", FilterControl.Grid.Columns.Contains("DPE_TotalDuplicates"));
					Assert("Contains DPE_HighDuplicates Column", FilterControl.Grid.Columns.Contains("DPE_HighDuplicates"));
					Assert("Contains DPE_MediumDuplicates Column", FilterControl.Grid.Columns.Contains("DPE_MediumDuplicates"));
					Assert("Contains DPE_LowDuplicates Column", FilterControl.Grid.Columns.Contains("DPE_LowDuplicates"));
					Assert("Contains DPE_Status Column", FilterControl.Grid.Columns.Contains("DPE_Status"));
					Assert("Contains DPE_ExcludedBy Column", FilterControl.Grid.Columns.Contains("DPE_ExcludedBy"));
					Assert("Contains PrimaryEmailAddress Column", FilterControl.Grid.Columns.Contains("DPE_EmailAddress"));
					Assert("Contains Workplace Column", FilterControl.Grid.Columns.Contains("Workplace"));
					Assert("Contains WorkplaceCode Column", FilterControl.Grid.Columns.Contains("WorkplaceCode"));
					Assert("Contains Location Column", FilterControl.Grid.Columns.Contains("Location"));
					Assert("Contains JobTitle Column", FilterControl.Grid.Columns.Contains("JobTitle"));
					Assert("Contains Gender Column", FilterControl.Grid.Columns.Contains("DPE_Gender"));
				});

				CombineAssertions(() =>
				{
					AssertEquals("Full Name", FilterControl.Grid.Columns["DPE_FullName"].ColumnStyle.HeaderText);
					AssertEquals("Status", FilterControl.Grid.Columns["DPE_Status"].ColumnStyle.HeaderText);
					AssertEquals("Excluded by", FilterControl.Grid.Columns["DPE_ExcludedBy"].ColumnStyle.HeaderText);
					AssertEquals("Primary Email", FilterControl.Grid.Columns["DPE_EmailAddress"].ColumnStyle.HeaderText);
					AssertEquals("Primary Workplace", FilterControl.Grid.Columns["Workplace"].ColumnStyle.HeaderText);
					AssertEquals("Primary Workplace Code", FilterControl.Grid.Columns["WorkplaceCode"].ColumnStyle.HeaderText);
					AssertEquals("Working Location", FilterControl.Grid.Columns["Location"].ColumnStyle.HeaderText);
					AssertEquals("Job Title", FilterControl.Grid.Columns["JobTitle"].ColumnStyle.HeaderText);
					AssertEquals("Gender", FilterControl.Grid.Columns["DPE_Gender"].ColumnStyle.HeaderText);
				});
			}
		}

		[RequiresSTA]
		public void TestShowMessageWhenRecoredNumberLargerThanRegistry()
		{
			var per1 = Factory.NewWithValidTestData<GlbPerson>();
			per1.PER_FullName = "TESTDUP1";
			var per2 = Factory.NewWithValidTestData<GlbPerson>();
			per2.PER_FullName = "TESTDUP2";
			var per3 = Factory.NewWithValidTestData<GlbPerson>();
			per3.PER_FullName = "TESTDUP3";

			Factory.Save();

			var codeFilter = FilterControl.FilterBusinessObject["Full Name"] as ModuleTextFilter;
			codeFilter.IsActive = true;
			codeFilter.Property = "TESTDUP";
			codeFilter.ComparisonOperator = "starts with";

			UnitTestUserNotification.Instance.ClearMessages();

			using (SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			using (SystemDataRegistry.Instance.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

				FilterControl.FirePerformSearch();
				AssertEquals(2, FilterControl.GridCollection.Count);
				AssertEquals("Should contain information message", "Found at least 3 records.\r\nOnly showing the top 2 records.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();

			using (SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			{
				using (SystemDataRegistry.Instance.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
				{
					AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

					FilterControl.FirePerformSearch();
					AssertEquals(2, FilterControl.GridCollection.Count);
					AssertEquals("Should contain information message", "Found 3 records.\r\nOnly showing the top 2 records.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessages();

				using (SystemDataRegistry.Instance.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
				{
					AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

					FilterControl.FirePerformSearch();
					AssertEquals(3, FilterControl.GridCollection.Count);
					AssertEquals("Should not contain information message", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			UnitTestUserNotification.Instance.ClearMessages();

			AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

			codeFilter.Property = "TESTDUP4";
			FilterControl.FirePerformSearch();
			AssertEquals(0, FilterControl.GridCollection.Count);
			AssertEquals("Should contain information message", "Found no records.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestShowTextInRecordsFoundLabelWhenSearch()
		{
			var codeFilter = FilterControl.FilterBusinessObject["Full Name"] as ModuleTextFilter;
			codeFilter.IsActive = true;
			codeFilter.Property = "TESTDUP";
			codeFilter.ComparisonOperator = "starts with";

			var recordsLabel = FilterControl.Controls.Find("ToolStripRecordsFoundLabel", true)[0] as ZLabel;
			AssertNotNull("Precondition", recordsLabel);
			AssertEquals("Precondition", string.Empty, recordsLabel.Text);

			FilterControl.FirePerformSearch();
			AssertEquals(0, FilterControl.GridCollection.Count);
			AssertEquals("Should show text in label", "    Found\r\n  no records", recordsLabel.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Form = GetFormToBashCore() as ZForm;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Form?.Dispose();
		}

		DeduplicationPersonFilterControl FilterControl => Form.Controls.Find("FilterControl", true)[0] as DeduplicationPersonFilterControl;
		ZForm Form;
	}
}
