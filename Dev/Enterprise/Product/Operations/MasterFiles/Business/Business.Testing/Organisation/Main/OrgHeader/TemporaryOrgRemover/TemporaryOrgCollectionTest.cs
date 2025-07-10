using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TemporaryOrgCollection))]
	sealed class TemporaryOrgCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryOrgCollection>
	{
		public void TestLoad()
		{
			TemporaryOrgCollection collection = new TemporaryOrgCollection(Factory);

			OrgHeader org = Factory.New<OrgHeader>();
			org.SetDefaultValuesForTemporaryOrganisation();
			org.OH_FullName = "TEST TEMPORARY ORG";
			org.OH_Code = "TEMPTESTORG";
			Factory.Save();

			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(org.PK, collection[0].OrgPK);
			AssertEquals(org.OH_Code, collection[0].Code);
			AssertEquals(org.OH_FullName, collection[0].FullName);
		}

		#region Implemantation

		protected override TemporaryOrgCollection GetCollectionToTest()
		{
			return new TemporaryOrgCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TemporaryOrg(ZGuid.NewZGuid(), "TEST", "TEST ORGANISATION");
		}

		#endregion
	}
}
