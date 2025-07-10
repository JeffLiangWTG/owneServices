using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class DA63UserControlTest : TestCaseWithFactory
	{
		public void TestReCalculateDA63ValuesIfNeeded()
		{
			var newFactory = new BusinessObjectFactory();
			var testHelper = new ZAUniversalReferenceTestDataHelper(newFactory);
			testHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var testDeclaration = newFactory.New<JobDeclaration>();
			testDeclaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			newFactory.Save();
			using (var testForm = new JobDeclarationForm(Factory.Load<JobDeclaration>(testDeclaration.PK)))
			{
				// Disable billings tab which has validation error that prevents saving
				testForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).Enabled = false;

				testForm.Show();
				var brokerageUserControl = testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				var da63UserControl = invoiceLinesUserControl.DA63UserControl;
				da63UserControl.Show();
				var invoiceLines = testForm.Declaration.InvoiceLines;
				var invoiceLine = invoiceLines[0] as JobComInvoiceLine;
				AssertEquals(false, (invoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
				invoiceLine.JI_PreviousEntryLineNumber = 5;
				AssertEquals(true, (invoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
				var notificationInstanse = UnitTestUserNotification.Instance;
				notificationInstanse.ClearMessagesAndAnswers();
				testForm.FireSaveButton();
				AssertEquals(DA63PreSaveDialogStrategy.ReCalculateConfimation, notificationInstanse.LastMessage.Text);
			}
		}

		public void TestReCalculateDA63ValuesWithDoesNotCallIsDA63WhenInvoiceLineIsDeleted()
		{
			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock.Setup(m => m.IsDeleted).Returns(false);

			using (var da63Control = new DA63UserControlForTest(invoiceLineMock.Object))
			{
				da63Control.ReCalculateDA63ValuesIfNeeded();
				AssertNoExceptionThrown("IsDA63 should not be called when IsDeleted = false", () =>
				{
					invoiceLineMock.Protected().VerifyGet<ZBool>("IsDA63Core", Times.AtLeastOnce());
				});

				invoiceLineMock.Invocations.Clear();
				invoiceLineMock.Setup(m => m.IsDeleted).Returns(true);
				da63Control.ReCalculateDA63ValuesIfNeeded();
				AssertNoExceptionThrown("IsDA63 should not be called when IsDeleted = true", () =>
				{
					invoiceLineMock.Protected().VerifyGet<ZBool>("IsDA63Core", Times.Never());
				});
			}
		}

		public void TestConversionFactorControls()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			using (var form = new ZForm(invoiceLine))
			using (var control = new DA63UserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertCalcEdit(control, "conversionFactorCalcEdit", "Conversion Factor", 105, "JI_ConversionFactor", 5);
				AssertCalcEdit(control, "previousConversionFactorCalcEdit", null, 105, "ConversionFactorFromRelatedImportBOE", 5);
			}
		}

		void AssertCalcEdit(Control parent, string controlName, string expectedCaption, int expectedWidth, string expectedBinding,
			int expectedDecimalPlaces)
		{
			var control = parent.FindSingleOrDefault<ZCalcEdit>(controlName);
			AssertNotNull(controlName, control);
			if (expectedCaption == null)
			{
				AssertEquals("{controlName}.CaptionVisible", expected: false, control.GetExtension<ILabelCaptionRenderer>().Visible);
			}
			else
			{
				AssertEquals("{controlName}.Caption", expectedCaption, control.GetExtension<ILabelCaptionRenderer>().Caption);
			}
			AssertEquals("{controlName}.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedWidth), control.Width);
			AssertEquals("{controlName}.Binding", expectedBinding, control.GetBindingMember());
			AssertEquals("{controlName}.DecimalPlaces", expectedDecimalPlaces, control.DecimalPlaces);
		}

		class DA63UserControlForTest : DA63UserControl
		{
			public DA63UserControlForTest(JobComInvoiceLine invLine) : base()
			{
				invoiceLine = invLine;
			}

			public void ReCalculateDA63ValuesIfNeededExposed() => ReCalculateDA63ValuesIfNeeded();

			readonly JobComInvoiceLine invoiceLine;

			protected override JobComInvoiceLine GetInvoiceLine() => invoiceLine;
		}
	}
}
