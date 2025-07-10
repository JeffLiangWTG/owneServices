using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExemptionOfControllingAgenciesCusSupportingCollection))]
	sealed class ExemptionOfControllingAgenciesCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ExemptionOfControllingAgenciesCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<ExemptionOfControllingAgenciesCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new ExemptionOfControllingAgenciesCusSupportingCollection(jobComInvoice);
		}

		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			var collection = (ExemptionOfControllingAgenciesCusSupportingCollection)GetCusSupportingInfoCollection();
			collection.AddNew();
			NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True);
			collection.AddNew();
			NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True);
			collection.AddNew();
			NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True);
			collection.AddNew();
			NUnit.Framework.Assert.That(collection.AllowNew, NUnit.Framework.Is.True);
			collection.AddNew();
			NUnit.Framework.Assert.That(!collection.AllowNew, NUnit.Framework.Is.True);
		}
	}
}
