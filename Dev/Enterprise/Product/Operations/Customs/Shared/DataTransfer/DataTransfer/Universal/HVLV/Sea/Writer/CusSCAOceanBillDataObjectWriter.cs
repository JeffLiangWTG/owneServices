using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	internal class CusSCAOceanBillDataObjectWriter : CusSCAOceanBillDataObjectWriter<BaseCusSCAOceanBill, BaseCusSCAHouse, BaseCusSCAContainer, BaseCusSCAPivot>
	{
		public CusSCAOceanBillDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override CusSCAHouseDataObjectWriter<BaseCusSCAHouse, BaseCusSCAPivot> GetNewCusSCAHouseDataObjectWriter()
		{
			return new CusSCAHouseDataObjectWriter(writeManager);
		}

		protected override CusSCAContainerDataObjectWriter<BaseCusSCAContainer> GetNewCusSCAContainerDataObjectWriter()
		{
			return new CusSCAContainerDataObjectWriter(writeManager);
		}
	}

	public abstract class CusSCAOceanBillDataObjectWriter<TCusSCAOceanBill, TCusSCAHouse, TCusSCAContainer, TCusSCAPivot> : TopLevelDataObjectWriter<TCusSCAOceanBill, Shipment>,
		IMergeDataObjectWriter,
		IHierarchicalDataObjectWriter
		where TCusSCAOceanBill : BaseCusSCAOceanBill
		where TCusSCAHouse : BaseCusSCAHouse
		where TCusSCAContainer : BaseCusSCAContainer
		where TCusSCAPivot : BaseCusSCAPivot
	{
		protected CusSCAOceanBillDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
			IncludeChildren = IncludeParent = true;
		}

		#region Overrides

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected sealed override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.SeaOceanBill;
		}

		protected sealed override void PopulateDataObject(TCusSCAOceanBill oceanBill, Shipment data)
		{
			PopulateDataObjectCore(oceanBill, data, false);
		}

		void PopulateDataObjectCore(TCusSCAOceanBill oceanBill, Shipment data, bool isMerging)
		{
			var keepExistingData = isMerging && ShouldKeepExistingData(oceanBill);
			data.TransportMode = PopulateValue(data.TransportMode, keepExistingData,
				() => ListHelper.GetWithDescription<CodeDescriptionPair>(Core.Constants.TransportModes.Sea, Factory.GetCachedCodeDescriptionPairList(Enterprise.ZArchitecture.Core.OLookUpEditType.TransportType)));
			data.WayBillType = PopulateValue(data.WayBillType, keepExistingData,
				() => GetWayBillType(WayBillTypeList.Codes.Master));
			data.WayBillNumber = PopulateValue(data.WayBillNumber, keepExistingData,
				() => oceanBill.GetValue(CusSCAOceanBillSchema.CB_OceanBill));
			data.VesselName = PopulateValue(data.VesselName, keepExistingData,
				() => oceanBill.GetValue(CusSCAOceanBillSchema.CB_VesselName));
			data.VoyageFlightNo = PopulateValue(data.VoyageFlightNo, keepExistingData,
				() => oceanBill.GetValue(CusSCAOceanBillSchema.CB_Voyage));
			data.LloydsIMO = PopulateValue(data.LloydsIMO, keepExistingData,
				() => oceanBill.GetValue(CusSCAOceanBillSchema.CB_LloydsIMO));
			data.Branch = PopulateValue(data.Branch, keepExistingData,
				() => Branch.New(oceanBill.Branch));

			data.PortOfLoading = PopulateValue(data.PortOfLoading, keepExistingData,
				() => ListHelper.GetWithName(oceanBill.GetValue(CusSCAOceanBillSchema.CB_RL_NKPortOfLoading), Ports));
			data.PortOfDischarge = PopulateValue(data.PortOfDischarge, keepExistingData,
				() => ListHelper.GetWithName(oceanBill.GetValue(CusSCAOceanBillSchema.CB_RL_NKPortOfDischarge), Ports));
			data.PortOfFirstArrival = PopulateValue(data.PortOfFirstArrival, keepExistingData,
				() => ListHelper.GetWithName(oceanBill.GetValue(CusSCAOceanBillSchema.CB_RL_NKPortOfFirstArrival), Ports));

			PopulateAdditionalReferences(oceanBill, data, keepExistingData);
			PopulateDates(oceanBill, data, keepExistingData);
			PopulateMasterHouse(oceanBill, data, keepExistingData);
			PopulateNotes(oceanBill, data, keepExistingData);
			PopulateOrganizationAddresses(oceanBill, data, keepExistingData);
			PopulateContainers(oceanBill, data, keepExistingData);
			PopulateSubshipments(oceanBill, data, keepExistingData);
		}

		RefUNLOCOCollection Ports
		{
			get { return fPorts ?? (fPorts = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection fPorts;

		#endregion // Overrides

		#region IHierarchicalDataObjectWriter

		public bool IncludeParent { get; set; }
		public bool IncludeChildren { get; set; }

		#endregion

		#region Implementation

		protected abstract CusSCAHouseDataObjectWriter<TCusSCAHouse, TCusSCAPivot> GetNewCusSCAHouseDataObjectWriter();
		protected abstract CusSCAContainerDataObjectWriter<TCusSCAContainer> GetNewCusSCAContainerDataObjectWriter();

		static void PopulateAdditionalReferences(IColumnIndexer oceanBill, Shipment data, bool keepExistingData)
		{
			data.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new List<AdditionalReference>();
				var responsiblePartyID = oceanBill.GetValue(CusSCAOceanBillSchema.CB_ResponsiblePartyID);
				if (!responsiblePartyID.IsEmpty)
				{
					additionalReferences.Add(new AdditionalReference()
					{
						ReferenceNumber = responsiblePartyID,
						Type = new EntryType()
						{
							Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							Description = Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
						}
					});
				}
				var principalID = oceanBill.GetValue(CusSCAOceanBillSchema.CB_PrincipalID);
				if (!principalID.IsEmpty)
				{
					additionalReferences.Add(new AdditionalReference()
					{
						ReferenceNumber = principalID,
						Type = new EntryType()
						{
							Code = Constants.AdditionalReference.EntryType.Codes.PrincipalID,
							Description = Constants.AdditionalReference.EntryType.Descriptions.PrincipalID
						}
					});
				}

				return data.AdditionalReferenceCollection.MergeCollectionByCandidateKey(additionalReferences, keepExistingData);
			});
		}

		static void PopulateDates(IColumnIndexer oceanBill, Shipment data, bool keepExistingData)
		{
			data.SetDateCollection(() =>
			{
				var list = new List<Date>();
				list.Add(DateType.Departure, ZBool.False, oceanBill.GetValue(CusSCAOceanBillSchema.CB_DateOfDeparture));
				list.Add(DateType.FirstArrivalInCountry, ZBool.False, oceanBill.GetValue(CusSCAOceanBillSchema.CB_DateOfFirstArrival));
				list.Add(DateType.DischargeDate, ZBool.False, oceanBill.GetValue(CusSCAOceanBillSchema.CB_DateOfArrival));
				return data.DateCollection.MergeCollectionByCandidateKey(list, keepExistingData);
			});
		}

		void PopulateMasterHouse(IColumnIndexer oceanBill, Shipment data, bool keepExistingData)
		{
			var masterHouseBillNumber = oceanBill.GetValue(CusSCAOceanBillSchema.CB_MasterHouseBill);
			if (!masterHouseBillNumber.IsEmpty)
			{
				data.SetAdditionalBillCollection(() =>
				{
					var masterHouseBill = new AdditionalBill(writeManager.WriterStrategy)
					{
						BillNumber = masterHouseBillNumber,
						BillType = GetWayBillType(WayBillTypeList.Codes.MasterHouse),
						ParentBillNumber = oceanBill.GetValue(CusSCAOceanBillSchema.CB_OceanBill)
					};
					return data.AdditionalBillCollection.MergeCollection(
						new[] { masterHouseBill }, keepExistingData, UniversalCommonHelper.IsBillMatched);
				});
			}
		}

		protected virtual bool ShouldKeepExistingData(TCusSCAOceanBill oceanBill)
		{
			return true;
		}

		BusinessObjectFactory Factory
		{
			get { return writeManager.Action.FactoryForProcessing; }
		}

		WayBillType GetWayBillType(string code)
		{
			return ListHelper.GetWithDescription<WayBillType>(code, Factory.GetCachedValue<WayBillTypeList>());
		}

		void PopulateNotes(IColumnIndexer oceanBill, Shipment data, bool keepExistingData)
		{
			if (oceanBill is IStmNoteParent noteParent)
			{
				var oceanBillNotes = new StmNoteCollection(noteParent, noteParent.NotesFactory);
				oceanBillNotes.Load();
				if (oceanBillNotes.Any())
				{
					oceanBillNotes.Sort(StmNote.Schema.ST_Description);
					var noteDataCollection = ProcessCollection(oceanBillNotes, new NoteDataObjectWriter(writeManager));
					data.SetNoteCollection(() => data.NoteCollection.MergeCollectionByCandidateKey(noteDataCollection, keepExistingData));
				}
			}
		}

		void PopulateOrganizationAddresses(IColumnIndexer oceanBill, Shipment data, bool keepExistingData)
		{
			var organizationAddresses = new List<OrganizationAddress>();

			var shippingLineOrgHeader = Factory.Load<OrgHeader>(oceanBill.GetValue(CusSCAOceanBillSchema.CB_OH_ShippingLine));
			if (shippingLineOrgHeader != null)
			{
				var shippingLineOrganizationAddress = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ShippingLineAddress)).GetDataObject(shippingLineOrgHeader.MainAddress);
				organizationAddresses.Add(shippingLineOrganizationAddress);
			}

			var goodsLocationOrgAddress = Factory.Load<OrgAddress>(oceanBill.GetValue(CusSCAOceanBillSchema.CB_OA_GoodsLocation));
			if (goodsLocationOrgAddress != null)
			{
				var goodsLocationOrganizationAddress = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.GoodsLocation)).GetDataObject(goodsLocationOrgAddress);
				organizationAddresses.Add(goodsLocationOrganizationAddress);
			}

			if (organizationAddresses.Any())
			{
				data.SetOrganizationAddressCollection(() => data.OrganizationAddressCollection.MergeCollection(organizationAddresses, keepExistingData, UniversalDataObjectReaderHelper.IsOrganizationAddressTypeMatched));
			}
		}

		void PopulateSubshipments(IColumnIndexer oceanBill, Shipment shipment, bool keepExistingData)
		{
			if (!keepExistingData && IncludeChildren)
			{
				var houseBills = CusSCADataObjectHelper.LoadHouseBills<TCusSCAHouse>(oceanBill, Factory);
				var data = ProcessCollection(houseBills, GetNewCusSCAHouseDataObjectWriter());
				shipment.SetSubShipmentCollection(() => data != null ? new DataObjectList<Shipment>(data) : null);
			}
		}

		void PopulateContainers(IColumnIndexer oceanBill, Shipment data, bool keepExistingData)
		{
			if (!keepExistingData)
			{
				data.SetContainerCollection(() => ProcessCollection(CusSCADataObjectHelper.LoadContainers<TCusSCAContainer>(oceanBill, Factory), GetNewCusSCAContainerDataObjectWriter(), CollectionContent.Complete));
			}
		}
		#endregion // Implementation

		#region IMergeDataObjectWriter

		void IMergeDataObjectWriter.MergeData(IDataObject dataObject, BusinessObject mergingBO)
		{
			var shipmentData = dataObject as Shipment;
			var cusSCA = mergingBO as TCusSCAOceanBill;
			if (shipmentData != null && cusSCA != null)
			{
				var dataContext = shipmentData.DataContext;
				if (dataContext != null && !HasRecipientRole(RecipientRoleType.HSA))
				{
					dataContext.AddDataSource(GetTopLevelDataContextType(), ZString.Empty);
				}
				PopulateDataObjectCore(cusSCA, shipmentData, true);
			}
		}

		#endregion // IMergeDataObjectWriter
	}
}
