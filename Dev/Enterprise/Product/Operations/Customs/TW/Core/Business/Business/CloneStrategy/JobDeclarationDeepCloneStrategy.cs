using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToClone, Customs.Business.CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override Customs.Business.CusEntryInstructionDeepCloneStrategy GetCusEntryInstructionDeepCloneStrategy(Customs.Business.CusEntryInstruction cusEntryInstructionToClone)
		{
			return new CusEntryInstructionDeepCloneStrategy(cusEntryInstructionToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		protected override Customs.Business.JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(Customs.Business.BaseJobComInvoiceHeader invoiceToClone, Customs.Business.BaseJobDeclaration clonedDeclaration)
		{
			return new JobComInvoiceHeaderDeepCloneStrategy(invoiceToClone, cloneType, clonedDeclaration, pkPairsDictionaryCollection);
		}
	}
}
