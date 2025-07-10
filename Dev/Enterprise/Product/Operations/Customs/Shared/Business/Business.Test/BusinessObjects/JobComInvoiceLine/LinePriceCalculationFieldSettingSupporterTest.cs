using System;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LinePriceCalculationFieldSettingSupporterTest : TestCase
	{
		public void TestStartAndStop()
		{
			dummyBizObj bizObj = new dummyBizObj();
			AssertNull(bizObj.StartType);
			AssertNull(bizObj.StopType);
			Type type1 = bizObj.GetType();
			using (new LinePriceCalculationFieldSettingSupporter(bizObj, type1))
			{
				AssertEquals(type1, bizObj.StartType);
				AssertNull(bizObj.StopType);
			}
			AssertEquals(type1, bizObj.StartType);
			AssertEquals(type1, bizObj.StopType);
		}

		class dummyBizObj : ILinePriceCalculationFieldSettingSupporter
		{
			#region ILinePriceCalculationFieldSettingSupporter Members

			public void Start(object type)
			{
				StartType = type;
			}
			public object StartType;

			public void Stop(object type)
			{
				StopType = type;
			}
			public object StopType;

			#endregion
		}
	}
}
