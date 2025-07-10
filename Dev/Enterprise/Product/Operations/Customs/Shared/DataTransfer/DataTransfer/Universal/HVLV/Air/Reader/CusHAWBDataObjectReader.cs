using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Matching;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class CusHAWBDataObjectReader : CusHAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper>
	{
		protected internal CusHAWBDataObjectReader(Shipment shipmentDataObject, Shipment mawbDataObject, IXmlImportLogger logger, AirManifestDataObjectReaderHelper helper, CusMAWB mawb, CusHAWB masterHouse, bool isHVLV, bool singleHAWBCheck = false)
			: base(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterHouse, isHVLV, singleHAWBCheck)
		{
		}

		protected override CusHAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper> GetNewCusHAWBDataObjectReader(Shipment shipmentDataObject, CusHAWB masterHouse)
		{
			return new CusHAWBDataObjectReader(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterHouse, isHVLV, singleHAWBCheck);
		}
	}

	public abstract class CusHAWBDataObjectReader<TMAWB, THAWB, THelper> : ShipmentDataObjectReader<THAWB>
		where TMAWB : CusMAWB
		where THAWB : CusHAWB
		where THelper : AirManifestDataObjectReaderHelper
	{
		protected CusHAWBDataObjectReader(Shipment shipmentDataObject, Shipment mawbDataObject, IXmlImportLogger logger, THelper helper, TMAWB mawb, THAWB masterHouse, bool isHVLV, bool singleHAWBCheck = false)
			: base(shipmentDataObject, logger, helper.Factory)
		{
			this.helper = Argument.NotNull(helper, "AirManifestDataObjectReaderHelper helper");
			this.mawb = Argument.NotNull(mawb, "CusMAWB mawb");
			this.masterHouse = masterHouse;
			this.singleHAWBCheck = singleHAWBCheck;
			this.isHVLV = isHVLV;
			this.mawbDataObject = mawbDataObject;
		}

		protected readonly THelper helper;
		protected readonly TMAWB mawb;
		protected readonly THAWB masterHouse;
		protected readonly bool singleHAWBCheck;
		protected readonly bool isHVLV;
		protected readonly Shipment mawbDataObject;

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		public override DataContextType DataContextType => DataContextType.AirManifestLine;

		protected sealed override THAWB GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			THAWB result = null;
			if (!isHVLV || mawb.IsInDatabase)
			{
				if (dataObject.WayBillNumber.HasValue)
				{
					var hawb = dataObject.WayBillNumber.GetValueOrDefault();
					result = (THAWB)mawb.ChildBills.Find((o) => o.CS_HAWB.EqualsIgnoringCase(hawb)).FirstOrDefault();
				}
			}
			return result;
		}

		protected sealed override THAWB GetNewBusinessObject()
		{
			// use this to handle correct type
			return (THAWB)factory.New(mawb.ChildBills.TypeOfElements);
		}

		protected sealed override void PopulateBusinessObject(THAWB hawb)
		{
			var hawbRow = GetColumnIndexer(hawb);
			var billPK = hawbRow.GetValue(CusHAWBSchema.PK);
			if (CheckUpdateHAWBDataIsAllowed(hawb))
			{
				SetValue(hawbRow, CusHAWBSchema.CS_CM, mawb.PK);
				SetValue(hawbRow, CusHAWBSchema.CS_HAWB, dataObject.WayBillNumber);
				if (masterHouse != null)
				{
					SetValue(hawbRow, CusHAWBSchema.CS_CS_MasterHouseBill, masterHouse.PK);
					SetValue(hawbRow, CusHAWBSchema.CS_MasterHouseBill, masterHouse.CS_HAWB);
					SetValue(hawbRow, CusHAWBSchema.CS_IsMasterHouse, ZBool.False);
				}
				else
				{
					SetValue(hawbRow, CusHAWBSchema.CS_CS_MasterHouseBill, ZGuid.Empty);
					var additionalBill = dataObject.AdditionalBillCollection.GetAdditionalBill(dataObject.WayBillNumber.GetValueOrDefault(), dataObject.WayBillType.GetCodeAsUpperCase());
					SetValue(hawbRow, CusHAWBSchema.CS_MasterHouseBill, additionalBill == null ? ZString.Empty : additionalBill.ParentBillNumber);
					SetValue(hawbRow, CusHAWBSchema.CS_IsMasterHouse, dataObject.SubShipmentCollection != null || dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse);
				}
				SetValue(hawbRow, CusHAWBSchema.CS_RL_NKOrigin, isHVLV ? mawbDataObject?.PortOfOrigin : dataObject.PortOfOrigin);
				SetValue(hawbRow, CusHAWBSchema.CS_RL_NKDestination, isHVLV ? mawbDataObject?.PortOfDestination : dataObject.PortOfDestination);

				FillPackType(hawbRow);
				FillGoodsLocation(hawbRow);

				FillWeight(hawbRow);
				FillPiecesManifested(hawbRow);
				FillGoodsDescription(hawbRow, hawb);
				FillGoodsValue(hawbRow);
				FillResponsiblePartyDetails(hawbRow);
				FillConsignor(hawb);
				FillConsignee(hawb);
				FillCountrySpecificDetails(hawb);
				if (isHVLV)
				{
					SetValue(hawbRow, CusHAWBSchema.CS_IsHVLV, ZBool.True);
				}
				SetValue(hawbRow, CusHAWBSchema.CS_VendorIdentifier, dataObject.VendorIdentifier);
			}
			FillSubBill(hawb);
		}

		protected void FillGoodsLocation(IColumnIndexer hawbRow)
		{
			OrganizationAddress goodsLocationData = null;

			if (dataObject.OrganizationAddressCollection != null)
			{
				goodsLocationData = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.GoodsLocation));
			}
			if (goodsLocationData == null)
			{
				goodsLocationData = mawbDataObject?.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.GoodsLocation));
			}

			if (goodsLocationData != null)
			{
				var goodsLocationAddress = GetOrgAddressBO(goodsLocationData, logger.TopLevelDataContext);
				if (goodsLocationAddress != null)
				{
					SetValue(hawbRow, CusHAWBSchema.CS_OA_GoodsLocation, goodsLocationAddress.PK);
				}
			}
		}

		protected virtual void FillGoodsDescription(IColumnIndexer hawbRow, THAWB hawb)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_GoodsDescription, dataObject.GoodsDescription);
		}

		protected virtual void FillGoodsValue(IColumnIndexer hawbRow)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_GoodsValue, dataObject.GoodsValue);
			SetValue(hawbRow, CusHAWBSchema.CS_RX_NKGoodsCurrency, dataObject.GoodsValueCurrency);
		}

		protected virtual void FillWeight(IColumnIndexer hawbRow)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_Weight, dataObject.TotalWeight);
			SetValue(hawbRow, CusHAWBSchema.CS_WeightUQ, dataObject.TotalWeightUnit);
		}

		protected virtual void FillPiecesManifested(IColumnIndexer hawbRow)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_PiecesManifested, dataObject.TotalNoOfPieces);
		}

		protected virtual void FillPackType(IColumnIndexer hawbRow)
		{
			if (dataObject.TotalNoOfPacksPackageType != null)
			{
				SetValue(hawbRow, CusHAWBSchema.CS_PackType, ConvertCustomsPackageType(dataObject.TotalNoOfPacksPackageType.Code ?? ZString.Empty));
			}
		}

		protected virtual ZString ConvertCustomsPackageType(ZString packType)
		{
			return packType;
		}

		protected virtual bool CheckUpdateHAWBDataIsAllowed(THAWB hawb)
		{
			return true;
		}

		protected void FillOrgAddress(IColumnIndexer hawbRow, ZString[] addressTypes, SchemaGuidColumn organisationPKColumn, SchemaStringColumn nameColumn, SchemaStringColumn address1Column, SchemaStringColumn address2Column, SchemaStringColumn cityColumn, SchemaStringColumn stateColumn, SchemaStringColumn postcodeColumn, SchemaStringColumn countryColumn, SchemaStringColumn contactNameColumn, SchemaStringColumn phoneColumn, Action<IColumnIndexer, OrgAddress, ZString?> updateHouseBillWithMatchedOrgAddress, Action<IColumnIndexer, OrganizationAddress, OrgAddress> additionalOrganizationUpdate = null)
		{
			var orgAddressData = dataObject.OrganizationAddressCollection.FirstOrDefault(addressTypes);
			var formattedAddress = new OrganizationAddressFormatted(orgAddressData);

			OrgAddress orgAddressBO = null;
			if (orgAddressData == null)
			{
				SetValue(hawbRow, nameColumn, ZString.Empty);
				SetValue(hawbRow, address1Column, ZString.Empty);
				SetValue(hawbRow, address2Column, ZString.Empty);
				SetValue(hawbRow, cityColumn, ZString.Empty);
				SetValue(hawbRow, stateColumn, ZString.Empty);
				SetValue(hawbRow, postcodeColumn, ZString.Empty);
				SetValue(hawbRow, countryColumn, ZString.Empty);
				SetValue(hawbRow, contactNameColumn, ZString.Empty);
				SetValue(hawbRow, phoneColumn, ZString.Empty);
				SetValue(hawbRow, organisationPKColumn, ZGuid.Empty);
			}
			else
			{
				orgAddressBO = GetOrgAddressBO(orgAddressData, logger.TopLevelDataContext);
				if (orgAddressBO == null || orgAddressBO.OA_OH == OrgHeader.UnmatchedOrganisationPK)
				{
					SetValue(hawbRow, nameColumn, formattedAddress.CompanyName);
					SetValue(hawbRow, address1Column, formattedAddress.Address1);
					SetValue(hawbRow, address2Column, formattedAddress.Address2);
					SetValue(hawbRow, cityColumn, formattedAddress.City);
					SetValue(hawbRow, stateColumn, formattedAddress.State);
					SetValue(hawbRow, postcodeColumn, formattedAddress.Postcode);
					SetValue(hawbRow, countryColumn, formattedAddress.Country);
					SetValue(hawbRow, contactNameColumn, formattedAddress.Contact);
					SetValue(hawbRow, phoneColumn, formattedAddress.Phone);
					SetValue(hawbRow, organisationPKColumn, ZGuid.Empty);
				}
				else
				{
					updateHouseBillWithMatchedOrgAddress.Invoke(hawbRow, orgAddressBO, formattedAddress.Contact);
				}
			}
			additionalOrganizationUpdate?.Invoke(hawbRow, orgAddressData, orgAddressBO);
		}

		protected OrgAddress GetOrgAddressBO(OrganizationAddress orgAddressData, IDataContextDataObject topLevelDataContext)
		{
			if (topLevelDataContext != null && topLevelDataContext.CodesMappedToTarget)
			{
				if (orgAddressData.AddressOverride.GetValueOrDefault())
				{
					logger.Log(LogType.Information, Res.GetString("b0e891c6-b872-4dcc-b744-dd1ae33aa7c9", "Skipped Organization Match on Company [{0}].", orgAddressData.CompanyName));
					return null;
				}
				else
				{
					var orgAddress = GetMatchedOrgAddress(orgAddressData, factory.BOFactory);
					if (orgAddress != null)
					{
						OrganisationDataObjectReader.LogSuccessfulMatch(orgAddress.Header, logger);
						return orgAddress;
					}
				}
			}

			return new OrganisationDataObjectReader(orgAddressData, logger, factory).GetMatched();
		}

		protected virtual OrgAddress GetMatchedOrgAddress(OrganizationAddress orgAddressData, BusinessObjectFactory boFactory)
		{
			return orgAddressData.GetMatchedUsingCodes(boFactory);
		}

		protected virtual void UpdateConsignorWithMatchedOrgAddress(IColumnIndexer hawbRow, OrgAddress orgAddressBO, ZString? contactName)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_OH_Consignor, orgAddressBO.OA_OH);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorName, orgAddressBO.EffectiveCompanyNameTruncated);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorStreet, orgAddressBO.OA_Address1);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorStreet2, orgAddressBO.OA_Address2);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorCity, orgAddressBO.OA_City);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorState, orgAddressBO.OA_State);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorPostcode, orgAddressBO.OA_PostCode);
			var relatedCountry = orgAddressBO.RelatedCountry;
			SetValue(hawbRow, CusHAWBSchema.CS_RN_NKConsignorCountry, relatedCountry == null ? ZString.Empty : relatedCountry.RN_Code);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorContactName, contactName);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsignorPhone, orgAddressBO.OA_Phone);
		}

		protected virtual void UpdateConsigneeWithMatchedOrgAddress(IColumnIndexer hawbRow, OrgAddress orgAddressBO, ZString? contactName)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_OH_Consignee, orgAddressBO.OA_OH);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneeName, orgAddressBO.EffectiveCompanyNameTruncated);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneeStreet, orgAddressBO.OA_Address1);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneeStreet2, orgAddressBO.OA_Address2);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneeCity, orgAddressBO.OA_City);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneeState, orgAddressBO.OA_State);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneePostcode, orgAddressBO.OA_PostCode);
			var relatedCountry = orgAddressBO.RelatedCountry;
			SetValue(hawbRow, CusHAWBSchema.CS_RN_NKConsigneeCountry, relatedCountry == null ? ZString.Empty : relatedCountry.RN_Code);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneeContactName, contactName);
			SetValue(hawbRow, CusHAWBSchema.CS_ConsigneePhone, orgAddressBO.OA_Phone);
		}

		protected virtual void FillConsignee(THAWB hawb)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var hawbRow = GetColumnIndexer(hawb);
				var isMasterHouse = hawbRow.GetValue(CusHAWBSchema.CS_IsMasterHouse);
				var addressTypes = isMasterHouse ? GetReceivingForwarderAddressTypesInPreferOrder() : GetConsigneeAddressTypesInPreferredOrder();
				FillOrgAddress(hawbRow, addressTypes, CusHAWBSchema.CS_OH_Consignee, CusHAWBSchema.CS_ConsigneeName, CusHAWBSchema.CS_ConsigneeStreet, CusHAWBSchema.CS_ConsigneeStreet2,
					CusHAWBSchema.CS_ConsigneeCity, CusHAWBSchema.CS_ConsigneeState, CusHAWBSchema.CS_ConsigneePostcode, CusHAWBSchema.CS_RN_NKConsigneeCountry, CusHAWBSchema.CS_ConsigneeContactName,
					CusHAWBSchema.CS_ConsigneePhone, UpdateConsigneeWithMatchedOrgAddress,
					AdditionalConsigneeUpdate);
			}
		}

		protected virtual ZString[] GetConsigneeAddressTypesInPreferredOrder()
		{
			return AddressTypeMatchHelper.GetConsigneeAddressTypesInPreferredOrder();
		}

		protected virtual ZString[] GetReceivingForwarderAddressTypesInPreferOrder()
		{
			return new ZString[] { nameof(DocAddressType.ReceivingForwarderAddress) };
		}

		protected virtual void FillConsignor(THAWB hawb)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var hawbRow = GetColumnIndexer(hawb);
				var isMasterHouse = hawbRow.GetValue(CusHAWBSchema.CS_IsMasterHouse);
				var addressTypes = isMasterHouse ? GetSendingForwarderAddressTypesInPreferOrder() : GetConsignorAddressTypesInPreferredOrder();
				FillOrgAddress(hawbRow, addressTypes, CusHAWBSchema.CS_OH_Consignor, CusHAWBSchema.CS_ConsignorName,
					CusHAWBSchema.CS_ConsignorStreet, CusHAWBSchema.CS_ConsignorStreet2,
					CusHAWBSchema.CS_ConsignorCity, CusHAWBSchema.CS_ConsignorState, CusHAWBSchema.CS_ConsignorPostcode,
					CusHAWBSchema.CS_RN_NKConsignorCountry, CusHAWBSchema.CS_ConsignorContactName,
					CusHAWBSchema.CS_ConsignorPhone, UpdateConsignorWithMatchedOrgAddress,
					AdditionalConsignorUpdate);
			}
		}

		protected virtual void AdditionalConsignorUpdate(IColumnIndexer houseBill, OrganizationAddress address, OrgAddress orgAddress)
		{
		}

		protected virtual void AdditionalConsigneeUpdate(IColumnIndexer houseBill, OrganizationAddress address, OrgAddress orgAddress)
		{
		}

		protected virtual ZString[] GetSendingForwarderAddressTypesInPreferOrder()
		{
			return new ZString[] { nameof(DocAddressType.SendingForwarderAddress) };
		}

		protected virtual ZString[] GetConsignorAddressTypesInPreferredOrder()
		{
			return AddressTypeMatchHelper.GetConsignorAddressTypesInPreferredOrder();
		}

		protected virtual void FillResponsiblePartyDetails(IColumnIndexer hawbRow)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				var responsiblePartyID = ZString.Empty;
				var responsiblePartyIDData = dataObject.AdditionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID);
				if (responsiblePartyIDData != null)
				{
					responsiblePartyID = responsiblePartyIDData.ReferenceNumber.GetValueOrDefault();
				}
				SetValue(hawbRow, CusHAWBSchema.CS_ResponsiblePartyID, responsiblePartyID);
			}
		}

		void FillSubBill(THAWB hawb)
		{
			if (masterHouse == null && dataObject.SubShipmentCollection != null)
			{
				foreach (var shipmentDataObject in dataObject.SubShipmentCollection)
				{
					var bill = GetNewCusHAWBDataObjectReader(shipmentDataObject, hawb).ReadIntoBusinessObject();
					helper.MarkProcessed(bill);
				}
			}
		}

		protected abstract CusHAWBDataObjectReader<TMAWB, THAWB, THelper> GetNewCusHAWBDataObjectReader(Shipment shipmentDataObject, THAWB masterHouse);

		protected virtual void FillCountrySpecificDetails(THAWB hawb)
		{
		}

		protected override IMatchingBusinessEntityFinder<THAWB> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for Customs. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(THAWB targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty && singleHAWBCheck && dataObject.SubShipmentCollection != null && dataObject.SubShipmentCollection.Count > 1)
			{
				result = Res.GetString("09D19144-4BCD-4DDE-A22C-0102A6319A16", "Only one Sub Bill ({0} element) per House Bill is allowed for '{1}' importation.", "SubShipmentCollection", nameof(DataContextType.AirManifestLine));
			}
			return result;
		}
	}
}
