using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVConsignmentDataWriter : TopLevelDataObjectWriter<CusUSLVConsignment, Shipment>
	{
		public USLVConsignmentDataWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.USCustomsLVConsignment;

		protected override void PopulateDataObject(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			shipment.OwnerRef = sourceBO.ULB_OwnerReferenceNumber;
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
			shipment.WayBillNumber = sourceBO.ULB_HouseBill;
			shipment.TotalNoOfPacksPackageType = new PackageType() { Code = sourceBO.ULB_PackType, Description = sourceBO.ULB_PackType };

			PopulateTotalNoOfPacks(sourceBO, shipment);
			PopulateCommercialInfoAndPackingLines(sourceBO, shipment);
			PopulateOrganizationAddressCollection(sourceBO, shipment);
			PopulateContainerCollection(sourceBO, shipment);
			PopulateAdditionalBillCollection(sourceBO, shipment);
			PopulateAddInfoCollection(sourceBO, shipment);
			PopulateEntryNumberCollection(sourceBO, shipment);
		}

		protected virtual void PopulateTotalNoOfPacks(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			shipment.TotalNoOfPacks = sourceBO.ULB_NumberOfPacks;
		}

		protected virtual void PopulateCommercialInfoAndPackingLines(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			shipment.CommercialInfo = GetCommercialInfo(sourceBO);
		}

		void PopulateOrganizationAddressCollection(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			var creatingConsolidatedSummary = writeManager.Action?.ActionType.Equals(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage) ?? false;
			if (!sourceBO.ULB_OA_Consignee.IsEmpty)
			{
				shipment.AddOrgAddress(writeManager, sourceBO.Consignee, Constants.AddressTypes.UltimateConsignee);
			}
			else if (creatingConsolidatedSummary)
			{
				var consigneeIdentifier = sourceBO.ULB_ConsigneeIdentifier;

				var newAddress = CreateOrgAddressFromFreeTexts(sourceBO.ULB_HouseBill, sourceBO.ULB_OA_ConsigneeInfo, (org) => org.OH_IsConsignee = true, sourceBO.ULB_ConsigneeName, sourceBO.ULB_ConsigneeAddress1, sourceBO.ULB_ConsigneeAddress2, sourceBO.ULB_ConsigneeCity, sourceBO.ULB_ConsigneePostCode, sourceBO.ULB_ConsigneeState, sourceBO.ULB_RN_NKConsigneeCountry);

				if (!consigneeIdentifier.IsEmpty)
				{
					newAddress.Header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, consigneeIdentifier, Core.Constants.CountryCodes.UnitedStates);
				}

				shipment.AddOrgAddress(writeManager, newAddress, Constants.AddressTypes.UltimateConsignee);
			}
			else if (!sourceBO.ULB_ConsigneeName.IsEmpty && !sourceBO.ULB_ConsigneeAddress1.IsEmpty)
			{
				shipment.AddOrgAddress(CreateOrganizationAddress(Constants.AddressTypes.UltimateConsignee, sourceBO.ULB_ConsigneeName, sourceBO.ULB_ConsigneeAddress1, sourceBO.ULB_ConsigneeAddress2, sourceBO.ULB_ConsigneeCity, sourceBO.ULB_ConsigneePostCode, sourceBO.ULB_ConsigneeState, Country.New(sourceBO.ConsigneeCountry)));
			}

			if (!sourceBO.ULB_OA_Seller.IsEmpty)
			{
				shipment.AddOrgAddress(writeManager, sourceBO.Seller, AddressType.Seller);
			}
			else if (creatingConsolidatedSummary)
			{
				var newAddress = CreateOrgAddressFromFreeTexts(sourceBO.ULB_HouseBill, sourceBO.ULB_OA_SellerInfo, (org) => org.OH_IsConsignor = true, sourceBO.ULB_SellerName, sourceBO.ULB_SellerAddress1, sourceBO.ULB_SellerAddress2, sourceBO.ULB_SellerCity, sourceBO.ULB_SellerPostCode, sourceBO.ULB_SellerState, sourceBO.ULB_RN_NKSellerCountry);

				if (!sourceBO.ULB_SellerIdentifier.IsEmpty)
				{
					var regNum = newAddress.Header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, sourceBO.ULB_SellerIdentifier, Core.Constants.CountryCodes.UnitedStates);
					regNum.OK_OA_PremisesAddress = newAddress.PK;
				}

				shipment.AddOrgAddress(writeManager, newAddress, AddressType.Seller);
			}
			else if (!sourceBO.ULB_SellerName.IsEmpty && !sourceBO.ULB_SellerAddress1.IsEmpty)
			{
				shipment.AddOrgAddress(CreateOrganizationAddress(AddressType.Seller, sourceBO.ULB_SellerName, sourceBO.ULB_SellerAddress1, sourceBO.ULB_SellerAddress2, sourceBO.ULB_SellerCity, sourceBO.ULB_SellerPostCode, sourceBO.ULB_SellerState, Country.New(sourceBO.SellerCountry)));
			}
		}

		OrgAddress CreateOrgAddressFromFreeTexts(ZString billNumber, ZPropertyInfo addressInfo, Action<OrgHeader> processOrgHeader, ZString companyName, ZString address1, ZString address2, ZString city, ZString postCode, ZString state, string countryCode)
		{
			var org = addressInfo.BizObj.Factory.New<OrgHeader>();
			org.OH_IsTempAccount = true;
			org.OH_Code = new ZString($"_{billNumber.KeepAlphanumericCharacters().Right(3)}{org.GetHashCode()}").Left(org.OH_CodeInfo.MaxLength);
			org.OH_FullName = companyName;

			processOrgHeader?.Invoke(org);

			var mainAddress = org.MainAddress;

			mainAddress.OA_Address1 = address1;
			mainAddress.OA_Address2 = address2;
			mainAddress.OA_City = city;
			mainAddress.OA_State = state;
			mainAddress.OA_PostCode = postCode;
			mainAddress.OA_RN_NKCountryCode = countryCode;

			addressInfo.Value = mainAddress.PK;

			return mainAddress;
		}

		protected OrganizationAddress CreateOrganizationAddress(ZString addressType, ZString companyName, ZString address1, ZString address2, ZString city, ZString postCode, ZString state, Country country)
		{
			return new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = addressType,
				CompanyName = companyName,
				Address1 = address1,
				Address2 = address2,
				City = city,
				Postcode = postCode,
				State = state,
				Country = country,
				AddressOverride = true
			};
		}

		void PopulateContainerCollection(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			if (!sourceBO.ULB_EquipmentNumber.IsEmpty)
			{
				shipment.SetContainerCollection(() => new DataObjectList<Container>(new[]
				{
					new Container
					{
						ContainerNumber = sourceBO.ULB_EquipmentNumber
					}
				}));
			}
		}

		protected virtual void PopulateAdditionalBillCollection(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			if (shipment.AdditionalBillCollection != null || shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>()))
			{
				var additionalBill = new AdditionalBill
				{
					BillNumber = sourceBO.ULB_HouseBill,
					BillType = new WayBillType
					{
						Code = WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House,
					},
				};

				var listAddInfo = new List<UAddInfo>
				{
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.UI_NKBillIssuerSCAC,
						Value = sourceBO.ULB_HouseBillIssuerSCAC,
					}
				};
				additionalBill.AddInfoCollection = listAddInfo;

				shipment.AdditionalBillCollection.Add(additionalBill);
			}
		}

		void PopulateAddInfoCollection(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			shipment.SetAddInfoCollection(() => new List<UAddInfo>
			{
				new UAddInfo
				{
					Key = LVSConstants.AddInfoConstants.WayBillIssuerSCAC,
					Value = sourceBO.ULB_HouseBillIssuerSCAC
				},
				new UAddInfo
				{
					Key = LVSConstants.AddInfoConstants.NonAMS,
					Value = sourceBO.ULB_NonAMSIndicator ? YesNoList.Codes.Yes : YesNoList.Codes.No
				},
				new UAddInfo
				{
					Key = LVSConstants.AddInfoConstants.ConsigneeType,
					Value = sourceBO.ULB_ConsigneeQualifier
				},
				new UAddInfo
				{
					Key = LVSConstants.AddInfoConstants.ConsigneeReference,
					Value = sourceBO.ULB_ConsigneeIdentifier
				},
			});
		}

		void PopulateEntryNumberCollection(CusUSLVConsignment sourceBO, Shipment shipment)
		{
			if (!sourceBO.CE_EntryNum.IsEmpty)
			{
				shipment.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>()
				{
					new UniversalDataBuss.DataObjects.Universal.EntryNumber()
					{
						Number = sourceBO.CE_EntryNum,
						Type = new EntryType()
						{
							Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary,
							Description = CusEntryHeaderMessageTypeList.Descriptions.EntrySummary
						}
					}
				});
			}
		}

		protected CommercialInfo GetCommercialInfo(CusUSLVConsignment sourceBO)
		{
			var commercialInfo = new CommercialInfo();
			var commercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			foreach (CusUSLVItem uSLVItem in sourceBO.CusUSLVItems)
			{
				var lineNo = 1;
				if (uSLVItem != null)
				{
					var commercialInvoice = commercialInvoiceCollection.FirstOrDefault(n => n.InvoiceCurrency.Code.ToString() == uSLVItem.ULI_RX_NKCurrency);
					if (commercialInvoice == null)
					{
						commercialInvoice = new CommercialInvoiceHeader(writeManager.WriterStrategy) { InvoiceCurrency = new Currency() { Code = uSLVItem.ULI_RX_NKCurrency } };
						commercialInvoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
						commercialInvoiceCollection.Add(commercialInvoice);
					}

					lineNo = commercialInvoice.CommercialInvoiceLineCollection.Count + 1;

					commercialInvoice.CommercialInvoiceLineCollection.Add(new CommercialInvoiceLine()
					{
						CountryOfOrigin = new Country() { Code = uSLVItem.ULI_RN_NKCountryOfOrigin },
						Description = uSLVItem.ULI_GoodsDescription,
						HarmonisedCode = uSLVItem.ULI_Tariff,
						LinePrice = uSLVItem.ULI_GoodsValue,
						LineNo = lineNo,
						PartNo = uSLVItem.ULI_PartNo,
						AddInfoCollection = GetInvoiceLineAddInfo(uSLVItem)
					});
				}
			}
			commercialInfo.CommercialInvoiceCollection = commercialInvoiceCollection;
			return commercialInfo;
		}

		protected List<UAddInfo> GetInvoiceLineAddInfo(CusUSLVItem cusUSLVItem)
		{
			var listAddInfo = new List<UAddInfo>()
			{
				new UAddInfo()
				{
					Key = LVSConstants.AddInfoConstants.ADD_NA,
					Value = cusUSLVItem.ULI_AntiDumping ? YesNoList.Codes.Yes : YesNoList.Codes.No,
				},
				new UAddInfo()
				{
					Key = LVSConstants.AddInfoConstants.CVD_NA,
					Value = cusUSLVItem.ULI_Countervailing ? YesNoList.Codes.Yes : YesNoList.Codes.No,
				},
			};
			listAddInfo.AddRange(PopulatePGADisclaimReasonFields(cusUSLVItem));
			listAddInfo.AddRange(PopulateAdditionalAddInfoFields(cusUSLVItem));
			return listAddInfo;
		}

		protected virtual List<UAddInfo> PopulateAdditionalAddInfoFields(CusUSLVItem cusUSLVItem)
		{
			return new List<UAddInfo>();
		}

		protected virtual List<UAddInfo> PopulatePGADisclaimReasonFields(CusUSLVItem cusUSLVItem)
		{
			var listAddInfo = new List<UAddInfo>();
			foreach (CusUSLVItemPGA itemPGA in cusUSLVItem.CusUSLVItemPGAs)
			{
				listAddInfo.Add(GenerateDisclaimReasonAddInfo(itemPGA));
			}
			return listAddInfo;
		}

		protected UAddInfo GenerateDisclaimReasonAddInfo(CusUSLVItemPGA itemPGA)
		{
			var addInfo = new UAddInfo();
			addInfo.Key = new PGAHelper(itemPGA).DisclaimReason.SubstringSafe(0, 25);
			addInfo.Value = itemPGA.ULP_DisclaimReason;
			return addInfo;
		}
	}
}
