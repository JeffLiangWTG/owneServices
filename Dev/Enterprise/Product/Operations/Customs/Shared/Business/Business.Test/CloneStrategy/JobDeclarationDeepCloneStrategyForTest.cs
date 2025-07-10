using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationDeepCloneStrategyForTest : JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategyForTest(BaseJobDeclaration declarationToClone, CloneType cloneType) : base(declarationToClone, cloneType)
		{
		}

		protected override IEnumerable<JobDocAddress> CountrySpecificJobDocAddresses
		{
			get
			{
				yield return DeclarationToClone.DocAddresses.FindByDocAddressType(DocAddressType.BuyerDocumentaryAddress);
			}
		}

		public IEnumerable<JobDocAddress> CountrySpecificJobDocAddressesExposed => base.CountrySpecificJobDocAddresses;
	}
}
