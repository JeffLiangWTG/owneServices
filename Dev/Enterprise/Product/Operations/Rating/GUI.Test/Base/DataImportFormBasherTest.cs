using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.DataImporter.Testing
{
	[TestedType(typeof(DataImportForm))]
	sealed class DataImportFormBasherTest : ZFormBasherTest
	{
		public void TestClearDuplicatesCheckBoxIsExclusive()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			Factory.Save();
			RateDataImporter.RegisterType(typeof(RateDataImporterTestClass));
			var importer = RateDataImporter.New(costing);
			importer.ShowClearRatesOptions = true;
			using (var form = new DataImportFormForTest(importer, importer.ImportIATA_TACT))
			{
				form.Show();

				Assert("ShouldClearOldTACTRates is false by default", !importer.ShouldClearOldTACTRates);
				Assert("ShouldClearOldStandardRates is false by default", !importer.ShouldClearOldStandardRates);

				form.CheckClearTACTCheckBox();
				Assert("ShouldClearOldTACTRates is true", importer.ShouldClearOldTACTRates);
				Assert("ShouldClearOldStandardRates is false", !importer.ShouldClearOldStandardRates);

				form.CheckClearStandardCheckBox();
				Assert(importer.ShouldClearOldTACTRates);
				Assert(importer.ShouldClearOldStandardRates);
			}
		}

		public void TestExcludedFromAutoRatingCheckBoxUpdatesImporterProperty()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			Factory.Save();
			RateDataImporter.RegisterType(typeof(RateDataImporterTestClass));
			var importer = RateDataImporter.New(costing);
			importer.ShowClearRatesOptions = true;
			using (var form = new DataImportFormForTest(importer, importer.ImportIATA_TACT))
			{
				form.Show();

				Assert("ShouldExcludeFromAutoRating is false by default", !importer.ShouldExcludeFromAutoRating);

				form.CheckExcludeFromAutocostingCheckBox();
				Assert("ShouldExcludeFromAutoRating is true", importer.ShouldExcludeFromAutoRating);
			}
		}

		public void TestImportDataBindingsWithDefaultValues()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			Factory.Save();
			RateDataImporter.RegisterType(typeof(RateDataImporterTestClass));
			var importer = RateDataImporter.New(costing);
			importer.ShowClearRatesOptions = true;
			importer.Rounding = RatingRoundingTypes.Chargeable;
			importer.IsJobLevelCharge = true;

			using (var form = new DataImportFormForTest(importer, importer.ImportIATA_TACT))
			{
				form.Show();

				Assert("IsJobLevelCharge is true by default", importer.IsJobLevelCharge);
				AssertEquals("Rounding is Chargeable by default", RatingRoundingTypes.Chargeable, importer.Rounding);
				AssertEquals("Rounding is Chargeable by default", RatingRoundingTypes.Chargeable, form.RoundingText);

				form.SetIsJobLevelChargeCheckBox(false);
				Assert("IsJobLevelCharge should set to false", !importer.IsJobLevelCharge);

				form.SetRoundingDropEdit(RatingRoundingTypes.Bankers);
				AssertEquals("Rounding should set to Bankers", RatingRoundingTypes.Bankers, form.RoundingText);
				AssertEquals("Rounding should set to Bankers", RatingRoundingTypes.Bankers, importer.Rounding);
			}
		}

		public void TestImportData()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			Factory.Save();
			RateDataImporter.RegisterType(typeof(RateDataImporterTestClass));
			var importer = RateDataImporter.New(costing);
			var validTestFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataImporter.ForwardAir.TestFiles.Test.csv", "Test.csv");
			using (var form = new DataImportFormForTest(importer, importer.ImportIATA_TACT))
			using (form.OpenFileDialogForTest = new ZOpenFileDialog())
			{
				form.Show();

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.ClickImportButton();
				AssertEquals("Cannot Import", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please select a file to import.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.OpenFileDialogForTest.FileName = @"c:\\Fake\test.156"; // Hard coded path for unit test
				form.ClickImportButton();
				AssertEquals("Cannot Import", UnitTestUserNotification.Instance.LastMessage.Caption);

				form.OpenFileDialogForTest.FileName = validTestFilePath;
				importer.Rounding = "SSS";
				form.ClickImportButton();
				AssertEquals("Cannot Import", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please fix all errors before importing.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				importer.Rounding = string.Empty;
				form.OpenFileDialogForTest.FileName = validTestFilePath;
				form.ClickImportButton();
				AssertEquals(true, ((RateDataImporterTestClass)importer).DoImportCalled);
				AssertEquals(100, form.ImportProgressBarValue);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.ClickImportButton();
				AssertEquals("Cannot Import", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("The selected file has been imported already. Please choose another file.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestImportRemoteData()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			Factory.Save();
			RateDataImporter.RegisterType(typeof(RateDataImporterTestClass));
			var importer = RateDataImporter.New(costing);
			var validTestFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataImporter.ForwardAir.TestFiles.Test.csv", "Test.csv");
			using (var form = new DataImportFormForTest(importer, importer.ImportIATA_TACT))
			using (form.OpenFileDialogForTest = new ZOpenFileDialogTestClass(validTestFilePath))
			{
				form.Show();

				form.OpenFileDialogForTest.FileName = @"c:\\Remote\test.156"; // Hard coded path for unit test
				form.ClickImportButton();
				AssertEquals(true, ((RateDataImporterTestClass)importer).DoImportCalled);
				AssertEquals(100, form.ImportProgressBarValue);
			}
		}

		public void TestCompanyTariffDataImportDoesNotNullifyControllerOfParentForm()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			Factory.Save();

			using (var parentForm = new GlobalTariffsForm(companyTariff))
			{
				parentForm.ControllerID = ControllerIDs.GlobalRates;
				parentForm.Show();

				AssertImport(companyTariff, parentForm);
			}
		}

		public void TestCostingDataImportDoesNotNullifyControllerOfParentForm()
		{
			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = ZGuid.Empty;
			Factory.Save();

			using (var parentForm = new CostingForm(costing))
			{
				parentForm.ControllerID = ControllerIDs.Costing;
				parentForm.Show();

				AssertImport(costing, parentForm);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var importer = RateDataImporter.New(Factory.New<Costing>());
			var form = new DataImportForm(importer, importer.ImportIATA_TACT);
			MissingResourceStringChecker.ExcludeFromTest(form.InfoTextBox);
			return form;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "InfoTextBox";
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DataTransfer.ForwardAir.Testing.ForwardAirRatesFlatFileDataImporterTest).Assembly));
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		void AssertImport(RatingHeader rates, RatingForm parentForm)
		{
			RateDataImporter.RegisterType(typeof(RateDataImporterTestClass));
			var importer = RateDataImporter.New(rates);
			using (var importDataForm = new DataImportFormForTest(importer, importer.ImportIATA_TACT))
			{
				try
				{
					using (importDataForm.OpenFileDialogForTest = new ZOpenFileDialog())
					{
						importDataForm.ParentRatingFormToClose = parentForm;
						importDataForm.Show();

						importDataForm.OpenFileDialogForTest.FileName = @"c:\\Fake\test.156"; // Hard coded path for unit test
						importDataForm.ClickImportButton();

						AssertNotNull("DataImportForm's Parent Form", importDataForm.ParentRatingFormToClose);
						AssertNotNull("Successful import has not nullified the parent form's controller", importDataForm.ParentRatingFormToClose.ControllerID);
					}
				}
				finally
				{
					importDataForm.ParentRatingFormToClose.Dispose();
				}
			}
		}

		sealed class DataImportFormForTest : DataImportForm
		{
			public DataImportFormForTest(RateDataImporter importer, Action<string, Stream> importAction)
				: base(importer, importAction) { }

			public void CheckClearStandardCheckBox()
			{
				ClearStandardCheckBox.Checked = true;
			}

			public void CheckClearTACTCheckBox()
			{
				ClearTACTCheckBox.Checked = true;
			}

			public void CheckExcludeFromAutocostingCheckBox()
			{
				ExcludeFromAutocostingCheckBox.Checked = true;
			}

			public void ClickImportButton()
			{
				ImportButton.PerformClick();
			}

			protected override DialogResult OpenFileDialogResult
			{
				get { return DialogResult.OK; }
			}

			protected override ZOpenFileDialog OpenFileDialog
			{
				get { return OpenFileDialogForTest; }
			}

			public ZOpenFileDialog OpenFileDialogForTest { get; set; }

			public ZString InfoText
			{
				get { return InfoTextBox.Text; }
			}

			public int ImportProgressBarValue
			{
				get { return ImportProgressBar.Value; }
			}

			public ZString RoundingText
			{
				get { return RoundingDropEdit.Text; }
			}

			public void SetIsJobLevelChargeCheckBox(bool isChecked)
			{
				IsJobLevelChargeCheckBox.Checked = isChecked;
			}

			public void SetRoundingDropEdit(string rounding)
			{
				RoundingDropEdit.SelectItem(rounding);
				RoundingDropEdit.CommitBoundValue();
			}
		}

		class RateDataImporterTestClass : RateDataImporter
		{
			protected RateDataImporterTestClass(RatingHeader ratingHeader)
				: base(ratingHeader)
			{
			}

			protected override bool IsActiveCore
			{
				get { return true; }
			}

			public override void ImportIATA_TACT(string fileName, Stream stream)
			{
				DoImportCalled = true;
				if (ImportCompletedHandler != null)
				{
					ImportCompletedHandler();
				}
				stream.Close();
			}

			public bool DoImportCalled { get; private set; }
		}

		sealed class ZOpenFileDialogTestClass : ZOpenFileDialog
		{
			public ZOpenFileDialogTestClass(string validTestFilePath)
				: base()
			{
				ValidTestFilePath = validTestFilePath;
			}

			public override Stream OpenFile() => File.OpenRead(ValidTestFilePath);

			string ValidTestFilePath { get; }
		}
	}
}
