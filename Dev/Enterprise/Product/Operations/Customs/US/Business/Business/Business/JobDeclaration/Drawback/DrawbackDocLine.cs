using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class DrawbackDocLine : NonPersistentBusinessObject, IDrawbackExportDocLine
	{
		public DrawbackDocLine(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			FormattedImportEntryNo = invoiceLine.GetFormattedImportEntryNoWithLineItemNo();
			US_DRWExportDate = ((IDrawbackExportDocLine)invoiceLine).DrawbackExportDate;
			US_DRWExportID = ((IDrawbackExportDocLine)invoiceLine).DrawbackExportID;
			US_DRWExportDest = ((IDrawbackExportDocLine)invoiceLine).DrawbackExportDest;
			DrawbackDocDescription = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackDocDescription;
			DrawbackExpDocDescription = ((IDrawbackExportDocLine)invoiceLine).DrawbackExpDocDescription;
			ExportQtyAndUnits = new ZString(invoiceLine.DRWExportQuantity.ToString("#.#####") + " " + invoiceLine.DRWExportUQ);
			US_ExportTariff = ((IDrawbackExportDocLine)invoiceLine).DrawbackExportTariff;
			US_DRWExportAction = ((IDrawbackExportDocLine)invoiceLine).DrawbackExportAction;
			US_DRWExporterName = ((IDrawbackExportDocLine)invoiceLine).DrawbackExporterName;
			ConstantOne = 1;// used on 7551 document to count lines in a group
		}

		public ZInt ConstantOne { get; private set; }
		public ZString US_DRWExporterName { get; private set; }
		public ZString FormattedImportEntryNo { get; private set; }
		public ZDateTime US_DRWExportDate { get; private set; }
		public ZString US_DRWExportID { get; private set; }
		public ZString US_DRWExportDest { get; private set; }
		public ZString DrawbackDocDescription { get; private set; }
		public ZString DrawbackExpDocDescription { get; private set; }
		public ZString ExportQtyAndUnits { get; private set; }
		public ZString US_ExportTariff { get; private set; }
		public ZString US_DRWExportAction { get; private set; }

		#region IDrawback7551ExportDocLine Members

		ZDateTime IDrawbackExportDocLine.DrawbackExportDate
		{
			get { return US_DRWExportDate; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportAction
		{
			get { return US_DRWExportAction; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportID
		{
			get { return US_DRWExportID; }
		}

		ZString IDrawbackExportDocLine.DrawbackExporterName
		{
			get { return US_DRWExporterName; }
		}

		ZString IDrawbackExportDocLine.DrawbackExpDocDescription
		{
			get { return DrawbackExpDocDescription; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportDest
		{
			get { return US_DRWExportDest; }
		}

		ZString IDrawbackExportDocLine.DrawbackExportTariff
		{
			get { return US_ExportTariff; }
		}

		#endregion
	}
}
