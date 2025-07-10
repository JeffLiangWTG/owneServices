using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Edifact.D96B.Segments;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class InvoiceInformationDocWrapper : NonPersistentBusinessObject, IInvoiceInformation
	{
		public InvoiceInformationDocWrapper(SegmentGroup5 input)
		{
			if (input != null)
			{
				InvoiceNumber = input.DOC[0].DocumentMessageDetails.DocumentMessageNumber;
				var invoiceDate = ZDateTime.Empty;
				var dateString = input.DTM.Cast<DTMSegment>().FirstOrDefault(dtm => dtm.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.InvoiceDateTime)?.DateTimePeriod.DateTimePeriod ?? ZString.Empty;
				ZDateTime.TryParseExact(dateString, out invoiceDate, Constants.DateFormatCCYYMMDD);
				InvoiceDate = invoiceDate;
			}
		}

		public InvoiceInformationDocWrapper(SegmentGroup10 input)
		{
			if (input != null)
			{
				InvoiceNumber = input.DMS[0].DocumentMessageNumber;
				var invoiceDate = ZDateTime.Empty;
				var dateString = input.DTM.Cast<DTMSegment>().FirstOrDefault(dtm => dtm.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.InvoiceDateTime)?.DateTimePeriod.DateTimePeriod ?? ZString.Empty;
				ZDateTime.TryParseExact(dateString, out invoiceDate, Constants.DateFormatCCYYMMDD);
				InvoiceDate = invoiceDate;
			}
		}

		public InvoiceInformationDocWrapper(IInvoiceInformation input)
		{
			if (input != null)
			{
				InvoiceDate = input.InvoiceDate;
				InvoiceNumber = input.InvoiceNumber;
			}
		}

		public ZDateTime InvoiceDate { get; private set; }

		public ZString InvoiceNumber { get; private set; }
	}
}
