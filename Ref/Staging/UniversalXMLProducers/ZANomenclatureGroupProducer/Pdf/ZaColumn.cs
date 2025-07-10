using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public class ZaColumn
	{
		internal readonly string columnName;
		internal readonly float xStartCoordinate;
		internal readonly float xEndCoordinate;

		public ZaColumn(string columnName, float xStartCoordinate, float xEndCoordinate)
		{
			Argument.NotNull(columnName, nameof(columnName));
			this.columnName = columnName;
			this.xStartCoordinate = xStartCoordinate;
			this.xEndCoordinate = xEndCoordinate;
		}
	}
}
