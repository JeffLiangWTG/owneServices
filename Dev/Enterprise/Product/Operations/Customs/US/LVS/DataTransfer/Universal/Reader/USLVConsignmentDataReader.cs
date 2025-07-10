using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;
using static Enterprise.Customs.US.LVS.Business.LVSConstants;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVConsignmentDataReader : ShipmentDataObjectReader<CusUSLVConsignment>
	{
		public USLVConsignmentDataReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusUSLVClearance clearance, CusUSLVConsignment existingConsignment)
		: base(dataObject, logger, factory)
		{
			this.clearance = clearance;
			this.existingConsignment = existingConsignment;
		}
		readonly CusUSLVClearance clearance;
		readonly CusUSLVConsignment existingConsignment;

		protected override IMatchingBusinessEntityFinder<CusUSLVConsignment> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override CusUSLVConsignment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return existingConsignment;
		}

		protected override CusUSLVConsignment GetNewBusinessObject()
		{
			return clearance?.CusUSLVConsignments.AddNew() ?? base.GetNewBusinessObject();
		}

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		protected override void PopulateBusinessObject(CusUSLVConsignment targetBO)
		{
			var delaySetters = new Dictionary<string, ValueSetter>();

			PopulateBusinessObjectWithDelaySetters(targetBO, delaySetters);
			delaySetters.SetValueInSpecificOrder(GetConsignmentDataSettingOrder(targetBO.PK));

			PopulateCommercialInfo(targetBO);
		}

		protected virtual void PopulateBusinessObjectWithDelaySetters(CusUSLVConsignment targetBO, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(targetBO, CusUSLVConsignmentSchema.ULB_OwnerReferenceNumber, dataObject.OwnerRef, delaySetters);

			if (dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House)
			{
				SetValue(targetBO, CusUSLVConsignmentSchema.ULB_HouseBill, dataObject.WayBillNumber, delaySetters);
			}

			SetValue(targetBO, CusUSLVConsignmentSchema.ULB_EquipmentNumber, dataObject.ContainerCollection?.FirstOrDefault()?.ContainerNumber, delaySetters);
			SetValue(targetBO, CusUSLVConsignmentSchema.ULB_NumberOfPacks, dataObject.TotalNoOfPacks ?? dataObject.TotalNoOfPieces, delaySetters);
			SetValue(targetBO, CusUSLVConsignmentSchema.ULB_PackType, dataObject.TotalNoOfPacksPackageType, delaySetters);

			PopulateValueFromAddInfoCollection(targetBO, delaySetters);
			PopulateValueOrganizationAddressCollection(targetBO, delaySetters);
		}

		IEnumerable<ZString> GetConsignmentDataSettingOrder(ZGuid pk)
		{
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_OwnerReferenceNumber);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_HouseBill);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_EquipmentNumber);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_NumberOfPacks);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_PackType);

			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_ConsigneeIdentifier);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_ConsigneeQualifier);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_ConsigneeName);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_ConsigneeAddress1);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_ConsigneeAddress2);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_RN_NKConsigneeCountry);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_ConsigneeCity);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_ConsigneePostCode);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_OA_Consignee);

			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_SellerIdentifier);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_SellerName);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_SellerAddress1);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_SellerAddress2);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_RN_NKSellerCountry);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_SellerCity);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_SellerPostCode);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVConsignmentSchema.ULB_OA_Seller);
		}

		void PopulateCommercialInfo(CusUSLVConsignment consignment)
		{
			consignment.CusUSLVItems.RemoveAndDeleteAll();
			if (dataObject.CommercialInfo?.CommercialInvoiceCollection?.Count > 0)
			{
				foreach (var invoice in dataObject.CommercialInfo.CommercialInvoiceCollection)
				{
					if (invoice.CommercialInvoiceLineCollection?.Count > 0)
					{
						foreach (var invoiceLine in invoice.CommercialInvoiceLineCollection)
						{
							var delaySetters = new Dictionary<string, ValueSetter>();

							var item = consignment.CusUSLVItems.AddNew();
							PopulateItem(item, invoice, invoiceLine, delaySetters);

							if (invoiceLine.AddInfoCollection?.Count > 0)
							{
								SetValue(item, CusUSLVItemSchema.ULI_AntiDumping, invoiceLine.AddInfoCollection.GetZBoolValue(AddInfoConstants.ADD_NA), delaySetters);
								SetValue(item, CusUSLVItemSchema.ULI_Countervailing, invoiceLine.AddInfoCollection.GetZBoolValue(AddInfoConstants.CVD_NA), delaySetters);
							}

							delaySetters.SetValueInSpecificOrder(GetUSLVItemDataSettingOrder(item.PK));

							if (invoiceLine.AddInfoCollection?.Count > 0)
							{
								PopulateInvoiceLineAddInfo(item, invoiceLine.AddInfoCollection);
							}
						}
					}
				}
			}
		}

		protected virtual void PopulateItem(CusUSLVItem item, CommercialInvoiceHeader invoice, CommercialInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(item, CusUSLVItemSchema.ULI_PartNo, invoiceLine.PartNo, delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_RX_NKCurrency, invoice.InvoiceCurrency.GetNullableCodeAsUpperCase(), delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_RN_NKCountryOfOrigin, invoiceLine.CountryOfOrigin.GetNullableCodeAsUpperCase(), delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_GoodsDescription, invoiceLine.Description, delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_Tariff, invoiceLine.HarmonisedCode, delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_GoodsValue, invoiceLine.LinePrice, delaySetters);
		}

		IEnumerable<ZString> GetUSLVItemDataSettingOrder(ZGuid pk)
		{
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_PartNo);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_RX_NKCurrency);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_RN_NKCountryOfOrigin);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_GoodsDescription);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_Tariff);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_GoodsValue);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_AntiDumping);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemSchema.ULI_Countervailing);
		}

		void PopulateInvoiceLineAddInfo(CusUSLVItem item, List<AddInfo> addInfoCollection)
		{
			foreach (var addInfo in addInfoCollection.Where(u => u.Key.GetValueOrDefault() != AddInfoConstants.ADD_NA && u.Key.GetValueOrDefault() != AddInfoConstants.CVD_NA).ToArray())
			{
				var delaySetters = new Dictionary<string, ValueSetter>();
				var pga = item.CusUSLVItemPGAs.AddNew();
				SetValue(pga, CusUSLVItemPGASchema.ULP_Indicator, US.Business.OGAIndicatorList.Codes.Disclaimed, delaySetters);
				SetValue(pga, CusUSLVItemPGASchema.ULP_DisclaimReason, addInfo.Value, delaySetters);

				if (addInfo.Key.HasValue)
				{
					var agencyInfo = PGAHelper.GetAgencyViaKey(addInfo.Key.Value);

					if (!string.IsNullOrEmpty(agencyInfo.Agency))
					{
						SetValue(pga, CusUSLVItemPGASchema.ULP_Agency, agencyInfo.Agency, delaySetters);
					}

					if (!string.IsNullOrEmpty(agencyInfo.AgencyProgram))
					{
						SetValue(pga, CusUSLVItemPGASchema.ULP_AgencyProgram, agencyInfo.AgencyProgram, delaySetters);
					}
				}

				delaySetters.SetValueInSpecificOrder(GetUSLVItemPGASettingOrder(pga.PK));
			}
		}

		IEnumerable<ZString> GetUSLVItemPGASettingOrder(ZGuid pk)
		{
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemPGASchema.ULP_DisclaimReason);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemPGASchema.ULP_Agency);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVItemPGASchema.ULP_AgencyProgram);
		}

		void PopulateValueOrganizationAddressCollection(CusUSLVConsignment consignment, Dictionary<string, ValueSetter> delaySetters)
		{
			var consigneeOrgAddress = GetMatchingOrgAddress(ConsigneeAddressType, out var consigneeAddress);
			if (consigneeOrgAddress != null)
			{
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_OA_Consignee, consigneeOrgAddress.PK, delaySetters);
			}
			else if (consigneeAddress != null)
			{
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneeName, consigneeAddress.CompanyName, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneeAddress1, consigneeAddress.Address1, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneeAddress2, consigneeAddress.Address2, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneeCity, consigneeAddress.City, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneePostCode, consigneeAddress.Postcode, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneeState, (ZString?)consigneeAddress.State, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_RN_NKConsigneeCountry, consigneeAddress.Country.GetNullableCodeAsUpperCase(), delaySetters);
			}

			var sellerOrgAddress = GetMatchingOrgAddress(ConsignorAddressType, out var sellerAddress);
			if (sellerOrgAddress != null)
			{
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_OA_Seller, sellerOrgAddress.PK, delaySetters);
			}
			else if (sellerAddress != null)
			{
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_SellerName, sellerAddress.CompanyName, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_SellerAddress1, sellerAddress.Address1, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_SellerAddress2, sellerAddress.Address2, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_SellerCity, sellerAddress.City, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_SellerPostCode, sellerAddress.Postcode, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_SellerState, (ZString?)sellerAddress.State, delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_RN_NKSellerCountry, sellerAddress.Country.GetNullableCodeAsUpperCase(), delaySetters);
			}
		}

		void PopulateValueFromAddInfoCollection(CusUSLVConsignment consignment, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.AddInfoCollection?.Count > 0)
			{
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_HouseBillIssuerSCAC, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.WayBillIssuerSCAC), delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_NonAMSIndicator, dataObject.AddInfoCollection.GetZBoolValue(AddInfoConstants.NonAMS), delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneeQualifier, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.ConsigneeType), delaySetters);
				SetValue(consignment, CusUSLVConsignmentSchema.ULB_ConsigneeIdentifier, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.ConsigneeReference), delaySetters);
			}
		}

		protected virtual string ConsigneeAddressType => Constants.AddressTypes.UltimateConsignee;
		protected virtual string ConsignorAddressType => AddressType.Seller;

		OrgAddress GetMatchingOrgAddress(string addressType, out OrganizationAddress organizationAddress)
		{
			organizationAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(addressType);

			OrgAddress result = null;
			if (organizationAddress != null && !organizationAddress.AddressOverride.GetValueOrDefault())
			{
				result = new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatched();
			}

			return result;
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.USCustomsLVConsignment; }
		}
	}
}
