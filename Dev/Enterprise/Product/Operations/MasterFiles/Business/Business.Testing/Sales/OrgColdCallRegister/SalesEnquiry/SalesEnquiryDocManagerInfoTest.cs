using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiry.SalesEnquiryDocManagerInfo))]
	sealed class SalesEnquiryDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<SalesEnquiry>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<SalesEnquiry>();
		}
	}
}
