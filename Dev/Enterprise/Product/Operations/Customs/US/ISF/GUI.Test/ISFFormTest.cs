using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BillTypeList = Enterprise.Customs.Common.US.ISF.BillTypeList;

namespace Enterprise.Customs.US.ISF.GUI.Testing
{
	[TestedType(typeof(ISFForm))]
	sealed class ISFFormTest : ZFormBasherTest
	{
		public void TestDoNotAllowToDeactiveIfMessageIsAcceptedByCustoms()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_IsCancelled = true;
			header.BF_JobReference = "XXXXX";
			using (var form = new ISFForm(header))
			{
				form.Show();
				using (header.SuspendValidationTesting())
				{
					form.FireSaveButton();
					AssertEquals(string.Format(ISFForm.DeactiveIsNotAllowed, "XXXXX"), UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.ResetToOriginal();
				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
			}
		}

		public void TestLinesTabImportLinesDataBasedOnManufacturersCompanyName()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = Factory.New<CusISFHeader>();
			var oh = Factory.New<OrgHeader>();
			oh.OH_Code = "TESTTESTTEST";
			oh.OH_FullName = "TEST COMPANY NAME";
			using (var form = new ISFForm(header))
			{
				form.Show();
				Application.DoEvents();
				ZTemplateTabControl mainTabControl = (ZTemplateTabControl)form.Find(c => c.Name == "MainTabControl").First();
				ZTabPage linesTabPage = (ZTabPage)form.Find(c => c.Name == "LinesTabPage").First();
				mainTabControl.SelectedTab = linesTabPage;
				Application.DoEvents();
				ZGrid mGrid = (ZGrid)form.Find(c => c.Name == "ManufacturersGrid").First();
				mGrid[0, 1] = oh.PK;
				AssertEquals(System.Data.DataRowState.Detached, ((INeedRow)mGrid.List[0]).Row.RowState);

				mGrid.List.AddNew();
				mGrid.Focus();
				((System.ComponentModel.ICancelAddNew)mGrid.List).EndNew(0);
				AssertEquals(System.Data.DataRowState.Added, ((INeedRow)mGrid.List[0]).Row.RowState);
				mGrid[0, 1] = oh.PK;
				Application.DoEvents();
				ZGrid linesGrid = (ZGrid)form.Find(c => c.Name == "LinesGrid").First();
				ContextMenu cm = linesGrid.ContextMenu;
				MenuItem menuItem = linesGrid.ContextMenu.MenuItems.FindByText("&Import Data...");
				menuItem.PerformClick();
				Application.DoEvents();
				FormCollection forms = Application.OpenForms;
				ZForm iwForm = (ZForm)forms.Cast<Form>().Single(f => f.Text == "Data Import Wizard");
				ImportWizard wizard = (ImportWizard)iwForm.BusinessEntity;
				string filename = EnvProxy.Instance.GetTempFileName();
				using (FileStream fs = File.Create(filename))
				{
					using (StreamWriter sw = new StreamWriter(fs))
					{
						sw.WriteLine("Y," + oh.OH_Code + ",65 DETROIT ROAD," + oh.OH_FullName + ",123 BROADWAY,US,NY,New York,1112,+1 (630) 555-1212,,CN,8306290000");
					}
				}

				wizard.FileName = filename;
				Application.DoEvents();
				var btnNext = (ZButton)iwForm.Find(c => c.Name == "NextButton").First();
				btnNext.PerformClick();
				Application.DoEvents();
				var toGrid = (ZGrid)iwForm.Find(c => c.Name == "ToGrid").First();
				ImportWizardMapping mapping = wizard.Mapping[3];
				mapping.AddFileColumnIndex(3);
				mapping.RefreshBinding();
				Application.DoEvents();
				btnNext.PerformClick();
				Application.DoEvents();
				var previewGrid = (ZGrid)iwForm.Find(c => c.Name == "PreviewGrid").First();
				var companyGuid = (ZGuid)previewGrid[0, 3];
				AssertEquals("the guid value in the target cell must be valid and be equal to the header.Manufacturer's pk", companyGuid, header.ManufacturerAddresses[0].PK);
				var btnCancel = (ZButton)iwForm.Find(c => c.Name == "CancelButtonX").First();
				btnCancel.PerformClick();
				Application.DoEvents();
				File.Delete(filename);
			}
		}

		public void TestCustomFieldsTab()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ISF";
			template.P0_OH_Client = client.PK;
			var customField = template.GenCustomColumnDefinitions.AddNew();
			customField.XC_Name = "DIFFICULTY";
			customField.XC_Type = "STR";
			Factory.Save();
			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_IsCancelled = true;
			header.BF_JobReference = "XXXXX";
			header.BF_OH_Importer = client.PK;
			using (var form = new ISFForm(header))
			{
				form.Show();
				using (header.SuspendValidationTesting())
				{
					var customFieldsControl = form.Controls.Find("shipmentCustomFieldsControl1", true);
					var customFieldsBO = ((ICustomFieldProvider)header).GetCustomBusinessObject();
					AssertEquals("__DIFFICULTY__prop__ZString", ((IDynamicBusinessObject)customFieldsBO).PropertyNames[0]);
				}
			}
		}

		public void TestFormCaptions()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF23423BD";
			Factory.Save();
			using (var form = new ISFForm(header))
			{
				form.Show();
				AssertContains(header.HumanReadableName, form.FormCaption);
			}
		}

		[TestDate(2009, 2, 20, 23, 20, 45)]
		public void TestExportToXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Factory.Save();
			using (var dir = new TempDirectory())
			using (var form = new ISFForm(header))
			{
				var exportMenu = form.Menu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Export to XML (Verbose)");
				var fileName = Path.Combine(dir.DirectoryName, header.BF_JobReference + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss"));
				AssertNotNull(exportMenu);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
				AssertEquals(false, File.Exists(fileName));
				exportMenu.PerformClick();
				AssertEquals(true, File.Exists(fileName));
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		protected override void BashScenario(Form testForm)
		{
			var form = testForm as ISFForm;
			if (form != null)
			{
				var header = form.BusinessEntity;
				foreach (ICodeDescription pair in new SubmissionTypeList())
				{
					header.BF_EntryType = pair.Code;
					((IBusinessObjectState)header).ClearHasChangesIncludingChildren();
					AssertEquals("Declaration should not have changes", false, header.HasChanges);
					base.BashScenario(testForm);
				}
			}
		}

		protected override void SetupTabPagesThatNeedToBeBashedOnlyOnce(Form testForm)
		{
			var form = testForm as ISFForm;
			if (form != null)
			{
				foreach (var plugIn in form.PlugIns.Instances)
				{
					if (plugIn.TabPage != null && !OnceOnlyBashTabPages.Contains(plugIn.TabPage))
					{
						OnceOnlyBashTabPages.Add(plugIn.TabPage);
					}
				}
			}
		}

		public void TestPlugIns()
		{
			var header = Factory.New<CusISFHeader>();
			using (var form = new ISFForm(header))
			{
				form.Show();
				AssertNotNull(ControllerIDs.Routing.Name, form.PlugIns.GetPlugIn(ControllerIDs.Routing));
				var tabControl = form.Controls["MainPanel"].Controls["MainTabControl"];
				AssertEquals("RoutingTabPage", tabControl.Controls[3].Name);
				AssertNotNull(ControllerIDs.JobInvoicing.Name, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(ControllerIDs.eDocsPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(ControllerIDs.DocDataPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestSetPartAttributeCaptionsByOrg()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "importer";
			importer.MiscServ.OM_IMPartAttrib1Name = "attr 1";
			importer.MiscServ.OM_IMPartAttrib2Name = "attr 2";
			importer.MiscServ.OM_IMPartAttrib3Name = "attr 3";
			var header = Factory.New<CusISFHeader>();
			using (var form = new ISFForm(header))
			{
				form.Show();
				var linesGrid = form.Controls.Find("LinesGrid", true)[0] as ZGrid;
				CombineAssertions("Default part attribute captions", () =>
				{
					AssertEquals("Part Attrib. 1", linesGrid.GetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib1));
					AssertEquals("Part Attrib. 2", linesGrid.GetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib2));
					AssertEquals("Part Attrib. 3", linesGrid.GetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib3));
				});
				header.BF_OH_Importer = importer.PK;
				CombineAssertions("Default part attribute captions", () =>
				{
					AssertEquals("attr 1", linesGrid.GetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib1));
					AssertEquals("attr 2", linesGrid.GetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib2));
					AssertEquals("attr 3", linesGrid.GetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib3));
				});
			}
		}

		public void TestDeleteMenuItemEnabled()
		{
			var header = Factory.New<CusISFHeader>();
			using (var form = new ISFForm(header))
			{
				Assert("AllowReadOnlyRowsToBeDeleted", form.BillsGrid.AllowReadOnlyRowsToBeDeleted);
			}
		}

		public void TestImportCommercialInvoice()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			var ref1 = header.ReferenceDatas.AddNew();
			ref1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			ref1.BB_BillNum = "HB10001";
			var ref2 = header.ReferenceDatas.AddNew();
			ref2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			ref2.BB_BillNum = "HB10002";
			var ref3 = header.ReferenceDatas.AddNew();
			ref3.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			ref3.BB_BillNum = "MB20001";
			var ref4 = header.ReferenceDatas.AddNew();
			ref4.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			ref4.BB_BillNum = "MB20002";
			var ref5 = header.ReferenceDatas.AddNew();
			ref5.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			ref5.BB_BillNum = "OB30001";
			var ref6 = header.ReferenceDatas.AddNew();
			ref6.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			ref6.BB_BillNum = "OB30002";
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			invoice1.JZ_OH_Buyer = importer.PK;
			invoice1.InvoiceHeaderRefs.AddNew("HB", "HB10001");
			invoice1.InvoiceHeaderRefs.AddNew("MB", "MB20001");
			invoice1.InvoiceHeaderRefs.AddNew("MB", "9999999");
			var invoice1Line1 = invoice1.InvoiceLines.AddNew();
			invoice1Line1.JI_PartNo = "TESTPART";
			invoice1Line1.JI_Tariff = "11.1111.111";
			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.JZ_OH_Buyer = importer.PK;
			invoice2.InvoiceHeaderRefs.AddNew("HB", "HB10002");
			invoice2.InvoiceHeaderRefs.AddNew("MB", "OB30001");
			var invoice1Line2 = invoice2.InvoiceLines.AddNew();
			invoice1Line2.JI_PartNo = "TESTPART";
			invoice1Line2.JI_Tariff = "22.2222.222";
			Factory.Save();
			using (var form = new ISFForm(header))
			{
				var importMenu = form.Menu.MenuItems.FindByText("Actio&ns")?.MenuItems.FindByText("Import Commercial Invoice");
				AssertNotNull("form should have 'Actions'->'Import Commercial Invoice' menu item", importMenu);
				importMenu.PerformClick();
				var searchPopup = ZFormModaliser.ActiveForm as EmbeddedModulePopup;
				AssertNotNull("expected search popup to be opened", searchPopup);
				var activeFilters = searchPopup.Module_ForTest.FilterBusinessObject.ActiveModuleFilters;
				AssertMultilineASCIIEquals(
					string.Join("\r\n", new string[]
					{
						"Created Time, None, (Last 3 Mths.)",
						$"Importer / Supplier, None, ({importer.PK}, 00000000-0000-0000-0000-000000000000)",
						"References, Red, (starts with 'HB10001', ref type: 'HB')",
						"References, Red, (starts with 'HB10002', ref type: 'HB')",
						"References, Red, (not found '', ref type: 'HB')",
						"References, Green, (starts with 'MB20001', ref type: 'MB')",
						"References, Green, (starts with 'MB20002', ref type: 'MB')",
						"References, Green, (starts with 'OB30001', ref type: 'MB')",
						"References, Green, (starts with 'OB30002', ref type: 'MB')",
						"References, Green, (not found '', ref type: 'MB')",
					}.OrderBy(t => t)),
					string.Join("\r\n", activeFilters.Select(f =>
						$"{f.OriginalCode}, {f.OrCategory}, {FormatFilter(f)}"
					).OrderBy(t => t))
				);

				searchPopup.Module_ForTest.PerformSearch_ForTest();
				AssertEquals("should find 2 records", 2, searchPopup.Module_ForTest.GridCollection.Count);
				searchPopup.ExposedOKButtonForTesting.PerformClick();
				AssertEquals("Please select an item from the grid.", UnitTestUserNotification.Instance.LastMessage?.Text);
				searchPopup.Module_ForTest.DisplayGrid.SelectAllElements();
				searchPopup.ExposedOKButtonForTesting.PerformClick();
				AssertEquals("Please select one item.", UnitTestUserNotification.Instance.LastMessage?.Text);
				AssertEquals("nothing should be imported yet", 0, header.Lines.Count);
				searchPopup.Module_ForTest.DisplayGrid.UnSelect(1);
				searchPopup.ExposedOKButtonForTesting.PerformClick();
				AssertEquals("1 line should be imported", 1, header.Lines.Count);
				AssertEquals("TESTPART", header.Lines[0].BL_TextProductCode);
				AssertEquals("original invoice was not attached to declaration", ZGuid.Empty, invoice1.JZ_JE);
				AssertEquals("original invoice was not attached to declaration", ZGuid.Empty, invoice2.JZ_JE);
			}
		}

		string FormatFilter(ModuleFilter moduleFilter)
		{
			if (moduleFilter == null)
			{
				return "null";
			}

			if (moduleFilter is ModuleGuidsFilter guidFilter)
			{
				return $"({guidFilter.Property1}, {guidFilter.Property2})";
			}

			if (moduleFilter is ModuleTextFilter moduleTextFilter)
			{
				return $"({moduleTextFilter.ComparisonOperator} '{moduleTextFilter.Property}', ref type: '{moduleTextFilter["ReferenceType"]}')";
			}

			if (moduleFilter is ModuleDateFilter moduleDateFilter)
			{
				return $"({moduleDateFilter.PropertySearch})";
			}

			return moduleFilter.GetType().Name;
		}

		protected override Form GetFormToBashCore()
		{
			var result = new ISFForm(Factory.New<CusISFHeader>());
			result.ControllerID = ControllerIDs.ImporterSecurityFiling;
			return result;
		}
	}
}
