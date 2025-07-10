using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchTransportationUnitDataObjectWriter : TopLevelDataObjectWriter<WhsItemDispatchTransportationUnit, UniversalShipment>
	{
		public WhsTransitDispatchTransportationUnitDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransitDispatchHeader;
		}

		protected override void PopulateDataObject(WhsItemDispatchTransportationUnit dispatchTransportationUnit, UniversalShipment dataObject)
		{
			var loadListsOrderedByStagingLocationASC = dispatchTransportationUnit.DispatchLoadLists.Where(dll => dll.Location != null)?.OrderBy(dll => dll.Location.WLV_LocationString);
			dataObject.WarehouseLocation = loadListsOrderedByStagingLocationASC.FirstOrDefault()?.Location?.ToLocationString();
			dataObject.VesselName = dispatchTransportationUnit.WDH_VehicleReference;

			PopulatePackages(dispatchTransportationUnit, dataObject);
			PopulateAddresses(dispatchTransportationUnit, dataObject);
			PopulateDates(dispatchTransportationUnit, dataObject);
			PopulateNotes(dispatchTransportationUnit, dataObject);
			PopulateReferences(dispatchTransportationUnit, dataObject);
		}

		void PopulatePackages(WhsItemDispatchTransportationUnit dispatchTransportationUnit, UniversalShipment dataObject)
		{
			var packages = dispatchTransportationUnit.PackageJob.Packages;
			var packagesExcludingTrucks = packages.Where(p => p.IsContainer || !p.PackageExtensions.Any());
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesExcludingTrucks, dispatchTransportationUnit);
			helper.PopulateDataObject(dataObject);
			if (dataObject.PackingLineCollection == null)
			{
				dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { Content = CollectionContent.Complete });
			}
		}

		void PopulateAddresses(WhsItemDispatchTransportationUnit dispatchTransportationUnit, UniversalShipment dataObject)
		{
			var writer = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageCFS));
			var warehouseOrganiastionAddress = writer.GetDataObject(dispatchTransportationUnit.Warehouse.WarehouseAddress);

			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(dispatchTransportationUnit.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
			if (dataObject.OrganizationAddressCollection != null)
			{
				dataObject.OrganizationAddressCollection.Add(warehouseOrganiastionAddress);
			}
			else
			{
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { warehouseOrganiastionAddress });
			}
		}

		void PopulateDates(WhsItemDispatchTransportationUnit dispatchTransportationUnit, UniversalShipment dataObject)
		{
			if (dispatchTransportationUnit.WDH_LoadCompleteTime.IsValid)
			{
				dataObject.SetDateCollection(() => new List<Date>(new[] { Date.New(DateType.Pack, ZBool.False, dispatchTransportationUnit.WDH_LoadCompleteTime.ToZDateTime()) }));
			}
		}

		void PopulateNotes(WhsItemDispatchTransportationUnit dispatchTransportationUnit, UniversalShipment dataObject)
		{
			dataObject.SetNoteCollection(() =>
			{
				var notes = dispatchTransportationUnit.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateReferences(WhsItemDispatchTransportationUnit dispatchTransportationUnit, UniversalShipment dataObject)
		{
			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(dispatchTransportationUnit.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Partial));
		}
	}
}
