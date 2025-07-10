using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackableItemCollection))]
	sealed class CusPackableItemCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPackableItemCollection>
	{
		protected override CusPackableItemCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			Factory.Save();
			var packingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(Factory);
			return packingList.PackableItems;
		}
	}
}
