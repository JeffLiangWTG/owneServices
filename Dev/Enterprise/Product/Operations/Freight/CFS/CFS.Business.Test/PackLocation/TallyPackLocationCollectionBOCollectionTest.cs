using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyPackLocationCollection))]
	public class TallyPackLocationCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestParent()
		{
			TallyPackLine parent = Factory.New<TallyPackLine>();
			TallyPackLocationCollection collection = new TallyPackLocationCollection(parent, Factory);
			TallyPackLine collectionParent = (TallyPackLine)ZReflection.GetPropertyIncludingNew(collection, "Parent", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(collection, null);
			AssertEquals("The TallyPackLocationCollection parent was not correctly set.", parent, collectionParent);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TallyPackLine parent = Factory.New<TallyPackLine>();
			return new TallyPackLocationCollection(parent, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TallyPackLocation>();
		}

		#endregion
	}
}
