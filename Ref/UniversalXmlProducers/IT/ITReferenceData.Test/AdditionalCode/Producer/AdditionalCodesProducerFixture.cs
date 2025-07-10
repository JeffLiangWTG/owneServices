using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class AdditionalCodesProducerFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new AdditionalCodesProducer(loader: null, mapper: new RawAdditionalCodeMapper()), "When loader is null");
			Assert.Throws<ArgumentNullException>(() => new AdditionalCodesProducer(loader: rawAdditionalCodesLoader.Object, mapper: null), "When mapper is null");
		}

		[Test]
		public void ProduceEntities()
		{
			rawAdditionalCodesLoader.Setup(x => x.GetRawAdditionalCodes()).Returns(GetRawAdditionalCodes().ToArray());
			var additionalCodesProducer = new AdditionalCodesProducer(rawAdditionalCodesLoader.Object, new RawAdditionalCodeMapper());
			Assert.AreEqual(3, additionalCodesProducer.ProduceEntities().Count(), "Number of produced RefCusCodeList elements");
		}

		IEnumerable<IRawAdditionalCode> GetRawAdditionalCodes()
		{
			yield return new Mock<IRawAdditionalCode>().Object;
			yield return new Mock<IRawAdditionalCode>().Object;
			yield return new Mock<IRawAdditionalCode>().Object;
		}

		[SetUp]
		protected void SetUp()
		{
			rawAdditionalCodesLoader = new Mock<IRawAdditionalCodesLoader>();
		}

		Mock<IRawAdditionalCodesLoader> rawAdditionalCodesLoader;
	}
}
