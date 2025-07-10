using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer;

public sealed class ProcessTemplateValidationConditionChecker : MasterFiles.Business.ProcessTemplateValidationConditionChecker
{
	readonly ForwardingShipmentProcessTaskConditionChecker conditionChecker;

	public ProcessTemplateValidationConditionChecker(IProcessTemplateValidation validationRule, ForwardingShipment shipment) : base(validationRule, shipment)
	{
		conditionChecker = new ForwardingShipmentProcessTaskConditionChecker(ValidationRule.Factory, shipment);
	}

	new ForwardingShipment BusinessEntity => (ForwardingShipment)base.BusinessEntity;

	protected override bool IsCondition1Met(ZString conditionCode) => IsBrokerageAttached
		? EntityToEvaluateMacro is Enterprise.Integration.Customs.IBaseJobDeclaration
		: conditionChecker.IsCondition1Met(conditionCode);

	protected override bool IsCondition2MetCore(ZString conditionCode, ZString value) => conditionChecker.IsCondition2Met(conditionCode, value);

	protected override bool IsCompanyMet(ZGuid company) => !IsBrokerageAttached || (BusinessEntity.GetDeclarationFor(company) is not null);

	protected override IBusiness EntityToEvaluateMacro
	{
		get
		{
			if (!IsBrokerageAttached)
			{
				return BusinessEntity;
			}

			var company = ValidationRule.P0V_GC_Company.IfEmptyUse(() => ValidationRule.P0_GC);
			return BusinessEntity.GetDeclarationFor(company);
		}
	}

	bool IsBrokerageAttached => ValidationRule.P0V_Condition1 == JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached;
}
