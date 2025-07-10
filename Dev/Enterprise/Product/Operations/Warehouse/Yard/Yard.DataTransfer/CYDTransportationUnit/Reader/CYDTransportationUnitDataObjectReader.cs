using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public abstract class CYDTransportationUnitDataObjectReader : ContainerYardDataObjectReader<CYDTransportationUnit>
	{
		protected CYDTransportationUnitDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			GateValidationServiceHelper.ValidateTransportOrgAddress(dataObject);
			GateValidationServiceHelper.ValidateVehicle(dataObject);

			Yard = ContainerYardUniversalHelper.GetYard(dataObject, factory, logger);
			TransportOrgAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress));
			MatchedTransportOrgAddress = CYDTransportationUnitMatchingHelper.GetMatchedTransportOrgAddress(dataObject, TransportOrgAddress, logger, factory);
			Vehicle = CYDTransportationUnitMatchingHelper.GetVehicle(dataObject);
		}

		public override DataContextType DataContextType => DataContextType.CYDTransportationUnit;
		protected WhsWarehouse Yard { get; }
		protected OrganizationAddress TransportOrgAddress { get; }
		protected Vehicle Vehicle { get; }
		protected OrgAddress MatchedTransportOrgAddress { get; }

		protected override IMatchingBusinessEntityFinder<CYDTransportationUnit> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override CYDTransportationUnit GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			CYDTransportationUnit transportationUnit = null;

			if (MatchedTransportOrgAddress != null)
			{
				transportationUnit = GetMatchingTransportationUnitFromJobLinks() ?? GetMatchingTransportationUnitUsingQuery();
			}

			return transportationUnit;
		}

		protected abstract CYDTransportationUnit GetMatchingTransportationUnitUsingQuery();

		protected abstract CYDTransportationUnit GetMatchingTransportationUnitFromJobLinks();

		protected CYDTransportationUnit GetLatestTransportationUnitRowsFromJobLinks(IEnumerable<IColumnIndexer> jobLinks)
		{
			var query = new ZQuery(CYDTransportationUnitSchema.PK, jobLinks.Select(link => link.GetValue(StmUniversalJobLinkSchema.UCL_ParentID)));
			query.AddToFilter(CYDTransportationUnitSchema.YTU_WW_Yard, Yard.PK);
			query.OrderBy = CYDTransportationUnitSchema.Constants.YTU_SystemCreateTimeUtc + OrderByClause.Descending;
			query.MaximumRows = 1;

			return factory.LoadTop1<CYDTransportationUnit>(query);
		}

		protected void PopulateJobDocAddressAndLink(CYDTransportationUnit transportationUnit, OrganizationAddress organizationAddress, DataContextType dataContext)
		{
			var jobDocAddress = PopulateJobDocAddress(transportationUnit, organizationAddress);
			var sourceDataContext = Shipment.GetSourceDataObject(dataObject).DataContext;
			var linkCreator = new UniversalJobLinkCreator(transportationUnit.Factory, transportationUnit, jobDocAddress.Organisation, sourceDataContext, logger);
			linkCreator.TryCreateJobLink(dataContext);
		}

		protected JobDocAddress PopulateJobDocAddress(CYDTransportationUnit transportationUnit, OrganizationAddress organizationAddress)
		{
			var gateAddressMatcher = ObjectFactory.New<IGateManagementOrganisationDataObjectReader>(organizationAddress, logger, factory);
			var jobDocAddress = gateAddressMatcher.GetMatchedOrNew(transportationUnit) as JobDocAddress;
			return jobDocAddress;
		}

		protected void PopulateDeliveriesAndPickups(CYDTransportationUnit transportationUnit)
		{
			var subShipmentCollection = dataObject.SubShipmentCollection;
			if (subShipmentCollection != null)
			{
				foreach (var subShipment in subShipmentCollection.Where(s => s.ContainerCollection != null && s.ContainerCollection.Any()))
				{
					var direction = subShipment.TransportBookingDirection.Code.GetValueOrDefault();
					var container = subShipment.ContainerCollection.FirstOrDefault();
					if (direction == TransportBookingDirections.Codes.Delivery)
					{
						var deliveryCollection = new CYDDeliveryCollectionDataObjectReader(new DataObjectList<Container> { container }, transportationUnit, subShipment, logger, factory);
						deliveryCollection.ReadIntoCollection();
					}
					else if (direction == TransportBookingDirections.Codes.Pickup)
					{
						var pickupCollection = new CYDPickupCollectionDataObjectReader(new DataObjectList<Container> { container }, transportationUnit, subShipment, logger, factory);
						pickupCollection.ReadIntoCollection();
					}
				}
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("5d510894-46e3-49ea-9b16-03d490344b0d", "No Sub-shipment details found in UXML."));
			}
		}
	}
}
