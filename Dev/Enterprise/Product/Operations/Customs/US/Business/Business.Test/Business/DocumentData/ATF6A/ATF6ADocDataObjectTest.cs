using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ATF6ADocDataObject))]
	sealed class ATF6ADocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ATF6ADocDataObject();
		}
	}

	[TestedType(typeof(ATFGroupDocDataObject))]
	sealed class ATFGroupDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ATFGroupDocDataObject();
		}
	}

	[TestedType(typeof(ATFDetailDocDataObject))]
	sealed class ATFDetailDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ATFDetailDocDataObject();
		}
	}
}
