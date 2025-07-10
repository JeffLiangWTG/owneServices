using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoiceDataTransferTool
	{
		public InvoiceDataTransferTool(bool isStandAlone)
		{
			this.IsStandAlone = isStandAlone;
		}

		protected readonly bool IsStandAlone;

		#region Import

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name")]
		public virtual void ImportInvoiceDetails(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(invoiceHeader.JZ_InvoiceNumberInfo, xmlInvoiceHeader.InvoiceNumber, xmlInvoiceHeader.InvoiceNumberSpecified, (NoResString)"Invoice Number");

			if (xmlInvoiceHeader.Consignor.IsSpecified)
			{
				invoiceHeader.JZ_OH_Supplier = GetMatchedOrganisation(xmlInvoiceHeader.Consignor, context);
			}

			if (xmlInvoiceHeader.Consignee.IsSpecified)
			{
				invoiceHeader.JZ_OH_Buyer = GetMatchedOrganisation(xmlInvoiceHeader.Consignee, context);
			}

			//Should be done after setting supplier and buyer to avoid possible data change
			if (!xmlInvoiceHeader.StandAloneInvoiceDirection.IsEmpty && IsStandAlone)
			{
				context.SetPropertyInfoValue(invoiceHeader.JZ_MessageTypeInfo, xmlInvoiceHeader.StandAloneInvoiceDirection, (NoResString)"Invoice Direction");
			}

			if (xmlInvoiceHeader.InvoiceAmount != null)
			{
				invoiceHeader.JZ_InvoiceAmount = xmlInvoiceHeader.InvoiceAmount.Value;
				context.SetPropertyInfoValue(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, xmlInvoiceHeader.InvoiceAmount.CurrencyCode, ForeignKeyType.CurrencyNK, "Invoice Currency");
			}

			if (xmlInvoiceHeader.InvoiceDate.IsValid)
			{
				invoiceHeader.JZ_InvoiceDate = (ZDateTime)xmlInvoiceHeader.InvoiceDate;
			}

			if (xmlInvoiceHeader.ValuationDate.IsValid)
			{
				invoiceHeader.JZ_ValuationDateOverride = xmlInvoiceHeader.ValuationDate;
			}

			if (xmlInvoiceHeader.IncotermSpecified)
			{
				context.SetPropertyInfoValue(invoiceHeader.JZ_IncoTermInfo, xmlInvoiceHeader.Incoterm, ForeignKeyType.IncoTermNK);
			}

			if (xmlInvoiceHeader.Weight != null)
			{
				invoiceHeader.JZ_Weight = xmlInvoiceHeader.Weight.Value;
				context.SetPropertyInfoValue(invoiceHeader.JZ_WeightUQInfo, xmlInvoiceHeader.Weight.DimensionType, xmlInvoiceHeader.Weight.DimensionTypeSpecified, "Invoice Weight");
			}

			if (xmlInvoiceHeader.Volume != null)
			{
				invoiceHeader.JZ_Volume = xmlInvoiceHeader.Volume.Value;
				context.SetPropertyInfoValue(invoiceHeader.JZ_VolumeUQInfo, xmlInvoiceHeader.Volume.DimensionType, xmlInvoiceHeader.Volume.DimensionTypeSpecified, "Invoice Volume");
			}

			if (!IsStandAlone)
			{
				if (xmlInvoiceHeader.PackagesSpecified)
				{
					invoiceHeader.JZ_NoOfPacks = xmlInvoiceHeader.Packages;
				}

				LinkBillAndGroupHeader(invoiceHeader, xmlInvoiceHeader, context);
			}

			ImportInvoiceHeaderAdditionalInfo(invoiceHeader, xmlInvoiceHeader.AddCustomsDetails, context);
			ImportInvoiceLinesDetail(xmlInvoiceHeader.InvoiceLines, invoiceHeader, context);
			ImportInvoiceHeaderReferences(xmlInvoiceHeader.References, invoiceHeader);
		}

		void LinkBillAndGroupHeader(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			Bill bill = invoiceHeader.JobDeclaration.Bills.FindAnyBillWithHouseBillMasterBillCombination(xmlInvoiceHeader.PackingDetails.Housebill, xmlInvoiceHeader.PackingDetails.Masterbill);
			if (bill != null)
			{
				invoiceHeader.JZ_CU_RelatedHouseBill = bill.PK;
			}

			ZQuery filter = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.Equal, invoiceHeader.JZ_JE);
			filter.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_InvoiceNumber, SQLComparisonOperator.Equal, xmlInvoiceHeader.RelatedGroupInvoiceNumber);
			filter.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_GroupInvoice, SQLComparisonOperator.Equal, "Y");
			var groupHeader = invoiceHeader.Factory.LoadTop1<BaseJobComInvoiceGroupHeader>(filter);
			if (groupHeader != null && groupHeader.PK != invoiceHeader.PK)
			{
				invoiceHeader.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
			}
		}

		public virtual ZGuid GetMatchedOrganisation(Xsd.Organisation organisation, IValueObjectImportContext context)
		{
			return context.FindOrCreateTempOrganisationPK(organisation, null, OrganisationTypes.None);
		}

		public void ImportInvoiceLinesDetail(Xsd.InvoiceLineCollection invoiceLines, BaseJobComInvoiceHeader invoiceHeader, IValueObjectImportContext context)
		{
			if (invoiceLines != null)
			{
				using (invoiceHeader.GetLineNumberRenumberingSuspender())
				{
					ZShort lineNumber = 1;

					foreach (Xsd.InvoiceLine invoiceLine in invoiceLines)
					{
						ZShort effectiveLineNo = lineNumber;

						if (!invoiceLine.InvoiceLineNumber.IsEmpty)
						{
							effectiveLineNo = ZShort.ParseSafe(invoiceLine.InvoiceLineNumber, lineNumber);
						}

						var newInvoiceLine = invoiceHeader.JobComInvoiceLines.GetByLineNo(effectiveLineNo)
							?? invoiceHeader.JobComInvoiceLines.AddNew();

						newInvoiceLine.SuspendValidation();
						newInvoiceLine.JI_LineNo = effectiveLineNo;
						ImportInvoiceLineDetail(newInvoiceLine, invoiceLine, context);
						SetParentForInvoiceLine(invoiceLine, newInvoiceLine, invoiceHeader);

						lineNumber++;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name")]
		protected virtual void ImportInvoiceLineDetail(BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLine xsdInvoiceLine, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notification = context;

			if (xsdInvoiceLine.NetWeight != null)
			{
				invoiceLine.JI_NetWeight = xsdInvoiceLine.NetWeight.Value;
				context.SetPropertyInfoValue(invoiceLine.JI_NetWeightUQInfo, xsdInvoiceLine.NetWeight.DimensionType, xsdInvoiceLine.NetWeight.DimensionTypeSpecified, "Invoice Line Net Weight");
			}

			SetProductNumber(context, invoiceLine, xsdInvoiceLine);

			ImportLineClassification(invoiceLine, xsdInvoiceLine.LineClassification, context);

			//Addinfo is set after the part No  and classification are set to avoid data change
			if (xsdInvoiceLine.LineClassification != null)
			{
				ImportInvoiceLinesAdditionalInfo(invoiceLine, xsdInvoiceLine.LineClassification.AddCustomsDetails, context);
			}

			if (!xsdInvoiceLine.ProductDescription.IsEmpty)
			{
				context.SetPropertyInfoValue(invoiceLine.JI_DescriptionInfo, xsdInvoiceLine.ProductDescription, xsdInvoiceLine.ProductDescriptionSpecified, "Invoice Line Product Description");
			}

			if (!xsdInvoiceLine.ExtendedProductDescription.IsEmpty && invoiceLine.IsExtendedCommercialDescriptionEnabled)
			{
				context.SetPropertyInfoValue(invoiceLine.JI_ExtraInfoForClassificationInfo, xsdInvoiceLine.ExtendedProductDescription, xsdInvoiceLine.ExtendedProductDescriptionSpecified, "Invoice Line Extended Product Description");
			}

			if (xsdInvoiceLine.InvoiceQty != null)
			{
				invoiceLine.JI_InvoiceQuantity = xsdInvoiceLine.InvoiceQty.Value;
				if (!xsdInvoiceLine.InvoiceQty.DimensionType.IsEmpty)
				{
					context.SetPropertyInfoValue(invoiceLine.JI_InvoiceUQInfo, xsdInvoiceLine.InvoiceQty.DimensionType, xsdInvoiceLine.InvoiceQty.DimensionTypeSpecified, "Invoice Line Quantity");
				}
			}

			if (xsdInvoiceLine.LinePrice != null)
			{
				invoiceLine.JI_LinePrice = xsdInvoiceLine.LinePrice.Value;
			}

			if (!invoiceLine.JI_CustomsUnitQty.IsEmpty && xsdInvoiceLine.CustomsInvoiceQty.Value != 0m)
			{
				invoiceLine.JI_CustomsQuantity = xsdInvoiceLine.CustomsInvoiceQty.Value;
			}
			context.SetPropertyInfoValue(invoiceLine.JI_OrderNumberInfo, xsdInvoiceLine.OrderNumber, xsdInvoiceLine.OrderNumberSpecified, "Invoice Line Order Number");

			if (xsdInvoiceLine.Volume != null)
			{
				invoiceLine.JI_Volume = xsdInvoiceLine.Volume.Value;
				context.SetPropertyInfoValue(invoiceLine.JI_VolumeUQInfo, xsdInvoiceLine.Volume.DimensionType, xsdInvoiceLine.Volume.DimensionTypeSpecified, "Invoice Line Volume");
			}

			if (xsdInvoiceLine.Weight != null)
			{
				invoiceLine.JI_Weight = xsdInvoiceLine.Weight.Value;
				context.SetPropertyInfoValue(invoiceLine.JI_WeightUQInfo, xsdInvoiceLine.Weight.DimensionType, xsdInvoiceLine.Weight.DimensionTypeSpecified, "Invoice Line Weight");
			}

			context.SetPropertyInfoValue(invoiceLine.JI_CustomAttrib1Info, xsdInvoiceLine.CustomText1, xsdInvoiceLine.CustomText1Specified, "Invoice Line Custom Text 1");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomAttrib2Info, xsdInvoiceLine.CustomText2, xsdInvoiceLine.CustomText2Specified, "Invoice Line Custom Text 2");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomAttrib3Info, xsdInvoiceLine.CustomText3, xsdInvoiceLine.CustomText3Specified, "Invoice Line Custom Text 3");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomAttrib4Info, xsdInvoiceLine.CustomText4, xsdInvoiceLine.CustomText4Specified, "Invoice Line Custom Text 4");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomAttrib5Info, xsdInvoiceLine.CustomText5, xsdInvoiceLine.CustomText5Specified, "Invoice Line Custom Text 5");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomAttrib6Info, xsdInvoiceLine.CustomText6, xsdInvoiceLine.CustomText6Specified, "Invoice Line Custom Text 6");

			context.SetPropertyInfoValue(invoiceLine.JI_CustomTextBlob1Info, xsdInvoiceLine.CustomTextField1, xsdInvoiceLine.CustomTextField1Specified, "Invoice Line Custom Text Field 1");

			context.SetPropertyInfoValue(invoiceLine.JI_CustomFlag1Info, xsdInvoiceLine.CustomFlag1.ToString(), xsdInvoiceLine.CustomFlag1Specified, "Invoice Line Custom Flag 1");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomFlag2Info, xsdInvoiceLine.CustomFlag2.ToString(), xsdInvoiceLine.CustomFlag2Specified, "Invoice Line Custom Flag 2");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomFlag3Info, xsdInvoiceLine.CustomFlag3.ToString(), xsdInvoiceLine.CustomFlag3Specified, "Invoice Line Custom Flag 3");

			if (xsdInvoiceLine.CustomDate1.IsValid && !xsdInvoiceLine.CustomDate1.IsEmpty)
			{
				context.SetPropertyInfoValue(invoiceLine.JI_CustomDate1Info, xsdInvoiceLine.CustomDate1.ToDateTime());
			}
			if (xsdInvoiceLine.CustomDate2.IsValid && !xsdInvoiceLine.CustomDate2.IsEmpty)
			{
				context.SetPropertyInfoValue(invoiceLine.JI_CustomDate2Info, xsdInvoiceLine.CustomDate2.ToDateTime());
			}
			if (xsdInvoiceLine.CustomDate3.IsValid && !xsdInvoiceLine.CustomDate3.IsEmpty)
			{
				context.SetPropertyInfoValue(invoiceLine.JI_CustomDate3Info, xsdInvoiceLine.CustomDate3.ToDateTime());
			}

			context.SetPropertyInfoValue(invoiceLine.JI_CustomDecimal1Info, xsdInvoiceLine.CustomDecimal1.ToString(), xsdInvoiceLine.CustomDecimal1Specified, "Invoice Line Custom Decimal 1");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomDecimal2Info, xsdInvoiceLine.CustomDecimal2.ToString(), xsdInvoiceLine.CustomDecimal2Specified, "Invoice Line Custom Decimal 2");
			context.SetPropertyInfoValue(invoiceLine.JI_CustomDecimal3Info, xsdInvoiceLine.CustomDecimal3.ToString(), xsdInvoiceLine.CustomDecimal3Specified, "Invoice Line Custom Decimal 3");

			context.SetPropertyInfoValue(invoiceLine.JI_PartAttrib1Info, xsdInvoiceLine.PartAttrib1, xsdInvoiceLine.PartAttrib1Specified, "Invoice Line Part Attrib 1");
			context.SetPropertyInfoValue(invoiceLine.JI_PartAttrib2Info, xsdInvoiceLine.PartAttrib2, xsdInvoiceLine.PartAttrib2Specified, "Invoice Line Part Attrib 2");
			context.SetPropertyInfoValue(invoiceLine.JI_PartAttrib3Info, xsdInvoiceLine.PartAttrib3, xsdInvoiceLine.PartAttrib3Specified, "Invoice Line Part Attrib 3");

			ImportInvoiceLineChargesDetails(xsdInvoiceLine.Charges, invoiceLine.Charges, context);

			if (!IsStandAlone)
			{
				ImportContainerDetailsForInvoiceLine(invoiceLine, xsdInvoiceLine, context);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name")]
		protected virtual void SetProductNumber(IValueObjectImportContext context, BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLine xsdInvoiceLine)
		{
			context.SetPropertyInfoValue(invoiceLine.JI_PartNoInfo, xsdInvoiceLine.ProductNumber, xsdInvoiceLine.ProductNumberSpecified, "Invoice Line Part Number");
		}

		protected void ImportContainerDetailsForInvoiceLine(BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLine xsdInvoiceLine, IValueObjectImportContext context)
		{
			if (xsdInvoiceLine.ContainerNumbers != null)
			{
				BaseJobDeclaration declaration = invoiceLine.Declaration;

				if (declaration != null)
				{
					foreach (ZString containerNumber in xsdInvoiceLine.ContainerNumbers)
					{
						BaseCusContainer cusContainer = declaration.CusContainers.Find(containerNumber);
						if (cusContainer != null)
						{
							if (!invoiceLine.ContainersPivot.Contains(cusContainer))
							{
								invoiceLine.ContainersPivot.AddPivotFor(cusContainer);
							}
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name")]
		protected void ImportInvoiceLineChargesDetails(Xsd.InvoiceChargeCollection xsdCharges, IJobComInvChargeCollection<BaseInvoiceLineCharge> invLineCharges, IValueObjectImportContext context)
		{
			if (xsdCharges != null)
			{
				foreach (Xsd.InvoiceCharge xsdCharge in xsdCharges)
				{
					BaseInvoiceLineCharge invLineCharge = invLineCharges.AddNew();

					context.SetPropertyInfoValue(invLineCharge.J7_ChargeTypeInfo, xsdCharge.ChargeType, xsdCharge.ChargeTypeSpecified);
					if (xsdCharge.ChargeValue != null)
					{
						invLineCharge.J7_Amount = xsdCharge.ChargeValue.Value;
						context.SetPropertyInfoValue(invLineCharge.J7_RX_NKCurrencyInfo, xsdCharge.ChargeValue.CurrencyCode, xsdCharge.ChargeValue.CurrencyCodeSpecified, "Invoice Charge Currency");
					}

					if (xsdCharge.DutyAppliesSpecified)
					{
						invLineCharge.J7_IsDutiable = (xsdCharge.DutyApplies == Xsd.TrueFalse.@true);
					}

					if (xsdCharge.GstAppliesSpecified)
					{
						invLineCharge.J7_IsGSTApplicable = (xsdCharge.GstApplies == Xsd.TrueFalse.@true);
					}

					if (xsdCharge.IsIncludedInTotalSpecified)
					{
						invLineCharge.J7_IsIncludedInITOT = (xsdCharge.IsIncludedInTotal == Xsd.TrueFalse.@true);
					}

					if (xsdCharge.IsIncludedInInvoiceSpecified)
					{
						invLineCharge.J7_IsNotIncludedInInvoice = (xsdCharge.IsIncludedInInvoice == Xsd.TrueFalse.@false);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name")]
		protected virtual void ImportLineClassification(BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLineLineClassification lineClassification, IValueObjectImportContext context)
		{
			if (lineClassification != null)
			{
				INotifications notification = context;
				context.SetPropertyInfoValue(invoiceLine.JI_CountryOfOriginInfo, lineClassification.OriginOfGoods, ForeignKeyType.CountryNK);

				ZString tariffLookup = lineClassification.TariffLookup;

				if (!tariffLookup.IsEmpty)
				{
					BaseCusClassification classification = BaseCusClassification.LoadFromLookupCode(invoiceLine.Factory, tariffLookup, GetClassificationTypeMatching(invoiceLine.InvoiceHeader), GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					if (classification != null)
					{
						invoiceLine.JI_CC = classification.PK;
					}
				}
				if (!lineClassification.TariffCode.Value.IsEmpty)
				{
					context.SetPropertyInfoValue(invoiceLine.JI_TariffInfo, lineClassification.TariffCode.Value, lineClassification.TariffCode.IsSpecified, "Invoice Line Tariff Code");
				}
			}
		}

		protected virtual string GetClassificationTypeMatching(BaseJobComInvoiceHeader invoice)
		{
			bool isImportJob = IsStandAlone ? invoice != null && invoice.IsImport : invoice != null && invoice.JobDeclaration != null && invoice.JobDeclaration.IsImport;

			return isImportJob ? BaseCusClassification.ClassificationType.IMP : BaseCusClassification.ClassificationType.EXP;
		}

		protected virtual void ImportInvoiceLinesAdditionalInfo(BaseJobComInvoiceLine invoiceLine, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
		}

		protected virtual void ImportInvoiceHeaderAdditionalInfo(BaseJobComInvoiceHeader invoiceHeader, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
		}

		protected void SetParentForInvoiceLine(Xsd.InvoiceLine invoiceLine, BaseJobComInvoiceLine newInvoiceLine, BaseJobComInvoiceHeader invoiceHeader)
		{
			if (invoiceLine.ParentInvoiceLineNumberSpecified)
			{
				BaseJobComInvoiceLine parentInvoiceLine = invoiceHeader.JobComInvoiceLines.GetByLineNo(invoiceLine.ParentInvoiceLineNumber);

				if (parentInvoiceLine == null)
				{
					parentInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					parentInvoiceLine.JI_LineNo = invoiceLine.ParentInvoiceLineNumber;
				}
				newInvoiceLine.JI_ParentID = parentInvoiceLine.PK;
			}
		}

		protected void ImportInvoiceHeaderReferences(Xsd.InvoiceReferenceCollection references, BaseJobComInvoiceHeader invoiceHeader)
		{
			foreach (Xsd.InvoiceReference xmlReference in references)
			{
				JobComInvoiceHeaderRefs reference = invoiceHeader.InvoiceHeaderRefs.AddNew();
				reference.J2_ReferenceType = xmlReference.Type.Left(reference.J2_ReferenceTypeInfo.MaxLength);
				reference.J2_ReferenceNumber = xmlReference.Value.Left(reference.J2_ReferenceNumberInfo.MaxLength);
			}
		}

		#endregion

		#region Export

		public virtual void ExportInvoiceHeaderValues(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectExportContext context)
		{
			xmlInvoiceHeader.Consignor = new OrganisationValueObjectDataAdapter().ExportToValueObject(invoiceHeader.Supplier, context);
			xmlInvoiceHeader.Consignee = new OrganisationValueObjectDataAdapter().ExportToValueObject(invoiceHeader.Buyer, context);

			if (IsStandAlone)
			{
				xmlInvoiceHeader.StandAloneInvoiceDirection = invoiceHeader.JZ_MessageType;
			}

			xmlInvoiceHeader.InvoiceNumber = invoiceHeader.JZ_InvoiceNumber;
			xmlInvoiceHeader.InvoiceAmount = Xsd.FinancialValue.FromAmountAndCurrency(invoiceHeader.JZ_InvoiceAmount, invoiceHeader.Invoice_Currency);
			xmlInvoiceHeader.ExchangeRate = invoiceHeader.JZ_InvoiceCurrExRate;
			xmlInvoiceHeader.ExchangeRateSpecified = true;

			if (invoiceHeader.JZ_InvoiceDate.IsValid)
			{
				xmlInvoiceHeader.InvoiceDate = (ZDate)invoiceHeader.JZ_InvoiceDate;
			}

			if (invoiceHeader.JZ_ValuationDateOverride.IsValid)
			{
				xmlInvoiceHeader.ValuationDate = invoiceHeader.JZ_ValuationDateOverride.ToDateTime();
			}

			xmlInvoiceHeader.IsGroupInvoice = Xsd.TrueFalse.@false;
			xmlInvoiceHeader.IsGroupInvoiceSpecified = true;

			xmlInvoiceHeader.Incoterm = invoiceHeader.JZ_IncoTerm;
			xmlInvoiceHeader.Weight = Xsd.DimensionValue.FromAmountAndUnit(invoiceHeader.JZ_Weight, invoiceHeader.JZ_WeightUQ);
			xmlInvoiceHeader.Volume = Xsd.DimensionValue.FromAmountAndUnit(invoiceHeader.JZ_Volume, invoiceHeader.JZ_VolumeUQ);

			if (!IsStandAlone)
			{
				xmlInvoiceHeader.Packages = invoiceHeader.JZ_NoOfPacks;
			}

			xmlInvoiceHeader.InvoiceLines = ExportInvoiceLines(invoiceHeader, context);

			xmlInvoiceHeader.AddCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();
			ExportInvoiceHeaderAdditionalInfo(xmlInvoiceHeader.AddCustomsDetails, invoiceHeader, context);

			if (!IsStandAlone)
			{
				ExportBillAndGroupHeaderForInvoice(invoiceHeader, xmlInvoiceHeader, context);
			}

			ExportInvoiceHeaderReferences(xmlInvoiceHeader.References, invoiceHeader.InvoiceHeaderRefs);
		}

		protected void ExportBillAndGroupHeaderForInvoice(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, INotifications notify)
		{
			if (invoiceHeader.Bill != null)
			{
				if (invoiceHeader.Bill.CU_BillType == Customs.Business.BillTypeList.Codes.HouseBill)
				{
					xmlInvoiceHeader.PackingDetails.Housebill = invoiceHeader.Bill.CU_HouseBill;
				}
				xmlInvoiceHeader.PackingDetails.Masterbill = invoiceHeader.Bill.CU_MasterBill;
			}

			if (invoiceHeader.GroupHeader != null && !invoiceHeader.GroupHeader.JZ_InvoiceNumber.IsEmpty)
			{
				xmlInvoiceHeader.RelatedGroupInvoiceNumber = invoiceHeader.GroupHeader.JZ_InvoiceNumber;
			}
		}

		protected Xsd.InvoiceLineCollection ExportInvoiceLines(BaseJobComInvoiceHeader invoice, INotifications notify)
		{
			Xsd.InvoiceLineCollection xmlInvoiceLines = new Xsd.InvoiceLineCollection();
			if (invoice.JobComInvoiceLines != null && invoice.JobComInvoiceLines.Count > 0)
			{
				foreach (BaseJobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
				{
					Xsd.InvoiceLine newInvoiceLine = xmlInvoiceLines.AddNew();
					ExportInvoiceLineDetails(newInvoiceLine, invoiceLine, notify);
				}
			}

			return xmlInvoiceLines;
		}

		public virtual void ExportInvoiceLineDetails(Xsd.InvoiceLine newInvoiceLine, BaseJobComInvoiceLine invoiceLine, INotifications notify)
		{
			newInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();

			BaseCusClassification classification = invoiceLine.Classification;
			if (classification != null)
			{
				newInvoiceLine.LineClassification.TariffLookup = classification.CC_LookupCode;
			}
			newInvoiceLine.LineClassification.OriginOfGoods = invoiceLine.JI_CountryOfOrigin;
			newInvoiceLine.LineClassification.TariffCode.Value = invoiceLine.JI_Tariff;
			newInvoiceLine.LineClassification.TariffCode.Description = GetTariffDescription(invoiceLine);

			newInvoiceLine.InvoiceLineNumber = invoiceLine.JI_LineNo.ToString();
			ExportParentLineNumber(invoiceLine.JI_ParentID, newInvoiceLine, invoiceLine.Factory);

			newInvoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(invoiceLine.JI_InvoiceQuantity, invoiceLine.JI_InvoiceUQ);
			newInvoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrency(invoiceLine.JI_LinePrice, invoiceLine.LinePriceRefCurrency);
			newInvoiceLine.ProductNumber = invoiceLine.JI_PartNo;
			newInvoiceLine.ProductDescription = invoiceLine.JI_Description;
			if (invoiceLine.IsExtendedCommercialDescriptionEnabled)
			{
				newInvoiceLine.ExtendedProductDescription = invoiceLine.JI_ExtraInfoForClassification;
			}

			newInvoiceLine.CustomsInvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty);
			newInvoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(invoiceLine.JI_InvoiceQuantity, invoiceLine.JI_InvoiceUQ);

			newInvoiceLine.OrderNumber = invoiceLine.JI_OrderNumber;
			newInvoiceLine.Volume = Xsd.DimensionValue.FromAmountAndUnit(invoiceLine.JI_Volume, invoiceLine.JI_VolumeUQ);
			newInvoiceLine.Weight = Xsd.DimensionValue.FromAmountAndUnit(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ);
			newInvoiceLine.NetWeight = Xsd.DimensionValue.FromAmountAndUnit(invoiceLine.JI_NetWeight, invoiceLine.JI_NetWeightUQ);
			newInvoiceLine.CustomText1 = invoiceLine.JI_CustomAttrib1;
			newInvoiceLine.CustomText2 = invoiceLine.JI_CustomAttrib2;
			newInvoiceLine.CustomText3 = invoiceLine.JI_CustomAttrib3;
			newInvoiceLine.CustomText4 = invoiceLine.JI_CustomAttrib4;
			newInvoiceLine.CustomText5 = invoiceLine.JI_CustomAttrib5;
			newInvoiceLine.CustomText6 = invoiceLine.JI_CustomAttrib6;
			newInvoiceLine.CustomTextField1 = invoiceLine.JI_CustomTextBlob1;
			newInvoiceLine.CustomFlag1 = invoiceLine.JI_CustomFlag1;
			newInvoiceLine.CustomFlag2 = invoiceLine.JI_CustomFlag2;
			newInvoiceLine.CustomFlag3 = invoiceLine.JI_CustomFlag3;
			newInvoiceLine.CustomDate1 = invoiceLine.JI_CustomDate1;
			newInvoiceLine.CustomDate2 = invoiceLine.JI_CustomDate2;
			newInvoiceLine.CustomDate3 = invoiceLine.JI_CustomDate3;
			newInvoiceLine.CustomDecimal1 = invoiceLine.JI_CustomDecimal1;
			newInvoiceLine.CustomDecimal2 = invoiceLine.JI_CustomDecimal2;
			newInvoiceLine.CustomDecimal3 = invoiceLine.JI_CustomDecimal3;
			newInvoiceLine.PartAttrib1 = invoiceLine.JI_PartAttrib1;
			newInvoiceLine.PartAttrib2 = invoiceLine.JI_PartAttrib2;
			newInvoiceLine.PartAttrib3 = invoiceLine.JI_PartAttrib3;
			ExportInvoiceLineSummary(newInvoiceLine.Summary, invoiceLine);

			newInvoiceLine.LineClassification.AddCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();
			ExportInvoiceLinesAdditionalInfo(newInvoiceLine.LineClassification.AddCustomsDetails, invoiceLine, new ValueObjectExportContext(notify));
			ExportInvoiceChargesDetails(newInvoiceLine, invoiceLine.Charges, notify);

			if (!IsStandAlone)
			{
				ExportInvoiceLineContainerDetails(newInvoiceLine, invoiceLine.ContainersPivot, notify);
				ExportLandedCostingDetails(newInvoiceLine.LandedCosting, invoiceLine, notify);
			}
		}

		protected virtual ZString GetTariffDescription(BaseJobComInvoiceLine invoiceLine)
		{
			return invoiceLine.TariffDescription;
		}

		protected void ExportLandedCostingDetails(Xsd.LandedCostingInfo landedCostingXml, BaseJobComInvoiceLine invoiceLine, INotifications notify)
		{
			BusinessObject landedCostingHeader = GetLandedCostingHeader(invoiceLine);
			BaseJobDeclaration declaration = invoiceLine.Declaration;

			if (declaration != null && declaration.IsImport && landedCostingHeader != null)
			{
				ZQuery queryForLandedCostingHistory = new ZQuery(LandedCostHistorySchema.LH_LT, landedCostingHeader.PK);
				queryForLandedCostingHistory.AddToFilter(LandedCostHistorySchema.LH_ParentID, invoiceLine.PK);
				queryForLandedCostingHistory.AddToFilter(LandedCostHistorySchema.LH_ParentTableCode, JobComInvoiceLineSchema.Constants.Prefix);

				var landedCostingHistory = invoiceLine.Factory.LoadTop1<Integration.LandedCosting.ILandedCostHistory>(queryForLandedCostingHistory);

				if (landedCostingHistory != null)
				{
					landedCostingXml.LineType = (ZString)landedCostingHistory[LandedCostHistorySchema.LH_LandedCostHistoryLineType.Name];

					var localCurrencyCode = declaration.LocalCurrencyCode;
					SetAmountAndCurrencyIfNotZero(((ZDecimal)landedCostingHistory[LandedCostPropertyNames.UnitPriceInLocalCurrency]), landedCostingXml.UnitPriceInLocalCurrency, localCurrencyCode);
					SetAmountAndCurrencyIfNotZero(((ZDecimal)landedCostingHistory[LandedCostPropertyNames.RoundedPerUnitCustomsDisbursementCharges]), landedCostingXml.DutiesAndTaxesPerUnit, localCurrencyCode);
					SetAmountAndCurrencyIfNotZero(((ZDecimal)landedCostingHistory[LandedCostPropertyNames.RoundedPerUnitLandingCost]), landedCostingXml.LandingCostPerUnit, localCurrencyCode);
					SetAmountAndCurrencyIfNotZero(((ZDecimal)landedCostingHistory[LandedCostPropertyNames.RoundedPerUnitTotalCost]), landedCostingXml.TotalCostPerUnit, localCurrencyCode);
					SetAmountAndCurrencyIfNotZero(landedCostingHistory.GetRoundedLineValue(LandedLineCostTypes.EntryFees), landedCostingXml.EntryFees, localCurrencyCode);
					SetAmountAndCurrencyIfNotZero(landedCostingHistory.GetRoundedLineValue(LandedLineCostTypes.Excise), landedCostingXml.Excise, localCurrencyCode);

					SetSpecialTaxDetails(landedCostingHistory, landedCostingXml, localCurrencyCode);
					SetSellDetails(landedCostingHistory, landedCostingXml, localCurrencyCode);
					SetGroupChargesDetails(landedCostingHistory, landedCostingXml, localCurrencyCode);
				}
			}
			else
			{
				landedCostingXml.IsSpecified = false;
			}
		}

		protected void ExportInvoiceLineContainerDetails(Xsd.InvoiceLine xmlInvLine, CusContainersInvoiceLinesCollection invLineContainers, INotifications notify)
		{
			if (invLineContainers.Count > 0)
			{
				string[] containerNumbers = new string[invLineContainers.Count];
				for (int i = 0; i < containerNumbers.Length; i++)
				{
					containerNumbers[i] = invLineContainers[i].Container.CO_ContainerNumber;
				}
				xmlInvLine.ContainerNumbers = containerNumbers;
			}
		}

		void SetGroupChargesDetails(Integration.LandedCosting.ILandedCostHistory landedCostingHistory, Xsd.LandedCostingInfo xmlLandedCosting, ZString localCurrencyCode)
		{
			ZString landedCostGroupPropertyName = LandedCostPropertyNames.LH_LandedCostGroup;

			for (int i = 1; i <= 6; i++)
			{
				ZDecimal grpChargeAmount = (ZDecimal)landedCostingHistory[landedCostGroupPropertyName + i.ToString(CultureInfo.InvariantCulture)];
				if (grpChargeAmount != 0)
				{
					Xsd.LandedCostGroupCharge groupCharge = xmlLandedCosting.LCGroupCharges.AddNew();
					groupCharge.Id = i;
					groupCharge.ChargesAmount.Value = grpChargeAmount;
					groupCharge.ChargesAmount.CurrencyCode = localCurrencyCode;
				}
			}
		}

		void SetSpecialTaxDetails(Integration.LandedCosting.ILandedCostHistory landedCostingHistory, Xsd.LandedCostingInfo xmlLandedCosting, ZString localCurrencyCode)
		{
			ZString specialTaxTypePrefix = LandedLineCostTypes.SpecialTaxPrefix;

			for (int i = 1; i <= 3; i++)
			{
				ZDecimal specialTax = landedCostingHistory.GetRoundedLineValue(specialTaxTypePrefix + i.ToString(CultureInfo.InvariantCulture));
				if (specialTax != 0)
				{
					Xsd.SpecialTax specialTaxXml = xmlLandedCosting.SpecialTax.AddNew();
					specialTaxXml.Sequence = i;
					specialTaxXml.TaxAmount.Value = specialTax;
					specialTaxXml.TaxAmount.CurrencyCode = localCurrencyCode;
				}
			}
		}

		void SetSellDetails(Integration.LandedCosting.ILandedCostHistory landedCostingHistory, Xsd.LandedCostingInfo xmlLandedCosting, ZString localCurrencyCode)
		{
			ZString markUpPercentagePropertyName = LandedCostPropertyNames.LH_LandedCostMarginPercent;
			ZString effectiveMarkUpPercentagePropertyName = LandedCostPropertyNames.EffectiveMarkUpPercentage;
			ZString sellPricePropertyName = LandedCostPropertyNames.RoundedSellPrice;
			ZString exGST = LandedCostPropertyNames.ExGST;
			ZString incGST = LandedCostPropertyNames.IncGST;

			for (int i = 1; i <= 3; i++)
			{
				Xsd.LandedCostSellDetails sellDetails = xmlLandedCosting.SellDetails.AddNew();
				sellDetails.Sequence = i;

				sellDetails.SellPriceExGST.Value = (ZDecimal)landedCostingHistory[sellPricePropertyName + i.ToString(CultureInfo.InvariantCulture) + exGST];
				sellDetails.SellPriceExGST.CurrencyCode = localCurrencyCode;
				sellDetails.SellPriceIncGST.Value = (ZDecimal)landedCostingHistory[sellPricePropertyName + i.ToString(CultureInfo.InvariantCulture) + incGST];
				sellDetails.SellPriceIncGST.CurrencyCode = localCurrencyCode;

				sellDetails.EffectiveMarkUpPercentage = (ZDecimal)landedCostingHistory[effectiveMarkUpPercentagePropertyName + i.ToString(CultureInfo.InvariantCulture)];
				sellDetails.EffectiveMarkUpPercentageSpecified = !(sellDetails.EffectiveMarkUpPercentage == 0);

				sellDetails.MarkUpPercentage = (ZDecimal)landedCostingHistory[markUpPercentagePropertyName + i.ToString(CultureInfo.InvariantCulture)];
				sellDetails.MarkUpPercentageSpecified = !(sellDetails.MarkUpPercentage == 0);
			}
		}

		BusinessObject GetLandedCostingHeader(BaseJobComInvoiceLine invoiceLine)
		{
			BaseJobDeclaration declaration = invoiceLine.Declaration;

			if (declaration != null)
			{
				ZQuery query = new ZQuery(LandedCostHeaderSchema.LT_ParentID, declaration.PK);

				return (BusinessObject)invoiceLine.Factory.LoadTop1<Integration.LandedCosting.ILandedCostHeader>(query);
			}
			else
			{
				return null;
			}
		}

		void SetAmountAndCurrencyIfNotZero(ZDecimal amount, Xsd.FinancialValue financialValue, ZString localCurrencyCode)
		{
			if (amount != 0)
			{
				financialValue.Value = amount;
				financialValue.CurrencyCode = localCurrencyCode;
			}
		}

		protected void ExportInvoiceChargesDetails(Xsd.InvoiceLine xmlInvLine, IJobComInvChargeCollection<BaseInvoiceLineCharge> invLineCharges, INotifications notify)
		{
			if (invLineCharges.Count > 0 && xmlInvLine.Charges == null)
			{
				xmlInvLine.Charges = new Xsd.InvoiceChargeCollection();
			}

			foreach (BaseInvoiceLineCharge charge in invLineCharges)
			{
				Xsd.InvoiceCharge xmlCharge = xmlInvLine.Charges.AddNew();
				xmlCharge.ChargeType = charge.J7_ChargeType;

				xmlCharge.ChargeValue = new Xsd.FinancialValue();
				xmlCharge.ChargeValue.Value = charge.J7_Amount;
				xmlCharge.ChargeValue.CurrencyCode = charge.J7_RX_NKCurrency;
				xmlCharge.DutyApplies = charge.J7_IsDutiable ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlCharge.DutyAppliesSpecified = true;
				xmlCharge.GstApplies = charge.J7_IsGSTApplicable ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlCharge.GstAppliesSpecified = true;
				xmlCharge.IsIncludedInTotal = charge.J7_IsIncludedInITOT ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlCharge.IsIncludedInTotalSpecified = true;
				xmlCharge.IsIncludedInInvoice = charge.J7_IsNotIncludedInInvoice ? Xsd.TrueFalse.@false : Xsd.TrueFalse.@true;
				xmlCharge.IsIncludedInInvoiceSpecified = true;
			}
		}

		protected virtual void ExportInvoiceLineSummary(Xsd.InvoiceLineSummary summary, BaseJobComInvoiceLine line)
		{
			if (line != null)
			{
				RefCurrency linePriceRefCurrency = line.LinePriceRefCurrency;
				RefCurrency localCurrency = line.LocalCurrency;

				if (linePriceRefCurrency != null)
				{
					summary.CIF.CurrencyCode = linePriceRefCurrency.RX_Code;
					summary.FOB.CurrencyCode = linePriceRefCurrency.RX_Code;
					summary.Freight.CurrencyCode = linePriceRefCurrency.RX_Code;
					summary.Insurance.CurrencyCode = linePriceRefCurrency.RX_Code;
				}

				if (localCurrency != null)
				{
					summary.Duty.CurrencyCode = localCurrency.RX_Code;
					summary.GST.CurrencyCode = localCurrency.RX_Code;
				}

				summary.CIF.Value = line.JI_Calc_CIF;
				summary.FOB.Value = line.JI_Calc_FOB;
				summary.Freight.Value = line.JI_Calc_FreightInInvoiceCurr;
				summary.Insurance.Value = line.JI_Calc_InsuranceInInvoiceCurr;
				summary.Duty.Value = line.JI_Calc_DutyAmount;
				summary.DutyPercent = line.AdValoremDutyPercent;
				summary.DutyPercentSpecified = summary.DutyPercent > 0;
				summary.GST.Value = line.JI_Calc_GSTVATAmount + line.JI_Calc_GSTVATDeferred;

				if (linePriceRefCurrency != null && localCurrency != null && line.CurrencyConverter != null)
				{
					summary.CIFInLocalCurr.Value = line.CurrencyConverter.ConvertExact(new Money(summary.CIF.Value, linePriceRefCurrency), localCurrency).Amount;
					summary.FOBInLocalCurr.Value = line.CurrencyConverter.ConvertExact(new Money(summary.FOB.Value, linePriceRefCurrency), localCurrency).Amount;
					summary.FreightInLocalCurr.Value = line.CurrencyConverter.ConvertExact(new Money(summary.Freight.Value, linePriceRefCurrency), localCurrency).Amount;
					summary.InsuranceInLocalCurr.Value = line.CurrencyConverter.ConvertExact(new Money(summary.Insurance.Value, linePriceRefCurrency), localCurrency).Amount;
				}
			}
		}

		protected virtual void ExportInvoiceLinesAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, BaseJobComInvoiceLine invoiceLine, IValueObjectExportContext context)
		{
		}

		protected virtual void ExportInvoiceHeaderAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, BaseJobComInvoiceHeader invoiceHeader, IValueObjectExportContext context)
		{
		}

		protected void ExportParentLineNumber(ZGuid jI_ParentID, Xsd.InvoiceLine newInvoiceLine, BusinessObjectFactory factory)
		{
			if (!jI_ParentID.IsEmpty)
			{
				BaseJobComInvoiceLine parentInvoiceLine = factory.Load<BaseJobComInvoiceLine>(jI_ParentID);
				if (parentInvoiceLine != null)
				{
					newInvoiceLine.ParentInvoiceLineNumber = parentInvoiceLine.JI_LineNo;
					newInvoiceLine.ParentInvoiceLineNumberSpecified = true;
				}
			}
		}

		protected void ExportInvoiceHeaderReferences(Xsd.InvoiceReferenceCollection references, InvoiceHeaderRefsCollection invoiceHeaderRefs)
		{
			foreach (JobComInvoiceHeaderRefs reference in invoiceHeaderRefs)
			{
				Xsd.InvoiceReference refs = references.AddNew();
				refs.Type = reference.J2_ReferenceType;
				refs.Value = reference.J2_ReferenceNumber;
			}
		}

		#endregion

		#region LandedCostHistoryPropertyName

		public static class LandedCostPropertyNames
		{
			public const string UnitPriceInLocalCurrency = "UnitPriceInLocalCurrency";
			public const string RoundedPerUnitCustomsDisbursementCharges = "RoundedPerUnitCustomsDisbursementCharges";
			public const string RoundedPerUnitLandingCost = "RoundedPerUnitLandingCost";
			public const string RoundedPerUnitTotalCost = "RoundedPerUnitTotalCost";
			public const string LH_LandedCostMarginPercent = "LH_LandedCostMarginPercent";
			public const string EffectiveMarkUpPercentage = "EffectiveMarkUpPercentage";
			public const string RoundedSellPrice = "RoundedSellPrice";
			public const string ExGST = "ExGST";
			public const string IncGST = "IncGST";
			public const string LH_LandedCostGroup = "LH_LandedCostGroup";
		}

		public static class LandedLineCostTypes
		{
			public const string EntryFees = "ENT";
			public const string Excise = "EXC";
			public const string SpecialTaxPrefix = "ST";
		}

		#endregion
	}
}
