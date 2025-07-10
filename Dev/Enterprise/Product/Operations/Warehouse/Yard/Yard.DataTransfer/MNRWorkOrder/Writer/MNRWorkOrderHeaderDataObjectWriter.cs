using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalMNRWorkOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.MNRWorkOrderLine;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class MNRWorkOrderHeaderDataObjectWriter : TopLevelDataObjectWriter<MNRWorkOrderHeader, Shipment>
	{
		public MNRWorkOrderHeaderDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.MNRWorkOrder;

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override void PopulateDataObject(MNRWorkOrderHeader sourceBO, Shipment dataObject)
		{
			PopulateContainer(sourceBO, dataObject);
		}

		void PopulateContainer(MNRWorkOrderHeader sourceBO, Shipment dataObject)
		{
			var yardUnitState = sourceBO.Factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.PK, sourceBO.MWO_ParentID));
			if (yardUnitState == null)
			{
				return;
			}

			dataObject.MNRStartEquipmentGrade = new CodeDescriptionPair { Code = sourceBO.StartEquipmentGrade.REG_Code };
			dataObject.MNREndEquipmentGrade = new CodeDescriptionPair { Code = sourceBO.EndEquipmentGrade.REG_Code };
			dataObject.MNRType = new CodeDescriptionPair { Code = sourceBO.MWO_Type };
			dataObject.MNRRevision = sourceBO.MWO_Revision;
			dataObject.MNRWorkOrderApprovedTime = sourceBO.MWO_WorkOrderApprovedTime;

			var universalMNRWorkOrderLines = new DataObjectList<UniversalMNRWorkOrderLine>(ProcessCollection(sourceBO.WorkOrderLines, new WorkOrderLineDataObjectWriter(writeManager)));
			dataObject.SetMNRWorkOrderLineCollection(() => universalMNRWorkOrderLines);

			dataObject.SetContainerCollection(() =>
			{
				var containers = new DataObjectList<Container>();
				var container = new Container(DefaultDataObjectWriterStrategy.Instance)
				{
					ContainerNumber = yardUnitState.YUS_UnitID,
					ContainerJobID = sourceBO.MWO_JobNumber,
				};

				containers.Add(container);
				return containers;
			});
		}
	}
}
