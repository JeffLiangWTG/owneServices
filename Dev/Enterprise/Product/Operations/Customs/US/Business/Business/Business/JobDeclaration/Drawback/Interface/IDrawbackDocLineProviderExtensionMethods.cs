using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class IDrawback7551ImportDocLineProviderExtensionMethods
	{
		public static ZBool ImportLineEqualsToInvoiceLine(this IDrawback7551ImportDocLine docLine, IDrawback7551ImportDocLine invoiceLine)
		{
			return docLine.DrawbackImportEntryNumber == invoiceLine.DrawbackImportEntryNumber
				   && docLine.DrawbackPortCode == invoiceLine.DrawbackPortCode
				   && docLine.DrawbackImportDate == invoiceLine.DrawbackImportDate
				   && docLine.DrawbackCDIndicator == invoiceLine.DrawbackCDIndicator
				   && docLine.DrawbackReceivedDates == invoiceLine.DrawbackReceivedDates
				   && docLine.DrawbackUsedDates == invoiceLine.DrawbackUsedDates
				   && docLine.DrawbackHTSUSNo == invoiceLine.DrawbackHTSUSNo
				   && docLine.DrawbackDocDescription == invoiceLine.DrawbackDocDescription
				   && docLine.DrawbackBox29SectionIIUQ == invoiceLine.DrawbackBox29SectionIIUQ
				   && docLine.DrawbackEnteredValuePer == invoiceLine.DrawbackEnteredValuePer
				   && docLine.DrawbackDutyRate == invoiceLine.DrawbackDutyRate;
		}
	}

	public static class IDrawbackExportDocLineProviderExtensionMethods
	{
		public static ZBool ExportLineEqualsToInvoiceLine(this IDrawback7551ExportDocLine docLine, IDrawback7551ExportDocLine invoiceLine)
		{
			return docLine.DrawbackExportDate == invoiceLine.DrawbackExportDate
				   && docLine.DrawbackExportAction == invoiceLine.DrawbackExportAction
				   && docLine.DrawbackExportID == invoiceLine.DrawbackExportID
				   && docLine.DrawbackExporterName == invoiceLine.DrawbackExporterName
				   && docLine.DrawbackExpDocDescription == invoiceLine.DrawbackExpDocDescription
				   && docLine.DrawbackExportDest == invoiceLine.DrawbackExportDest
				   && docLine.DrawbackExportTariff == invoiceLine.DrawbackExportTariff
				   && docLine.DrawbackExportUQ == invoiceLine.DrawbackExportUQ;
		}
	}
}
