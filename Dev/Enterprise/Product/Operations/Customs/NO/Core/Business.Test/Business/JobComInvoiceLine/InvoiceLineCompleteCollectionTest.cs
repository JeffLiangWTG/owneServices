using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	sealed class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

		protected override BaseJobDeclaration GetMeANewJobDeclaration() => Factory.New<JobDeclaration>();

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
