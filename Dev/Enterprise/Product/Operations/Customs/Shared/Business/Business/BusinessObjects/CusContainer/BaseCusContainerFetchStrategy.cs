using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class BaseCusContainerFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public BaseCusContainerFetchStrategy(BaseCusContainer container)
			: base(container)
		{
		}

		protected new BaseCusContainer BusinessObject
		{
			get { return (BaseCusContainer)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(BaseJobDeclaration), BusinessObject.CO_JE);
			Factory.AddFetchHint(JobContainerSchema.PK, BusinessObject.CO_JC);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(RefContainerSchema.PK, BusinessObject.CO_RC);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			if (BusinessObject.Declaration?.SupportContainerEntryHeaderPivot ?? false)
			{
				Factory.AddFetchHint(CusContainerEntryHeaderPivotSchema.CCE_CO_Container, BusinessObject.PK);
			}
			if (BusinessObject.Declaration?.SupportContainerEntryInstructionPivot ?? false)
			{
				Factory.AddFetchHint(CusContainerEntryInstructionPivotSchema.CEP_CO_Container, BusinessObject.PK);
			}
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(JobPickupDeliveryConfirmSchema.EU_JC, BusinessObject.CO_JC);
		}
	}
}
