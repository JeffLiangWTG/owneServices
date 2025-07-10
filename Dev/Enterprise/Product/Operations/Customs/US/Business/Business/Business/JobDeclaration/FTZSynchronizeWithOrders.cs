using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Warehouse.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FTZSynchronizeWithOrders : SynchronizeWithOrders
	{
		public FTZSynchronizeWithOrders(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString GetOutwardEntryNumber()
		{
			return PermitEntryLineGrouping.GetOutwardEntryNumber(declaration as JobDeclaration);
		}

		protected override ZString GetMissingWarehouseAddressMessage()
		{
			return ResString.GetMultilingualString("{05FB06B0-D52A-45A8-B2DF-F7B4AAA95DDC}", "Cannot synchronize as there is no FTZ entered on this job.");
		}

		protected override void PopulateHeaderData(BaseJobComInvoiceHeader invoice)
		{
			var invoiceAmount = ZDecimal.Zero;
			var hasMultipleUnit = false;
			var packType = ZString.Empty;
			foreach (JobComInvoiceLine invoiceLine in invoice.InvoiceLines)
			{
				invoiceAmount += invoiceLine.JI_LinePrice;
				if (!hasMultipleUnit)
				{
					var invoiceUQ = invoiceLine.JI_InvoiceUQ;
					if (!invoiceUQ.IsEmpty)
					{
						if (packType.IsEmpty)
						{
							packType = invoiceUQ;
						}
						else if (packType != invoiceUQ)
						{
							hasMultipleUnit = true;
						}
					}
				}
			}
			invoice.JZ_InvoiceAmount = invoiceAmount;
			declaration.JE_TotalNoOfPacksPackType = hasMultipleUnit ? Messaging.Business.ShippingOrPackingingUnitList.Codes.Package : packType.ToString();
		}

		protected override void DeleteExistingInvoiceData()
		{
			declaration.InvoiceLines.RemoveAndDeleteAll();
		}

		protected override BaseJobComInvoiceHeader GetInvoice()
		{
			return declaration.Invoices.Count > 0 ? declaration.Invoices[0] : declaration.Invoices.AddNew();
		}

		protected override void PopulateNewInvoiceLine(BaseJobComInvoiceHeader invoice, IFTZWhsOrderLineData whsOrderLineData)
		{
			var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(whsOrderLineData.ReceiveCustomsAddInfo);
			if (!IsDomesticLine(addInfos))
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				PopulateInvoiceLine(invoiceLine, whsOrderLineData);
			}
		}

		bool IsDomesticLine(Dictionary<ZString, ZString> addInfos)
		{
			var result = false;
			var keyZone = JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3);
			var dicValue = ZString.Empty;
			if (addInfos.TryGetValue(keyZone, out dicValue) && dicValue == ZoneStatusList.Codes.Domestic)
			{
				result = true;
			}
			return result;
		}

		protected override void PopulateInvoiceLine(BaseJobComInvoiceLine invoiceLine, IFTZWhsOrderLineData whsOrderLineData)
		{
			base.PopulateInvoiceLine(invoiceLine, whsOrderLineData);
			var usInvoiceLine = invoiceLine as JobComInvoiceLine;
			if (usInvoiceLine != null)
			{
				var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(whsOrderLineData.ReceiveCustomsAddInfo);

				var thirdUQ = ZString.Empty;
				var thirdQty = ZDecimal.Zero;

				var ratio = whsOrderLineData.TotalReceiveQuantity.IsEmpty ? 0m : whsOrderLineData.Quantity / whsOrderLineData.TotalReceiveQuantity;
				addInfos = WarehouseCustomsAddInfoWrapper.GetAddInfosDetailsAndThirdQtyAndUQ(usInvoiceLine, addInfos, ref thirdUQ, ref thirdQty);
				usInvoiceLine.GetAddInfo().LoadPropertiesFromString(AddInfoParser.Serialise(addInfos), false);
				WarehouseCustomsAddInfoWrapper.SetupQty(ratio, usInvoiceLine.JI_CustomsThirdQuantityInfo, usInvoiceLine.JI_CustomsThirdUnitQty, thirdUQ, thirdQty); // if IFTZWhsOrderLineData add thirdQty and thirdUQ, we should change this line, the ThirdQty should comes from IFTZWhsOrderLineData rather than AddInfo
				usInvoiceLine.US_ManifestQty = (ZInt)whsOrderLineData.Quantity;
			}
		}
	}
}
