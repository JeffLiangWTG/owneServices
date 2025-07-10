using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WebSecurityRightsListBaseFunctionalityTest : TestCase
	{
		class DummyProvider : WebSecurityRightsProviderForTest
		{
			public WebSecurityRight Add(string code)
			{
				var right = new WebSecurityRight(code, (NoResString)"", WebSecurityApplication.EdiWebTracker);
				Add(right);

				return right;
			}
		}

		public void TestIterate_IncludesProviders()
		{
			var provider = new DummyProvider();
			var first = provider.Add("First");
			var second = provider.Add("Second");

			var list = new WebSecurityRightsListForTest();
			list.Add(provider);

			AssertCollectionContains("Should have each item when we iterate", first, list);
			AssertCollectionContains("Should have each item when we iterate", second, list);

			var third = provider.Add("Third");
			AssertCollectionContains("Should include items added after the provider was added", third, list);
		}

		public void TestFindByApplication_IncludesProviders()
		{
			var provider = new DummyProvider();
			var first = provider.Add("First");

			var list = new WebSecurityRightsListForTest();
			list.Add(provider);

			AssertCollectionContains("Should have each item when we iterate", first, list.FindByApplication(WebSecurityApplication.EdiWebTracker));
		}

		public void TestFindCode_IncludesProviders()
		{
			var provider = new DummyProvider();
			var first = provider.Add("First");

			var list = new WebSecurityRightsListForTest();
			list.Add(provider);

			AssertEquals("Should also check the providers for codes", first, list.FindByCode("First"));

			var second = provider.Add("Second");
			AssertEquals("Should include items added after the provider was added", second, list.FindByCode("Second"));
		}

		public void TestElements()
		{
			WebSecurityRight viewRight = new WebSecurityRight("View", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			WebSecurityRight newRight = new WebSecurityRight("New", (NoResString)"", WebSecurityApplication.EdiWebTracker);
			WebSecurityRight editRight = new WebSecurityRight("Edit", (NoResString)"", WebSecurityApplication.EdiWebTracker, false);
			WebSecurityRight deleteRight = new WebSecurityRight("Delete", (NoResString)"", WebSecurityApplication.EdiWebTracker, false);
			WebSecurityRightsListForTest list = new WebSecurityRightsListForTest();
			AssertEquals(0, list.Count);

			list.Add(viewRight);
			AssertEquals(1, list.Count);

			list.Add(newRight);
			list.Add(editRight);
			list.Add(deleteRight);
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode("View"));
			AssertEquals(viewRight, list.FindByCode("View"));
			Assert(list.ContainsCode("Edit"));
			AssertEquals(editRight, list.FindByCode("Edit"));
			Assert(list.ContainsCode("Delete"));
			AssertEquals(deleteRight, list.FindByCode("Delete"));
			Assert(list.ContainsCode("New"));
			AssertEquals(deleteRight, list.FindByCode("Delete"));
			Assert(!list.ContainsCode("MEH"));
			AssertNull(list.FindByCode("MEH"));
		}

		public void TestFindByApplication()
		{
			WebSecurityApplicationForTest app1 = new WebSecurityApplicationForTest("app1", true);
			WebSecurityApplicationForTest app2 = new WebSecurityApplicationForTest("app2", true);

			WebSecurityRightsListForTest list = new WebSecurityRightsListForTest();
			WebSecurityRight right1 = new WebSecurityRight("right1", (NoResString)"", app1);
			WebSecurityRight right2 = new WebSecurityRight("right2", (NoResString)"", app2);
			WebSecurityRight right3 = new WebSecurityRight("right3", (NoResString)"", app2);
			WebSecurityRight right4 = new WebSecurityRight("right4", (NoResString)"", app1);

			AssertEquals(0, list.FindByApplication(app1).Length);
			AssertEquals(0, list.FindByApplication(app2).Length);

			list.Add(right1);
			list.Add(right2);
			list.Add(right3);
			list.Add(right4);
			WebSecurityRight[] app1Rights = list.FindByApplication(app1);
			WebSecurityRight[] app2Rights = list.FindByApplication(app2);
			AssertEquals(2, app1Rights.Length);
			AssertCollectionContains(right1, app1Rights);
			AssertCollectionContains(right4, app1Rights);
			AssertEquals(2, app2Rights.Length);
			AssertCollectionContains(right2, app2Rights);
			AssertCollectionContains(right3, app2Rights);
		}

		class WebSecurityRightsListForTest : AllWebSecurityRights
		{
			readonly WebSecurityRightsProviderForTest rightsDictionary = new WebSecurityRightsProviderForTest();

			public WebSecurityRightsListForTest()
			{
				Add(rightsDictionary);
			}

			public void Add(WebSecurityRight right) => rightsDictionary.Add(right);

			public new void Add(IWebSecurityRightProvider provider) => base.Add(provider);
		}

		class WebSecurityApplicationForTest : WebSecurityApplication
		{
			public WebSecurityApplicationForTest(string name, bool shouldGrantAllAccessRightsByDefault)
				: base(name, shouldGrantAllAccessRightsByDefault)
			{
			}
		}
	}
}
