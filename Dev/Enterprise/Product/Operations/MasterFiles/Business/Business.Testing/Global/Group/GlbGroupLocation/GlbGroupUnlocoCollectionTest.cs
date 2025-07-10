using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupUnlocoCollection))]
	sealed class GlbGroupUnlocoCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbGroupUnlocoCollection>
	{
		public void TestRemoveAllFromRelationship()
		{
			var group = Factory.New<GlbGroup>();
			var collection = new GlbGroupUnlocoCollection(group);
			var unloco1 = collection.AddNew();
			var unloco2 = collection.AddNew();

			collection.RemoveAllFromRelationship();

			AssertEquals(0, collection.Count);
			AssertEquals(false, unloco1.IsDeleted);
			AssertEquals(false, unloco2.IsDeleted);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var element = base.GetNewElementToAddToTheCollection();
			((RefUNLOCO)element).RL_Code = new ZString((char)getNewElementToAddToTheCollectionCalls++, RefUNLOCO.Schema.RL_CodeMaxLength);
			return element;
		}
		int getNewElementToAddToTheCollectionCalls;

		protected override GlbGroupUnlocoCollection GetCollectionToTest()
		{
			var group = Factory.New<GlbGroup>();
			return new GlbGroupUnlocoCollection(group);
		}

		#endregion
	}
}
