using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.MasterFiles.Testing
{
	public class OrgSupplierPartFormCustomsControlGlobalTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestTariffControlsVisibility()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.TariffCodeFindBox.Visible);
				AssertEquals(true, control.PartsOfClassificationNZCClassFindBox.Visible);
				AssertEquals(true, control.ConcessionCodeCodeFindBox.Visible);

				AssertEquals(false, control.TariffNumFindBox.Visible);
				AssertEquals(false, control.PartsOfClassificationFindBox.Visible);
				AssertEquals(false, control.ConcessionCodeDropEdit.Visible);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.TariffCodeFindBox.Visible);
				AssertEquals(false, control.PartsOfClassificationNZCClassFindBox.Visible);
				AssertEquals(false, control.ConcessionCodeCodeFindBox.Visible);

				AssertEquals(true, control.TariffNumFindBox.Visible);
				AssertEquals(true, control.PartsOfClassificationFindBox.Visible);
				AssertEquals(true, control.ConcessionCodeDropEdit.Visible);
			}
		}

		[TestDate(2023, 6, 1)]
		public void TestTariffFindBoxEffectiveDate()
		{
			var orgSupplierPart = Factory.New<Business.MasterFiles.OrgSupplierPart>();
			orgSupplierPart.PivotsForBinding.AddNew();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				form.Show();
				form.Controls.Add(control);
				form.SetDataBinding(orgSupplierPart, ".");
				var grid = control.FindSingle<ZGrid>("PivotGrid");

				AssertEquals(new ZDateTime(2023, 6, 1), control.TariffNumFindBox.GetEffectiveDate.Invoke());
				AssertEquals(new ZDateTime(2023, 6, 1), control.PartsOfClassificationFindBox.GetEffectiveDate.Invoke());
				AssertEquals(new ZDateTime(2023, 6, 1), (grid.GetColumnStyle(control.TariffColumnName) as Universal.GUI.TariffColumnStyleInfo).GetEffectiveDate.Invoke());
			}
		}

		public void TestColumnAvailability()
		{
			var orgSupplierPart = Factory.New<Business.MasterFiles.OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = (OrgSupplierPartFormCustomsControlGlobal)GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("PivotGrid");
				AssertEquals("TariffNum", false, grid.GetColumnStyle(control.TariffColumnName).IsUnavailable);
				AssertEquals("DateStart", true, grid.GetColumnStyle(control.DateStartColumnName).IsUnavailable);
				AssertEquals("DateEnd", true, grid.GetColumnStyle(control.DateEndColumnName).IsUnavailable);
				AssertEquals("Tariff Column is customised", CusClassPartPivot.Schema.CI_TariffNum, control.TariffColumnName);
			}
		}

		public void TestClassificationAndTariff()
		{
			var orgSupplierPart = Factory.New<Business.MasterFiles.OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = (OrgSupplierPartFormCustomsControlGlobal)GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivot = orgSupplierPart.PivotsForBinding[0];
				var grid = control.FindSingle<ZGrid>("PivotGrid");
				var tariffColumn = grid.GetColumnStyle(control.TariffColumnName);
				AssertType("tariffColumn is NZCClassColumnStyleInfo", typeof(NZCClassColumnStyleInfo), tariffColumn);
				var classificationColumn = grid.GetColumnStyle(BaseCusClassPartPivot.Schema.CI_CC);
				var tariffFindBox = control.FindSingle<NZCClassFindBox>("TariffCodeFindBox");
				var lookupFindBox = control.FindSingle<ZGuidFindBox>("LookupFindBox");
				AssertEquals("tariffColumn visible", true, tariffColumn.IsVisible);
				AssertEquals("classificationColumn visible", true, classificationColumn.IsVisible);
				AssertEquals("tariffFindBox visible", true, tariffFindBox.Visible);
				AssertEquals("lookupFindBox visible", true, lookupFindBox.Visible);
				AssertEquals("tariffFindBox ReadOnly", false, tariffFindBox.ReadOnly);
				AssertEquals("lookupFindBox ReadOnly", false, lookupFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.NewZGuid();
				AssertEquals("tariffFindBox ReadOnly", true, tariffFindBox.ReadOnly);
				AssertEquals("lookupFindBox ReadOnly", false, lookupFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.Empty;
				pivot.CI_FormattedTariffNum = "12345";
				AssertEquals("tariffFindBox ReadOnly", false, tariffFindBox.ReadOnly);
				AssertEquals("lookupFindBox ReadOnly", true, lookupFindBox.ReadOnly);
			}
		}

		public void TestAuditClassificationControlsAreReadOnly()
		{
			var orgSupplierPart = Factory.New<Business.MasterFiles.OrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("LastAuditedByCodeFindBox ReadOnly", true, control.FindSingle<ZCodeFindBox>("LastAuditedByCodeFindBox").ReadOnly);
				AssertEquals("AuditDateEdit ReadOnly", true, control.FindSingle<ZDateEdit>("AuditDateEdit").ReadOnly);
			}
		}

		protected override void AssertTariffColumns(Core.Forms.ZGridColumnInfo columnStyleInfo)
		{
			AssertType<NZCClassColumnStyleInfo>("TariffColumnStyleInfo", columnStyleInfo);
		}

		protected override ZUserControl GetUserControl()
		{
			return new OrgSupplierPartFormCustomsControlGlobal();
		}

		protected override string UserControlName => "OrgSupplierPartFormCustomsControlGlobal";
		protected override string ExpectedTariffColumnName => BaseCusClassPartPivot.Schema.CI_TariffNum;
	}
}
