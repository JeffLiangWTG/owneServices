using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
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
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal
{
	public class WhsTransitDispatchConsignmentDataObjectWriter : TopLevelDataObjectWriter<WhsItemDispatchConsignment, UniversalShipment>
	{
		Dictionary<ZGuid, ZInt> orderReferenceDictionary;

		public WhsTransitDispatchConsignmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			dataObject.PopulateTransportMode(consignment.WDC_TransportMode);
			PopulatePackages(consignment, dataObject);
			PopulateDispatchTransportationUnits(consignment, dataObject);
			PopulateAddresses(consignment, dataObject);
			PopulateAdditionalServices(consignment, dataObject);
			PopulateNotes(consignment, dataObject);
			PopulateReferences(consignment, dataObject);
			PopulateDestination(consignment, dataObject);
			PopulateValidationRuleCollection(dataObject);
			PopulateConsignmentOrderReferences(consignment, dataObject);
		}

		void PopulateValidationRuleCollection(UniversalShipment dataObject)
		{
			dataObject.SetValidationRuleCollection(() => new List<UniversalDataBuss.DataObjects.ValidationRule>());
		}

		void PopulatePackages(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			var dcnPackageStates = consignment.PackageStates.Where(p => p.WPS_Status != TransitWarehouseStatuses.Codes.AdjustedOut);
			try
			{
				var ovpInnerPackagePKs = dcnPackageStates.Where(ps => ps.WPS_UnitType == PackageStateUnitType.Codes.Overpack)
					.SelectMany(h => h.Package.HandlingUnitPackedPackages)
					.Select(i => i.PK);

				var packageStates = dcnPackageStates.Where(ps => !ovpInnerPackagePKs.Contains(ps.WPS_KP_Package));
				var huInnerPackages = dcnPackageStates.Where(ps => !ps.Package.KP_KP_TopHandlingUnitPackage.IsEmpty && !ovpInnerPackagePKs.Contains(ps.PK));
				var handlingUnits = GetHandlingUnits(consignment.Factory, huInnerPackages);
				var count = 0;
				orderReferenceDictionary = packageStates.Where(ps => ps.Package.PackageOrderReference != null).Select(ps => ps.Package.PackageOrderReference).Distinct().OrderBy(ps => ps.Package.KP_PackageID).ToDictionary(k => k.KPO_KP_Package, v => (ZInt)count++);

				var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packageStates.Select(ps => ps.Package), consignment, null, handlingUnits, orderReferenceDictionary, contentType: consignment.IsSplit ? CollectionContent.Partial : CollectionContent.Complete);

				helper.PopulateDataObject(dataObject);
				if (dataObject.PackingLineCollection == null)
				{
					dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { Content = CollectionContent.Complete });
				}
			}
			catch (NullReferenceException ex)
			{
				ReportNullPackageException(ex, consignment, dcnPackageStates);
			}
		}

		void ReportNullPackageException(NullReferenceException ex, WhsItemDispatchConsignment consignment, IEnumerable<WhsItemPackageState> dcnPackageStates)
		{
			var packageStateDescriptions = dcnPackageStates.Select(p =>
				$"PackagePK: {p.WPS_KP_Package} PackageID: {p.Package.KP_PackageID} UnitType: {p.WPS_UnitType} TopHandlingUnit: {p.Package.KP_KP_TopHandlingUnitPackage} Divots: {p.Package.PackageHandlingUnitHandlingUnitDivots.Count}"
			);

			var errorDescription =
				$@"DCN PK: {consignment.PK} JobID: {consignment.WDC_JobID}
DCN's Package list:
{string.Join("\r\n", packageStateDescriptions)}";

			ErrorReporter.ReportOnce(errorDescription, ex);
		}

		void PopulateConsignmentOrderReferences(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			if (consignment.OrderReferences.Count > 0)
			{
				dataObject.LocalProcessing.SetOrderNumberCollection(() => ProcessCollection(consignment.OrderReferences, new TransitConsignmentOrderReferenceDataObjectWriter(writeManager), CollectionContent.Partial));
			}
		}

		IEnumerable<PkgPackage> GetHandlingUnits(BusinessObjectFactory factory, IEnumerable<WhsItemPackageState> innerPackageStates)
		{
			if (!innerPackageStates.Any())
			{
				return null;
			}
			var typeIsHUSubQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_KP_Package);
			typeIsHUSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_UnitType, PackageStateUnitType.Codes.HandlingUnit);
			typeIsHUSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_KP_Package, innerPackageStates.Select(ps => ps.Package.KP_KP_TopHandlingUnitPackage));

			var notInCurrentDCNPackageStateSubQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_KP_Package);
			notInCurrentDCNPackageStateSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, SQLComparisonOperator.NotEqual, innerPackageStates.First().WPS_WDC_TransitDispatchConsignment);
			var attachedToOtherDCNSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KP_TopHandlingUnitPackage, true);
			attachedToOtherDCNSubQuery.AddToFilter(PkgPackageSchema.KP_KP_TopHandlingUnitPackage, innerPackageStates.Select(ps => ps.Package.KP_KP_TopHandlingUnitPackage).ToArray());
			attachedToOtherDCNSubQuery.AddSubQuery(PkgPackageSchema.PK, notInCurrentDCNPackageStateSubQuery, JoinCondition.And);

			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			packageQuery.AddSubQuery(PkgPackageSchema.PK, typeIsHUSubQuery, JoinCondition.And);
			packageQuery.AddSubQuery(PkgPackageSchema.PK, attachedToOtherDCNSubQuery, JoinCondition.And);

			return factory.Load<PkgPackage>(packageQuery);
		}

		void PopulateDispatchTransportationUnits(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			var packageStates = consignment.PackageStates;
			var dtusForPackages = packageStates.Where(ps => ps.DispatchTransportationUnit != null).Select(ps => ps.DispatchTransportationUnit).Distinct().ToArray();
			var dllsForPackages = packageStates.Where(ps => ps.DispatchLoadList != null).Select(ps => ps.DispatchLoadList).Distinct().ToArray();
			var orderReferences = packageStates.Where(ps => ps.Package.PackageOrderReference != null).Select(ps => ps.Package.PackageOrderReference).Distinct().ToArray();
			if (dtusForPackages.Any() || dllsForPackages.Any() || orderReferences.Any())
			{
				dataObject.SetRelatedShipmentCollection(() => WriteDTUsToShipments(dtusForPackages).Concat(PackageOrderReferencesToShipments(orderReferences)).ToList());
				dataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(WriteDLLsToShipments(dllsForPackages)));
			}
		}

		IEnumerable<UniversalShipment> WriteDTUsToShipments(WhsItemDispatchTransportationUnit[] dtus)
		{
			var dtuWriter = new WhsTransitDispatchTransportationUnitDataObjectWriter(writeManager);
			return dtus.Select(l => dtuWriter.GetDataObject(l));
		}

		IEnumerable<UniversalShipment> WriteDLLsToShipments(WhsItemDispatchLoadList[] dlls)
		{
			var dllWriter = new WhsTransitDispatchLoadListDataObjectWriter(writeManager);
			return dlls.Select(l => dllWriter.GetDataObject(l));
		}

		IEnumerable<UniversalShipment> PackageOrderReferencesToShipments(PkgPackageOrderReference[] orderReferences)
		{
			var packageOrderReferenceWriter = new PkgPackageOrderReferenceDataObjectWriter(writeManager, orderReferenceDictionary);
			return orderReferences.Select(l => packageOrderReferenceWriter.GetDataObject(l));
		}

		void PopulateAddresses(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			var writer = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageCFS));
			var warehouseOrganiastionAddress = writer.GetDataObject(consignment.Warehouse.WarehouseAddress);

			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(consignment.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
			if (dataObject.OrganizationAddressCollection != null)
			{
				dataObject.OrganizationAddressCollection.Add(warehouseOrganiastionAddress);
			}
			else
			{
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { warehouseOrganiastionAddress });
			}
		}

		void PopulateAdditionalServices(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			var localProcessing = new LocalProcessing(writeManager.WriterStrategy);
			localProcessing.SetAdditionalServiceCollection(() => ProcessCollection(consignment.Services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Partial));
			dataObject.LocalProcessing = localProcessing;
		}

		void PopulateNotes(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			var notValidNoteTypes = TransitWarehouseNoteHelper.GetNoteTypesNotPopulate();
			dataObject.SetNoteCollection(() =>
			{
				var notes = consignment.Notes.GetAllNotesVisibleToCurrentCompany()
					.Where(note => !notValidNoteTypes.Contains(note.ST_Description)).OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateReferences(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
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

			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(consignment.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Partial));
			dataObject.CreateAdditionalReference(consignment.WDC_JobID, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatch, TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseDispatch);
			dataObject.CreateAdditionalReference(consignment.WDC_ConsignmentID, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchReference, TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseDispatchReference);
		}

		void PopulateDestination(WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
			=> dataObject.PortOfDestination = UNLOCO.New(consignment.Destination);

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransitDispatch;
		}
	}
}
