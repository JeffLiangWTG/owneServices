using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Commission Agreement Wolf Pack Report")]
	public class CommissionAgreementWolfPackReportTemplateTest : TemplateTestCase
	{
		public void TestOpportunityEstimatedValueShare()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			SetupReportData();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			var staffCodeHeading = columnHeadings["Wolf Pack Staff Code"];
			staffCodeHeading.Hidden = false;
			staffCodeHeading.CurrentPosition = 0;

			var valueShareHeading = columnHeadings["Opportunity Estimated Value Share"];
			valueShareHeading.Hidden = false;
			valueShareHeading.CurrentPosition = 1;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				const int firstDataRow = 16;
				const int firstDataColumn = 2;
				var worksheet = excelInterface.WorkSheets[0];
				var staffsAndShareValues = new List<(string StaffCode, string ValueShare)>();
				staffsAndShareValues.Add((worksheet[firstDataRow, firstDataColumn].ToString(), excelInterface.Xls.GetStringFromCell(firstDataRow + 1, firstDataColumn + 2).ToString()));
				staffsAndShareValues.Add((worksheet[firstDataRow + 1, firstDataColumn].ToString(), excelInterface.Xls.GetStringFromCell(firstDataRow + 2, firstDataColumn + 2).ToString()));
				staffsAndShareValues.Sort((x, y) => x.StaffCode.CompareTo(y.StaffCode));

				AssertEquals("DEB", staffsAndShareValues[0].StaffCode);
				AssertEquals("740.40", staffsAndShareValues[0].ValueShare);

				AssertEquals("DEX", staffsAndShareValues[1].StaffCode);
				AssertEquals("493.60", staffsAndShareValues[1].ValueShare);
			}
		}

		public void TestOpportunityLabelValue()
		{
			using (OrganisationsDataRegistry.Instance.PotentialLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PotentialLabel"))
			using (OrganisationsDataRegistry.Instance.CurrentLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CurrentLabel"))
			{
				PrepareReportForRender();
				FillReportWithDefaultValues();
				SetupReportData();
				var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
				var potentialLabelHeading = columnHeadings["Opportunity PotentialLabel"];
				potentialLabelHeading.Hidden = false;
				potentialLabelHeading.CurrentPosition = 0;

				var currentLabelHeading = columnHeadings["Opportunity CurrentLabel"];
				currentLabelHeading.Hidden = false;
				currentLabelHeading.CurrentPosition = 1;

				using (var stream = new MemoryStream())
				using (var excelInterface = new ExcelInterface())
				{
					Report.Save(stream);
					stream.Position = 0;
					excelInterface.LoadExcelFile(stream);

					var worksheet = excelInterface.WorkSheets[0];
					var opportunityCurrent = worksheet[16, 2].ToString();
					var opportunityPotential = worksheet[16, 3].ToString();

					AssertEquals("2", opportunityCurrent);
					AssertEquals("1", opportunityPotential);
				}
			}
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<SalesTeam>();
			Factory.Save();
		}

		void SetupReportData()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "72 O'Riordan St";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "DEB";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "DEX";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_DiscountAmount = 1m;
			opportunity.P8_RentalMultiplier = 2m;
			opportunity.P8_EstimatedValue = 1234m;
			opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";

			var commissionAgreement = opportunity.ApprovedCommissionAgreements.AddNew();
			commissionAgreement.CA0_OH_Customer = org.PK;
			commissionAgreement.FillWithValidTestData();
			commissionAgreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement.CA0_EffectiveDate = ZDate.Today;
			var item = commissionAgreement.ProductItems.AddNew(true, "SHP");
			item.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var agreementPctRecipient1 = commissionAgreement.Recipients.AddNew();
			agreementPctRecipient1.CAR_GS_NKStaff = "DEX";
			agreementPctRecipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient1.CAR_Share = 4;
			var agreementPctRecipient1Rate = agreementPctRecipient1.Rates.AddNew();
			agreementPctRecipient1Rate.CAT_CommissionPercentage = 10;

			var agreementPctRecipient2 = commissionAgreement.Recipients.AddNew();
			agreementPctRecipient2.CAR_GS_NKStaff = "DEB";
			agreementPctRecipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient2.CAR_Share = 6;
			var agreementPctRecipient2Rate = agreementPctRecipient2.Rates.AddNew();
			agreementPctRecipient2Rate.CAT_CommissionPercentage = 10;

			Factory.Save();
		}
	}

	public class CommissionAgreementWolfPackReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new SalesMgrReports();

		public override string MenuName => "Commission Agreement Wolf Pack Report";

		public override string Hint => "This report shows a list of Commission Agreements set on Opportunities, organized by their Agreements details (ID, Stream, Status, Wolf Pack Staff and Organization)";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new CommissionAgreementWolfPackReportTemplateTest();
		}
	}
}
