using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	sealed class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(JobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedResult = (JobDeclaration)base.CloneInternal(args);
			clonedResult.JE_GoodsNumber = DeclarationToClone.JE_GoodsNumber;
			clonedResult.JE_Position = DeclarationToClone.JE_Position;
			return clonedResult;
		}

		new JobDeclaration DeclarationToClone => (JobDeclaration)base.DeclarationToClone;
	}
}
