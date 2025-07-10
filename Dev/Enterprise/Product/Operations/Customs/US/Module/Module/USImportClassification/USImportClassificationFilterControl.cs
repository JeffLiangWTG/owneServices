using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public partial class USImportClassificationFilterControl : Customs.Module.CusClassificationFilterControl
	{
		public USImportClassificationFilterControl()
		{
			InitializeComponentAndUpdateScheduleBCaption();
		}

		public USImportClassificationFilterControl(IBusinessObjectCollection gridCollection, USImportClassificationFilterBusinessObject filterBusinessObject)
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
					columnInfo.Caption = "Tariff No.";
					break;
				}
			}
		}
	}
}
