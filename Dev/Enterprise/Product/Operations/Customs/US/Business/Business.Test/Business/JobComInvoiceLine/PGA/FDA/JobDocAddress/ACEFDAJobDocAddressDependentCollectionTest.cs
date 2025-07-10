using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEFDAJobDocAddressDependentCollection))]
	public sealed class ACEFDAJobDocAddressDependentCollectionTest : JobDocAddressDependentCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ACEFDAJobDocAddressDependentCollection(Factory.New<ACEFDA>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ACEFDAJobDocAddress>();
		}

		#endregion
	}
}
