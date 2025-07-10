using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeDependencyConfigurationCollection))]
	sealed class ComplianceSubTypeDependencyConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceSubTypeDependencyConfigurationCollection>
	{
		public void TestSearchCyclicSubTypeRelationship()
		{
			ComplianceSubTypeDependencyConfiguration newConfig = Collection.AddNew();
			newConfig.Country = "TW";
			newConfig.ChildSubType = "TXI";
			newConfig.ParentSubType = "TXC";
			Assert(!Collection.SearchCyclicSubTypeRelationship("TW", "TXI", "TXC"));

			newConfig = Collection.AddNew();
			newConfig.Country = "TW";
			newConfig.ChildSubType = "TXC";
			newConfig.ParentSubType = "TDI";
			Assert(!Collection.SearchCyclicSubTypeRelationship("TW", "TXI", "TXC"));

			newConfig = Collection.AddNew();
			newConfig.Country = "TW";
			newConfig.ChildSubType = "TDI";
			newConfig.ParentSubType = "TXI";
			Assert(Collection.SearchCyclicSubTypeRelationship("TW", "TXI", "TXC"));
		}

		public void TestFindParentSubTypeInCollection()
		{
			ComplianceSubTypeDependencyConfiguration item = Collection.AddNew();
			item.Country = "TW";
			item.ChildSubType = "TXI";
			item.ParentSubType = "TXS";
			item = Collection.AddNew();
			item.Country = "TW";
			item.ChildSubType = "TXS";
			item.ParentSubType = "TXP";
			item = Collection.AddNew();
			item.Country = "TW";
			item.ChildSubType = "TXP";
			item.ParentSubType = "TXE";
			AssertEquals("TXE", Collection.FindParentSubTypeInCollection("TW", "TXI"));
		}

		public void TestSearchCyclicSubTypeRelationshipInfiniteLoop()
		{
			var newConfig1 = Collection.AddNew();
			newConfig1.Country = "ID";
			newConfig1.ChildSubType = "T01";
			newConfig1.ParentSubType = "TXI";
			AssertNoErrors(newConfig1.ChildSubTypeInfo);

			var newConfig2 = Collection.AddNew();
			newConfig2.Country = "ID";
			newConfig2.ChildSubType = "T02";
			newConfig2.ParentSubType = "TXI";
			AssertNoErrors(newConfig2.ChildSubTypeInfo);

			var newConfig3 = Collection.AddNew();
			newConfig3.Country = "ID";
			newConfig3.ChildSubType = "T03";
			newConfig3.ParentSubType = "TXI";
			AssertNoErrors(newConfig3.ChildSubTypeInfo);

			newConfig1.ChildSubType = "TXI";

			AssertHasErrors(newConfig1.ChildSubTypeInfo);
			AssertNoExceptionThrown(() => newConfig2.RunPreSaveValidation());
		}

		#region Implementation

		protected override ComplianceSubTypeDependencyConfigurationCollection GetCollectionToTest()
		{
			return new ComplianceSubTypeDependencyConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceSubTypeDependencyConfiguration();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new ComplianceSubTypeDependencyConfigurationCollection Collection => base.Collection;

		#endregion
	}
}
