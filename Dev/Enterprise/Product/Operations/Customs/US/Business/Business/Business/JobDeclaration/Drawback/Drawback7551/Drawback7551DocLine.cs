using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class Drawback7551DocLine : DrawbackDocLine, IObsoleteValidation, IDrawback7551ImportDocLine, IDrawback7551ExportDocLine
	{
		public Drawback7551DocLine(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
			if (invoiceLine != null)
			{
				CMorImportEntryNo = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackImportEntryNumber;
				US_DRWPort = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackPortCode;
				US_DRWEntryDate = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackImportDate;
				US_DRWExportDateFormatted = US_DRWExportDate.ToString("MM/dd/yyyy", CultureInfo.CurrentCulture);
				CDIndicator = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackCDIndicator;
				ReceivedDates = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackReceivedDates;
				UsedDates = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackUsedDates;
				JI_Tariff = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackHTSUSNo;
				US_DRWValuePer = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackEnteredValuePer;
				US_DRWDutyRateInPercentage = !invoiceLine.DutyRate.IsEmpty ? ((invoiceLine.DutyRate * 100).ToString("0.000", CultureInfo.CurrentCulture) + " %") : string.Empty;
				US_DRWDutyDesc = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackDutyRate;
				US_DRWClaimDuty = invoiceLine.DrawbackClaimDuty.ToString(2);
				DrawbackClaimDuty = invoiceLine.DrawbackClaimDuty;
				US_DRWClaimTax = invoiceLine.DrawbackClaimTax.ToString(2);
				DrawbackClaimTax = invoiceLine.DrawbackClaimTax;
				US_DRWCDUse = invoiceLine.US_DRWCDUse;
				DeliveredDates = SetDeliveredDates(invoiceLine);
				US_DRWCDIndicator = invoiceLine.US_DRWCDInd ? "Y" : "";
				ImportQtyAndUnit = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackImportQtyAndUnit;
				ImportQty = invoiceLine.DRWImportQuantity;
				ImportUQ = ((IDrawback7551ImportDocLine)invoiceLine).DrawbackImportUQ;
				ExportQtyAndUnit = ((IDrawback7551DocLine)invoiceLine).DrawbackExportQtyAndUnit;
				ExportQty = invoiceLine.DRWExportQuantity;
				ExportUQ = ((IDrawback7551DocLine)invoiceLine).DrawbackExportUQ;
				ShouldShowSubTotals = invoiceLine.Declaration != null && invoiceLine.Declaration.US_DRWPrintSubTotals;

				DateOfManufactureFormatted = invoiceLine.US_DRWDateOfManufacture.ToString("MM/dd/yyyy", CultureInfo.CurrentCulture);
				DescrManufactured = invoiceLine.US_DRWDescrManufactured;

				var manufacturedQtyAndUQ = ((IDrawback7551ManufDocLine)invoiceLine).DrawbackManufQtyAndUQ;
				ManufacturedQtyAndDescrOfMerchandiseUsed = manufacturedQtyAndUQ + " " + invoiceLine.US_DRWDescrUsed;
				ManufacturedLocation = invoiceLine.US_DRWFactoryLocation;

				Box29SectionIIQtyAndUQ = invoiceLine.US_DRWQuantityUsed > 0 ? manufacturedQtyAndUQ : ExportQtyAndUnit;
				Box29SectionIIQty = invoiceLine.US_DRWQuantityUsed > 0 ? invoiceLine.US_DRWQuantityUsed : ExportQty;
				Box29SectionIIUQ = invoiceLine.US_DRWQuantityUsed > 0 ? invoiceLine.US_DRWUQUsed : ExportUQ;
			}
		}

		public ZBool ShouldShowSubTotals { get; private set; }

		public ZDecimal DrawbackClaimDuty { get; private set; }

		public ZDecimal DrawbackClaimTax { get; private set; }

		public ZString CMorImportEntryNo { get; private set; }

		public ZString US_DRWPort { get; private set; }

		public ZString US_DRWEntryDate { get; private set; }

		public ZString US_DRWExportDateFormatted { get; private set; }

		public ZString CDIndicator { get; private set; }

		public ZString ReceivedDates { get; private set; }

		public ZString UsedDates { get; private set; }

		public ZString JI_Tariff { get; private set; }

		public ZString US_DRWValuePer { get; private set; }

		public ZString US_DRWDutyRateInPercentage { get; private set; }

		public ZString US_DRWDutyDesc { get; private set; }

		public ZString US_DRWClaimDuty { get; private set; }

		public ZString US_DRWClaimTax { get; private set; }

		public ZString US_DRWCDUse { get; private set; }

		public ZString US_DRWCDIndicator { get; private set; }

		public ZString DeliveredDates { get; private set; }

		public ZString ImportQtyAndUnit { get; private set; }

		public ZDecimal ImportQty { get; private set; }

		public ZString ImportUQ { get; private set; }

		public ZString ExportQtyAndUnit { get; private set; }

		public ZDecimal ExportQty { get; private set; }

		public ZString ExportUQ { get; private set; }

		public ZString DateOfManufactureFormatted { get; private set; }

		public ZString DescrManufactured { get; private set; }

		public ZString ManufacturedQtyAndDescrOfMerchandiseUsed { get; private set; }

		public ZString ManufacturedLocation { get; private set; }

		public ZString Box29SectionIIQtyAndUQ { get; private set; }

		public ZDecimal Box29SectionIIQty { get; private set; }

		public ZString Box29SectionIIUQ { get; private set; }

		public ZString SetDeliveredDates(JobComInvoiceLine invoiceLine)
		{
			ZString result = ZString.Empty;
			if (!invoiceLine.US_DRWDateDelFrom.IsEmpty)
			{
				result = invoiceLine.US_DRWDateDelFrom.ToString("MMddyy");
				if (!invoiceLine.US_DRWDateDelTo.IsEmpty)
				{
					result += "/" + invoiceLine.US_DRWDateDelTo.ToString("MMddyy");
				}
			}
			return result;
		}

		public void UpdateImportDocLineDetails(JobComInvoiceLine invoiceLine)
		{
			var oldClaimDuty = ZDecimal.ParseSafe(US_DRWClaimDuty, ZDecimal.Zero);
			US_DRWClaimDuty = ((ZDecimal)(oldClaimDuty + invoiceLine.DrawbackClaimDuty)).ToString(2);
			DrawbackClaimDuty += invoiceLine.DrawbackClaimDuty;

			var oldClaimTax = ZDecimal.ParseSafe(US_DRWClaimTax, ZDecimal.Zero);
			US_DRWClaimTax = ((ZDecimal)(oldClaimTax + invoiceLine.DrawbackClaimTax)).ToString(2);
			DrawbackClaimTax += invoiceLine.DrawbackClaimTax;

			UpdateExportDocLineDetails(invoiceLine);
		}

		public void UpdateExportDocLineDetails(JobComInvoiceLine invoiceLine)
		{
			Box29SectionIIQty += invoiceLine.US_DRWQuantityUsed > 0 ? invoiceLine.US_DRWQuantityUsed : invoiceLine.DRWExportQuantity;
			Box29SectionIIQtyAndUQ = new ZStringBuilder().Append(Box29SectionIIQty.ToString("0.#####", CultureInfo.CurrentCulture)).Append(Box29SectionIIUQ).ToStringWithDelimiterBetweenAppends(" ");

			ExportQty += invoiceLine.DRWExportQuantity;
			ExportQtyAndUnit = new ZStringBuilder().Append(ExportQty.ToString("#.#####", CultureInfo.CurrentCulture)).Append(invoiceLine.DRWExportUQ).ToStringWithDelimiterBetweenAppends(" ");
		}

		#region IDrawback7551DocLine Members

		ZString IDrawback7551ImportDocLine.DrawbackImportEntryNumber
		{
			get { return CMorImportEntryNo; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackPortCode
		{
			get { return US_DRWPort; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackImportDate
		{
			get { return US_DRWEntryDate; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackCDIndicator
		{
			get { return CDIndicator; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackReceivedDates
		{
			get { return ReceivedDates; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackUsedDates
		{
			get { return UsedDates; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackHTSUSNo
		{
			get { return JI_Tariff; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackDocDescription
		{
			get { return DrawbackDocDescription; }
		}

		ZString IDrawback7551DocLine.DrawbackExportQtyAndUnit
		{
			get { return ExportQtyAndUnit; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackImportQtyAndUnit
		{
			get { return ImportQtyAndUnit; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackImportUQ
		{
			get { return ImportUQ; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackEnteredValuePer
		{
			get { return US_DRWValuePer; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackDutyRate
		{
			get { return US_DRWDutyDesc; }
		}

		ZString IDrawback7551DocLine.DrawbackExportUQ
		{
			get { return ExportUQ; }
		}

		ZString IDrawback7551ManufDocLine.DrawbackManufQtyAndUQ
		{
			get { return Box29SectionIIQtyAndUQ; }
		}

		ZString IDrawback7551ImportDocLine.DrawbackBox29SectionIIUQ
		{
			get { return Box29SectionIIUQ; }
		}

		#endregion
	}
}
