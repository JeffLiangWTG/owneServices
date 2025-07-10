using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TemplateConfigurationUserControlTest : TestCaseWithFactory
	{
		public void TestColumnGrid()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (ZForm form = new ZForm(GlbCompany.CurrentCompany))
			{
				using (TemplateConfigurationUserControl userControl = new TemplateConfigurationUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					var grid = userControl.Controls.Find("TemplateConfigurationGrid", true)[0] as ZGrid;
					IEnumerable<ZGridColumnInfo> columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
					ZGridColumnInfo levelNameColumn = columns.First(c => c.ColumnName == "LevelName");
					ZGridColumnInfo jobTypeColumn = columns.First(c => c.ColumnName == "ETF_JobType");
					ZGridColumnInfo jobTypeDescriptionColumn = columns.First(c => c.ColumnName == "JobTypeDescription");
					ZGridColumnInfo transportModeColumn = columns.First(c => c.ColumnName == "ETF_TransportMode");
					ZGridColumnInfo templateCodeColumn = columns.First(c => c.ColumnName == "ETF_TemplateCode");
					ZGridColumnInfo templateFileDescColumn = columns.First(c => c.ColumnName == "TemplateFileDesc");
					ZGridColumnInfo templateFileNameColumn = columns.First(c => c.ColumnName == "TemplateFileName");
					Assert("levelNameColumn should be read only.", levelNameColumn.IsReadOnly);
					Assert("jobTypeColumn should not be read only.", !jobTypeColumn.IsReadOnly);
					Assert("jobTypeDescriptionColumn should not be read only.", !jobTypeDescriptionColumn.IsReadOnly);
					Assert("transportModeColumn should not be read only.", !transportModeColumn.IsReadOnly);
					Assert("templateCodeColumn should not be read only.", !templateCodeColumn.IsReadOnly);
					Assert("templateFileDescColumn should be read only.", templateFileDescColumn.IsReadOnly);
					Assert("templateFileNameColumn should be read only.", templateFileNameColumn.IsReadOnly);
				}
			}
		}
	}
}
