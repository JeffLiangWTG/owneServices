using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.ExcelTemplates.Integration;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationUSVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetAdditionalData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceline = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceline.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atfLine = invoiceline.ATFLines.AddNew();
			atfLine.US_PermitNumber = "123";

			var template = new Mock<IStmTemplate>();
			template.SetupGet(x => x.SO_DataContext).Returns(DataContext.USATF6A);
			var pivot = new Mock<IStmMenuTemplatePivot>();
			pivot.SetupGet(x => x.Template).Returns(template.Object);
			var menu = new Mock<IStmMenuItem>();
			menu.SetupGet(x => x.Documents).Returns(new[] { pivot.Object });

			var supporter = new JobDeclarationUSVisualizableDocumentSupporter(declaration);
			var additionalData = supporter.GetAdditionalData(declaration, menu.Object);
			var result = additionalData.Right as ZString[];
			AssertContainsExactElementsInAnyOrder(new ZString[] { "123" }, result);
		}

		public void TestCustomizeFormCheckpoint()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = new JobDeclarationUSVisualizableDocumentSupporter(declaration);
			AssertEquals("CustomizeFormCheckpoint", Env.Security.MaintainJobDeclarationCustomiseForms, supporter.CustomizeFormCheckpoint);
		}
	}
}
