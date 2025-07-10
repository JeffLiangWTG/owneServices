using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSPackLocationCollection))]
	public class CFSPackLocationCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestParent()
		{
			CFSPackLine parent = Factory.New<CFSPackLine>();
			CFSPackLocationCollection collection = new CFSPackLocationCollection(parent, Factory);
			CFSPackLine collectionParent = (CFSPackLine)collection.GetType().GetProperty("Parent", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(collection, null);
			AssertEquals("The CFSPackLocationCollection parent was not correctly set.", parent, collectionParent);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CFSPackLine parent = Factory.New<CFSPackLine>();
			return new CFSPackLocationCollection(parent, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CFSPackLocation>();
		}

		#endregion
	}
}
