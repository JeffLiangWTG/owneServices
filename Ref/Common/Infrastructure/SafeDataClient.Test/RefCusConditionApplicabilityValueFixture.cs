using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test.NonPersistentObject
{
	[TestFixture]
	public class RefCusConditionApplicabilityValueFixture
	{
		[Test]
		public void Create()
		{
			var condVal = new RefCusConditionValue
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = Guid.NewGuid(),
				ZX3_Value = "ABC"
			};
			var condAppVal = new RefCusConditionApplicabilityValue(condVal, new RefCusConditionApplicability(safeRepository));
			Assert.AreEqual(condVal, condAppVal.RefCusConditionValue);
			Assert.IsTrue(condVal.RefCusConditionApplicabilityValues.Contains(condAppVal));
			Assert.AreEqual("ABC", condAppVal.S08_Value);
		}

		[Test]
		public void Link()
		{
			var condVal = new RefCusConditionValue
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = Guid.NewGuid()
			};
			var condAppVal = new RefCusConditionApplicabilityValue(new RefCusConditionApplicability(safeRepository));
			condAppVal.Link(condVal);
			Assert.AreEqual(condVal, condAppVal.RefCusConditionValue);
			Assert.IsTrue(condVal.RefCusConditionApplicabilityValues.Contains(condAppVal));
		}

		[Test]
		public void Update()
		{
			var condVal = new RefCusConditionValue
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = Guid.NewGuid(),
				ZX3_Value= "ABC"
			};
			var condAppVal = new RefCusConditionApplicabilityValue(new RefCusConditionApplicability(safeRepository)) { S08_Value = "BCD" };
			condAppVal.Link(condVal);
			Assert.AreNotEqual("BCD", condVal.ZX3_Value);
			condAppVal.Update();
			Assert.AreEqual("BCD", condVal.ZX3_Value);
		}

		[Test]
		public void UnLink()
		{
			var condVal = new RefCusConditionValue
			{
				ZX3_PK = Guid.NewGuid(),
				ZX3_ZX1_Condition = Guid.NewGuid()
			};
			var condAppVal = new RefCusConditionApplicabilityValue(condVal, new RefCusConditionApplicability(safeRepository));
			var result = condAppVal.Unlink();
			Assert.That(result, Is.Not.Empty);
			Assert.AreEqual(condVal, result.First());
			Assert.IsNull(condAppVal.RefCusConditionValue);
			Assert.IsFalse(condVal.RefCusConditionApplicabilityValues.Contains(condAppVal));
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
