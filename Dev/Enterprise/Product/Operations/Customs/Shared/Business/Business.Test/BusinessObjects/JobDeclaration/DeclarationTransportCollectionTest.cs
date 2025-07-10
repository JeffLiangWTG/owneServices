using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DeclarationTransportCollection))]
	sealed class DeclarationTransportCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRemovingTransport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var transport1 = declaration.Transports.AddNew();
			var transport2 = declaration.Transports.AddNew();
			AssertEquals("Should have 2 transports", 2, declaration.Transports.Count);

			declaration.Transports.RemoveAndDelete(transport1);
			AssertEquals("Should have 1 transport", 1, declaration.Transports.Count);

			transport2 = declaration.Transports.AddNew();
			AssertEquals("Should have 2 transports", 2, declaration.Transports.Count);

			declaration.Transports.RemoveAndDeleteAll();
			AssertEquals("Should have 0 transports", 0, declaration.Transports.Count);
		}

		public void TestDefaultingLegOrder()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var transport1 = declaration.Transports.AddNew();
			AssertEquals("The first number issued should be 1", 1, (int)transport1.JW_LegOrder);

			var transport2 = declaration.Transports.AddNew();
			AssertEquals("The second number issued should be 2", 2, (int)transport2.JW_LegOrder);

			var transport3 = declaration.Transports.AddNew();
			AssertEquals("The third number issued should be 3", 3, (int)transport3.JW_LegOrder);

			declaration.Transports.RemoveAndDelete(transport2);
			transport2 = declaration.Transports.AddNew();
			AssertEquals("Don't fill gaps, the new leg should always have the heighest value", 4, (int)transport2.JW_LegOrder);

			declaration.Transports.RemoveAndDelete(transport3);
			declaration.Transports.RemoveAndDelete(transport2);
			transport2 = declaration.Transports.AddNew();
			AssertEquals("Use the smallest value greater than all the rest, don't leave unnecessary gaps", 2, (int)transport2.JW_LegOrder);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<BaseJobDeclaration>().Transports;
	}
}
