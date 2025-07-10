using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using PartyIdTypes = Enterprise.Customs.US.eManifest.Business.PartyIdTypes;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVConsignmentDataObjectWriter : TopLevelDataObjectWriter<HVLVConsignment, UniversalShipment>, IHVLVConsignmentDataObjectWriter
	{
		public HVLVConsignmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
			codeDataObjectCache = new CodeDataObjectCache();
			DataExportStrategy = DefaultHVLVConsignmentDataExportStrategy.Instance;
		}

		public HVLVConsignmentDataObjectWriter(IDataWritingManager manager, IHVLVConsignmentDataExportStrategy dataExportStrategy)
			: this(manager)
		{
			DataExportStrategy = dataExportStrategy;
		}

		IEnumerable<HVLVConsignment> consignmentsToMerge;
		protected CodeDataObjectCache codeDataObjectCache;
		public readonly IHVLVConsignmentDataExportStrategy DataExportStrategy;

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.HVLVConsignment;

		protected override void PopulateDataObject(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			using (SetWriterStrategyTemporarily(dataObject))
			{
				PopulateConsignmentDetails(consignmentBO, dataObject);
				PopulateNotes(consignmentBO, dataObject);
				PopulateInstructions(consignmentBO, dataObject);
				PopulateOrganizations(consignmentBO, dataObject);
				PopulateCustomsReferences(consignmentBO, dataObject);
				PopulateAdditionalReferences(consignmentBO, dataObject);
				PopulateCommercialInvoiceInformation(consignmentBO, dataObject);
				PopulatePackingLines(consignmentBO, dataObject);
				PopulateLastMileCarrierDetails(consignmentBO, dataObject);
				PopulateCustomsClearanceStatus(consignmentBO, dataObject);
				PopulateAddInfoCollection(consignmentBO, dataObject);
				PopulateGoodsDescriptionWithFallback(consignmentBO, dataObject);
				PopulateGoodsValueWithFallback(consignmentBO, dataObject);
			}
		}

		protected override void InsertParents(HVLVConsignment consignmentBO, ref UniversalShipment dataObject)
		{
			if (DataExportStrategy is DefaultHVLVConsignmentDataExportStrategy || DataExportStrategy is HVLVConsignmentDataExportStrategyWithoutNotes)
			{
				InsertParentsForDefaultDataExportStrategy(consignmentBO, ref dataObject);
			}
			else if (DataExportStrategy is HVLVConsignmentToDeclarationDataExportStrategy)
			{
				InsertParentsForDeclarationDataExportStrategy(consignmentBO, ref dataObject);
			}
		}

		public void SetConsignmentsToMerge(IEnumerable<IHVLVConsignment> consignmentsToMerge)
		{
			this.consignmentsToMerge = consignmentsToMerge?.OfType<HVLVConsignment>().ToList();
		}

		#region PopulateDataObject Helpers

		void PopulateConsignmentDetails(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			var goodsValueCurrencyKey = codeDataObjectCache.BuildKey(consignmentBO.Lookups.GoodsValueCurrencies.GetType().FullName, consignmentBO.HVC_RX_NKGoodsValueCurrency);
			var incoTermKey = codeDataObjectCache.BuildKey(consignmentBO.Lookups.INCOTermsList.GetType().FullName, consignmentBO.HVC_INCO);

			var goodsValueCurrency = codeDataObjectCache.GetValue(goodsValueCurrencyKey, () => ListHelper.GetWithDescription<Currency>(consignmentBO.HVC_RX_NKGoodsValueCurrency, consignmentBO.Lookups.GoodsValueCurrencies));
			var incoTerm = codeDataObjectCache.GetValue(incoTermKey, () => ListHelper.GetWithDescription<UniversalIncoTerm>(consignmentBO.HVC_INCO, consignmentBO.Lookups.INCOTermsList));

			var itemBizos = consignmentBO.Items.OfType<HVLVItem>();
			var itemLines = itemBizos.SelectMany(item => item.Lines);

			var combinedItemCountFromConsignmentsToMerge = consignmentsToMerge?.Sum(x => x.HVC_ItemCount) ?? ZShort.Zero;

			if (consignmentsToMerge != null && consignmentsToMerge.Any())
			{
				var convertedWeightUnit = (ZString)Weight.Kilograms;
				var convertedTotalWeight = (ZDecimal)TotalCalculation.GetTotalWeight(
					consignmentsToMerge.Prepend(consignmentBO),
					x => x.HVC_ActualWeight.IsDefault ? x.HVC_ManifestedWeight : x.HVC_ActualWeight,
					x => x.HVC_WeightUQ,
					convertedWeightUnit);
				new WeightConversionStrategy().ReScale(ref convertedTotalWeight, ref convertedWeightUnit, HVLVConsignmentSchema.HVC_ActualWeight.Precision, HVLVConsignmentSchema.HVC_ActualWeight.Scale);
				dataObject.TotalWeight = convertedTotalWeight;
				dataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(convertedWeightUnit, consignmentBO.Lookups.HVC_WeightUQ_List);

				var convertedVolumeUnit = (ZString)Volume.CubicMetres;
				var convertedTotalVolume = (ZDecimal)TotalCalculation.GetTotalVolume(
					consignmentsToMerge.Prepend(consignmentBO),
					x => x.HVC_ActualVolume.IsDefault ? x.HVC_ManifestedVolume : x.HVC_ActualVolume,
					x => x.HVC_VolumeUQ,
					convertedVolumeUnit);
				new VolumeConversionStrategy().ReScale(ref convertedTotalVolume, ref convertedVolumeUnit, HVLVConsignmentSchema.HVC_ActualVolume.Precision, HVLVConsignmentSchema.HVC_ActualVolume.Scale);
				dataObject.TotalVolume = convertedTotalVolume;
				dataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(convertedVolumeUnit, consignmentBO.Lookups.HVC_VolumeUQ_List);
			}
			else
			{
				var totalWeightUnitKey = codeDataObjectCache.BuildKey(nameof(UnitOfWeight), consignmentBO.HVC_WeightUQ);
				var totalVolumeUnitKey = codeDataObjectCache.BuildKey(nameof(UnitOfVolume), consignmentBO.HVC_VolumeUQ);

				dataObject.TotalWeight = consignmentBO.HVC_ActualWeight.IsDefault ? consignmentBO.HVC_ManifestedWeight : consignmentBO.HVC_ActualWeight;
				dataObject.TotalWeightUnit = codeDataObjectCache.GetValue(totalWeightUnitKey, () => ListHelper.GetWithDescription<UnitOfWeight>(consignmentBO.HVC_WeightUQ, consignmentBO.Lookups.HVC_WeightUQ_List));
				dataObject.TotalVolume = consignmentBO.HVC_ActualVolume.IsDefault ? consignmentBO.HVC_ManifestedVolume : consignmentBO.HVC_ActualVolume;
				dataObject.TotalVolumeUnit = codeDataObjectCache.GetValue(totalVolumeUnitKey, () => ListHelper.GetWithDescription<UnitOfVolume>(consignmentBO.HVC_VolumeUQ, consignmentBO.Lookups.HVC_VolumeUQ_List));
			}

			dataObject.WayBillNumber = consignmentBO.HVC_WaybillNumber;
			dataObject.OwnerRef = consignmentBO.HVC_ShipperReference;
			dataObject.TotalNoOfPacks = consignmentBO.HVC_ItemCount;
			dataObject.TotalNoOfPieces = consignmentsToMerge != null && consignmentsToMerge.Any() ? consignmentsToMerge.Count() + 1 : consignmentBO.HVC_ItemCount;
			dataObject.GoodsValue = consignmentBO.HVC_GoodsValue;
			dataObject.InsuranceValue = consignmentBO.HVC_InsuranceValue;
			dataObject.TransportValue = consignmentBO.HVC_TransportValue;
			dataObject.GoodsValueCurrency = goodsValueCurrency;
			dataObject.TotalNoOfPacksPackageType = new PackageType() { Code = itemBizos.FirstOrDefault()?.HVI_F3_NKPackType };
			dataObject.OuterPacks = consignmentBO.HVC_ItemCount + combinedItemCountFromConsignmentsToMerge;
			dataObject.OuterPacksPackageType = new PackageType() { Code = itemBizos.FirstOrDefault()?.HVI_F3_NKPackType };
			dataObject.ManifestedWeight = consignmentBO.HVC_ManifestedWeight;
			dataObject.ManifestedVolume = consignmentBO.HVC_ManifestedVolume;
			dataObject.GoodsDescription = consignmentBO.HVC_GoodsDescription;
			dataObject.IsHazardous = consignmentBO.HVC_IsHazardous;
			dataObject.IsSignatureRequired = consignmentBO.HVC_IsSignatureRequired;
			dataObject.IsAuthorizedToLeave = consignmentBO.HVC_AuthorityToLeave;
			dataObject.IsTracked = consignmentBO.HVC_IsTracked;
			dataObject.CarrierAccount = new CarrierAccount() { AccountNumber = consignmentBO.HVC_CarrierAccountNumber };
			dataObject.CarrierServiceLevel = new ServiceLevel() { Code = consignmentBO.HVC_PL_NKLastMileCarrierServiceLevel };
			dataObject.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.HighVolumeLowValue };
			dataObject.SetAddInfoCollection(() => new List<AddInfo>() { GetIsTaxPrepaidAddInfo(consignmentBO) });
			dataObject.ShipmentIncoTerm = incoTerm;
			dataObject.ServiceLevel = new ServiceLevel { Code = consignmentBO.HVC_RS_NKServiceLevel };
			dataObject.WarehouseReleaseStatus = new CodeDescriptionPair { Code = consignmentBO.HVC_ImportReleaseStatus == HVLVReleaseStatus.None ?
				consignmentBO.HVC_ExportReleaseStatus : consignmentBO.HVC_ImportReleaseStatus };

			if (!consignmentBO.HVC_VendorIdentifier.IsEmpty)
			{
				dataObject.VendorIdentifier = consignmentBO.HVC_VendorIdentifier;
			}

			if (writeManager.FilteredDataContextType.Equals(DataContextType.USCustomsLowValueEntriesClearance))
			{
				var entryNumber = consignmentBO.CustomsReferenceNumbers.GetFirstReferenceNumberByTypeAndCountry(PartyIdTypes.Codes.EmployerIdentificationNumber, CountryCodes.UnitedStates);
				if (entryNumber != null)
				{
					dataObject.ConsigneeIdentifier = entryNumber.CE_EntryNum;
				}
			}

			if (IsRecipientRoleTypeASY)
			{
				dataObject.WayBillType = new WayBillType() { Code = ShipmentTypes.StandardHouse, Description = ShipmentTypeDescriptions.StandardHouse };
			}
			else
			{
				dataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
			}
		}

		void PopulateNotes(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			var notes = consignmentBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			var noteCollection = ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);

			if (dataObject.NoteCollection != null)
			{
				dataObject.NoteCollection.AddRange(noteCollection);
			}
			else
			{
				dataObject.SetNoteCollection(() => noteCollection);
			}
		}

		void PopulateInstructions(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			if (!consignmentBO.HVC_ConsigneeInstructions.IsEmpty)
			{
				var consigneeInstructionNote = new Instruction()
				{
					ServiceInstruction = consignmentBO.HVC_ConsigneeInstructions
				};

				dataObject.SetInstructionCollection(() => new DataObjectList<Instruction>(new[] { consigneeInstructionNote }) { Content = CollectionContent.Partial });
			}
		}

		void PopulateOrganizations(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			dataObject.AddOrgAddress(writeManager, consignmentBO.DestinationDepot, DocAddressType.CustomsDepotAddress);
			dataObject.AddOrgAddress(writeManager, consignmentBO.DestinationDepot, DocAddressType.ArrivalCFSAddress);

			dataObject.AddOrgAddress(writeManager, consignmentBO.LastMileCarrier?.MainAddress, AddressTypes.DeliveryLocalCartage);
			dataObject.AddOrgAddress(writeManager, consignmentBO.LastMileCarrier?.MainAddress, AddressTypes.PickupLocalCartage);

			dataObject.AddOrgAddress(writeManager, consignmentBO.LastMileCarrierBookingAgent?.MainAddress, DocAddressType.CarrierBookingAgent);

			if (dataObject.OrganizationAddressCollection != null || dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()))
			{
				if (consignmentBO.HVC_OA_ConsigneeAddress.IsDefault)
				{
					var consignee = GetConsigneeAddress(consignmentBO);
					dataObject.OrganizationAddressCollection.Add(consignee);
				}
				else
				{
					var consigneeAddress = dataObject.AddOrgAddress(writeManager, consignmentBO.ConsigneeAddress, DocAddressType.ConsigneeDocumentaryAddress);
					consigneeAddress.Contact = consignmentBO.HVC_ConsigneeContact;
					consigneeAddress.Email = consignmentBO.HVC_ConsigneeEmail;
					consigneeAddress.Phone = consignmentBO.HVC_ConsigneePhone;
					consigneeAddress.Mobile = consignmentBO.HVC_ConsigneeMobile;
					consigneeAddress.Fax = consignmentBO.HVC_ConsigneeFax;
				}

				if (consignmentBO.HVC_OA_ShipperAddress.IsDefault)
				{
					var shipper = GetShipperAddress(consignmentBO);
					dataObject.OrganizationAddressCollection.Add(shipper);
				}
				else
				{
					var consignorAddress = dataObject.AddOrgAddress(writeManager, consignmentBO.ShipperAddress, DocAddressType.ConsignorDocumentaryAddress);
					consignorAddress.Contact = consignmentBO.HVC_ShipperContact;
					consignorAddress.Email = consignmentBO.HVC_ShipperEmail;
					consignorAddress.Phone = consignmentBO.HVC_ShipperPhone;
					consignorAddress.Mobile = consignmentBO.HVC_ShipperMobile;
					consignorAddress.Fax = consignmentBO.HVC_ShipperFax;
				}

				if (consignmentBO.HVC_OA_ReturnLocation.IsDefault)
				{
					var returnAddress = GetReturnAddress(consignmentBO);
					dataObject.OrganizationAddressCollection?.Add(returnAddress);
				}
				else
				{
					var returnLocation = dataObject.AddOrgAddress(writeManager, consignmentBO.ReturnLocation, DocAddressType.ReturnAddress);
					returnLocation.Contact = consignmentBO.HVC_ReturnContact;
					returnLocation.Email = consignmentBO.HVC_ReturnEmail;
					returnLocation.Phone = consignmentBO.HVC_ReturnPhone;
					returnLocation.Mobile = consignmentBO.HVC_ReturnMobile;
					returnLocation.Fax = consignmentBO.HVC_ReturnFax;
				}

				if (consignmentBO.ManifestedOnShipment != null)
				{
					dataObject.AddOrgAddress(writeManager, consignmentBO.ManifestedOnShipment.NotifyParty, DocAddressType.NotifyParty);
				}
			}
		}

		void PopulateCustomsReferences(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			if (consignmentBO.HVC_JE_ImportDeclaration.IsEmpty && consignmentBO.HVC_JE_ExportDeclaration.IsEmpty)
			{
				return;
			}

			dataObject.SetCustomsReferenceCollection(() => PopulateCustomsReferenceCollection(consignmentBO));
		}

		static List<CustomsReference> PopulateCustomsReferenceCollection(HVLVConsignment consignmentBO)
		{
			var customsReferenceCollection = new List<CustomsReference>();

			if (consignmentBO.ImportDeclaration is BaseJobDeclaration importDeclaration)
			{
				customsReferenceCollection.Add(new CustomsReference
				{
					Type = new CodeDescriptionPair { Code = nameof(DataContext.Declaration) },
					SubType = new CodeDescriptionPair35Char { Code = FreightShipmentDirection.Code.Import },
					Reference = importDeclaration.JE_DeclarationReference,
				});
			}

			if (consignmentBO.ExportDeclaration is BaseJobDeclaration exportDeclaration)
			{
				customsReferenceCollection.Add(new CustomsReference
				{
					Type = new CodeDescriptionPair { Code = nameof(DataContext.Declaration) },
					SubType = new CodeDescriptionPair35Char { Code = FreightShipmentDirection.Code.Export },
					Reference = exportDeclaration.JE_DeclarationReference,
				});
			}

			return customsReferenceCollection;
		}

		void PopulateAdditionalReferences(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			var additionalReferences = consignmentBO.CustomsReferenceNumbers;
			var additionalReferenceCollection = ProcessCollection(additionalReferences, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete);

			if (additionalReferenceCollection != null && additionalReferenceCollection.Count > 0)
			{
				dataObject.SetAdditionalReferenceCollection(() => additionalReferenceCollection);
			}
		}

		void PopulateCommercialInvoiceInformation(HVLVConsignment consignment, UniversalShipment dataObject)
		{
			PopulateCommercialInvoice(consignment, dataObject);
			consignmentsToMerge?.ForEach(x => PopulateCommercialInvoice(x, dataObject));
		}

		public void WriteCommercialInvoiceAndLines(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			var invoiceHeader = PopulateCommercialInvoice(consignmentBO, dataObject);
			var invoiceLines = new DataObjectList<CommercialInvoiceLine>();
			foreach (HVLVItemLine line in consignmentBO.ActiveItems.SelectMany(item => item.Lines))
			{
				invoiceLines.Add(new HVLVItemDataObjectWriter(writeManager, dataObject).WriteInvoiceLine(line, consignmentBO.IsExport));
			}

			invoiceHeader.SetCommercialInvoiceLineCollection(() => invoiceLines);

			writeManager.NotifyExported(dataObject, consignmentBO);
		}

		CommercialInvoiceHeader PopulateCommercialInvoice(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			var result = new CommercialInvoiceHeader(writeManager.WriterStrategy)
			{
				InvoiceNumber = consignmentBO.HVC_WaybillNumber,
				AddInfoCollection = new List<AddInfo>(),
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));
			result.AddInfoCollection.Add(GetIsTaxPrepaidAddInfo(consignmentBO));
			result.AddInfoCollection.Add(AddInfo.New(Constants.AddInfoKeys.SupplierGSTNumber, consignmentBO.HVC_VendorIdentifier));

			var shipper = default(OrganizationAddress);
			if (consignmentBO.HVC_OA_ShipperAddress.IsDefault)
			{
				shipper = GetShipperAddress(consignmentBO);
			}
			else
			{
				shipper = OrganizationAddressHelper.GetAddressDataObject(consignmentBO.ShipperAddress, writeManager, nameof(DocAddressType.ConsignorDocumentaryAddress));
			}

			result.Supplier = shipper;

			var itemLines = consignmentBO.Items.OfType<HVLVItem>().SelectMany(item => item.Lines).OfType<HVLVItemLine>();
			if (itemLines.Any())
			{
				var firstLineWeightUnit = itemLines.First().HVS_WeightUnit.IsDefault ? consignmentBO.HVC_WeightUQ : itemLines.First().HVS_WeightUnit;
				result.InvoiceAmount = consignmentBO.HVC_GoodsValue.IsDefault ? (ZDecimal)itemLines.Sum(line => line.HVS_CustomsValue) : consignmentBO.HVC_GoodsValue;
				result.InvoiceCurrency = dataObject.GoodsValueCurrency;
				result.Weight = itemLines.Sum(line => Weight.Convert(line.HVS_GrossWeight, line.HVS_WeightUnit, firstLineWeightUnit));
				result.WeightUnit = new UnitOfWeight() { Code = firstLineWeightUnit };
				result.NetWeight = itemLines.Sum(line => Weight.Convert(line.HVS_NetWeight, line.HVS_WeightUnit, firstLineWeightUnit));
				result.NetWeightUQ = new UnitOfWeight() { Code = firstLineWeightUnit };
			}

			var commercialInfo = dataObject.CommercialInfo ?? (dataObject.CommercialInfo = new CommercialInfo());
			var commercialInvoiceCollection = commercialInfo.CommercialInvoiceCollection ?? (commercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>());
			commercialInvoiceCollection.Add(result);

			return result;
		}

		void PopulatePackingLines(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			var itemsToMerge = consignmentsToMerge?.SelectMany(x => x.ActiveItems) ?? Enumerable.Empty<HVLVItem>();
			var itemBizos = consignmentBO.ActiveItems.Concat(itemsToMerge);

			if (!itemBizos.Any())
			{
				throw new DataObjectValidationException(Res.GetString("2658a3de-6df7-4a25-af63-0800bfd230a5",
					"Item data is missing for {0}", consignmentBO.HumanReadableName));
			}

			var packingLines = ProcessCollection(itemBizos, new HVLVItemDataObjectWriter(writeManager, dataObject), CollectionContent.Complete);

			if (dataObject.PackingLineCollection != null)
			{
				dataObject.PackingLineCollection.AddRange(packingLines);
			}
			else
			{
				dataObject.SetPackingLineCollection(() => packingLines);
			}
		}

		void PopulateLastMileCarrierDetails(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			dataObject.CarrierAccount.DepotID = consignmentBO.LastMileCarrierDepotID;
			dataObject.CarrierServiceLevel.CarrierServiceCode = consignmentBO.LastMileCarrierServiceCode;
			dataObject.CarrierServiceLevel.Description = consignmentBO.LastMileCarrierServiceLevelDescription;
		}

		void PopulateCustomsClearanceStatus(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			dataObject.ConsolidatedCargoStatus = new CodeDescriptionPair
			{
				Code = string.IsNullOrEmpty(consignmentBO.HVC_ImportCustomsClearanceStatus) ? consignmentBO.HVC_ExportCustomsClearanceStatus : consignmentBO.HVC_ImportCustomsClearanceStatus,
				Description = string.IsNullOrEmpty(consignmentBO.HVC_ImportCustomsClearanceStatus) ? consignmentBO.ExportCustomsClearanceStatusDescription : consignmentBO.ImportCustomsClearanceStatusDescription
			};
		}

		void PopulateAddInfoCollection(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			var shipmentBO = consignmentBO.ManifestedOnShipment;
			if (shipmentBO != null && IsUSFreight(shipmentBO))
			{
				var scacCode = GetSCACCode(shipmentBO);
				if (scacCode.HasValue)
				{
					var addInfoCollection = dataObject.AddInfoCollection ?? new List<AddInfo>();
					addInfoCollection.Add(AddInfo.New(Constants.AddInfoKeys.WayBillIssuerSCAC, scacCode.Value));
					dataObject.SetAddInfoCollection(() => { return addInfoCollection; });
				}
			}
		}

		void PopulateGoodsDescriptionWithFallback(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			if (dataObject.GoodsDescription.GetValueOrDefault().IsDefault)
			{
				var items = consignmentBO.Items.OfType<HVLVItem>();
				var firstDescription = items.Select(item => item.HVI_GoodsDescription)
					.FirstOrDefault(description => !description.IsDefault);

				if (firstDescription.IsDefault)
				{
					firstDescription = items.SelectMany(item => item.Lines).OfType<HVLVItemLine>()
						.Select(line => line.HVS_GoodsDescription)
						.FirstOrDefault(description => !description.IsDefault);
				}

				dataObject.GoodsDescription = firstDescription;
			}
		}

		void PopulateGoodsValueWithFallback(HVLVConsignment consignmentBO, UniversalShipment dataObject)
		{
			if (dataObject.GoodsValue.GetValueOrDefault().IsDefault)
			{
				var sumItemLines = consignmentBO.Items.OfType<HVLVItem>().SelectMany(item => item.Lines).OfType<HVLVItemLine>().
					Select(line => line.HVS_CustomsValue).
					Sum(value => value);
				dataObject.GoodsValue = sumItemLines;
			}
		}

		AddInfo GetIsTaxPrepaidAddInfo(HVLVConsignment consignmentBO)
		{
			var isTaxPrepaid = ZString.Empty;
			if (consignmentBO.HVC_IsTaxPrePaid)
			{
				isTaxPrepaid = YesNoList.Codes.Yes;
			}
			else
			{
				if (!consignmentBO.HVC_VendorIdentifier.IsEmpty)
				{
					isTaxPrepaid = YesNoList.Codes.No;
				}
			}

			return AddInfo.New(Constants.AddInfoKeys.IsGSTPrePaid, isTaxPrepaid);
		}

		OrganizationAddress GetConsigneeAddress(HVLVConsignment consignmentBO)
		{
			var consigneeCountryKey = codeDataObjectCache.BuildKey(consignmentBO.Lookups.ConsigneeCountryCodes.GetType().FullName, consignmentBO.HVC_RN_NKConsigneeCountryCode);
			var consigneeCountry = codeDataObjectCache.GetValue(consigneeCountryKey, () => { return ListHelper.GetWithName<Country>(consignmentBO.HVC_RN_NKConsigneeCountryCode, consignmentBO.Lookups.ConsigneeCountryCodes); });

			return new OrganizationAddress(writerStrategy)
			{
				AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
				CompanyName = consignmentBO.HVC_ConsigneeName,
				Address1 = consignmentBO.HVC_ConsigneeAddress1,
				Address2 = consignmentBO.HVC_ConsigneeAddress2,
				City = consignmentBO.HVC_ConsigneeCity,
				State = consignmentBO.HVC_ConsigneeState,
				Postcode = consignmentBO.HVC_ConsigneePostcode,
				Country = consigneeCountry,
				Contact = consignmentBO.HVC_ConsigneeContact,
				Email = consignmentBO.HVC_ConsigneeEmail,
				Phone = consignmentBO.HVC_ConsigneePhone,
				Mobile = consignmentBO.HVC_ConsigneeMobile,
				Fax = consignmentBO.HVC_ConsigneeFax,
				AddressOverride = true
			};
		}

		OrganizationAddress GetShipperAddress(HVLVConsignment consignmentBO)
		{
			var consignorCountryKey = codeDataObjectCache.BuildKey(consignmentBO.Lookups.ShipperCountryCodes.GetType().FullName, consignmentBO.HVC_RN_NKShipperCountryCode);
			var consignorCountry = codeDataObjectCache.GetValue(consignorCountryKey, () => { return ListHelper.GetWithName<Country>(consignmentBO.HVC_RN_NKShipperCountryCode, consignmentBO.Lookups.ShipperCountryCodes); });

			return new OrganizationAddress(writerStrategy)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				CompanyName = consignmentBO.HVC_ShipperName,
				Address1 = consignmentBO.HVC_ShipperAddress1,
				Address2 = consignmentBO.HVC_ShipperAddress2,
				City = consignmentBO.HVC_ShipperCity,
				State = consignmentBO.HVC_ShipperState,
				Postcode = consignmentBO.HVC_ShipperPostcode,
				Country = consignorCountry,
				Contact = consignmentBO.HVC_ShipperContact,
				Email = consignmentBO.HVC_ShipperEmail,
				Phone = consignmentBO.HVC_ShipperPhone,
				Mobile = consignmentBO.HVC_ShipperMobile,
				Fax = consignmentBO.HVC_ShipperFax,
				AddressOverride = true
			};
		}

		OrganizationAddress GetReturnAddress(HVLVConsignment consignmentBO)
		{
			var returnCountryKey = codeDataObjectCache.BuildKey(consignmentBO.Lookups.ConsigneeCountryCodes.GetType().FullName, consignmentBO.HVC_RN_NKReturnCountryCode);
			var returnCountry = codeDataObjectCache.GetValue(returnCountryKey, () => { return ListHelper.GetWithName<Country>(consignmentBO.HVC_RN_NKReturnCountryCode, consignmentBO.Lookups.ReturnCountryCodes); });

			return new OrganizationAddress(writerStrategy)
			{
				AddressType = nameof(DocAddressType.ReturnAddress),
				CompanyName = consignmentBO.HVC_ReturnName,
				Address1 = consignmentBO.HVC_ReturnAddress1,
				Address2 = consignmentBO.HVC_ReturnAddress2,
				Country = returnCountry,
				City = consignmentBO.HVC_ReturnCity,
				State = consignmentBO.HVC_ReturnState,
				Postcode = consignmentBO.HVC_ReturnPostcode,
				Contact = consignmentBO.HVC_ReturnContact,
				Email = consignmentBO.HVC_ReturnEmail,
				Phone = consignmentBO.HVC_ReturnPhone,
				Mobile = consignmentBO.HVC_ReturnMobile,
				Fax = consignmentBO.HVC_ReturnFax,
				AddressOverride = true
			};
		}

		bool IsUSFreight(ForwardingShipment shipmentBO)
		{
			return shipmentBO.Destination != null && shipmentBO.Destination.Country.Code == CountryCodes.UnitedStates;
		}

		ZString? GetSCACCode(ForwardingShipment shipmentBO)
		{
			ZString? scacCode = null;
			if (shipmentBO.TransportMode == TransportModes.Sea || shipmentBO.TransportMode == TransportModes.Rail)
			{
				var consolBO = shipmentBO.ArrivalConsol;
				scacCode = consolBO?.SendingForwarder?.SCACCode;
				if (!scacCode.HasValue || scacCode.Value.IsEmpty)
				{
					scacCode = GlbCompany.CurrentCompany.OrgProxy?.SCACCode;
				}
			}
			return scacCode;
		}

		bool IsRecipientRoleTypeASY
		{
			get
			{
				var recipientRole = writeManager.Action?.RecipientRoleDetails;
				return recipientRole != null && recipientRole.Any(x => x.Type == RecipientRoleType.ASY);
			}
		}

		#endregion

		#region Insert Parents Helpers

		void InsertParentsForDefaultDataExportStrategy(HVLVConsignment consignmentBO, ref UniversalShipment dataObject)
		{
			if (writeManager.Action.ParentBO is HVLVConsignment)
			{
				var shipmentBO = consignmentBO.ManifestedOnShipment;
				if (shipmentBO != null)
				{
					AddShipmentDataContext(shipmentBO, dataObject);
					PopulateEstimateTime(shipmentBO, dataObject);

					var consolBO = shipmentBO.ArrivalConsol;
					if (consolBO != null)
					{
						dataObject = InsertParentWithWaybillAndTransportDetailsFromConsol(writeManager, consolBO, dataObject);
					}
				}
			}
		}

		void InsertParentsForDeclarationDataExportStrategy(HVLVConsignment consignmentBO, ref UniversalShipment dataObject)
		{
			dataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var shipmentBO = consignmentBO.ManifestedOnShipment;
			if (shipmentBO != null)
			{
				AddShipmentDataContext(shipmentBO, dataObject);

				var consolBO = shipmentBO.ArrivalConsol;
				if (consolBO != null)
				{
					PopulateTransportDetails(consolBO, dataObject);
					var consolDataObject = InsertParentConsol(writeManager, consolBO, dataObject, shipmentBO);

					if (consolBO.IsAir)
					{
						FormatMasterBillNumberForAirConsol(consolDataObject);
					}

					MoveContainerCollectionToShipmentLevel(consolDataObject, dataObject);
					PopulateAdditionalBillsWithParentWaybillNumberFromConsolLevel(shipmentBO, consolDataObject, dataObject);

					dataObject = consolDataObject;
				}
			}
		}

		void AddShipmentDataContext(ForwardingShipment shipmentBO, UniversalShipment dataObject)
		{
			dataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentBO.JS_UniqueConsignRef);
		}

		void FormatMasterBillNumberForAirConsol(UniversalShipment consolDataObject)
		{
			if (consolDataObject.WayBillNumber.HasValue)
			{
				consolDataObject.WayBillNumber = consolDataObject.WayBillNumber.Value.Replace("-", "").Replace(" ", "");
			}
		}

		void PopulateEstimateTime(ForwardingShipment shipmentBO, UniversalShipment dataObject)
		{
			dataObject.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				dates.Add(Date.New(DateType.Departure, ZBool.True, shipmentBO.JS_E_DEP));
				dates.Add(Date.New(DateType.Arrival, ZBool.True, shipmentBO.JS_E_ARV));
				return dates;
			});
		}

		UniversalShipment InsertParentWithWaybillAndTransportDetailsFromConsol(IDataWritingManager writeManager, ForwardingConsol consolBO, UniversalShipment dataObject)
		{
			var parentDataObject = new UniversalShipment(writeManager.WriterStrategy);

			var dataContextManager = consolBO.GetUniversalDataContextManager();
			parentDataObject.DataContext = DataContextFactory.New(dataContextManager, writeManager.Schema.Namespace);

			var shipmentDataSource = dataObject.DataContext.GetMatchingDataSource(DataContextType.ForwardingShipment);
			parentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentDataSource?.Key ?? ZString.Empty);

			var consignmentDataSource = dataObject.DataContext.GetMatchingDataSource(DataContextType.HVLVConsignment);
			parentDataObject.DataContext.AddDataSource(DataContextType.HVLVConsignment, consignmentDataSource?.Key ?? ZString.Empty);

			parentDataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_TransportMode, consolBO.JK_TransportMode_List);
			parentDataObject.VesselName = consolBO.JK_JX_JV_NKVessel;
			parentDataObject.VoyageFlightNo = consolBO.JK_JX_JV_VoyageFlight;

			parentDataObject.PortOfLoading = ListHelper.GetWithName(consolBO.JK_RL_NKLoadPort, consolBO.RefUNLOCO_List);
			parentDataObject.PortOfDischarge = ListHelper.GetWithName(consolBO.JK_RL_NKDischargePort, consolBO.RefUNLOCO_List);

			if (consolBO.JK_TransportMode == TransportModes.Air)
			{
				parentDataObject.WayBillNumber = consolBO.JK_MasterBillNum.FormatAirMAWB();
			}
			else
			{
				parentDataObject.WayBillNumber = consolBO.JK_MasterBillNum;
			}

			parentDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			parentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { dataObject }));
			return parentDataObject;
		}

		void MoveContainerCollectionToShipmentLevel(UniversalShipment consolDataObject, UniversalShipment shipmentDataObject)
		{
			shipmentDataObject.SetContainerCollection(() => consolDataObject.ContainerCollection);
			consolDataObject.SetContainerCollection(() => null);
		}

		void PopulateAdditionalBillsWithParentWaybillNumberFromConsolLevel(ForwardingShipment shipmentBO, UniversalShipment consolDataObject, UniversalShipment shipmentDataObject)
		{
			consignmentsToMerge?.ForEach(x => PopulateAdditionalBill(writeManager.WriterStrategy, x, shipmentDataObject, consolDataObject.WayBillNumber ?? ZString.Empty));
		}

		void PopulateAdditionalBill(IDataObjectWriterStrategy writerStrategy, HVLVConsignment consignmentBO, UniversalShipment dataObject, ZString parentBillNumber)
		{
			if (dataObject.AdditionalBillCollection != null || dataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>()))
			{
				var firstItem = consignmentBO.ActiveItems.FirstOrDefault();
				dataObject.AdditionalBillCollection.Add(new AdditionalBill(writerStrategy)
				{
					BillNumber = consignmentBO.HVC_WaybillNumber,
					BillType = new WayBillType() { Code = BillTypeList.Codes.HouseBill, Description = BillTypeList.Descriptions.HouseBill },
					ParentBillNumber = parentBillNumber,
					NoOfPacks = (ZDecimal)consignmentBO.HVC_ItemCount,
					PackType = ListHelper.GetWithDescription<PackageType>(firstItem?.HVI_F3_NKPackType ?? ZString.Empty, firstItem?.Lookups.PackTypes)
				});
			}
		}

		UniversalShipment InsertParentConsol(IDataWritingManager writeManager, ForwardingConsol consolBO, UniversalShipment dataObject, ForwardingShipment shipmentDataSource = null)
		{
			var dataWriterOptions = new DataWriterOptions()
			{
				IncludeSubShipments = false
			};

			UniversalShipment dataObjectWithParentConsol;

			var dataObjectCacheFactoryService = consolBO.Factory.ServiceContainer.GetService<DataObjectCacheFactoryService>();
			if (dataObjectCacheFactoryService != null)
			{
				dataObjectWithParentConsol = dataObjectCacheFactoryService.GetValue(dataObjectCacheFactoryService.BuildKey(JobConsolSchema.Constants.TableName, consolBO.PK.ToString()),
					() => new ConsolDataObjectWriter(writeManager, null, dataWriterOptions).GetDataObject(consolBO));
			}
			else
			{
				dataObjectWithParentConsol = new ConsolDataObjectWriter(writeManager, null, dataWriterOptions).GetDataObject(consolBO);
			}

			if (shipmentDataSource != null)
			{
				AddShipmentDataContext(shipmentDataSource, dataObjectWithParentConsol);
			}

			dataObjectWithParentConsol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { dataObject }));
			return dataObjectWithParentConsol;
		}

		void PopulateTransportDetails(ForwardingConsol consolBO, UniversalShipment dataObject)
		{
			dataObject.VesselName = consolBO.JK_JX_JV_NKVessel;
			dataObject.VoyageFlightNo = consolBO.JK_JX_JV_VoyageFlight;
			dataObject.PortOfLoading = ListHelper.GetWithName(consolBO.JK_RL_NKLoadPort, consolBO.RefUNLOCO_List);
			dataObject.PortOfDischarge = ListHelper.GetWithName(consolBO.JK_RL_NKDischargePort, consolBO.RefUNLOCO_List);
			dataObject.PortOfOrigin = ListHelper.GetWithName(consolBO.JK_RL_NKLoadPort, consolBO.RefUNLOCO_List);
			dataObject.PortOfDestination = ListHelper.GetWithName(consolBO.JK_RL_NKDischargePort, consolBO.RefUNLOCO_List);
			if (dataObject.DateCollection != null || dataObject.SetDateCollection(() => new List<Date>()))
			{
				dataObject.DateCollection.Add(new Date() { Type = DateType.Departure, Value = consolBO.JK_JX_JA_E_DEP, IsEstimate = ZBool.True });
				dataObject.DateCollection.Add(new Date() { Type = DateType.Arrival, Value = consolBO.JK_JX_JB_E_ARV, IsEstimate = ZBool.True });
				dataObject.DateCollection.Add(new Date() { Type = DateType.LoadingDate, Value = consolBO.JK_JX_JA_E_DEP, IsEstimate = ZBool.False });
				dataObject.DateCollection.Add(new Date() { Type = DateType.EntryDate, Value = consolBO.JK_JX_JB_E_ARV, IsEstimate = ZBool.False });
			}
		}

		#endregion

		#region HVLVConsignmentDataObjectWriterStrategy

		class HVLVConsignmentDataObjectWriterStrategy : IDataObjectWriterStrategy
		{
			public HVLVConsignmentDataObjectWriterStrategy(IDataObjectWriterStrategy parentStrategy, IHVLVConsignmentDataExportStrategy dataExportStrategy)
			{
				ParentStrategy = parentStrategy;
				DataExportStrategy = dataExportStrategy;
			}

			IDataObjectWriterStrategy ParentStrategy { get; }

			IHVLVConsignmentDataExportStrategy DataExportStrategy { get; }

			public bool IsAllowSet(string fieldName)
			{
				if (!DataExportStrategy.IsAllowSetForConsignmentDataExportContext(fieldName))
				{
					return false;
				}

				return ParentStrategy.IsAllowSet(fieldName);
			}
		}

		DisposableAction SetWriterStrategyTemporarily(UniversalShipment dataObject)
		{
			writerStrategy = new HVLVConsignmentDataObjectWriterStrategy(writeManager.WriterStrategy, DataExportStrategy);
			dataObject.SetWriterStrategy(writerStrategy);

			return new DisposableAction(() =>
			{
				writerStrategy = writeManager.WriterStrategy;
				dataObject.SetWriterStrategy(writerStrategy);
			});
		}

		IDataObjectWriterStrategy writerStrategy;

		#endregion
	}
}
