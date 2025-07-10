using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class LineDetailsTest : TestCaseWithFactory
	{
		public void TestTWInvoiceQuantityCalcDropEdit()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var lineDetails = invoiceLineUserControl.LineDetailsUserControl;
					var invoiceQuantityCalcDropEdit = lineDetails.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "TWInvoiceQuantityCalcDropEdit");
					AssertEquals(6, invoiceQuantityCalcDropEdit.UnitPreBoundMaxLength);
					AssertEquals(false, invoiceQuantityCalcDropEdit.UnitShouldResizeByMaxLength);
				}
			}
		}

		public void TestBindingMembers()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var lineDetails = invoiceLineUserControl.LineDetailsUserControl;
					var bingdingSource = lineDetails.BindingSource;
					var compositionsLongTextControl = lineDetails.FindSingleOrDefault<Customs.GUI.LongTextControl>(c => c.Name == "TW_CompositionsLongTextControl");
					AssertEquals("BindingMember", "FilteredInvoiceLines.JI_Compositions", bingdingSource.GetBindingMember(compositionsLongTextControl));
					var nDescriptionLongTextControl = lineDetails.FindSingleOrDefault<Customs.GUI.LongTextControl>(c => c.Name == "JI_NDescriptionLongTextControl");
					AssertEquals("BindingMember", "FilteredInvoiceLines.JI_NDescription", bingdingSource.GetBindingMember(nDescriptionLongTextControl));
					var groupLongTextControl = lineDetails.FindSingleOrDefault<Customs.GUI.LongTextControl>(c => c.Name == "TWGroupLongTextControl");
					AssertEquals("BindingMember", "FilteredInvoiceLines.JI_Group", bingdingSource.GetBindingMember(groupLongTextControl));
					var descriptionLongTextControl = lineDetails.FindSingleOrDefault<Customs.GUI.LongTextControl>(c => c.Name == "JI_DescriptionLongTextControl");
					AssertEquals("BindingMember", "FilteredInvoiceLines.JI_Description", bingdingSource.GetBindingMember(descriptionLongTextControl));
				}
			}
		}

		public void TestUNDGCodeFindBox()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Air;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var lineDetails = invoiceLineUserControl.LineDetailsUserControl;
					var undgCodeFindBox = lineDetails.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "UNDGCodeFindBox");
					Assert(undgCodeFindBox.Visible);
					AssertEquals("IATA DG Code", undgCodeFindBox.GetExtension<LabelCaptionRenderer>().Caption);
					AssertEquals("The standard classification code for dangerous goods.", undgCodeFindBox.GetExtension<HintExtension>().Description);
					jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Sea;
					Assert(undgCodeFindBox.Visible);
					AssertEquals("UN DG Code", undgCodeFindBox.GetExtension<LabelCaptionRenderer>().Caption);
					AssertEquals("The standard classification code for dangerous goods.", undgCodeFindBox.GetExtension<HintExtension>().Description);
				}
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ImportLineDetailsUserControl())
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
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is ImportLineDetailsUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is ImportLineDetailsUserControl));
					}
				}
			}
		}
	}
}
