using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrganisationTaxRateFileImportForm))]
	public class OrganisationTaxRateFileImportFormTest : ZFormBasherTest
	{
		public void TestFormBusinessEntity()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				orgTaxRateForm.Show();
				AssertType<OrganisationTaxRateFileImport>(orgTaxRateForm.BusinessEntity);
			}
		}

		[RequiresSTA]
		public void TestImportDataUseCorrectBusinessEntity()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				var entity = (OrganisationTaxRateFileImport)orgTaxRateForm.BusinessEntity;

				var mockImporter = new Mock<IOrganisationTaxRateFileImportFileDataImporter>();
				mockImporter.Setup(x => x.ImportData(It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<OrganisationTaxRateFileImport>()));

				orgTaxRateForm.Show();

				using (ObjectFactory.Substitute(mockImporter.Object))
				using (var tempFile = TempFile.New(Env.TempPath, "txt"))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
					AssertNoExceptionThrown(importButton.PerformClick);
					mockImporter.Verify(x => x.ImportData(tempFile.Filename, It.IsNotNull<INotifications>(), entity), Times.Once);
				}
			}
		}

		public void TestIsError()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				var entity = (OrganisationTaxRateFileImport)orgTaxRateForm.BusinessEntity;

				var mockImporter = new Mock<IOrganisationTaxRateFileImportFileDataImporter>();
				mockImporter
					.Setup(x => x.ImportData(It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<OrganisationTaxRateFileImport>()))
					.Callback<string, INotifications, OrganisationTaxRateFileImport>((file, notifications, importObject) => notifications.AddError("Could not find file"));

				orgTaxRateForm.Show();

				using (ObjectFactory.Substitute(mockImporter.Object))
				using (var tempFile = TempFile.New(Env.TempPath, "txt"))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
					importButton.PerformClick();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertStartsWith("Expected Message Error.", "Could not find file", UnitTestUserNotification.Instance.LastMessage.Text);

					mockImporter.Verify(x => x.ImportData(tempFile.Filename, It.IsNotNull<INotifications>(), entity), Times.Once);
				}
			}
		}

		[RequiresSTA]
		public void TestIsWarning()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				var entity = (OrganisationTaxRateFileImport)orgTaxRateForm.BusinessEntity;

				var mockImporter = new Mock<IOrganisationTaxRateFileImportFileDataImporter>();
				mockImporter
					.Setup(x => x.ImportData(It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<OrganisationTaxRateFileImport>()))
					.Callback<string, INotifications, OrganisationTaxRateFileImport>((file, notifications, importObject) => notifications.AddWarning("A warning notification."));

				orgTaxRateForm.Show();

				using (ObjectFactory.Substitute(mockImporter.Object))
				using (var tempFile = TempFile.New(Env.TempPath, "txt"))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
					importButton.PerformClick();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
					AssertStartsWith("Expected Message Warning.", "A warning notification.", UnitTestUserNotification.Instance.LastMessage.Text);

					mockImporter.Verify(x => x.ImportData(tempFile.Filename, It.IsNotNull<INotifications>(), entity), Times.Once);
				}
			}
		}

		public void TestFormCaptionAndControlsVisibility()
		{
			using (var orgTaxRateForm = new OrganisationTaxRateFileImportForm())
			{
				orgTaxRateForm.Show();
				AssertEquals("FormVerb", "", orgTaxRateForm.FormVerb);
				AssertEquals("Form Caption should be 'Tax Configuration Organization Rate Update File Import'", "Tax Configuration Organization Rate Update File Import", orgTaxRateForm.FormCaption);

				var zDropEdits = orgTaxRateForm.FindAll<ZDropEdit>();
				AssertEquals(2, zDropEdits.Count());

				var zGuidDropEdits = orgTaxRateForm.FindAll<ZGuidDropEdit>();
				AssertEquals(1, zGuidDropEdits.Count());

				var panels = orgTaxRateForm.FindAll<ZPanel>();
				AssertEquals(2, panels.Count());

				var postingButtonsUserControl = orgTaxRateForm.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl", true);
				Assert(postingButtonsUserControl.Visible);

				var dropEditTaxConfiguration = orgTaxRateForm.GetControl<ZGuidDropEdit>("TaxConfigurationCodeAndDescriptionZGuidDropEdit", true);
				Assert(dropEditTaxConfiguration.Visible);

				var gridImportLines = orgTaxRateForm.GetControl<ZGrid>("ImportLinesGrid", true);
				Assert(gridImportLines.Visible);

				var dropEditRateSource = orgTaxRateForm.GetControl<ZDropEdit>("RateSourceDropEdit", true);
				Assert(dropEditRateSource.Visible);

				var tabPageImportLines = orgTaxRateForm.GetControl<ZTabPage>("ImportLinesTabPage", true);
				AssertNotNull(tabPageImportLines);
				Assert(tabPageImportLines.TabVisible);

				var fileNameTextBox = orgTaxRateForm.GetControl<ZTextBox>("FileNameTextBox", true);
				Assert(fileNameTextBox.Visible);

				var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
				Assert(importButton.Visible);
			}
		}

		public void TestImport()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				orgTaxRateForm.Show();

				using (var tempFile = TempFile.New(Env.TempPath, "txt"))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				}

				var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
				importButton.PerformClick();
				AssertEquals(typeof(OpenFileDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());
			}
		}

		public void TestImportCancel()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				orgTaxRateForm.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
				AssertNoExceptionThrown(importButton.PerformClick);
			}
		}

		public void TestImportSelectThenCancel()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				orgTaxRateForm.Show();

				using (var tempFile = TempFile.New(Env.TempPath, "txt"))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				}

				var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
				AssertNoExceptionThrown(importButton.PerformClick);
			}
		}

		public void TestTaxRateGrid()
		{
			using (var orgTaxRateForm = new OrganisationTaxRateFileImportForm())
			{
				orgTaxRateForm.Show();

				var grid = orgTaxRateForm.GetControl<ZGrid>("ImportLinesGrid");
				var expectedColumns = new[]
				{
					"OrganizationCode (ZTextBoxColumnStyleInfo) IsVisible:True",
					"OrganizationName (ZTextBoxColumnStyleInfo) IsVisible:True",
					"RegistrationCode (ZTextBoxColumnStyleInfo) IsVisible:True",
					"RateSource (ZTextBoxColumnStyleInfo) IsVisible:True",
					"StartDate (ZDateEditColumnStyleInfo) IsVisible:True",
					"EndDate (ZDateEditColumnStyleInfo) IsVisible:True",
					"RateNumerator (ZTextBoxColumnStyleInfo) IsVisible:True",
					"RateDenominator (ZTextBoxColumnStyleInfo) IsVisible:True",
				};

				var gridColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedColumns, gridColumns);

				AssertDateEditColumnStyle(grid.GetColumnStyle("StartDate"), ZDateTimePickerFormat.Short);
				AssertDateEditColumnStyle(grid.GetColumnStyle("EndDate"), ZDateTimePickerFormat.Short);
			}

			void AssertDateEditColumnStyle(ZGridColumnInfo columnStyle, ZDateTimePickerFormat dateFormat)
			{
				AssertType<ZDateEditColumnStyleInfo>(columnStyle);
				var dateColumnStyle = (ZDateEditColumnStyleInfo)columnStyle;
				AssertEquals(columnStyle.ColumnName, dateFormat, dateColumnStyle.DateTimeFormat);
			}
		}

		public void TestDataSourceType()
		{
			using (var orgTaxRateForm = new OrganisationTaxRateFileImportForm())
			{
				AssertEquals("DataSourceType is OrganisationTaxRateFileImport", typeof(OrganisationTaxRateFileImport), orgTaxRateForm.DataSourceType);
			}
		}

		[RequiresSTA]
		public void TestImportButtonAndDropDownDisabled()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				CheckControlsAreEnabled(orgTaxRateForm, false);
			}
		}

		[RequiresSTA]
		public void TestImportButtonAndDropDownDisabledWhenErrorOccurs()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				var mockImporter = new Mock<IOrganisationTaxRateFileImportFileDataImporter>();
				mockImporter
					.Setup(x => x.ImportData(It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<OrganisationTaxRateFileImport>()))
					.Callback<string, INotifications, OrganisationTaxRateFileImport>((file, notifications, importObject) => notifications.AddError("Ops!"));

				ObjectFactory.Substitute(mockImporter.Object);
				CheckControlsAreEnabled(orgTaxRateForm, false);
			}
		}

		[RequiresSTA]
		public void TestImportButtonAndDropDownDisabledWhenWarningOccurs()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				var mockImporter = new Mock<IOrganisationTaxRateFileImportFileDataImporter>();
				mockImporter
					.Setup(x => x.ImportData(It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<OrganisationTaxRateFileImport>()))
					.Callback<string, INotifications, OrganisationTaxRateFileImport>((file, notifications, importObject) => notifications.AddWarning("Warning!"));

				ObjectFactory.Substitute(mockImporter.Object);
				CheckControlsAreEnabled(orgTaxRateForm, false);
			}
		}

		public void TestImportButtonAndDropDownEnabledWhenCancelDialog()
		{
			using (var orgTaxRateForm = GetFormToBashCore() as OrganisationTaxRateFileImportForm)
			{
				orgTaxRateForm.Show();
				CheckControlsAreEnabled(orgTaxRateForm, true, DialogResult.Cancel);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore() => new OrganisationTaxRateFileImportForm();

		void CheckControlsAreEnabled(OrganisationTaxRateFileImportForm orgTaxRateForm, bool areEnabled, DialogResult dialogResult = DialogResult.OK)
		{
			using (var tempFile = TempFile.New(Env.TempPath, "txt"))
			{
				orgTaxRateForm.Show();

				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = dialogResult;

				var importButton = orgTaxRateForm.GetControl<ZButton>("ImportButton", true);
				importButton.PerformClick();

				var taxConfigurationDropDown = orgTaxRateForm.GetControl<ZGuidDropEdit>("TaxConfigurationCodeAndDescriptionZGuidDropEdit", true);
				var rateSourceDropDown = orgTaxRateForm.GetControl<ZDropEdit>("RateSourceDropEdit", true);

				AssertEquals(areEnabled, importButton.Enabled);
				AssertEquals(areEnabled, taxConfigurationDropDown.Enabled);
				AssertEquals(areEnabled, rateSourceDropDown.Enabled);
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			if (!ObjectFactory.HasBeenSubstituted<IAccountingMasterFilesDependencyFactory>())
			{
				var helperMock = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

				var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
				Assert(!taxConfig.PK.IsEmpty);
				var collection = new AccTaxConfigurationCollection(Factory);
				AssertEquals(1, collection.Count);

				helperMock.Setup(x => x.GetTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(collection);
			}
		}
	}
}
