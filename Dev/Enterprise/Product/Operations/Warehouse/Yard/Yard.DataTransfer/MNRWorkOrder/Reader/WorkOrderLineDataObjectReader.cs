using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using MNRWorkOrderLine = Enterprise.Warehouse.Yard.Business.MNRWorkOrderLine;
using UniversalMNRWorkOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.MNRWorkOrderLine;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class WorkOrderLineDataObjectReader : DataObjectReader<UniversalMNRWorkOrderLine, MNRWorkOrderLine>
	{
		public WorkOrderLineDataObjectReader(UniversalMNRWorkOrderLine dataObject, MNRWorkOrderHeader workOrderHeader, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			this.workOrderHeader = workOrderHeader;
		}

		readonly MNRWorkOrderHeader workOrderHeader;

		protected override MNRWorkOrderLine GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(MNRWorkOrderLine workOrderLine)
		{
			var componentCode = GetRefData<RefMRComponentCode>(dataObject.ComponentCode, RefMRComponentCodeSchema.RCC_Code, RefMRComponentCodeSchema.RCC_Group);
			var unitSection = GetRefData<RefUnitSection>(dataObject.UnitSection, RefUnitSectionSchema.RUS_Code, RefUnitSectionSchema.RUS_Group);
			var repairCode = GetRefData<RefRepairCode>(dataObject.RepairCode, RefRepairCodeSchema.RRC_Code, RefRepairCodeSchema.RRC_Group);
			var material = GetRefData<RefMaterial>(dataObject.Material, RefMaterialSchema.RMC_Code, RefMaterialSchema.RMC_Group);
			var damage = GetRefData<RefDamage>(dataObject.Damage, RefDamageSchema.RFM_Code, RefDamageSchema.RFM_Group);

			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_MWO_MNRWorkOrderHeader, workOrderHeader.PK);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_RCC_ComponentCode, componentCode.PK);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_RUS_UnitSection, unitSection.PK);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_RRC_RepairCode, repairCode.PK);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_RMC_Material, material.PK);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_RFM_Damage, damage.PK);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_UnitOfDimension, dataObject.UnitOfDimension);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_ResponsibleParty, dataObject.ResponsibleParty);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_Width, dataObject.Width);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_Length, dataObject.Length);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_MaterialQuantity, dataObject.MaterialQuantity);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_LaborHours, dataObject.LaborHours);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_IsActive, true);
			SetValue(workOrderLine, MNRWorkOrderLineSchema.MWL_Description, dataObject.Description);
		}

		T GetRefData<T>(CodeGroupPair codeAndGroup, SchemaStringColumn code, SchemaStringColumn group) where T : BusinessObject
		{
			var filter = new ZQuery().AddToFilter(code, codeAndGroup.Code).AddToFilter(group, codeAndGroup.Group);
			return factory.LoadTop1<T>(filter);
		}
	}
}
