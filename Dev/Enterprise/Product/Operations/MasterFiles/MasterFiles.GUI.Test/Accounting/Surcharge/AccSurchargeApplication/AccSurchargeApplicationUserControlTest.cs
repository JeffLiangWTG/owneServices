using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccSurchargeApplication))]
	public class AccSurchargeApplicationUserControlTest : BasherTest
	{
		public void TestSurchargeApplicationGrid()
		{
			using (var form = GetFormToBash())
			{
				var grid = form.GetControl<ZGrid>("surchargeApplicationGrid");

				var expectedListOfColumns = new[]
				{
					$"{AccSurchargeApplication.Schema.ASP_JobType} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccSurchargeApplication.Schema.ASP_SupplyType} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccSurchargeApplication.Schema.ASP_HomeCountryOrZone} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccSurchargeApplication.Schema.ASP_OrganizationCategory} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccSurchargeApplication.Schema.ASP_PlaceOfSupply} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccSurchargeApplication.Schema.ASP_ASC_NKSurchargeCode} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccSurchargeApplication.Schema.ASP_AT} (ZGuidFindBoxColumnStyleInfo) IsVisible:False",
					$"SurchargeCodeDescription (ZTextBoxColumnStyleInfo) IsVisible:True",
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();

				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		public void TestSupplyTypeColumn()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = GetFormToBash())
			{
				form.Show();
				var surchargeApplicationGrid = (ZGrid)form.Controls.Find("surchargeApplicationGrid", true).SingleOrDefault();
				AssertNotNull("Grid should not be null", surchargeApplicationGrid);
				var columnInfo = surchargeApplicationGrid.GetColumnStyle("ASP_SupplyType");
				Assert("'ASP_SupplyType' IsUnavailable shoule be false.", !columnInfo.IsUnavailable);
			}

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			using (var form = GetFormToBash())
			{
				form.Show();
				var surchargeApplicationGrid = (ZGrid)form.Controls.Find("surchargeApplicationGrid", true).SingleOrDefault();
				AssertNotNull("Grid should not be null", surchargeApplicationGrid);
				var columnInfo = surchargeApplicationGrid.GetColumnStyle("ASP_SupplyType");
				Assert("'ASP_SupplyType' IsUnavailable should be true.", columnInfo.IsUnavailable);
			}
		}

		public void TestASP_PlaceOfSupplyColumn()
		{
			var settings = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.Value;
			foreach (CodeDescriptionBool code in settings)
			{
				code.Bool = true;
			}
			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);

			using (var form = GetFormToBash())
			{
				form.Show();
				var surchargeApplicationGrid = (ZGrid)form.Controls.Find("surchargeApplicationGrid", true).SingleOrDefault();
				AssertNotNull("Grid should not be null", surchargeApplicationGrid);
				var columnInfo = surchargeApplicationGrid.GetColumnStyle("ASP_PlaceOfSupply");
				Assert("'ASP_PlaceOfSupply' IsUnavailable should be false.", !columnInfo.IsUnavailable);
			}

			foreach (CodeDescriptionBool code in settings)
			{
				code.Bool = false;
			}
			AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);

			using (var form = GetFormToBash())
			{
				form.Show();
				var surchargeApplicationGrid = (ZGrid)form.Controls.Find("surchargeApplicationGrid", true).SingleOrDefault();
				AssertNotNull("Grid should not be null", surchargeApplicationGrid);
				var columnInfo = surchargeApplicationGrid.GetColumnStyle("ASP_PlaceOfSupply");
				Assert("'ASP_PlaceOfSupply' IsUnavailable should be true.", columnInfo.IsUnavailable);
			}
		}

		GlbCompany company;
		protected override void SetUp()
		{
			base.SetUp();
			company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true
		}

		public override Form GetFormToBash() => CreateFormForTest();

		ZChildForm CreateFormForTest()
		{
			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AccSurchargeApplicationUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(company, nameof(GlbCompany.AccSurchargeApplications));
			return form;
		}
	}
}
