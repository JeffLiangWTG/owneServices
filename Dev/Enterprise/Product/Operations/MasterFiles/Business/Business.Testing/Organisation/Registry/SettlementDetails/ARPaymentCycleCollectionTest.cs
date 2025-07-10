using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARPaymentCycleCollection))]
	sealed class ARPaymentCycleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ARPaymentCycleCollection>
	{
		public void TestAllowRemoveCore()
		{
			ARPaymentCycleCollection testCollection = GetCollectionToTest();
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

		protected override ARPaymentCycleCollection GetCollectionToTest()
		{
			return new ARPaymentCycleCollection(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ARPaymentCycle();
		}

		#endregion
	}
}
