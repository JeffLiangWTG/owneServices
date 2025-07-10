using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IDrawback7551DocLine
	{
		ZString DrawbackExportQtyAndUnit { get; }
		ZString DrawbackExportUQ { get; }
	}

	public interface IDrawback7551ManufDocLine
	{
		ZString DrawbackManufQtyAndUQ { get; }
	}

	public interface IDrawback7551ImportDocLine : IDrawback7551DocLine, IDrawback7551ManufDocLine
	{
		ZString DrawbackImportEntryNumber { get; }
		ZString DrawbackPortCode { get; }
		ZString DrawbackImportDate { get; }
		ZString DrawbackCDIndicator { get; }
		ZString DrawbackReceivedDates { get; }
		ZString DrawbackUsedDates { get; }
		ZString DrawbackHTSUSNo { get; }
		ZString DrawbackDocDescription { get; }
		ZString DrawbackImportQtyAndUnit { get; }
		ZString DrawbackImportUQ { get; }
		ZString DrawbackEnteredValuePer { get; }
		ZString DrawbackDutyRate { get; }
		ZString DrawbackBox29SectionIIUQ { get; }
	}

	public interface IDrawbackExportDocLine
	{
		ZDateTime DrawbackExportDate { get; }
		ZString DrawbackExportAction { get; }
		ZString DrawbackExportID { get; }
		ZString DrawbackExporterName { get; }
		ZString DrawbackExpDocDescription { get; }
		ZString DrawbackExportDest { get; }
		ZString DrawbackExportTariff { get; }
	}

	public interface IDrawback7551ExportDocLine : IDrawbackExportDocLine, IDrawback7551DocLine
	{
	}
}
