using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ContainerToSelectFromForPrintingCollection))]
	sealed class ContainerToSelectFromForPrintingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ContainerToSelectFromForPrintingCollection>
	{
		protected override ContainerToSelectFromForPrintingCollection GetCollectionToTest()
		{
			return new ContainerToSelectFromForPrintingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContainerToSelectFromForPrinting(Factory.New<CommonContainer>());
		}

		public void TestCreateNonPersistentBusinessObjectNotSupported()
		{
			ContainerToSelectFromForPrintingCollection collection = new ContainerToSelectFromForPrintingCollection(Factory);
			AssertExceptionThrown(typeof(NotSupportedException), () => collection.AddNew());
		}
	}
}
