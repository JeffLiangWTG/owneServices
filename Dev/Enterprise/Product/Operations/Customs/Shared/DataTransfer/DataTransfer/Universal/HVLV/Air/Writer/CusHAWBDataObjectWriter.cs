using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class CusHAWBDataObjectWriter : CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusHAWBDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override AirManifestDataObjectWriterHelper GetNewAirManifestLineDataObjectWriterHelper(CusHAWB hawbBO)
		{
			return helper == null ? new AirManifestDataObjectWriterHelper(hawbBO) : new AirManifestDataObjectWriterHelper(hawbBO, helper);
		}

		protected override CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper hawbHelper)
		{
			return new CusHAWBDataObjectWriter(writeManager, hawbHelper);
		}
	}

	public abstract class CusHAWBDataObjectWriter<THAWB, THelper> : TopLevelDataObjectWriter<THAWB, Shipment>, IMergeDataObjectWriter, IHierarchicalDataObjectWriter
		where THAWB : CusHAWB
		where THelper : AirManifestDataObjectWriterHelper
	{
		protected CusHAWBDataObjectWriter(IDataWritingManager manager, THelper helper)
			: base(manager)
		{
			this.helper = helper;
			IncludeChildren = IncludeParent = true;
		}

		protected readonly THelper helper;

		protected sealed override void PopulateDataObject(THAWB hawbBO, Shipment hawbData)
		{
			PopulateDataObjectCore(hawbBO, hawbData, false);
		}

		protected virtual bool ShouldKeepExistingData(THAWB hawbBO)
		{
			return true;
		}

		protected void PopulateDataObjectCore(THAWB hawbBO, Shipment hawbData, bool checkKeepExistingData)
		{
			var keepExistingData = checkKeepExistingData && ShouldKeepExistingData(hawbBO);
			parents = FindParents(hawbBO);
			var hawbHelper = GetNewAirManifestLineDataObjectWriterHelper(hawbBO);
			PopulateWayBillDetails(hawbData, hawbBO, hawbHelper, keepExistingData);

			var refUNLOCOList = hawbBO.Factory.GetRefUNLOCOList();
			hawbData.PortOfOrigin = PopulateValue(hawbData.PortOfOrigin, keepExistingData, () => ListHelper.GetWithName(hawbBO.CS_RL_NKOrigin, refUNLOCOList));
			hawbData.PortOfDestination = PopulateValue(hawbData.PortOfDestination, keepExistingData, () => ListHelper.GetWithName(hawbBO.CS_RL_NKDestination, refUNLOCOList));
			hawbData.TotalWeight = PopulateValue(hawbData.TotalWeight, keepExistingData, () => hawbBO.CS_Weight);
			hawbData.TotalWeightUnit = PopulateValue(hawbData.TotalWeightUnit, keepExistingData, () => ListHelper.GetWithDescription<UnitOfWeight>(hawbBO.CS_WeightUQ, hawbBO.Lookups.UnitOfWeightList));
			hawbData.TotalNoOfPieces = PopulateValue(hawbData.TotalNoOfPieces, keepExistingData, () => hawbBO.CS_PiecesManifested);
			hawbData.GoodsDescription = PopulateValue(hawbData.GoodsDescription, keepExistingData, () => hawbBO.CS_GoodsDescription);
			hawbData.GoodsValue = PopulateValue(hawbData.GoodsValue, keepExistingData, () => hawbBO.CS_GoodsValue);
			hawbData.GoodsValueCurrency = PopulateValue(hawbData.GoodsValueCurrency, keepExistingData, () => ListHelper.GetWithDescription<Currency>(hawbBO.CS_RX_NKGoodsCurrency, hawbBO.Lookups.GoodsCurrencies));
			PopulateResponsiblePartyDetails(hawbData, hawbBO, keepExistingData);
			hawbData.VendorIdentifier = PopulateValue(hawbData.VendorIdentifier, keepExistingData, () => hawbBO.CS_VendorIdentifier);
			PopulateConsignor(hawbData, hawbBO, hawbHelper, keepExistingData);
			PopulateConsignee(hawbData, hawbBO, hawbHelper, keepExistingData);
			PopulateNotifyParty(hawbData, hawbBO, hawbHelper, keepExistingData);
			PopulateCountrySpecificDetails(hawbData, hawbBO, hawbHelper, keepExistingData);
			if (IncludeChildren && !keepExistingData)
			{
				PopulateColoadBills(hawbData, hawbBO, hawbHelper);
			}
		}

		protected virtual void PopulateConsignee(Shipment hawbData, THAWB hawbBO, THelper hawbHelper, bool keepExistingData)
		{
			var isMasterHouse = hawbBO.CS_IsMasterHouse;
			var addressType = isMasterHouse ? nameof(DocAddressType.ReceivingForwarderAddress) : nameof(DocAddressType.ConsigneeDocumentaryAddress);
			OrganizationAddress consigneeAddress = null;
			var organisation = hawbBO.Consignee;
			if (organisation != null)
			{
				consigneeAddress = new OrganizationDataObjectWriter(writeManager, addressType).GetDataObject(organisation.MainAddress);
			}
			else
			{
				consigneeAddress = CreateOrgAddress(hawbBO, hawbHelper, addressType, CusHAWBSchema.CS_ConsigneeName, CusHAWBSchema.CS_ConsigneeStreet, CusHAWBSchema.CS_ConsigneeStreet2, CusHAWBSchema.CS_ConsigneeCity, CusHAWBSchema.CS_ConsigneeState, CusHAWBSchema.CS_ConsigneePostcode, CusHAWBSchema.CS_RN_NKConsigneeCountry, CusHAWBSchema.CS_ConsigneePhone, CusHAWBSchema.CS_ConsigneeContactName, keepExistingData);
			}
			hawbData.SetOrganizationAddressCollection(() => hawbData.OrganizationAddressCollection.MergeCollection(new[] { consigneeAddress }, keepExistingData, UniversalCommonHelper.IsOrganizationAddressTypeMatched));
		}

		protected virtual void PopulateConsignor(Shipment hawbData, THAWB hawbBO, THelper hawbHelper, bool keepExistingData)
		{
			var isMasterHouse = hawbBO.CS_IsMasterHouse;
			var addressType = isMasterHouse ? nameof(DocAddressType.SendingForwarderAddress) : nameof(DocAddressType.ConsignorDocumentaryAddress);
			var organisation = hawbBO.Consignor;

			OrganizationAddress consignorAddress = null;
			if (organisation != null)
			{
				consignorAddress = new OrganizationDataObjectWriter(writeManager, addressType).GetDataObject(organisation.MainAddress);
			}
			else
			{
				consignorAddress = CreateOrgAddress(hawbBO, hawbHelper, addressType, CusHAWBSchema.CS_ConsignorName, CusHAWBSchema.CS_ConsignorStreet, CusHAWBSchema.CS_ConsignorStreet2, CusHAWBSchema.CS_ConsignorCity, CusHAWBSchema.CS_ConsignorState, CusHAWBSchema.CS_ConsignorPostcode, CusHAWBSchema.CS_RN_NKConsignorCountry, CusHAWBSchema.CS_ConsignorPhone, CusHAWBSchema.CS_ConsignorContactName, keepExistingData);
			}
			hawbData.SetOrganizationAddressCollection(() => hawbData.OrganizationAddressCollection.MergeCollection(new[] { consignorAddress }, keepExistingData, UniversalCommonHelper.IsOrganizationAddressTypeMatched));
		}

		void PopulateNotifyParty(Shipment hawbData, THAWB hawbBO, THelper hawbHelper, bool keepExistingData)
		{
			var organisation = hawbBO.Notify;
			var addressType = nameof(DocAddressType.NotifyParty);
			var notifyPartyAddress = organisation != null
				? new OrganizationDataObjectWriter(writeManager, addressType).GetDataObject(organisation.MainAddress)
				: CreateOrgAddress(
					hawbBO,
					hawbHelper,
					addressType,
					CusHAWBSchema.CS_NotifyName,
					CusHAWBSchema.CS_NotifyAddress1,
					CusHAWBSchema.CS_NotifyAddress2,
					CusHAWBSchema.CS_NotifySuburb,
					CusHAWBSchema.CS_NotifyState,
					CusHAWBSchema.CS_NotifyPostcode,
					CusHAWBSchema.CS_RN_NKNotifyCountryCode,
					CusHAWBSchema.CS_NotifyPhone,
					CusHAWBSchema.CS_NotifyContactName,
					keepExistingData);

			hawbData.SetOrganizationAddressCollection(() => hawbData.OrganizationAddressCollection.MergeCollection(new[] { notifyPartyAddress }, keepExistingData, UniversalCommonHelper.IsOrganizationAddressTypeMatched));
		}

		protected OrganizationAddress CreateOrgAddress(THAWB hawbBO, THelper hawbHelper, ZString addressType, SchemaStringColumn nameColumn, SchemaStringColumn address1Column, SchemaStringColumn address2Column, SchemaStringColumn cityColumn, SchemaStringColumn stateColumn, SchemaStringColumn postcodeColumn, SchemaStringColumn countryColumn, SchemaStringColumn phoneColumn, SchemaStringColumn contactNameColumn, bool keepExistingData)
		{
			return new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = addressType,
				CompanyName = hawbBO.GetValue(nameColumn),
				Address1 = hawbBO.GetValue(address1Column),
				Address2 = hawbBO.GetValue(address2Column),
				City = hawbBO.GetValue(cityColumn),
				State = hawbBO.GetValue(stateColumn),
				Postcode = hawbBO.GetValue(postcodeColumn),
				Country = Country.New(hawbHelper.LoadFromNaturalKey<RefCountry>(hawbBO, countryColumn, RefCountrySchema.RN_Code)),
				Contact = hawbBO.GetValue(contactNameColumn),
				Phone = hawbBO.GetValue(phoneColumn)
			};
		}

		void PopulateResponsiblePartyDetails(Shipment hawbData, THAWB hawbBO, bool keepExistingData)
		{
			var responsiblePartyID = hawbBO.CS_ResponsiblePartyID;
			if (!responsiblePartyID.IsEmpty)
			{
				hawbData.SetAdditionalReferenceCollection(() =>
				{
					var additionalReferenceCollection = hawbData.AdditionalReferenceCollection ?? new DataObjectList<AdditionalReference>();
					additionalReferenceCollection.Add(new AdditionalReference()
					{
						Type = new EntryType()
						{
							Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							Description = Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
						},
						ReferenceNumber = responsiblePartyID
					});
					return hawbData.AdditionalReferenceCollection.MergeCollectionByCandidateKey(additionalReferenceCollection, keepExistingData);
				});
			}
		}

		protected abstract THelper GetNewAirManifestLineDataObjectWriterHelper(THAWB hawbBO);

		protected virtual void PopulateColoadBills(Shipment hawbData, THAWB hawbBO, THelper hawbHelper)
		{
			var subHAWBWritter = GetNewCusHAWBDataObjectWriter(hawbHelper);
			subHAWBWritter.IncludeChildren = false;
			subHAWBWritter.IncludeParent = false;

			var data = ProcessCollection(hawbHelper.Load<THAWB>(new ZQuery(CusHAWBSchema.CS_CS_MasterHouseBill, hawbBO.PK)), subHAWBWritter);
			hawbData.SetSubShipmentCollection(() => data != null ? new DataObjectList<Shipment>(data) : null);
		}

		protected abstract CusHAWBDataObjectWriter<THAWB, THelper> GetNewCusHAWBDataObjectWriter(THelper hawbHelper);

		protected virtual void PopulateCountrySpecificDetails(Shipment hawbData, THAWB hawbBO, THelper hawbHelper, bool keepExistingData)
		{
		}

		protected virtual void PopulateWayBillDetails(Shipment hawbData, THAWB hawbBO, THelper hawbHelper, bool keepExistingData)
		{
			var existingWayBillNumber = hawbData.WayBillNumber;
			hawbData.WayBillNumber = PopulateValue(existingWayBillNumber, keepExistingData, () => hawbBO.CS_HAWB);
			var wayBillType = hawbBO.CS_IsMasterHouse ? WayBillTypeList.Codes.MasterHouse : WayBillTypeList.Codes.House;
			var masterHouseBillNumber = hawbBO.CS_MasterHouseBill;
			if (!masterHouseBillNumber.IsEmpty)
			{
				hawbData.SetAdditionalBillCollection(() =>
				{
					var additionalBillCollection = new List<AdditionalBill>();
					additionalBillCollection.Add(new AdditionalBill(writeManager.WriterStrategy)
					{
						BillNumber = hawbBO.CS_HAWB,
						BillType = ListHelper.GetWithDescription<WayBillType>(wayBillType, hawbHelper.WayBillTypeList),
						ParentBillNumber = masterHouseBillNumber
					});
					return hawbData.AdditionalBillCollection.MergeCollection(additionalBillCollection, keepExistingData, BillIsMatch);
				});
			}
			if (!keepExistingData || !existingWayBillNumber.HasValue)
			{
				hawbData.WayBillType = ListHelper.GetWithDescription<WayBillType>(wayBillType, hawbHelper.WayBillTypeList);
			}
		}

		bool BillIsMatch(AdditionalBill x, AdditionalBill y)
		{
			var xBillType = x.BillType;
			var yBillType = y.BillType;
			if (xBillType == null || yBillType == null || xBillType.GetCodeAsUpperCase() != yBillType.GetCodeAsUpperCase())
			{
				return false;
			}

			var xBillNumber = x.BillNumber;
			var yBillNumber = y.BillNumber;
			return xBillNumber.HasValue && yBillNumber.HasValue && xBillNumber.GetValueOrDefault() == yBillNumber.GetValueOrDefault();
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AirManifestLine;
		}

		protected override void InsertParents(THAWB sourceBO, ref Shipment hAWBData)
		{
			if (IncludeParent && parents != null)
			{
				var originalTopLevelHAWBData = hAWBData;

				foreach (var businessObject in parents)
				{
					var parentHAWB = businessObject as THAWB;
					if (parentHAWB != null)
					{
						hAWBData = InsertParent(parentHAWB, hAWBData);
					}
					else
					{
						var parentMAWB = businessObject as CusMAWB;
						if (parentMAWB != null)
						{
							hAWBData = InsertParent(parentMAWB, hAWBData);
						}
					}
				}

				if (hAWBData != originalTopLevelHAWBData)
				{
					hAWBData.DataContext.AddDataSource(DataContextType.AirManifestLine, ZString.Empty);
				}
			}
		}

		Shipment InsertParent(THAWB parentHAWBBO, Shipment hawbData)
		{
			var writer = GetNewCusHAWBDataObjectWriter(GetNewAirManifestLineDataObjectWriterHelper(parentHAWBBO));
			writer.IncludeChildren = false;
			writer.IncludeParent = false;

			var parentDataObject = writer.GetDataObject(parentHAWBBO);

			return GetTopLevelBOWithParentLinked(hawbData, parentDataObject);
		}

		Shipment InsertParent(CusMAWB parentMAWBBO, Shipment hawbData)
		{
			var writer = GetMAWBDataObjectWriter(parentMAWBBO);
			writer.IncludeChildren = false;

			var parentDataObject = writer.GetDataObject(parentMAWBBO);

			return GetTopLevelBOWithParentLinked(hawbData, parentDataObject);
		}

		protected virtual CusMAWBDataObjectWriter GetMAWBDataObjectWriter(CusMAWB parentMAWBBO)
		{
			return new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, parentMAWBBO)));
		}

		Shipment GetTopLevelBOWithParentLinked(Shipment hawbData, Shipment parentDataObject)
		{
			parentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { hawbData }));
			return parentDataObject;
		}

		BusinessObject[] FindParents(THAWB sourceBO)
		{
			var result = new List<BusinessObject>();

			var masterHouseBill = sourceBO.CS_CS_MasterHouseBill;
			if (masterHouseBill.IsValid)
			{
				var parentHAWBBO = sourceBO.Factory.Load<THAWB>(masterHouseBill);
				if (parentHAWBBO != null)
				{
					result.Add(parentHAWBBO);
					result.AddRange(FindParents(parentHAWBBO));
				}
			}
			else
			{
				var parentMAWBBO = sourceBO.MAWB;
				if (parentMAWBBO != null)
				{
					result.Add(parentMAWBBO);
				}
			}

			return result.ToArray();
		}

		BusinessObject[] parents;

		#region IMergeDataObjectWriter Members

		void IMergeDataObjectWriter.MergeData(IDataObject dataObject, BusinessObject bussinesBO)
		{
			var shipmentData = dataObject as Shipment;
			var hawbBO = bussinesBO as THAWB;
			if (shipmentData != null && hawbBO != null)
			{
				var dataContext = shipmentData.DataContext;
				if (dataContext != null)
				{
					dataContext.AddDataSource(DataContextType.AirManifestLine, ZString.Empty);
				}
				PopulateDataObjectCore(hawbBO, shipmentData, true);
			}
		}

		#endregion

		#region IHierarchicalDataObjectWriter

		public bool IncludeParent { get; set; }
		public bool IncludeChildren { get; set; }

		#endregion
	}
}
