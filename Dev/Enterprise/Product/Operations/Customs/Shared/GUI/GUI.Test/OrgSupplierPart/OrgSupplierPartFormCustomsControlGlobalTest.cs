using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlGlobalBaseOnlyTest : OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestTariffColumnUseCorrectCountry()
		{
			var part = Factory.New<Business.OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.Guadeloupe;
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_RN_NKCountry = Core.Constants.CountryCodes.Guadeloupe;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Liechtenstein))
			using (var form = new ZForm(part))
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var pivotGrid = (ZGrid)control.Controls.Find("PivotGrid", true)[0];
				pivotGrid.ListManager.Position = 1;
				var tariffColumnStyleInfo = (Universal.GUI.TariffColumnStyleInfo)pivotGrid.GetColumnStyle(control.TariffColumnName);
				AssertEquals("TariffColumnStyleInfo.GetCountryCode()", Core.Constants.CountryCodes.Guadeloupe, tariffColumnStyleInfo.GetCountryCode());
				AssertEquals("TariffColumnStyleInfo.GetDataGrouping()", Core.Constants.CountryCodes.France, tariffColumnStyleInfo.GetDataGrouping());

				pivot1.Delete();
				pivot2.Delete();
				AssertEquals("TariffColumnStyleInfo.GetCountryCode() - Deleted", Core.Constants.CountryCodes.Liechtenstein, tariffColumnStyleInfo.GetCountryCode());
				AssertEquals("TariffColumnStyleInfo.GetDataGrouping() - Deleted", Core.Constants.CountryCodes.Switzerland, tariffColumnStyleInfo.GetDataGrouping());

				using (var control2 = new OrgSupplierPartFormCustomsControlGlobal())
				{
					control2.CreateControl();
					pivotGrid = (ZGrid)control2.Controls.Find("PivotGrid", true)[0];
					AssertEquals("TariffColumnStyleInfo.GetCountryCode() - Null", Core.Constants.CountryCodes.Liechtenstein, tariffColumnStyleInfo.GetCountryCode());
					AssertEquals("TariffColumnStyleInfo.GetDataGrouping() - Null", Core.Constants.CountryCodes.Switzerland, tariffColumnStyleInfo.GetDataGrouping());
				}
			}
		}

		public void TestDeletePivotByDataRefresh()
		{
			var part = Factory.NewWithValidTestData<Business.OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_TariffNum = "19112500";
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = "19112501";
			Factory.Save();
			using (var frm = new ZForm(part))
			using (var ctr = new OrgSupplierPartFormCustomsControlGlobalForTest())
			{
				frm.Controls.Add(ctr);
				frm.Show();
				Application.DoEvents();
				var pivotGrid = (ZGrid)ctr.Controls.Find("PivotGrid", true)[0];
				pivotGrid.Select(0);
				AssertSame("Should set the current pivot.", pivot1, ctr.CurrentPivot);
				Assert("Should not be deleted.", !pivot1.IsDeleted);
				((IBusiness)pivot1).DeleteForDataRefresh();
				Assert("Should be deleted from the data refresh.", pivot1.IsDeleted);
				AssertEquals("Should change to the next pivot data from the data refresh.", pivot2, ctr.CurrentPivot);
			}
		}

		sealed class OrgSupplierPartFormCustomsControlGlobalForTest : OrgSupplierPartFormCustomsControlGlobal
		{
			public BaseCusClassPartPivot CurrentPivot => currentPartPivot;
		}
	}

	public abstract class OrgSupplierPartFormCustomsControlGlobalTest : OrgSupplierPartFormCustomsControlAbstractTest
	{
		protected OrgSupplierPartFormCustomsControlGlobalTest()
		{ }

		public void TestTariffColumn()
		{
			var part = Factory.NewWithValidTestData<Business.OrgSupplierPart>();
			var cusClassPivot = part.PivotsForBinding.AddNew();
			Factory.Save();
			using (var form = new ZForm(part))
			using (var control = GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivotGrid = (ZGrid)control.Controls.Find("PivotGrid", true)[0];
				AssertTariffColumns(pivotGrid.GetColumnStyle(ExpectedTariffColumnName));
			}
		}

		protected override ZUserControl GetUserControl() => new OrgSupplierPartFormCustomsControlGlobal();

		protected override string UserControlName => "OrgSupplierPartFormCustomsControlGlobal";

		protected virtual void AssertTariffColumns(Core.Forms.ZGridColumnInfo columnStyleInfo)
		{
			AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", columnStyleInfo);
			var tariffColumnStyleInfo = (Universal.GUI.TariffColumnStyleInfo)columnStyleInfo;
			AssertEquals("TariffColumnStyleInfo.TariffType", ExpectedUniversalTariffType, tariffColumnStyleInfo.TariffType);
			AssertEquals("TariffColumnStyleInfo.GetCountryCode()", ExpectedCustomsCountryCode, tariffColumnStyleInfo.GetCountryCode());
			AssertEquals("TariffColumnStyleInfo.GetDataGrouping()", ExpectedDataGrouping, tariffColumnStyleInfo.GetDataGrouping());
		}

		protected virtual string ExpectedTariffColumnName => BaseCusClassPartPivot.Schema.CI_FormattedTariffNum;
		protected virtual string ExpectedUniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;
		protected virtual string ExpectedCustomsCountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		protected virtual string ExpectedDataGrouping => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
