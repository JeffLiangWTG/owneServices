using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOriginLoadListDocManagerInfo))]
	public class HVLVOriginLoadListDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<HVLVOriginLoadList>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<HVLVOriginLoadList>();
	}
}
