using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SqlSystemConfigurationsGrid
	{
		void InitializeComponent()
		{
			var columnInfos = new List<(string Name, int Width, HorizontalAlignment TextAlign, bool IsReadOnly, ResourceStringData CaptionResourceString)>
			{
				("ConfigurationId", 50, HorizontalAlignment.Center, true, Res.GetData("SqlSystemConfigurationsGrid|C7E6E34A-03DE-48B7-81DB-EE78816C3477", "Id")),
				("Name", 190, HorizontalAlignment.Left,true, Res.GetData("SqlSystemConfigurationsGrid|FD8FF997-72BE-422B-9C99-394379E0AC39", "Name")),
				("RequiresRestart", 120, HorizontalAlignment.Center,true, Res.GetData("SqlSystemConfigurationsGrid|DED1B473-632D-48AC-98C2-DCBE8FAB04AD", "Requires Restart")),
				("MinValue", 90, HorizontalAlignment.Right,true, Res.GetData("SqlSystemConfigurationsGrid|4F400713-7668-47A6-A6C2-CE9F086B5A46", "Min")),
				("MaxValue", 90, HorizontalAlignment.Right,true, Res.GetData("SqlSystemConfigurationsGrid|407A5DD6-C983-4B09-9C4C-240FDFDF41BE", "Max")),
				("ConfiguredValue", 110, HorizontalAlignment.Right, true, Res.GetData("SqlSystemConfigurationsGrid|607C90E9-8D0B-4975-AD04-6EC3B7BA5A06", "Configured Value")),
				("RecommendedValue", 110, HorizontalAlignment.Right, true, Res.GetData("SqlSystemConfigurationsGrid|AD3B8C87-CA5A-4F3D-A3A5-4D3C31DDF1C5", "Recommended Value")),
				("ProposedValueText", 95, HorizontalAlignment.Right, false, Res.GetData("SqlSystemConfigurationsGrid|950828DE-9145-49CC-A88D-072DF0CAB373", "Proposed Value")),
				("ItemDescription", 300, HorizontalAlignment.Left,true, Res.GetData("SqlSystemConfigurationsGrid|F3C48039-4EFE-4421-B01F-E33F3DA8E48D", "Description")),
			};

			grid = new ZGrid { EnableToolTips = true };
			grid.ColourDeciding += OnGridColorDeciding;

			((System.ComponentModel.ISupportInitialize)BindingSource).BeginInit();
			((System.ComponentModel.ISupportInitialize)grid).BeginInit();

			grid.SuspendLayout();
			SuspendLayout();

			grid.AllowNavigation = false;
			grid.CaptionVisible = false;

			BindingSource.DataSourceType = typeof(SqlSystemConfigurationsCollection);
			BindingSource.SetBindingMember(grid, ".");

			foreach (var info in columnInfos)
			{
				var column = new ZTextBoxColumnStyleInfo
				{
					ColumnName = info.Name,
					IsReadOnly = info.IsReadOnly,
					CaptionResourceString = info.CaptionResourceString,
					TextAlign = info.TextAlign
				};

				ControlDpiScalingHelper.SetWidth(ref column, info.Width, true);
				grid.ColumnStyles.Add(column);
			}

			grid.Dock = DockStyle.Fill;
			grid.GridId = "3E46B743-96D4-4E43-9503-4509911CEBD0";
			grid.HeaderForeColor = SystemColors.ControlText;
			grid.LayoutKey = nameof(grid);
			grid.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			grid.Name = nameof(grid);
			grid.Size = ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			grid.TabIndex = 0;

			AutoScaleMode = AutoScaleMode.Dpi;
			CaptionRenderingEnabled = true;
			Controls.Add(grid);

			Name = nameof(SqlSystemConfigurationsGrid);
			((System.ComponentModel.ISupportInitialize)BindingSource).EndInit();
			((System.ComponentModel.ISupportInitialize)grid).EndInit();

			grid.ResumeLayout(false);
			grid.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		Enterprise.ZArchitecture.ZGrid grid;
	}
}
