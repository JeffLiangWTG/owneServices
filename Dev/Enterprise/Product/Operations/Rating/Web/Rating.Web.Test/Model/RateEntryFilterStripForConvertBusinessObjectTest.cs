using Enterprise.Rating.Web.Model.Conversion;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model
{
	[TestedType(typeof(RateEntryFilterStripForConvertBusinessObject))]
	public class RateEntryFilterStripForConvertBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RateEntryFilterStripForConvertBusinessObject();
		}
	}
}
