using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(ZForm))]
	public class DeduplicationOrganisationFilterControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var codeSender = ObjectFactory.Get<FilterStripBusinessObject>("DeduplicationOrganisationFilterBusinessObject");
			var manager = new AdministrationPanelManager(Factory);
			var form = new ZForm(manager);
			form.CaptionRenderingEnabled = true;
			var control = new DeduplicationOrganisationFilterControl(manager.DeduplicationOrganisationCollection, codeSender);
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
					AssertEquals("Number of potential duplicates with high confidence", GetResourceString("DOH_HighDuplicates").FullDescription);
					AssertEquals("Number of potential duplicates with medium confidence", GetResourceString("DOH_MediumDuplicates").FullDescription);
					AssertEquals("Number of potential duplicates with low confidence", GetResourceString("DOH_LowDuplicates").FullDescription);
					AssertEquals("Total number of potential duplicates found", GetResourceString("DOH_TotalDuplicates").FullDescription);
					AssertEquals("Branch that the organization was created under", GetResourceString("DOH_CreatedUnderBranch").FullDescription);
					AssertEquals("Company that the organization was created under", GetResourceString("DOH_CreatedUnderCompany").FullDescription);
					AssertEquals("Organization Associated Declarations Count", GetResourceString("DOH_DeclarationsCount").FullDescription);
					AssertEquals("Organization Associated Consolidations Count", GetResourceString("DOH_ConsolidationsCount").FullDescription);
					AssertEquals("Organization Associated Shipments Count", GetResourceString("DOH_ShipmentsCount").FullDescription);
				});

				CombineAssertions(() =>
				{
					AssertEquals("High Confidence Results", GetResourceString("DOH_HighDuplicates").Caption);
					AssertEquals("Medium Confidence Results", GetResourceString("DOH_MediumDuplicates").Caption);
					AssertEquals("Low Confidence Results", GetResourceString("DOH_LowDuplicates").Caption);
					AssertEquals("Total Results", GetResourceString("DOH_TotalDuplicates").Caption);
					AssertEquals("Created Under Company", GetResourceString("DOH_CreatedUnderCompany").Caption);
					AssertEquals("Created Under Branch", GetResourceString("DOH_CreatedUnderBranch").Caption);
					AssertEquals("Associated Declarations Count", GetResourceString("DOH_DeclarationsCount").Caption);
					AssertEquals("Associated Consolidations Count", GetResourceString("DOH_ConsolidationsCount").Caption);
					AssertEquals("Associated Shipments Count", GetResourceString("DOH_ShipmentsCount").Caption);
				});

				CombineAssertions(() =>
				{
					AssertEquals("High Results", GetResourceString("DOH_HighDuplicates").MediumCaption);
					AssertEquals("Medium Results", GetResourceString("DOH_MediumDuplicates").MediumCaption);
					AssertEquals("Low Results", GetResourceString("DOH_LowDuplicates").MediumCaption);
					AssertEquals("", GetResourceString("DOH_TotalDuplicates").MediumCaption);
					AssertEquals("By Company", GetResourceString("DOH_CreatedUnderCompany").MediumCaption);
					AssertEquals("By Branch", GetResourceString("DOH_CreatedUnderBranch").MediumCaption);
					AssertEquals("Associated Declarations", GetResourceString("DOH_DeclarationsCount").MediumCaption);
					AssertEquals("Associated Consolidations", GetResourceString("DOH_ConsolidationsCount").MediumCaption);
					AssertEquals("Associated Shipments", GetResourceString("DOH_ShipmentsCount").MediumCaption);
				});

				CombineAssertions(() =>
				{
					AssertEquals("High", GetResourceString("DOH_HighDuplicates").ShortCaption);
					AssertEquals("Medium", GetResourceString("DOH_MediumDuplicates").ShortCaption);
					AssertEquals("Low", GetResourceString("DOH_LowDuplicates").ShortCaption);
					AssertEquals("Total", GetResourceString("DOH_TotalDuplicates").ShortCaption);
					AssertEquals("Company", GetResourceString("DOH_CreatedUnderCompany").ShortCaption);
					AssertEquals("Branch", GetResourceString("DOH_CreatedUnderBranch").ShortCaption);
					AssertEquals("Declarations", GetResourceString("DOH_DeclarationsCount").ShortCaption);
					AssertEquals("Consolidations", GetResourceString("DOH_ConsolidationsCount").ShortCaption);
					AssertEquals("Shipments", GetResourceString("DOH_ShipmentsCount").ShortCaption);
				});

				ResourceStringData GetResourceString(string columnName) => (FilterControl.Grid.Columns[columnName].ColumnStyle as ZTextBoxColumnStyle).CaptionResourceString;
			}
		}

		[RequiresSTA]
		public void TestFilterStripTypeIsOrganisationFilterStrip()
		{
			using (Form)
			{
				Form.Show();
				AssertEquals("OrganisationFilterStrip", FilterControl.LastFilterStripType.Name);
			}
		}

		[RequiresSTA]
		public void TestEDIFilterGridColumns()
		{
			TearDown();
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				SetUp();
				using (Form)
				{
					Form.Show();
					AssertEquals("Including shared columns", 21, FilterControl.Grid.Columns.Count);
					CombineAssertions(() =>
					{
						Assert("Should not contain DOH_EnterpriseId Column", FilterControl.Grid.Columns.Contains("DOH_EnterpriseId"));
						Assert("Should not contain DOH_EnterpriseCode Column", FilterControl.Grid.Columns.Contains("DOH_EnterpriseCode"));
						Assert("Should not contain DOH_CompanyCode Column", FilterControl.Grid.Columns.Contains("DOH_CompanyCode"));
						Assert("Should not contain DOH_ProductId Column", FilterControl.Grid.Columns.Contains("DOH_ProductId"));
					});
				}
				TearDown();
			}
			SetUp();
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
					Assert("Contains DOH_FullName Column", FilterControl.Grid.Columns.Contains("DOH_FullName"));
					AssertEquals("Organization Name", FilterControl.Grid.Columns["DOH_FullName"].ColumnStyle.HeaderText);

					Assert("Contains DOH_Code Column", FilterControl.Grid.Columns.Contains("DOH_Code"));
					AssertEquals("Code", FilterControl.Grid.Columns["DOH_Code"].ColumnStyle.HeaderText);
				});

				CombineAssertions(() =>
				{
					Assert("Contains DOH_TotalDuplicates Column", FilterControl.Grid.Columns.Contains("DOH_TotalDuplicates"));
					Assert("Contains DOH_HighDuplicates Column", FilterControl.Grid.Columns.Contains("DOH_HighDuplicates"));
					Assert("Contains DOH_MediumDuplicates Column", FilterControl.Grid.Columns.Contains("DOH_MediumDuplicates"));
					Assert("Contains DOH_LowDuplicates Column", FilterControl.Grid.Columns.Contains("DOH_LowDuplicates"));
				});

				CombineAssertions(() =>
				{
					Assert("Should not contain DOH_EnterpriseId Column", !FilterControl.Grid.Columns.Contains("DOH_EnterpriseId"));
					Assert("Should not contain DOH_EnterpriseCode Column", !FilterControl.Grid.Columns.Contains("DOH_EnterpriseCode"));
					Assert("Should not contain DOH_CompanyCode Column", !FilterControl.Grid.Columns.Contains("DOH_CompanyCode"));
					Assert("Should not contain DOH_ProductId Column", !FilterControl.Grid.Columns.Contains("DOH_ProductId"));
				});

				CombineAssertions(() =>
				{
					Assert("Contains DOH_Status Column", FilterControl.Grid.Columns.Contains("DOH_Status"));
					AssertEquals("Status", FilterControl.Grid.Columns["DOH_Status"].ColumnStyle.HeaderText);

					Assert("Contains DOH_ExcludedBy Column", FilterControl.Grid.Columns.Contains("DOH_ExcludedBy"));
					AssertEquals("Excluded by", FilterControl.Grid.Columns["DOH_ExcludedBy"].ColumnStyle.HeaderText);
				});

				CombineAssertions(() =>
				{
					Assert($"Contains {DeduplicationOrganisation.Schema.DOH_CreatedUnderBranch} Column", FilterControl.Grid.Columns.Contains("DOH_CreatedUnderBranch"));
					Assert("DOH_CreatedUnderBranch is not mandatory", !FilterControl.Grid.Columns["DOH_CreatedUnderBranch"].IsMandatory);
					Assert("DOH_CreatedUnderBranch is not visible", !FilterControl.Grid.Columns["DOH_CreatedUnderBranch"].IsVisible);

					Assert($"Contains {DeduplicationOrganisation.Schema.DOH_CreatedUnderCompany} Column", FilterControl.Grid.Columns.Contains("DOH_CreatedUnderCompany"));
					Assert("DOH_CreatedUnderCompany is not mandatory", !FilterControl.Grid.Columns["DOH_CreatedUnderCompany"].IsMandatory);
					Assert("DOH_CreatedUnderCompany is not visible", !FilterControl.Grid.Columns["DOH_CreatedUnderCompany"].IsVisible);
				});

				CombineAssertions(() =>
				{
					Assert("Contains DOH_DeclarationsCount Column", FilterControl.Grid.Columns.Contains("DOH_DeclarationsCount"));
					Assert("DOH_DeclarationsCount is not mandatory", !FilterControl.Grid.Columns["DOH_DeclarationsCount"].IsMandatory);
					Assert("DOH_DeclarationsCount is not visible", !FilterControl.Grid.Columns["DOH_DeclarationsCount"].IsVisible);

					Assert("Contains DOH_ConsolidationsCount Column", FilterControl.Grid.Columns.Contains("DOH_ConsolidationsCount"));
					Assert("DOH_ConsolidationsCount is not mandatory", !FilterControl.Grid.Columns["DOH_ConsolidationsCount"].IsMandatory);
					Assert("DOH_ConsolidationsCount is not visible", !FilterControl.Grid.Columns["DOH_ConsolidationsCount"].IsVisible);

					Assert("Contains DOH_ShipmentsCount Column", FilterControl.Grid.Columns.Contains("DOH_ShipmentsCount"));
					Assert("DOH_ShipmentsCount is not mandatory", !FilterControl.Grid.Columns["DOH_ShipmentsCount"].IsMandatory);
					Assert("DOH_ShipmentsCount is not visible", !FilterControl.Grid.Columns["DOH_ShipmentsCount"].IsVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestShowMessageWhenRecordedNumberLargerThanRegistry()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTDUP1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTDUP2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TESTDUP3";

			Factory.Save();

			var codeFilter = FilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
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
			var codeFilter = FilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
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

		[RequiresSTA]
		public void TestActionMenu_ShouldIncludeRecalculateConfidenceScoreActionMenuITem()
		{
			var recalculateConfidenceScoreActionMenuItem = FilterControl.FilteredGrid.ContextMenu.MenuItems.FindByText("Recalculate Confidence Score");
			AssertNotNull("Recalculate Confidence Score action item should exist", recalculateConfidenceScoreActionMenuItem);
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

		DeduplicationOrganisationFilterControl FilterControl => Form.Controls.Find("FilterControl", true)[0] as DeduplicationOrganisationFilterControl;
		ZForm Form;

		[RequiresSTA]
		public void TestJobConsolidationsCountIsCorrect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			CreateConsolidationForOrg(org);
			CreateConsolidationForOrg(org);
			CreateConsolidationForOrg(org);
			CreateConsolidationForOrg(org);

			Factory.Save();

			var codeFilter = FilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
			codeFilter.IsActive = true;
			codeFilter.Property = org.OH_Code;

			FilterControl.FirePerformSearch();

			AssertEquals(1, FilterControl.GridCollection.Count);
			AssertEquals(4, ((DeduplicationOrganisation)FilterControl.GridCollection[0]).DOH_ConsolidationsCount);
		}

		[RequiresSTA]
		public void TestJobDeclarationsCountIsCorrect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			CreateDeclarationForOrg(org);
			CreateDeclarationForOrg(org);

			Factory.Save();

			var codeFilter = FilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
			codeFilter.IsActive = true;
			codeFilter.Property = org.OH_Code;

			FilterControl.FirePerformSearch();

			AssertEquals(1, FilterControl.GridCollection.Count);
			AssertEquals(2, ((DeduplicationOrganisation)FilterControl.GridCollection[0]).DOH_DeclarationsCount);
		}

		[RequiresSTA]
		public void TestJobShipmentsCountIsCorrect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			CreateShipmentForOrg(org);
			CreateShipmentForOrg(org);
			CreateShipmentForOrg(org);

			Factory.Save();

			var codeFilter = FilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
			codeFilter.IsActive = true;
			codeFilter.Property = org.OH_Code;

			FilterControl.FirePerformSearch();

			AssertEquals(1, FilterControl.GridCollection.Count);
			AssertEquals(3, ((DeduplicationOrganisation)FilterControl.GridCollection[0]).DOH_ShipmentsCount);
		}

		[RequiresSTA]
		public void TestRecalculateConfidenceScoreShouldUpdateSelectedRows()
		{
			using (Form)
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "TESTDUP1";
				org2.OH_Code = "TESTDUP2";

				var patternMatch1 = Factory.NewWithValidTestData<PatternMatchingResult>();
				patternMatch1.PMT_MasterPK = org1.PK;
				patternMatch1.PMT_TargetPK = org2.PK;
				patternMatch1.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
				patternMatch1.PMT_MasterTableCode = patternMatch1.PMT_TargetTableCode = "OH";
				patternMatch1.PMT_FoundTimeUtc = ZDateTime.UtcNow;
				patternMatch1.PMT_GS_NKExcludeBy = "";

				var patternMatch2 = Factory.NewWithValidTestData<PatternMatchingResult>();
				patternMatch2.PMT_MasterPK = org2.PK;
				patternMatch2.PMT_TargetPK = org1.PK;
				patternMatch2.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
				patternMatch2.PMT_MasterTableCode = patternMatch2.PMT_TargetTableCode = "OH";
				patternMatch2.PMT_FoundTimeUtc = ZDateTime.UtcNow;
				patternMatch2.PMT_GS_NKExcludeBy = "";

				Factory.Save();

				Form.Show();
				var codeFilter = FilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
				codeFilter.IsActive = true;
				codeFilter.Property = "TESTDUP";
				codeFilter.ComparisonOperator = SQLComparisonOperator.StartsWith.ToString();

				UnitTestUserNotification.Instance.ClearMessages();
				FilterControl.FirePerformSearch();

				var dedupOrg1 = ((DeduplicationOrganisation)FilterControl.GridCollection[0]).MasterOrgHeader;
				var dedupOrg2 = ((DeduplicationOrganisation)FilterControl.GridCollection[1]).MasterOrgHeader;
				var isDedupeOrg1ReloadRequiredPropagated = false;
				var isDedupeOrg2ReloadRequiredPropagated = false;
				dedupOrg1.DeduplicationActionOccurred += (o, e) => isDedupeOrg1ReloadRequiredPropagated = e.InvokedAction == DeduplicationAction.ReloadRequired;
				dedupOrg2.DeduplicationActionOccurred += (o, e) => isDedupeOrg2ReloadRequiredPropagated = e.InvokedAction == DeduplicationAction.ReloadRequired;

				FilterControl.Grid.SelectAllElements();
				AssertEquals("Precondition: 2 elements are selected", 2, FilterControl.Grid.SelectedElements.Length);

				var recalculateConfidenceScoreActionMenuItem = FilterControl.FilteredGrid.ContextMenu.MenuItems.FindByText("Recalculate Confidence Score");
				AssertNotNull("Precondition: action menu exists", recalculateConfidenceScoreActionMenuItem);

				var org1patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org1.PK));
				var org2patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org2.PK));

				CombineAssertions("Preconditions", () =>
				{
					AssertEquals("org1 has not propagated ReloadRequired event yet", false, isDedupeOrg1ReloadRequiredPropagated);
					AssertEquals("org2 has not propagated ReloadRequired event yet", false, isDedupeOrg2ReloadRequiredPropagated);
					AssertEquals("org1 has 1 pattern matching result", 1, org1patternMatchingResults.Length);
					AssertEquals("org2 has 1 pattern matching result", 1, org2patternMatchingResults.Length);
				});

				recalculateConfidenceScoreActionMenuItem.PerformClick();

				org1patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org1.PK));
				org2patternMatchingResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org2.PK));

				CombineAssertions("Confidence Score of both selected item is recalculated", () =>
				{
					AssertEquals("org1 has propagated ReloadRequired event", true, isDedupeOrg1ReloadRequiredPropagated);
					AssertEquals("org2 has propagated ReloadRequired event", true, isDedupeOrg2ReloadRequiredPropagated);
					AssertEquals("org1 has deleted all pattern matching results", 0, org1patternMatchingResults.Length);
					AssertEquals("org2 has deleted all pattern matching results", 0, org2patternMatchingResults.Length);
				});
			}
		}

		#region Implementation

		void CreateConsolidationForOrg(OrgHeader org)
		{
			var jobConsolidation = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			jobConsolidation[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;
		}

		void CreateDeclarationForOrg(OrgHeader org)
		{
			var jobConsolidation = Factory.NewWithValidTestData(ObjectFactory.GetType<Customs.IBaseJobDeclaration>());
			jobConsolidation[JobDeclarationSchema.JE_OH_Supplier] = org.PK;
			jobConsolidation[JobDeclarationSchema.JE_OH_Importer] = org.PK;
			jobConsolidation[JobDeclarationSchema.JE_OH_ShippingLine] = org.PK;
			jobConsolidation[JobDeclarationSchema.JE_OH_Forwarder] = org.PK;
		}

		void CreateShipmentForOrg(OrgHeader org)
		{
			var jobShipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			jobShipment[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;
			jobShipment[JobShipmentSchema.JS_OH_ImportBroker] = org.PK;
		}

		#endregion

	}
}
