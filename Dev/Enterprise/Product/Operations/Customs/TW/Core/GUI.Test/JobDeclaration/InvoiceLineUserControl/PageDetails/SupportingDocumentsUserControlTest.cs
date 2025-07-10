using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestSupportingDocuments()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "EXP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
					var supportingDocuments = invoiceLineUserControl.FindSingleOrDefault<SupportingDocumentsUserControl>();
					AssertNotNull(supportingDocuments);
				}
			}

			jobDeclartion.JE_MessageType = "IMP";
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
					var supportingDocuments = invoiceLineUserControl.FindSingleOrDefault<SupportingDocumentsUserControl>();
					AssertNotNull(supportingDocuments);
				}
			}
		}

		public void TestPropertiesofCalcEdit()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var quotaPermitNumberItemNumberCalcEdit = control.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "QuotaPermitNumberItemNumberCalcEdit");
				Assert(quotaPermitNumberItemNumberCalcEdit.ShowEmptyStringForEmptyValue);
				Assert(!quotaPermitNumberItemNumberCalcEdit.ShowGroupSeparators);
				AssertEquals(9999m, quotaPermitNumberItemNumberCalcEdit.MaxValue);

				var permitNumberGrid = control.FindSingleOrDefault<ZGrid>(c => c.Name == "PermitNumberGrid");
				var lineNoCalcEditColumnStyleInfo = permitNumberGrid.GetColumnStyle("CSI_LineNo") as ZCalcEditColumnStyleInfo;
				Assert(lineNoCalcEditColumnStyleInfo.ShowEmptyStringForEmptyValue);
			}

			AssertPropertiesofCalcEdit("CertificateOfOriginNumberItemNumberCalcEdit", 9999m);
			AssertPropertiesofCalcEdit("JI_PreviousEntryLineNumberCalcEdit", 9999m);
			AssertPropertiesofCalcEdit("PreviousBondedEntryLineNumberCalcEdit", 9999m);
		}

		void AssertPropertiesofCalcEdit(string controlName, decimal maxValue)
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var calcEdit = control.FindSingleOrDefault<ZCalcEdit>(c => c.Name == controlName);
				Assert(calcEdit.ShowEmptyStringForEmptyValue);
				Assert(!calcEdit.ShowGroupSeparators);
				AssertEquals(maxValue, calcEdit.MaxValue);
			}
		}

		public void TestControlsVisibilityAndCaptions()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
					var supportingDocuments = invoiceLineUserControl.FindSingleOrDefault<SupportingDocumentsUserControl>();

					var quotaPermitNumberTextBox = supportingDocuments.FindSingleOrDefault<ZTextBox>(c => c.Name == "QuotaPermitNumberTextBox");
					AssertEquals(true, quotaPermitNumberTextBox.Visible);

					var quotaPermitNumberItemNumberCalcEdit = supportingDocuments.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "QuotaPermitNumberItemNumberCalcEdit");
					AssertEquals(true, quotaPermitNumberItemNumberCalcEdit.Visible);

					var highTechLicenseTextBox = supportingDocuments.FindSingleOrDefault<ZTextBox>(c => c.Name == "HighTechLicenseTextBox");
					AssertEquals(true, highTechLicenseTextBox.Visible);

					var citesPermitTextBox = supportingDocuments.FindSingleOrDefault<ZTextBox>(c => c.Name == "CitesPermitTextBox");
					AssertEquals(true, citesPermitTextBox.Visible);

					var importExportRegulationsGroupBox = supportingDocuments.FindSingleOrDefault<ZGroupBox>(c => c.Name == "ImportExportRegulationsGroupBox");
					AssertEquals("Import Regulations", importExportRegulationsGroupBox.CaptionResourceString.Caption);
				}
			}

			jobDeclartion.JE_MessageType = "EXP";
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
					var supportingDocuments = invoiceLineUserControl.FindSingleOrDefault<SupportingDocumentsUserControl>();

					var quotaPermitNumberTextBox = supportingDocuments.FindSingleOrDefault<ZTextBox>(c => c.Name == "QuotaPermitNumberTextBox");
					AssertEquals(false, quotaPermitNumberTextBox.Visible);

					var quotaPermitNumberItemNumberCalcEdit = supportingDocuments.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "QuotaPermitNumberItemNumberCalcEdit");
					AssertEquals(false, quotaPermitNumberItemNumberCalcEdit.Visible);

					var highTechLicenseTextBox = supportingDocuments.FindSingleOrDefault<ZTextBox>(c => c.Name == "HighTechLicenseTextBox");
					AssertEquals(false, highTechLicenseTextBox.Visible);

					var citesPermitTextBox = supportingDocuments.FindSingleOrDefault<ZTextBox>(c => c.Name == "CitesPermitTextBox");
					AssertEquals(false, citesPermitTextBox.Visible);

					var importExportRegulationsGroupBox = supportingDocuments.FindSingleOrDefault<ZGroupBox>(c => c.Name == "ImportExportRegulationsGroupBox");
					AssertEquals("Export Regulations", importExportRegulationsGroupBox.CaptionResourceString.Caption);
				}
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.PermitCusSupportingCollection.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new SupportingDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);

			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);

			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					var subcriberTarget = subcriber.Value.Target;
					if (subcriberTarget is DeclarationValueChangedAnnouncer)
					{
						var onValueChangedField = subcriberTarget.GetType().GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
						var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
						var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is SupportingDocumentsUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is SupportingDocumentsUserControl));
					}
				}
			}
		}
	}
}
