using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentDocManagerInfo))]
	public class HVLVConsignmentDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<HVLVConsignment>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<HVLVConsignment>();
	}
}
