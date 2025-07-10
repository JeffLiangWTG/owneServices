using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbSecurityCollectionView))]
	sealed class GlbSecurityCollectionViewTest : BusinessObjectCollectionViewTestCase<GlbSecurityCollectionView>
	{
		new GlbSecurityCollectionView Collection
		{
			get { return base.Collection; }
		}

		protected override GlbSecurityCollectionView GetCollectionToTest()
		{
			GlbSecurityCollection securityCollection = new GlbSecurityCollection(Factory);
			GlbSecurityCollectionView view = new GlbSecurityCollectionView(securityCollection);
			view.FilterBySecurityKey(new CheckpointLookupKey("TEST"), ZGuid.Empty);
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = "TEST";
			security.GU_ItemGUID = ZGuid.Empty;
			return security;
		}

		public void TestIsThisPartOfTheCollection()
		{
			GlbSecurity testSecurity1 = Factory.New<GlbSecurity>();
			testSecurity1.GU_SecurityRight = "Operations";
			testSecurity1.GU_ItemGUID = ZGuid.Empty;

			Guid testSecurity2ItemGuid = Guid.NewGuid();
			GlbSecurity testSecurity2 = Factory.New<GlbSecurity>();
			testSecurity2.GU_SecurityRight = "Config";
			testSecurity2.GU_ItemGUID = testSecurity2ItemGuid;

			GlbSecurityCollection securityCollection = new GlbSecurityCollection(Factory);
			securityCollection.Add(testSecurity1);
			securityCollection.Add(testSecurity2);

			GlbSecurityCollectionView securityView = new GlbSecurityCollectionView(securityCollection);

			securityView.FilterBySecurityKey(new CheckpointLookupKey("FakeKey"), ZGuid.Empty);
			AssertEquals("Should not be part of the collection.", 0, securityView.Count);

			securityView.FilterBySecurityKey(new CheckpointLookupKey("FakeKey", testSecurity2ItemGuid), ZGuid.Empty);
			AssertEquals("Should not be part of the collection.", 0, securityView.Count);

			securityView.FilterBySecurityKey(new CheckpointLookupKey("Operations"), ZGuid.Empty);
			AssertEquals("Should be part of the collection.", 1, securityView.Count);

			securityView.FilterBySecurityKey(new CheckpointLookupKey("Operations", testSecurity2ItemGuid), ZGuid.Empty);
			AssertEquals("Should not be part of the collection.", 0, securityView.Count);

			securityView.FilterBySecurityKey(new CheckpointLookupKey("Config"), ZGuid.Empty);
			AssertEquals("Should not be part of the collection.", 0, securityView.Count);

			securityView.FilterBySecurityKey(new CheckpointLookupKey("Config", testSecurity2ItemGuid), ZGuid.Empty);
			AssertEquals("Should be part of the collection.", 1, securityView.Count);
		}

		public void TestSecurity()
		{
			SecurityCore security = new SecurityCore(Collection.CollectionToFilter, GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			Collection.CollectionToFilter.Security = security;
			AssertEquals("Security", security, ((IGlbSecurityCollectionWithSecurity)Collection).Security);
		}

		public void TestSetCollectionRelationships()
		{
			Collection.FilterBySecurityKey(new CheckpointLookupKey("FakeKey"), ZGuid.Empty);
			AssertEquals("No security permissions should be in collection.", 0, Collection.Count);

			Collection.AddNew();
			Collection.FilterBySecurityKey(new CheckpointLookupKey("FakeKey"), ZGuid.Empty);
			AssertEquals("Security right should be set with FakeKey as security right by default.", 1, Collection.Count);
		}
	}
}
