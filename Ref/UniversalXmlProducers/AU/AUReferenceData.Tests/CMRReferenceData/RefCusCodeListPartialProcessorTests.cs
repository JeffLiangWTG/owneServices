using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	public class RefCusCodeListPartialProcessorTests
	{
		RefCusCodeListPartialProcessor processor;

		[SetUp]
		public void Setup()
		{
			processor = new RefCusCodeListPartialProcessor(new Mock<IRefDataLoader>().Object, "CODE_TYPE");
		}

		[Test]
		public void GetEntityKeySelector_ReturnsCorrectKey()
		{
			var entity = new RefCusCodeList { ZZD_Code = "ABC123" };
			var keySelector = processor.GetEntityKeySelector();

			Assert.AreEqual("ABC123", keySelector(entity));
		}

		[Test]
		public void ShouldSkipDeleteEntity_ReturnsTrue_WhenEndDateBeforePublished()
		{
			var entity = new RefCusCodeList();
			var existing = new RefCusCodeList { ZZD_EndDate = new DateTime(2020, 1, 1) };
			var published = new DateTime(2021, 1, 1);

			var result = processor.ShouldSkipDeleteEntity(entity, existing, published);

			Assert.IsTrue(result);
		}

		[Test]
		public void DeleteEntity_SetsEndDate()
		{
			var entity = new RefCusCodeList();
			var published = new DateTime(2023, 5, 1);

			processor.DeleteEntity(entity, published);

			Assert.AreEqual(published, entity.ZZD_EndDate);
		}

		[Test]
		public void ShouldSkipUpdateEntity_ReturnsTrue_WhenMatchingAttributeExists()
		{
			var published = new DateTime(2023, 1, 1);
			var attr = new RefCusCodeListAttribute { ZZE_Value = "VALUE", ZZE_EndDate = new DateTime(2024, 1, 1) };
			var entity = new RefCusCodeList { RefCusCodeListAttributes = [attr] };
			var existing = new RefCusCodeList { RefCusCodeListAttributes = [attr] };

			var result = processor.ShouldSkipUpdateEntity(entity, existing, published);

			Assert.IsTrue(result);
		}

		[Test]
		public void UpdateEntity_ExpiresOldAttributes()
		{
			var published = new DateTime(2023, 1, 1);
			var newAttributeEndDate = new DateTime(2025, 1, 1);
			var oldAttribute = new RefCusCodeListAttribute { ZZE_Value = "OLD", ZZE_EndDate = new DateTime(2024, 1, 1) };
			var newAttribute = new RefCusCodeListAttribute { ZZE_Value = "NEW", ZZE_EndDate = newAttributeEndDate };

			var entity = new RefCusCodeList { RefCusCodeListAttributes = [newAttribute] };
			var existing = new RefCusCodeList { RefCusCodeListAttributes = [oldAttribute] };

			processor.UpdateEntity(entity, existing, published);

			Assert.AreEqual(published, entity.RefCusCodeListAttributes[0].ZZE_EndDate);
			Assert.AreEqual("OLD", entity.RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual(newAttributeEndDate, entity.RefCusCodeListAttributes[1].ZZE_EndDate);
			Assert.AreEqual("NEW", entity.RefCusCodeListAttributes[1].ZZE_Value);
			Assert.AreEqual(2, entity.RefCusCodeListAttributes.Length);
		}
	}
}
