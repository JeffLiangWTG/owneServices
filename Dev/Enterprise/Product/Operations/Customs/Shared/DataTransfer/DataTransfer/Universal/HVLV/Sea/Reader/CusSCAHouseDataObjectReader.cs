using System;
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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public abstract class CusSCAHouseDataObjectReader<TCusSCAHouse, TCusSCAPivot> : ShipmentDataObjectReader<TCusSCAHouse>
		where TCusSCAHouse : BaseCusSCAHouse
		where TCusSCAPivot : BaseCusSCAPivot
	{
		protected CusSCAHouseDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.oceanBill = Argument.NotNull(oceanBill, "oceanBill");
			this.hvlvConsolidatorShipmentWrapper = hvlvConsolidatorShipmentWrapper;
		}

		#region Overrides

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		public sealed override DataContextType DataContextType
		{
			get { return DataContextType.SeaHouseBill; }
		}

		protected sealed override IMatchingBusinessEntityFinder<TCusSCAHouse> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected sealed override TCusSCAHouse GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			TCusSCAHouse result = null;
			if (!IsHVLV || ((BusinessObject)oceanBill).IsInDatabase)
			{
				if (dataObject.WayBillNumber.HasValue)
				{
					var houseBill = dataObject.WayBillNumber.GetValueOrDefault().ToUpper();
					result = CusSCADataObjectHelper.LoadHouseBill<TCusSCAHouse>(oceanBill, houseBill, factory.BOFactory);
				}
			}
			return result;
		}

		protected override void PopulateBusinessObject(TCusSCAHouse targetBO)
		{
			using (targetBO.SuspendMarkingAsNeedingValidation())
			{
				var houseBill = GetColumnIndexer(targetBO);
				SetValue(houseBill, CusSCAHouseSchema.CA_CB, oceanBill.GetValue(CusSCAOceanBillSchema.PK));

				SetValue(houseBill, CusSCAHouseSchema.CA_HouseBill, dataObject.WayBillNumber);
				SetValue(houseBill, CusSCAHouseSchema.CA_RN_NKGoodsOrigin, dataObject.GoodsOrigin);
				SetValue(houseBill, CusSCAHouseSchema.CA_GoodsValue, dataObject.GoodsValue);
				SetValue(houseBill, CusSCAHouseSchema.CA_RX_NKGoodsCurrency, dataObject.GoodsValueCurrency.GetCodeAsUpperCase());
				SetValue(houseBill, CusSCAHouseSchema.CA_RL_NKLoadPort, dataObject.PortOfLoading);
				SetValue(houseBill, CusSCAHouseSchema.CA_RL_NKDischargePort, dataObject.PortOfDischarge);

				var hvlv = hvlvConsolidatorShipmentWrapper?.hvlvShipment;

				SetValue(houseBill, CusSCAHouseSchema.CA_RL_NK_PortOfDestination, hvlv != null ? hvlv.PortOfDestination : dataObject.PortOfDestination);
				SetValue(houseBill, CusSCAHouseSchema.CA_RL_NK_PortOfOrigin, hvlv != null ? hvlv.PortOfOrigin : dataObject.PortOfOrigin);
				SetValue(houseBill, CusSCAHouseSchema.CA_VendorIdentifier, dataObject.VendorIdentifier);
				SetValue(houseBill, CusSCAHouseSchema.CA_IsHVLV, hvlv != null);

				PopulatePackages(houseBill, dataObject);
				PopulateOrganizationsAddresses(houseBill);
				PopulateCountrySpecificDetails(houseBill);
				PopulateAddInfo(houseBill, dataObject);
			}
		}

		protected override LogType LogTypeForReasonNotAbleToUpdate
		{
			get
			{
				if (IsHVLV)
				{
					return LogType.Warning;
				}

				return base.LogTypeForReasonNotAbleToUpdate;
			}
		}

		#endregion // Overrides

		#region Implementation

		protected virtual void PopulateAddInfo(IColumnIndexer houseBill, Shipment shipmentDataObject)
		{
			SetValue(houseBill, CusSCAHouseSchema.CA_IsGSTPrePaid, shipmentDataObject.AddInfoCollection.GetZStringValue("IsGSTPrePaid", logger));
		}

		protected virtual void PopulateOrganizationsAddresses(IColumnIndexer houseBill)
		{
			PopulateConsignorAddress(houseBill);
			PopulateConsigneeAddress(houseBill);
			PopulateNotifyAddress(houseBill);
			PopulateGoodsLocationAddress(houseBill);
		}

		void PopulatePackages(IColumnIndexer houseBill, Shipment shipmentDataObject)
		{
			if (shipmentDataObject.PackingLineCollection != null)
			{
				if (!IsHVLV || ((IBusiness)houseBill).IsInDatabase)
				{
					var packages = CusSCADataObjectHelper.LoadPackages<TCusSCAPivot>(houseBill, factory.BOFactory);
					packages.DeleteAll();
				}

				PopulatePackingLineCollection(houseBill);
			}
		}

		protected virtual void PopulatePackingLineCollection(IColumnIndexer houseBill)
		{
			int lineNo = 0;

			foreach (var packageData in dataObject.PackingLineCollection)
			{
				lineNo++;
				GetNewCusSCAPivotDataObjectReader(lineNo, houseBill, packageData).ReadIntoBusinessObject();
			}
		}

		protected abstract CusSCAPivotDataObjectReader<TCusSCAPivot> GetNewCusSCAPivotDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, PackingLine packageDataObject);

		protected virtual ZString[] GetConsignorAddressTypesInPreferredOrder(IColumnIndexer houseBill)
		{
			return AddressTypeMatchHelper.GetConsignorAddressTypesInPreferredOrder();
		}

		protected virtual ZString[] GetConsigneeAddressTypesInPreferredOrder(IColumnIndexer houseBill)
		{
			return AddressTypeMatchHelper.GetConsigneeAddressTypesInPreferredOrder();
		}

		protected abstract void PopulateCountrySpecificDetails(IColumnIndexer houseBill);

		void PopulateOrganizationAddress(IColumnIndexer houseBill, ZString[] addressTypesInPreferredOrder, SchemaGuidColumn foreignKeyColumn, SchemaStringColumn nameColumn, SchemaStringColumn address1Column, SchemaStringColumn address2Column, SchemaStringColumn cityColumn, SchemaStringColumn stateColumn, SchemaStringColumn postcodeColumn, SchemaStringColumn countryColumn, SchemaStringColumn contactNameColumn, SchemaStringColumn phoneColumn, SchemaStringColumn faxColumn, Action<IColumnIndexer, OrgAddress, ZString?> updateHouseBillWithMatchedOrgAddress, Action<IColumnIndexer, OrganizationAddress, OrgAddress> additionalOrganizationUpdate = null)
		{
			OrganizationAddress addressData = null;
			OrgAddress address = null;
			if (dataObject.OrganizationAddressCollection != null)
			{
				addressData = dataObject.OrganizationAddressCollection.FirstOrDefault(addressTypesInPreferredOrder);

				if (addressData != null)
				{
					address = GetOrgAddressBO(addressData, logger.TopLevelDataContext);
					if (address != null && address.OA_OH != OrgHeader.UnmatchedOrganisationPK)
					{
						updateHouseBillWithMatchedOrgAddress.Invoke(houseBill, address, addressData.Contact);
					}
					else
					{
						PopulateOrganizationAddress(houseBill, addressData, foreignKeyColumn, nameColumn, address1Column, address2Column, cityColumn, stateColumn, postcodeColumn, countryColumn, contactNameColumn, phoneColumn, faxColumn);
					}
				}
				else
				{
					PopulateEmptyOrganizationAddress(houseBill, foreignKeyColumn, nameColumn, address1Column, address2Column, cityColumn, stateColumn, postcodeColumn, countryColumn, contactNameColumn, phoneColumn, faxColumn);
				}
			}
			additionalOrganizationUpdate?.Invoke(houseBill, addressData, address);
		}

		protected OrgAddress GetOrgAddressBO(OrganizationAddress orgAddressData, IDataContextDataObject topLevelDataContext)
		{
			if (topLevelDataContext != null && topLevelDataContext.CodesMappedToTarget)
			{
				if (orgAddressData.AddressOverride.GetValueOrDefault())
				{
					logger.Log(LogType.Information, Res.GetString("aacb9d09-ef7c-4f69-b338-115cbb889610", "Skipped Organization Match on Company [{0}].", orgAddressData.CompanyName));
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

		protected virtual void UpdateConsignorWithMatchedOrgAddress(IColumnIndexer houseBill, OrgAddress address, ZString? contactName)
		{
			SetValue(houseBill, CusSCAHouseSchema.CA_OH_Consignor, address.OA_OH);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorName, address.EffectiveCompanyNameTruncated);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorAddress1, address.OA_Address1);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorAddress2, address.OA_Address2);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorSuburb, address.OA_City);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorState, address.OA_State);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorPostcode, address.OA_PostCode);
			var relatedCountry = address.RelatedCountry;
			SetValue(houseBill, CusSCAHouseSchema.CA_RN_NKConsignorCountryCode, relatedCountry != null ? relatedCountry.RN_Code : ZString.Empty);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorContactName, contactName);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorPhone, address.OA_Phone);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsignorFax, address.OA_Fax);
		}

		protected virtual void UpdateConsigneeWithMatchedOrgAddress(IColumnIndexer houseBill, OrgAddress address, ZString? contactName)
		{
			SetValue(houseBill, CusSCAHouseSchema.CA_OH_Consignee, address.OA_OH);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneeName, address.EffectiveCompanyNameTruncated);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneeAddress1, address.OA_Address1);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneeAddress2, address.OA_Address2);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneeSuburb, address.OA_City);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneeState, address.OA_State);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneePostcode, address.OA_PostCode);
			var relatedCountry = address.RelatedCountry;
			SetValue(houseBill, CusSCAHouseSchema.CA_RN_NKConsigneeCountryCode, relatedCountry != null ? relatedCountry.RN_Code : ZString.Empty);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneeContactName, contactName);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneePhone, address.OA_Phone);
			SetValue(houseBill, CusSCAHouseSchema.CA_ConsigneeFax, address.OA_Fax);
		}

		protected virtual void UpdateNotificationAddressWithMatchedOrgAddress(IColumnIndexer houseBill, OrgAddress address, ZString? contactName)
		{
			SetValue(houseBill, CusSCAHouseSchema.CA_OH_Notify, address.OA_OH);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyName, address.EffectiveCompanyNameTruncated);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyAddress1, address.OA_Address1);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyAddress2, address.OA_Address2);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifySuburb, address.OA_City);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyState, address.OA_State);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyPostcode, address.OA_PostCode);
			var relatedCountry = address.RelatedCountry;
			SetValue(houseBill, CusSCAHouseSchema.CA_RN_NKNotifyCountryCode, relatedCountry != null ? relatedCountry.RN_Code : ZString.Empty);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyContactName, contactName);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyPhone, address.OA_Phone);
			SetValue(houseBill, CusSCAHouseSchema.CA_NotifyFax, address.OA_Fax);
		}

		protected void PopulateOrganizationAddress(IColumnIndexer houseBill, OrganizationAddress address, SchemaGuidColumn foreignKeyColumn, SchemaStringColumn nameColumn, SchemaStringColumn address1Column, SchemaStringColumn address2Column, SchemaStringColumn cityColumn, SchemaStringColumn stateColumn, SchemaStringColumn postcodeColumn, SchemaStringColumn countryColumn, SchemaStringColumn contactNameColumn, SchemaStringColumn phoneColumn, SchemaStringColumn faxColumn)
		{
			SetValue(houseBill, foreignKeyColumn, ZGuid.Empty);
			SetValue(houseBill, nameColumn, address.CompanyName);
			SetValue(houseBill, address1Column, address.Address1);
			SetValue(houseBill, address2Column, address.Address2);
			SetValue(houseBill, cityColumn, address.City);
			SetValue(houseBill, stateColumn, (ZString?)address.State);
			SetValue(houseBill, postcodeColumn, address.Postcode);
			SetValue(houseBill, countryColumn, address.Country);
			SetValue(houseBill, contactNameColumn, address.Contact);
			SetValue(houseBill, phoneColumn, address.Phone);
			SetValue(houseBill, faxColumn, address.Fax);
		}

		protected void PopulateEmptyOrganizationAddress(IColumnIndexer houseBill, SchemaGuidColumn foreignKeyColumn, SchemaStringColumn nameColumn, SchemaStringColumn address1Column, SchemaStringColumn address2Column, SchemaStringColumn cityColumn, SchemaStringColumn stateColumn, SchemaStringColumn postcodeColumn, SchemaStringColumn countryColumn, SchemaStringColumn contactNameColumn, SchemaStringColumn phoneColumn, SchemaStringColumn faxColumn)
		{
			SetValue(houseBill, foreignKeyColumn, ZGuid.Empty);
			SetValue(houseBill, nameColumn, ZString.Empty);
			SetValue(houseBill, address1Column, ZString.Empty);
			SetValue(houseBill, address2Column, ZString.Empty);
			SetValue(houseBill, cityColumn, ZString.Empty);
			SetValue(houseBill, stateColumn, ZString.Empty);
			SetValue(houseBill, postcodeColumn, ZString.Empty);
			SetValue(houseBill, countryColumn, ZString.Empty);
			SetValue(houseBill, contactNameColumn, ZString.Empty);
			SetValue(houseBill, phoneColumn, ZString.Empty);
			SetValue(houseBill, faxColumn, ZString.Empty);
		}

		void PopulateConsignorAddress(IColumnIndexer houseBill)
		{
			PopulateOrganizationAddress(houseBill, GetConsignorAddressTypesInPreferredOrder(houseBill),
				CusSCAHouseSchema.CA_OH_Consignor,
				CusSCAHouseSchema.CA_ConsignorName,
				CusSCAHouseSchema.CA_ConsignorAddress1,
				CusSCAHouseSchema.CA_ConsignorAddress2,
				CusSCAHouseSchema.CA_ConsignorSuburb,
				CusSCAHouseSchema.CA_ConsignorState,
				CusSCAHouseSchema.CA_ConsignorPostcode,
				CusSCAHouseSchema.CA_RN_NKConsignorCountryCode,
				CusSCAHouseSchema.CA_ConsignorContactName,
				CusSCAHouseSchema.CA_ConsignorPhone,
				CusSCAHouseSchema.CA_ConsignorFax,
				UpdateConsignorWithMatchedOrgAddress,
				AdditionalConsignorUpdate);
		}

		protected virtual void AdditionalConsignorUpdate(IColumnIndexer houseBill, OrganizationAddress address, OrgAddress orgAddress)
		{
		}

		void PopulateConsigneeAddress(IColumnIndexer houseBill)
		{
			PopulateOrganizationAddress(houseBill, GetConsigneeAddressTypesInPreferredOrder(houseBill),
				CusSCAHouseSchema.CA_OH_Consignee,
				CusSCAHouseSchema.CA_ConsigneeName,
				CusSCAHouseSchema.CA_ConsigneeAddress1,
				CusSCAHouseSchema.CA_ConsigneeAddress2,
				CusSCAHouseSchema.CA_ConsigneeSuburb,
				CusSCAHouseSchema.CA_ConsigneeState,
				CusSCAHouseSchema.CA_ConsigneePostcode,
				CusSCAHouseSchema.CA_RN_NKConsigneeCountryCode,
				CusSCAHouseSchema.CA_ConsigneeContactName,
				CusSCAHouseSchema.CA_ConsigneePhone,
				CusSCAHouseSchema.CA_ConsigneeFax,
				UpdateConsigneeWithMatchedOrgAddress);
		}

		void PopulateNotifyAddress(IColumnIndexer houseBill)
		{
			PopulateOrganizationAddress(houseBill, new ZString[] { nameof(DocAddressType.NotifyParty) },
				CusSCAHouseSchema.CA_OH_Notify,
				CusSCAHouseSchema.CA_NotifyName,
				CusSCAHouseSchema.CA_NotifyAddress1,
				CusSCAHouseSchema.CA_NotifyAddress2,
				CusSCAHouseSchema.CA_NotifySuburb,
				CusSCAHouseSchema.CA_NotifyState,
				CusSCAHouseSchema.CA_NotifyPostcode,
				CusSCAHouseSchema.CA_RN_NKNotifyCountryCode,
				CusSCAHouseSchema.CA_NotifyContactName,
				CusSCAHouseSchema.CA_NotifyPhone,
				CusSCAHouseSchema.CA_NotifyFax,
				UpdateNotificationAddressWithMatchedOrgAddress);
		}

		void PopulateGoodsLocationAddress(IColumnIndexer houseBill)
		{
			var goodsLocationDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.GoodsLocation));
			if (goodsLocationDataObject != null)
			{
				var goodsLocationAddress = new OrganisationDataObjectReader(goodsLocationDataObject, logger, factory).GetMatched();
				if (goodsLocationAddress != null)
				{
					SetValue(houseBill, CusSCAHouseSchema.CA_OA_GoodsLocation, goodsLocationAddress.PK);
				}
			}
		}

		protected bool IsHVLV => hvlvConsolidatorShipmentWrapper != null;

		protected readonly IColumnIndexer oceanBill;
		protected readonly HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper;

		#endregion // Implementation
	}
}
