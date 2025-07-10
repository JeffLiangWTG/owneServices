using System;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class FilterDataHelperFixture
	{
		[Test]
		public void TestFilterWithParentDataMethodWithGUID()
		{
			var guid = Guid.NewGuid();
			var condition1 = new RefCusCondition { ZX1_PK = guid };
			var refCusConditionValue = new RefCusConditionValue() { ZX3_ZX1_Condition = guid };
			condition1.RefCusConditionValues.Add(refCusConditionValue);
			var refCusCondition = (new[] { condition1 }).AsQueryable();
			var refCusConditionValues = (new[] { refCusConditionValue }).AsQueryable();
			var results = FilterDataHelper.FilterWithParentData<RefCusConditionValue, RefCusCondition>(refCusConditionValues, refCusCondition);
			Assert.AreEqual(1, results.Count());
			var conditionValue = results.FirstOrDefault();
			Assert.AreEqual(refCusConditionValue, conditionValue);
		}

		[Test]
		public void TestFilterWithParentDataMethodWithNullableGUID()
		{
			var guid = Guid.NewGuid();
			var condition1 = new RefCusCondition { ZX1_PK = guid };
			var refCusApplicability = new RefCusApplicability() { ZZT_ZX1_Conditions = guid };
			condition1.RefCusApplicabilities.Add(refCusApplicability);
			var refCusCondition = (new[] { condition1 }).AsQueryable();
			var reCusApplicabilities = (new[] { refCusApplicability }).AsQueryable();
			var results = FilterDataHelper.FilterWithParentData<RefCusApplicability, RefCusCondition>(reCusApplicabilities, refCusCondition);
			Assert.AreEqual(1, results.Count());
			var applicabilityValue = results.FirstOrDefault();
			Assert.AreEqual(refCusApplicability, applicabilityValue);
		}
	}
}
