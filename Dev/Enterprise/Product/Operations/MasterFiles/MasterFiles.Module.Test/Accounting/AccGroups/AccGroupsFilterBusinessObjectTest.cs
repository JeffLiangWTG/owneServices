using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGroupsFilterBusinessObject))]
	sealed class AccGroupsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccGroupsFilterBusinessObject();
		}

		#endregion
	}
}
