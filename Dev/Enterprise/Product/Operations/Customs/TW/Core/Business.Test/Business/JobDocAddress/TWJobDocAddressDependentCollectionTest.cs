using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWJobDocAddressDependentCollection))]
	sealed class TWJobDocAddressDependentCollectionTest : JobDocAddressDependentCollectionTest
	{
		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TWJobDocAddressDependentCollection(Factory.New<JobDeclaration>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TWJobDocAddress>();
		}
		#endregion
	}
}
