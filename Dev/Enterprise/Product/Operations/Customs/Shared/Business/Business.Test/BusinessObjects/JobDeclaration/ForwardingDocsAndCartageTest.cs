using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ForwardingDocsAndCartage))]
	sealed class ForwardingDocsAndCartageTest : Freight.Forwarding.Business.Testing.ForwardingDocsAndCartageBOTest
	{
		public void TestGetNewValidation() => AssertType<ForwardingDocsAndCartageValidation>(docsAndCartage.Validation);

		protected override BusinessObject GetNewBusinessObject() => docsAndCartage;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => docsAndCartage;

		protected override BusinessObject GetLogParentForEventDateProperty() => declaration;

		protected override void SetUp()
		{
			declaration = Factory.New<BaseJobDeclaration>();
			docsAndCartage = declaration.DocsAndCartage;
		}
		JobDocsAndCartage docsAndCartage;
		BaseJobDeclaration declaration;
	}
}
