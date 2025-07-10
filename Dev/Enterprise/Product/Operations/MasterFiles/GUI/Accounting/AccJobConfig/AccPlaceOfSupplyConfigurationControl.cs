using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccPlaceOfSupplyConfigurationControl : ZUserControl
	{
		public AccPlaceOfSupplyConfigurationControl()
		{
			InitializeComponent();

			SetBranchColumnVisibility();
		}

		void SetBranchColumnVisibility()
		{
			if (DesignModeFinder.IsDesigning)
			{
				return;
			}

			removeColumn(!AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value, AccPOSConfiguration.Schema.PSC_NK_Branch);
			removeColumn(!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value, AccPOSConfiguration.Schema.PSC_SupplyType);

			void removeColumn(bool remove, string columnName)
			{
				if (remove)
				{
					var columnToRemove = PlaceOfSupplyConfigurationGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
					if (columnToRemove != null)
					{
						PlaceOfSupplyConfigurationGrid.ColumnStyles.Remove(columnToRemove);
					}
				}
			}
		}
	}
}
