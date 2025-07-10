using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.SADH
{
	public class JobDeclarationWriter
	{
		public JobDeclarationWriter(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly BaseJobDeclaration declaration;

		public virtual void WriteFrom(SADHFormData formData)
		{
			declaration.IsImportingData = true;

			InvoiceHeaderActiveCollection invoices = declaration.Invoices;
			BaseJobComInvoiceHeader invoice = invoices.Count > 0 ? invoices[0] : invoices.AddNew();

			BaseJobComInvoiceLineViewCollection invoiceLines = invoice.JobComInvoiceLines;
			BaseJobComInvoiceLine invoiceLine = invoiceLines.Count > 0 ? invoiceLines.GetByLineNo(1) : invoiceLines.AddNew();

			WriteDeclarationFields(formData);
			WriteInvoiceHeaderFields(formData, invoice);
			WriteInvoiceLineFields(formData, invoiceLine);

			declaration.IsImportingData = false;
		}

		protected virtual void WriteDeclarationFields(SADHFormData formData)
		{
			//Form Message Type (import/export)
			declaration.JE_MessageType = formData.D1_MessageType.Left(declaration.JE_MessageTypeInfo.MaxLength);
			//Box 2 Consignor
			if (formData.D1_OH_Consignor.IsValid)
			{
				declaration.JE_OH_Supplier = formData.D1_OH_Consignor;
				declaration.SupplierDocumentaryAddress.E2_OA_Address = formData.Consignor.MainAddress.PK;
			}
			//Box 6 Total Packages
			declaration.JE_TotalNoOfPacks = formData.D1_TotalPackages;
			declaration.JE_TotalNoOfPacksPackType = formData.D1_TotalPackagesPackType.Left(declaration.JE_TotalNoOfPacksPackTypeInfo.MaxLength);
			//Box 8 Consignee
			if (formData.D1_OH_Consignee.IsValid)
			{
				declaration.JE_OH_Importer = formData.D1_OH_Consignee;
				declaration.ImporterDocumentaryAddress.E2_OA_Address = formData.Consignee.MainAddress.PK;
			}
			//Box 15 Country of dispatch/export
			declaration.JE_RL_NKPortOfLoading = formData.D1_RL_NKCountryOfDispatchOrExport.Left(declaration.JE_RL_NKPortOfLoadingInfo.MaxLength);
			//Box 16 Country of origin
			declaration.JE_RL_NKOrigin = formData.D1_RL_NKCountryOfOrigin.Left(declaration.JE_RL_NKOriginInfo.MaxLength);
			//Box 17b Country of destination Code
			declaration.JE_RL_NKFinalDestination = formData.D1_RL_NKCountryOfDestination.Left(declaration.JE_RL_NKFinalDestinationInfo.MaxLength);
			//Box 20 Delivery Terms
			declaration.JE_ShipmentIncoTerm = formData.D1_DeliveryTerms.Left(declaration.JE_ShipmentIncoTermInfo.MaxLength);
			//Box 25 Mode of transport at the border
			declaration.JE_TransportMode = formData.D1_ModeOfTransportAtTheBorder.Left(declaration.JE_TransportModeInfo.MaxLength);
			// Box 21 Identity and nationality of active means of transport crossing the border
			declaration.JE_VoyageFlightNo = formData.D1_FlightNo.Left(declaration.JE_VoyageFlightNoInfo.MaxLength);
			if (formData.D1_FlightDate.IsValid)
			{
				declaration.JE_ExportDate = formData.D1_FlightDate;
			}

			declaration.JE_VesselName = formData.D1_VesselCode.Left(declaration.JE_VesselNameInfo.MaxLength);
			//Box 27 Place of unloading
			declaration.JE_RL_NKPortOfArrival = formData.D1_RL_NKPlaceOfUnloading.Left(declaration.JE_RL_NKPortOfArrivalInfo.MaxLength);
		}

		protected virtual void WriteInvoiceHeaderFields(SADHFormData formData, BaseJobComInvoiceHeader invoice)
		{
			//Box 20 Delivery Terms
			invoice.JZ_IncoTerm = formData.D1_DeliveryTerms;
			//Box 22 Currency and total amount invoiced
			invoice.JZ_InvoiceAmount = formData.D1_InvoiceTotalAmount;
			if (formData.D1_RX_InvoiceCurrency.IsValid)
			{
				RefCurrency currency = formData.Factory.Load<RefCurrency>(formData.D1_RX_InvoiceCurrency);
				if (currency != null)
				{
					invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
				}
				else
				{
					invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
				}
			}
		}

		protected virtual void WriteInvoiceLineFields(SADHFormData formData, BaseJobComInvoiceLine line)
		{
			//Box 31 Packages and description of goods
			line.JI_Description = formData.D1_DescriptionOfGoods;
			declaration.JE_MarksAndNumbers = formData.D1_MarksAndNumbers;
			line.JI_InvoiceUQ = formData.D1_ItemUQ;
			line.JI_InvoiceQuantity = formData.D1_ItemQty;
			//Box 33 Commodity Code
			line.JI_Tariff = formData.D1_CommodityCode.Left(line.JI_TariffInfo.MaxLength);
			//Box 34 Goods Country origin Code
			line.JI_CountryOfOrigin = formData.D1_RN_NKItemCountryOfOrigin;
			//Box 35 Gross mass (kg)
			line.JI_WeightUQ = formData.D1_GrossMassUQ;
			line.JI_Weight = formData.D1_GrossMass;
			//Box 41 Supplementary Units
			line.JI_CustomsUnitQty = formData.D1_SupplementaryUQ;
			line.JI_CustomsQuantity = formData.D1_SupplementaryQty;
			//Box 42 Item Price
			line.JI_LinePrice = formData.D1_ItemPrice;
		}
	}
}
