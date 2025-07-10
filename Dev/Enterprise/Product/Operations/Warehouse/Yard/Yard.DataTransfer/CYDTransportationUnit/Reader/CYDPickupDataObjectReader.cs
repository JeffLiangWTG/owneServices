using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDPickupDataObjectReader : ContainerYardDataObjectReader<CYDPickup>
	{
		public CYDPickupDataObjectReader(Container container, string bookingReference, string transportReference, CYDTransportationUnit transportationUnit, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			IsBooking = dataObject.GetMatchingDataSource(DataContextType.GateMovementBooking) is not null && dataObject.GetMatchingDataSource(DataContextType.GateMovement) is null;
			this.container = Argument.NotNull(container, nameof(container));
			Argument.NotNullOrEmpty(container.ContainerType.Code, nameof(container.ContainerType.Code));
			this.bookingReference = Argument.NotNullOrEmpty(bookingReference, nameof(bookingReference));
			this.transportReference = transportReference ?? string.Empty;
			this.transportationUnit = Argument.NotNull(transportationUnit, nameof(transportationUnit));
			yardUnit = CYDTransportationUnitMatchingHelper.FindYardUnitForPickup(container, bookingReference, factory);
		}

		readonly Container container;
		readonly string bookingReference;
		readonly string transportReference;
		readonly CYDTransportationUnit transportationUnit;
		readonly CYDYardUnitState yardUnit;
		bool IsBooking { get; }

		public override DataContextType DataContextType => DataContextType.CYDTransportationUnit;

		protected override IMatchingBusinessEntityFinder<CYDPickup> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override string GetBusinessObjectHumanReadableName(CYDPickup businessObject)
		{
			return Res.GetString("ebf2aed6-b3c9-42dd-8827-969762fb8a18", "Pickup");
		}

		protected override CYDPickup GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return GetEntityFromJobLinks() ?? yardUnit?.Pickup;
		}

		CYDPickup GetEntityFromJobLinks()
		{
			var dataContextType = IsBooking ? DataContextType.GateMovementBooking : DataContextType.GateMovement;
			var source = dataObject.GetMatchingDataSource(dataContextType);
			var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, dataContextType, null, CYDPickupSchema.Constants.Prefix);

			var query = new ZQuery(CYDPickupSchema.PK, jobLinks.Select(link => link.GetValue(StmUniversalJobLinkSchema.UCL_ParentID)));
			query.OrderBy = CYDPickupSchema.Constants.YPL_SystemCreateTimeUtc + OrderByClause.Descending;
			query.MaximumRows = 1;

			return factory.LoadTop1<CYDPickup>(query);
		}

		protected override void PopulateBusinessObject(CYDPickup pickup)
		{
			var releaseAdviceLine = pickup.ReleaseAdviceLine ?? yardUnit?.ReleaseAdviceLine ?? CYDTransportationUnitMatchingHelper.GetReleaseAdviceLine(bookingReference, container.ContainerType.Code, pickup.Factory);

			PopulatePickup(pickup, transportationUnit, transportReference, releaseAdviceLine);

			if (yardUnit != null)
			{
				SetValue(yardUnit, CYDYardUnitStateSchema.YUS_YTU_DispatchTransportationUnit, transportationUnit.PK);
				SetValue(yardUnit, CYDYardUnitStateSchema.YUS_YPL_Pickup, pickup.PK);
			}

			if (releaseAdviceLine.UnitLineItem.YLI_IsPreAdvice && yardUnit == null)
			{
				var yardUnitState = factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YEL_ReleaseLine, releaseAdviceLine.PK)).FirstOrDefault();
				SetValue(yardUnitState, CYDYardUnitStateSchema.YUS_YTU_DispatchTransportationUnit, transportationUnit.PK);
				SetValue(yardUnitState, CYDYardUnitStateSchema.YUS_YPL_Pickup, pickup.PK);
			}

			var sourceDataContext = Shipment.GetSourceDataObject(dataObject).DataContext;
			if (sourceDataContext != null)
			{
				var linkCreator = new UniversalJobLinkCreator(pickup.Factory, pickup, null, sourceDataContext, logger, true);
				linkCreator.TryCreateJobLink(DataContextType.GateMovementBooking);
				linkCreator.TryCreateJobLink(DataContextType.GateMovement);
			}
		}

		void PopulatePickup(CYDPickup pickup, CYDTransportationUnit transportationUnit, string transportReference, CYDReleaseAdviceLine releaseAdviceLine)
		{
			SetValue(pickup, CYDPickupSchema.YPL_YEL_ReleaseAdviceLine, releaseAdviceLine.PK);

			var unitLineItem = factory.New<CYDUnitLineItem>();
			SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_Quantity, 1);
			SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_Type, releaseAdviceLine.UnitLineItem.YLI_Type);
			SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_RC_ContainerType, releaseAdviceLine.UnitLineItem.YLI_RC_ContainerType);
			SetValue(pickup, CYDPickupSchema.YPL_YLI_UnitLineItem, unitLineItem.PK);

			SetValue(pickup, CYDPickupSchema.YPL_YTU_PickupTransportationUnit, transportationUnit.PK);
			SetValue(pickup, CYDPickupSchema.YPL_TransportReference, transportReference);

			if (pickup.YPL_PickupID.IsEmpty)
			{
				SetValue(pickup, CYDPickupSchema.YPL_PickupID, new PickupIDNumberFoutainStrategy(factory.BOFactory).GetPickupID());
			}
		}
	}
}
