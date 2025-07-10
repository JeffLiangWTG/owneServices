using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVBookingHeaderDocManagerInfo))]
	public class HVLVBookingHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<HVLVBookingHeader>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<HVLVBookingHeader>();
	}
}
