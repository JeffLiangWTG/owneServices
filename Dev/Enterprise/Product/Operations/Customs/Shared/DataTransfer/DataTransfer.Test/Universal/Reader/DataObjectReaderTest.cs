using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public class DataObjectReaderTest : DataObjectReaderTestHelper
	{
		protected AddInfoGroup SetupAddInfoGroup(CodeDescriptionPair type, List<AddInfo> addInfoCollection = null, List<AddInfoGroup> addInfoGroupCollection = null, List<CustomsReference> customsReferenceCollection = null)
		{
			return new AddInfoGroup()
			{
				Type = type,
				AddInfoCollection = addInfoCollection,
				AddInfoGroupCollection = addInfoGroupCollection,
				CustomsReferenceCollection = customsReferenceCollection
			};
		}

		protected void AssertCusAddInfoContents(IColumnIndexer cusAddInfoBO, ZString parentTableCode, ZGuid parentID, ZString type, string fullAddInfoData = null, string partialAddInfoData = null)
		{
			AssertEquals("cusAddInfoBO.B7_ParentTableCode", parentTableCode, cusAddInfoBO.GetValue(CusAddInfoSchema.B7_ParentTableCode));
			AssertEquals("cusAddInfoBO.B7_ParentID", parentID, cusAddInfoBO.GetValue(CusAddInfoSchema.B7_ParentID));
			AssertEquals("cusAddInfoBO.B7_Type", type, cusAddInfoBO.GetValue(CusAddInfoSchema.B7_Type));
			if (fullAddInfoData != null)
			{
				AssertEquals("cusAddInfoBO.B7_AddInfoData", fullAddInfoData, cusAddInfoBO.GetValue(CusAddInfoSchema.B7_AddInfoData));
			}
			if (partialAddInfoData != null)
			{
				AssertContains("cusAddInfoBO.B7_AddInfoData", partialAddInfoData, cusAddInfoBO.GetValue(CusAddInfoSchema.B7_AddInfoData));
			}
		}

		protected IColumnIndexer[] LoadCusAddInfo(ZString prefix, ZGuid pk)
		{
			return LoadCusAddInfo(Factory.RowFactory, prefix, pk);
		}

		protected IColumnIndexer[] LoadCusAddInfo(RowFactory factory, ZString prefix, ZGuid pk)
		{
			var query = new ZQuery(CusAddInfoSchema.B7_ParentID, pk);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, prefix);
			return UniversalDataObjectReaderHelperAbstractTest.ConvertToColumnIndexer(factory.Load(CusAddInfoSchema.Constants.TableName, query));
		}

		protected AdditionalBill SetupAdditionalBill(ZString? billNumber, ZString billTypeCode, ZString billTypeDescription, ZDateTime? issueDate, ZString? parentBillNumber, List<AddInfo> addInfoCollection, ZDecimal? noOfPacks, ZString packTypeCode, ZString packTypeDescription)
		{
			return SetupAdditionalBill(billNumber, new WayBillType() { Code = billTypeCode, Description = billTypeDescription }, issueDate, parentBillNumber, addInfoCollection, noOfPacks, new PackageType() { Code = packTypeCode, Description = packTypeDescription });
		}

		protected AdditionalBill SetupAdditionalBill(ZString? billNumber, WayBillType billType, ZDateTime? issueDate, ZString? parentBillNumber, List<AddInfo> addInfoCollection, ZDecimal? noOfPacks, PackageType packType)
		{
			var bill = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = billNumber,
				BillType = billType,
				IssueDate = issueDate,
				ParentBillNumber = parentBillNumber,
				NoOfPacks = noOfPacks,
				PackType = packType
			};
			bill.SetAddInfoCollection(() => addInfoCollection);
			return bill;
		}

		protected void AssertContents(Bill billBO, ZString billNumber, ZString billType, ZGuid parentBillPK, ZDateTime issueDate, ZDecimal noOfPacks, ZString packType, ZString addInfo)
		{
			AssertEquals("billBO.CU_BillNum", billNumber, billBO.CU_BillNum);
			AssertEquals("billBO.CU_BillType", billType, billBO.CU_BillType);
			AssertEquals("billBO.CU_CU_ParentBill", parentBillPK, billBO.CU_CU_ParentBill);
			AssertEquals("billBO.CU_IssueDate", issueDate, billBO.CU_IssueDate);
			AssertEquals("billBO.CU_NoOfPacks", noOfPacks, billBO.CU_NoOfPacks);
			AssertEquals("billBO.CU_PackType", packType, billBO.CU_PackType);
			AssertEquals("billBO.CU_AddInfo", addInfo, billBO.CU_AddInfo);
		}

		protected CommercialCharge SetupCommercialCharge(ZBool? isApportionedCharge, ZBool? isStatisticalValueApplicable = null)
		{
			return SetupCommercialCharge(ZBool.False, 100.60m, new CodeDescriptionPair() { Code = Business.CustomsChargeTypeList.Codes.OverseasFreight, Description = CustomsChargeTypeList.Descriptions.OverseasFreight }, new Currency() { Code = Core.Constants.CurrencyCodes.Australia, Description = "Australia, Dollars" }, new CodeDescriptionPair() { Code = ChargeDistributeByList.Codes.Weight, Description = ChargeDistributeByList.Descriptions.Weight }, new CodeDescriptionPair() { Code = Common.ChargeExchangeRateTypeList.Codes.FixedRate, Description = Common.ChargeExchangeRateTypeList.Descriptions.FixedRate }, 1.5060m, new CodeDescriptionPair() { Code = ApportionmentTypeList.Codes.PartialApportionment, Description = ApportionmentTypeList.Descriptions.PartialApportionment }, isApportionedCharge, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 10m, new CodeDescriptionPair() { Code = Core.Constants.PaymentType.Prepaid, Description = "Prepaid" }, isStatisticalValueApplicable);
		}

		protected CommercialCharge SetupCommercialCharge(ZBool? adjustedCharge, ZDecimal? amount, CodeDescriptionPair chargeType, Currency currency, CodeDescriptionPair distributeBy, CodeDescriptionPair exchangeRateType, ZDecimal? agreedExchangeRate, CodeDescriptionPair apportionmentType, ZBool? isApportionedCharge, ZBool? isDutiable, ZBool? isGSTApplicable, ZBool? isIncludedInITOT, ZBool? isNotIncludedInInvoice, ZDecimal? percentage, CodeDescriptionPair prepaidCollect, ZBool? isStatisticalValueApplicable = null)
		{
			return new CommercialCharge()
			{
				AdjustedCharge = adjustedCharge,
				Amount = amount,
				ChargeType = chargeType,
				Currency = currency,
				DistributeBy = distributeBy,
				ExchangeRateType = exchangeRateType,
				AgreedExchangeRate = agreedExchangeRate,
				ApportionmentType = apportionmentType,
				IsApportionedCharge = isApportionedCharge,
				IsDutiable = isDutiable,
				IsGSTApplicable = isGSTApplicable,
				IsIncludedInITOT = isIncludedInITOT,
				IsNotIncludedInInvoice = isNotIncludedInInvoice,
				PercentageOfLinePrice = percentage,
				PrepaidCollect = prepaidCollect,
				IsStatisticalValueApplicable = isStatisticalValueApplicable
			};
		}

		protected void AssertContents(BaseInvoiceLineCharge chargeBO, ZBool? isStatisticalValueApplicable = null)
		{
			AssertContents(chargeBO, ZBool.False, 100.60m, Business.CustomsChargeTypeList.Codes.OverseasFreight, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Weight, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.5060m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 10m, Core.Constants.PaymentType.Prepaid, isStatisticalValueApplicable);
		}

		protected void AssertContents(CommonNonApportionedCharge chargeBO, ZBool adjustedCharge, ZDecimal amount, ZString chargeType, ZString currency, ZString distributeBy, ZString exchangeRateType, ZDecimal exchangeRate, ZString fullOrPartialApportionment, ZBool isDutiable, ZBool isGSTApplicable, ZBool isIncludedInITOT, ZBool isNotIncludedInInvoice, ZDecimal percentage, ZString prepaidCollect, ZBool? isStatisticalValueApplicable = null)
		{
			AssertEquals("chargeBO.J7_Percentage", percentage, chargeBO.J7_Percentage);
			AssertEquals("chargeBO.J7_AdjustedCharge", adjustedCharge, chargeBO.J7_AdjustedCharge);
			AssertEquals("chargeBO.J7_Amount", amount, chargeBO.J7_Amount);
			AssertEquals("chargeBO.J7_ChargeType", chargeType, chargeBO.J7_ChargeType);
			AssertEquals("chargeBO.J7_RX_NKCurrency", currency, chargeBO.J7_RX_NKCurrency);
			AssertEquals("chargeBO.J7_DistributeBy", distributeBy, chargeBO.J7_DistributeBy);
			AssertEquals("chargeBO.J7_ExchangeRateType", exchangeRateType, chargeBO.J7_ExchangeRateType);
			AssertEquals("chargeBO.J7_ExchangeRate", exchangeRate, chargeBO.J7_ExchangeRate);
			AssertEquals("chargeBO.J7_FullOrPartialApportionment", fullOrPartialApportionment, chargeBO.J7_FullOrPartialApportionment);
			AssertEquals("chargeBO.J7_IsDutiable", isDutiable, chargeBO.J7_IsDutiable);
			AssertEquals("chargeBO.J7_IsGSTApplicable", isGSTApplicable, chargeBO.J7_IsGSTApplicable);
			AssertEquals("chargeBO.J7_IsIncludedInITOT", isIncludedInITOT, chargeBO.J7_IsIncludedInITOT);
			AssertEquals("chargeBO.J7_IsNotIncludedInInvoice", isNotIncludedInInvoice, chargeBO.J7_IsNotIncludedInInvoice);
			AssertEquals("chargeBO.J7_PrepaidCollect", prepaidCollect, chargeBO.J7_PrepaidCollect);
			if (isStatisticalValueApplicable.HasValue)
			{
				AssertEquals("chargeBO.J7_IsStatisticalValueApplicable", isStatisticalValueApplicable.Value, chargeBO.J7_IsStatisticalValueApplicable);
			}
		}

		protected CodeDescriptionPair DistributeByWeight
		{
			get { return new CodeDescriptionPair() { Code = ChargeDistributeByList.Codes.Weight, Description = ChargeDistributeByList.Descriptions.Weight }; }
		}

		protected CodeDescriptionPair DistributeByValue
		{
			get { return new CodeDescriptionPair() { Code = ChargeDistributeByList.Codes.Value, Description = ChargeDistributeByList.Descriptions.Value }; }
		}

		protected CodeDescriptionPair DistributeByVolume
		{
			get { return new CodeDescriptionPair() { Code = ChargeDistributeByList.Codes.Volume, Description = ChargeDistributeByList.Descriptions.Volume }; }
		}

		protected CodeDescriptionPair PartialApportionment
		{
			get { return new CodeDescriptionPair() { Code = ApportionmentTypeList.Codes.PartialApportionment, Description = ApportionmentTypeList.Descriptions.PartialApportionment }; }
		}

		protected CodeDescriptionPair FullApportionment
		{
			get { return new CodeDescriptionPair() { Code = ApportionmentTypeList.Codes.FullApportionment, Description = ApportionmentTypeList.Descriptions.FullApportionment }; }
		}

		protected CodeDescriptionPair Prepaid
		{
			get { return new CodeDescriptionPair() { Code = Core.Constants.PaymentType.Prepaid, Description = "Prepaid" }; }
		}

		protected CodeDescriptionPair Collect
		{
			get { return new CodeDescriptionPair() { Code = Core.Constants.PaymentType.Collect, Description = "Collect" }; }
		}

		protected CodeDescriptionPair AdditionCharge
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.AdditionCharge, Description = CustomsChargeTypeList.Descriptions.AdditionCharge }; }
		}

		protected CodeDescriptionPair Commission
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.Commission, Description = CustomsChargeTypeList.Descriptions.Commission }; }
		}

		protected CodeDescriptionPair DeductionCharge
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.DeductionCharge, Description = CustomsChargeTypeList.Descriptions.DeductionCharge }; }
		}

		protected CodeDescriptionPair Discount
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.Discount, Description = CustomsChargeTypeList.Descriptions.Discount }; }
		}

		protected CodeDescriptionPair ExWorks
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.ExWorks, Description = CustomsChargeTypeList.Descriptions.ExWorks }; }
		}

		protected CodeDescriptionPair ForeignInlandFreight
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.ForeignInlandFreight, Description = CustomsChargeTypeList.Descriptions.ForeignInlandFreight }; }
		}

		protected CodeDescriptionPair LandingCharges
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.LandingCharges, Description = CustomsChargeTypeList.Descriptions.LandingCharges }; }
		}

		protected CodeDescriptionPair OtherCharges
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OtherCharges, Description = CustomsChargeTypeList.Descriptions.OtherCharges }; }
		}

		protected CodeDescriptionPair OverseasFreight
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OverseasFreight, Description = CustomsChargeTypeList.Descriptions.OverseasFreight }; }
		}

		protected CodeDescriptionPair OverseasInsurance
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OverseasInsurance, Description = CustomsChargeTypeList.Descriptions.OverseasInsurance }; }
		}

		protected CodeDescriptionPair PackingCost
		{
			get { return new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.PackingCost, Description = CustomsChargeTypeList.Descriptions.PackingCost }; }
		}

		protected CodeDescriptionPair FixedRate
		{
			get { return new CodeDescriptionPair() { Code = Enterprise.Customs.Common.ChargeExchangeRateTypeList.Codes.FixedRate, Description = Enterprise.Customs.Common.ChargeExchangeRateTypeList.Descriptions.FixedRate }; }
		}

		protected PackedItem SetupPackedItem(ZInt? commercialInvoiceLineLink, ZDecimal? grossWeight, ZDecimal? netWeight, ZDecimal? packQty, ZDecimal? value)
		{
			var result = new PackedItem()
			{
				CommercialInvoiceLineLink = commercialInvoiceLineLink,
				GrossWeight = grossWeight,
				GrossWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms },
				NetWeight = netWeight,
				NetWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms },
				PackedQuantity = packQty,
				GoodsValue = value,
			};
			return result;
		}

		protected void AssertContents(CusContainerInvoiceLinePivot containerPivot, ZDecimal grossWeight, ZDecimal netWeight, ZInt packQty, ZDecimal value)
		{
			AssertEquals("containerPivot.C2_GrossWeight", grossWeight, containerPivot.C2_GrossWeight);
			AssertEquals("containerPivot.C2_NetWeight", netWeight, containerPivot.C2_NetWeight);
			AssertEquals("containerPivot.C2_PackQty", packQty, containerPivot.C2_PackQty);
			AssertEquals("containerPivot.C2_SplitValue", value, containerPivot.C2_SplitValue);
		}

		protected void AssertContents(BaseJobComInvoiceLine invoiceLine, ZShort lineNo, string addInfo = "")
		{
			AssertContents(invoiceLine, lineNo, "GOODS", 1040.50m, Core.Constants.PkgUnit.Box, 4140.53m, "PART12", 3.2m, Core.Constants.Volume.CubicYards, 202.92m, Core.Constants.Weight.Hectograms, addInfo);
		}

		protected void AssertContents2(BaseJobComInvoiceLine invoiceLine, ZShort lineNo, string addInfo = "")
		{
			AssertContents(invoiceLine, lineNo, "BAD", 968.45m, Core.Constants.PkgUnit.Package, 6953.85m, "PART89", 1.69m, Core.Constants.Volume.CubicInches, 365.88m, Core.Constants.Weight.Ounces, addInfo);
		}

		protected void AssertContents(BaseJobComInvoiceLine invoiceLine, ZShort lineNo, ZString description, ZDecimal invoiceQuantity, ZString invoiceQuantityUnit, ZDecimal linePrice, ZString partNo, ZDecimal volume, ZString volumeUnit, ZDecimal weight, ZString weightUnit, string addInfo = "")
		{
			AssertEquals("invoiceLine.JI_LineNo", lineNo, invoiceLine.JI_LineNo);
			AssertEquals("invoiceLine.JI_Description", description, invoiceLine.JI_Description);
			AssertEquals("invoiceLine.JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("invoiceLine.JI_InvoiceUQ", invoiceQuantityUnit, invoiceLine.JI_InvoiceUQ);
			AssertEquals("invoiceLine.JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals("invoiceLine.JI_PartNo", partNo, invoiceLine.JI_PartNo);
			AssertEquals("invoiceLine.JI_Volume", volume, invoiceLine.JI_Volume);
			AssertEquals("invoiceLine.JI_VolumeUQ", volumeUnit, invoiceLine.JI_VolumeUQ);
			AssertEquals("invoiceLine.JI_Weight", weight, invoiceLine.JI_Weight);
			AssertEquals("invoiceLine.JI_WeightUQ", weightUnit, invoiceLine.JI_WeightUQ);
			AssertEquals("invoiceLine.JI_AddInfo", addInfo, invoiceLine.JI_AddInfo);
		}

		protected CommercialInvoiceLine SetupCommercialInvoiceLine(ZInt lineNo, List<AddInfo> addInfoCollection = null, List<CommercialCharge> commercialInvoiceChargeCollection = null, List<AddInfoGroup> addInfoGroupCollection = null, List<CustomsReference> customsReferenceCollection = null, List<EntryReference> entryReferenceCollection = null)
		{
			return SetupCommercialInvoiceLine(lineNo, "GOODS", 1040.50m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Box, Description = "Box" }, 4140.53m, "PART12", 3.2m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicYards, Description = "Cubic Yards" }, 202.92m, new UnitOfWeight() { Code = Core.Constants.Weight.Hectograms, Description = "Hectograms" }, addInfoCollection, commercialInvoiceChargeCollection, addInfoGroupCollection, customsReferenceCollection, entryReferenceCollection);
		}

		protected CommercialInvoiceLine SetupCommercialInvoiceLine2(ZInt lineNo, List<AddInfo> addInfoCollection = null, List<CommercialCharge> commercialInvoiceChargeCollection = null, List<AddInfoGroup> addInfoGroupCollection = null, List<CustomsReference> customsReferenceCollection = null, List<EntryReference> entryReferenceCollection = null)
		{
			return SetupCommercialInvoiceLine(lineNo, "BAD", 968.45m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Package, Description = "Package" }, 6953.85m, "PART89", 1.69m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicInches, Description = "Cubic Inches" }, 365.88m, new UnitOfWeight() { Code = Core.Constants.Weight.Ounces, Description = "Ounces" }, addInfoCollection, commercialInvoiceChargeCollection, addInfoGroupCollection, customsReferenceCollection, entryReferenceCollection);
		}

		protected CommercialInvoiceLine SetupCommercialInvoiceLine(ZInt lineNo, ZString description, ZDecimal invoiceQuantity, CodeDescriptionPair invoiceQuantityUnit, ZDecimal linePrice, ZString partNo, ZDecimal volume, UnitOfVolume volumeUnit, ZDecimal weight, UnitOfWeight weightUnit, List<AddInfo> addInfoCollection = null, List<CommercialCharge> commercialInvoiceChargeCollection = null, List<AddInfoGroup> addInfoGroupCollection = null, List<CustomsReference> customsReferenceCollection = null, List<EntryReference> entryReferenceCollection = null)
		{
			return new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = lineNo,
				Description = description,
				InvoiceQuantity = invoiceQuantity,
				InvoiceQuantityUnit = invoiceQuantityUnit,
				LinePrice = linePrice,
				PartNo = partNo,
				Volume = volume,
				VolumeUnit = volumeUnit,
				Weight = weight,
				WeightUnit = weightUnit,
				AddInfoCollection = addInfoCollection,
				CommercialChargeCollection = commercialInvoiceChargeCollection,
				AddInfoGroupCollection = addInfoGroupCollection,
				CustomsReferenceCollection = customsReferenceCollection,
				EntryReferenceCollection = entryReferenceCollection
			};
		}

		protected CommercialInfo SetupCommercialInfo(ZString? groupName, List<CommercialCharge> chargeCollection = null, DataObjectList<CommercialInvoiceHeader> invoiceCollection = null, List<CommercialInfo> groupCollection = null)
		{
			return new CommercialInfo()
			{
				Name = groupName,
				CommercialChargeCollection = chargeCollection,
				CommercialInvoiceCollection = invoiceCollection,
				SubGroupCollection = groupCollection
			};
		}

		protected void AssertContents(BaseJobComInvoiceHeader invoice, string addInfo = "")
		{
			AssertContents(invoice, "INV3243", OrgHeader.UnmatchedOrganisationPK, OrgHeader.UnmatchedOrganisationPK, 3420.34m, invoice.LocalCurrencyCode, new ZDateTime(2011, 4, 3), Core.Constants.IncoTerms.FreeOnBoard, 14.72m, Core.Constants.Volume.CubicMetres, 2.53m, Core.Constants.Weight.Tonnes, 11.11m, Core.Constants.Weight.Kilograms, ZString.Empty, 1m, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, ZDecimal.Zero, addInfo);
		}

		protected void AssertContents2(BaseJobComInvoiceHeader invoice, string addInfo = "")
		{
			AssertContents(invoice, "INV6854", OrgHeader.UnmatchedOrganisationPK, OrgHeader.UnmatchedOrganisationPK, 8685.54m, ForeignCurrencyBO.RX_Code, new ZDateTime(2011, 4, 2), Core.Constants.IncoTerms.CostInsuranceAndFreight, 96.87m, Core.Constants.Volume.CubicYards, 86.69m, Core.Constants.Weight.Kilotonnes, 22.22m, Core.Constants.Weight.Kilograms, Enterprise.Customs.Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.5m, 1.55m, "PA123", 1234.43m, 2.3m, new ZDateTime(2012, 2, 3), 2.5m, addInfo);
		}

		protected void AssertContents(BaseJobComInvoiceHeader invoice, ZString invoiceNumber, ZGuid buyerPK, ZGuid supplierPK, ZDecimal invoiceAmount, ZString invoiceCurrency, ZDateTime invoiceDate, ZString incoTerm, ZDecimal volume, ZString volumeUnit, ZDecimal weight, ZString weightUnit, ZDecimal netWeight, ZString netWeightUQ, ZString invoiceCurrExRateType, ZDecimal invoiceCurrExRate, ZDecimal invoiceCurrLandedCostExRate, ZString paymentNo, ZDecimal paymentAmount, ZDecimal paymentExRate, ZDateTime paymentDate, ZDecimal noOfPacks, string addInfo = "")
		{
			AssertEquals("invoice.JZ_InvoiceNumber", invoiceNumber, invoice.JZ_InvoiceNumber);
			AssertEquals("invoice.JZ_OH_Buyer", buyerPK, invoice.JZ_OH_Buyer);
			AssertEquals("invoice.JZ_OH_Supplier", supplierPK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_InvoiceAmount", invoiceAmount, invoice.JZ_InvoiceAmount);
			AssertEquals("invoice.JZ_RX_NKInvoice_Currency", invoiceCurrency, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("invoice.JZ_InvoiceDate", invoiceDate, invoice.JZ_InvoiceDate);
			AssertEquals("invoice.JZ_IncoTerm", incoTerm, invoice.JZ_IncoTerm);
			AssertEquals("invoice.JZ_Volume", volume, invoice.JZ_Volume);
			AssertEquals("invoice.JZ_VolumeUQ", volumeUnit, invoice.JZ_VolumeUQ);
			AssertEquals("invoice.JZ_Weight", weight, invoice.JZ_Weight);
			AssertEquals("invoice.JZ_WeightUQ", weightUnit, invoice.JZ_WeightUQ);
			AssertEquals("invoice.JZ_NetWeight", netWeight, invoice.JZ_NetWeight);
			AssertEquals("invoice.JZ_NetWeightUQ", netWeightUQ, invoice.JZ_NetWeightUQ);
			AssertEquals("invoice.JZ_InvoiceCurrExRateType", invoiceCurrExRateType, invoice.JZ_InvoiceCurrExRateType);
			AssertEquals("invoice.JZ_InvoiceCurrExRate", invoiceCurrExRate, invoice.JZ_InvoiceCurrExRate);
			AssertEquals("invoice.JZ_InvoiceCurrLandedCostExRate", invoiceCurrLandedCostExRate, invoice.JZ_InvoiceCurrLandedCostExRate);
			AssertEquals("invoice.JZ_PaymentNo", paymentNo, invoice.JZ_PaymentNo);
			AssertEquals("invoice.JZ_PaymentAmount", paymentAmount, invoice.JZ_PaymentAmount);
			AssertEquals("invoice.JZ_PaymentExRate", paymentExRate, invoice.JZ_PaymentExRate);
			AssertEquals("invoice.JZ_PaymentDate", paymentDate, invoice.JZ_PaymentDate);
			AssertEquals("invoice.JZ_NoOfPacks", noOfPacks, invoice.JZ_NoOfPacks);

			AssertEquals("invoice.JZ_AddInfo", addInfo, invoice.JZ_AddInfo);
		}

		protected CommercialInvoiceHeader SetupCommercialInvoiceHeaderData(List<AddInfo> addInfoCollection = null, List<CommercialCharge> commercialInvoiceChargeCollection = null, DataObjectList<CommercialInvoiceLine> commercialInvoiceLineCollection = null, List<AddInfoGroup> addInfoGroupCollection = null, List<CustomsReference> customsReferenceCollection = null)
		{
			var buyer = SetupOrganizationAddress(AddressTypes.Importer);
			var supplier = SetupOrganizationAddress2(AddressTypes.Supplier);
			return SetupCommercialInvoiceHeaderData("INV3243", buyer, supplier, 3420.34m, LocalCurrency, new ZDateTime(2011, 4, 3), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.FreeOnBoard, Description = "Free On Board" }, 14.72m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres, Description = "Cubic Meters" }, 2.53m, new UnitOfWeight() { Code = Core.Constants.Weight.Tonnes, Description = "Tonnes" }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null, null, null, null, null, null, addInfoCollection, commercialInvoiceChargeCollection, commercialInvoiceLineCollection, addInfoGroupCollection, customsReferenceCollection);
		}

		protected CommercialInvoiceHeader SetupCommercialInvoiceHeaderData2(List<AddInfo> addInfoCollection = null, List<CommercialCharge> commercialInvoiceChargeCollection = null, DataObjectList<CommercialInvoiceLine> commercialInvoiceLineCollection = null, List<AddInfoGroup> addInfoGroupCollection = null, List<CustomsReference> customsReferenceCollection = null)
		{
			var buyer = SetupOrganizationAddress2(AddressTypes.Importer);
			var supplier = SetupOrganizationAddress(AddressTypes.Supplier);
			return SetupCommercialInvoiceHeaderData("INV6854", buyer, supplier, 8685.54m, ForeignCurrency, new ZDateTime(2011, 4, 2), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.CostInsuranceAndFreight, Description = "Cost, Insurance And Freight" }, 96.87m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicYards, Description = "Cubic Yards" }, 86.69m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilotonnes, Description = "Kilotons" }, 22.22m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.55m, 2.5m, "PA123", 1234.43m, 2.3m, new ZDateTime(2012, 2, 3), new CodeDescriptionPair() { Code = "MS1", Description = "MESSAGE STATUS 1" }, addInfoCollection, commercialInvoiceChargeCollection, commercialInvoiceLineCollection, addInfoGroupCollection, customsReferenceCollection);
		}

		protected CommercialInvoiceHeader SetupCommercialInvoiceHeaderData(ZString? invoiceNumber, OrganizationAddress buyer, OrganizationAddress supplier, ZDecimal? invoiceAmount, Currency invoiceCurrency, ZDateTime? invoiceDate, CodeDescriptionPair incoTerm, ZDecimal? volume, UnitOfVolume volumeUnit, ZDecimal? weight, UnitOfWeight weightUnit, ZDecimal? netWeight, UnitOfWeight netWeightUQ, CodeDescriptionPair invoiceCurrExRateType, ZDecimal? invoiceCurrExRate, ZDecimal? landedCostExchangeRate, ZDecimal? noOfPacks, ZString? paymentNumber, ZDecimal? paymentAmount, ZDecimal? paymentExchangeRate, ZDateTime? paymentDate, CodeDescriptionPair messageStatus, List<AddInfo> addInfoCollection = null, List<CommercialCharge> commercialInvoiceChargeCollection = null, DataObjectList<CommercialInvoiceLine> commercialInvoiceLineCollection = null, List<AddInfoGroup> addInfoGroupCollection = null, List<CustomsReference> customsReferenceCollection = null)
		{
			var result = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = invoiceNumber,
				Buyer = buyer,
				Supplier = supplier,
				InvoiceAmount = invoiceAmount,
				InvoiceCurrency = invoiceCurrency,
				InvoiceDate = invoiceDate,
				IncoTerm = incoTerm,
				Volume = volume,
				VolumeUnit = volumeUnit,
				Weight = weight,
				WeightUnit = weightUnit,
				NetWeight = netWeight,
				NetWeightUQ = netWeightUQ,
				ExchangeRateType = invoiceCurrExRateType,
				AgreedExchangeRate = invoiceCurrExRate,
				LandedCostExchangeRate = landedCostExchangeRate,
				NoOfPacks = noOfPacks,
				PaymentNumber = paymentNumber,
				PaymentAmount = paymentAmount,
				PaymentExchangeRate = paymentExchangeRate,
				PaymentDate = paymentDate,
				MessageStatus = messageStatus,
				AddInfoCollection = addInfoCollection,
				CommercialChargeCollection = commercialInvoiceChargeCollection,
				AddInfoGroupCollection = addInfoGroupCollection,
				CustomsReferenceCollection = customsReferenceCollection
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => commercialInvoiceLineCollection));
			return result;
		}

		protected EntryReference SetupEntryReference(ZShort? lineNumber, EntryType type, ZString? reference)
		{
			var result = new EntryReference()
			{
				LineNumber = lineNumber,
				Type = type,
				Reference = reference
			};
			return result;
		}

		protected Note SetupPublicAAACustomNote(string description, string noteText)
		{
			return SetupNote(description, new CodeDescriptionPair() { Code = nameof(CargoWise.Definitions.StmNoteVisibility.PUB) }, new NoteContext() { Code = "AAA" }, true, noteText);
		}

		protected void AssertContainsPublicAAACustomNote(Notes notes, string description, string noteText)
		{
			var matchedNotes = notes.FindByDescription(description);
			AssertEquals("matchedNotes.Length", 1, matchedNotes.Length);
			StmNote note = matchedNotes[0];
			AssertEquals("note.ST_NoteText", noteText, note.ST_NoteDataAsText);
			AssertEquals(note.ST_NoteType, nameof(CargoWise.Definitions.StmNoteVisibility.PUB), note.ST_NoteType);
			AssertEquals(note.ST_NoteContext, "AAA", note.ST_NoteContext);
			AssertEquals("note.ST_IsCustomDescription", true, note.ST_IsCustomDescription);
		}

		protected Container SetupContainer(List<AddInfo> addInfos = null)
		{
			return SetupContainer("OOCL0000027", 72.998m, new UnitOfWeight() { Code = "LB", Description = "Pounds" }, "SEAL1", "SEAL2", new ContainerMode() { Code = "LCL", Description = "Less Container Load" }, new ContainerType() { Code = "ZW0W", Description = "Container Type WOW!!", ISOCode = "21G5" }, new CodeDescriptionPair2Char() { Code = "21", Description = "Container 21!" }, addInfos);
		}

		protected Container SetupContainer2(List<AddInfo> addInfos = null)
		{
			return SetupContainer("GKDS0000089", 100.50m, new UnitOfWeight() { Code = "KG", Description = "Kilograms" }, "GKDS1", "GKDS2", new ContainerMode() { Code = "FCL", Description = "Full Container Load" }, new ContainerType() { Code = "KD20", Description = "Container Type KD20!!", ISOCode = "20G5" }, new CodeDescriptionPair2Char() { Code = "20", Description = "Container 20!" }, addInfos);
		}

		protected Container SetupContainer(ZString? containerNumber, ZDecimal? goodsWeight, UnitOfWeight weightUnit, ZString? seal, ZString? secondSeal, ContainerMode fCL_LCL_AIR, ContainerType containerType, CodeDescriptionPair2Char customsContainerSize, List<AddInfo> addInfoCollection)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = containerNumber,
				GoodsWeight = goodsWeight,
				WeightUnit = weightUnit,
				Seal = seal,
				SecondSeal = secondSeal,
				FCL_LCL_AIR = fCL_LCL_AIR,
				ContainerType = containerType,
				CustomsContainerSize = customsContainerSize
			};
			container.SetAddInfoCollection(() => addInfoCollection);
			return container;
		}

		protected void AssertContents(BaseCusContainer containerBO, string addInfo = "")
		{
			AssertContents(containerBO, "OOCL0000027", 72.998m, "LB", "SEAL1", "SEAL2", "LCL", "", addInfo);
		}

		protected void AssertContents2(BaseCusContainer containerBO, string addInfo = "")
		{
			AssertContents(containerBO, "GKDS0000089", 100.50m, "KG", "GKDS1", "GKDS2", "FCL", "", addInfo);
		}

		protected void AssertContents(BaseCusContainer containerBO, ZString containerNumber, ZDecimal weight, ZString weightUQ, ZString seal, ZString secondSeal, ZString fCL_LCL_AIR, ZString containerSize, ZString addInfo)
		{
			AssertEquals("containerBO.CO_ContainerNumber", containerNumber, containerBO.CO_ContainerNumber);
			AssertEquals("containerBO.CO_Weight", weight, containerBO.CO_Weight);
			AssertEquals("containerBO.CO_WeightUQ", weightUQ, containerBO.CO_WeightUQ);
			AssertEquals("containerBO.CO_Seal", seal, containerBO.CO_Seal);
			AssertEquals("containerBO.CO_SecondSeal", secondSeal, containerBO.CO_SecondSeal);
			AssertEquals("containerBO.CO_FCL_LCL_AIR", fCL_LCL_AIR, containerBO.CO_FCL_LCL_AIR);
			AssertEquals("containerBO.CO_ContainerSize", containerSize, containerBO.CO_ContainerSize);
			AssertEquals("containerBO.CO_AddInfo", addInfo, containerBO.CO_AddInfo);
		}

		protected EntryHeaderCharge SetupEntryHeaderCharge()
		{
			return SetupEntryHeaderCharge(new CodeDescriptionPair() { Code = "BCH", Description = "BCH DESC" }, 2032.34m);
		}

		protected EntryHeaderCharge SetupEntryHeaderCharge2()
		{
			return SetupEntryHeaderCharge(new CodeDescriptionPair() { Code = "WCH", Description = "WCH DESC" }, 1503.53m);
		}

		protected EntryHeaderCharge SetupEntryHeaderCharge(CodeDescriptionPair type, ZDecimal? amount)
		{
			return new EntryHeaderCharge()
			{
				Type = type,
				Amount = amount
			};
		}

		protected void AssertCusEntryHeaderChargeContents(CusEntryHeaderCharges entryHeaderChargeBO, ZGuid entryHeaderPK)
		{
			AssertCusEntryHeaderChargeContents(entryHeaderChargeBO, entryHeaderPK, "BCH", 2032.34m);
		}

		protected void AssertCusEntryHeaderChargeContents2(CusEntryHeaderCharges entryHeaderChargeBO, ZGuid entryHeaderPK)
		{
			AssertCusEntryHeaderChargeContents(entryHeaderChargeBO, entryHeaderPK, "WCH", 1503.53m);
		}

		protected void AssertCusEntryHeaderChargeContents(CusEntryHeaderCharges entryHeaderChargeBO, ZGuid entryHeaderPK, ZString type, ZDecimal amount)
		{
			AssertEquals("entryHeaderChargeBO.C1_CH", entryHeaderPK, entryHeaderChargeBO.C1_CH);
			AssertEquals("entryHeaderChargeBO.C1_ChargeType", type, entryHeaderChargeBO.C1_ChargeType);
			AssertEquals("entryHeaderChargeBO.C1_ChargeAmount", amount, entryHeaderChargeBO.C1_ChargeAmount);
		}

		protected EntryHeader SetupEntryHeader(ZString type, ZString messageStatus, ZString entryStatus, ZString? reference, ZDecimal? totalAmountPaid, ZDateTime? entrySubmittedDate, ZDateTime? entryReleaseDate, List<AddInfo> addInfoCollection = null)
		{
			return SetupEntryHeader(new EntryType() { Code = type, Description = type + " DESC" }, new CodeDescriptionPair() { Code = messageStatus, Description = messageStatus + " DESC" }, new EntryStatus() { Code = entryStatus, Description = entryStatus + " DESC" }, reference, totalAmountPaid, entrySubmittedDate, entryReleaseDate, addInfoCollection);
		}

		protected EntryHeader SetupEntryHeader(EntryType type, CodeDescriptionPair messageStatus, EntryStatus entryStatus, ZString? reference, ZDecimal? totalAmountPaid, ZDateTime? entrySubmittedDate, ZDateTime? entryReleaseDate, List<AddInfo> addInfoCollection = null)
		{
			return new EntryHeader()
			{
				Type = type,
				MessageStatus = messageStatus,
				EntryStatus = entryStatus,
				Reference = reference,
				TotalAmountPaid = totalAmountPaid,
				EntrySubmittedDate = entrySubmittedDate,
				EntryReleaseDate = entryReleaseDate,
				AddInfoCollection = addInfoCollection
			};
		}

		protected void AssertCusEntryHeaderContents(CusEntryHeader entryHeaderBO, ZGuid declarationPK, ZString type, ZString messageStatus, ZString entryStatus, ZString reference, ZDecimal totalAmountPaid, ZDateTime entrySubmittedDate, ZDateTime entryReleaseDate, ZString addInfo, ZGuid primeEntryPK)
		{
			AssertEquals("entryHeaderBO.CH_JE", declarationPK, entryHeaderBO.CH_JE);
			AssertEquals("entryHeaderBO.CH_MessageType", type, entryHeaderBO.CH_MessageType);
			AssertEquals("entryHeaderBO.CH_Status", messageStatus, entryHeaderBO.CH_Status);
			AssertEquals("entryHeaderBO.CH_EntryStatus", entryStatus, entryHeaderBO.CH_EntryStatus);
			AssertEquals("entryHeaderBO.CH_BGMReference", reference, entryHeaderBO.CH_BGMReference);
			AssertEquals("entryHeaderBO.CH_TotalPaid", totalAmountPaid, entryHeaderBO.CH_TotalPaid);
			AssertEquals("entryHeaderBO.CH_EntrySubmittedDate", entrySubmittedDate, entryHeaderBO.CH_EntrySubmittedDate);
			AssertEquals("entryHeaderBO.CH_EntryReleaseDate", entryReleaseDate, entryHeaderBO.CH_EntryReleaseDate);
			AssertEquals("entryHeaderBO.CH_AddInfo", addInfo, entryHeaderBO.CH_AddInfo);
			AssertEquals("entryHeaderBO.CH_CH_PrimeEntry", primeEntryPK, entryHeaderBO.CH_CH_PrimeEntry);
		}

		protected CusEntryHeader[] LoadCusEntryHeaderFromPrimeEntryPK(ZGuid primeEntryPK)
		{
			return Factory.Load<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.CH_CH_PrimeEntry, primeEntryPK));
		}

		public void TestBasicCusEntryLineFeeFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var entryLineChargeDataObject = new EntryLineCharge
			{
				Type = new CodeDescriptionPair { Code = "BCL", Description = "BCL DESC" },
				Amount = 2032.34m,
				BaseValue = 135.62m,
				Rate = 0.68m,
				Source = "CW1",
				RateOverrideReason = new CodeDescriptionPair { Code = "OTH", Description = "OTH DESC" },
				MethodOfPayment = new CodeDescriptionPair { Code = "PPD", Description = "PPD DESC" },
				MethodOfCalculation = new CodeDescriptionPair6Char { Code = "SUMFUN", Description = "SUM DESC" }
			};

			var entryLineChargeBO = new CustomsEntryLineChargeDataObjectReader(entryLineChargeDataObject, logger, CurrentCompanyHelper, entryLine).ReadIntoBusinessObject();
			assertCusEntryLineFee();

			entryLineChargeBO.CF_ChargeAmount = 1000.10m;
			entryLineChargeBO = new CustomsEntryLineChargeDataObjectReader(entryLineChargeDataObject, logger, CurrentCompanyHelper, entryLine).ReadIntoBusinessObject();
			assertCusEntryLineFee();

			void assertCusEntryLineFee()
			{
				CombineAssertions(() =>
				{
					AssertEquals("entryLineChargeBO.CF_CL", entryLine.PK, entryLineChargeBO.CF_CL);
					AssertEquals("entryLineChargeBO.CF_ChargeType", "BCL", entryLineChargeBO.CF_ChargeType);
					AssertEquals("entryLineChargeBO.CF_ChargeAmount", 2032.34m, entryLineChargeBO.CF_ChargeAmount);
					AssertEquals("entryLineChargeBO.CF_BaseValue", 135.62m, entryLineChargeBO.CF_BaseValue);
					AssertEquals("entryLineChargeBO.CF_Rate", 0.68m, entryLineChargeBO.CF_Rate);
					AssertEquals("entryLineChargeBO.CF_Rate", "CW1", entryLineChargeBO.CF_Source);
					AssertEquals("entryLineChargeBO.CF_RateOverrideReasonCode", "OTH", entryLineChargeBO.CF_RateOverrideReasonCode);
					AssertEquals("entryLineChargeBO.CF_MethodOfPayment", "PPD", entryLineChargeBO.CF_MethodOfPayment);
					AssertEquals("entryLineChargeBO.CF_MethodOfCalculation", "SUMFUN", entryLineChargeBO.CF_MethodOfCalculation);
				});
			}
		}

		protected EntryLineCharge SetupEntryLineCharge()
		{
			return SetupEntryLineCharge(new CodeDescriptionPair() { Code = "BCL", Description = "BCL DESC" }, 2032.34m, null);
		}

		protected EntryLineCharge SetupEntryLineCharge2()
		{
			return SetupEntryLineCharge(new CodeDescriptionPair() { Code = "WCL", Description = "WCL DESC" }, 1503.53m, "CW1");
		}

		protected EntryLineCharge SetupEntryLineCharge(CodeDescriptionPair type, ZDecimal? amount, ZString? source)
		{
			return new EntryLineCharge()
			{
				Type = type,
				Amount = amount,
				Source = source
			};
		}

		protected void AssertCusEntryLineFeeContents(CusEntryLineFee entryLineChargeBO, ZGuid entryLinePK)
		{
			AssertCusEntryLineFeeContents(entryLineChargeBO, entryLinePK, "BCL", 2032.34m, null);
		}

		protected void AssertCusEntryLineFeeContents2(CusEntryLineFee entryLineChargeBO, ZGuid entryLinePK)
		{
			AssertCusEntryLineFeeContents(entryLineChargeBO, entryLinePK, "WCL", 1503.53m, "CW1");
		}

		protected void AssertCusEntryLineFeeContents(CusEntryLineFee entryLineChargeBO, ZGuid entryLinePK, ZString type, ZDecimal amount, ZString source)
		{
			AssertEquals("entryLineChargeBO.CF_CL", entryLinePK, entryLineChargeBO.CF_CL);
			AssertEquals("entryLineChargeBO.CF_ChargeType", type, entryLineChargeBO.CF_ChargeType);
			AssertEquals("entryLineChargeBO.CF_ChargeAmount", amount, entryLineChargeBO.CF_ChargeAmount);
			AssertEquals("entryLineChargeBO.CF_Source", source, entryLineChargeBO.CF_Source);
		}

		protected EntryLine SetupEntryLine(List<AddInfo> addInfos = null)
		{
			return SetupEntryLine(1, "1020304050", 2585.87m, 0.84m, 800.96m, new CodeDescriptionPair() { Code = "NO", Description = "NO DESC" }, "DESC 1", new CodeDescriptionPair() { Code = "AAA", Description = "TEST 1" }, addInfos);
		}

		protected EntryLine SetupEntryLine2(List<AddInfo> addInfos = null)
		{
			return SetupEntryLine(2, "2010304040", 1502.53m, 0.23m, 1200.40m, new CodeDescriptionPair() { Code = "KG", Description = "KG DESC" }, "DESC 2", new CodeDescriptionPair() { Code = "TTT", Description = "TEST 2" }, addInfos);
		}

		protected EntryLine SetupEntryLine(ZShort? lineNumber, ZString? harmonisedCode, ZDecimal? customsValue, ZDecimal? dutyRatePercent, ZDecimal? dutyRateFlatAmount, CodeDescriptionPair dutyRateFlatAmountUnit, ZString? description, CodeDescriptionPair postedStatus, List<AddInfo> addInfoCollection = null)
		{
			return new EntryLine()
			{
				LineNumber = lineNumber,
				HarmonisedCode = harmonisedCode,
				CustomsValue = customsValue,
				DutyRatePercent = dutyRatePercent,
				DutyRateFlatAmount = dutyRateFlatAmount,
				DutyRateFlatAmountUnit = dutyRateFlatAmountUnit,
				Description = description,
				CustomsStatus = postedStatus,
				AddInfoCollection = addInfoCollection
			};
		}

		protected void AssertCusEntryLineContents(CusEntryLine entryLineBO, ZGuid entryHeaderPK, string addInfo = "")
		{
			AssertCusEntryLineContents(entryLineBO, entryHeaderPK, 1, "1020304050", 2585.87m, 0.84m, 800.96m, "NO", "DESC 1", addInfo);
		}

		protected void AssertCusEntryLineContents2(CusEntryLine entryLineBO, ZGuid entryHeaderPK, string addInfo = "")
		{
			AssertCusEntryLineContents(entryLineBO, entryHeaderPK, 2, "2010304040", 1502.53m, 0.23m, 1200.40m, "KG", "DESC 2", addInfo);
		}

		protected void AssertCusEntryLineContents(CusEntryLine entryLineBO, ZGuid entryHeaderPK, ZShort lineNumber, ZString harmonisedCode, ZDecimal customsValue, ZDecimal dutyRatePercent, ZDecimal dutyRateFlatAmount, ZString dutyRateFlatAmountUnit, ZString description, ZString addInfo)
		{
			AssertEquals("entryLineBO.CL_CH", entryHeaderPK, entryLineBO.CL_CH);
			AssertEquals("entryLineBO.CL_LineNumber", lineNumber, entryLineBO.CL_LineNumber);
			AssertEquals("entryLineBO.CL_AdValoremTariff", harmonisedCode, entryLineBO.CL_AdValoremTariff);
			AssertEquals("entryLineBO.CL_CustomsValue", customsValue, entryLineBO.CL_CustomsValue);
			AssertEquals("entryLineBO.CL_DutyPercent", dutyRatePercent, entryLineBO.CL_DutyPercent);
			AssertEquals("entryLineBO.CL_FlatAmount", dutyRateFlatAmount, entryLineBO.CL_FlatAmount);
			AssertEquals("entryLineBO.CL_FlatAmountUQ", dutyRateFlatAmountUnit, entryLineBO.CL_FlatAmountUQ);
			AssertEquals("entryLineBO.CL_Description", description, entryLineBO.CL_Description);
			AssertEquals("entryLineBO.CL_AddInfo", addInfo, entryLineBO.CL_AddInfo);
			AssertEquals("entryLineBO.CL_CustomsPostedStatus", entryLineBO.CL_CustomsPostedStatus, entryLineBO.CL_CustomsPostedStatus);
		}

		protected UniversalCustoms.EntryNumber SetupEntryNumber()
		{
			return SetupEntryNumber(new EntryType() { Code = "B12", Description = "B12 DESC" }, "B32432", ZBool.True, new EntryStatus() { Code = "CLR", Description = "Clear" }, new ZDateTime(2017, 7, 6), new ZDateTime(2020, 5, 19));
		}

		protected UniversalCustoms.EntryNumber SetupEntryNumber2()
		{
			return SetupEntryNumber(new EntryType() { Code = "W89", Description = "W89 DESC" }, "W986548", ZBool.False, new EntryStatus() { Code = "02", Description = "NotClear" }, new ZDateTime(2017, 7, 7), new ZDateTime(2020, 5, 20));
		}

		protected UniversalCustoms.EntryNumber SetupEntryNumber(EntryType type, ZString? number, ZBool? entryIsSystemGenerated, EntryStatus entryStatus = null, ZDateTime? issueDate = null, ZDateTime? expiryDate = null)
		{
			return new UniversalCustoms.EntryNumber()
			{
				Type = type,
				Number = number,
				EntryIsSystemGenerated = entryIsSystemGenerated,
				EntryStatus = entryStatus,
				IssueDate = issueDate,
				ExpiryDate = expiryDate
			};
		}

		protected void AssertCusEntryNumberContents(CusEntryNumber entryNumberBO, ZString parentTableCode, ZGuid parentID, ZString countryCode, string type = null)
		{
			AssertCusEntryNumberContents(entryNumberBO, parentTableCode, parentID, type ?? "B12", "B32432", ZBool.True, countryCode, "CLR", new ZDateTime(2017, 7, 6), new ZDateTime(2020, 5, 19));
		}

		protected void AssertCusEntryNumberContents2(CusEntryNumber entryNumberBO, ZString parentTable, ZGuid parentID, ZString countryCode, string type = null)
		{
			AssertCusEntryNumberContents(entryNumberBO, parentTable, parentID, type ?? "W89", "W986548", ZBool.False, countryCode, "02", new ZDateTime(2017, 7, 7), new ZDateTime(2020, 5, 20));
		}

		protected void AssertCusEntryNumberContents(CusEntryNumber entryNumberBO, ZString parentTable, ZGuid parentID, ZString type, ZString number, ZBool isSystemGenerated, ZString countryCode, ZString? entryStatusCode = null, ZDateTime? issueDate = null, ZDateTime? expiryDate = null)
		{
			AssertEquals("entryNumberBO.CE_ParentTable", parentTable, entryNumberBO.CE_ParentTable);
			AssertEquals("entryNumberBO.CE_ParentID", parentID, entryNumberBO.CE_ParentID);
			AssertEquals("entryNumberBO.CE_Type", type, entryNumberBO.CE_EntryType);
			AssertEquals("entryNumberBO.CE_EntryNum", number, entryNumberBO.CE_EntryNum);
			AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", isSystemGenerated, entryNumberBO.CE_EntryIsSystemGenerated);
			AssertEquals("entryNumberBO.CE_RN_NKCountryCode", countryCode, entryNumberBO.CE_RN_NKCountryCode);
			if (entryStatusCode.HasValue)
			{
				AssertEquals("entryNumberBO.CE_EntryStatus", entryStatusCode, entryNumberBO.CE_EntryStatus);
			}
			if (issueDate.HasValue)
			{
				AssertEquals("entryNumberBO.CE_IssueDate", issueDate, entryNumberBO.CE_IssueDate);
			}
			if (expiryDate.HasValue)
			{
				AssertEquals("entryNumberBO.CE_ExpiryDate", expiryDate, entryNumberBO.CE_ExpiryDate);
			}
		}

		protected CusEntryNumber[] LoadCusEntryNum(ZString table, ZGuid pk)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, pk);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, table);
			return Factory.Load<CusEntryNumber>(query);
		}

		protected CusEntryNumber CreateCusEntryNumber(ZString parentTable, ZGuid parentID, ZString countryCode, ZString type, ZString number, ZBool isSystemGenerated)
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = parentTable;
			entryNumber.CE_ParentID = parentID;
			entryNumber.CE_RN_NKCountryCode = countryCode;
			entryNumber.CE_EntryType = type;
			entryNumber.CE_EntryNum = number;
			entryNumber.CE_EntryIsSystemGenerated = isSystemGenerated;
			return entryNumber;
		}

		protected PackingLine SetupPackingLine(ZString? billNumber, WayBillType billType, ZString? containerNumber)
		{
			return SetupPackingLine(billNumber, billType, containerNumber, "MORE ART RIPPING KNOT", 12, new PackageType() { Code = Core.Constants.PkgUnit.Box, Description = "Box" }, 13, 10, "SHIPPING SYMBOL ALL OVER");
		}

		protected PackingLine SetupPackingLine2(ZString? billNumber, WayBillType billType, ZString? containerNumber)
		{
			return SetupPackingLine(billNumber, billType, containerNumber, "LOOKS LIKE SNOOPY", 20, new PackageType() { Code = Core.Constants.PkgUnit.Package, Description = "Package" }, 19, 21, "VERY UGLY");
		}

		protected PackingLine SetupPackingLine3(ZString? billNumber, WayBillType billType, ZString? containerNumber)
		{
			return SetupPackingLine(billNumber, billType, containerNumber, "NO MORE CHILDREN", 30, new PackageType() { Code = Core.Constants.PkgUnit.Basket, Description = "Basket" }, 30, 32, "AHH PARENTS");
		}

		protected PackingLine SetupPackingLine(ZString? billNumber, WayBillType billType, ZString? containerNumber, ZString? marksAndNos, ZLong? packQty, PackageType packType, ZInt? inBondPackQty, ZInt? outerPacks, ZString? shippingSymbol)
		{
			var packingLineDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);

			packingLineDataObject.BillNumber = billNumber;
			packingLineDataObject.BillType = billType;
			packingLineDataObject.ContainerNumber = containerNumber;
			packingLineDataObject.MarksAndNos = marksAndNos;
			packingLineDataObject.PackQty = packQty;
			packingLineDataObject.PackType = packType;
			packingLineDataObject.InBondPackQty = inBondPackQty;
			packingLineDataObject.CustomsOuterPacks = outerPacks;
			packingLineDataObject.ShippingSymbol = shippingSymbol;

			return packingLineDataObject;
		}

		protected void AssertContents(BasePackage packageBO, ZString houseBill, ZString containerNumber)
		{
			AssertContents(packageBO, houseBill, containerNumber, "MORE ART RIPPING KNOT", 12, Core.Constants.PkgUnit.Box, 13, 10, "SHIPPING SYMBOL ALL OVER");
		}

		protected void AssertContents2(BasePackage packageBO, ZString houseBill, ZString containerNumber)
		{
			AssertContents(packageBO, houseBill, containerNumber, "LOOKS LIKE SNOOPY", 20, Core.Constants.PkgUnit.Package, 19, 21, "VERY UGLY");
		}

		protected void AssertContents3(BasePackage packageBO, ZString houseBill, ZString containerNumber)
		{
			AssertContents(packageBO, houseBill, containerNumber, "NO MORE CHILDREN", 30, Core.Constants.PkgUnit.Basket, 30, 32, "AHH PARENTS");
		}

		protected void AssertContents(BasePackage packageBO, ZString houseBill, ZString containerNumber, ZString marksAndNos, ZInt packQty, ZString packType, ZInt inBondPackQty, ZInt outerPacks, ZString shippingSymbol)
		{
			AssertEquals("packageBO.CW_HouseBill", houseBill, packageBO.CW_HouseBill);
			AssertEquals("packageBO.CW_ContainerNoOrEquipmentNo", containerNumber, packageBO.CW_ContainerNoOrEquipmentNo);
			AssertEquals("packageBO.CW_MarksAndNos", marksAndNos, packageBO.CW_MarksAndNos);
			AssertEquals("packageBO.CW_PackQty", packQty, packageBO.CW_PackQty);
			AssertEquals("packageBO.CW_PackType", packType, packageBO.CW_PackType);
			AssertEquals("packageBO.CW_InBondPackQty", inBondPackQty, packageBO.CW_InBondPackQty);
			AssertEquals("packageBO.CW_OuterPacks", outerPacks, packageBO.CW_OuterPacks);
			AssertEquals("packageBO.CW_ShippingSymbol", shippingSymbol, packageBO.CW_ShippingSymbol);
		}

		protected CustomsReference SetupCustomsReference(CodeDescriptionPair type, CodeDescriptionPair35Char subType, ZString? reference, ZBool? isOverridden, ZShort? order, ZDateTime? submittedDateToCustoms = null)
		{
			return new CustomsReference
			{
				Type = type,
				SubType = subType,
				Reference = reference,
				IsOverridden = isOverridden,
				Order = order,
				DateCollection = new List<Date>
				{
					new Date
					{
						Type = DateType.DateAtOffice,
						Value = submittedDateToCustoms
					}
				}
			};
		}

		protected void AssertCusCodeDataContents(IColumnIndexer cusCodeDataBO, ZString parentTableCode, ZGuid parentID, ZString type, ZString code, ZString data, ZBool isOverridden, ZShort order, ZDateTime? date = null)
		{
			AssertEquals("cusCodeDataBO.CY_ParentTableCode", parentTableCode, cusCodeDataBO[CusCodeDataSchema.Constants.CY_ParentTableCode]);
			AssertEquals("cusCodeDataBO.CY_ParentID", parentID, cusCodeDataBO[CusCodeDataSchema.Constants.CY_ParentID]);
			AssertEquals("cusCodeDataBO.CY_Type", type, cusCodeDataBO[CusCodeDataSchema.Constants.CY_Type]);
			AssertEquals("cusCodeDataBO.CY_Code", code, cusCodeDataBO[CusCodeDataSchema.Constants.CY_Code]);
			AssertEquals("cusCodeDataBO.CY_Data", data, cusCodeDataBO[CusCodeDataSchema.Constants.CY_Data]);
			AssertEquals("cusCodeDataBO.CY_IsOverridden", isOverridden, cusCodeDataBO[CusCodeDataSchema.Constants.CY_IsOverridden]);
			AssertEquals("cusCodeDataBO.CY_Order", order, cusCodeDataBO[CusCodeDataSchema.Constants.CY_Order]);
			AssertEquals("cusCodeDataBO.CY_Date", date ?? ZDateTime.Empty, cusCodeDataBO[CusCodeDataSchema.Constants.CY_Date] == DBNull.Value ? ZDateTime.Empty : cusCodeDataBO[CusCodeDataSchema.Constants.CY_Date]);
		}

		protected IColumnIndexer[] LoadCusCodeData(ZString prefix, ZGuid pk)
		{
			return LoadCusCodeData(Factory.RowFactory, prefix, pk);
		}

		protected IColumnIndexer[] LoadCusCodeData(RowFactory factory, ZString prefix, ZGuid pk)
		{
			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, pk);
			query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, prefix);
			return UniversalDataObjectReaderHelperAbstractTest.ConvertToColumnIndexer(factory.Load(CusCodeDataSchema.Constants.TableName, query));
		}

		protected CustomsSupportingInformation SetupCustomsSupportingInformation(CodeDescriptionPair category, ZString? type = null, ZString? country = null, ZString? customsOffice = null, ZDate? dateOfIssue = null, ZString? description = null, ZShort? lineNo = null, ZString? procedure = null, ZDecimal? quantity = null, ZDecimal? quantity2 = null, ZString? referenceNumber = null, ZString? status = null, ZString? subType = null, ZString? unitOfQuantity = null, ZString? unitOfQuantity2 = null)
		{
			return new CustomsSupportingInformation()
			{
				Category = category,
				Type = type.HasValue ? new CodeDescriptionPair6Char() { Code = type.Value } : null,
				Country = country.HasValue ? new Country() { Code = country.Value } : null,
				CustomsOffice = customsOffice.HasValue ? new CodeDescriptionPair10Char() { Code = customsOffice.Value } : null,
				DateOfIssue = dateOfIssue,
				Description = description,
				LineNo = lineNo,
				Procedure = procedure.HasValue ? new CodeDescriptionPair7Char() { Code = procedure.Value } : null,
				Quantity = quantity,
				Quantity2 = quantity2,
				ReferenceNumber = referenceNumber,
				Status = status.HasValue ? new CodeDescriptionPair() { Code = status.Value } : null,
				SubType = subType.HasValue ? new CodeDescriptionPair5Char() { Code = subType.Value } : null,
				UnitOfQuantity = unitOfQuantity.HasValue ? new CodeDescriptionPair4Char() { Code = unitOfQuantity.Value } : null,
				UnitOfQuantity2 = unitOfQuantity2.HasValue ? new CodeDescriptionPair4Char() { Code = unitOfQuantity2.Value } : null
			};
		}

		protected void AssertCusSupportingInfoContents(
					IColumnIndexer cusSupportingInfoBO,

					ZString parentTableCode,
					ZGuid parentID,
					ZString type,
					ZString? code = null,
					ZString? country = null,

					ZString? customsOffice = null,
					ZDate? dateOfIssue = null,
					ZString? description = null,
					ZShort? lineNo = null,
					ZString? procedure = null,

					ZDecimal? quantity = null,
					ZDecimal? quantity2 = null,
					ZDecimal? quantity3 = null,
					ZString? referenceNumber = null,
					ZString? referenceNumber2 = null,
					ZInt? itemNumber = null,
					ZString? status = null,

					ZString? subType = null,
					ZString? tariff = null,
					ZString? unitOfQuantity = null,
					ZString? unitOfQuantity2 = null,
					ZString? unitOfQuantity3 = null,

					ZDate? dateOfExpiry = null,
					ZString? valueCurrency = null,
					ZDecimal? value = null,

					ZString? additionalDescription = null,
					ZString? issuerType = null,
					ZInt? packQty = null,
					ZString? packType = null
					)
		{
			AssertEquals("cusSupportingInfoBO.CSI_ParentTableCode", parentTableCode, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_ParentTableCode));
			AssertEquals("cusSupportingInfoBO.CSI_ParentID", parentID, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_ParentID));
			AssertEquals("cusSupportingInfoBO.CSI_Type", type, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Type));
			if (code.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Code", code.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Code));
			}
			if (country.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_RN_NKCountryCode", country.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_RN_NKCountryCode));
			}
			if (customsOffice.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_CustomsOffice", customsOffice.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_CustomsOffice));
			}
			if (dateOfIssue.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_DateOfIssue", dateOfIssue.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_DateOfIssue));
			}
			if (description.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Description", description.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Description));
			}
			if (lineNo.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_LineNo", lineNo.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_LineNo));
			}
			if (procedure.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Procedure", procedure.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Procedure));
			}
			if (quantity.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Quantity", quantity.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Quantity));
			}
			if (quantity2.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Quantity2", quantity2.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Quantity2));
			}
			if (quantity3.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Quantity3", quantity3.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Quantity3));
			}
			if (referenceNumber.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_ReferenceNumber", referenceNumber.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_ReferenceNumber));
			}
			if (referenceNumber2.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_ReferenceNumber2", referenceNumber2.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_ReferenceNumber2));
			}
			if (itemNumber.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.itemNumber", itemNumber.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_ItemNumber));
			}
			if (status.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Status", status.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Status));
			}
			if (subType.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_SubType", subType.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_SubType));
			}
			if (tariff.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Tariff", tariff.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Tariff));
			}
			if (unitOfQuantity.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_UnitOfQuantity", unitOfQuantity.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_UnitOfQuantity));
			}
			if (unitOfQuantity2.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_UnitOfQuantity2", unitOfQuantity2.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_UnitOfQuantity2));
			}
			if (unitOfQuantity3.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_UnitOfQuantity3", unitOfQuantity3.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_UnitOfQuantity3));
			}
			if (dateOfExpiry.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_DateOfExpiry", dateOfExpiry.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_DateOfExpiry));
			}
			if (valueCurrency.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_RX_NKCurrency", valueCurrency.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_RX_NKCurrency));
			}
			if (value.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_Value", value.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_Value));
			}
			if (additionalDescription.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_AdditionalDescription", additionalDescription.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_AdditionalDescription));
			}
			if (issuerType.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_IssuerType", issuerType.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_IssuerType));
			}
			if (packQty.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_PackQty", packQty.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_PackQty));
			}
			if (packType.HasValue)
			{
				AssertEquals("cusSupportingInfoBO.CSI_PackType", packType.Value, cusSupportingInfoBO.GetValue(CusSupportingInfoSchema.CSI_PackType));
			}
		}

		protected IColumnIndexer[] LoadCusSupportingInfo(ZString prefix, ZGuid pk)
		{
			return LoadCusSupportingInfo(Factory.RowFactory, prefix, pk);
		}

		protected IColumnIndexer[] LoadCusSupportingInfo(RowFactory factory, ZString prefix, ZGuid pk)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, pk);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, prefix);
			return UniversalDataObjectReaderHelperAbstractTest.ConvertToColumnIndexer(factory.Load(CusSupportingInfoSchema.Constants.TableName, query));
		}

		protected class InfoValueChangeData
		{
			public InfoValueChangeData(ZPropertyInfo info, IZType oldValue, IZType newValue)
			{
				this.Info = info;
				this.OldValue = oldValue;
				this.NewValue = newValue;
			}
			public readonly ZPropertyInfo Info;
			public readonly IZType OldValue;
			public readonly IZType NewValue;
		}

		protected void AssertDataWasSetInSpecificOrder(List<InfoValueChangeData> expectedOrders, Action populateData)
		{
			foreach (var order in expectedOrders.ToArray())
			{
				var localOrder = order;
				var info = localOrder.Info;
				info.Value = order.OldValue.Default;
				info.ValueChanged -= (object sender, EventArgs e) => { AssertRightSetterWasCalled(expectedOrders, localOrder, e); };
				info.ValueChanged += (object sender, EventArgs e) => { AssertRightSetterWasCalled(expectedOrders, localOrder, e); };
			}

			CombineAssertions(delegate
			{
				discrepancyDataCollection = new ZStringBuilder();
				invalidOrderCollection = new ZStringBuilder();
				populateData();

				if (!invalidOrderCollection.IsEmpty)
				{
					invalidOrderCollection.Prepend("Invalid Setting Order:");
					invalidOrderCollection.AppendLine();
				}
				if (expectedOrders.Count > 0)
				{
					invalidOrderCollection.Append("The following setters haven't been called:");
					foreach (var value in expectedOrders.Select<InfoValueChangeData, ZString>(x => x.Info.Name))
					{
						invalidOrderCollection.Append(value);
					}
				}

				if (invalidOrderCollection.IsEmpty)
				{
					Assert("WELL DONE", true);
				}
				else
				{
					if (!discrepancyDataCollection.IsEmpty)
					{
						invalidOrderCollection.AppendLine();
						invalidOrderCollection.Append("Actual Changes:");
						invalidOrderCollection.Append(discrepancyDataCollection);
					}
					Fail(invalidOrderCollection.ToStringWithNewLineBetweenAppends());
				}
			});

			foreach (var order in expectedOrders.ToArray())
			{
				var localOrder = order;
				var info = localOrder.Info;
				info.ValueChanged -= (object sender, EventArgs e) => { AssertRightSetterWasCalled(expectedOrders, localOrder, e); };
			}
		}

		ZStringBuilder discrepancyDataCollection;

		ZStringBuilder invalidOrderCollection;

		protected void AssertRightSetterWasCalled(List<InfoValueChangeData> expectedOrders, InfoValueChangeData expectedInfo, EventArgs e)
		{
			var changeEventArgs = e as ValueChangedEventArgs;
			var propertyName = expectedInfo.Info.Name;
			if (changeEventArgs != null && changeEventArgs.Info.Name == propertyName)
			{
				var eventBizObj = GetInnerMostInfo(changeEventArgs.Info).BizObj;
				var expectedInfoBizObj = GetInnerMostInfo(expectedInfo.Info).BizObj;
				if (eventBizObj == expectedInfoBizObj)
				{
					var newValue = expectedInfo.NewValue;
					var oldValue = expectedInfo.OldValue;
					if (newValue.Equals(changeEventArgs.NewValue) && oldValue.Equals(changeEventArgs.OldValue))
					{
						if (expectedOrders.Count > 0)
						{
							var firstOrder = expectedOrders[0];
							var firstOrderName = firstOrder.Info.Name;
							var firstOrderBizObj = GetInnerMostInfo(firstOrder.Info).BizObj;
							expectedOrders.Remove(firstOrder);
							if (firstOrderName != propertyName || firstOrderBizObj != expectedInfoBizObj)
							{
								if (expectedOrders[0].Info.Name == propertyName && GetInnerMostInfo(expectedOrders[0].Info).BizObj == expectedInfoBizObj)
								{
									expectedOrders.RemoveAt(0);
								}
								invalidOrderCollection.Append(string.Format("{0}({1}) should be called before {2}({3}).", firstOrder.Info.HumanReadableName, firstOrderName, changeEventArgs.Info.HumanReadableName, propertyName));
							}
						}
					}
					else
					{
						discrepancyDataCollection.Append(string.Format("{0}({3}) OldValue='{1}' NewValue='{2}'", changeEventArgs.Info.HumanReadableName, changeEventArgs.OldValue, changeEventArgs.NewValue, propertyName));
					}
				}
			}
		}

		ZPropertyInfo GetInnerMostInfo(ZPropertyInfo info)
		{
			var result = info;
			var wrappedInfo = result as ZWrappedPropertyInfo;
			if (wrappedInfo != null)
			{
				result = GetInnerMostInfo(wrappedInfo.InnerInfo);
			}
			return result;
		}

		protected UniversalDataObjectReaderHelper CurrentCompanyHelper
		{
			get { return currentCompanyHelper ?? (currentCompanyHelper = GetNewCurrentCompanyHelper()); }
		}

		UniversalDataObjectReaderHelper currentCompanyHelper;

		protected virtual UniversalDataObjectReaderHelper GetNewCurrentCompanyHelper()
		{
			return new UniversalDataObjectReaderHelper(Factory, CurrentCompany.GC_RN_NKCountryCode, CurrentCompany.GC_RN_NKCountryCode);
		}

		protected UniversalShipment SetupDeclaration(ZString? ownerRef, ZString? wayBillNumber, WayBillType wayBillType, List<AddInfo> addInfos = null, DataContextType dataContextType = DataContextType.CustomsDeclaration, List<EntryHeader> addEntryHeaders = null)
		{
			return SetupDeclaration(new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import, Description = JobMessageTypeList.Descriptions.Import },
				new CodeDescriptionPair() { Code = "ST1", Description = "STANDARD" },
				new ContainerMode() { Code = Core.Constants.ContainerModes.Containerised, Description = "Containerized" },
				"RAT HATS",
				new UNLOCO() { Code = "NZDUD", Name = "Dunedin" },
				new UNLOCO() { Code = "NZCHC", Name = "Christchurch" },
				new UNLOCO() { Code = "AUNTL", Name = "Newcastle" },
				new UNLOCO() { Code = "AUSYD", Name = "Sydney" },
				new UNLOCO() { Code = "AUBDG", Name = "Bendigo" },
				"BUNGA DELIMA",
				"343L",
				0,
				45,
				new PackageType() { Code = "KEG", Description = "Keg" },
				23.45m,
				new UnitOfVolume() { Code = "CF", Description = "Cubic Feet" },
				34.56m,
				new UnitOfWeight() { Code = "KT", Description = "Kilotons" },
				new CodeDescriptionPair() { Code = "SEA", Description = "Sea Freight" },
				ownerRef,
				wayBillNumber,
				wayBillType,
				addInfos,
				new ServiceLevel() { Code = ServiceLevel1.RS_Code, Description = ServiceLevel1.RS_Description },
				ZBool.True, new CodeDescriptionPair() { Code = "EFT", Description = "EFT MODE" }, new CodeDescriptionPair() { Code = OrgConstants.MergeInvoiceLines.Tariff, Description = "Tariff" }, 112, 306,
				"7819369", new CodeDescriptionPair() { Code = "SP", Description = "STANDARD P" }, "AGREF123", "F234",
				new CodeDescriptionPair() { Code = PaymentPartyCodeDescriptionList.Codes.Broker, Description = PaymentPartyCodeDescriptionList.Descriptions.Broker },
				new CodeDescriptionPair() { Code = MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, Description = MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK },
				new IncoTerm() { Code = "FOB", Description = "FREE ON BOARD" }, 789.012m, dataContextType, "INCO PLACE", new CodeDescriptionPair() { Code = "IFD" }, addEntryHeaders);
		}

		protected UniversalShipment SetupDeclaration(CodeDescriptionPair messageType, CodeDescriptionPair messageSubType, ContainerMode customsContainerMode, ZString? goodsDescription, UNLOCO portOfOrigin, UNLOCO portOfLoading, UNLOCO portOfFirstArrival, UNLOCO portOfDischarge, UNLOCO portOfDestination, ZString? vesselName, ZString? voyageFlightNo, ZInt? containerCount, ZInt? outerPack, PackageType outerPackType, ZDecimal? totalVolume, UnitOfVolume totalVolumeUnit, ZDecimal? totalWeight, UnitOfWeight totalWeightUnit, CodeDescriptionPair transportMode, ZString? ownerRef, ZString? wayBillNumber, WayBillType wayBillType, List<AddInfo> addInfoCollection,
					ServiceLevel serviceLevel, ZBool? isPersonalEffects, CodeDescriptionPair eftMode, CodeDescriptionPair mergeBy, ZInt? totalNoOfPieces, ZInt? totalNoOfPiecesLanded,
					ZString? lloydsIMO, CodeDescriptionPair exportGoodsType, ZString? agentsReference, ZString? folio, CodeDescriptionPair paymentMethod, CodeDescriptionPair paidBy, IncoTerm shipmentIncoTerm, ZDecimal? totalNoOfPacksDecimal,
					DataContextType dataContextType = DataContextType.CustomsDeclaration, ZString? shipmentIncoTermPlace = null, CodeDescriptionPair declarantType = null, List<EntryHeader> addEntryHeaders = null)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.CodesMappedToTarget = true; // Required to import JobCosting
			dataContext.AddDataTarget(dataContextType, null);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = messageType,
				MessageSubType = messageSubType,
				CustomsContainerMode = customsContainerMode,
				GoodsDescription = goodsDescription,
				PortOfOrigin = portOfOrigin,
				PortOfLoading = portOfLoading,
				PortOfFirstArrival = portOfFirstArrival,
				PortOfDischarge = portOfDischarge,
				PortOfDestination = portOfDestination,
				VesselName = vesselName,
				VoyageFlightNo = voyageFlightNo,
				ContainerCount = containerCount,
				OuterPacks = outerPack,
				OuterPacksPackageType = outerPackType,
				TotalVolume = totalVolume,
				TotalVolumeUnit = totalVolumeUnit,
				TotalWeight = totalWeight,
				TotalWeightUnit = totalWeightUnit,
				TransportMode = transportMode,
				OwnerRef = ownerRef,
				WayBillNumber = wayBillNumber,
				WayBillType = wayBillType,
				ServiceLevel = serviceLevel,
				IsPersonalEffects = isPersonalEffects,
				EFTMode = eftMode,
				MergeBy = mergeBy,
				TotalNoOfPieces = totalNoOfPieces,
				TotalNoOfPiecesLanded = totalNoOfPiecesLanded,
				LloydsIMO = lloydsIMO,
				ExportGoodsType = exportGoodsType,
				AgentsReference = agentsReference,
				Folio = folio,
				PaymentMethod = paymentMethod,
				PaidBy = paidBy,
				ShipmentIncoTerm = shipmentIncoTerm,
				AdditionalTerms = shipmentIncoTermPlace,
				TotalNoOfPacksDecimal = totalNoOfPacksDecimal,
				DeclarantType = declarantType
			};
			shipment.SetAddInfoCollection(() => addInfoCollection);
			shipment.SetEntryHeaderCollection(() => addEntryHeaders);
			return shipment;
		}

		protected void AssertContents(BaseJobDeclaration declarationBO, ZString ownerRef, ZString masterBill, ZString houseBill, short containerCount = 0, string addInfoString = "")
		{
			AssertContents(declarationBO, JobMessageTypeList.Codes.Import, Core.Constants.ContainerModes.Containerised, "RAT HATS", "NZDUD",
				"NZCHC", "", "AUSYD", "AUBDG", "BUNGA DELIMA",
				"343L", containerCount, 45, "KEG", 23.45m,
				"CF", 34.56m, "KT", Core.Constants.TransportModes.Sea, ownerRef,
				masterBill, houseBill, addInfoString, ServiceLevel1.RS_Code, ZBool.True,
				"EFT", OrgConstants.MergeInvoiceLines.Tariff, 112, 306, "7819369",
				"SP", "AGREF123", "F234", PaymentPartyCodeDescriptionList.Codes.Broker, MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, "FOB",
				789.012m, "INCO PLACE", "IFD");
		}

		protected void AssertContents(BaseJobDeclaration declarationBO, ZString messageType, ZString customsContainerMode, ZString goodsDescription, ZString origin,
					ZString portOfLoading, ZString portOfFirstArrival, ZString portOfArrival, ZString finalDestination, ZString vessel,
					ZString voyageFlightNo, ZShort containerCount, ZInt totalNoOfPacks, ZString totalNoOfPacksPackType, ZDecimal totalVolume,
					ZString totalVolumeUnit, ZDecimal totalWeight, ZString totalWeightUnit, ZString transportMode, ZString ownerRef,
					ZString masterBill, ZString houseBill, ZString addInfo, ZString serviceLevel, ZBool isPersonalEffects,
					ZString eftMode, ZString mergeBy, ZInt totalNoOfPieces, ZInt totalNoOfPiecesLanded, ZString lloydsIMO,
					ZString exportGoodsType, ZString agentsReference, ZString folio, ZString paymentMethod, ZString paidBy, ZString shipmentIncoTerm,
					ZDecimal totalNoOfPacksDecimal, ZString? shipmentIncoTermPlace = null, ZString? declarantType = null)
		{
			AssertEquals("declarationBO.JE_MessageType", messageType, declarationBO.JE_MessageType);
			AssertEquals("declarationBO.JE_ContainerMode", customsContainerMode, declarationBO.JE_ContainerMode);
			AssertEquals("declarationBO.JE_GoodsDescription", goodsDescription, declarationBO.JE_GoodsDescription);
			AssertEquals("declarationBO.JE_RL_NKOrigin", origin, declarationBO.JE_RL_NKOrigin);
			AssertEquals("declarationBO.JE_RL_NKPortOfLoading", portOfLoading, declarationBO.JE_RL_NKPortOfLoading);
			AssertEquals("declarationBO.JE_RL_NKPortOfFirstArrival", portOfFirstArrival, declarationBO.JE_RL_NKPortOfFirstArrival);
			AssertEquals("declarationBO.JE_RL_NKPortOfArrival", portOfArrival, declarationBO.JE_RL_NKPortOfArrival);
			AssertEquals("declarationBO.JE_RL_NKFinalDestination", finalDestination, declarationBO.JE_RL_NKFinalDestination);
			AssertEquals("declarationBO.JE_VesselName", vessel, declarationBO.JE_VesselName);
			AssertEquals("declarationBO.JE_VoyageFlightNo", voyageFlightNo, declarationBO.JE_VoyageFlightNo);
			AssertEquals("declarationBO.JE_MasterBill", masterBill, declarationBO.JE_MasterBill);
			AssertEquals("declarationBO.JE_HouseBill", houseBill, declarationBO.JE_HouseBill);
			AssertEquals("declarationBO.JE_ContainerCount", containerCount, declarationBO.JE_ContainerCount);
			AssertEquals("declarationBO.JE_TotalNoOfPacks", totalNoOfPacks, declarationBO.JE_TotalNoOfPacks);
			AssertEquals("declarationBO.JE_TotalNoOfPacksPackType", totalNoOfPacksPackType, declarationBO.JE_TotalNoOfPacksPackType);
			AssertEquals("declarationBO.JE_TotalVolume", totalVolume, declarationBO.JE_TotalVolume);
			AssertEquals("declarationBO.JE_TotalVolumeUnit", totalVolumeUnit, declarationBO.JE_TotalVolumeUnit);
			AssertEquals("declarationBO.JE_TotalWeight", totalWeight, declarationBO.JE_TotalWeight);
			AssertEquals("declarationBO.JE_TotalWeightUnit", totalWeightUnit, declarationBO.JE_TotalWeightUnit);
			AssertEquals("declarationBO.JE_TransportMode", transportMode, declarationBO.JE_TransportMode);
			AssertEquals("declarationBO.JE_OwnerRef", ownerRef, declarationBO.JE_OwnerRef);
			AssertEquals("declarationBO.JE_MasterBill", masterBill, declarationBO.JE_MasterBill);
			AssertEquals("declarationBO.JE_HouseBill", houseBill, declarationBO.JE_HouseBill);
			if (shipmentIncoTermPlace.HasValue)
			{
				AssertEquals("JE_ShipmentIncoTermPlace.JE_HouseBill", shipmentIncoTermPlace.Value, declarationBO.JE_ShipmentIncoTermPlace);
			}
			if (declarantType.HasValue)
			{
				AssertEquals("declarationBO.JE_DeclarantType", declarantType.Value, declarationBO.JE_DeclarantType);
			}
			AssertEquals("declarationBO.JE_RS_NKServiceLevel", serviceLevel, declarationBO.JE_RS_NKServiceLevel);
			AssertEquals("declarationBO.JE_IsPersonalEffects", isPersonalEffects, declarationBO.JE_IsPersonalEffects);
			AssertEquals("declarationBO.JE_EFTMode", eftMode, declarationBO.JE_EFTMode);
			AssertEquals("declarationBO.JE_MergeBy", mergeBy, declarationBO.JE_MergeBy);
			AssertEquals("declarationBO.JE_TotalNoOfPieces", totalNoOfPieces, declarationBO.JE_TotalNoOfPieces);
			AssertEquals("declarationBO.JE_LandedPieces", totalNoOfPiecesLanded, declarationBO.JE_LandedPieces);
			AssertEquals("declarationBO.JE_LloydsIMO", lloydsIMO, declarationBO.JE_LloydsIMO);
			AssertEquals("declarationBO.JE_ExportGoodsType", exportGoodsType, declarationBO.JE_ExportGoodsType);
			AssertEquals("declarationBO.JE_AgentsReference", agentsReference, declarationBO.JE_AgentsReference);
			AssertEquals("declarationBO.JE_Folio", folio, declarationBO.JE_Folio);
			AssertEquals("declarationBO.JE_PaymentMethod", paymentMethod, declarationBO.JE_PaymentMethod);
			AssertEquals("declarationBO.JE_PaidBy", paidBy, declarationBO.JE_PaidBy);
			AssertEquals("declarationBO.JE_ShipmentIncoTerm", shipmentIncoTerm, declarationBO.JE_ShipmentIncoTerm);
			AssertEquals("declarationBO.JE_TotalNoOfPacksDecimal", totalNoOfPacksDecimal, declarationBO.JE_TotalNoOfPacksDecimal);

			AssertEquals("declarationBO.JE_AddInfo", addInfo, declarationBO.JE_AddInfo);
		}

		protected void AssertAdditionalReferenceNumberContents(CusEntryNumber additionalReferenceBO)
		{
			AssertContents(additionalReferenceBO, CusEntryNumber.Categories.AdditionalReferenceNumber, "CE00001", "AMS", "INFORMER", true, CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2011, 3, 3));
		}

		protected void AssertAdditionalReferenceNumberContents2(CusEntryNumber additionalReferenceBO)
		{
			AssertContents(additionalReferenceBO, CusEntryNumber.Categories.AdditionalReferenceNumber, "DG00005", "CON", "NOTICED", true, CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2011, 4, 3));
		}

		protected void AssertContents(CusEntryNumber entryNumberBO, ZString category, ZString entryNum, ZString entryType, ZString entryLineReference, ZBool entryIsSystemGenerated, ZString countryCode, ZDateTime issueDate)
		{
			AssertEquals("entryNumberBO.CE_Category", category, entryNumberBO.CE_Category);
			AssertEquals("entryNumberBO.CE_EntryNum", entryNum, entryNumberBO.CE_EntryNum);
			AssertEquals("entryNumberBO.CE_EntryType", entryType, entryNumberBO.CE_EntryType);
			//AssertEquals("entryNumberBO.CE_EntryLineReference", entryLineReference, entryNumberBO.CE_EntryLineReference);// speak to Ben before adding this back
			AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", entryIsSystemGenerated, entryNumberBO.CE_EntryIsSystemGenerated);
			AssertEquals("entryNumberBO.CE_RN_NKCountryCode", countryCode, entryNumberBO.CE_RN_NKCountryCode);
			AssertEquals("entryNumberBO.CE_IssueDate", issueDate, entryNumberBO.CE_IssueDate);
		}

		protected List<AddInfo> SetupAddInfos(ZString addInfoString)
		{
			return AddInfoCollectionCreator.CreateCollection(addInfoString);
		}
	}
}
