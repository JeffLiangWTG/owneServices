using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestColumnAvailability()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = new OrgSupplierPartControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("PivotGrid");
				AssertEquals("TariffNum", false, grid.GetColumnStyle(control.TariffColumnName).IsUnavailable);
				AssertEquals("DateStart", true, grid.GetColumnStyle(control.DateStartColumnName).IsUnavailable);
				AssertEquals("DateEnd", true, grid.GetColumnStyle(control.DateEndColumnName).IsUnavailable);
				AssertEquals("Tariff Column is customised", Business.CusClassPartPivot.Schema.CI_TariffNum, control.TariffColumnName);
			}
		}

		public void TestClassificationAndTariff()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = new OrgSupplierPartControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivot = orgSupplierPart.PivotsForBinding[0];
				var grid = control.FindSingle<ZGrid>("PivotGrid");
				var tariffColumn = grid.GetColumnStyle(control.TariffColumnName);
				AssertType("tariffColumn is TariffColumnStyleInfo", typeof(TariffColumnStyleInfo), tariffColumn);
				var classificationColumn = grid.GetColumnStyle(Customs.Business.BaseCusClassPartPivot.Schema.CI_CC);
				var tariffFindBox = control.FindSingle<Universal.GUI.TariffFindBox>("TariffCodeFindBox");
				var classificationFindBox = control.FindSingle<ZGuidFindBox>("ClassificationFindBox");
				AssertEquals("tariffColumn visible", true, tariffColumn.IsVisible);
				AssertEquals("classificationColumn visible", true, classificationColumn.IsVisible);
				AssertEquals("tariffFindBox visible", true, tariffFindBox.Visible);
				AssertEquals("classificationFindBox visible", true, classificationFindBox.Visible);
				AssertEquals("tariffFindBox ReadOnly", false, tariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", false, classificationFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.NewZGuid();
				AssertEquals("tariffFindBox ReadOnly", true, tariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", false, classificationFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.Empty;
				pivot.CI_FormattedTariffNum = "12345";
				AssertEquals("tariffFindBox ReadOnly", false, tariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", true, classificationFindBox.ReadOnly);
			}
		}

		public void TestProductCodesGrid()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "PART1";
			var classification = Factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_TariffNum = "22030090";
			var pivot = orgSupplierPart.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var partInOtherFactory = otherFactory.Load<OrgSupplierPart>(orgSupplierPart.PK);
			using (var form = new ZForm(partInOtherFactory))
			using (var control = new OrgSupplierPartControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("PivotGrid");
				var productCodesGrid = control.FindSingle<ZGrid>("ProductCodesGrid");
				AssertEquals("ProductCodesGrid visible", true, productCodesGrid.Visible);
				AssertEquals("ProductCodesGrid ReadOnly", true, productCodesGrid.ReadOnly);
				var pivotInOtherFactory = partInOtherFactory.PivotsForBinding[0];
				pivotInOtherFactory.CI_CC = ZGuid.Empty;
				pivotInOtherFactory.CI_FormattedTariffNum = "12345";
				AssertEquals("ProductCodesGrid ReadOnly", false, productCodesGrid.ReadOnly);
			}
		}

		protected override void AssertTariffColumns(Core.Forms.ZGridColumnInfo columnStyleInfo)
		{
			AssertType<TariffColumnStyleInfo>("TariffColumnStyleInfo", columnStyleInfo);
		}

		protected override ZUserControl GetUserControl() => new OrgSupplierPartControl();

		protected override string UserControlName => nameof(OrgSupplierPartControl);

		protected override string ExpectedTariffColumnName => Business.CusClassPartPivot.Schema.CI_TariffNum;
	}
}
