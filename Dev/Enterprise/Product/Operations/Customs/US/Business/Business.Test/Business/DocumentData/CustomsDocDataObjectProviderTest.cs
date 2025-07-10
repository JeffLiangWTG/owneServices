using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CustomsDocDataObjectProvider))]
	sealed class CustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomsDocDataObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceline = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceline.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atfLine = invoiceline.ATFLines.AddNew();
			atfLine.US_PermitNumber = "123";

			var parameters = new Mock<IDocDataObjectParameters>();
			parameters.SetupGet(x => x.Data).Returns(new ZString[] { "123" });

			var customsDocDataObjectProvider = new CustomsDocDataObjectProvider();
			var customsDocDataObject = customsDocDataObjectProvider.GetDocDataObject(declaration, DataContext.USATF6A, parameters.Object);
			AssertNotNull(customsDocDataObject);
			AssertType<ATF6ADocDataObject>(customsDocDataObject);
		}
	}
}
