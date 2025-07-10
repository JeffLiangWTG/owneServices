using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	class InvoiceDataTransfer
	{
		internal static void BookingWithQuoteToShipment(ZGuid bookingPK, ZGuid shipmentPK, BusinessObjectFactory factory)
		{
			if (!ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled
				|| bookingPK.IsEmpty
				|| factory.Exists(typeof(GlobalCommercialInvoiceHeader), new ZQuery(GlobalCommercialInvoiceHeaderSchema.GIH_ParentID, shipmentPK)))
			{
				return;
			}

			var bookingInvoiceHeaders = factory.Load<GlobalCommercialInvoiceHeader>(new ZQuery(GlobalCommercialInvoiceHeaderSchema.GIH_ParentID, bookingPK)).ToList();

			if (bookingInvoiceHeaders.Count > 0)
			{
				var bookingInvoiceLines = factory.Load<GlobalCommercialInvoiceLine>(new ZQuery(GlobalCommercialInvoiceLineSchema.GIL_GIH_Header,
					bookingInvoiceHeaders.Select(u => u.PK))).GroupBy(u => u.GIL_GIH_Header).ToDictionary(u => u.Key);

				bookingInvoiceHeaders.ForEach(header =>
				{
					var targetHeader = CopyInvoiceHeader(factory, shipmentPK, JobShipmentSchema.Constants.Prefix, header);

					if (bookingInvoiceLines.TryGetValue(header.PK, out var invoicingLinesFromCurrentHeader))
					{
						CopyInvoiceLine(factory, invoicingLinesFromCurrentHeader.ToList(), targetHeader);
					}
				});
			}
		}

		static GlobalCommercialInvoiceHeader CopyInvoiceHeader(BusinessObjectFactory factory, ZGuid parentID, string parentTableCode, GlobalCommercialInvoiceHeader sourceHeader)
		{
			var targetHeader = factory.New<GlobalCommercialInvoiceHeader>();
			targetHeader.CopyPersistentValuesFrom(sourceHeader, new BusinessObjectCloneArgs(ExcludedInvoiceHeaderProperties));
			targetHeader.GIH_ParentID = parentID;
			targetHeader.GIH_ParentTableCode = parentTableCode;

			return targetHeader;
		}

		static void CopyInvoiceLine(BusinessObjectFactory factory, List<GlobalCommercialInvoiceLine> bookingInvoiceLines, GlobalCommercialInvoiceHeader targetHeader)
		{
			bookingInvoiceLines.ForEach(sourceLine =>
			{
				var targetLine = factory.New<GlobalCommercialInvoiceLine>();
				targetLine.CopyPersistentValuesFrom(sourceLine, new BusinessObjectCloneArgs(ExcludedInvoiceLineProperties));
				targetLine.Headers = new GlobalCommercialInvoiceHeaderCollection(factory, targetHeader.GIH_ParentID, targetHeader.GIH_ParentTableCode);
				targetLine.GIL_GIH_Header = targetHeader.PK;
			});
		}

		internal static IEnumerable<string> ExcludedInvoiceHeaderProperties => new[]
		{
			GlobalCommercialInvoiceHeaderSchema.Constants.GIH_SystemCreateTimeUtc,
			GlobalCommercialInvoiceHeaderSchema.Constants.GIH_SystemCreateUser,
			GlobalCommercialInvoiceHeaderSchema.Constants.GIH_SystemLastEditTimeUtc,
			GlobalCommercialInvoiceHeaderSchema.Constants.GIH_SystemLastEditUser,
		};

		internal static IEnumerable<string> ExcludedInvoiceLineProperties => new[]
		{
			GlobalCommercialInvoiceLineSchema.Constants.GIL_SystemCreateTimeUtc,
			GlobalCommercialInvoiceLineSchema.Constants.GIL_SystemCreateUser,
			GlobalCommercialInvoiceLineSchema.Constants.GIL_SystemLastEditTimeUtc,
			GlobalCommercialInvoiceLineSchema.Constants.GIL_SystemLastEditUser,
		};
	}
}
