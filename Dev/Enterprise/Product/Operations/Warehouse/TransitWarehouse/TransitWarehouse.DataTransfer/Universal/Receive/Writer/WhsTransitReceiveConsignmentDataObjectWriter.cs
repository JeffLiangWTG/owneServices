using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveConsignmentDataObjectWriter : TopLevelDataObjectWriter<WhsItemReceiveConsignment, UniversalShipment>
	{
		readonly WhsItemReceiveASN receiveASN;
		readonly Container container;
		readonly WhsItemReceiveTransportationUnit receiveTransportationUnit;
		Dictionary<ZGuid, ZInt> orderReferenceDictionary;

		public WhsTransitReceiveConsignmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		public WhsTransitReceiveConsignmentDataObjectWriter(IDataWritingManager manager, WhsItemReceiveTransportationUnit receiveTransportationUnit, Container container)
			: base(manager)
		{
			this.container = container;
			this.receiveTransportationUnit = receiveTransportationUnit;
		}

		public WhsTransitReceiveConsignmentDataObjectWriter(IDataWritingManager manager, WhsItemReceiveASN receiveASN)
			: base(manager)
		{
			this.receiveASN = receiveASN;
		}

		protected override void PopulateDataObject(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			dataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(consignment.WRC_RS_NKServiceLevel, consignment.Lookups.ServiceLevels);
			dataObject.PopulateTransportMode(consignment.WRC_TransportMode);
			PopulatePackages(consignment, dataObject);
			PopulateRelatedShipments(consignment, dataObject);
			PopulatePackageScreeningResults(consignment, dataObject);
			PopulateAddresses(consignment, dataObject);
			PopulateAdditionalServices(consignment, dataObject);
			PopulateNotes(consignment, dataObject);
			PopulateTransportRoutings(consignment, dataObject);
			PopulateReferences(consignment, dataObject);
			PopulateDestination(consignment, dataObject);
			PopulateDateCollection(consignment, dataObject);
			PopulateTarget(dataObject);
			PopulateValidationRuleCollection(dataObject);
			PopulateConsignmentOrderReferences(consignment, dataObject);
			PopulateForAirCargo(consignment, dataObject);
		}

		void PopulateForAirCargo(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			dataObject.WayBillNumber = consignment.HouseBillNumber;
			dataObject.WayBillType = new WayBillType()
			{
				Code = WayBillTypeList.Codes.House,
				Description = WayBillTypeList.Descriptions.House,
			};

			if (consignment.MasterBillNumber != string.Empty)
			{
				var additionalReference = new AdditionalReference()
				{
					Type = new EntryType()
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.MasterBill,
					},
					ReferenceNumber = consignment.MasterBillNumber
				};

				if (dataObject.AdditionalReferenceCollection == null)
				{
					dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
				}

				dataObject.AdditionalReferenceCollection.Add(additionalReference);
			}

			if (consignment.PackageStates.Count > 0)
			{
				var firstPackage = consignment.PackageStates.FirstOrDefault();
				dataObject.TotalNoOfPacksPackageType = new PackageType()
				{
					Code = firstPackage.Package.KP_F3_NKPackType,
					Description = Constants.PkgUnit.GetNonPluralDescription(firstPackage.Package.KP_F3_NKPackType)
				};

				dataObject.TotalWeight = consignment.PackageStates.Sum(p => Core.Constants.Weight.Convert(p.Package.KP_Weight, p.Package.KP_WeightUQ, Weight.Kilograms));
				dataObject.TotalWeightUnit = new UnitOfWeight()
				{
					Code = Weight.Kilograms,
					Description = Constants.Weight.GetDescription(Weight.Kilograms, PluralState.NonPlural)
				};
				dataObject.TotalNoOfPieces = consignment.PackageStates.ToList().Count;
			}
		}

		void PopulateDateCollection(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			dataObject.SetDateCollection(() => new List<Date>
			{
				Date.New(DateType.Arrival, false, consignment.WRC_ExpectedArrivalTime),
			});
		}

		void PopulateValidationRuleCollection(UniversalShipment dataObject)
		{
			dataObject.SetValidationRuleCollection(() => new List<UniversalDataBuss.DataObjects.ValidationRule>());
		}

		void PopulateTarget(UniversalShipment dataObject)
		{
			if (HasRecipientRole(RecipientRoleType.COA))
			{
				dataObject.DataContext.AddDataTarget(DataContextType.UnderBond, null);
			}
		}

		void PopulateConsignmentOrderReferences(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			if (consignment.OrderReferences.Count > 0)
			{
				dataObject.LocalProcessing.SetOrderNumberCollection(() => ProcessCollection(consignment.OrderReferences, new TransitConsignmentOrderReferenceDataObjectWriter(writeManager), CollectionContent.Partial));
			}
		}

		void PopulatePackages(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			PkgPackageJobDataObjectWriterHelper helper = null;
			var rcnPackageStates = consignment.PackageStates.Where(p => p.WPS_Status != TransitWarehouseStatuses.Codes.AdjustedOut);
			var innerPackagePks = rcnPackageStates.Where(ps => ps.WPS_UnitType == PackageStateUnitType.Codes.Overpack)
				.SelectMany(h => h.Package.HandlingUnitPackedPackages)
				.Select(i => i.PK);

			var packageStates = rcnPackageStates.Where(ps => !innerPackagePks.Contains(ps.WPS_KP_Package));
			var count = 0;
			orderReferenceDictionary = packageStates.Where(ps => ps.Package.PackageOrderReference != null).Select(ps => ps.Package.PackageOrderReference).Distinct().OrderBy(ps => ps.Package.KP_PackageID).ToDictionary(k => k.KPO_KP_Package, v => (ZInt)count++);

			try
			{
				if (receiveTransportationUnit != null)
				{
					var packageStatesForRTU = System.Array.Empty<WhsItemPackageState>();
					if (container != null)
					{
						packageStatesForRTU = packageStates.Where(ps => ps.WPS_WRH_TransitReceiveHeader == receiveTransportationUnit.PK).ToArray();
						helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packageStatesForRTU.Select(ps => ps.Package), consignment, container, orderReferenceDictionary: orderReferenceDictionary);
						helper.PopulateDataObject(dataObject);
					}

					var packageStatesForRCN = packageStates.Except(packageStatesForRTU);
					helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packageStatesForRCN.Select(ps => ps.Package), orderReferenceDictionary: orderReferenceDictionary);
					helper.PopulateDataObject(dataObject);
				}
				else if (receiveASN != null)
				{
					var packageStatesForASN = packageStates.Where(ps => ps.WPS_WRP_ReceiveExpectedPacking == receiveASN.PK);
					helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packageStatesForASN.Select(ps => ps.Package), orderReferenceDictionary: orderReferenceDictionary);
					helper.PopulateDataObject(dataObject);
				}
				else
				{
					helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packageStates.Select(ps => ps.Package), consignment, orderReferenceDictionary: orderReferenceDictionary);
					helper.PopulateDataObject(dataObject);
				}
				if (dataObject.PackingLineCollection == null)
				{
					dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { Content = CollectionContent.Complete });
				}
			}
			catch (PkgPackageJobDataObjectWriterHelper.DuplicatePackageException ex)
			{
				ReportDuplicatePackageException(ex, consignment, rcnPackageStates, helper);
			}
		}

		void ReportDuplicatePackageException(PkgPackageJobDataObjectWriterHelper.DuplicatePackageException ex, WhsItemReceiveConsignment consignment, IEnumerable<WhsItemPackageState> packageStates, PkgPackageJobDataObjectWriterHelper helper)
		{
			var duplicatePackage = packageStates.FirstOrDefault(p => p.WPS_KP_Package == ex.Package.PK);

			var packageStateDescriptions = packageStates.Select(p =>
				$"PackagePK : {p.WPS_KP_Package} PackageID : {p.Package.KP_PackageID} UnitType : {p.WPS_UnitType}"
			);

			var errorDescription =
				$@"Cannot add duplicate package {ex.Package.PK} to dictionary in WhsTransitReceiveConsignmentDataObjectWriter.

RCN PK: {consignment.PK} JobID: {consignment.WRC_JobID}
Duplicated Package: {duplicatePackage.WPS_KP_Package} + PackageID: {duplicatePackage.Package.KP_PackageID} + Duplicated LinkID: {helper.LinksDictionary[duplicatePackage.WPS_KP_Package]}
RCN's Package list:
{string.Join("\r\n", packageStateDescriptions)}";
			ErrorReporter.ReportOnce(errorDescription, ex);
		}

		void PopulateRelatedShipments(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			var packageStates = consignment.PackageStates;
			var rtusForPackages = packageStates.Where(ps => ps.ReceiveTransportationUnit != null).Select(ps => ps.ReceiveTransportationUnit).Distinct().ToArray();
			var asnsForPackages = packageStates.Where(ps => ps.ReceiveASN != null).Select(ps => ps.ReceiveASN).Distinct().ToArray();
			var orderReferences = packageStates.Where(ps => ps.Package.PackageOrderReference != null).Select(ps => ps.Package.PackageOrderReference).Distinct().ToArray();
			if (rtusForPackages.Any() || asnsForPackages.Any() || orderReferences.Any())
			{
				dataObject.SetRelatedShipmentCollection(() => WriteRelatedJobsToShipments(rtusForPackages, asnsForPackages, orderReferences));
			}
		}

		List<UniversalShipment> WriteRelatedJobsToShipments(WhsItemReceiveTransportationUnit[] rtus, WhsItemReceiveASN[] asns, PkgPackageOrderReference[] orderReferences)
		{
			var rtuWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(writeManager, shouldPopulateConsignments: false);
			var asnWriter = new WhsTransitReceiveASNDataObjectWriter(writeManager, shouldPopulateConsignments: false);
			var packageOrderReferenceWriter = new PkgPackageOrderReferenceDataObjectWriter(writeManager, orderReferenceDictionary);
			var result = new List<UniversalShipment>();
			foreach (var rtu in rtus)
			{
				var subshipment = rtuWriter.GetDataObject(rtu);
				result.Add(subshipment);
			}

			foreach (var asn in asns)
			{
				var subshipment = asnWriter.GetDataObject(asn);
				result.Add(subshipment);
			}

			foreach (var orderReference in orderReferences)
			{
				var subshipment = packageOrderReferenceWriter.GetDataObject(orderReference);
				result.Add(subshipment);
			}

			return result;
		}

		void PopulateAddresses(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			var writerCFS = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageCFS));
			var warehouseOrganiastionAddressCFS = writerCFS.GetDataObject(consignment.Warehouse.WarehouseAddress);

			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(consignment.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
			if (dataObject.OrganizationAddressCollection != null)
			{
				dataObject.OrganizationAddressCollection.Add(warehouseOrganiastionAddressCFS);
			}
			else
			{
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { warehouseOrganiastionAddressCFS });
			}

			if (consignment.CTODocAddress.Address != null)
			{
				var writerCTO = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageCTO));
				var warehouseOrganiastionAddressCTO = writerCTO.GetDataObject(consignment.CTODocAddress.Address);

				dataObject.OrganizationAddressCollection.Add(warehouseOrganiastionAddressCTO);
			}
		}

		void PopulateAdditionalServices(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			dataObject.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy);
			dataObject.LocalProcessing.SetAdditionalServiceCollection(() => ProcessCollection(consignment.Services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		void PopulateNotes(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			var notValidNoteTypes = TransitWarehouseNoteHelper.GetNoteTypesNotPopulate();
			dataObject.SetNoteCollection(() =>
			{
				var notes = consignment.Notes.GetAllNotesVisibleToCurrentCompany()
					.Where(note => !notValidNoteTypes.Contains(note.ST_Description)).OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateTransportRoutings(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			var writer = ObjectFactory.Get<ITransportLegDataObjectWriter>("ITransportLegDataObjectWriter", writeManager) as DataObjectWriter<BusinessObject, TransportLeg>;
			var collection = consignment.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, consignment.PK));
			// TODO: Every instance of a transport should have its ParentType set per comment in Transport.get_Parent.
			// Some other operational actions may refer to its property and ParentType check will throw exception with reason "ParentType not set". Test included.
			dataObject.SetTransportLegCollection(() => ProcessCollection(collection, writer, CollectionContent.Complete, true));
			var vaildTransports = TransportLegImportHelper.GetValidTransportLegs(collection, consignment.Warehouse, false);
			dataObject.VoyageFlightNo = vaildTransports.FirstOrDefault(t => !t.JW_VoyageFlight.IsEmpty)?.JW_VoyageFlight;
		}

		void PopulateReferences(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			var referenceCollection = consignment.AdditionalReferenceNumbers;
			referenceCollection.AddRange(consignment.CustomsReferenceNumbers);

			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(referenceCollection, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Partial));

			var portReferences = consignment.PortReferences.Cast<CusEntryNumber>().ToList();
			var hasOldPANReference = portReferences.GroupBy(p => p.CE_EntryType).Count() > 1;
			if (hasOldPANReference)
			{
				dataObject.SetPortReferenceCollection(() => ProcessCollection(portReferences.Where(p => p.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport), new PortReferenceDataObjectWriter(writeManager)));
			}
			else
			{
				dataObject.SetPortReferenceCollection(() => ProcessCollection(portReferences, new PortReferenceDataObjectWriter(writeManager)));
			}

			if (portReferences.Any(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport))
			{
				var portReferenceCollectionToBeAdded = new List<PortReference>();
				foreach (var penPortReference in dataObject.PortReferenceCollection.Where(r => r.Type.Code.HasValue && r.Type.Code.Value == TransitWarehousePortReferenceTypes.Codes.PortExport))
				{
					var panPortReference = new PortReference()
					{
						Country = penPortReference.Country,
						Status = penPortReference.Status,
						Reference = penPortReference.Reference,
						Type = new PortReferenceType() { Code = TransitWarehousePortReferenceTypes.Codes.PortAuthority, Description = TransitWarehousePortReferenceTypes.Descriptions.PortAuthority },
					};
					portReferenceCollectionToBeAdded.Add(panPortReference);
				}
				dataObject.PortReferenceCollection.AddRange(portReferenceCollectionToBeAdded);
			}

			dataObject.CreateAdditionalReference(consignment.WRC_JobID, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceive, TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseReceive);
			dataObject.CreateAdditionalReference(consignment.WRC_ConsignmentID, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveReference, TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseReceiveReference);

			dataObject.WayBillNumber = consignment.WRC_HouseBillNumber;
			dataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());
		}

		void PopulateDestination(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
			=> dataObject.PortOfDestination = UNLOCO.New(consignment.Destination);

		void PopulatePackageScreeningResults(WhsItemReceiveConsignment consignment, UniversalShipment dataObject)
		{
			var latestScreenings = consignment.PackageStates.Select(ps => ps.Package.Screenings.OrderByDescending(s => s.KPS_Time).FirstOrDefault());
			var hasScreenings = latestScreenings.Any(s => s != null);
			if (hasScreenings)
			{
				if (latestScreenings.All(s => s != null && s.KPS_Passed))
				{
					dataObject.ScreeningStatus = new CodeDescriptionPair() { Code = ScreeningStatusesList.Codes.Clear, Description = ScreeningStatusesList.Descriptions.Clear };
				}
				else
				{
					dataObject.ScreeningStatus = new CodeDescriptionPair() { Code = ScreeningStatusesList.Codes.Unknown, Description = ScreeningStatusesList.Descriptions.Unknown };
				}
			}
			else
			{
				dataObject.ScreeningStatus = new CodeDescriptionPair() { Code = ScreeningStatusesList.Codes.NotScreened, Description = ScreeningStatusesList.Descriptions.NotScreened };
			}
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransitReceive;
		}
	}
}
