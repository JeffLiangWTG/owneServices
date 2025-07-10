using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCarrierConsortiumFilterBusinessObject))]
	sealed class RefCarrierConsortiumFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCarrierConsortiumFilterBusinessObject();
		}

		#endregion
	}
}
