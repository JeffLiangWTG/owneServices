using Enterprise.Telematics.Module.Filters;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test.Filters
{
	[TestedType(typeof(TelPreDriveChecklistTemplateHeaderFilterBusinessObject))]
	class TelPreDriveChecklistTemplateHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TelPreDriveChecklistTemplateHeaderFilterBusinessObject();
		}
	}
}
