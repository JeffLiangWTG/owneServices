using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupCountryCollection))]
	sealed class GlbGroupCountryCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbGroupCountryCollection>
	{
		public void TestRemoveAllFromRelationship()
		{
			var group = Factory.New<GlbGroup>();
			var collection = new GlbGroupCountryCollection(group);
			var country1 = collection.AddNew();
			var country2 = collection.AddNew();

			collection.RemoveAllFromRelationship();

			AssertEquals(0, collection.Count);
			AssertEquals(false, country1.IsDeleted);
			AssertEquals(false, country2.IsDeleted);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var element = base.GetNewElementToAddToTheCollection();
			((RefCountry)element).RN_Code = new ZString((char)getNewElementToAddToTheCollectionCalls++, RefCountry.Schema.RN_CodeMaxLength);
			return element;
		}
		int getNewElementToAddToTheCollectionCalls;

		protected override GlbGroupCountryCollection GetCollectionToTest()
		{
			var group = Factory.New<GlbGroup>();
			return new GlbGroupCountryCollection(group);
		}

		#endregion
	}
}
