using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAJobDeclarationDeepCloneStrategy : JobDeclarationDeepCloneStrategy
	{
		public ZAJobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn) : base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override Customs.Business.JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new JobComInvoiceHeaderDeepCopyStrategy((JobComInvoiceHeader)invoiceToClone, cloneType, (JobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
		}

		protected override Customs.Business.CusEntryInstructionDeepCloneStrategy GetCusEntryInstructionDeepCloneStrategy(Customs.Business.CusEntryInstruction cusEntryInstructionToClone)
																										 => new CusEntryInstructionDeepCloneStrategy(cusEntryInstructionToClone, CloneType.DeepTemplateCopy, alternativeFactoryToInstantiateCloneIn);
	}
}
