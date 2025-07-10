using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class JobDocAddressConsigneeTest : TestCaseWithFactory
	{
		public void TestIConsigneeMember()
		{
			var docAddress = Factory.New<JobDocAddress>();
			AssertEquals("", ((IConsignee)new JobDocAddressConsignee(docAddress)).TypeCode);
			docAddress.DocAddressType = DocAddressType.ConsigneeAddress;
			AssertEquals("U", ((IConsignee)new JobDocAddressConsignee(docAddress)).TypeCode);
			docAddress.DocAddressType = DocAddressType.IntermediateConsigneeAddress;
			AssertEquals("I", ((IConsignee)new JobDocAddressConsignee(docAddress)).TypeCode);
		}
	}
}
