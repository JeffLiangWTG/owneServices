using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExemptionOfControllingAgenciesCusSupportingLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestSpecialCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var exemptionOfControllingAgenciesCusSupporting = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			NUnit.Framework.Assert.That(exemptionOfControllingAgenciesCusSupporting.Lookups.SpecialCodeList, NUnit.Framework.Is.EqualTo(ExemptionOfControllingAgenciesCusSupporting.GetSpecialCodeList(Factory)));
		}
	}
}
