using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	abstract class CustomsUnitDefaultingStrategyTest : TestCaseWithFactory
	{
		public abstract void TestDefaultUOMs();

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			invoiceLineMock.Protected().Setup<ZString>("DefaultDataGroupingForTaxOrFee").Returns(GlbCompany.CurrentCompany.Country.Code);
		}

		protected Mock<BaseJobComInvoiceLine> invoiceLineMock;

		protected BaseJobComInvoiceLine InvoiceLine => invoiceLineMock.Object;
	}
}
