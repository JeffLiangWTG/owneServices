using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class MNRWorkOrderHeaderDataObjectReader : ContainerYardDataObjectReader<MNRWorkOrderHeader>
	{
		public MNRWorkOrderHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.MNRWorkOrder;

		protected override IMatchingBusinessEntityFinder<MNRWorkOrderHeader> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override MNRWorkOrderHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override void PopulateBusinessObject(MNRWorkOrderHeader workOrderHeader)
		{
			var container = dataObject.ContainerCollection.FirstOrDefault() ?? throw new DataObjectReadFailureException("Container details are missing from UXML.");
			var yardUnitState = GetYardUnitState(container.ContainerNumber);

			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_REG_StartEquipmentGrade, GetRefEquipmentGrade(dataObject.MNRStartEquipmentGrade.Code).PK);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_REG_EndEquipmentGrade, GetRefEquipmentGrade(dataObject.MNREndEquipmentGrade.Code).PK);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_WorkOrderApprovedTime, dataObject.MNRWorkOrderApprovedTime);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_Type, dataObject.MNRType.Code);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_ParentID, yardUnitState.PK);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_WW_Facility, yardUnitState.YUS_WW_CurrentYard);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_Revision, dataObject.MNRRevision);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_IsEstimateCompleted, GetIsEstimateCompleted(workOrderHeader));
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_ParentTableCode, CYDYardUnitStateSchema.Constants.Prefix);
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_JobNumber, GetJobNumber(workOrderHeader));
			SetValue(workOrderHeader, MNRWorkOrderHeaderSchema.MWO_IsActive, true);

			if (dataObject.MNRWorkOrderLineCollection.Any())
			{
				DeleteExsitLines(workOrderHeader);
				var workOrderLineCollectionDataObjectReader = new WorkOrderLineCollectionDataObjectReader(dataObject.MNRWorkOrderLineCollection, workOrderHeader, logger, factory);
				workOrderLineCollectionDataObjectReader.ReadIntoCollection();
			}
			else
			{
				throw new DataObjectReadFailureException("WorkOrder lines are missing from UXML.");
			}
		}

		string GetJobNumber(MNRWorkOrderHeader workOrderHeader)
		{
			return IsNewBO ? new MNRWorkOrderJobNumberStrategy(factory.BOFactory).GetJobNumber() : workOrderHeader.JobNumber;
		}

		CYDYardUnitState GetYardUnitState(string containerNumber)
		{
			return factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, containerNumber));
		}

		ZBool GetIsEstimateCompleted(MNRWorkOrderHeader workOrderHeader)
		{
			return IsNewBO ? true : workOrderHeader.MWO_IsEstimateCompleted;
		}

		RefEquipmentGrade GetRefEquipmentGrade(string code)
		{
			return factory.LoadTop1<RefEquipmentGrade>(new ZQuery(RefEquipmentGradeSchema.REG_Code, code));
		}

		void DeleteExsitLines(MNRWorkOrderHeader workOrderHeader)
		{
			foreach (var line in workOrderHeader.WorkOrderLines)
			{
				if (line.MWL_IsActive && !line.MWL_TaskStartTime.IsEmpty)
				{
					throw new DataObjectReadFailureException("At least one of the lines related to this work order has started the task.");
				}
			}
			workOrderHeader.WorkOrderLines.DeleteAll();
		}
	}
}
