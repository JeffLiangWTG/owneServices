using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class CusEntryInstructionFetchStrategy : EnterpriseBusinessObjectFetchStrategy, IDocumentSupporterFetchStrategy
	{
		public CusEntryInstructionFetchStrategy(CusEntryInstruction parent) : base(parent) { }

		protected new CusEntryInstruction BusinessObject
		{
			get { return (CusEntryInstruction)base.BusinessObject; }
		}

		void IDocumentSupporterFetchStrategy.AddDocumentSupporterFetchHints()
		{
			AddDocumentSupporterFetchHintsForCusEntryInstruction();
		}

		protected virtual void AddDocumentSupporterFetchHintsForCusEntryInstruction()
		{
			var instructionPk = BusinessObject.PK;

			var externalFetchHintSupporter = (IExternalFetchHintSupporter)BusinessObject.Factory;
			externalFetchHintSupporter.AddFetchHint(new FetchHint(JobDocAddressSchema.E2_ParentID, instructionPk));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusAuthorizationUsageSchema.AGC_ParentID, instructionPk));
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusAuthorizationUsageSchema.AGC_ParentID, BusinessObject.PK);
			if (BusinessObject.JobDeclaration?.SupportContainerEntryInstructionPivot ?? false)
			{
				Factory.AddFetchHint(CusContainerEntryInstructionPivotSchema.CEP_CEI_EntryInstruction, BusinessObject.PK);
			}
		}
	}
}
