using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	internal class CusSCAHouseDataObjectWriter : CusSCAHouseDataObjectWriter<BaseCusSCAHouse, BaseCusSCAPivot>
	{
		public CusSCAHouseDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ICodeDescriptionPairList GetShipmentStatuses()
		{
			return null;
		}

		protected override ICodeDescriptionPairList GetMessageStatuses()
		{
			return null;
		}

		protected override CusSCAPivotDataObjectWriter<BaseCusSCAPivot> GetNewCusSCAPivotDataObjectWriter()
		{
			return new CusSCAPivotDataObjectWriter(writeManager);
		}
	}

	public abstract class CusSCAHouseDataObjectWriter<TCusSCAHouse, TCusSCAPivot> : TopLevelDataObjectWriter<TCusSCAHouse, Shipment>
		where TCusSCAHouse : BaseCusSCAHouse
		where TCusSCAPivot : BaseCusSCAPivot
	{
		protected CusSCAHouseDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		#region Overrides

		protected override CargoWise.Types.ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected sealed override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.SeaHouseBill;
		}

		protected sealed override void PopulateDataObject(TCusSCAHouse sourceBO, Shipment dataObject)
		{
			PopulateDataObjectCore(sourceBO, dataObject, false);
		}

		void PopulateDataObjectCore(TCusSCAHouse houseBill, Shipment data, bool keepExistingData)
		{
			data.TransportMode = PopulateValue(data.TransportMode, keepExistingData,
				() => ListHelper.GetWithDescription<CodeDescriptionPair>(Core.Constants.TransportModes.Sea, Factory.GetCachedCodeDescriptionPairList(Enterprise.ZArchitecture.Core.OLookUpEditType.TransportType)));
			data.WayBillType = PopulateValue(data.WayBillType, keepExistingData, () => GetWayBillType(GetWayBillTypeCode(houseBill)));
			data.WayBillNumber = PopulateValue(data.WayBillNumber, keepExistingData,
				() => houseBill.GetValue(CusSCAHouseSchema.CA_HouseBill));
			data.ShipmentStatus = PopulateValue(data.ShipmentStatus, keepExistingData,
				() => ListHelper.GetWithDescription<CodeDescriptionPair>(houseBill.GetValue(CusSCAHouseSchema.CA_ShipmentStatus), GetShipmentStatuses()));
			data.MessageStatus = PopulateValue(data.MessageStatus, keepExistingData,
				() => ListHelper.GetWithDescription<CodeDescriptionPair>(houseBill.GetValue(CusSCAHouseSchema.CA_MessageStatus), GetMessageStatuses()));
			data.GoodsOrigin = PopulateValue(data.GoodsOrigin, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(houseBill.CA_RN_NKGoodsOrigin, houseBill.Lookups.GoodsOrigins));
			data.PortOfOrigin = PopulateValue(data.PortOfOrigin, keepExistingData,
				() => ListHelper.GetWithName(houseBill.GetValue(CusSCAHouseSchema.CA_RL_NK_PortOfOrigin), Ports));
			data.PortOfDestination = PopulateValue(data.PortOfDestination, keepExistingData,
				() => ListHelper.GetWithName(houseBill.GetValue(CusSCAHouseSchema.CA_RL_NK_PortOfDestination), Ports));
			data.PortOfLoading = PopulateValue(data.PortOfLoading, keepExistingData,
				() => ListHelper.GetWithName(houseBill.GetValue(CusSCAHouseSchema.CA_RL_NKLoadPort), Ports));
			data.PortOfDischarge = PopulateValue(data.PortOfDischarge, keepExistingData,
				() => ListHelper.GetWithName(houseBill.GetValue(CusSCAHouseSchema.CA_RL_NKDischargePort), Ports));
			data.GoodsValue = PopulateValue(data.GoodsValue, keepExistingData,
				() => houseBill.GetValue(CusSCAHouseSchema.CA_GoodsValue));
			data.GoodsValueCurrency = PopulateValue(data.GoodsValueCurrency, keepExistingData,
				() => ListHelper.GetWithDescription<Currency>(houseBill.CA_RX_NKGoodsCurrency, houseBill.Lookups.GoodsCurrencies));
			data.SetOrganizationAddressCollection(() => data.OrganizationAddressCollection.MergeCollection(
				GetOrganizationsAddresses(houseBill), keepExistingData, UniversalDataObjectWriterHelper.IsOrganizationAddressTypeMatched));
			data.VendorIdentifier = PopulateValue(data.VendorIdentifier, keepExistingData,
				() => houseBill.GetValue(CusSCAHouseSchema.CA_VendorIdentifier));

			PopulatePackages(houseBill, data, keepExistingData);
			PopulateCountrySpecificDetails(houseBill, data, keepExistingData);
			PopulateAddInfo(houseBill, data);
		}

		protected virtual void PopulateAddInfo(TCusSCAHouse houseBill, Shipment data)
		{
			var isGstPrePaid = houseBill.GetValue(CusSCAHouseSchema.CA_IsGSTPrePaid);
			if (!isGstPrePaid.IsEmpty)
			{
				data.SetAddInfoCollection(() => new List<AddInfo>() { new AddInfo() { Key = "IsGSTPrePaid", Value = houseBill.GetValue(CusSCAHouseSchema.CA_IsGSTPrePaid) } });
			}
		}

		protected RefUNLOCOCollection Ports
		{
			get { return fPorts ?? (fPorts = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection fPorts;

		protected BusinessObjectFactory Factory
		{
			get { return writeManager.Action.FactoryForProcessing; }
		}

		protected WayBillType GetWayBillType(string code)
		{
			return ListHelper.GetWithDescription<WayBillType>(code, Factory.GetCachedValue<WayBillTypeList>());
		}

		#endregion // Overrides

		#region Implementation

		protected abstract ICodeDescriptionPairList GetShipmentStatuses();

		protected abstract ICodeDescriptionPairList GetMessageStatuses();

		protected virtual IEnumerable<OrganizationAddress> GetOrganizationsAddresses(IColumnIndexer houseBill)
		{
			yield return GetConsignorAddress(houseBill);
			yield return GetConsigneeAddress(houseBill);
			yield return GetNotifyAddress(houseBill);

			var goodsLocation = GetGoodsLocationAddress(houseBill);
			if (goodsLocation != null)
			{
				yield return goodsLocation;
			}
		}

		protected virtual OrganizationAddress GetConsignorAddress(IColumnIndexer houseBill)
		{
			var addressType = GetConsignorAddressType(houseBill);
			var consignor = Factory.Load<OrgHeader>(houseBill.GetValue(CusSCAHouseSchema.CA_OH_Consignor));
			var consignorAddress = consignor != null ? new PortBasedOrgAddressDecider(consignor, ((TCusSCAHouse)houseBill).ConsignorAddressType, () => { return houseBill.GetValue(CusSCAHouseSchema.CA_RL_NK_PortOfOrigin); }).Address : null;

			var orgWriter = new OrganizationDataObjectWriter(writeManager, addressType.ToString(), null);
			return
				orgWriter.GetDataObject(consignorAddress) ??
				OrganizationDataObjectWriter.GetDataObject(Factory, writeManager, houseBill, addressType.ToString(),
					CusSCAHouseSchema.CA_ConsignorName,
					CusSCAHouseSchema.CA_ConsignorAddress1,
					CusSCAHouseSchema.CA_ConsignorAddress2,
					CusSCAHouseSchema.CA_ConsignorSuburb,
					CusSCAHouseSchema.CA_ConsignorState,
					CusSCAHouseSchema.CA_ConsignorPostcode,
					CusSCAHouseSchema.CA_RN_NKConsignorCountryCode,
					CusSCAHouseSchema.CA_ConsignorPhone,
					CusSCAHouseSchema.CA_ConsignorFax,
					CusSCAHouseSchema.CA_ConsignorContactName);
		}

		protected virtual DocAddressType GetConsignorAddressType(IColumnIndexer houseBill)
		{
			return DocAddressType.ConsignorDocumentaryAddress;
		}

		protected virtual OrganizationAddress GetConsigneeAddress(IColumnIndexer houseBill)
		{
			var addressType = GetConsigneeAddressType(houseBill);
			var consignee = Factory.Load<OrgHeader>(houseBill.GetValue(CusSCAHouseSchema.CA_OH_Consignee));
			var consigneeAddress = consignee != null ? new PortBasedOrgAddressDecider(consignee, ((TCusSCAHouse)houseBill).ConsigneeAddressType, () => { return houseBill.GetValue(CusSCAHouseSchema.CA_RL_NK_PortOfDestination); }).Address : null;

			var orgWriter = new OrganizationDataObjectWriter(writeManager, addressType.ToString(), null);
			return
				orgWriter.GetDataObject(consigneeAddress) ??
				OrganizationDataObjectWriter.GetDataObject(Factory, writeManager, houseBill, addressType.ToString(),
					CusSCAHouseSchema.CA_ConsigneeName,
					CusSCAHouseSchema.CA_ConsigneeAddress1,
					CusSCAHouseSchema.CA_ConsigneeAddress2,
					CusSCAHouseSchema.CA_ConsigneeSuburb,
					CusSCAHouseSchema.CA_ConsigneeState,
					CusSCAHouseSchema.CA_ConsigneePostcode,
					CusSCAHouseSchema.CA_RN_NKConsigneeCountryCode,
					CusSCAHouseSchema.CA_ConsigneePhone,
					CusSCAHouseSchema.CA_ConsigneeFax,
					CusSCAHouseSchema.CA_ConsigneeContactName);
		}

		protected virtual DocAddressType GetConsigneeAddressType(IColumnIndexer houseBill)
		{
			return DocAddressType.ConsigneeDocumentaryAddress;
		}

		OrganizationAddress GetNotifyAddress(IColumnIndexer houseBill)
		{
			var addressType = DocAddressType.NotifyParty;
			var notifyParty = Factory.Load<OrgHeader>(houseBill.GetValue(CusSCAHouseSchema.CA_OH_Notify));
			var notifyPartyAddress = notifyParty != null ? new OrgAddressDecider(notifyParty, CargoAddressType.Notify).Address : null;

			var orgWriter = new OrganizationDataObjectWriter(writeManager, addressType.ToString(), null);
			return
				orgWriter.GetDataObject(notifyPartyAddress) ??
				OrganizationDataObjectWriter.GetDataObject(Factory, writeManager, houseBill, addressType.ToString(),
					CusSCAHouseSchema.CA_NotifyName,
					CusSCAHouseSchema.CA_NotifyAddress1,
					CusSCAHouseSchema.CA_NotifyAddress2,
					CusSCAHouseSchema.CA_NotifySuburb,
					CusSCAHouseSchema.CA_NotifyState,
					CusSCAHouseSchema.CA_NotifyPostcode,
					CusSCAHouseSchema.CA_RN_NKNotifyCountryCode,
					CusSCAHouseSchema.CA_NotifyPhone,
					CusSCAHouseSchema.CA_NotifyFax,
					CusSCAHouseSchema.CA_NotifyContactName);
		}

		OrganizationAddress GetGoodsLocationAddress(IColumnIndexer houseBill)
		{
			var goodsLocationOrgAddress = Factory.Load<OrgAddress>(houseBill.GetValue(CusSCAHouseSchema.CA_OA_GoodsLocation));
			return goodsLocationOrgAddress != null ? new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.GoodsLocation)).GetDataObject(goodsLocationOrgAddress) : null;
		}

		void PopulatePackages(IColumnIndexer houseBill, Shipment data, bool keepExistingData)
		{
			if (!keepExistingData)
			{
				data.SetPackingLineCollection(() => ProcessCollection(CusSCADataObjectHelper.LoadPackages<TCusSCAPivot>(houseBill, Factory), GetNewCusSCAPivotDataObjectWriter(), CollectionContent.Complete));
			}
		}
		protected abstract CusSCAPivotDataObjectWriter<TCusSCAPivot> GetNewCusSCAPivotDataObjectWriter();

		protected virtual string GetWayBillTypeCode(IColumnIndexer houseBill)
		{
			return WayBillTypeList.Codes.House;
		}

		protected virtual void PopulateCountrySpecificDetails(IColumnIndexer houseBill, Shipment data, bool keepExistingData)
		{
		}

		#endregion // Implementation
	}
}

// Tested in CusSCAOceanBillDataObjectWriter.cs
