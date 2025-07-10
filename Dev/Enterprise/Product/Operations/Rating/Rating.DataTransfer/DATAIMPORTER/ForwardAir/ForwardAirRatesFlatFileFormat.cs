using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Rating.DataTransfer.ForwardAir
{
	internal class ForwardAirRatesFlatFileFormat : CsvFlatFileFormat
	{
		public ForwardAirRatesFlatFileFormat(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public override FlatFileDataRow ConvertToRow(ZString rawDataRow)
		{
			var csvLine = new OCsvLine(rawDataRow, Delimiter);
			var result = new ForwardAirRatesFlatFileDataRow(csvLine.FieldValues, Factory);
			return result;
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotSupportedException("Export to CSV format is not supported");
		}
	}
}
