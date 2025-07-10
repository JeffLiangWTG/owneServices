using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(MultiJobDeclarationCollection))]
	sealed class MultiJobDeclarationCollectionTestCase : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration declaration = factory2.New<BaseJobDeclaration>();
			return new MultiJobDeclarationCollection(declaration, Factory);
		}

		public void TestSetDefaultsForNewChild()
		{
			BaseJobDeclaration declaration = new BusinessObjectFactory().New<BaseJobDeclaration>();
			declaration.JE_TransportMode = "ABC";
			declaration.JE_MessageType = "XYZ";
			MultiJobDeclarationCollection collection = new MultiJobDeclarationCollection(declaration, Factory);
			BaseJobComInvoiceLine invoiceLine = collection.AddNew();
			AssertNotNull(invoiceLine.InvoiceHeader);
			AssertNotNull(invoiceLine.Declaration);
			AssertEquals("ABC", invoiceLine.Declaration.JE_TransportMode);
			AssertEquals("XYZ", invoiceLine.Declaration.JE_MessageType);

			invoiceLine.Declaration.JE_RL_NKOrigin = "NZAKL";

			BaseJobComInvoiceLine invoiceLine2 = collection.AddNew();
			AssertEquals("NZAKL", invoiceLine2.Declaration.JE_RL_NKOrigin);

			declaration.JE_TransportMode = "SEA";
			AssertEquals("SEA", invoiceLine.Declaration.JE_TransportMode);
			AssertEquals("SEA", invoiceLine2.Declaration.JE_TransportMode);

			declaration.JE_MessageType = "IMP";
			AssertEquals("IMP", invoiceLine.Declaration.JE_MessageType);
			AssertEquals("IMP", invoiceLine2.Declaration.JE_MessageType);
		}
	}
}
