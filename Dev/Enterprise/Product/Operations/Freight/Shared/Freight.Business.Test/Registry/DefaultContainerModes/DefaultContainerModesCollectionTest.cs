using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DefaultContainerModesCollection))]
	sealed class DefaultContainerModesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultContainerModesCollection>
	{
		public void TestDuplicateRegistryEntry()
		{
			var setupCollection = new DefaultContainerModesCollection();
			var registrySetup = new DefaultContainerModes();
			registrySetup.TransportMode = "AIR";
			registrySetup.ContainerMode = "ULD";
			setupCollection.Add(registrySetup);
			AssertEquals("Duplicate item not expected", false, setupCollection.IsDuplicateItem(registrySetup));

			var duplicateRegistry = new DefaultContainerModes();
			duplicateRegistry.TransportMode = "AIR";
			duplicateRegistry.ContainerMode = "ULD";
			setupCollection.Add(duplicateRegistry);
			AssertEquals("Duplicate item expected", true, setupCollection.IsDuplicateItem(duplicateRegistry));
		}

		public void TestFindDefaultContainerMode()
		{
			var setupCollection = new DefaultContainerModesCollection();
			var registryItem1 = new DefaultContainerModes();
			registryItem1.TransportMode = "AIR";
			registryItem1.ContainerMode = "ULD";
			setupCollection.Add(registryItem1);

			var registryItem2 = new DefaultContainerModes();
			registryItem2.TransportMode = "ROA";
			registryItem2.ContainerMode = "LQD";
			setupCollection.Add(registryItem2);
			AssertEquals("Should now be 2 default container modes", 2, setupCollection.Count);

			string dCM = setupCollection.FindDefaultContainerMode("SEA");
			AssertEquals("Should not find a default value", "", dCM);

			dCM = setupCollection.FindDefaultContainerMode("ROA");
			AssertEquals("Should find a default value", "LQD", dCM);

			dCM = setupCollection.FindDefaultContainerMode("AIR");
			AssertEquals("Should find a default value", "ULD", dCM);
		}

		#region Implementation

		protected override DefaultContainerModesCollection GetCollectionToTest()
		{
			return new DefaultContainerModesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultContainerModes(Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
