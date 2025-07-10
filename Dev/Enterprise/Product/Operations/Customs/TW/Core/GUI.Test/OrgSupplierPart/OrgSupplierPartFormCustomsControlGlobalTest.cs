using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlGlobalTest : TestCaseWithFactory
	{
		public void TestAddPivotColumns()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = new OrgSupplierPartFormCustomsControlGlobal();
				form.Controls.Add(userControl);
				form.Show();
				var pivotGrid = userControl.FindSingle<ZGrid>("PivotGrid");
				CombineAssertions(() =>
				{
					var tariffColumn = pivotGrid.GetColumnStyle(Customs.Business.BaseCusClassPartPivot.Schema.CI_FormattedTariffNum);
					AssertNotNull("User control should have CI_FormattedTariffNum column", tariffColumn);
					AssertNotNull("User control should have CI_LastAuditedDate column", pivotGrid.GetColumnStyle(CusClassPartPivot.Schema.CI_LastAuditedDate));
					AssertNotNull("User control should have LastAuditedUserFullName column", pivotGrid.GetColumnStyle("LastAuditedUserFullName"));
					AssertNotNull("User control should have CI_PartPivotUOM column", pivotGrid.GetColumnStyle(CusClassPartPivot.Schema.CI_PartPivotUOM));
					AssertEquals("The width of CI_FormattedTariffNum should be 95px", 95, tariffColumn.Width);
				}

				);
			}
		}

		public void TestChangeControlsVisibility()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = new OrgSupplierPartFormCustomsControlGlobal();
				var pivot = orgSupplierPart.PivotsForBinding.AddNew();
				pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				form.Controls.Add(userControl);
				form.Show();
				var detailTabControl = userControl.FindSingle<ZTabControl>("DetailTabControl");
				var detailsTabPage = detailTabControl.FindSingle<ZTabPage>("DetailsTabPage");
				var modeOfStatisticsDropEdit = detailsTabPage.FindSingle<ZDropEdit>("CI_ModeOfStatisticsDropEdit");
				var dutyTreatmentDropEdit = detailsTabPage.FindSingle<ZDropEdit>("CI_DutyTreatmentDropEdit");
				AssertEquals("CI_ModeOfStatisticsDropEdit visibility when ChildType is Import", false, modeOfStatisticsDropEdit.Visible);
				AssertEquals("CI_DutyTreatmentDropEdit visibility when ChildType is Import", true, dutyTreatmentDropEdit.Visible);
				pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				AssertEquals("CI_ModeOfStatisticsDropEdit visibility when ChildType is Import", true, modeOfStatisticsDropEdit.Visible);
				AssertEquals("CI_DutyTreatmentDropEdit visibility when ChildType is Import", false, dutyTreatmentDropEdit.Visible);
				pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
				AssertEquals("CI_ModeOfStatisticsDropEdit visibility when ChildType is Import", true, modeOfStatisticsDropEdit.Visible);
				AssertEquals("CI_DutyTreatmentDropEdit visibility when ChildType is Import", true, dutyTreatmentDropEdit.Visible);
			}
		}

		public void TestEnvironmentalProtectionTariffsGroupBoxVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "3513200001", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff, "TRUE", tariff2);
			Factory.Save();
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = new OrgSupplierPartFormCustomsControlGlobal();
				var pivot = orgSupplierPart.PivotsForBinding.AddNew();
				pivot.CI_TariffNum = "3513200001";
				form.Controls.Add(userControl);
				form.Show();
				var eptGroup = userControl.FindSingle<ZGroupBox>("EnvironmentalProtectionTariffsGroupBox");
				AssertEquals("EnvironmentalProtectionTariffsGroupBox should not be visible when Environmental Protection Tariff is not applicable", false, eptGroup.Visible);
				pivot.CI_TariffNum = "2713200001";
				AssertEquals("EnvironmentalProtectionTariffsGroupBox should be visible when Environmental Protection Tariff is applicable", true, eptGroup.Visible);
			}
		}

		public void TestCaption()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = new OrgSupplierPartFormCustomsControlGlobal();
				var pivot = orgSupplierPart.PivotsForBinding.AddNew();
				pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				form.Controls.Add(userControl);
				form.Show();
				var ndescription = userControl.FindSingle<Customs.GUI.LongTextControl>("CI_NDescription");
				AssertEquals("Chinese Description", ndescription.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestCarInfoTabPageVisibility()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = new OrgSupplierPartFormCustomsControlGlobal();
				var pivot = orgSupplierPart.PivotsForBinding.AddNew();
				pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = "86";
				form.Controls.Add(userControl);
				form.Show();
				var carInfoTabPage = userControl.FindSingle<ZTabPage>(x => x.Name == "CarInfoTabPage");
				AssertEquals(true, carInfoTabPage.TabVisible);
				pivot.CI_TariffNum = "87";
				AssertEquals(true, carInfoTabPage.TabVisible);
				pivot.CI_TariffNum = "XX";
				AssertEquals(false, carInfoTabPage.TabVisible);
				pivot.CI_TariffNum = "87";
				pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				AssertEquals(false, carInfoTabPage.TabVisible);
				pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
				AssertEquals(true, carInfoTabPage.TabVisible);
			}
		}

		public void TestPropertiesofCalcEdits()
		{
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				var permitNumberGrid = control.FindSingleOrDefault<ZGrid>(c => c.Name == "PermitNumberGrid");
				var lineNoCalcEditColumnStyleInfo = permitNumberGrid.GetColumnStyle("CSI_LineNo") as ZCalcEditColumnStyleInfo;
				Assert(lineNoCalcEditColumnStyleInfo.ShowEmptyStringForEmptyValue);
			}
		}

		public void TestClassificationDescriptionTextBoxProperty()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = new OrgSupplierPartFormCustomsControlGlobal();
				form.Controls.Add(userControl);
				form.Show();
				var detailTabControl = userControl.FindSingle<ZTabControl>("DetailTabControl");
				var detailsTabPage = detailTabControl.FindSingle<ZTabPage>("DetailsTabPage");
				var classificationDescriptionTextBox = detailsTabPage.FindSingle<Customs.GUI.LongTextControl>("classificationDescriptionTextBox");
				Assert("ClassificationDescriptionTextBox visibility", classificationDescriptionTextBox.Visible);
				AssertEquals("ClassificationDescriptionTextBox TabIndex", 2, classificationDescriptionTextBox.TabIndex);
				AssertEquals("English Description", classificationDescriptionTextBox.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}
	}
}
