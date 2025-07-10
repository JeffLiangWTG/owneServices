using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class USJobDeclarationDeepCloneStrategy : JobDeclarationDeepCloneStrategy
	{
		public USJobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType) : base(declarationToClone, cloneType)
		{
		}

		public USJobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn) : base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		public USJobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, ZGuid jE_GBForCloneResult) : base(declarationToClone, cloneType, jE_GBForCloneResult)
		{
		}

		public USJobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, ZGuid jE_GBForCloneResult, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn) : base(declarationToClone, cloneType, jE_GBForCloneResult, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override void DeepCopyNotesCore(BaseJobDeclaration clonedResult, StmNoteCollection notes)
		{
			//do not copy Notes in US JobDeclaration
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone,
			BaseJobDeclaration clonedDeclaration)
		{
			return new USJobComInvoiceHeaderDeepCloneStrategy(invoiceToClone, cloneType, clonedDeclaration, pkPairsDictionaryCollection);
		}

		protected override void DeepCopyCountrySpecificDataCore(BaseJobDeclaration clonedResult)
		{
			var usDeclaration = (JobDeclaration)DeclarationToClone;
			if (usDeclaration.Shipment == null && usDeclaration.InBondHeader is CusInBondHeader originalHeader)
			{
				var copiedTemplate = ((ITemplateCopyable)originalHeader).TemplateCopy();
				var newHeader = (CusInBondHeader)copiedTemplate;
				newHeader.BH_ParentID = clonedResult.PK;
				newHeader.BH_ParentTableCode = clonedResult.TablePrefix;
			}
		}
	}
}
