using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackableItem))]
	sealed class CusPackableItemTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCusPackingListType()
		{
			var packableItem = (CusPackableItem)GeNewBusinessObject(Factory);
			NUnit.Framework.Assert.That(packableItem.PackingList, NUnit.Framework.Is.TypeOf<CusPackingList>());
		}

		protected override BusinessObject GetNewBusinessObject() => GeNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GeNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GeNewBusinessObject(Factory);

		BusinessObject GeNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "line4";
			invoiceLine.JI_InvoiceQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "ACR";
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackableItem = cusPackingList.PackableItems.AddNew();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_ClusterKey = 1;
			return cusPackableItem;
		}
	}
}
