using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgsEvaluatedForCreditControlCollection))]
	sealed class OrgsEvaluatedForCreditControlCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrgsEvaluatedForCreditControlCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override OrgsEvaluatedForCreditControlCollection GetCollectionToTest()
		{
			return new OrgsEvaluatedForCreditControlCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgsEvaluatedForCreditControl();
		}

		public void TestCreateNonPersistentBusinessObject()
		{
			var collection = GetCollectionToTest();
			collection.CurrentFallbackLevel = NewFallbackLevel();
			var control = collection.AddNew();
			AssertEquals(control.CurrentFallbackLevel, collection.CurrentFallbackLevel);
			AssertNotNull(control.CurrentFallbackLevel);
		}

		#endregion
	}
}
