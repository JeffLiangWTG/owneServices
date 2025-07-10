using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(CustomsDSBCreditorOverrideCollection))]
	sealed class CustomsDisbursementCreditorOverrideCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CustomsDSBCreditorOverrideCollection>
	{
		public void TestGetCreditorFor()
		{
			var organization1 = Factory.New<OrgHeader>();
			var organization2 = Factory.New<OrgHeader>();
			var collection = new CustomsDSBCreditorOverrideCollection();
			var mapping1 = collection.AddNew();
			mapping1.DistrictOfficeCode = "BIA";
			mapping1.CreditorPK = organization1.PK;
			var mapping2 = collection.AddNew();
			mapping2.DistrictOfficeCode = "BBR";
			mapping2.CreditorPK = organization2.PK;
			AssertEquals(organization2.PK, collection.GetCreditorFor("BBR"));
			AssertEquals(ZGuid.Empty, collection.GetCreditorFor("BFN"));
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override CustomsDSBCreditorOverrideCollection GetCollectionToTest()
		{
			return new CustomsDSBCreditorOverrideCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomsDSBCreditorOverride();
		}
	}
}
