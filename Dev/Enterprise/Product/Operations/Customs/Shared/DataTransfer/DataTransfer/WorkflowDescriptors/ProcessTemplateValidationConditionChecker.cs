using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business;

public sealed class ProcessTemplateValidationConditionChecker : MasterFiles.Business.ProcessTemplateValidationConditionChecker
{
	readonly JobDeclarationProcessTaskConditionChecker conditionChecker;

	public ProcessTemplateValidationConditionChecker(IProcessTemplateValidation validationRule, BaseJobDeclaration declaration) : base(validationRule, declaration)
	{
		conditionChecker = new JobDeclarationProcessTaskConditionChecker(declaration);
	}

	new BaseJobDeclaration BusinessEntity => (BaseJobDeclaration)base.BusinessEntity;

	protected override bool IsCondition1Met(ZString conditionCode) => conditionChecker.IsCondition1Met(conditionCode);

	protected override bool IsCondition2MetCore(ZString conditionCode, ZString value) => conditionChecker.IsCondition2Met(conditionCode, value);

	protected override bool IsCompanyMet(ZGuid company) => BusinessEntity.CompanyPK == company;
}
