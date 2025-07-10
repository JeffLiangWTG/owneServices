using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiryCollection))]
	sealed class SalesEnquiryCollectionTest : ActiveBusinessObjectCollectionTestCase<SalesEnquiryCollection>
	{
		protected override SalesEnquiryCollection GetCollectionToTest()
		{
			return new SalesEnquiryCollection(Factory);
		}
	}
}
