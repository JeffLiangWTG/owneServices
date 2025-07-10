using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SendScheduleFilterBusinessObject))]
	public class SendScheduleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SendScheduleFilterBusinessObject();
		}

		#endregion
	}
}
