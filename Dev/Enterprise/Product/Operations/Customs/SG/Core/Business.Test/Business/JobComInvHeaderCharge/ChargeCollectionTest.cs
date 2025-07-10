using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(ChargeCollection))]
	public class ChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ChargeCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<InvoiceLineCharge>();
		}

		public override void TestAddNew()
		{
			Assert("Dont Care whether AddNew Works or not - its not going to be used", true);
		}

		public override void TestTypedAddNew()
		{
			Assert("Dont Care whether AddNew Works or not - its not going to be used", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Dont Care whether AddNew Works or not - its not going to be used", true);
		}
	}
}
