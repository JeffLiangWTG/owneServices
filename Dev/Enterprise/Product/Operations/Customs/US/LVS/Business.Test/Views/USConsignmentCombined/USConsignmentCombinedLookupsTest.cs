using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	public class USConsignmentCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportModes()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			Factory.Save();
			var consignmentView = Factory.Load<USConsignmentCombined>(consignment.PK);

			var list = consignmentView.Lookups.TransportModes;

			CombineAssertions(() =>
			{
				AssertEquals(6, list.Count);
				AssertEquals(TransportTypeList.Descriptions.Sea, list[TransportTypeList.Codes.Sea].Description);
				AssertEquals(TransportTypeList.Descriptions.Air, list[TransportTypeList.Codes.Air].Description);
				AssertEquals(TransportTypeList.Descriptions.Road, list[TransportTypeList.Codes.Road].Description);
				AssertEquals(TransportTypeList.Descriptions.Rail, list[TransportTypeList.Codes.Rail].Description);
				AssertEquals(TransportTypeList.Descriptions.Mail, list[TransportTypeList.Codes.Mail].Description);
				AssertEquals(TransportTypeList.Descriptions.Truck, list[TransportTypeList.Codes.Truck].Description);
			});
		}

		public void TestJobTypes()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			Factory.Save();
			var consignmentView = Factory.Load<USConsignmentCombined>(consignment.PK);

			var list = consignmentView.Lookups.JobTypes;

			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(USConsignmentCombinedJobTypes.Descriptions.Consignment, list[USConsignmentCombinedJobTypes.Codes.Consignment].Description);
				AssertEquals(USConsignmentCombinedJobTypes.Descriptions.Declaration, list[USConsignmentCombinedJobTypes.Codes.Declaration].Description);
			});
		}
	}
}
