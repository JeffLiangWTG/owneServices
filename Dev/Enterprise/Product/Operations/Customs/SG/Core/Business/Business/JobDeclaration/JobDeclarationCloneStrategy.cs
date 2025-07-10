using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationCloneStrategy : JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationCloneStrategy(JobDeclaration decToClone, CloneType cloneType, BusinessObjectFactory alternateFactory)
			: base(decToClone, cloneType, alternateFactory)
		{
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new JobComInvoiceHeaderCloneStrategy((JobComInvoiceHeader)invoiceToClone, cloneType, (JobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
		}
	}
}
