using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class CommercialInvoiceHeaderDataObjectWriter : DataObjectWriter<BaseJobComInvoiceHeader, UniversalCustoms.CommercialInvoiceHeader>
	{
		public CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, CusEntryHeader relatedEntry = null)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
			this.landedCostDataWriter = landedCostDataWriter;
			this.relatedEntryInstruction = relatedEntry?.EntryInstruction;
			this.needsOwnerProductDetails = (relatedEntryInstruction?.HasBothOutOfAndIntoRegimeProcedure ?? false) && manager.HasRecipientRoleDetail(RecipientRoleType.BWI);
		}
		readonly ILandedCostDataWriter landedCostDataWriter;
		protected readonly UniversalDataObjectWriterHelper helper;
		readonly bool needsOwnerProductDetails;
		readonly CusEntryInstruction relatedEntryInstruction;

		protected override UniversalCustoms.CommercialInvoiceHeader PopulateDataObject(BaseJobComInvoiceHeader invoiceBO)
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(writeManager.WriterStrategy)
			{
				InvoiceDate = invoiceBO.JZ_InvoiceDate,
				ValuationDateOverride = invoiceBO.JZ_ValuationDateOverride,
				InvoiceNumber = invoiceBO.JZ_InvoiceNumber,
				InvoiceAmount = invoiceBO.JZ_InvoiceAmount,
				IncoTerm = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceBO.JZ_IncoTerm, invoiceBO.Lookups.JZ_IncoTerm_List),
				//TODO - JZ_RN_NKDefaultOrigin
				AdditionalTerms = invoiceBO.JZ_IncoTermPlace,
				DeliveryTerms = invoiceBO.JZ_AdditionalTerms,
				Volume = invoiceBO.JZ_Volume,
				VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(invoiceBO.JZ_VolumeUQ, invoiceBO.Lookups.JZ_VolumeUQ_List),
				Weight = invoiceBO.JZ_Weight,
				WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(invoiceBO.JZ_WeightUQ, invoiceBO.Lookups.JZ_WeightUQ_List),
				NetWeight = invoiceBO.JZ_NetWeight,
				NetWeightUQ = ListHelper.GetWithDescription<UnitOfWeight>(invoiceBO.JZ_NetWeightUQ, invoiceBO.Lookups.JZ_WeightUQ_List),
				LandedCostExchangeRate = invoiceBO.JZ_InvoiceCurrLandedCostExRate,
				//TODO - JZ_OverrideFOB
				//TODO - JZ_FOBValue
				PaymentNumber = invoiceBO.JZ_PaymentNo,
				PaymentAmount = invoiceBO.JZ_PaymentAmount,
				PaymentExchangeRate = invoiceBO.JZ_PaymentExRate,
				PaymentDate = invoiceBO.JZ_PaymentDate,

				ExchangeRateType = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceBO.JZ_InvoiceCurrExRateType, invoiceBO.Lookups.ExchangeRateTypeList),
				MessageStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceBO.JZ_MessageStatus, invoiceBO.Lookups.MessageStatusList),
				//TODO - JZ_InvoiceDisplaySequence
				InvoiceCurrency = ListHelper.GetWithDescription<Currency>(invoiceBO.JZ_RX_NKInvoice_Currency, invoiceBO.Lookups.Invoice_Currencies),
				NoOfPacks = invoiceBO.JZ_NoOfPacks,
				RelatedIndicator = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceBO.JZ_RelatedIndicator, invoiceBO.Lookups.RelatedIndicatorList),
				ValuationCode = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceBO.JZ_ValuationCode, invoiceBO.Lookups.ValuationCodeList),
				AddInfoCollection = GetInvoiceAddInfoCollection(invoiceBO),
			};

			if (landedCostDataWriter != null)
			{
				invoiceData.TransportLogisticsCostCollection = landedCostDataWriter.PopulateTransportLogisticsCostCollection(invoiceBO);
			}

			var notes = invoiceBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			invoiceData.NoteCollection = ProcessCollection(notes, new NoteDataObjectWriter(writeManager));
			PopulateCommercialInvoiceOrganizationAddressData(invoiceBO, invoiceData);

			invoiceData.AgreedExchangeRate = invoiceBO.JZ_InvoiceCurrExRate;

			if (invoiceBO.SupportsRelatedBill)
			{
				var bill = invoiceBO.Bill;
				if (bill != null)
				{
					invoiceData.BillNumber = bill.CU_BillNum;
					invoiceData.BillType = ListHelper.GetWithDescription<WayBillType>(Constants.GetWayBillType(bill.CU_BillType), helper.WayBillTypeList);
				}
				else
				{
					invoiceData.BillNumber = "";
				}
			}

			invoiceData.CommercialChargeCollection = CommercialChargeCollectionCreator.CreateCollection(helper, invoiceBO.PK);
			invoiceData.AddInfoGroupCollection = GetInvoiceAddInfoGroupCollection(invoiceBO);
			invoiceData.SetCustomsReferenceCollection(() => CustomsReferenceCollectionCreator.CreateCollection(helper, invoiceBO, writeManager));
			invoiceData.SetCustomsSupportingInformationCollection(() => invoiceBO is Integration.Customs.ICusSupportingInfoTypeSupporter ? GetInvoiceCustomsSupportingInformationCollectionCore(invoiceBO) : null);
			invoiceData.PackingLinkCollection = GetPackingLinkCollection(invoiceBO);

			PopulateCommercialInvoiceLineData(invoiceBO, invoiceData);

			return invoiceData;
		}

		protected List<PackingLink> GetPackingLinkCollection(BaseJobComInvoiceHeader invoiceBO)
		{
			var result = new List<PackingLink>();
			var packagePivots = invoiceBO.PackagesPivot.Cast<InvoiceHeaderPackagePivot>();
			foreach (var pivot in packagePivots)
			{
				var package = pivot.Package;
				if (!package.Children.Any())
				{
					var packingLink = new PackingLink
					{
						PackingLineLink = helper.GetPackingLineLink(package),
						PackedQuantity = (ZDecimal)pivot.CHZ_NumberOfPacks
					};

					result.Add(packingLink);
				}
			}

			return result.Count > 0 ? result : null;
		}

		protected virtual List<UniversalCustoms.CustomsSupportingInformation> GetInvoiceCustomsSupportingInformationCollectionCore(BaseJobComInvoiceHeader invoiceBO)
		{
			return CustomsSupportingInformationCollectionCreator.CreateCollection(helper, invoiceBO, writeManager);
		}

		protected virtual List<AddInfo> GetInvoiceAddInfoCollection(BaseJobComInvoiceHeader invoiceBO)
		{
			return AddInfoCollectionCreator.CreateCollection(invoiceBO, JobComInvoiceHeaderSchema.JZ_AddInfo);
		}

		protected virtual List<UniversalCustoms.AddInfoGroup> GetInvoiceAddInfoGroupCollection(BaseJobComInvoiceHeader invoiceBO)
		{
			return AddInfoGroupCollectionCreator.CreateCollection(helper, invoiceBO, writeManager);
		}

		protected virtual void PopulateCommercialInvoiceOrganizationAddressData(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.Supplier = helper.CreateOrganizationAddressFromOrganization(writeManager, invoiceBO.Supplier, AddressTypes.Supplier);
			invoiceData.Buyer = helper.CreateOrganizationAddressFromOrganization(writeManager, invoiceBO.Buyer, AddressTypes.Importer);

			PopulateShipToParty(invoiceBO, invoiceData);
			PopulateSeller(invoiceBO, invoiceData);
			PopulateManufacturer(invoiceBO, invoiceData);
			PopulateSoldToPartyAddress(invoiceBO, invoiceData);
			PopulateConsigneeAddress(invoiceBO, invoiceData);
			PopulateConsignee(invoiceBO, invoiceData);
			PopulateSellingAgent(invoiceBO, invoiceData);
			PopulateBuyerAgent(invoiceBO, invoiceData);
			PopulateExporterAddress(invoiceBO, invoiceData);
			PopulateBuyerAddress(invoiceBO, invoiceData);
			PopulateSupplierAddress(invoiceBO, invoiceData);
			PopulateIntermConsigneeAddress(invoiceBO, invoiceData);
			PopulateDocAddresses(invoiceBO, invoiceData);
		}

		protected void PopulateShipToParty(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_ShipToPartyAddress), DocAddressType.ShipToParty);
		}

		protected void PopulateSeller(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_SellerAddress), Constants.AddressTypes.Seller);
		}

		protected void PopulateManufacturer(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_ManufacturerAddress), DocAddressType.Manufacturer);
		}

		protected void PopulateSoldToPartyAddress(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_SoldToPartyAddress), Constants.AddressTypes.SoldToParty);
		}

		protected void PopulateBuyerAgent(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgHeader>(invoiceBO.JZ_OH_BuyerAgent), Constants.AddressTypes.BuyingAgent);
		}

		protected void PopulateSellingAgent(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgHeader>(invoiceBO.JZ_OH_SellingAgent), Constants.AddressTypes.SellingAgent);
		}

		protected void PopulateExporterAddress(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_ExporterAddress), DocAddressType.Exporter);
		}

		protected void PopulateConsigneeAddress(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_ConsigneeAddress), Constants.AddressTypes.UltimateConsignee);
		}

		protected void PopulateConsignee(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgHeader>(invoiceBO.JZ_OH_Consignee), Constants.AddressTypes.IntermediateConsignee);
		}

		protected void PopulateBuyerAddress(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_BuyerAddress), Constants.AddressTypes.BuyerAddress);
		}

		protected void PopulateSupplierAddress(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_SupplierAddress), Constants.AddressTypes.SupplierAddress);
		}

		protected void PopulateIntermConsigneeAddress(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceBO.JZ_OA_IntermediateConsigneeAddress), Constants.AddressTypes.IntermediateConsignee);
		}

		protected RecipientRoleDetail? BondedWarehouseChangeOfOwnershipRecipientRoleDetail
		{
			get
			{
				if (!hasCheckedBondedWarehouseChangeOfOwnershipRecipientRoleDetail)
				{
					hasCheckedBondedWarehouseChangeOfOwnershipRecipientRoleDetail = true;
					bondedWarehouseChangeOfOwnershipRecipientRoleDetail = writeManager.GetRecipientRoleDetail(RecipientRoleType.BCO);
				}
				return bondedWarehouseChangeOfOwnershipRecipientRoleDetail;
			}
		}
		bool hasCheckedBondedWarehouseChangeOfOwnershipRecipientRoleDetail;
		RecipientRoleDetail? bondedWarehouseChangeOfOwnershipRecipientRoleDetail;

		protected RecipientRoleDetail? BondedWarehouseOrderRecipientRoleDetail
		{
			get
			{
				if (!hasCheckedBondedWarehouseOrderRecipientRoleDetail)
				{
					hasCheckedBondedWarehouseOrderRecipientRoleDetail = true;
					bondedWarehouseOrderRecipientRoleDetail = writeManager.GetRecipientRoleDetail(RecipientRoleType.BWR);
				}
				return bondedWarehouseOrderRecipientRoleDetail;
			}
		}
		bool hasCheckedBondedWarehouseOrderRecipientRoleDetail;
		RecipientRoleDetail? bondedWarehouseOrderRecipientRoleDetail;

		protected RecipientRoleDetail? BondedWarehouseReceiveRecipientRoleDetail
		{
			get
			{
				if (!hasCheckedBondedWarehouseReceiveRecipientRoleDetail)
				{
					hasCheckedBondedWarehouseReceiveRecipientRoleDetail = true;
					bondedWarehouseReceiveRecipientRoleDetail = writeManager.GetRecipientRoleDetail(RecipientRoleType.BWI);
				}
				return bondedWarehouseReceiveRecipientRoleDetail;
			}
		}
		bool hasCheckedBondedWarehouseReceiveRecipientRoleDetail;
		RecipientRoleDetail? bondedWarehouseReceiveRecipientRoleDetail;

		protected bool IsBondedWarehouseRecipientRole
		{
			get
			{
				if (!isBondedWarehouseRecipientRoleCached.HasValue)
				{
					isBondedWarehouseRecipientRoleCached =
						BondedWarehouseReceiveRecipientRoleDetail.HasValue ||
						BondedWarehouseOrderRecipientRoleDetail.HasValue ||
						BondedWarehouseChangeOfOwnershipRecipientRoleDetail.HasValue;
				}
				return isBondedWarehouseRecipientRoleCached.Value;
			}
		}
		bool? isBondedWarehouseRecipientRoleCached;

		bool CanUseEntryNumberPlaceHolder
		{
			get
			{
				if (!canUseEntryNumberPlaceHolderCached.HasValue)
				{
					canUseEntryNumberPlaceHolderCached =
						(BondedWarehouseReceiveRecipientRoleDetail.HasValue && BondedWarehouseReceiveRecipientRoleDetail.Value.ServiceCode.HasValue && BondedWarehouseReceiveRecipientRoleDetail.Value.ServiceCode.Value == ServiceCodeType.HLD) ||
						(BondedWarehouseOrderRecipientRoleDetail.HasValue && BondedWarehouseOrderRecipientRoleDetail.Value.ServiceCode.HasValue && BondedWarehouseOrderRecipientRoleDetail.Value.ServiceCode.Value == ServiceCodeType.HLD);
				}
				return canUseEntryNumberPlaceHolderCached.Value;
			}
		}
		bool? canUseEntryNumberPlaceHolderCached;

		void PopulateCommercialInvoiceLineData(BaseJobComInvoiceHeader invoiceBO, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			invoiceData.SetCommercialInvoiceLineCollection(() =>
			{
				DataObjectList<UniversalCustoms.CommercialInvoiceLine> commercialInvoiceLineCollection = null;
				var invoiceLineBOs = invoiceBO.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Where(x => helper.IsRelatedToEntry(x) && x.IncludedInUniversalXML).ToArray();
				if (invoiceLineBOs.Length > 0)
				{
					var canUseEntryNumberPlaceHolder = CanUseEntryNumberPlaceHolder;
					commercialInvoiceLineCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceLine>();
					var declaration = invoiceBO.JobDeclaration;
					ICustomLabelsProvider invoiceLineCustomLabelsProvider = null;
					Func<IColumnIndexer, string, IZType> getCustomizedFieldValue = null;
					if (needsOwnerProductDetails && relatedEntryInstruction != null)
					{
						invoiceLineCustomLabelsProvider = helper.GetJobComInvoiceLineCustomLabelsProvider(relatedEntryInstruction);
						ICustomsCustomLabelsConfigOrgProvider provider = relatedEntryInstruction;
						getCustomizedFieldValue = (r, s) =>
						{
							return s == provider.PartAttribute1
								|| s == provider.PartAttribute2
								|| s == provider.PartAttribute3
								|| s == provider.SerialNumber
									? (IZType)r[s]
									: GetInvoiceLineValue(r, s);
						};
					}
					else
					{
						invoiceLineCustomLabelsProvider = helper.GetJobComInvoiceLineCustomLabelsProvider(declaration);
						getCustomizedFieldValue = GetInvoiceLineValue;
					}

					var customsEntryInstructions = declaration?.CustomsEntryInstructionProvider?.CustomsEntryInstructions;
					//TODO: We need to fix this code as it's wrong to use an owner from random CusEntryInstruction, we should group the invoice line by JI_CEI and use the owner from the related CusEntryInstruction
					var owner = customsEntryInstructions == null ? null : customsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault()?.Owner;
					foreach (var invoiceLineBO in invoiceLineBOs)
					{
						var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(writeManager.WriterStrategy)
						{
							Link = helper.GetCommercialInvoiceLineLink(invoiceLineBO),
							LineNo = invoiceLineBO.JI_LineNo,
							PartNo = GetPartNo(invoiceLineBO),
							OrderNumber = invoiceLineBO.JI_OrderNumber,
							Description = GetGoodsDescription(invoiceLineBO),
							InvoiceQuantity = invoiceLineBO.JI_InvoiceQuantity,
							InvoiceQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceLineBO.JI_InvoiceUQ, invoiceLineBO.Lookups.InvoiceUQList),
							LinePrice = invoiceLineBO.JI_LinePrice,
							UnitPrice = invoiceLineBO.UnitPrice,
							Volume = invoiceLineBO.JI_Volume,
							VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(invoiceLineBO.JI_VolumeUQ, invoiceLineBO.Lookups.VolumeUQList),
							Weight = invoiceLineBO.JI_Weight,
							WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(invoiceLineBO.JI_WeightUQ, invoiceLineBO.Lookups.WeightUQList),
							NetWeight = invoiceLineBO.JI_NetWeight,
							NetWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(invoiceLineBO.JI_NetWeightUQ, invoiceLineBO.Lookups.WeightUQList),
							CustomsQuantity = invoiceLineBO.JI_CustomsQuantity,
							CustomsQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(invoiceLineBO.JI_CustomsUnitQty, GetCustomsUQCodeDescription(invoiceLineBO, invoiceLineBO.JI_CustomsUnitQtyInfo)),
							CustomsSecondQuantity = invoiceLineBO.JI_CustomsSecondQuantity,
							CustomsSecondQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(invoiceLineBO.JI_CustomsSecondUnitQty, GetCustomsUQCodeDescription(invoiceLineBO, invoiceLineBO.JI_CustomsSecondUnitQtyInfo)),
							CustomsThirdQuantity = invoiceLineBO.JI_CustomsThirdQuantity,
							CustomsThirdQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(invoiceLineBO.JI_CustomsThirdUnitQty, GetCustomsUQCodeDescription(invoiceLineBO, invoiceLineBO.JI_CustomsThirdUnitQtyInfo)),
							CustomsFourthQuantity = invoiceLineBO.JI_CustomsFourthQuantity,
							CustomsFourthQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(invoiceLineBO.JI_CustomsFourthUnitQty, GetCustomsUQCodeDescription(invoiceLineBO, invoiceLineBO.JI_CustomsFourthUnitQtyInfo)),
							CustomsFifthQuantity = invoiceLineBO.JI_CustomsFifthQuantity,
							CustomsFifthQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(invoiceLineBO.JI_CustomsFifthUnitQty, GetCustomsUQCodeDescription(invoiceLineBO, invoiceLineBO.JI_CustomsFifthUnitQtyInfo)),
							//TODO - JI_ParentLine
							HarmonisedCode = invoiceLineBO.JI_Tariff,
							FormattedTariff = invoiceLineBO.JI_FormattedTariff,
							CountryOfExport = Country.NewOrEmpty(invoiceLineBO.CountryOfExportFallback),
							CountryOfOrigin = Country.NewOrEmpty(invoiceLineBO.CountryOfOriginFallback),
							StateOfOrigin = GetStateOfOrigin(invoiceLineBO),
							PrimaryPreference = invoiceLineBO.JI_PrimaryPreference,
							SecondaryPreference = invoiceLineBO.JI_SecondaryPreference,
							ConcessionOrder = invoiceLineBO.JI_ConcessionOrder,
							//TODO - JI_ParentID
							//TODO - JI_ParentTableCode
							//TODO - JI_ExtraInfoForClassification
							Commodity = ListHelper.GetWithDescription<Commodity>(invoiceLineBO.JI_RH_NKCommodity_Code, invoiceLineBO.Lookups.Commodity_Codes),
							ContainerMode = ListHelper.GetWithDescription<ContainerMode>(invoiceLineBO.JI_ContainerMode, invoiceLineBO.Lookups.ContainerModeList),
							CustomsValue = invoiceLineBO.JI_CustomsValue,
							RelatedIndicator = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceLineBO.JI_RelatedIndicator, invoiceLineBO.Lookups.RelatedIndicatorList),
							ValuationCode = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceLineBO.JI_ValuationCode, invoiceLineBO.Lookups.ValuationCodeList),
							ValuationMarkup = invoiceLineBO.JI_ValuationMarkup,

							BrandName = invoiceLineBO.JI_BrandName,
							Model = invoiceLineBO.JI_Model,
							LocalDescription = invoiceLineBO.JI_NDescription,
							TaxType = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(invoiceLineBO.JI_ZZF_NKTaxType, invoiceLineBO.Lookups.TaxOrFeeCodeList),
							AddInfoCollection = GetInvoiceLineAddInfoCollection(invoiceLineBO),
							AddInfoGroupCollection = GetInvoiceLineAddInfoGroupCollection(invoiceLineBO),

							DataImportMatchingKey = invoiceLineBO.JI_MatchingKey,
							ClassUsageComment = invoiceLineBO.JI_ClassUsageComment,
							ClassUsageCommentStaff = Staff.New(invoiceLineBO.ClassUsageCommentReviewer),
							CustomAttributeCollection = GetInvoiceLineCustomAttributeCollection(invoiceLineBO)
						};
						invoiceLineData.SetCustomsReferenceCollection(() => CustomsReferenceCollectionCreator.CreateCollection(helper, invoiceLineBO, writeManager));
						invoiceLineData.SetCustomsSupportingInformationCollection(() => invoiceLineBO is Integration.Customs.ICusSupportingInfoTypeSupporter ? GetInvoiceLineCustomsSupportingInformationCollectionCore(invoiceLineBO) : null);

						if (invoiceLineBO.SupportsJobComInvoiceLineTax)
						{
							invoiceLineData.TaxOrFeeCollection = TaxOrFeeCollectionCreator.CreateCollection(helper, invoiceLineBO, writeManager);
						}

						if (landedCostDataWriter != null)
						{
							invoiceLineData.LandedCostDetail = landedCostDataWriter.PopulateLandedCostDetail(invoiceLineBO);
							invoiceLineData.TransportLogisticsCostCollection = landedCostDataWriter.PopulateTransportLogisticsCostCollection(invoiceLineBO);
						}

						if (IsBondedWarehouseRecipientRole)
						{
							PopulateBondedWarehouseDetails(invoiceLineBO, invoiceLineData);
						}
						else
						{
							invoiceLineData.BondedWarehouseQuantity = invoiceLineBO.ComponentInventoryCollection.Count > 0 && invoiceLineBO.JI_BondedWhsQuantity == 0 ? null : invoiceLineBO.JI_BondedWhsQuantity;
							invoiceLineData.BondedWarehouseQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceLineBO.JI_BondedWhsUnitQty, invoiceLineBO.Lookups.BondedWhsUnitQtyList);
							invoiceLineData.BondedWarehouseRemarks = invoiceLineBO.JI_BondedWarehouseRemarks;
							invoiceLineData.BondedWHSOrderNumber = invoiceLineBO.JI_BondedWHSOrderNumber;
							invoiceLineData.BondedWHSOrderLineNumber = invoiceLineBO.JI_BondedWHSOrderLineNumber;
						}

						var parentTariffLine = invoiceLineBO.ParentTariffLine;
						invoiceLineData.ParentLineNo = parentTariffLine != null ? parentTariffLine.JI_LineNo : ZShort.Zero;

						var classificationBO = invoiceLineBO.Classification;
						invoiceLineData.ClassificationCode = classificationBO != null ? classificationBO.CC_LookupCode : null;

						if (invoiceLineBO.IncludeEntryDetailsInUniversalXML)
						{
							var entryLineBO = invoiceLineBO.CusEntryLine;
							if (entryLineBO != null)
							{
								invoiceLineData.EntryLineNumber = entryLineBO.CL_LineNumber;
								var entryHeaderBO = entryLineBO.Header;
								if (entryHeaderBO != null)
								{
									var entryNumber = entryHeaderBO.EntryNumber;
									if (entryNumber.IsEmpty && canUseEntryNumberPlaceHolder)
									{
										entryNumber = Constants.EntryNumberPlaceHolder;
									}
									invoiceLineData.EntryNumber = entryNumber;

									invoiceLineData.EntryStatus = entryHeaderBO.CH_EntryStatus;
									invoiceLineData.EntryReleaseDate = entryHeaderBO.CH_EntryReleaseDate;
								}
							}
						}

						PopulateEntryReference(invoiceLineData, invoiceLineBO);
						var orderLineBO = invoiceLineBO.OrderLine;
						invoiceLineData.OrderLineLink = orderLineBO != null ? orderLineBO.JO_LineNo : ZInt.Zero;
						invoiceLineData.CommercialChargeCollection = CommercialChargeCollectionCreator.CreateCollection(helper, invoiceLineBO.PK);
						invoiceLineData.EntryInstructionLink = helper.GetAllocatedEntryInstructionLink(invoiceLineBO.JI_CEI);
						invoiceLineData.Procedure = invoiceLineBO.JI_Procedure;
						invoiceLineData.PreviousEntryNumber = invoiceLineBO.JI_PreviousEntryNumber;
						invoiceLineData.PreviousEntryLineNumber = invoiceLineBO.JI_PreviousEntryLineNumber;
						CustomLabelsCustomizedFieldDataObjectWriter.Write(getCustomizedFieldValue, invoiceLineBO, invoiceLineData, invoiceLineCustomLabelsProvider);
						PopulateCommercialInvoiceLineOrganizationAddressData(invoiceLineBO, invoiceLineData);
						PopulateHazardousMaterial(invoiceLineBO, invoiceLineData);
						PopulateAdditionalTariffDetails(invoiceLineBO, invoiceLineData);
						commercialInvoiceLineCollection.Add(invoiceLineData);
						PopulateChangeOfOwnershipData(invoiceLineData, invoiceLineBO, owner);
						PopulateCustomFields(invoiceLineData, invoiceLineBO);
						PopulateCommercialInvoiceLineFields(invoiceLineData, invoiceLineBO);
						PopulateCountrySpecificLineData(invoiceLineData, invoiceLineBO);
					}
				}
				return commercialInvoiceLineCollection ?? invoiceData.CommercialInvoiceLineCollection;
			});

			writeManager.NotifyExported(invoiceData, invoiceBO);
		}

		protected virtual State GetStateOfOrigin(BaseJobComInvoiceLine invoiceLineBO) => State.NewOrEmpty(invoiceLineBO.OriginState);

		List<UniversalCustoms.CustomAttribute> GetInvoiceLineCustomAttributeCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			(string Key, ZString Value)[] customAttributes = new[] {
				(Constants.CustomAttributeKeys.CustomAttribute1, invoiceLineBO.JI_CustomAttrib1),
				(Constants.CustomAttributeKeys.CustomAttribute2, invoiceLineBO.JI_CustomAttrib2),
				(Constants.CustomAttributeKeys.CustomAttribute3, invoiceLineBO.JI_CustomAttrib3),
				(Constants.CustomAttributeKeys.CustomAttribute4, invoiceLineBO.JI_CustomAttrib4),
				(Constants.CustomAttributeKeys.CustomAttribute5, invoiceLineBO.JI_CustomAttrib5),
				(Constants.CustomAttributeKeys.CustomAttribute6, invoiceLineBO.JI_CustomAttrib6)
			};
			return customAttributes.Where(x => !x.Value.IsEmpty).Select(attrib => UniversalCustoms.CustomAttribute.New(attrib.Key, attrib.Value)).ToList();
		}

		protected virtual void PopulateCountrySpecificLineData(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLineBO)
		{
		}

		static ZArchitecture.Core.CodeDescriptionPairList GetCustomsUQCodeDescription(BaseJobComInvoiceLine invoiceLineBO, ZPropertyInfo info)
		{
			var list = MetaData.GetListDataSource(invoiceLineBO, info.PropertyDescriptor) as ZArchitecture.Core.CodeDescriptionPairList;
			return list ?? invoiceLineBO.Lookups.CustomsUQList;
		}

		IZType GetInvoiceLineValue(IColumnIndexer row, string columnName)
		{
			var column = ((ITableSchema)JobComInvoiceLineSchema.Instance).All[columnName];
			return column != null ? row.GetValue(column) : null;
		}

		ZString GetPartNo(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = ZString.Empty;
			if (needsOwnerProductDetails)
			{
				var changeOfOwnershipLineDetails = invoiceLineBO as IChangeOfOwnershipLineDetails;
				result = changeOfOwnershipLineDetails == null ? ZString.Empty : (ZString)changeOfOwnershipLineDetails.NewOwnerProductCodeInfo.Value;
			}
			else
			{
				result = invoiceLineBO.JI_PartNo;
			}
			return result;
		}

		void PopulateAdditionalTariffDetails(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			var additionalTariffParent = invoiceLineBO as IAdditionalLineTariffDetailParent;
			if (additionalTariffParent != null)
			{
				invoiceLineData.AdditionalLineTariffDetailCollection = ProcessCollection(additionalTariffParent.CusLineTariffDetails, CreateAdditionalLineTariffDetailDataObjectWriter());
			}
		}

		protected virtual AdditionalLineTariffDetailDataObjectWriter CreateAdditionalLineTariffDetailDataObjectWriter()
		{
			return new AdditionalLineTariffDetailDataObjectWriter(writeManager);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void PopulateChangeOfOwnershipData(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLineBO, OrgHeader owner)
		{
			var changeOfOwnershipLineDetails = invoiceLineBO as IChangeOfOwnershipLineDetails;
			if (changeOfOwnershipLineDetails != null && BondedWarehouseChangeOfOwnershipRecipientRoleDetail != null)
			{
				var addInfos = invoiceLineData.AddInfoCollection;
				if (addInfos == null)
				{
					addInfos = new List<AddInfo>();
					invoiceLineData.AddInfoCollection = addInfos;
				}
				var miscServ = owner?.MiscServ;
				var hasMiscServ = miscServ != null;
				addInfos.AddOrUpdate(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode, (ZString)changeOfOwnershipLineDetails.NewOwnerProductCodeInfo.Value);
				addInfos.AddOrUpdate(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1, hasMiscServ && !miscServ.OM_IMPartAttrib1Name.IsEmpty ? (ZString)changeOfOwnershipLineDetails.NewOwnerPartAttribute1Info.Value : ZString.Empty);
				addInfos.AddOrUpdate(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2, hasMiscServ && !miscServ.OM_IMPartAttrib2Name.IsEmpty ? (ZString)changeOfOwnershipLineDetails.NewOwnerPartAttribute2Info.Value : ZString.Empty);
				addInfos.AddOrUpdate(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3, hasMiscServ && !miscServ.OM_IMPartAttrib3Name.IsEmpty ? (ZString)changeOfOwnershipLineDetails.NewOwnerPartAttribute3Info.Value : ZString.Empty);
				addInfos.AddOrUpdate(Constants.AddInfoKeys.InvoiceLine.NewOwnerSerialNumber, hasMiscServ ? (ZString)changeOfOwnershipLineDetails.NewOwnerSerialNumberInfo.Value : ZString.Empty);
			}
		}

		void PopulateCustomFields(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
			var customFieldsWriter = new CustomFieldsDataObjectWriter<BaseJobComInvoiceLine, UniversalCustoms.CommercialInvoiceLine>(writeManager, invoiceLineData);
			customFieldsWriter.GetDataObject(invoiceLine);
		}

		protected virtual ZString GetGoodsDescription(BaseJobComInvoiceLine invoiceLineBO)
		{
			return invoiceLineBO.JI_Description;
		}

		protected virtual void PopulateCommercialInvoiceLineFields(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
		}

		protected virtual void PopulateBondedWarehouseDetails(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			if (IsPopulateBondedWarehouseDetails(invoiceLineBO))
			{
				PopulateBondedWarehouseQuantityAndUnit(invoiceLineBO, invoiceLineData);
				PopulateBondedWarehouseCommonInfo(invoiceLineBO, invoiceLineData);
			}
		}

		protected virtual ZBool IsPopulateBondedWarehouseDetails(BaseJobComInvoiceLine invoiceLineBO) =>
			invoiceLineBO.SupplierPart != null &&
			invoiceLineBO.CusEntryLine?.Header is CusEntryHeader entry &&
			(entry.IsInwardBondedWarehousingEnabled || entry.IsOutwardBondedWarehousingEnabled || entry.IsIntoInwardProcessingEnabled);

		protected virtual List<UniversalCustoms.CustomsSupportingInformation> GetInvoiceLineCustomsSupportingInformationCollectionCore(BaseJobComInvoiceLine invoiceLineBO)
		{
			return CustomsSupportingInformationCollectionCreator.CreateCollection(helper, invoiceLineBO, writeManager);
		}

		protected virtual void PopulateBondedWarehouseQuantityAndUnit(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.BondedWarehouseQuantity = invoiceLineBO.ComponentInventoryCollection.Count > 0 && invoiceLineBO.JI_BondedWhsQuantity == 0 ? null : invoiceLineBO.JI_BondedWhsQuantity;
			invoiceLineData.BondedWarehouseQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceLineBO.JI_BondedWhsUnitQty, invoiceLineBO.Lookups.BondedWhsUnitQtyList);
		}

		protected void PopulateBondedWarehouseCommonInfo(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.BondedWarehouseRemarks = invoiceLineBO.JI_BondedWarehouseRemarks;
			invoiceLineData.BondedWHSOrderNumber = invoiceLineBO.JI_BondedWHSOrderNumber;
			invoiceLineData.BondedWHSOrderLineNumber = invoiceLineBO.JI_BondedWHSOrderLineNumber;
		}

		protected virtual List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			return AddInfoCollectionCreator.CreateCollection(invoiceLineBO, JobComInvoiceLineSchema.JI_AddInfo);
		}

		protected virtual List<UniversalCustoms.AddInfoGroup> GetInvoiceLineAddInfoGroupCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var addInfoGroupList = AddInfoGroupCollectionCreator.CreateCollection(helper, invoiceLineBO, writeManager);

			if (invoiceLineBO.ComponentInventoryCollection.Count > 0)
			{
				if (addInfoGroupList == null)
				{ addInfoGroupList = new List<UniversalCustoms.AddInfoGroup>(); }

				foreach (JobComInvLineComponentInventory componentInventory in invoiceLineBO.ComponentInventoryCollection)
				{
					addInfoGroupList.Add(new UniversalCustoms.AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.WarehouseAllocationInfo, Description = Constants.WarehouseAllocationInfoDescription },
						AddInfoCollection = new List<AddInfo>()
						{
							new AddInfo() { Key = Constants.AddInfoKeys.AllocationInfo.AllocationKey, Value = componentInventory.JIV_AllocationKey },
							new AddInfo() { Key = Constants.AddInfoKeys.AllocationInfo.Quantity, Value = componentInventory.JIV_QuantityToDraw.ToString() }
						}
					});
				}
			}

			return addInfoGroupList;
		}

		void PopulateEntryReference(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLineBO)
		{
			if (helper.SupportAdditionalInvoiceLineEntryLineLink)
			{
				var entryReferenceCollection = new List<UniversalCustoms.EntryReference>();
				var additionalInvoiceLineEntryLineLinks = helper.Load<AdditionalInvoiceLineEntryLineLink>(new ZQuery(CusUnderbondDecSchema.BU_JI, invoiceLineBO.PK));
				if (additionalInvoiceLineEntryLineLinks.Length > 0)
				{
					foreach (var additionalEntryLineBO in helper.Load<CusEntryLine>(new ZQuery(CusEntryLineSchema.PK, additionalInvoiceLineEntryLineLinks.Select(x => x.BU_CL).Distinct())))
					{
						entryReferenceCollection.Add(CreateEntryReference(additionalEntryLineBO));
					}
				}
				invoiceLineData.EntryReferenceCollection = entryReferenceCollection.Count > 0 ? entryReferenceCollection : null;
			}
		}

		UniversalCustoms.EntryReference CreateEntryReference(CusEntryLine entryLineBO)
		{
			var result = new UniversalCustoms.EntryReference()
			{
				LineNumber = entryLineBO.CL_LineNumber
			};
			var entryHeaderBO = entryLineBO.Header;
			if (entryHeaderBO != null)
			{
				result.Type = ListHelper.GetWithDescription<EntryType>(entryHeaderBO.CH_MessageType, entryHeaderBO.Lookups.CH_MessageTypeList);
				result.Reference = entryHeaderBO.CH_BGMReference;
			}
			return result;
		}

		protected virtual void PopulateCommercialInvoiceLineOrganizationAddressData(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			PopulateManufacturer(invoiceLineBO, invoiceLineData);
			PopulateConsignee(invoiceLineBO, invoiceLineData);
			PopulateShipToParty(invoiceLineBO, invoiceLineData);
			PopulateSeller(invoiceLineBO, invoiceLineData);
			PopulateSoldToPartyAddress(invoiceLineBO, invoiceLineData);
			PopulateExporterAddress(invoiceLineBO, invoiceLineData);
			PopulateConsigneeAddress(invoiceLineBO, invoiceLineData);
			PopulateDocAddresses(invoiceLineBO as IDocAddresses, invoiceLineData);
		}

		protected void PopulateManufacturer(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLineBO.JI_OA_ManufacturerAddress), DocAddressType.Manufacturer);
		}

		protected void PopulateConsignee(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLineBO.JI_OA_ConsigneeAddress), DocAddressType.ConsigneeAddress);
		}

		protected void PopulateShipToParty(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLineBO.JI_OA_ShipToPartyAddress), DocAddressType.ShipToParty);
		}

		protected void PopulateSeller(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLineBO.JI_OA_Seller), Constants.AddressTypes.Seller);
		}

		protected void PopulateSoldToPartyAddress(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLineBO.JI_OA_SoldToPartyAddress), Constants.AddressTypes.SoldToParty);
		}

		protected void PopulateExporterAddress(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLineBO.JI_OA_ExporterAddress), DocAddressType.Exporter);
		}

		protected void PopulateDocAddresses(IDocAddresses docAddresses, IOrganizationAddressCollectionParent addressCollectionParent)
		{
			if (docAddresses != null && addressCollectionParent != null)
			{
				addressCollectionParent.AddOrgAddresses(writeManager, docAddresses);
			}
		}

		protected void PopulateConsigneeAddress(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.AddOrgAddress(writeManager, helper.Load<OrgAddress>(invoiceLineBO.JI_OA_ConsigneeAddress), Constants.AddressTypes.UltimateConsignee);
		}

		void PopulateHazardousMaterial(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			var hazardousMaterialData = new UniversalCustoms.HazardousMaterial()
			{
				Code = invoiceLineBO.JI_HazMatCode,
				CodeType = ListHelper.GetWithDescription<CodeDescriptionPair5Char>(invoiceLineBO.JI_HazMatCodeQualifier, invoiceLineBO.Lookups.HazardousMaterialCodeQualifierList)
			};

			var undgs = helper.Load<UNDGDataItem>(new ZQuery(UNDGDataItemSchema.DI_ParentID, invoiceLineBO.PK));
			hazardousMaterialData.UNDGCollection = ProcessCollection(undgs, new UNDGDataObjectWriter(writeManager));
			invoiceLineData.HazardousMaterial = hazardousMaterialData;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(BaseJobComInvoiceHeader sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}
	}
}
