using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgARTermsCycleCollection))]
	sealed class OrgARTermsCycleCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgARTermsCycleCollection>
	{
		public void TestNewElementReadonly()
		{
			OrgARTermsCycleCollection collection = GetCollectionToTest();
			AssertEquals("Collection readonly", false, collection.ReadOnly);
			AssertEquals("New element readonly", false, collection.AddNew().ReadOnly);

			collection.SetReadOnlyIncludingChildren(true);
			AssertEquals("Collection readonly", true, collection.ReadOnly);
			AssertEquals("New element readonly", true, collection.AddNew().ReadOnly);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(OrgARTermsCycleCollection);
		}

		protected override OrgARTermsCycleCollection GetCollectionToTest()
		{
			return new OrgARTermsCycleCollection(Factory);
		}
	}
}
