using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class PreArrivalDataObjectWriter : TopLevelDataObjectWriter<CYDReceiveAdvice, Shipment>
	{
		public PreArrivalDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.CYDReceiveAdvice;

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override void PopulateDataObject(CYDReceiveAdvice sourceBO, Shipment dataObject)
		{
			ValidateReceiveAdvice(sourceBO);
			PopulateContainerCollection(sourceBO, dataObject);
			PopulateOrganizationAddressCollection(sourceBO, dataObject);
			PopulateDateCollection(sourceBO, dataObject);
			populateBookingConfirmationReference(sourceBO, dataObject);
		}

		void ValidateReceiveAdvice(CYDReceiveAdvice sourceBO)
		{
			var deliveryQuery = new ZDBOnlyQuery(typeof(CYDDelivery));
			var receiveAdviceLineQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdviceLine), CYDReceiveAdviceLineSchema.PK);
			receiveAdviceLineQuery.AddToFilter(CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice, SQLComparisonOperator.Equal, sourceBO.PK);
			deliveryQuery.AddSubQuery(CYDDeliverySchema.YDL_YRL_ReceiveAdviceLine, CYDReceiveAdviceLineSchema.PK, receiveAdviceLineQuery, JoinCondition.And);

			var deliveries = sourceBO.Factory.Load<CYDDelivery>(deliveryQuery);
			if (deliveries.Any())
			{
				throw new DataObjectValidationException("The PRA is associated with one or more deliveries.");
			}
		}

		void populateBookingConfirmationReference(CYDReceiveAdvice sourceBO, Shipment dataObject)
		{
			dataObject.BookingConfirmationReference = sourceBO.YRA_AcceptanceNumber;
		}

		void PopulateDateCollection(CYDReceiveAdvice sourceBO, Shipment dataObject)
		{
			var dateCollection = dataObject.DateCollection ?? new List<Date>();
			dateCollection.Add(DateType.Start, ZBool.True, sourceBO.YRA_FromDate);
			dateCollection.Add(DateType.End, ZBool.True, sourceBO.YRA_ToDate);
			dataObject.SetDateCollection(() => dateCollection);
		}

		void PopulateContainerCollection(CYDReceiveAdvice sourceBO, Shipment dataObject)
		{
			var yardUnitStateQuery = new ZDBOnlyQuery(typeof(CYDYardUnitState));
			var receiveAdviceLineQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdviceLine), CYDReceiveAdviceLineSchema.PK);
			receiveAdviceLineQuery.AddToFilter(CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice, SQLComparisonOperator.Equal, sourceBO.PK);
			yardUnitStateQuery.AddSubQuery(CYDYardUnitStateSchema.YUS_YRL_ReceiveLine, CYDReceiveAdviceLineSchema.PK, receiveAdviceLineQuery, JoinCondition.And);

			var yardUnits = sourceBO.Factory.Load<CYDYardUnitState>(yardUnitStateQuery);

			var containers = new DataObjectList<Container>();
			foreach (var yardUnit in yardUnits)
			{
				var unitLineItem = yardUnit.ReceiveAdviceLine.UnitLineItem;
				if (unitLineItem.YLI_Type == "CNT")
				{
					var container = new Container(DefaultDataObjectWriterStrategy.Instance)
					{
						ContainerNumber = yardUnit.YUS_UnitID,
						ContainerCount = unitLineItem.YLI_Quantity,
						IsEmptyContainer = unitLineItem.YLI_IsEmpty,
						ContainerType = new ContainerType
						{
							Code = unitLineItem.ContainerType.RC_Code
						}
					};
					container.SetAdditionalSealNumberCollection(() => new List<SealNumber>
					{
						new SealNumber
						{
							Number = unitLineItem.YLI_SealNumber
						}
					});
					containers.Add(container);
				}
			}

			dataObject.SetSubShipmentCollection(() =>
			{
				var subShipment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
				subShipment.SetRelatedShipmentCollection(() =>
				{
					var relatedShipment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
					relatedShipment.SetContainerCollection(() => containers);
					return new List<Shipment> { relatedShipment };
				});
				return new DataObjectList<Shipment> { subShipment };
			});
		}

		void PopulateOrganizationAddressCollection(CYDReceiveAdvice receiveAdvice, Shipment shipment)
		{
			var yardOrganizationAddress = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageYard)).GetDataObject(receiveAdvice.Yard.WarehouseAddress);
			shipment.SetOrganizationAddressCollection(() => ProcessCollection(receiveAdvice.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)).AddSafe(yardOrganizationAddress));
		}
	}
}
