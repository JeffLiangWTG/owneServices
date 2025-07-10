using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PermitCusSupportingCollection))]
	sealed class PermitCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PermitCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<PermitCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new PermitCusSupportingCollection(jobComInvoice);
		}

		[ExpectNoExceptions]
		public void TestAllowNewCore()
		{
			var testCollection = new PermitCusSupportingCollection(Factory.New<JobDeclaration>().InvoiceLines.AddNew());
			for (var time = 0; time < 4; time++)
			{
				testCollection.AddNew();
				NUnit.Framework.Assert.That(testCollection.AllowNew, NUnit.Framework.Is.True, "Should allow new when before more than 5 items");
			}

			testCollection.AddNew();
			NUnit.Framework.Assert.That(!testCollection.AllowNew, NUnit.Framework.Is.True, "Should NOT allow new when after 5th item.");
		}
	}
}
