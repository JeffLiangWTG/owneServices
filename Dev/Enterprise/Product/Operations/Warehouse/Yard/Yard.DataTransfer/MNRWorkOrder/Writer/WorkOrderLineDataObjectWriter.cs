using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using MNRWorkOrderLine = Enterprise.Warehouse.Yard.Business.MNRWorkOrderLine;
using UniversalMNRWorkOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.MNRWorkOrderLine;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class WorkOrderLineDataObjectWriter : DataObjectWriter<MNRWorkOrderLine, UniversalMNRWorkOrderLine>
	{
		public WorkOrderLineDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override UniversalMNRWorkOrderLine PopulateDataObject(MNRWorkOrderLine sourceBO)
		{
			var universalMNRWorkOrderLine = new UniversalMNRWorkOrderLine
			{
				ComponentCode = new CodeGroupPair { Code = sourceBO.ComponentCode.RCC_Code, Group = sourceBO.ComponentCode.RCC_Group },
				UnitSection = new CodeGroupPair { Code = sourceBO.UnitSection.RUS_Code, Group = sourceBO.UnitSection.RUS_Group },
				RepairCode = new CodeGroupPair { Code = sourceBO.RepairCode.RRC_Code, Group = sourceBO.RepairCode.RRC_Group },
				Material = new CodeGroupPair { Code = sourceBO.Material.RMC_Code, Group = sourceBO.Material.RMC_Group },
				Damage = new CodeGroupPair { Code = sourceBO.Damage.RFM_Code, Group = sourceBO.Damage.RFM_Group },
				UnitOfDimension = sourceBO.MWL_UnitOfDimension,
				ResponsibleParty = sourceBO.MWL_ResponsibleParty,
				Width = sourceBO.MWL_Width,
				Length = sourceBO.MWL_Length,
				MaterialQuantity = sourceBO.MWL_MaterialQuantity,
				LaborHours = sourceBO.MWL_LaborHours,
				Description = sourceBO.MWL_Description
			};

			return universalMNRWorkOrderLine;
		}
	}
}
