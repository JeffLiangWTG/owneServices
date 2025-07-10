using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module.Testing
{
	sealed class JobSeaSailingFilterControlForTest : JobSailingFilterControlForTest
	{
		public void TestJobSeaSailingFilterControlChangedColumnsStyle()
		{
			using (ZForm form = new ZForm(Voyages))
			{
				form.Controls.Add(FilterControl);
				FilterControl.FilteredGrid.SetDataBinding(Voyages, "");
				form.Show();

				ZGridColumn jX_JV_NKVessel = null;
				ZGridColumn jX_JV_VoyageFlight = null;

				foreach (ZGridColumn column in FilterControl.FilteredGrid.Columns)
				{
					if (column.ColumnStyle.MappingName == "JX_JV_NKVessel")
					{
						jX_JV_NKVessel = column;
					}
					if (column.ColumnStyle.MappingName == "JX_JV_VoyageFlight")
					{
						jX_JV_VoyageFlight = column;
					}
				}

				jX_JV_NKVessel.ColumnStyle.Width = 300;
				jX_JV_VoyageFlight.ColumnStyle.Width = 300;

				AssertEquals("The value should be Vessel", "Vessel", jX_JV_NKVessel.ColumnStyle.HeaderText);
				AssertEquals("The value should be Voyage", "Voyage Number", jX_JV_VoyageFlight.ColumnStyle.HeaderText);
			}
		}

		public void TestJobSeaSailingFilterControl_Columns()
		{
			using (var form = new ZForm(Voyages))
			{
				form.Controls.Add(FilterControl);
				FilterControl.FilteredGrid.SetDataBinding(Voyages, "");
				form.Show();

				var exchangeRateColumn = FilterControl.FilteredGrid.Columns.FirstOrDefault(c => c.ColumnName == "ExchangeRate");
				AssertNotNull(exchangeRateColumn);
				Assert(!exchangeRateColumn.IsVisible);
			}
		}

		#region Implementation

		protected override IEnumerable<string> DateColumnStylesShouldHaveLongFormat
			=> new string[] {
			JobSailing.Schema.JX_JA_CTOCutOff,
			JobSailing.Schema.JX_JA_CTOReceivalCommences,
			JobSailing.Schema.JX_JB_CTOAvailabilityDate,
			JobSailing.Schema.JX_JB_CTOStorageDate,
			JobSailing.Schema.JX_JA_DocumentaryCutoff,
			JobSailing.Schema.JX_JA_VGMCutOff,
			JobSailing.Schema.JX_JA_DGFCLReceivalCommences,
			JobSailing.Schema.JX_JA_DGFCLCutOff,
		};

		protected override IEnumerable<string> DateColumnStylesShouldHaveShortFormat
			=> new string[] {
			JobSailing.Schema.JX_DepotCutOff,
			JobSailing.Schema.JX_DepotReceivalCommences,
			JobSailing.Schema.JX_DepotAvailabilityDate,
			JobSailing.Schema.JX_DepotStorageDate,

			JobSailing.Schema.JX_JA_E_ARV,
			JobSailing.Schema.JX_JA_S_ARV,
			JobSailing.Schema.JX_JA_A_ARV,
			JobSailing.Schema.JX_JA_E_DEP,
			JobSailing.Schema.JX_JA_S_DEP,
			JobSailing.Schema.JX_JA_A_DEP,
			JobSailing.Schema.JX_JB_E_ARV,
			JobSailing.Schema.JX_JB_S_ARV,
			JobSailing.Schema.JX_JB_A_ARV,
		};

		public override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobSeaSailingFilterBusinessObject();

		public override JobSailingFilterControl GetNewJobSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			=> new JobSeaSailingFilterControl(gridCollection, filterBusinessObject);

		#endregion
	}
}
