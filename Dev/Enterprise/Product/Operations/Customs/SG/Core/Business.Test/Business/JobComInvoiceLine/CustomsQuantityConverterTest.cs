using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CustomsQuantityConverterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			new CustomsQuantityConverter(invoiceLine, (ZPropertyInfoDecimal)invoiceLine.JI_CustomsQuantityInfo, (ZPropertyInfoString)invoiceLine.JI_CustomsUnitQtyInfo);
		}
	}
}
