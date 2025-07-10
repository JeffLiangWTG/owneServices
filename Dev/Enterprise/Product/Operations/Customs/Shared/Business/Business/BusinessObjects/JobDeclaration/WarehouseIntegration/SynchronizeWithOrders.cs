using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.Business
{
	public abstract class SynchronizeWithOrders
	{
		protected SynchronizeWithOrders(BaseJobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public ZString Synchronize()
		{
			var result = ZString.Empty;
			var outwardEntryNumber = GetOutwardEntryNumber();
			if (!outwardEntryNumber.IsEmpty)
			{
				var importer = GetImporter();
				if (importer != null && importer.OH_IsWarehouseClient)
				{
					var warehouseAddress = GetWarehouseAddress();
					if (warehouseAddress != null)
					{
						var whsOrderLinesData = GetWhsOrderLinesData(importer, warehouseAddress, outwardEntryNumber);
						if (whsOrderLinesData.Length > 0)
						{
							DeleteExistingInvoiceData();
							CreateNewInvoiceLineByOrders(whsOrderLinesData);
						}
						else
						{
							result = ResString.GetMultilingualString("{8D68594B-8B13-40CF-8C34-D50E8C11DC22}", "No orders found to synchronize.");
						}
					}
					else
					{
						result = GetMissingWarehouseAddressMessage();
					}
				}
				else
				{
					result = GetValidateImporterMessage();
				}
			}
			else
			{
				result = GetMissingEntryNumber();
			}

			return result;
		}

		IFTZWhsOrderLineData[] GetWhsOrderLinesData(OrgHeader importer, OrgAddress warehouseAddress, ZString entryNumber)
		{
			var result = ObjectFactory.Get<IFTZWhsOrderLinesDataProvider>().GetFTZWhsOrderLinesData(importer, warehouseAddress, entryNumber);
			return result == null ? Array.Empty<IFTZWhsOrderLineData>() : result.ToArray();
		}

		protected virtual OrgAddress GetWarehouseAddress()
		{
			return declaration.WarehouseAddress;
		}

		protected virtual OrgHeader GetImporter()
		{
			return declaration.WarehouseClient;
		}

		protected virtual ZString GetMissingWarehouseAddressMessage()
		{
			return ResString.GetMultilingualString("BED92448-94FB-4945-9671-8E5EBD180A31", "Cannot synchronize as there is no Bonded Warehouse entered on this job.");
		}

		protected ZString GetValidateImporterMessage()
		{
			return ResString.GetMultilingualString("8F87DE9B-7AFD-446A-908A-F29B7D1179D9", "Cannot synchronize as there is no Bonded Warehouse Client entered on this job.");
		}

		protected ZString GetMissingEntryNumber()
		{
			return ResString.GetMultilingualString("B8C5F460-B5D5-416A-8A2D-CF1BAA699E22", "Cannot synchronize as there is no Entry Number on this job.");
		}

		readonly protected BaseJobDeclaration declaration;

		protected abstract ZString GetOutwardEntryNumber();

		protected virtual void DeleteExistingInvoiceData()
		{
			declaration.Invoices.DeleteAll();
			declaration.InvoiceLines.RemoveAndDeleteAll();
		}

		void CreateNewInvoiceLineByOrders(IFTZWhsOrderLineData[] whsOrderLinesData)
		{
			var invoice = GetInvoice();
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			foreach (var whsOrderLineData in whsOrderLinesData)
			{
				PopulateNewInvoiceLine(invoice, whsOrderLineData);
			}
			PopulateHeaderData(invoice);
		}

		protected virtual BaseJobComInvoiceHeader GetInvoice()
		{
			return declaration.Invoices.AddNew();
		}

		protected virtual void PopulateNewInvoiceLine(BaseJobComInvoiceHeader invoice, IFTZWhsOrderLineData whsOrderLineData)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			PopulateInvoiceLine(invoiceLine, whsOrderLineData);
		}

		protected virtual void PopulateHeaderData(BaseJobComInvoiceHeader invoice)
		{
			invoice.JZ_InvoiceAmount = invoice.InvoiceLines.Cast<BaseJobComInvoiceLine>().Sum(x => x.JI_LinePrice);
		}

		protected virtual void PopulateInvoiceLine(BaseJobComInvoiceLine invoiceLine, IFTZWhsOrderLineData whsOrderLineData)
		{
			invoiceLine.JI_PartNo = whsOrderLineData.ProductCode;
			invoiceLine.JI_Tariff = whsOrderLineData.Tariff;
			invoiceLine.JI_CountryOfOrigin = whsOrderLineData.CountryOfOriginCode;
			invoiceLine.JI_InvoiceQuantity = whsOrderLineData.Quantity;
			invoiceLine.JI_InvoiceUQ = whsOrderLineData.QuantityUnit;
			var ratio = whsOrderLineData.TotalReceiveQuantity.IsEmpty ? 0m : whsOrderLineData.Quantity / whsOrderLineData.TotalReceiveQuantity;
			invoiceLine.JI_LinePrice = ratio * whsOrderLineData.TotalReceiveValueForDuty;
			if (invoiceLine.JI_CustomsUnitQty == whsOrderLineData.ReceiveCustomsQtyUnit)
			{
				invoiceLine.JI_CustomsQuantity = ratio * whsOrderLineData.TotalReceiveCustomsQty;
			}

			if (invoiceLine.JI_CustomsSecondUnitQty == whsOrderLineData.ReceiveCustomsSecondQtyUnit)
			{
				invoiceLine.JI_CustomsSecondQuantity = ratio * whsOrderLineData.TotalReceiveCustomsSecondQty;
			}
		}
	}
}
