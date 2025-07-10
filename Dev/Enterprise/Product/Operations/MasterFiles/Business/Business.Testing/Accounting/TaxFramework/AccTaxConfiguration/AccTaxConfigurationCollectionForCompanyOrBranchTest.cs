using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxConfigurationCollectionForCompanyOrBranch))]
	abstract class AccTaxConfigurationCollectionForCompanyOrBranchTest<ParentType> : AccTaxConfigurationCollectionTest where ParentType : BusinessObject
	{
		public void TestSecurityRightsAppliedForTaxConfigurationCollection()
		{
			var parent = GetCollectionParent();
			var securityCheckpoint = parent is GlbCompany ? Env.Security.CompaniesModifyTaxConfiguration : Env.Security.BranchModifyTaxConfiguration;

			securityCheckpoint.IsAllowed = false;
			var taxConfigurationCollection = GetCollection(parent);

			Assert(taxConfigurationCollection.ReadOnly);

			securityCheckpoint.IsAllowed = true;
			taxConfigurationCollection = GetCollection(parent);

			Assert(!taxConfigurationCollection.ReadOnly);
		}

		public void TestDefaultValues()
		{
			var parent = GetCollectionParent();

			var collection = GetCollection(parent);
			var taxConfiguration = collection.AddNew();

			AssertEquals("RN_NKCountryCode should be equal to company parent country.", GetParentCountry(parent), taxConfiguration.ETC_RN_NKCountry);
			AssertEquals("ETC_ParentID guid should be equal to parent PK.", parent.PK.ToGuid(), taxConfiguration.ETC_ParentId);
			AssertEquals("ETC_ParentTableCode should be equal to GC what is the parent table code prefix.", parent.TablePrefix, taxConfiguration.ETC_ParentTableCode);
		}

		public void TestCollectionLoadsCorrectRecords()
		{
			var parent1 = GetCollectionParent();
			var parent2 = GetCollectionParent();

			var collection1 = GetCollection(parent1);
			var collection2 = GetCollection(parent2);

			var config1 = Factory.New<AccTaxConfiguration>();
			var config2 = Factory.New<AccTaxConfiguration>();

			config1.ETC_ParentId = parent1.PK;
			config1.ETC_ParentTableCode = parent1.TablePrefix;

			config2.ETC_ParentId = parent2.PK;
			config2.ETC_ParentTableCode = parent2.TablePrefix;

			AssertEquals(1, collection1.Count);
			AssertEquals(1, collection2.Count);
			AssertCollectionContains(config1, collection1);
			AssertCollectionContains(config2, collection2);
		}

		protected abstract AccTaxConfigurationCollectionForCompanyOrBranch GetCollection(ParentType parent);
		protected abstract ParentType GetCollectionParent();
		protected abstract ZString GetParentCountry(ParentType parent);
	}
}
