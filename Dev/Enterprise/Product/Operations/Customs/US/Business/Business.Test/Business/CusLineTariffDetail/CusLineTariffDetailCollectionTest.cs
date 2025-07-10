using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetailCollection))]
	sealed class CusLineTariffDetailCollectionTest : Customs.Business.Testing.CusLineTariffDetailCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => CusLineTariffDetailCollection;

		CusLineTariffDetailCollection cusLineTariffDetailCollection;
		CusLineTariffDetailCollection CusLineTariffDetailCollection => cusLineTariffDetailCollection ?? (cusLineTariffDetailCollection = new CusLineTariffDetailCollection(Factory.New<JobComInvoiceLine>()));
	}
}
