using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test.NonPersistentObject
{
	[TestFixture]
	public class RefCusConditionApplicabilityLanguageFixture
	{
		[Test]
		public void Create()
		{
			var condVal = new RefCusConditionLanguage
			{
				ZXJ_PK = Guid.NewGuid(),
				ZXJ_ZX1_Condition = Guid.NewGuid()
			};
			var condAppVal = new RefCusConditionApplicabilityLanguage(condVal, new RefCusConditionApplicability(safeRepository));
			Assert.AreEqual(condVal, condAppVal.RefCusConditionLanguage);
			Assert.IsTrue(condVal.RefCusConditionApplicabilityLanguages.Contains(condAppVal));
		}

		[Test]
		public void Link()
		{
			var condVal = new RefCusConditionLanguage
			{
				ZXJ_PK = Guid.NewGuid(),
				ZXJ_ZX1_Condition = Guid.NewGuid()
			};
			var condAppVal = new RefCusConditionApplicabilityLanguage(new RefCusConditionApplicability(safeRepository));
			condAppVal.Link(condVal);
			Assert.AreEqual(condVal, condAppVal.RefCusConditionLanguage);
			Assert.IsTrue(condVal.RefCusConditionApplicabilityLanguages.Contains(condAppVal));
		}

		[Test]
		public void Update()
		{
			var condVal = new RefCusConditionLanguage
			{
				ZXJ_PK = Guid.NewGuid(),
				ZXJ_ZX1_Condition = Guid.NewGuid(),
				ZXJ_Comment = "ABC"
			};
			var condAppVal = new RefCusConditionApplicabilityLanguage(new RefCusConditionApplicability(safeRepository)) { S09_Comment = "BCD" };
			condAppVal.Link(condVal);
			Assert.AreNotEqual("BCD", condVal.ZXJ_Comment);
			condAppVal.Update();
			Assert.AreEqual("BCD", condVal.ZXJ_Comment);
		}

		[Test]
		public void UnLink()
		{
			var condVal = new RefCusConditionLanguage
			{
				ZXJ_PK = Guid.NewGuid(),
				ZXJ_ZX1_Condition = Guid.NewGuid()
			};
			var condAppVal = new RefCusConditionApplicabilityLanguage(condVal, new RefCusConditionApplicability(safeRepository));
			var result = condAppVal.Unlink();
			Assert.That(result, Is.Not.Empty);
			Assert.AreEqual(condVal, result.First());
			Assert.IsNull(condAppVal.RefCusConditionLanguage);
			Assert.IsFalse(condVal.RefCusConditionApplicabilityLanguages.Contains(condAppVal));
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
