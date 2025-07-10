using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class SetUpDefaultsForNewChildInvoiceCollectionTest : TestCaseWithFactory
	{
		public void TestDefaultsForAdditionalInvoiceCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var firstInvoice = declaration.Invoices.AddNew();
			AssertEquals(firstInvoice.RelatedIndicator, ZBool.False);
			firstInvoice.JZ_RelatedIndicator = RelationCodeList.Codes.Y;

			var secondInvoice = declaration.Invoices.AddNew();
			AssertEquals(secondInvoice.JZ_RelatedIndicator, RelationCodeList.Codes.Y);
			secondInvoice.JZ_RelatedIndicator = RelationCodeList.Codes.N;

			var thirdInvoice = declaration.Invoices.AddNew();
			AssertEquals(thirdInvoice.JZ_RelatedIndicator, RelationCodeList.Codes.N);
		}
	}
}
