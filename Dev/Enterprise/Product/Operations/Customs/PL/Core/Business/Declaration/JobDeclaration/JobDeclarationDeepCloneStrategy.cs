using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobDeclarationDeepCloneStrategy : EU.Business.Declaration.JobDeclarationDeepCloneStrategy
{
	public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
	{
	}

	protected override Customs.Business.JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
	{
		return new JobComInvoiceHeaderDeepCopyStrategy((JobComInvoiceHeader)invoiceToClone, cloneType, (JobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
	}
}
