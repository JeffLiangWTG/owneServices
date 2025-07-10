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
	public class CYDDeliveryDataObjectReader : ContainerYardDataObjectReader<CYDDelivery>
	{
		public CYDDeliveryDataObjectReader(Container container, string bookingReference, string transportReference, CYDTransportationUnit transportationUnit, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			IsBooking = CYDTransportationUnitMatchingHelper.IsBooking(dataObject);
			Container = Argument.NotNull(container, nameof(container));
			Argument.NotNullOrEmpty(container.ContainerNumber, nameof(container.ContainerNumber));
			if (IsBooking)
			{
				BookingReference = bookingReference ?? string.Empty;
			}
			else
			{
				BookingReference = !string.IsNullOrEmpty(bookingReference)
					? bookingReference
					: throw new DataObjectReadFailureException(Res.GetString("d9bd0f51-de3b-428a-be22-915f23d1fcca", "Booking Confirmation Reference is required in UXML for delivery."));
			}
			TransportReference = transportReference ?? string.Empty;
			TransportationUnit = Argument.NotNull(transportationUnit, nameof(transportationUnit));

			YardUnit = CYDTransportationUnitMatchingHelper.FindYardUnitForDelivery(transportationUnit, container, BookingReference)
				?? CYDTransportationUnitMatchingHelper.CreateYardUnitForDelivery(transportationUnit, container, BookingReference, logger);
		}

		Container Container { get; }
		string BookingReference { get; }
		string TransportReference { get; }
		CYDTransportationUnit TransportationUnit { get; }
		CYDYardUnitState YardUnit { get; set; }
		bool IsBooking { get; }

		public override DataContextType DataContextType => DataContextType.CYDTransportationUnit;

		protected override IMatchingBusinessEntityFinder<CYDDelivery> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override string GetBusinessObjectHumanReadableName(CYDDelivery businessObject)
		{
			return Res.GetString("f1e9d567-c331-433f-a742-24fcd912683a", "Delivery");
		}

		protected override CYDDelivery GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return GetEntityFromJobLinks() ?? YardUnit?.Delivery;
		}

		CYDDelivery GetEntityFromJobLinks()
		{
			var dataContextType = IsBooking ? DataContextType.GateMovementBooking : DataContextType.GateMovement;
			var source = dataObject.GetMatchingDataSource(dataContextType);
			var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, dataContextType, null, CYDDeliverySchema.Constants.Prefix);

			var query = new ZQuery(CYDDeliverySchema.PK, jobLinks.Select(link => link.GetValue(StmUniversalJobLinkSchema.UCL_ParentID)));
			query.OrderBy = CYDDeliverySchema.Constants.YDL_SystemCreateTimeUtc + OrderByClause.Descending;
			query.MaximumRows = 1;

			return factory.LoadTop1<CYDDelivery>(query);
		}

		protected override void PopulateBusinessObject(CYDDelivery delivery)
		{
			if (IsNewBO)
			{
				if (!IsBooking && YardUnit.YUS_YRL_ReceiveLine.IsEmpty && !TransportationUnit.YTU_GateInTime.IsEmpty)
				{
					throw new DataObjectReadFailureException(Res.GetString("3077dadf-666b-43cc-83b5-c1879f61382a", "TPU Gate In when PRA is not available."));
				}

				var unitLineItem = factory.New<CYDUnitLineItem>();

				PopulateUnitLineItem(unitLineItem);

				PopulateYardUnit(YardUnit, delivery);

				PopulateDelivery(delivery, unitLineItem);
			}
			else
			{
				if (YardUnit.PK != delivery.LinkedYardUnit.PK)
				{
					var obsoleteYardUnit = delivery.LinkedYardUnit;

					PopulateUnitLineItem(delivery.UnitLineItem);

					PopulateYardUnit(YardUnit, delivery);

					if (!YardUnit.YUS_YRL_ReceiveLine.IsEmpty)
					{
						SetValue(delivery, CYDDeliverySchema.YDL_YRL_ReceiveAdviceLine, YardUnit.YUS_YRL_ReceiveLine);
					}

					obsoleteYardUnit.Delete();
				}
			}

			if (!IsBooking && YardUnit.YUS_YLI_UnitLineItem.IsEmpty)
			{
				var unitLineItemForYardUnit = delivery.UnitLineItem.Clone();
				SetValue(YardUnit, CYDYardUnitStateSchema.YUS_YLI_UnitLineItem, unitLineItemForYardUnit.PK);
			}

			var sourceDataContext = Shipment.GetSourceDataObject(dataObject).DataContext;
			if (sourceDataContext != null)
			{
				var linkCreator = new UniversalJobLinkCreator(delivery.Factory, delivery, null, sourceDataContext, logger, true);
				linkCreator.TryCreateJobLink(DataContextType.GateMovementBooking);
				linkCreator.TryCreateJobLink(DataContextType.GateMovement);
			}
		}

		void PopulateUnitLineItem(CYDUnitLineItem unitLineItem)
		{
			unitLineItem.YLI_Quantity = 1;
			unitLineItem.YLI_Type = "CNT";

			if (!YardUnit.YUS_YRL_ReceiveLine.IsEmpty)
			{
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_Type, YardUnit.ReceiveAdviceLine.UnitLineItem.YLI_Type);
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_RC_ContainerType, YardUnit.ReceiveAdviceLine.UnitLineItem.YLI_RC_ContainerType);
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_IsEmpty, YardUnit.ReceiveAdviceLine.UnitLineItem.YLI_IsEmpty);
			}

			if (!string.IsNullOrEmpty(Container.ContainerType?.Code))
			{
				var containerType = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, Container.ContainerType.Code));
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_RC_ContainerType, containerType.PK);
			}
			else if (!YardUnit.YUS_YRL_ReceiveLine.IsEmpty)
			{
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_RC_ContainerType, YardUnit.ReceiveAdviceLine.UnitLineItem.ContainerType.PK);
			}
		}

		void PopulateYardUnit(CYDYardUnitState yardUnit, CYDDelivery delivery)
		{
			SetValue(yardUnit, CYDYardUnitStateSchema.YUS_YDL_Delivery, delivery.PK);
			SetValue(yardUnit, CYDYardUnitStateSchema.YUS_YTU_ReceiveTransportationUnit, TransportationUnit.PK);
		}

		void PopulateDelivery(CYDDelivery delivery, CYDUnitLineItem unitLineItem)
		{
			if (!YardUnit.YUS_YRL_ReceiveLine.IsEmpty)
			{
				SetValue(delivery, CYDDeliverySchema.YDL_YRL_ReceiveAdviceLine, YardUnit.YUS_YRL_ReceiveLine);
			}
			SetValue(delivery, CYDDeliverySchema.YDL_DeliveryID, new DeliveryIDNumberFoutainStrategy(factory.BOFactory).GetDeliveryID());
			SetValue(delivery, CYDDeliverySchema.YDL_TransportReference, TransportReference);
			SetValue(delivery, CYDDeliverySchema.YDL_YTU_DeliveryTransportationUnit, TransportationUnit.PK);
			SetValue(delivery, CYDDeliverySchema.YDL_YLI_UnitLineItem, unitLineItem.PK);
		}
	}
}
