using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefContainer.Loader))]
	sealed class RefContainerLoaderTest : LoaderTestCase
	{
		public void TestLoadFromCode()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_Code = "XYZ";
			AssertEquals(container, new RefContainer.Loader(Factory).LoadFromCode("XYZ"));
			container.RC_Code = "";
			AssertEquals(null, new RefContainer.Loader(Factory).LoadFromCode("XYZ"));
		}

		public void TestLoadFromISOType()
		{
			const string ISOType = "7878";
			RefContainer container = RefContainer.New(Factory);
			container.RC_ISOType = ISOType;
			AssertEquals("Active container matches ISO Type, so it should have loaded and yet...", container, new RefContainer.Loader(Factory).LoadFromISOType(ISOType));

			container.RC_ISOType = "";
			AssertEquals(null, new RefContainer.Loader(Factory).LoadFromISOType(ISOType));
		}

		public void TestLoadFromISOTypeShouldOnlyReturnActiveContainers()
		{
			const string isoType = "7878";
			var container = RefContainer.New(Factory);
			container.RC_ISOType = isoType;
			container.RC_IsActive = false;
			AssertEquals("This container is inactive and shouldn't have loaded", null, new RefContainer.Loader(Factory).LoadFromISOType(isoType));

			container.RC_IsActive = true;
			AssertEquals("The container is active and should have loaded", container, new RefContainer.Loader(Factory).LoadFromISOType(isoType));
		}

		public void TestLoadFromISOTypeShouldOnlyReturnContainersWithUniqueISOTypes()
		{
			const string isoType = "2112";
			var container = RefContainer.New(Factory);
			container.RC_ISOType = isoType;

			AssertEquals("Single active container matches ISO Type, so it should have loaded", container, new RefContainer.Loader(Factory).LoadFromISOType(isoType));

			var duplicateContainer = RefContainer.New(Factory);
			duplicateContainer.RC_ISOType = isoType;
			AssertEquals("Two active containers matched ISO Type so neither should have loaded", null, new RefContainer.Loader(Factory).LoadFromISOType(isoType));

			duplicateContainer.RC_IsActive = false;
			AssertEquals("Two containers matched ISO Type, but only one was active so it should have loaded", container, new RefContainer.Loader(Factory).LoadFromISOType(isoType));

			var thirdContainer = RefContainer.New(Factory);
			thirdContainer.RC_ISOType = isoType;
			AssertEquals("Two active containers matched ISO Type, so neither should have loaded", null, new RefContainer.Loader(Factory).LoadFromISOType(isoType));

			thirdContainer.RC_IsActive = false;
			AssertEquals("Single active container matched ISO Type, so it should have loaded", container, new RefContainer.Loader(Factory).LoadFromISOType(isoType));
		}

		public void TestLoadFromISOTypeWithFallback()
		{
			string isoType1 = "20S0";
			var container = RefContainer.New(Factory);
			container.RC_ISOType = isoType1;

			var refContainers = new RefContainer.Loader(Factory).LoadFromISOTypeWithGroupFallback(isoType1).ToList();
			AssertCollectionContains("Single active container matches ISO Type, so it should have loaded", container, refContainers);

			var duplicateContainer = RefContainer.New(Factory);
			duplicateContainer.RC_ISOType = isoType1;

			refContainers = new RefContainer.Loader(Factory).LoadFromISOTypeWithGroupFallback(isoType1).ToList();
			AssertEquals(2, refContainers.Count);
			AssertCollectionContains("Two active container matches ISO Type, so it should have loaded", container, refContainers);
			AssertCollectionContains("Two active container matches ISO Type, so it should have loaded", duplicateContainer, refContainers);

			duplicateContainer.RC_IsActive = false;
			refContainers = new RefContainer.Loader(Factory).LoadFromISOTypeWithGroupFallback(isoType1).ToList();
			AssertEquals(1, refContainers.Count);
			AssertCollectionContains("Two containers matched ISO Type, but only one was active so it should have loaded", container, refContainers);

			string isoType2 = "20S1";
			refContainers = new RefContainer.Loader(Factory).LoadFromISOTypeWithGroupFallback(isoType2).ToList();
			AssertCollectionContains("Fallback with same ISO group, so it should load", container, refContainers);

			duplicateContainer.RC_IsActive = true;
			duplicateContainer.RC_ISOType = "20S2";
			refContainers = new RefContainer.Loader(Factory).LoadFromISOTypeWithGroupFallback(isoType2).ToList();
			AssertEquals(2, refContainers.Count);
			AssertCollectionContains("Two active containers fallback with same ISO group, so it should have loaded", container, refContainers);
			AssertCollectionContains("Two active containers fallback with same ISO group, so it should have loaded", duplicateContainer, refContainers);

			duplicateContainer.RC_IsActive = false;
			refContainers = new RefContainer.Loader(Factory).LoadFromISOTypeWithGroupFallback(isoType2).ToList();
			AssertEquals(1, refContainers.Count);
			AssertCollectionContains("Two active containers fallback with same ISO group, but only one was active so it should have loaded", container, refContainers);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefContainer.Loader(Factory);
		}
	}
}
