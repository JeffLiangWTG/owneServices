using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class FCLPackLinesControlTest : BaseAgencyTest
	{
		public void TestAdditionalColumns_PackLineCustomColumns()
		{
			RunControlTest(control => AssertEquals(true, GetPackLinesGrid(control).Columns.Contains(JobPackLinesSchema.Constants.JL_CustomAttrib1)));
		}

		public void TestAdditionalColumns_UNDG()
		{
			RunControlTest(control => AssertEquals(true, GetPackLinesGrid(control).Columns.Contains("UNDGs+UNDGSubstanceManagerGuid+Value")));
		}

		public void TestHarmonisedCodeColumn()
		{
			RunControlTest(control =>
			{
				var harmonisedCodeColumnInfo = (TariffColumnStyleInfo)GetPackLinesGrid(control).ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == AutoJobPackLines.Schema.JL_HarmonisedCode);
				AssertNotNull(harmonisedCodeColumnInfo);
				AssertNull(harmonisedCodeColumnInfo.GetCountryCode?.Invoke());
				AssertEquals("WCO", harmonisedCodeColumnInfo.GetDataGrouping());
				AssertEquals("HSN", harmonisedCodeColumnInfo.TariffType);
			});
		}

		public void TestShowTotals()
		{
			RunControlTest(control =>
			{
				var totalsPanel = control.Controls.Find("TotalsBottomPanel", true)[0];
				AssertEquals("ShowTotals is true by default", true, control.ShowTotals);
				AssertEquals("Totals are visible by default", true, totalsPanel.Visible);
				control.ShowTotals = false;
				AssertEquals(false, control.ShowTotals);
				AssertEquals(false, totalsPanel.Visible);
				control.ShowTotals = true;
				AssertEquals(true, control.ShowTotals);
				AssertEquals(true, totalsPanel.Visible);
			});
		}

		public void TestUpdateTotals()
		{
			RunControlTest(control =>
			{
				var shipment = control.CurrentDataItem as AgencyShipment;
				bool totalPacksUpdated = false;
				shipment.TotalOuterPacksInfo.ValueChanged += (s, e) => totalPacksUpdated = true;
				bool totalVolumeUpdated = false;
				shipment.TotalOuterPacksVolumeInfo.ValueChanged += (s, e) => totalVolumeUpdated = true;
				bool totalWeightUpdated = false;
				shipment.TotalOuterPacksWeightInfo.ValueChanged += (s, e) => totalWeightUpdated = true;
				Action<Action> assertTotalsUpdated = (triggeringAction) =>
				{
					totalPacksUpdated = false;
					totalVolumeUpdated = false;
					totalWeightUpdated = false;
					triggeringAction();
					AssertEquals(true, totalPacksUpdated);
					AssertEquals(true, totalVolumeUpdated);
					AssertEquals(true, totalWeightUpdated);
				};
				AgencyShipmentPackLine packLine = null;
				assertTotalsUpdated(() => packLine = shipment.OuterPackLines.AddNew());
				assertTotalsUpdated(() => packLine.JL_PackageCount = 10);
				assertTotalsUpdated(() => packLine.JL_ActualVolume = 10);
				assertTotalsUpdated(() => packLine.JL_ActualWeight = 10);
			});
		}

		public void TestTestExportReferenceNumber()
		{
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.China, "CNXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.China, "TWXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.China, "HKXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.China, "AUMEL", "Export Reference Number");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.Taiwan, "CNXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.Taiwan, "TWXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.Taiwan, "HKXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.Taiwan, "AUMEL", "Export Reference Number");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.HongKong, "CNXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.HongKong, "TWXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.HongKong, "HKXXX", "Shipping Order/Shi Lian Dan");
			AssertTestExportReferenceNumber(Core.Constants.CountryCodes.HongKong, "AUMEL", "Export Reference Number");
		}

		void AssertTestExportReferenceNumber(string currentLoginCountry, string portOfOrigin, string expectedColumnText)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(currentLoginCountry))
			{
				RunControlTest(control =>
				{
					var shipment = control.CurrentDataItem as AgencyShipment;
					AssertNotNull(shipment);
					shipment.JS_RL_NKOrigin = portOfOrigin;
					var exportReferenceColumn = GetPackLinesGrid(control).Columns.FirstOrDefault(c => c.ColumnName == AgencyShipmentPackLine.Schema.JL_ExportRefNumber);
					AssertNotNull(exportReferenceColumn);
					AssertEquals(expectedColumnText, exportReferenceColumn.ColumnStyle.HeaderText);
				});
			}
		}

		#region Implementation
		void RunControlTest(Action<FCLPackLinesControl> testAction)
		{
			var shipment = Factory.New<AgencyShipment>();
			using (ZForm form = new ZForm(shipment))
			{
				var control = new FCLPackLinesControl();
				control.Dock = DockStyle.Fill;
				control.SetDataBinding(shipment, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				testAction(control);
			}
		}

		ZGrid GetPackLinesGrid(FCLPackLinesControl control)
		{
			return (ZGrid)control.Controls.Find("PackLinesGrid", true)[0];
		}
		#endregion
	}
}
