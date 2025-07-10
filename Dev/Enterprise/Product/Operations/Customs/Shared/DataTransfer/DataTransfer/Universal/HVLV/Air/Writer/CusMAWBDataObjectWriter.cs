using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class CusMAWBDataObjectWriter : CusMAWBDataObjectWriter<CusMAWB, CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusMAWBDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper mawbHelper)
		{
			return new CusHAWBDataObjectWriter(writeManager, mawbHelper);
		}

		protected override AirManifestDataObjectWriterHelper CreateNewHVLVAirDataObjectWriterHelper(CusMAWB mawbBO)
		{
			return new AirManifestDataObjectWriterHelper(mawbBO);
		}
	}

	public abstract class CusMAWBDataObjectWriter<TMAWB, THAWB, THelper> : TopLevelDataObjectWriter<TMAWB, Shipment>, IMergeDataObjectWriter, IHierarchicalDataObjectWriter
		where TMAWB : CusMAWB
		where THAWB : CusHAWB
		where THelper : AirManifestDataObjectWriterHelper
	{
		protected CusMAWBDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
			IncludeChildren = IncludeParent = true;
		}

		protected virtual bool ShouldKeepExistingData(TMAWB mawbBO)
		{
			return true;
		}

		protected sealed override void PopulateDataObject(TMAWB mawbBO, Shipment mawbData)
		{
			PopulateDataObjectCore(mawbBO, mawbData, false);
		}

		void PopulateDataObjectCore(TMAWB mawbBO, Shipment mawbData, bool checkKeepExistingData)
		{
			var keepExistingData = checkKeepExistingData && ShouldKeepExistingData(mawbBO);
			var mawbHelper = CreateNewHVLVAirDataObjectWriterHelper(mawbBO);
			var wayBillNumber = mawbData.WayBillNumber.GetValueOrDefault();
			mawbData.WayBillNumber = PopulateValue(mawbData.WayBillNumber, keepExistingData, () => mawbBO.CM_MAWB);
			if (!keepExistingData || wayBillNumber.IsEmpty)
			{
				mawbData.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			}
			mawbData.TransportMode = PopulateValue(mawbData.TransportMode, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(Core.Constants.TransportModes.Air, mawbHelper.ForwardingTransportTypeList));
			mawbData.VoyageFlightNo = PopulateValue(mawbData.VoyageFlightNo, keepExistingData, () => mawbBO.CM_FlightNo);
			mawbData.Folio = PopulateValue(mawbData.Folio, keepExistingData, () => mawbBO.CM_Folio);

			var refUNLOCOList = mawbBO.Factory.GetRefUNLOCOList();
			mawbData.PortOfLoading = PopulateValue(mawbData.PortOfLoading, keepExistingData, () => ListHelper.GetWithName(mawbBO.CM_RL_NKLoadPort, refUNLOCOList));
			if (SupportFirstArrival)
			{
				mawbData.PortOfFirstArrival = PopulateValue(mawbData.PortOfFirstArrival, keepExistingData, () => ListHelper.GetWithName(mawbBO.CM_RL_NKFirstArrivalPort, refUNLOCOList));
			}
			mawbData.PortOfDischarge = PopulateValue(mawbData.PortOfDischarge, keepExistingData, () => ListHelper.GetWithName(mawbBO.CM_RL_NKDischargePort, refUNLOCOList));
			mawbData.Branch = PopulateValue(mawbData.Branch, keepExistingData, () => Branch.New(mawbBO.Branch));
			if (mawbBO.CM_JK.IsEmpty)
			{
				PopulateNotes(mawbBO, mawbData, mawbHelper);
			}
			PopulateDates(mawbBO, mawbData, mawbHelper, keepExistingData);
			PopulateResponsiblePartyDetails(mawbBO, mawbData, mawbHelper, keepExistingData);
			PopulateCountrySpecificData(mawbBO, mawbData, mawbHelper, keepExistingData);

			if (IncludeChildren && !keepExistingData)
			{
				mawbData.SetSubShipmentCollection(() => PopulateSubShipmentCollection(mawbBO, mawbHelper));
			}

			var depotAddress = mawbBO.UnpackDepotAddress;
			if (depotAddress != null)
			{
				var depotAddressData = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ArrivalCFSAddress)).GetDataObject(depotAddress);
				mawbData.SetOrganizationAddressCollection(() => mawbData.OrganizationAddressCollection.MergeCollection(new[] { depotAddressData }, keepExistingData, UniversalDataObjectWriterHelper.IsOrganizationAddressTypeMatched));
			}
		}

		protected virtual bool SupportFirstArrival
		{
			get { return true; }
		}

		protected virtual void PopulateResponsiblePartyDetails(TMAWB mawbBO, Shipment mawbData, THelper mawbHelper, bool keepExistingData)
		{
			if (mawbBO.ResponsibleParty != null)
			{
				var responsibleParty = new OrganizationDataObjectWriter(writeManager, ResponsiblePartyAddressType).GetDataObject(mawbBO.ResponsibleParty.MainAddress);
				mawbData.SetOrganizationAddressCollection(() => mawbData.OrganizationAddressCollection.MergeCollection(new[] { responsibleParty }, keepExistingData, UniversalDataObjectWriterHelper.IsOrganizationAddressTypeMatched));
			}

			var responsiblePartyID = mawbBO.CM_ResponsiblePartyID;
			if (!responsiblePartyID.IsEmpty)
			{
				mawbData.SetAdditionalReferenceCollection(() =>
				{
					var additionalReference = new AdditionalReference()
					{
						Type = new EntryType()
						{
							Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							Description = Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
						},
						ReferenceNumber = responsiblePartyID
					};
					return mawbData.AdditionalReferenceCollection.MergeCollectionByCandidateKey(new[] { additionalReference }, keepExistingData);
				});
			}
		}

		protected virtual string ResponsiblePartyAddressType
		{
			get { return AddressTypes.ResponsibleParty; }
		}

		protected virtual DataObjectList<Shipment> PopulateSubShipmentCollection(TMAWB mawbBO, THelper mawbHelper)
		{
			var query = new ZQuery(CusHAWBSchema.CS_CS_MasterHouseBill, null);
			query.AddToFilter(mawbBO.ChildBills.CompleteFilter);
			var hawbs = mawbHelper.Load<THAWB>(query);

			var hawbWritter = GetNewCusHAWBDataObjectWriter(mawbHelper);
			hawbWritter.IncludeParent = false;

			var data = ProcessCollection(hawbs, hawbWritter);
			return data != null ? new DataObjectList<Shipment>(data) : null;
		}

		protected abstract CusHAWBDataObjectWriter<THAWB, THelper> GetNewCusHAWBDataObjectWriter(THelper mawbHelper);

		protected virtual void PopulateNotes(TMAWB mawbBO, Shipment mawbData, THelper mawbHelper)
		{
			var notes = mawbBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			mawbData.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		protected virtual void PopulateDates(TMAWB mawbBO, Shipment mawbData, THelper mawbHelper, bool keepExistingData)
		{
			var list = new List<Date>();
			list.Add(DateType.LoadingDate, ZBool.False, mawbBO.CM_DepartureDate);
			if (SupportFirstArrival)
			{
				list.Add(DateType.FirstArrivalInCountry, ZBool.False, mawbBO.CM_DateOfFirstArrival);
			}
			list.Add(DateType.DischargeDate, ZBool.False, mawbBO.CM_ArrivalDate);
			mawbData.SetDateCollection(() => mawbData.DateCollection.MergeCollectionByCandidateKey(list, keepExistingData));
		}

		protected abstract THelper CreateNewHVLVAirDataObjectWriterHelper(TMAWB mawbBO);

		protected virtual void PopulateCountrySpecificData(TMAWB mawbBO, Shipment mawbData, THelper mawbHelper, bool keepExistingData)
		{
			var masterHouseBill = mawbBO.CM_MasterHouseBill;
			if (!masterHouseBill.IsEmpty && mawbData.SetAdditionalBillCollection(() => mawbData.AdditionalBillCollection ?? new List<AdditionalBill>()))
			{
				if (mawbData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.GetValueOrDefault() == masterHouseBill && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse && x.ParentBillNumber.GetValueOrDefault() == mawbBO.CM_MAWB) == null)
				{
					mawbData.AdditionalBillCollection.Add(new AdditionalBill(writeManager.WriterStrategy)
					{
						BillNumber = masterHouseBill,
						BillType = new WayBillType()
						{
							Code = WayBillTypeList.Codes.MasterHouse,
							Description = WayBillTypeList.Descriptions.MasterHouse
						},
						ParentBillNumber = mawbBO.CM_MAWB
					});
				}
			}
		}

		protected sealed override IEnumerable<IPropertyValue> GetUserDefinedValues(TMAWB sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		protected sealed override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected sealed override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AirManifest;
		}

		#region IMergeDataObjectWriter Members

		void IMergeDataObjectWriter.MergeData(IDataObject dataObject, BusinessObject bussinesBO)
		{
			var shipmentData = dataObject as Shipment;
			var mawbBO = bussinesBO as TMAWB;
			if (shipmentData != null && mawbBO != null)
			{
				var dataContext = shipmentData.DataContext;
				if (dataContext != null && !HasRecipientRole(RecipientRoleType.HCA))
				{
					dataContext.AddDataSource(DataContextType.AirManifest, ZString.Empty);
				}
				PopulateDataObjectCore(mawbBO, shipmentData, true);
			}
		}

		#endregion

		#region IHierarchicalDataObjectWriter

		public bool IncludeParent { get; set; }
		public bool IncludeChildren { get; set; }

		#endregion
	}
}
