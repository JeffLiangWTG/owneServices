using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution.ExportToExcel
{
	internal class ExcelExportColumn<T> : ExcelExportColumnBase where T : BusinessObject
	{
		public ExcelExportColumn(string propertyName, string description, int width)
		{
			PropertyName = propertyName;
			Description = description;
			Width = width;
		}

		readonly string PropertyName;

		public override DocumentEngineIntegration.CellFormat GetFormat(IZType value) => new DocumentEngineIntegration.CellFormat();

		protected override string GetDescription() => Description;

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var value = typeof(T).GetProperty(PropertyName).GetValue(bizObj);
			return (ZString)value?.ToString();
		}
	}
}
