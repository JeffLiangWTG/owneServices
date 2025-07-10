using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARTermsCycleCollection))]
	sealed class ARTermsCycleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ARTermsCycleCollection>
	{
		public void TestAllowRemoveCore()
		{
			ARTermsCycleCollection testCollection = GetCollectionToTest();
			Assert(!testCollection.AllowRemove);

			testCollection.AddNew();
			Assert(!testCollection.AllowRemove);

			testCollection.AddNew();
			Assert(testCollection.AllowRemove);

			testCollection.Remove(testCollection[0]);
			Assert(!testCollection.AllowRemove);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ARTermsCycleCollection GetCollectionToTest()
		{
			return new ARTermsCycleCollection(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ARTermsCycle();
		}

		#endregion
	}
}
