using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public partial class USExportClassificationFilterControl : Customs.Module.CusClassificationFilterControl
	{
		public USExportClassificationFilterControl()
		{
			InitializeComponentAndUpdateScheduleBCaption();
		}

		public USExportClassificationFilterControl(IBusinessObjectCollection gridCollection, USExportClassificationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponentAndUpdateScheduleBCaption();
		}

		void InitializeComponentAndUpdateScheduleBCaption()
		{
			InitializeComponent();
			foreach (Core.Forms.ZGridColumnInfo columnInfo in FilteredGrid.ColumnStyles)
			{
				if (columnInfo.ColumnName == CusClassificationSchema.CC_TariffNum.Name)
				{
					columnInfo.ColumnName = "CC_FormattedTariffNum";
					columnInfo.Caption = "Schedule B";
					break;
				}
			}
		}
	}
}
