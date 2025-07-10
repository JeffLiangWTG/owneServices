using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ProductInformationValidation))]
	sealed class ProductInformationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSendFrom()
		{
			ProductInfo.SendFrom = "123@email.address";
			AssertNoErrors(ProductInfo.SendFromInfo);

			ProductInfo.SendFrom = ZString.Empty;
			AssertHasError(ProductInfo.SendFromInfo, "You have not entered a Send From.");

			ProductInfo.SendFrom = "123";
			AssertHasError(ProductInfo.SendFromInfo, "Email Address is not valid .");
		}

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration => declaration ??= ProductInformationTest.CreateTestDeclaration(Factory);

		BaseJobComInvoiceLine[] InvoiceLines => Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().ToArray();

		ProductInformation productInfo;
		ProductInformation ProductInfo => productInfo ??= new ProductInformation(Factory, Declaration, InvoiceLines);
	}
}
