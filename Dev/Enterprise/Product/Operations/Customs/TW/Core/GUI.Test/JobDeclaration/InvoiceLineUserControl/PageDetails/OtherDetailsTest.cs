using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class OtherDetailsTest : TestCaseWithFactory
	{
		public void TestOtherDetails()
		{
			declarationForTesting.JE_MessageType = "EXP";
			using (var form = new JobDeclarationForm(declarationForTesting))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
					var otherDetails = invoiceLineUserControl.FindSingleOrDefault<OtherDetailsUserControl>();
					CombineAssertions(() =>
					{
						AssertNotNull(otherDetails);
						AssertEquals(true, otherDetails.FindSingle<TWJobDocAddressControl>("ManuFacturerAddressControl").Visible);
						AssertEquals(false, otherDetails.FindSingle<ZGroupBox>("EnvironmentalProtectionTariffGroupBox").Visible);
						AssertEquals(true, otherDetails.FindSingle<ZDropEdit>("TW_TpfPymntMthdDropEdit").Visible);
					});
				}
			}

			declarationForTesting.JE_MessageType = "IMP";
			using (var form = new JobDeclarationForm(declarationForTesting))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
					var otherDetails = invoiceLineUserControl.FindSingleOrDefault<OtherDetailsUserControl>();
					CombineAssertions(() =>
					{
						AssertNotNull(otherDetails);
						AssertEquals(true, otherDetails.FindSingle<TWJobDocAddressControl>("ManuFacturerAddressControl").Visible);
						AssertEquals(true, otherDetails.FindSingle<ZGroupBox>("EnvironmentalProtectionTariffGroupBox").Visible);
						AssertEquals(false, otherDetails.FindSingle<ZDropEdit>("TW_TpfPymntMthdDropEdit").Visible);
					});
				}
			}
		}

		public void TestControlsReadOnly()
		{
			declarationForTesting.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			var entryInstruction = declarationForTesting.CusEntryInstruction;
			invoiceLineForTesting.JI_CEI = entryInstruction.PK;
			ControllingMsgHeaderTestHelper.AddControllingAgencysTo(entryInstruction.ControllingMessageHeaders, new string[] { "20", "IF", "DN", "CD" });
			using (var form = new JobDeclarationForm(declarationForTesting))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
					var otherDetails = invoiceLineUserControl.FindSingleOrDefault<OtherDetailsUserControl>();
					var controlEPTDigit2DropEdit = otherDetails.FindSingleOrDefault<ZDropEdit>(c => c.Name == "TW_EPTDigit2DropEdit");
					var controlEPTDigit3DropEdit = otherDetails.FindSingleOrDefault<ZDropEdit>(c => c.Name == "TW_EPTDigit3DropEdit");
					CombineAssertions(() =>
					{
						Assert("TW_EPTDigit2DropEdit is not ReadOnly", !controlEPTDigit2DropEdit.ReadOnly);
						Assert("TW_EPTDigit3DropEdit is not ReadOnly", !controlEPTDigit3DropEdit.ReadOnly);
						invoiceLineForTesting.JI_EPTDigit1 = ContainerMaterialList.Codes.Z;
						Assert("TW_EPTDigit2DropEdit is ReadOnly", controlEPTDigit2DropEdit.ReadOnly);
						Assert("TW_EPTDigit3DropEdit is ReadOnly", controlEPTDigit3DropEdit.ReadOnly);
					});
				}
			}
		}

		public void TestManuFacturerAddressControlCaptionWhenExport()
		{
			declarationForTesting.JE_MessageType = "EXP";
			using (var form = new JobDeclarationForm(declarationForTesting))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
					var otherDetails = invoiceLineUserControl.FindSingleOrDefault<OtherDetailsUserControl>();
					var captionResourceString = otherDetails.FindSingle<TWJobDocAddressControl>("ManuFacturerAddressControl").CaptionResourceString;
					CombineAssertions(() =>
					{
						AssertEquals("Manufacturer", captionResourceString.Caption);
						AssertEquals("Manuf.", captionResourceString.ShortCaption);
						AssertEquals("The name and VAT number of the manufacturer.", captionResourceString.FullDescription);
					});
				}
			}
		}

		public void TestManuFacturerAddressControlCaptionWhenImport()
		{
			using (var form = new JobDeclarationForm(declarationForTesting))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
					var otherDetails = invoiceLineUserControl.FindSingleOrDefault<OtherDetailsUserControl>();
					var captionResourceString = otherDetails.FindSingle<TWJobDocAddressControl>("ManuFacturerAddressControl").CaptionResourceString;
					CombineAssertions(() =>
					{
						AssertEquals("Foreign Manufacturer", captionResourceString.Caption);
						AssertEquals("Foreign Manuf.", captionResourceString.ShortCaption);
						AssertEquals("The code of foreign manufacturer issued by the foreign authority.", captionResourceString.FullDescription);
					});
				}
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new OtherDetailsUserControl())
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
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is OtherDetailsUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is OtherDetailsUserControl));
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declarationForTesting = Factory.NewWithValidTestData<JobDeclaration>();
			declarationForTesting.JE_MessageType = "IMP";
			var header = declarationForTesting.Invoices.AddNew();
			invoiceLineForTesting = (JobComInvoiceLine)header.InvoiceLines.AddNew();
		}

		JobDeclaration declarationForTesting;
		JobComInvoiceLine invoiceLineForTesting;
	}
}
