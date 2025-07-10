using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocEntryHeaderCommercialChargesCollection))]
	sealed class DocEntryHeaderCommercialChargesCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocEntryHeaderCommercialChargesCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			var apportionedCharge = jobComInvoiceLine.ApportionedCharges.AddNew();
			return DocEntryHeaderCommercialCharge.New(apportionedCharge, Factory);
		}

		protected override DocEntryHeaderCommercialChargesCollection GetCollectionToTest()
		{
			return new DocEntryHeaderCommercialChargesCollection(Factory);
		}
	}
}
