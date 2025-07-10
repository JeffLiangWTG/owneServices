using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	public class EnableExpirableCheckerFixture
	{
		Mock<IMetadataProvider> metadataProviderMock;
		EnableExpirableChecker enableExpirableChecker;

		[Test]
		public void ValidateWrapper()
		{
			var codeListAttribute1WrapperMock = new Mock<IStagingDataWrapper>();
			codeListAttribute1WrapperMock.Setup(x => x.GetStagingType()).Returns(typeof(Schema_New.RefCusCodeListAttribute));
			codeListAttribute1WrapperMock.Setup(x => x.GetWrapperValue(nameof(RefCusCodeListAttribute.ZZE_StartDate))).Returns("2020-01-01");
			codeListAttribute1WrapperMock.Setup(x => x.GetWrapperValue(nameof(RefCusCodeListAttribute.ZZE_EndDate))).Returns("2020-01-02");

			//correct record.
			Assert.That(() => enableExpirableChecker.ValidateWrapper(codeListAttribute1WrapperMock.Object), Throws.Nothing);

			var codeListAttribute2WrapperMock = new Mock<IStagingDataWrapper>();
			codeListAttribute2WrapperMock.Setup(x => x.GetStagingType()).Returns(typeof(Schema_New.RefCusCodeListAttribute));
			codeListAttribute2WrapperMock.Setup(x => x.GetWrapperValue(nameof(RefCusCodeListAttribute.ZZE_StartDate))).Returns(null);
			codeListAttribute2WrapperMock.Setup(x => x.GetWrapperValue(nameof(RefCusCodeListAttribute.ZZE_EndDate))).Returns("2020-01-02");

			//incorrect record.
			Assert.That(() => enableExpirableChecker.ValidateWrapper(codeListAttribute2WrapperMock.Object),
				Throws.TypeOf<RefDataProcessingException>().With.Message.Contain("The setup for EnableExpirable is invalid. Current matched StartDate and EndDate values from the database differ from values provided on the XML."));
		}

		[Test]
		public void ValidateNewestObjectFromSafeDb_RefCusCodeListAttribute()
		{
			var newestObjectFromDb = new RefCusCodeListAttribute { ZZE_StartDate = DateTime.Parse("2020-01-01", null), ZZE_EndDate = DateTime.Parse("2020-01-02", null) };
			//correct record.
			Assert.That(() => enableExpirableChecker.ValidateNewestObjectFromSafeDb(newestObjectFromDb), Throws.Nothing);

			newestObjectFromDb.ZZE_StartDate = null;
			newestObjectFromDb.ZZE_EndDate = null;

			//incorrect record.
			Assert.That(() => enableExpirableChecker.ValidateNewestObjectFromSafeDb(newestObjectFromDb),
				Throws.TypeOf<RefDataProcessingException>().With.Message.Contain("The setup for EnableExpirable is invalid. Current matched StartDate and EndDate values from the database differ from values provided on the XML."));
		}

		[Test]
		public void ValidateNewestObjectFromSafeDb_RefCusTariffUOM()
		{
			var newestObjectFromDb = new RefCusTariffUOM { ZZ8_StartDate = DateTime.Parse("2020-01-01", null), ZZ8_EndDate = DateTime.Parse("2020-01-02", null) };
			//correct record.
			Assert.That(() => enableExpirableChecker.ValidateNewestObjectFromSafeDb(newestObjectFromDb), Throws.Nothing);

			newestObjectFromDb.ZZ8_StartDate = null;
			newestObjectFromDb.ZZ8_EndDate = null;

			//incorrect record.
			Assert.That(() => enableExpirableChecker.ValidateNewestObjectFromSafeDb(newestObjectFromDb),
				Throws.TypeOf<RefDataProcessingException>().With.Message.Contain("The setup for EnableExpirable is invalid. Current matched StartDate and EndDate values from the database differ from values provided on the XML."));
		}

		[Test]
		public void ShouldValidateEnableExpirable()
		{
			var types = typeof(RefCusTariff).Assembly.GetTypes().Where(x => x.BaseType != null && x.BaseType.Name == "BaseEntityType");
			foreach (var type in types)
			{
				var shouldValidate = EnableExpirableChecker.ShouldValidateEnableExpirable(type);
				if (type.Name == nameof(RefCusTariffUOM) || type.Name == nameof(RefCusCodeListAttribute))
				{
					Assert.IsTrue(shouldValidate, $"{type.Name} should validate EnableExpirable");
				}
				else
				{
					Assert.IsFalse(shouldValidate, $"{type.Name} should not validate EnableExpirable");
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			metadataProviderMock = new Mock<IMetadataProvider>();
			metadataProviderMock.SetupGet(x => x.EnableExpirable).Returns(true);
			enableExpirableChecker = new EnableExpirableChecker(metadataProviderMock.Object);
		}
	}
}
