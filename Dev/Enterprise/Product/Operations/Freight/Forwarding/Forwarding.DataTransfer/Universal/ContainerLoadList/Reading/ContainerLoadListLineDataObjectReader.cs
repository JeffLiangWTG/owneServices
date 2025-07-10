using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ContainerLoadListLineDataObjectReader : ShipmentDataObjectReader<ContainerLoadListLine>
	{
		protected override LogType LogTypeForReasonNotAbleToUpdate => LogType.Warning;
		readonly CommonContainerLoadList containerLoadList;
		readonly ContainerLoadListContainerLinkManager containerLinkManager;

		public override DataContextType DataContextType => DataContextType.ContainerLoadList;

		public ContainerLoadListLineDataObjectReader(UniversalShipment dataObject, CommonContainerLoadList containerLoadList, IXmlImportLogger logger, UniversalObjectFactory factory, ContainerLoadListContainerLinkManager containerLinkManager)
			: base(dataObject, logger, factory)
		{
			this.containerLoadList = containerLoadList;
			this.containerLinkManager = containerLinkManager;
		}

		protected override IMatchingBusinessEntityFinder<ContainerLoadListLine> GetCombinedReferenceMatcher() => null;

		protected override ContainerLoadListLine GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var checkResult = CheckDataObject();
			return string.IsNullOrEmpty(checkResult.errorMessage)
				? containerLoadList.LoadListLines.FirstOrDefault(line => line.CLL_JSL_BookingLine == checkResult.matchedSupplierBookingLine?.PK && line.CLL_JC_Container == (checkResult.matchedContainer?.PK ?? ZGuid.Empty))
				: null;
		}

		protected override ContainerLoadListLine GetNewBusinessObject()
		{
			return containerLoadList.LoadListLines.AddNew();
		}

		protected override void PopulateBusinessObject(ContainerLoadListLine targetBO)
		{
			var matchedSupplierBooking = factory.BOFactory.LoadFromNaturalKey<JobSupplierBooking>(JobSupplierBookingSchema.JSB_BookingId, GetSupplierBookingKey());
			var matchedSupplierBookingLine = matchedSupplierBooking.SupplierBookingLines?.FirstOrDefault(line => line.JSL_BookingLineId == GetSupplierBookingLineId());
			var container = containerLinkManager.GetContainer(GetContainerLink() ?? ZInt.Zero);

			var packLine = dataObject.PackingLineCollection.Single();

			SetValue(targetBO, ContainerLoadListLineSchema.CLL_CLH_LoadListHeader, containerLoadList.PK);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_LoadMode, containerLoadList.CLH_LoadMode);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_JSL_BookingLine, matchedSupplierBookingLine.PK);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_LoadSequence, packLine.ContainerPackingOrder ?? ZInt.Zero);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_F3_NKPackagesUnit, packLine.PackType?.Code ?? ZString.Empty);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_VolumeUnit, packLine.VolumeUnit);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_WeightUnit, packLine.WeightUnit);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_HarmonizedCode, packLine.HarmonisedCode);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_RH_NKCommodityCode, packLine.Commodity?.Code.GetValueOrDefault() ?? ZString.Empty);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_ReferenceNumber, packLine.ReferenceNumber);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_JC_Container, container?.PK ?? ZGuid.Empty);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_Description, packLine.GoodsDescription);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_MarksAndNumbers, packLine.MarksAndNos);

			SetValue(targetBO, ContainerLoadListLineSchema.CLL_PackedQuantity, dataObject.TotalNoOfPacksDecimal);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_Volume, packLine.Volume);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_Weight, packLine.Weight);
			SetValue(targetBO, ContainerLoadListLineSchema.CLL_Packages, packLine.PackQty);
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(ContainerLoadListLine targetBO)
		{
			var checkResult = CheckDataObject();
			if (!string.IsNullOrEmpty(checkResult.errorMessage))
			{
				return checkResult.errorMessage;
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		(string errorMessage, JobSupplierBookingLine matchedSupplierBookingLine, CommonContainer matchedContainer) CheckDataObject()
		{
			if (dataObject.PackingLineCollection?.Count != 1)
			{
				return (Res.GetString("a57af817-2e09-4ee4-9b44-175012c306b9", "There is no or more than one Packing Line."), null, null);
			}

			var supplierBookingKey = GetSupplierBookingKey();
			if (supplierBookingKey.IsEmpty)
			{
				return (Res.GetString("408102f5-8872-44e4-98c7-18605d4cf859", "There is no supplier booking key."), null, null);
			}

			var matchedSupplierBooking = factory.BOFactory.LoadFromNaturalKey<JobSupplierBooking>(JobSupplierBookingSchema.JSB_BookingId, supplierBookingKey);
			if (matchedSupplierBooking == null)
			{
				return (Res.GetString("c396b7dd-1682-4612-b3bf-80b48aef501d", "The supplier booking {0} does not exist.", supplierBookingKey), null, null);
			}

			var supplierBookingDataObject = dataObject.SubShipmentCollection?.FirstOrDefault();
			if (supplierBookingDataObject.SubShipmentCollection?.Count != 1)
			{
				return (Res.GetString("3f6106c1-e56f-4365-a168-b6347be7afc4", "There is no or more than one order in {0}.", supplierBookingKey), null, null);
			}

			var orderDataObject = supplierBookingDataObject.SubShipmentCollection?.FirstOrDefault();
			if (orderDataObject?.Order == null)
			{
				return (Res.GetString("dec6e55e-fdee-4492-95cc-016589dc1c20", "There is no order in {0}.", supplierBookingKey), null, null);
			}

			if (orderDataObject?.Order?.OrderLineCollection?.Count != 1)
			{
				return (Res.GetString("084309c2-14de-4b3e-9938-ca77244a2b89", "There is no or more than one order line in {0}.", supplierBookingKey), null, null);
			}

			var orderLine = GetMatchedOrderLine(orderDataObject);
			if (orderLine == null)
			{
				var orderData = orderDataObject?.Order;
				var orderLineData = orderData.OrderLineCollection[0];
				return (
					Res.GetString("843f0e81-e8f4-42fe-b368-fa22b5eb412e",
						"The order line {0}-{1} [{2}-{3}] does not exist.",
						orderData.OrderNumber,
						orderData.OrderNumberSplit,
						orderLineData.LineNumber,
						orderLineData.SubLineNumber),
					null,
					null
				);
			}

			var supplierBookingLineId = GetSupplierBookingLineId();
			JobSupplierBookingLine matchedSupplierBookingLine;
			if (!supplierBookingLineId.IsEmpty)
			{
				matchedSupplierBookingLine = matchedSupplierBooking.SupplierBookingLines?.FirstOrDefault(line => line.JSL_BookingLineId == supplierBookingLineId);
				if (matchedSupplierBookingLine == null)
				{
					return (Res.GetString("63a2d44a-63c1-46e7-8502-8f30b9f70890", "The supplier booking line {0} does not exist.", supplierBookingLineId), null, null);
				}
			}
			else
			{
				matchedSupplierBookingLine = matchedSupplierBooking.SupplierBookingLines?.FirstOrDefault(line => line.JSL_JO_OrderLine == orderLine.PK);
				if (matchedSupplierBookingLine == null)
				{
					var orderData = orderDataObject?.Order;
					var orderLineData = orderData.OrderLineCollection[0];
					return (
						Res.GetString(
							"54f45f18-1138-44d4-b992-97249ff13a6e",
							"The supplier booking line related to order line {0}-{1} [{2}-{3}] does not exist on supplier booking {4}.",
							orderData.OrderNumber,
							orderData.OrderNumberSplit,
							orderLineData.LineNumber,
							orderLineData.SubLineNumber,
							supplierBookingKey),
						null,
						null
					);
				}
			}

			if (containerLoadList.CLH_JSB_Booking != matchedSupplierBooking.PK)
			{
				return (Res.GetString("a3a2f4eb-f023-4e13-9765-6132d455b40a", "The supplier booking {0} is different from supplier booking of Container Load List.", supplierBookingKey), null, null);
			}

			if (dataObject.PackingLineCollection?.Count != 1)
			{
				return (Res.GetString("5331ab98-2374-48d4-ba5f-992b826d2f3d", "should have only 1 pack line"), null, null);
			}

			var containerLink = GetContainerLink();
			if (containerLink == null || containerLink <= 0)
			{
				return (Res.GetString("f73584c7-85d5-482f-93d1-bcf3542a1d29", "Should provide container link for booking line {0}.", matchedSupplierBookingLine.JSL_BookingLineId), null, null);
			}

			var matchedContainer = containerLinkManager.GetContainer(containerLink.Value);
			if (matchedContainer == null)
			{
				return (Res.GetString("b1947711-5c10-456b-aabb-492f4f8bf8ba", "Could not find container by link {1} for booking line {0}.", matchedSupplierBookingLine.JSL_BookingLineId, containerLink.Value), null, null);
			}

			if (matchedContainer.JC_ContainerNum.IsEmpty)
			{
				return (Res.GetString("5ce181a4-2a73-4fe4-89fa-0c31df5438ba", "Allocated Container ({0}) must have container number.", containerLink.Value), null, null);
			}

			return(string.Empty, matchedSupplierBookingLine, matchedContainer);
		}

		OrderLine GetMatchedOrderLine(UniversalShipment orderData)
		{
			if ((orderData?.Order?.OrderLineCollection?.Count ?? 0) == 0)
			{
				return null;
			}

			var matchedOrder = new OrderDataObjectReader(orderData, logger, factory).TryGetExistingBusinessObject();
			if (matchedOrder == null)
			{
				return null;
			}

			return new OrderLineDataObjectReader(orderData.Order.OrderLineCollection[0], logger, factory, matchedOrder).TryGetExistingBusinessObject();
		}

		ZInt? GetContainerLink()
		{
			return dataObject.PackingLineCollection?.Single().ContainerLink;
		}

		ZString GetSupplierBookingLineId()
		{
			return dataObject.PackingLineCollection?.Single().PackingLineID ?? ZString.Empty;
		}

		ZString GetSupplierBookingKey()
		{
			return dataObject.SubShipmentCollection?.FirstOrDefault().GetMatchingDataSource(DataContextType.JobSupplierBooking)?.Key.GetValueOrDefault() ?? ZString.Empty;
		}
	}
}
