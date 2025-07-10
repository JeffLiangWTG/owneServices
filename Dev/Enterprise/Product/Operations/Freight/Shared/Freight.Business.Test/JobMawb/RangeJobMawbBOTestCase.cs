using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(RangeJobMawb))]
	sealed class RangeJobMawbBOTestCase : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("RangeJobMawb BO serves as a proxy for JobMawb which is saved instead", true);
		}
	}
}
