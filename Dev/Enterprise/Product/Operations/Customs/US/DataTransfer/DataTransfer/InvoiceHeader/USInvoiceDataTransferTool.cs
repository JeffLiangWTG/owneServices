using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	public class USInvoiceDataTransferTool : InvoiceDataTransferTool
	{
		public USInvoiceDataTransferTool(bool isStandAlone)
			: base(isStandAlone)
		{
		}

		#region Import

		public override void ImportInvoiceDetails(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			base.ImportInvoiceDetails(invoiceHeader, xmlInvoiceHeader, context);
			JobComInvoiceHeader header = (JobComInvoiceHeader)invoiceHeader;

			ImportUSPayloadInvoiceHeaderDetails(header, xmlInvoiceHeader, context);

			if (xmlInvoiceHeader.ExchangeRateSpecified)
			{
				header.IsJZ_InvoiceCurrExRateUserEnterable = true;
				header.JZ_InvoiceCurrExRate = xmlInvoiceHeader.ExchangeRate;
			}
		}

		void ImportInvoiceRelatedDocuments(JobComInvoiceHeader header, Xsd.USInvoice xmlInvoice)
		{
			foreach (Xsd.TypeNumber xmlDoc in xmlInvoice.RelatedDocuments)
			{
				RelatedDocument doc = header.Factory.New<RelatedDocument>();
				header.RelatedDocuments.Add(doc);
				doc.CY_Data = xmlDoc.Number.Left(doc.CY_DataInfo.MaxLength);
				doc.CY_Code = xmlDoc.Type.Left(doc.CY_CodeInfo.MaxLength);
			}
		}

		void ImportUSPayloadInvoiceHeaderDetails(JobComInvoiceHeader header, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			Xsd.USInvoice xmlInvoice = xmlInvoiceHeader.CountryPayload.USInvoice;

			ImportInvoiceOrganizations(xmlInvoice.Organisations, header, context);

			if (xmlInvoice.TransactionRelatedSpecified)
			{
				header.US_TransactionsRelated = xmlInvoice.TransactionRelated ? "Y" : "N";
			}

			#region Import (not export) Shipment Type related data

			if (!header.IsExport)
			{
				if (xmlInvoice.CountryOfExportSpecified)
				{
					context.SetPropertyInfoValue(header.US_UC_NKCountryOfExportInfo, xmlInvoice.CountryOfExport, ForeignKeyType.CountryNK);
				}
				if (xmlInvoice.CountryOfOriginSpecified)
				{
					context.SetPropertyInfoValue(header.US_UC_NKCountryOfOriginInfo, xmlInvoice.CountryOfOrigin, ForeignKeyType.CountryNK);
				}
				if (xmlInvoice.DateOfExportSpecified)
				{
					header.US_DateOfExport = xmlInvoice.DateOfExport;
				}
				if (xmlInvoice.DateOfExportFromCOSpecified)
				{
					header.US_DateOfExportFromCountryOfOrigin = xmlInvoice.DateOfExportFromCO;
				}
				if (xmlInvoice.DestinationStateSpecified)
				{
					header.US_DestinationState = xmlInvoice.DestinationState.Left(AutoUSAddInfo.Schema.US_DestinationStateMaxLength);
				}
				if (xmlInvoice.InvoiceTypeSpecified)
				{
					header.US_InvoiceType = xmlInvoice.InvoiceType.Left(header.US_InvoiceTypeInfo.MaxLength);
				}
				if (xmlInvoice.PaymentTerms.CodeSpecified)
				{
					header.US_PaymentTerms = xmlInvoice.PaymentTerms.Code.Left(header.US_PaymentTermsInfo.MaxLength);
				}
				if (xmlInvoice.PaymentTerms.DescSpecified)
				{
					header.US_PaymentTermsDesc = xmlInvoice.PaymentTerms.Desc.Left(header.US_PaymentTermsDescInfo.MaxLength);
				}
				if (xmlInvoice.TermsOfDelivery.IndicatorSpecified)
				{
					header.US_TermsOfDeliveryLocationIndicator = xmlInvoice.TermsOfDelivery.Indicator.Left(header.US_TermsOfDeliveryLocationIndicatorInfo.MaxLength);
				}
				if (xmlInvoice.TermsOfDelivery.LocationSpecified)
				{
					header.US_TermsOfDeliveryLocation = xmlInvoice.TermsOfDelivery.Location.Left(header.US_TermsOfDeliveryLocationInfo.MaxLength);
				}
				if (xmlInvoice.TermsOfDelivery.QualifierSpecified)
				{
					header.US_TermsOfDeliveryLocationQualifier = xmlInvoice.TermsOfDelivery.Qualifier.Left(header.US_TermsOfDeliveryLocationQualifierInfo.MaxLength);
				}

				if (xmlInvoice.ValueForDiscountSpecified)
				{
					header.US_ValueForDiscount = xmlInvoice.ValueForDiscount;
				}
				if (xmlInvoice.ValueForForeignTaxSpecified)
				{
					header.US_ValueForForeignTax = xmlInvoice.ValueForForeignTax;
				}
				if (xmlInvoice.ContactNameSpecified)
				{
					header.US_FDAContactName = xmlInvoice.ContactName.Left(header.US_FDAContactNameInfo.MaxLength);
				}
				if (xmlInvoice.ContactPhoneNoSpecified)
				{
					header.US_FDAContactPhoneNo = xmlInvoice.ContactPhoneNo.Left(header.US_FDAContactPhoneNoInfo.MaxLength);
				}
			}
			#endregion

			ImportInvoiceRelatedDocuments(header, xmlInvoice);

			#region Export Shipment Type related Data

			header.US_ForeignTradeZone = xmlInvoice.FTZ.Left(header.US_ForeignTradeZoneInfo.MaxLength);
			if (xmlInvoice.InBondTypeSpecified)
			{
				context.SetPropertyInfoValue(header.US_InbondTypeInfo, InBondTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlInvoice.InBondType.ToString(), "", context), true, "Export Invoice InBond Type");
			}
			header.US_StateOfOrigin = xmlInvoice.StateOfOrigin.Left(header.US_StateOfOriginInfo.MaxLength);
			header.US_ImportEntryNo = xmlInvoice.ImpEntryNo.Left(header.US_ImportEntryNoInfo.MaxLength);
			header.US_HazardousCargo = (xmlInvoice.Hazardous == Xsd.TrueFalse.@true) ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
			header.US_RoutedTransaction = xmlInvoice.RoutedTran.Left(header.US_RoutedTransactionInfo.MaxLength);

			if (header.IsExport)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(xmlInvoiceHeader.Consignor, context, header.JZ_OA_SupplierAddressInfo);
			}

			#endregion
		}

		void ImportInvoiceOrganizations(Xsd.USInvoiceOrganisations organisations, JobComInvoiceHeader header, IValueObjectImportContext context)
		{
			if (organisations.Buyer.IsSpecified)
			{
				header.BuyerOrgPK = GetMatchedOrganisation(organisations.Buyer, context);
			}

			if (organisations.BuyingAgent.IsSpecified)
			{
				header.JZ_OH_BuyerAgent = GetMatchedOrganisation(organisations.BuyingAgent, context);
			}

			if (organisations.Exporter.IsSpecified)
			{
				header.ExporterOrgPK = GetMatchedOrganisation(organisations.Exporter, context);
			}

			if (organisations.Invoicer.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(organisations.Invoicer.Item, context, header.JZ_OA_InvoicerDocAddressInfo);
			}

			if (organisations.Manufacturer.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(organisations.Manufacturer.Item, context, header.JZ_OA_ManufacturerAddressInfo);
			}

			if (organisations.Seller.IsSpecified)
			{
				AdaptersHelper.SetOrgAddressDetails(organisations.Seller, context, header.JZ_OA_SellerAddressInfo);
			}

			if (organisations.SellingAgent.IsSpecified)
			{
				header.JZ_OH_SellingAgent = GetMatchedOrganisation(organisations.SellingAgent, context);
			}

			if (organisations.SupplierContactDetails.IsSpecified)
			{
				header.USPPIDocAddress.E2_Contact = organisations.SupplierContactDetails.Name.Left(header.USPPIDocAddress.E2_ContactInfo.MaxLength);
				header.USPPIDocAddress.E2_Phone_Formatted = organisations.SupplierContactDetails.Phone.Left(header.USPPIDocAddress.E2_Phone_FormattedInfo.MaxLength);
			}

			if (organisations.UltimateConsignee.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(organisations.UltimateConsignee.Organisation, context, header.JZ_OA_BuyerAddressInfo);
				header.UltimateConsigneeDocAddress.E2_Contact = organisations.UltimateConsignee.ContactDetails.Name.Left(header.UltimateConsigneeDocAddress.E2_ContactInfo.MaxLength);
				header.UltimateConsigneeDocAddress.E2_Phone_Formatted = organisations.UltimateConsignee.ContactDetails.Phone.Left(header.UltimateConsigneeDocAddress.E2_Phone_FormattedInfo.MaxLength);
			}

			if (organisations.IntermediateConsignee.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(organisations.IntermediateConsignee.Organisation, context, header.JZ_OA_IntermediateConsigneeAddressInfo);
				header.IntermediateConsigneeDocAddress.E2_Contact = organisations.IntermediateConsignee.ContactDetails.Name.Left(header.IntermediateConsigneeDocAddress.E2_ContactInfo.MaxLength);
				header.IntermediateConsigneeDocAddress.E2_Phone_Formatted = organisations.IntermediateConsignee.ContactDetails.Phone.Left(header.IntermediateConsigneeDocAddress.E2_Phone_FormattedInfo.MaxLength);
			}
		}

		protected override void SetProductNumber(IValueObjectImportContext context, BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLine xsdInvoiceLine)
		{
			JobComInvoiceLine uSInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (xsdInvoiceLine.CountryPayload != null)
			{
				Xsd.USInvoiceLine xmlInvoiceLine = xsdInvoiceLine.CountryPayload.USInvoiceLine;

				uSInvoiceLine.ShouldNotDefaultDOTForProductXMLImport = xmlInvoiceLine.DOTs.Count > 0;
				uSInvoiceLine.ShouldNotDefaultFDAForProductXMLImport = xmlInvoiceLine.FDAs.Count > 0;
				uSInvoiceLine.ShouldNotDefaultPGAForProductXMLImport = xmlInvoiceLine.LaceyActDetails.Count > 0;
			}

			base.SetProductNumber(context, uSInvoiceLine, xsdInvoiceLine);
		}

		protected override void ImportInvoiceLineDetail(BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLine xsdInvoiceLine, IValueObjectImportContext context)
		{
			base.ImportInvoiceLineDetail(invoiceLine, xsdInvoiceLine, context);

			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;
			if (xsdInvoiceLine.CountryPayload.USInvoiceLine != null)
			{
				ImportInvoiceLineUSCountryPayloadData(line, xsdInvoiceLine.CountryPayload.USInvoiceLine, context);
			}
		}

		protected override ZString GetTariffDescription(BaseJobComInvoiceLine invoiceLine)
		{
			ZString result = base.GetTariffDescription(invoiceLine);

			if (result.IsEmpty)
			{
				JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;
				if (line.ImportTariff != null)
				{
					result = line.ImportTariff.UE_ShortDescription;
				}
			}

			return result;
		}

		void ImportInvoiceLineUSCountryPayloadData(JobComInvoiceLine invoiceLine, Xsd.USInvoiceLine xmlInvoiceLine, IValueObjectImportContext context)
		{
			bool notExportShipmentType = invoiceLine.InvoiceHeader != null && !invoiceLine.InvoiceHeader.IsExport;
			if (notExportShipmentType)
			{
				#region Line Organisations

				if (xmlInvoiceLine.Manufacturer.IsSpecified)
				{
					AdaptersHelper.FindOrCreateOrgFromMID(xmlInvoiceLine.Manufacturer.Item, context, invoiceLine.JI_OA_ManufacturerAddressInfo);
				}

				if (xmlInvoiceLine.UltimateConsignee.IsSpecified)
				{
					AdaptersHelper.SetOrgDetailsToBusinessObjectOrganization(xmlInvoiceLine.UltimateConsignee, context, invoiceLine.JI_OA_ConsigneeAddressInfo);
				}

				#endregion

				#region Supplementary

				invoiceLine.US_SupTariff = xmlInvoiceLine.Supplementary.Tariff;
				invoiceLine.US_SupUQ1 = xmlInvoiceLine.Supplementary.Quantity1.DimensionType.Left(invoiceLine.US_SupUQ1Info.MaxLength);
				invoiceLine.US_SupQty1 = xmlInvoiceLine.Supplementary.Quantity1.Value;
				invoiceLine.US_SupUQ2 = xmlInvoiceLine.Supplementary.Quantity2.DimensionType.Left(invoiceLine.US_SupUQ2Info.MaxLength);
				invoiceLine.US_SupQty2 = xmlInvoiceLine.Supplementary.Quantity2.Value;
				invoiceLine.US_SupUQ3 = xmlInvoiceLine.Supplementary.Quantity3.DimensionType.Left(invoiceLine.US_SupUQ3Info.MaxLength);
				invoiceLine.US_SupQty3 = xmlInvoiceLine.Supplementary.Quantity3.Value;

				#endregion

				if (xmlInvoiceLine.USOrOriginalGoodsValueSpecified)
				{
					invoiceLine.US_98GoodsValue = xmlInvoiceLine.USOrOriginalGoodsValue;
				}

				if (xmlInvoiceLine.USOrOriginalValueInInvCurrSpecified)
				{
					invoiceLine.US_98ValueInvCurr = xmlInvoiceLine.USOrOriginalValueInInvCurr;
				}

				if (xmlInvoiceLine.ArticleNo.ID1Specified)
				{
					invoiceLine.US_ArticleNoA = xmlInvoiceLine.ArticleNo.ID1.Left(AutoUSAddInfo.Schema.US_ArticleNoAMaxLength);
				}
				if (xmlInvoiceLine.ArticleNo.ID2Specified)
				{
					invoiceLine.US_ArticleNoB = xmlInvoiceLine.ArticleNo.ID2.Left(AutoUSAddInfo.Schema.US_ArticleNoBMaxLength);
				}

				if (!invoiceLine.IsSecondaryTariffLine)
				{
					if (xmlInvoiceLine.CountryOfExportSpecified)
					{
						context.SetPropertyInfoValue(invoiceLine.US_UC_NKCountryOfExportInfo, xmlInvoiceLine.CountryOfExport, ForeignKeyType.CountryNK);
					}
					if (xmlInvoiceLine.CountryOfOriginSpecified)
					{
						context.SetPropertyInfoValue(invoiceLine.US_UC_NKCountryOfOriginInfo, xmlInvoiceLine.CountryOfOrigin, ForeignKeyType.CountryNK);
					}
				}

				ImportAntiDumpingCountervailingDetails(invoiceLine, xmlInvoiceLine);

				if (xmlInvoiceLine.DateOfExportFromCOSpecified)
				{
					invoiceLine.US_DateOfExportFromCountryOfOrigin = xmlInvoiceLine.DateOfExportFromCO;
				}

				if (xmlInvoiceLine.DestinationStateSpecified)
				{
					invoiceLine.US_DestinationState = xmlInvoiceLine.DestinationState.Left(AutoUSAddInfo.Schema.US_DestinationStateMaxLength);
				}

				if (xmlInvoiceLine.TransactionRelatedSpecified)
				{
					invoiceLine.US_TransactionsRelated = xmlInvoiceLine.TransactionRelated ? "Y" : "N";
				}

				#region OGA

				if (xmlInvoiceLine.OGAIndicators.DOTIndicatorSpecified)
				{
					context.SetPropertyInfoValue(invoiceLine.US_DOTIndicatorInfo, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(xmlInvoiceLine.OGAIndicators.DOTIndicator.ToString(), "", context), true, "Invoice Line DOT Indicator");
				}
				if (xmlInvoiceLine.OGAIndicators.FCCIndicatorSpecified)
				{
					context.SetPropertyInfoValue(invoiceLine.US_FCCIndicatorInfo, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(xmlInvoiceLine.OGAIndicators.FCCIndicator.ToString(), "", context), true, "Invoice Line FCC Indicator");
				}
				if (xmlInvoiceLine.OGAIndicators.FDAIndicatorSpecified)
				{
					context.SetPropertyInfoValue(invoiceLine.US_FDAIndicatorInfo, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(xmlInvoiceLine.OGAIndicators.FDAIndicator.ToString(), "", context), true, "Invoice Line FDA Indicator");
				}

				OGADataTransferTool.ImportDOTDetails(xmlInvoiceLine.DOTs, invoiceLine.DOTs);
				OGADataTransferTool.ImportFCCDetails(xmlInvoiceLine.FCCs, invoiceLine.FCCs);
				OGADataTransferTool.ImportFDADetails(xmlInvoiceLine.FDAs, invoiceLine.FDAs, context);

				#endregion

				if (xmlInvoiceLine.FTZ.PrivilegedStatusDateSpecified)
				{
					invoiceLine.US_PrivilegedStatusDate = xmlInvoiceLine.FTZ.PrivilegedStatusDate;
				}
				if (xmlInvoiceLine.FTZ.ZoneStatusSpecified)
				{
					invoiceLine.US_ZoneStatus = xmlInvoiceLine.FTZ.ZoneStatus.Left(invoiceLine.US_ZoneStatusInfo.MaxLength);
				}

				if (xmlInvoiceLine.HazardousMaterial.ClassificationDescSpecified)
				{
					invoiceLine.US_HazMatClassDesc = xmlInvoiceLine.HazardousMaterial.ClassificationDesc.Left(invoiceLine.US_HazMatClassDescInfo.MaxLength);
				}
				if (xmlInvoiceLine.HazardousMaterial.DescSpecified)
				{
					invoiceLine.US_HazMatDesc = xmlInvoiceLine.HazardousMaterial.Desc.Left(invoiceLine.US_HazMatDescInfo.MaxLength);
				}

				if (xmlInvoiceLine.ManifestQtySpecified)
				{
					invoiceLine.US_ManifestQty = xmlInvoiceLine.ManifestQty.ToZInt();
				}

				if (xmlInvoiceLine.NAFTANetSpecified)
				{
					invoiceLine.US_IsNAFTANet = xmlInvoiceLine.NAFTANet;
				}

				ImportPermitAndLicenceData(invoiceLine, xmlInvoiceLine);

				if (xmlInvoiceLine.SecondarySPISpecified)
				{
					invoiceLine.US_SecondarySPI = xmlInvoiceLine.SecondarySPI.Left(invoiceLine.US_SecondarySPIInfo.MaxLength);
				}

				if (xmlInvoiceLine.SelectedRateTypeSpecified)
				{
					invoiceLine.US_SelectedRateType = xmlInvoiceLine.SelectedRateType.Left(invoiceLine.US_SelectedRateTypeInfo.MaxLength);
				}
				if (xmlInvoiceLine.SPISpecified)
				{
					invoiceLine.US_SPI = xmlInvoiceLine.SPI.Left(invoiceLine.US_SPIInfo.MaxLength);
				}

				if (xmlInvoiceLine.ThirdQty.DimensionTypeSpecified)
				{
					invoiceLine.JI_CustomsThirdUnitQty = xmlInvoiceLine.ThirdQty.DimensionType.Left(invoiceLine.JI_CustomsThirdUnitQtyInfo.MaxLength);
				}
				if (xmlInvoiceLine.ThirdQty.Value > 0)
				{
					invoiceLine.JI_CustomsThirdQuantity = xmlInvoiceLine.ThirdQty.Value;
				}

				if (xmlInvoiceLine.TSCA.IndicatorSpecified)
				{
					invoiceLine.US_TSCAIndicator = xmlInvoiceLine.TSCA.Indicator.Left(invoiceLine.US_TSCAIndicatorInfo.MaxLength);
				}
				if (xmlInvoiceLine.TSCA.NameSpecified)
				{
					invoiceLine.US_TSCAName = xmlInvoiceLine.TSCA.Name.Left(invoiceLine.US_TSCANameInfo.MaxLength);
				}

				ImportInvoiceLinesOverridenTaxRate(invoiceLine, xmlInvoiceLine, context);

				ImportInvoiceLineFees(invoiceLine, xmlInvoiceLine);

				OGADataTransferTool.ImportLaceyActDetails(xmlInvoiceLine.LaceyActDetails, invoiceLine.LaceyActLines, invoiceLine.Factory, true, invoiceLine.ContainersForInvoiceLinesForBindingOnly);
			}

			if (xmlInvoiceLine.SecondQty.DimensionTypeSpecified)
			{
				invoiceLine.JI_CustomsSecondUnitQty = xmlInvoiceLine.SecondQty.DimensionType.Left(invoiceLine.JI_CustomsSecondUnitQtyInfo.MaxLength);
			}
			if (xmlInvoiceLine.SecondQty.Value > 0)
			{
				invoiceLine.JI_CustomsSecondQuantity = xmlInvoiceLine.SecondQty.Value;
			}

			#region Export Shipment Type Related Data

			invoiceLine.US_ECCN = xmlInvoiceLine.ExportLicense.ECCN.Left(invoiceLine.US_ECCNInfo.MaxLength);
			invoiceLine.US_LicenseNo = xmlInvoiceLine.ExportLicense.Number.Left(invoiceLine.US_LicenseNoInfo.MaxLength);
			invoiceLine.US_LicenseType = xmlInvoiceLine.ExportLicense.Type.Left(invoiceLine.US_LicenseTypeInfo.MaxLength);
			invoiceLine.US_ExportCode = xmlInvoiceLine.ExportCode.Left(invoiceLine.US_ExportCodeInfo.MaxLength);
			invoiceLine.US_DDTCITARExemptionNo = xmlInvoiceLine.DDTCDetails.ITARExemptionNumber.Left(invoiceLine.US_DDTCITARExemptionNoInfo.MaxLength);
			if (xmlInvoiceLine.DDTCDetails.MilitaryEquipmentIndicatorSpecified)
			{
				invoiceLine.US_DDTCMilitaryEquipmentIndicator = (xmlInvoiceLine.DDTCDetails.MilitaryEquipmentIndicator == Xsd.TrueFalse.@true) ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
			}
			if (xmlInvoiceLine.DDTCDetails.PartyCertificationIndicatorSpecified)
			{
				invoiceLine.US_DDTCPartyCertificationIndicator = (xmlInvoiceLine.DDTCDetails.PartyCertificationIndicator == Xsd.TrueFalse.@true) ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
			}
			invoiceLine.US_DDTCQuantity = xmlInvoiceLine.DDTCDetails.Quantity.Value;
			invoiceLine.US_DDTCUnit = xmlInvoiceLine.DDTCDetails.Quantity.DimensionType.Left(invoiceLine.US_DDTCUnitInfo.MaxLength);
			invoiceLine.US_DDTCRegistrationNo = xmlInvoiceLine.DDTCDetails.RegistrationNumber.Left(invoiceLine.US_DDTCRegistrationNoInfo.MaxLength);
			invoiceLine.US_DDTCUSMLCategoryCode = xmlInvoiceLine.DDTCDetails.USMLCategoryCode.Left(invoiceLine.US_DDTCUSMLCategoryCodeInfo.MaxLength);
			context.SetPropertyInfoValue(invoiceLine.US_AESOriginIndicatorInfo, AESOriginIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(xmlInvoiceLine.OriginIndicator.ToString(), "", context), true, "Export Original Indicator");
			invoiceLine.US_IsUsedVehicle = xmlInvoiceLine.VehicleDetails.UsedVehicle;
			if (invoiceLine.US_IsUsedVehicle)
			{
				invoiceLine.US_VehicleID = xmlInvoiceLine.VehicleDetails.ID;
				invoiceLine.US_VehicleIDType = xmlInvoiceLine.VehicleDetails.IDType;
				invoiceLine.US_VehicleTitleNo = xmlInvoiceLine.VehicleDetails.TitleNumber;
				invoiceLine.US_VehicleTitleState = xmlInvoiceLine.VehicleDetails.TitleState;
			}

			#endregion
		}

		void ImportInvoiceLinesOverridenTaxRate(JobComInvoiceLine invoiceLine, Xsd.USInvoiceLine xmlInvoiceLine, IValueObjectImportContext context)
		{
			if (xmlInvoiceLine.OverriddenTaxRateSpecified || xmlInvoiceLine.TaxCodeSpecified)
			{
				invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
				invoiceLine.US_TaxCode = USInvoiceLineTaxCodeMappings.Instance.GetEnterpriseCode(xmlInvoiceLine.TaxCode, "", context);
			}

			if (xmlInvoiceLine.TaxRateTypeSpecified)
			{
				invoiceLine.US_TaxRateT = USInvoiceLineTaxRateTypeMappings.Instance.GetEnterpriseCode(xmlInvoiceLine.TaxRateType, "", context);
			}

			if (xmlInvoiceLine.OverriddenTaxRateSpecified || xmlInvoiceLine.TaxCodeSpecified)
			{
				invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
				invoiceLine.US_TaxRate = xmlInvoiceLine.OverriddenTaxRate;
			}
		}

		void ImportPermitAndLicenceData(JobComInvoiceLine invoiceLine, Xsd.USInvoiceLine xmlInvoiceLine)
		{
			if (xmlInvoiceLine.PermitAndLicence.AgricultureLicNoSpecified)
			{
				invoiceLine.US_AgricultureLicNo = xmlInvoiceLine.PermitAndLicence.AgricultureLicNo.Left(invoiceLine.US_AgricultureLicNoInfo.MaxLength);
			}

			if (xmlInvoiceLine.PermitAndLicence.CottonFeeExemptIndicatorSpecified)
			{
				invoiceLine.US_CottonFeeExempt = xmlInvoiceLine.PermitAndLicence.CottonFeeExemptIndicator.Left(1);
			}

			if (xmlInvoiceLine.PermitAndLicence.CottonFeeExemptIndicator != "Y" && xmlInvoiceLine.PermitAndLicence.CottonCertificateNoSpecified)
			{
				invoiceLine.US_CottonCertificateNo = xmlInvoiceLine.PermitAndLicence.CottonCertificateNo.Left(invoiceLine.US_CottonCertificateNoInfo.MaxLength);
			}

			if (xmlInvoiceLine.PermitAndLicence.CAExportCertificateSpecified)
			{
				invoiceLine.US_CAExportCertificate = xmlInvoiceLine.PermitAndLicence.CAExportCertificate.Left(invoiceLine.US_CAExportCertificateInfo.MaxLength);
			}
			if (xmlInvoiceLine.PermitAndLicence.CBTPACertificateSpecified)
			{
				invoiceLine.US_CBTPACertificateNo = xmlInvoiceLine.PermitAndLicence.CBTPACertificate.Left(invoiceLine.US_CBTPACertificateNoInfo.MaxLength);
			}
			if (xmlInvoiceLine.PermitAndLicence.MiscPermitNoSpecified)
			{
				invoiceLine.US_MiscPermitNo = xmlInvoiceLine.PermitAndLicence.MiscPermitNo.Left(invoiceLine.US_MiscPermitNoInfo.MaxLength);
			}
			if (xmlInvoiceLine.PermitAndLicence.SWPMIndicatorSpecified)
			{
				invoiceLine.US_SWPMIndicator = xmlInvoiceLine.PermitAndLicence.SWPMIndicator.Left(1);
			}
			if (xmlInvoiceLine.PermitAndLicence.WoolLicenceNoSpecified)
			{
				invoiceLine.US_WoolLicenceNo = xmlInvoiceLine.PermitAndLicence.WoolLicenceNo.Left(invoiceLine.US_WoolLicenceNoInfo.MaxLength);
			}

			if (xmlInvoiceLine.PIRPRuling.NumberSpecified)
			{
				invoiceLine.US_PIRPRulingNo = xmlInvoiceLine.PIRPRuling.Number.Left(invoiceLine.US_PIRPRulingNoInfo.MaxLength);
			}
			if (xmlInvoiceLine.PIRPRuling.TypeSpecified)
			{
				invoiceLine.US_PIRPRulingType = xmlInvoiceLine.PIRPRuling.Type.Left(invoiceLine.US_PIRPRulingTypeInfo.MaxLength);
			}

			if (xmlInvoiceLine.SoftwoodLumber.ExportChargeSpecified)
			{
				invoiceLine.US_LumberExportCharges = xmlInvoiceLine.SoftwoodLumber.ExportCharge;
			}
			if (xmlInvoiceLine.SoftwoodLumber.ExportPriceSpecified)
			{
				invoiceLine.US_LumberExportPrice = xmlInvoiceLine.SoftwoodLumber.ExportPrice;
			}
			if (xmlInvoiceLine.SoftwoodLumber.ImporterDecSpecified)
			{
				invoiceLine.US_LumberImporterDeclaration = xmlInvoiceLine.SoftwoodLumber.ImporterDec ? "Y" : "";
			}

			if (xmlInvoiceLine.Textile.CategoryNoSpecified)
			{
				invoiceLine.US_TextileCategoryNo = xmlInvoiceLine.Textile.CategoryNo.Left(invoiceLine.US_TextileCategoryNoInfo.MaxLength);
			}
			if (xmlInvoiceLine.Textile.VisaNoSpecified)
			{
				invoiceLine.US_VisaNo = xmlInvoiceLine.Textile.VisaNo.Left(invoiceLine.US_VisaNoInfo.MaxLength);
			}
			if (xmlInvoiceLine.Textile.VisaQtySpecified)
			{
				invoiceLine.US_VisaQty = xmlInvoiceLine.Textile.VisaQty;
			}
			if (xmlInvoiceLine.Textile.VisaUQSpecified)
			{
				invoiceLine.US_VisaUQ = xmlInvoiceLine.Textile.VisaUQ.Left(invoiceLine.US_VisaUQInfo.MaxLength);
			}
		}

		void ImportAntiDumpingCountervailingDetails(JobComInvoiceLine invoiceLine, Xsd.USInvoiceLine xmlInvoiceLine)
		{
			if (xmlInvoiceLine.AntiDumping.IsSpecified)
			{
				if (xmlInvoiceLine.AntiDumping.BondedSpecified)
				{
					invoiceLine.US_IsBondedADD = xmlInvoiceLine.AntiDumping.Bonded;
				}
				invoiceLine.US_ADDCaseNo = xmlInvoiceLine.AntiDumping.CaseNo.Left(AutoUSAddInfo.Schema.US_ADDCaseNoMaxLength);
				if (xmlInvoiceLine.AntiDumping.DepositRateIndicatorSpecified)
				{
					invoiceLine.US_ADDDepositRateIndicator = xmlInvoiceLine.AntiDumping.DepositRateIndicator.Left(1);
				}
				if (xmlInvoiceLine.AntiDumping.DepositValueSpecified)
				{
					invoiceLine.US_ADDDepositValue = xmlInvoiceLine.AntiDumping.DepositValue;
				}
			}

			if (xmlInvoiceLine.Countervailing.IsSpecified)
			{
				if (xmlInvoiceLine.Countervailing.BondedSpecified)
				{
					invoiceLine.US_IsBondedCVD = xmlInvoiceLine.Countervailing.Bonded;
				}
				invoiceLine.US_CVDCaseNo = xmlInvoiceLine.Countervailing.CaseNo.Left(AutoUSAddInfo.Schema.US_CVDCaseNoMaxLength);
				if (xmlInvoiceLine.Countervailing.DepositRateIndicatorSpecified)
				{
					invoiceLine.US_CVDDepositRateIndicator = xmlInvoiceLine.Countervailing.DepositRateIndicator.Left(1);
				}
				if (xmlInvoiceLine.Countervailing.DepositValueSpecified)
				{
					invoiceLine.US_CVDDepositValue = xmlInvoiceLine.Countervailing.DepositValue;
				}
			}
		}

		void ImportInvoiceLineFees(JobComInvoiceLine invoiceLine, Xsd.USInvoiceLine xmlInvoiceLine)
		{
			foreach (Xsd.USInvoiceLineFee xmlFee in xmlInvoiceLine.Fees)
			{
				FeeCusCodeData fee = invoiceLine.Factory.New<FeeCusCodeData>();
				invoiceLine.FeeCusCodes.Add(fee);
				fee.CY_Code = xmlFee.Code.Left(fee.CY_CodeInfo.MaxLength);
				fee.CY_FeeAmount = xmlFee.FeeAmount;
				fee.CY_IsOverridden = xmlFee.Overidden == Xsd.TrueFalse.@true;
				fee.CY_SelectedRateType = xmlFee.RateType.Left(fee.CY_SelectedRateTypeInfo.MaxLength);
			}
		}

		protected override void ImportLineClassification(BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLineLineClassification lineClassification, IValueObjectImportContext context)
		{
			if (lineClassification != null)
			{
				base.ImportLineClassification(invoiceLine, lineClassification, context);

				if (!lineClassification.TariffCode.Type.IsEmpty)
				{
					JobComInvoiceLine usInvLine = (JobComInvoiceLine)invoiceLine;
					context.SetPropertyInfoValue(usInvLine.US_TariffTypeInfo, lineClassification.TariffCode.Type);
				}
			}
		}

		#endregion

		#region Export

		public override void ExportInvoiceHeaderValues(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectExportContext context)
		{
			base.ExportInvoiceHeaderValues(invoiceHeader, xmlInvoiceHeader, context);
			ExportUSPayloadDetails(invoiceHeader, xmlInvoiceHeader, context);
		}

		void ExportUSPayloadDetails(BaseJobComInvoiceHeader bizObj, Xsd.InvoiceHeader constructedValueObject, IValueObjectExportContext context)
		{
			JobComInvoiceHeader header = (JobComInvoiceHeader)bizObj;
			Xsd.USInvoice invoice = new Xsd.USInvoice();
			invoice.IsSpecified = true;

			//no US specific xml data for Export Shipment Type Standalone Commercial Invoice
			bool standaloneExportCommercialInvoice = IsStandAlone && header.IsExport;
			if (standaloneExportCommercialInvoice)
			{
				invoice.IsSpecified = false;
			}
			else
			{
				ExportUSPayloadDetailsCore(header, invoice, context);
				ExportInvoiceUSPPIOrg(header.USPPIDocAddress.Address, constructedValueObject, context);
			}

			if (invoice.IsSpecified)
			{
				constructedValueObject.CountryPayload.USInvoice = invoice;
			}

			if (!header.IsJZ_InvoiceCurrExRateUserEnterable)
			{
				constructedValueObject.ExchangeRate = ZDecimal.Zero;
				constructedValueObject.ExchangeRateSpecified = false;
			}
		}

		void ExportInvoiceUSPPIOrg(OrgAddress uSPPIAddress, Xsd.InvoiceHeader constructedValueObject, IValueObjectExportContext context)
		{
			if (uSPPIAddress != null)
			{
				constructedValueObject.Consignor = OrganisationDataAdapter.ExportToValueObject(uSPPIAddress.Header, context);
				AdaptersHelper.SetAddressSequence(constructedValueObject.Consignor.OrganisationDetails.Addresses, uSPPIAddress.OA_Address1, uSPPIAddress.OA_Address2);
			}
		}

		void ExportInvoiceRelatedDocuments(JobComInvoiceHeader invoiceHeaderBO, Xsd.USInvoice invoice)
		{
			foreach (RelatedDocument document in invoiceHeaderBO.RelatedDocuments)
			{
				Xsd.TypeNumber xmlDoc = new Xsd.TypeNumber();
				xmlDoc.Number = document.CY_Data;
				xmlDoc.Type = document.CY_Code;
				invoice.RelatedDocuments.Add(xmlDoc);
			}
		}

		void ExportUSPayloadDetailsCore(JobComInvoiceHeader invoiceHeaderBO, Xsd.USInvoice invoice, IValueObjectExportContext context)
		{
			if (!invoiceHeaderBO.US_TransactionsRelated.IsEmpty)
			{
				invoice.TransactionRelated = invoiceHeaderBO.US_TransactionsRelated == "Y";
				invoice.TransactionRelatedSpecified = true;
			}

			if (!invoiceHeaderBO.IsExport)
			{
				ExportInvoiceRelatedDocuments(invoiceHeaderBO, invoice);
				invoice.CountryOfExport = invoiceHeaderBO.US_UC_NKCountryOfExport;
				if (!IsStandAlone)
				{
					invoice.CountryOfOrigin = invoiceHeaderBO.US_UC_NKCountryOfOrigin;
				}
				invoice.DateOfExport = invoiceHeaderBO.US_DateOfExport.Date;
				invoice.DateOfExportFromCO = invoiceHeaderBO.US_DateOfExportFromCountryOfOrigin.Date;
				invoice.DestinationState = invoiceHeaderBO.US_DestinationState;
				invoice.InvoiceType = invoiceHeaderBO.US_InvoiceType;

				#region Import Shipment Type Invoice Org

				invoice.Organisations.Buyer = OrganisationDataAdapter.ExportToValueObject(invoiceHeaderBO.USBuyer, context);
				invoice.Organisations.BuyingAgent = OrganisationDataAdapter.ExportToValueObject(invoiceHeaderBO.BuyerAgent, context);
				invoice.Organisations.Exporter = OrganisationDataAdapter.ExportToValueObject(invoiceHeaderBO.ExporterAddress?.Header, context);

				Xsd.Organisation org;
				if (invoiceHeaderBO.InvoicerAddress != null)
				{
					org = OrganisationDataAdapter.ExportToValueObject(invoiceHeaderBO.InvoicerAddress.Header, context);
					invoice.Organisations.Invoicer.Item = org;
					AdaptersHelper.SetAddressSequence(((Xsd.Organisation)invoice.Organisations.Invoicer.Item).OrganisationDetails.Addresses, invoiceHeaderBO.InvoicerAddress.OA_Address1, invoiceHeaderBO.InvoicerAddress.OA_Address2);
					invoice.Organisations.Invoicer.IsSpecified = true;
				}

				if (invoiceHeaderBO.ManufacturerAddress != null)
				{
					org = OrganisationDataAdapter.ExportToValueObject(invoiceHeaderBO.ManufacturerAddress.Header, context);
					invoice.Organisations.Manufacturer.Item = org;
					AdaptersHelper.SetAddressSequence(((Xsd.Organisation)invoice.Organisations.Manufacturer.Item).OrganisationDetails.Addresses, invoiceHeaderBO.ManufacturerAddress.OA_Address1, invoiceHeaderBO.ManufacturerAddress.OA_Address2);
					invoice.Organisations.Manufacturer.IsSpecified = true;
				}

				invoice.Organisations.Seller = OrganisationDataAdapter.ExportToValueObject(invoiceHeaderBO.Seller, context);
				invoice.Organisations.SellingAgent = OrganisationDataAdapter.ExportToValueObject(invoiceHeaderBO.SellingAgent, context);

				#endregion

				invoice.PaymentTerms.Code = invoiceHeaderBO.US_PaymentTerms;
				invoice.PaymentTerms.Desc = invoiceHeaderBO.US_PaymentTermsDesc;
				invoice.TermsOfDelivery.Indicator = invoiceHeaderBO.US_TermsOfDeliveryLocationIndicator;
				invoice.TermsOfDelivery.Location = invoiceHeaderBO.US_TermsOfDeliveryLocation;
				invoice.TermsOfDelivery.Qualifier = invoiceHeaderBO.US_TermsOfDeliveryLocationQualifier;

				invoice.ValueForDiscount = invoiceHeaderBO.US_ValueForDiscount;
				invoice.ValueForDiscountSpecified = true;
				invoice.ValueForForeignTax = invoiceHeaderBO.US_ValueForForeignTax;
				invoice.ValueForForeignTaxSpecified = true;
				invoice.ContactName = invoiceHeaderBO.US_FDAContactName;
				invoice.ContactPhoneNo = invoiceHeaderBO.US_FDAContactPhoneNo;

				invoice.InBondTypeSpecified = false;
				invoice.HazardousSpecified = false;
				invoice.RoutedTranSpecified = false;
				invoice.Organisations.SupplierContactDetails.IsSpecified = false;
				invoice.Organisations.IntermediateConsignee.IsSpecified = false;
				invoice.Organisations.UltimateConsignee.IsSpecified = false;
			}
			else
			{
				#region Export Shipment Type related Properties

				if (invoiceHeaderBO.USPPIDocAddress.Address != null)
				{
					invoice.Organisations.SupplierContactDetails.Name = invoiceHeaderBO.USPPIDocAddress.E2_Contact;
					invoice.Organisations.SupplierContactDetails.Phone = invoiceHeaderBO.USPPIDocAddress.E2_Phone_Formatted;
				}

				if (invoiceHeaderBO.BuyerAddress != null)
				{
					ExportOrgWithContactDetails(invoice.Organisations.UltimateConsignee, invoiceHeaderBO.BuyerAddress, invoiceHeaderBO.UltimateConsigneeDocAddress.E2_Contact, invoiceHeaderBO.UltimateConsigneeDocAddress.E2_Phone_Formatted, context);
				}

				if (invoiceHeaderBO.IntermediateConsigneeAddress != null)
				{
					ExportOrgWithContactDetails(invoice.Organisations.IntermediateConsignee, invoiceHeaderBO.IntermediateConsigneeAddress, invoiceHeaderBO.IntermediateConsigneeDocAddress.E2_Contact, invoiceHeaderBO.IntermediateConsigneeDocAddress.E2_Phone_Formatted, context);
				}

				invoice.FTZ = invoiceHeaderBO.US_ForeignTradeZone;
				invoice.InBondType = InBondTypeToXmlCodeMappings.Instance.GetExternalCode(invoiceHeaderBO.US_InbondType, "", context);
				invoice.StateOfOrigin = invoiceHeaderBO.US_StateOfOrigin;
				invoice.ImpEntryNo = invoiceHeaderBO.US_ImportEntryNo;
				invoice.Hazardous = invoiceHeaderBO.US_HazardousCargo == YesNoDefaultList.Codes.Yes ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				invoice.RoutedTran = invoiceHeaderBO.US_RoutedTransaction;

				#endregion

				invoice.PaymentTerms.IsSpecified = false;
				invoice.TermsOfDelivery.IsSpecified = false;
			}
		}

		void ExportOrgWithContactDetails(Xsd.USOrganisationWithContactDetails orgWithContact, OrgAddress address, ZString contactName, ZString contactPhone, IValueObjectExportContext context)
		{
			orgWithContact.Organisation = OrganisationDataAdapter.ExportToValueObject(address.Header, context);
			AdaptersHelper.SetAddressSequence(orgWithContact.Organisation.OrganisationDetails.Addresses, address.OA_Address1, address.OA_Address2);
			orgWithContact.ContactDetails.Name = contactName;
			orgWithContact.ContactDetails.Phone = contactPhone;
			orgWithContact.IsSpecified = true;
		}

		public override void ExportInvoiceLineDetails(Xsd.InvoiceLine newInvoiceLine, BaseJobComInvoiceLine invoiceLine, INotifications notifications)
		{
			base.ExportInvoiceLineDetails(newInvoiceLine, invoiceLine, notifications);

			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;

			if (newInvoiceLine.LineClassification.IsSpecified && newInvoiceLine.LineClassification.TariffCode.ValueSpecified && !line.US_TariffType.IsEmpty)
			{
				newInvoiceLine.LineClassification.TariffCode.Type = line.US_TariffType;
			}

			Xsd.USInvoiceLine usXsdInvoiceLine = new Xsd.USInvoiceLine();
			usXsdInvoiceLine.IsSpecified = true;
			ExportInvoiceLineUSCountryPayloadData(usXsdInvoiceLine, line, new ValueObjectExportContext(notifications));
			newInvoiceLine.CountryPayload.USInvoiceLine = usXsdInvoiceLine;
		}

		void ExportInvoiceLineUSCountryPayloadData(Xsd.USInvoiceLine usXsdInvoiceLine, JobComInvoiceLine invoiceLine, IValueObjectExportContext context)
		{
			var invoiceHeader = invoiceLine.InvoiceHeader;
			bool notExportInvoice = invoiceHeader != null && !invoiceHeader.IsExport;

			if (notExportInvoice)
			{
				usXsdInvoiceLine.USOrOriginalGoodsValue = invoiceLine.US_98GoodsValue;
				usXsdInvoiceLine.USOrOriginalGoodsValueSpecified = true;

				usXsdInvoiceLine.USOrOriginalValueInInvCurr = invoiceLine.US_98ValueInvCurr;
				usXsdInvoiceLine.USOrOriginalValueInInvCurrSpecified = true;

				ExportAntidumpingCountervailing(usXsdInvoiceLine, invoiceLine);

				usXsdInvoiceLine.ArticleNo.ID1 = invoiceLine.US_ArticleNoA;
				usXsdInvoiceLine.ArticleNo.ID2 = invoiceLine.US_ArticleNoB;
				usXsdInvoiceLine.ArticleNo.IsSpecified = !invoiceLine.US_ArticleNoA.IsEmpty || !invoiceLine.US_ArticleNoB.IsEmpty;

				#region aiiLine Details
				var declaration = invoiceHeader != null ? invoiceHeader.JobDeclaration : null;
				AIILine aiiLine = declaration == null || !declaration.IsPersistent || declaration.US_EnableAII ? invoiceLine.FirstAIILine : null;
				if (aiiLine == null)
				{
					usXsdInvoiceLine.BasisUnit.IsSpecified = false;
					usXsdInvoiceLine.DispatchedInvoiceQty.IsSpecified = false;
				}
				else
				{
					usXsdInvoiceLine.BasisUnit.BasisUnit = Convert.ToDecimal(aiiLine.US_UnitBasis);
					usXsdInvoiceLine.BasisUnit.BasisUnitSpecified = true;
					usXsdInvoiceLine.BasisUnit.Price = aiiLine.US_UnitPrice;
					usXsdInvoiceLine.BasisUnit.PriceSpecified = true;
					usXsdInvoiceLine.PercentageOfActiveIngredient = aiiLine.US_PercActvIngr;
					usXsdInvoiceLine.PercentageOfActiveIngredientSpecified = true;

					#region Quantity Dispatched

					usXsdInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonDesc = aiiLine.US_QtyDiffRsn;
					usXsdInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonCode = aiiLine.US_QtyDiffRsnCode;
					usXsdInvoiceLine.DispatchedInvoiceQty.Quantity = Xsd.DimensionValue.FromAmountAndUnit(aiiLine.US_InvQtyDisp, aiiLine.US_InvUQDisp);
					usXsdInvoiceLine.DispatchedInvoiceQty.IsSpecified = !aiiLine.US_QtyDiffRsn.IsEmpty ||
																	!aiiLine.US_QtyDiffRsnCode.IsEmpty ||
																	!aiiLine.US_InvQtyDisp.IsEmpty ||
																	!aiiLine.US_InvUQDisp.IsEmpty;
					#endregion
				}

				#endregion

				#region OGA

				if (!invoiceLine.US_DOTIndicator.IsEmpty)
				{
					usXsdInvoiceLine.OGAIndicators.DOTIndicator = OGAIndicatorToXmlCodeMappings.Instance.GetExternalCode(invoiceLine.US_DOTIndicator, "", context);
					usXsdInvoiceLine.OGAIndicators.DOTIndicatorSpecified = true;
				}
				if (!invoiceLine.US_FCCIndicator.IsEmpty)
				{
					usXsdInvoiceLine.OGAIndicators.FCCIndicator = OGAIndicatorToXmlCodeMappings.Instance.GetExternalCode(invoiceLine.US_FCCIndicator, "", context);
					usXsdInvoiceLine.OGAIndicators.FCCIndicatorSpecified = true;
				}
				if (!invoiceLine.US_FDAIndicator.IsEmpty)
				{
					usXsdInvoiceLine.OGAIndicators.FDAIndicator = OGAIndicatorToXmlCodeMappings.Instance.GetExternalCode(invoiceLine.US_FDAIndicator, "", context);
					usXsdInvoiceLine.OGAIndicators.FDAIndicatorSpecified = true;
				}
				usXsdInvoiceLine.OGAIndicators.IsSpecified = !invoiceLine.US_DOTIndicator.IsEmpty ||
															!invoiceLine.US_FCCIndicator.IsEmpty ||
															!invoiceLine.US_FDAIndicator.IsEmpty;

				usXsdInvoiceLine.DOTs = OGADataTransferTool.ExportDOTsDetails(invoiceLine.DOTs);
				usXsdInvoiceLine.FCCs = OGADataTransferTool.ExportFCCsDetails(invoiceLine.FCCs);
				usXsdInvoiceLine.FDAs = OGADataTransferTool.ExportFDAsDetails(invoiceLine.FDAs, context);

				#endregion

				usXsdInvoiceLine.CountryOfExport = invoiceLine.US_UC_NKCountryOfExport;
				usXsdInvoiceLine.CountryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
				usXsdInvoiceLine.DateOfExportFromCO = invoiceLine.US_DateOfExportFromCountryOfOrigin.Date;
				usXsdInvoiceLine.DestinationState = invoiceLine.US_DestinationState;
				if (!invoiceLine.US_TransactionsRelated.IsEmpty)
				{
					usXsdInvoiceLine.TransactionRelated = invoiceLine.US_TransactionsRelated == "Y";
					usXsdInvoiceLine.TransactionRelatedSpecified = true;
				}

				usXsdInvoiceLine.FTZ.PrivilegedStatusDate = invoiceLine.US_PrivilegedStatusDate.Date;
				usXsdInvoiceLine.FTZ.ZoneStatus = invoiceLine.US_ZoneStatus;
				usXsdInvoiceLine.FTZ.IsSpecified = !invoiceLine.US_PrivilegedStatusDate.IsEmpty || !invoiceLine.US_ZoneStatus.IsEmpty;

				usXsdInvoiceLine.HazardousMaterial.ClassificationDesc = invoiceLine.US_HazMatClassDesc;
				usXsdInvoiceLine.HazardousMaterial.Desc = invoiceLine.US_HazMatDesc;
				usXsdInvoiceLine.HazardousMaterial.IsSpecified = !invoiceLine.US_HazMatClassDesc.IsEmpty || !invoiceLine.US_HazMatDesc.IsEmpty;

				usXsdInvoiceLine.ManifestQty = Convert.ToDecimal(invoiceLine.US_ManifestQty);
				usXsdInvoiceLine.ManifestQtySpecified = true;

				#region Line Organisations

				OGADataTransferTool.ExportOrganisation(usXsdInvoiceLine.Manufacturer, invoiceLine.ManufacturerAddress, context);

				if (invoiceLine.ConsigneeOrgAddress != null)
				{
					var org = OrganisationDataAdapter.ExportToValueObject(invoiceLine.ConsigneeOrgAddress, context);
					usXsdInvoiceLine.UltimateConsignee.Item = org;
					usXsdInvoiceLine.UltimateConsignee.IsSpecified = true;
				}

				#endregion

				usXsdInvoiceLine.NAFTANet = invoiceLine.US_IsNAFTANet;
				usXsdInvoiceLine.NAFTANetSpecified = true;

				ExportPermitAndLicence(usXsdInvoiceLine, invoiceLine);

				usXsdInvoiceLine.SecondarySPI = invoiceLine.US_SecondarySPI;

				usXsdInvoiceLine.SelectedRateType = invoiceLine.US_SelectedRateType;
				usXsdInvoiceLine.SPI = invoiceLine.US_SPI;
				usXsdInvoiceLine.ThirdQty.DimensionType = invoiceLine.JI_CustomsThirdUnitQty;
				usXsdInvoiceLine.ThirdQty.Value = invoiceLine.JI_CustomsThirdQuantity;
				usXsdInvoiceLine.ThirdQty.IsSpecified = invoiceLine.JI_CustomsThirdQuantity > 0;

				usXsdInvoiceLine.TSCA.Indicator = invoiceLine.US_TSCAIndicator;
				usXsdInvoiceLine.TSCA.Name = invoiceLine.US_TSCAName;
				usXsdInvoiceLine.TSCA.IsSpecified = !invoiceLine.US_TSCAIndicator.IsEmpty || !invoiceLine.US_TSCAName.IsEmpty;
				ExportInvoiceLineRegistrationNumbers(usXsdInvoiceLine, aiiLine);
				usXsdInvoiceLine.LaceyActDetails = OGADataTransferTool.ExportLaceyActData(true, invoiceLine.LaceyActLines);
				ExportInvoiceLineOverridenTaxRate(usXsdInvoiceLine, invoiceLine, context);
				ExportInvoiceLineFees(usXsdInvoiceLine, invoiceLine);
				usXsdInvoiceLine.ExportLicense.IsSpecified = false;
				usXsdInvoiceLine.DDTCDetails.IsSpecified = false;
				usXsdInvoiceLine.VehicleDetails.IsSpecified = false;

				#region Supplementary

				usXsdInvoiceLine.Supplementary.Tariff = invoiceLine.US_SupTariff;
				usXsdInvoiceLine.Supplementary.Quantity1.Value = invoiceLine.US_SupQty1;
				usXsdInvoiceLine.Supplementary.Quantity1.DimensionType = invoiceLine.US_SupUQ1;
				usXsdInvoiceLine.Supplementary.Quantity1.IsSpecified = invoiceLine.US_SupQty1 > 0;

				usXsdInvoiceLine.Supplementary.Quantity2.Value = invoiceLine.US_SupQty2;
				usXsdInvoiceLine.Supplementary.Quantity2.DimensionType = invoiceLine.US_SupUQ2;
				usXsdInvoiceLine.Supplementary.Quantity2.IsSpecified = invoiceLine.US_SupQty2 > 0;

				usXsdInvoiceLine.Supplementary.Quantity3.Value = invoiceLine.US_SupQty3;
				usXsdInvoiceLine.Supplementary.Quantity3.DimensionType = invoiceLine.US_SupUQ3;
				usXsdInvoiceLine.Supplementary.Quantity3.IsSpecified = invoiceLine.US_SupQty3 > 0;

				usXsdInvoiceLine.Supplementary.IsSpecified = !usXsdInvoiceLine.Supplementary.Tariff.IsEmpty ||
									usXsdInvoiceLine.Supplementary.Quantity1.IsSpecified ||
									usXsdInvoiceLine.Supplementary.Quantity2.IsSpecified ||
									usXsdInvoiceLine.Supplementary.Quantity3.IsSpecified;
				#endregion
			}
			else
			{
				#region Export Shipment Type Related Data

				usXsdInvoiceLine.ExportLicense.ECCN = invoiceLine.US_ECCN;
				usXsdInvoiceLine.ExportLicense.Number = invoiceLine.US_LicenseNo;
				usXsdInvoiceLine.ExportLicense.Type = invoiceLine.US_LicenseType;

				usXsdInvoiceLine.ExportCode = invoiceLine.US_ExportCode;
				if (!invoiceLine.US_DDTCITARExemptionNo.IsEmpty)
				{
					usXsdInvoiceLine.DDTCDetails.ITARExemptionNumber = invoiceLine.US_DDTCITARExemptionNo;
				}
				if (!invoiceLine.US_DDTCMilitaryEquipmentIndicator.IsEmpty)
				{
					usXsdInvoiceLine.DDTCDetails.MilitaryEquipmentIndicator = invoiceLine.US_DDTCMilitaryEquipmentIndicator == YesNoDefaultList.Codes.Yes ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
					usXsdInvoiceLine.DDTCDetails.MilitaryEquipmentIndicatorSpecified = true;
				}
				if (!invoiceLine.US_DDTCPartyCertificationIndicator.IsEmpty)
				{
					usXsdInvoiceLine.DDTCDetails.PartyCertificationIndicator = invoiceLine.US_DDTCPartyCertificationIndicator == YesNoDefaultList.Codes.Yes ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
					usXsdInvoiceLine.DDTCDetails.PartyCertificationIndicatorSpecified = true;
				}
				usXsdInvoiceLine.DDTCDetails.Quantity.Value = invoiceLine.US_DDTCQuantity;
				usXsdInvoiceLine.DDTCDetails.Quantity.DimensionType = invoiceLine.US_DDTCUnit;
				usXsdInvoiceLine.DDTCDetails.RegistrationNumber = invoiceLine.US_DDTCRegistrationNo;
				if (!invoiceLine.US_DDTCUSMLCategoryCode.IsEmpty)
				{
					usXsdInvoiceLine.DDTCDetails.USMLCategoryCode = invoiceLine.US_DDTCUSMLCategoryCode;
				}
				usXsdInvoiceLine.OriginIndicator = AESOriginIndicatorToXmlCodeMappings.Instance.GetExternalCode(invoiceLine.US_AESOriginIndicator, "", context);

				if (invoiceLine.US_IsUsedVehicle)
				{
					usXsdInvoiceLine.VehicleDetails.UsedVehicle = true;
					usXsdInvoiceLine.VehicleDetails.UsedVehicleSpecified = true;
					usXsdInvoiceLine.VehicleDetails.ID = invoiceLine.US_VehicleID;
					usXsdInvoiceLine.VehicleDetails.IDType = invoiceLine.US_VehicleIDType;
					usXsdInvoiceLine.VehicleDetails.TitleNumber = invoiceLine.US_VehicleTitleNo;
					usXsdInvoiceLine.VehicleDetails.TitleState = invoiceLine.US_VehicleTitleState;
				}

				#endregion

				usXsdInvoiceLine.ArticleNo.IsSpecified = false;
				usXsdInvoiceLine.Textile.IsSpecified = false;
				usXsdInvoiceLine.HazardousMaterial.IsSpecified = false;
				usXsdInvoiceLine.DispatchedInvoiceQty.IsSpecified = false;
				usXsdInvoiceLine.BasisUnit.IsSpecified = false;
				usXsdInvoiceLine.FTZ.IsSpecified = false;
				usXsdInvoiceLine.TSCA.IsSpecified = false;
				usXsdInvoiceLine.Supplementary.IsSpecified = false;
			}

			usXsdInvoiceLine.SecondQty.DimensionType = invoiceLine.JI_CustomsSecondUnitQty;
			usXsdInvoiceLine.SecondQty.Value = invoiceLine.JI_CustomsSecondQuantity;
			usXsdInvoiceLine.SecondQty.IsSpecified = invoiceLine.JI_CustomsSecondQuantity > 0;
		}

		void ExportInvoiceLineOverridenTaxRate(Xsd.USInvoiceLine usXsdInvoiceLine, JobComInvoiceLine invoiceLine, IValueObjectExportContext context)
		{
			if (invoiceLine.US_TaxApply == TaxApplyList.Codes.Override)
			{
				usXsdInvoiceLine.TaxCode = USInvoiceLineTaxCodeMappings.Instance.GetExternalCode(invoiceLine.US_TaxCode, "", context);
				usXsdInvoiceLine.OverriddenTaxRate = invoiceLine.US_TaxRate;
			}

			if (!invoiceLine.US_TaxRateT.IsEmpty)
			{
				usXsdInvoiceLine.TaxRateType = USInvoiceLineTaxRateTypeMappings.Instance.GetExternalCode(invoiceLine.US_TaxRateT, "", context);
			}
		}

		void ExportAntidumpingCountervailing(Xsd.USInvoiceLine usXsdInvoiceLine, JobComInvoiceLine invoiceLine)
		{
			usXsdInvoiceLine.AntiDumping.Bonded = invoiceLine.US_IsBondedADD;
			usXsdInvoiceLine.AntiDumping.BondedSpecified = true;
			usXsdInvoiceLine.AntiDumping.CaseNo = invoiceLine.US_ADDCaseNo;
			usXsdInvoiceLine.AntiDumping.DepositRateIndicator = invoiceLine.US_ADDDepositRateIndicator;
			usXsdInvoiceLine.AntiDumping.DepositValue = invoiceLine.US_ADDDepositValue;
			usXsdInvoiceLine.AntiDumping.DepositValueSpecified = true;
			usXsdInvoiceLine.AntiDumping.IsSpecified = !invoiceLine.US_ADDCaseNo.IsEmpty;

			usXsdInvoiceLine.Countervailing.Bonded = invoiceLine.US_IsBondedCVD;
			usXsdInvoiceLine.Countervailing.BondedSpecified = true;
			usXsdInvoiceLine.Countervailing.CaseNo = invoiceLine.US_CVDCaseNo;
			usXsdInvoiceLine.Countervailing.DepositRateIndicator = invoiceLine.US_CVDDepositRateIndicator;
			usXsdInvoiceLine.Countervailing.DepositValue = invoiceLine.US_CVDDepositValue;
			usXsdInvoiceLine.Countervailing.DepositValueSpecified = true;
			usXsdInvoiceLine.Countervailing.IsSpecified = !invoiceLine.US_CVDCaseNo.IsEmpty;
		}

		void ExportPermitAndLicence(Xsd.USInvoiceLine usXsdInvoiceLine, JobComInvoiceLine invoiceLine)
		{
			usXsdInvoiceLine.PermitAndLicence.AgricultureLicNo = invoiceLine.US_AgricultureLicNo;
			usXsdInvoiceLine.PermitAndLicence.CottonCertificateNo = invoiceLine.US_CottonCertificateNo;
			usXsdInvoiceLine.PermitAndLicence.CottonFeeExemptIndicator = invoiceLine.US_CottonFeeExempt;
			usXsdInvoiceLine.PermitAndLicence.CAExportCertificate = invoiceLine.US_CAExportCertificate;
			usXsdInvoiceLine.PermitAndLicence.CBTPACertificate = invoiceLine.US_CBTPACertificateNo;
			usXsdInvoiceLine.PermitAndLicence.MiscPermitNo = invoiceLine.US_MiscPermitNo;
			usXsdInvoiceLine.PermitAndLicence.SWPMIndicator = invoiceLine.US_SWPMIndicator;
			usXsdInvoiceLine.PermitAndLicence.WoolLicenceNo = invoiceLine.US_WoolLicenceNo;

			bool permitAndLicenceSpecified = !invoiceLine.US_AgricultureLicNo.IsEmpty ||
											!invoiceLine.US_CottonCertificateNo.IsEmpty ||
											!invoiceLine.US_CottonFeeExempt.IsEmpty ||
											!invoiceLine.US_CAExportCertificate.IsEmpty ||
											!invoiceLine.US_CBTPACertificateNo.IsEmpty ||
											!invoiceLine.US_MiscPermitNo.IsEmpty ||
											!invoiceLine.US_SWPMIndicator.IsEmpty ||
											!invoiceLine.US_WoolLicenceNo.IsEmpty;

			usXsdInvoiceLine.PermitAndLicence.IsSpecified = permitAndLicenceSpecified;

			usXsdInvoiceLine.SoftwoodLumber.ExportCharge = invoiceLine.US_LumberExportCharges;
			usXsdInvoiceLine.SoftwoodLumber.ExportChargeSpecified = true;
			usXsdInvoiceLine.SoftwoodLumber.ExportPrice = invoiceLine.US_LumberExportPrice;
			usXsdInvoiceLine.SoftwoodLumber.ExportPriceSpecified = true;

			bool importerDeclaration = invoiceLine.US_LumberImporterDeclaration == "Y";
			usXsdInvoiceLine.SoftwoodLumber.ImporterDec = importerDeclaration;
			usXsdInvoiceLine.SoftwoodLumber.ImporterDecSpecified = true;

			usXsdInvoiceLine.SoftwoodLumber.IsSpecified = !invoiceLine.US_LumberExportCharges.IsEmpty ||
														!invoiceLine.US_LumberExportPrice.IsEmpty ||
														!invoiceLine.US_LumberImporterDeclaration.IsEmpty;

			usXsdInvoiceLine.Textile.CategoryNo = invoiceLine.US_TextileCategoryNo;
			usXsdInvoiceLine.Textile.VisaNo = invoiceLine.US_VisaNo;
			usXsdInvoiceLine.Textile.VisaQty = invoiceLine.US_VisaQty;
			usXsdInvoiceLine.Textile.VisaQtySpecified = true;
			usXsdInvoiceLine.Textile.VisaUQ = invoiceLine.US_VisaUQ;

			bool textileSpecified = !invoiceLine.US_TextileCategoryNo.IsEmpty ||
									!invoiceLine.US_VisaNo.IsEmpty ||
									!invoiceLine.US_VisaQty.IsEmpty;

			usXsdInvoiceLine.Textile.IsSpecified = textileSpecified;

			usXsdInvoiceLine.PIRPRuling.Number = invoiceLine.US_PIRPRulingNo;
			usXsdInvoiceLine.PIRPRuling.Type = invoiceLine.US_PIRPRulingType;
			usXsdInvoiceLine.PIRPRuling.IsSpecified = !invoiceLine.US_PIRPRulingNo.IsEmpty || !invoiceLine.US_PIRPRulingType.IsEmpty;
		}

		void ExportInvoiceLineRegistrationNumbers(Xsd.USInvoiceLine usXsdInvoiceLine, AIILine aiiLine)
		{
			if (aiiLine != null)
			{
				foreach (RegoNumber regNo in aiiLine.RegoNumbers)
				{
					Xsd.TypeNumber xmlRegNo = new Xsd.TypeNumber();
					xmlRegNo.Number = regNo.CY_Data;
					xmlRegNo.Type = regNo.CY_Code;
					usXsdInvoiceLine.RegistrationNumbers.Add(xmlRegNo);
				}
			}
		}

		void ExportInvoiceLineFees(Xsd.USInvoiceLine usXsdInvoiceLine, JobComInvoiceLine invoiceLine)
		{
			foreach (FeeCusCodeData fee in invoiceLine.FeeCusCodes)
			{
				Xsd.USInvoiceLineFee xmlFee = new Xsd.USInvoiceLineFee();
				xmlFee.Code = fee.CY_Code;
				xmlFee.FeeAmount = fee.CY_FeeAmount;
				xmlFee.FeeAmountSpecified = true;
				xmlFee.Overidden = fee.CY_IsOverridden ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlFee.OveriddenSpecified = true;
				xmlFee.RateType = fee.CY_SelectedRateType;
				usXsdInvoiceLine.Fees.Add(xmlFee);
			}
		}

		#endregion

		#region Related Objects

		OrganisationValueObjectDataAdapter OrganisationDataAdapter
		{
			get { return organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter()); }
		}
		OrganisationValueObjectDataAdapter organisationDataAdapter;

		public AdaptersHelper AdaptersHelper
		{
			get { return adaptersHelper ?? (adaptersHelper = new AdaptersHelper()); }
		}
		AdaptersHelper adaptersHelper;

		OGADataTransferTool OGADataTransferTool
		{
			get { return ogaDataTransferTool ?? (ogaDataTransferTool = new OGADataTransferTool()); }
		}
		OGADataTransferTool ogaDataTransferTool;

		#endregion
	}
}
