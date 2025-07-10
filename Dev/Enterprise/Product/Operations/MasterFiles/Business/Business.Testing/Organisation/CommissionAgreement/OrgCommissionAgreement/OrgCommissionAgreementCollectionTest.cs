using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreementCollection))]
	sealed class OrgCommissionAgreementCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCommissionAgreementCollection>
	{
		#region Default Values

		public void TestDefaultValues_CA0_Name()
		{
			var collection = new OrgCommissionAgreementCollection(Factory);
			var agreement1 = collection.AddNew();
			AssertEquals("#1", agreement1.CA0_Name);

			var agreement2 = collection.AddNew();
			AssertEquals("#2", agreement2.CA0_Name);

			var agreement3 = collection.AddNew();
			AssertEquals("#3", agreement3.CA0_Name);

			collection.Delete(agreement2);

			var agreement4 = collection.AddNew();
			AssertEquals("#4", agreement4.CA0_Name);

			collection.Delete(agreement4);

			var agreement5 = collection.AddNew();
			AssertEquals("#4", agreement5.CA0_Name);
			agreement5.CA0_Name = "PAV";

			var agreement6 = collection.AddNew();
			AssertEquals("#4", agreement6.CA0_Name);
		}

		#endregion
	}
}
