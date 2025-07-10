using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class ConstantValueKeyPropertyFilterFixture
	{
		[Test]
		public void TestGetEntitiesOrSafeObjectsWithoutFilterProperty()
		{
			var keyProperties = new[]
			{
				new KeyProperty { Name = "ZZ3_Name", ConstantValue = string.Empty },
				new KeyProperty { Name = "ZZ3_Value", ConstantValue = string.Empty }
			};
			var keyPropertyFilter = new ConstantValueKeyPropertyFilter(typeof(RefCusTariffAttribute), keyProperties);
			var metaProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var stagingDataWrapperFactory = new Mock<IStagingDataWrapperFactory>();
			var relatedSafeObjects = new[] { new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "5" } };
			var relatedEntities = new[]
			{
				new StagingDataWrapper(metaProvider.Object, stagingDataProvider.Object,
					overlappingCalculator, stagingDataWrapperFactory.Object,
					new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "5" }),
				new StagingDataWrapper(metaProvider.Object, stagingDataProvider.Object,
					overlappingCalculator, stagingDataWrapperFactory.Object,
					new RefCusTariffAttribute { ZZ3_Name = "ImportPermit", ZZ3_Value = "MANDATORY" })
			};

			var filteredEntities = keyPropertyFilter.GetFilteredEntities(relatedEntities);
			Assert.AreEqual(relatedEntities, filteredEntities);

			var filteredSafeObjects = keyPropertyFilter.GetFilteredSafeObjects(relatedSafeObjects);
			Assert.AreEqual(relatedSafeObjects, filteredSafeObjects);
		}

		[Test]
		public void TestGetEntitiesOrSafeObjectsWithSingleFilterProperty()
		{
			var keyProperties = new[]
			{
				new KeyProperty { Name = "ZZ3_Name", ConstantValue = "CheckDigit" },
				new KeyProperty { Name = "ZZ3_Value", ConstantValue = string.Empty }
			};
			var keyPropertyFilter = new ConstantValueKeyPropertyFilter(typeof(RefCusTariffAttribute), keyProperties);
			var metaProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var stagingDataWrapperFactory = new Mock<IStagingDataWrapperFactory>();
			var relatedSafeObjects = new[]
			{
				new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "3" },
				new RefCusTariffAttribute { ZZ3_Name = "ImportPermit", ZZ3_Value = "MANDATORY" }
			};
			var relatedEntities = new[]
			{
				new StagingDataWrapper(metaProvider.Object, stagingDataProvider.Object,
					overlappingCalculator, stagingDataWrapperFactory.Object,
					new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "5" }),
				new StagingDataWrapper(metaProvider.Object, stagingDataProvider.Object,
					overlappingCalculator, stagingDataWrapperFactory.Object,
					new RefCusTariffAttribute { ZZ3_Name = "ImportPermit", ZZ3_Value = "MANDATORY" })
			};
		
			var filteredEntities = keyPropertyFilter.GetFilteredEntities(relatedEntities);
			Assert.AreEqual(new[] { relatedEntities[0] }, filteredEntities);

			var filteredSafeObjects = keyPropertyFilter.GetFilteredSafeObjects(relatedSafeObjects);
			Assert.AreEqual(new[] { relatedSafeObjects[0] }, filteredSafeObjects);
		}
		
		[Test]
		public void TestGetEntitiesOrSafeObjectsWithMultipleFilterProperty()
		{
			var keyProperties = new[]
			{
				new KeyProperty { Name = "ZZ3_Name", ConstantValue = "CheckDigit" },
				new KeyProperty { Name = "ZZ3_Value", ConstantValue = "3" }
			};
			var keyPropertyFilter = new ConstantValueKeyPropertyFilter(typeof(RefCusTariffAttribute), keyProperties);
			var metaProvider = new Mock<IMetadataProvider>();
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var stagingDataWrapperFactory = new Mock<IStagingDataWrapperFactory>();
			var relatedSafeObjects = new[]
			{
				new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "3" },
				new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "5" },
				new RefCusTariffAttribute { ZZ3_Name = "ImportPermit", ZZ3_Value = "MANDATORY" }
			};
			var relatedEntities = new[]
			{
				new StagingDataWrapper(metaProvider.Object, stagingDataProvider.Object,
					overlappingCalculator, stagingDataWrapperFactory.Object,
					new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "3" }),
				new StagingDataWrapper(metaProvider.Object, stagingDataProvider.Object,
					overlappingCalculator, stagingDataWrapperFactory.Object,
					new RefCusTariffAttribute { ZZ3_Name = "CheckDigit", ZZ3_Value = "7" })
			};
		
			var filteredEntities = keyPropertyFilter.GetFilteredEntities(relatedEntities);
			Assert.AreEqual(new[] { relatedEntities[0] }, filteredEntities);

			var filteredSafeObjects = keyPropertyFilter.GetFilteredSafeObjects(relatedSafeObjects);
			Assert.AreEqual(new[] { relatedSafeObjects[0] }, filteredSafeObjects);
		}

		IOverlappingCalculator overlappingCalculator;
		[SetUp]
		public void SetUp()
		{
			overlappingCalculator = new OverlappingCalculator(false);
		}
	}
}
