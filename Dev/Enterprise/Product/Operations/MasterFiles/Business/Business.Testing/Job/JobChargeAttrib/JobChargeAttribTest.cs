using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobChargeAttrib))]
	sealed class JobChargeAttribTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestNotComparableAttributes()
		{
			var expectedNotComparableAttributes = new List<string>()
			{
				JobChargeAttribTypeList.Codes.MinimumRateUsed,
				JobChargeAttribTypeList.Codes.ItemsToRate,
				JobChargeAttribTypeList.Codes.CartageZoneDescription,
				JobChargeAttribTypeList.Codes.ItemsToRateUnit,
				JobChargeAttribTypeList.Codes.UnroundedItemsToRate,
				JobChargeAttribTypeList.Codes.RateId,
				JobChargeAttribTypeList.Codes.CalculatorDescription
			};

			var actualNotComparableAttributes = new JobChargeAttribTypeList().GetAllCodes().Where(code => !JobChargeAttrib.IsComparableType(code));

			AssertContainsExactElementsInAnyOrder(expectedNotComparableAttributes, actualNotComparableAttributes);
		}
	}
}
