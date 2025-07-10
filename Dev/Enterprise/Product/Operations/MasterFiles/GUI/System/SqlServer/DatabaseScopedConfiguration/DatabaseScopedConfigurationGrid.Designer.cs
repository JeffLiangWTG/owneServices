using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	sealed partial class DatabaseScopedConfigurationGrid
	{
		void InitializeComponent()
		{
			var columnInfos = new List<(string Name, int Width, HorizontalAlignment TextAlign, bool IsReadOnly, ResourceStringData CaptionResourceString)>
			{
				("ConfigurationId", 50, HorizontalAlignment.Center, true, Res.GetData("DatabaseScopedConfigurationGrid|BF5F3C37-4BFD-4897-A9A3-3BB688E89B55", "Id")),
				("Name", 220, HorizontalAlignment.Left,true, Res.GetData("DatabaseScopedConfigurationGrid|8D8935FF-B6AA-451A-B8E7-0F7184EB8489", "Name")),
				("CurrentValueText", 160, HorizontalAlignment.Left,true, Res.GetData("DatabaseScopedConfigurationGrid|FA10E2EC-0FBD-43DD-AB9D-86D46D969011", "Current Value")),
				("CurrentValueForSecondaryText", 160, HorizontalAlignment.Left,true, Res.GetData("DatabaseScopedConfigurationGrid|207A4DBF-A08B-42AE-A028-0203A4D523F2", "Current Value For Secondary")),
				("IsValueDefault", 110, HorizontalAlignment.Left,true, Res.GetData("DatabaseScopedConfigurationGrid|289D7086-27B3-49FA-B5F0-286950A92C7F", "Is Value Default")),
				("RecommendedValue", 130, HorizontalAlignment.Right, true, Res.GetData("DatabaseScopedConfigurationGrid|E2663E8C-0126-4DAB-8890-22D7F68C8906", "Recommended Value")),
				("ProposedValue", 220, HorizontalAlignment.Right, false, Res.GetData("DatabaseScopedConfigurationGrid|6E8B5413-C1D0-46BD-BD64-FB7E86356658", "Proposed Value")),
			};

			grid = new ZGrid { EnableToolTips = true };
			grid.ColourDeciding += OnGridColorDeciding;

			((System.ComponentModel.ISupportInitialize)BindingSource).BeginInit();
			((System.ComponentModel.ISupportInitialize)grid).BeginInit();

			grid.SuspendLayout();
			SuspendLayout();

			grid.AllowNavigation = false;
			grid.CaptionVisible = false;

			BindingSource.DataSourceType = typeof(DatabaseScopedConfigurationCollection);
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
			grid.GridId = "E3381B43-89E0-4A6A-95E1-E49B8B56B73D";
			grid.HeaderForeColor = SystemColors.ControlText;
			grid.LayoutKey = nameof(grid);
			grid.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			grid.Name = nameof(grid);
			grid.Size = ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			grid.TabIndex = 0;

			AutoScaleMode = AutoScaleMode.Dpi;
			CaptionRenderingEnabled = true;
			Controls.Add(grid);

			Name = nameof(DatabaseScopedConfigurationGrid);
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
