using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class FTZMessageSplitBillObjectWrapper : IFTZBill
	{
		public FTZMessageSplitBillObjectWrapper(ITAndSplitDetails splitDetails)
		{
			this.splitDetails = splitDetails;
			this.ftzBill = Argument.NotNull(splitDetails.Bill, nameof(ftzBill));
		}
		readonly ITAndSplitDetails splitDetails;

		public FTZMessageSplitBillObjectWrapper(Bill splitBillWithoutShipmentDetailsSelected)
		{
			this.ftzBill = Argument.NotNull(splitBillWithoutShipmentDetailsSelected, nameof(ftzBill));
		}
		readonly IFTZBill ftzBill;

		ZString IFTZBill.BillOfLading => ftzBill.BillOfLading;

		ZString IFTZBill.HouseBill => ftzBill.HouseBill;

		ZString IFTZBill.CountryOfExport => ftzBill.CountryOfExport;

		ZString IFTZBill.ForeignLoadPort => ftzBill.ForeignLoadPort;

		IEnumerable<IFTZLine> IFTZBill.Lines
		{
			get
			{
				if (fFTZBillLinesList == null)
				{
					fFTZBillLinesList = new List<IFTZLine>();

					if (splitDetails != null)
					{
						foreach (CusEntryLine entryLine in ftzBill.Lines)
						{
							if (entryLine.RandomLine is JobComInvoiceLine invoiceLine && invoiceLine.InvoiceHeader is JobComInvoiceHeader invoice)
							{
								if (invoice.US_SplitShipmentDetail.EqualsIgnoringCase(string.Join("/", splitDetails.US_CarrierCode, splitDetails.US_FlightNumber, splitDetails.US_ArrivalDate.ToShortDateString())))
								{
									fFTZBillLinesList.Add(entryLine);
								}
							}
						}
					}
					else
					{
						foreach (CusEntryLine entryLine in ftzBill.Lines)
						{
							if (entryLine.RandomLine is JobComInvoiceLine invoiceLine && invoiceLine.InvoiceHeader is JobComInvoiceHeader invoice)
							{
								if (invoice.US_SplitShipmentDetail.IsEmpty)
								{
									fFTZBillLinesList.Add(entryLine);
								}
							}
						}
					}
				}

				return fFTZBillLinesList;
			}
		}
		List<IFTZLine> fFTZBillLinesList;

		ZDecimal IFTZBillCommon.Quantity
		{
			get
			{
				if (splitDetails != null)
				{
					return (ZDecimal)splitDetails.US_NoOfPacks;
				}
				return ftzBill.Quantity;
			}
		}

		ZString IFTZBillCommon.FIRMSCode => ftzBill.FIRMSCode;

		IEnumerable<IITNumber> IFTZBillCommon.ITNumbers
		{
			get
			{
				if (splitDetails != null)
				{
					return !splitDetails.US_ITNumber.IsEmpty ? new[] { splitDetails } : Enumerable.Empty<IITNumber>();
				}

				return ftzBill.ITNumbers;
			}
		}

		ZString IFTZBillCommon.IRSIdentifier => ftzBill.IRSIdentifier;

		IEnumerable<IContainer> IFTZBillCommon.Containers => ftzBill.Containers;

		ZString IFTZBillCommon.CU_BillNum => ftzBill.CU_BillNum;

		ZString IFTZBillCommon.CU_PackType => ftzBill.CU_PackType;

		ZDecimal IFTZBillCommon.CU_NoOfPacks
		{
			get
			{
				if (splitDetails != null)
				{
					return (ZDecimal)splitDetails.US_NoOfPacks;
				}

				return ftzBill.CU_NoOfPacks;
			}
		}

		IEnumerable<IFTZITAndSplitDetail> IFTZBillCommon.ITAndSplitDetails
		{
			get
			{
				if (splitDetails != null)
				{
					return new[] { splitDetails };
				}

				return ftzBill.ITAndSplitDetails;
			}
		}

		ZDecimal IFTZConcurrence.FTZConcurrenceQty
		{
			get => splitDetails?.US_FTZConcurrenceQty ?? ftzBill.FTZConcurrenceQty;
			set
			{
				if (splitDetails != null)
				{
					splitDetails.US_FTZConcurrenceQty = value;
				}
				else
				{
					ftzBill.FTZConcurrenceQty = value;
				}
			}
		}

		BusinessObjectFactory IFTZConcurrence.Factory => ftzBill.Factory;
	}
}
