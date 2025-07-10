using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.SADH
{
	public class SADHFormDataReader
	{
		public SADHFormDataReader(SADHFormData formData)
		{
			this.formData = formData;
		}
		readonly SADHFormData formData;

		public virtual void ReadFrom(BaseJobDeclaration declaration)
		{
			using (formData.SuspendSettingHasChanges())
			{
				ReadDeclarationFields(declaration);

				if (declaration.Invoices.Count > 0)
				{
					BaseJobComInvoiceHeader invoice = declaration.Invoices[0];
					ReadInvoiceHeaderFields(invoice);

					if (invoice.JobComInvoiceLines.Count > 0)
					{
						ReadInvoiceLineFields(invoice.JobComInvoiceLines[0]);
					}
				}
			}
		}

		protected virtual void ReadDeclarationFields(BaseJobDeclaration declaration)
		{
			//Form Message Type (import/export)
			formData.D1_MessageType = declaration.JE_MessageType.Left(formData.D1_MessageTypeInfo.MaxLength);
			//Box 1 Declaration (Message Style)
			//FormData.D1_MessageSubType = declaration.JE_MessageSubType;
			//Box 2 Consignor
			formData.D1_OH_Consignor = declaration.JE_OH_Supplier;
			//Box 6 Total Packages
			formData.D1_TotalPackages = declaration.JE_TotalNoOfPacks;
			formData.D1_TotalPackagesPackType = declaration.JE_TotalNoOfPacksPackType.Left(formData.D1_TotalPackagesPackTypeInfo.MaxLength);
			//Box 8 Consignee
			formData.D1_OH_Consignee = declaration.JE_OH_Importer;
			//Box 14 Declarant/Representative (company that is filling out the form)
			//FormData.D1_OH_DeclarantRepresentative = GlbCompany.CurrentCompany.PK;
			//Box 15 Country of dispatch/export
			formData.D1_RL_NKCountryOfDispatchOrExport = declaration.JE_RL_NKPortOfLoading.Left(formData.D1_RL_NKCountryOfDispatchOrExportInfo.MaxLength);
			//Box 16 Country origin code
			formData.D1_RL_NKCountryOfOrigin = declaration.JE_RL_NKOrigin.Left(formData.D1_RL_NKCountryOfOriginInfo.MaxLength);
			//Box 17b Country of destination Code
			formData.D1_RL_NKCountryOfDestination = declaration.JE_RL_NKFinalDestination.Left(formData.D1_RL_NKCountryOfDestinationInfo.MaxLength);
			//Box 20 Delivery Terms
			formData.D1_DeliveryTerms = declaration.JE_ShipmentIncoTerm.Left(formData.D1_DeliveryTermsInfo.MaxLength);
			// Box 21 Identity and nationality of active means of transport crossing the border
			formData.D1_FlightNo = declaration.JE_VoyageFlightNo.Left(formData.D1_FlightNoInfo.MaxLength);
			formData.D1_FlightDate = declaration.JE_ExportDate;
			formData.D1_VesselCode = declaration.JE_VesselName.Left(formData.D1_VesselCodeInfo.MaxLength);
			//Box 25 Mode of transport at the border
			formData.D1_ModeOfTransportAtTheBorder = declaration.JE_TransportMode.Left(formData.D1_ModeOfTransportAtTheBorderInfo.MaxLength);
			//Box 27 Place of unloading
			formData.D1_RL_NKPlaceOfUnloading = declaration.JE_RL_NKPortOfArrival.Left(formData.D1_RL_NKPlaceOfUnloadingInfo.MaxLength);
			//Box 31 Packages and description of goods
			formData.D1_MarksAndNumbers = declaration.JE_MarksAndNumbers.Left(formData.D1_MarksAndNumbersInfo.MaxLength);
		}

		protected virtual void ReadInvoiceLineFields(BaseJobComInvoiceLine line)
		{
			//Box 31 Packages and description of goods
			formData.D1_DescriptionOfGoods = line.JI_Description.Left(formData.D1_DescriptionOfGoodsInfo.MaxLength);
			formData.D1_ItemUQ = line.JI_InvoiceUQ.Left(formData.D1_ItemUQInfo.MaxLength);
			formData.D1_ItemQty = line.JI_InvoiceQuantity;
			//Box 33 Commodity Code
			formData.D1_CommodityCode = line.JI_Tariff.Left(formData.D1_CommodityCodeInfo.MaxLength);
			//Box 34 Goods Country origin Code
			formData.D1_RN_NKItemCountryOfOrigin = line.JI_CountryOfOrigin.Left(formData.D1_RN_NKItemCountryOfOriginInfo.MaxLength);
			//Box 35 Gross mass (kg)
			formData.D1_GrossMassUQ = line.JI_WeightUQ.Left(formData.D1_GrossMassUQInfo.MaxLength);
			formData.D1_GrossMass = line.JI_Weight;
			//Supplementary Units
			formData.D1_SupplementaryUQ = line.JI_CustomsUnitQty.Left(formData.D1_SupplementaryUQInfo.MaxLength);
			formData.D1_SupplementaryQty = line.JI_CustomsQuantity;
			//Box 42 Item Price
			formData.D1_ItemPrice = line.JI_LinePrice;
		}

		protected virtual void ReadInvoiceHeaderFields(BaseJobComInvoiceHeader invoice)
		{
			formData.CurrConverter = invoice.CurrencyConverter;
			//Box 20 Delivery Terms
			formData.D1_DeliveryTerms = invoice.JZ_IncoTerm.Left(formData.D1_DeliveryTermsInfo.MaxLength);
			//Box 22 Currency and total amount invoiced
			formData.D1_InvoiceTotalAmount = invoice.JZ_InvoiceAmount;
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(invoice.Factory, invoice.JZ_RX_NKInvoice_Currency);
			if (currency != null)
			{
				formData.D1_RX_InvoiceCurrency = currency.PK;
			}
			else
			{
				formData.D1_RX_InvoiceCurrency = ZGuid.Empty;
			}
		}
	}
}
