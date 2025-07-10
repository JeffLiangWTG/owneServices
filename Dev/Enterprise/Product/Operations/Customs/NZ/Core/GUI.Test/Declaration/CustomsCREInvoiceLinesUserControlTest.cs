using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class CustomsCREInvoiceLinesUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibility()
		{
			Business.EDITariff_ReferenceFiles_NZ.Testing.NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
			var declaration = JobDeclaration.New(Factory);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsCREInvoiceLinesUserControl;

				AssertEquals(true, invoiceLineUserControl.NZCClassificationFindBox.Visible);
				AssertEquals(false, invoiceLineUserControl.TariffCodeFindBox.Visible);
				CheckFieldsVisibility(invoiceLineUserControl);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsCREInvoiceLinesUserControl;

				AssertEquals(false, invoiceLineUserControl.NZCClassificationFindBox.Visible);
				AssertEquals(true, invoiceLineUserControl.TariffCodeFindBox.Visible);
				CheckFieldsVisibility(invoiceLineUserControl);
			}
		}

		public void TestTariffFindBoxEffectiveDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_ExportDate = new ZDateTime(2023, 6, 1);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsCREInvoiceLinesUserControl;

				AssertEquals(declaration.JE_ExportDate, control.TariffCodeFindBox.GetEffectiveDate.Invoke());
			}
		}

		public void TestOrderOfColumnsAndNewTitles()
		{
			using (CustomsCREInvoiceLinesUserControl invoiceLineUserControl = new CustomsCREInvoiceLinesUserControl())
			{
				CheckCaption(invoiceLineUserControl, 0, "Inv. Line#");
				CheckCaption(invoiceLineUserControl, 1, "Product Code");
				CheckCaption(invoiceLineUserControl, 2, "Lookup Code");
				CheckCaption(invoiceLineUserControl, 3, "Tariff Code");
				CheckCaption(invoiceLineUserControl, 4, "Goods Description");
				CheckCaption(invoiceLineUserControl, 5, "No. Packages");
				CheckCaption(invoiceLineUserControl, 6, "Package Type");
				CheckCaption(invoiceLineUserControl, 7, "Item Value");
				CheckCaption(invoiceLineUserControl, 8, "Origin");
			}
		}

		#region Implementation
		void CheckCaption(CustomsCREInvoiceLinesUserControl invoiceLineUserControl, int index, string expectedCaption)
		{
			AssertEquals("Invalid Caption at index [" + index.ToString() + "].", expectedCaption, ((Core.Forms.ZGridColumnInfo)invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles[index]).Caption);
		}

		void CheckFieldsVisibility(CustomsCREInvoiceLinesUserControl invoiceLineUserControl)
		{
			var linesGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LineChargesTabPage;
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("LineDetailsTabPage");
			AssertEquals("JI_AntiDumpingDutyAmount is not in Grid", false, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_AntiDumpingDutyAmount));
			//AssertEquals("ExciseDutyCreditCalcEdit.Visible", ExciseDutyCreditVisible, InvoiceLineUserControl.ExciseDutyCreditCalcEdit.Visible);
			//AssertEquals("JI_ExciseDutyCreditAmount is in Grid", ExciseDutyCreditVisible, LinesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ExciseDutyCreditAmount));
			//AssertEquals("JI_IsZeroRatedCheckBox.Visible", IsZeroRatedVisible, InvoiceLineUserControl.ZeroRatedGroupBox.Visible);
			AssertEquals("JI_IsZeroRatedDuty is not in Grid", false, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedDuty));
			AssertEquals("JI_IsZeroRatedExcise is not in Grid", false, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedExcise));
			AssertEquals("JI_IsZeroRatedLevies is not in Grid", false, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedLevies));
			AssertEquals("JI_IsZeroRatedGST is not in Grid", false, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedGST));
			AssertEquals("JI_SerialNumber is not in Grid", false, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_SerialNumber));
		}
		#endregion
	}
}
