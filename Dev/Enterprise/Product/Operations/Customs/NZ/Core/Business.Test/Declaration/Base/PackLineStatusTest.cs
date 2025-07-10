using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;

	public class PackLineStatusTest : TestCaseWithFactory
	{
		public void TestGetCustomsStatusDescription()
		{
			PackLineStatus packLineStatus = new PackLineStatus(Factory);

			JobDeclaration jobDeclaration1 = CreateJobDeclaration("OBL1", "HBL1", "OCLU111110", FormalEntryStatusList.Codes.NotSentToCustoms);
			JobDeclaration jobDeclaration2 = CreateJobDeclaration("OBL1", "HBL1", "OCLU111110", FormalEntryStatusList.Codes.QueuedForSending);
			JobDeclaration jobDeclaration3 = CreateJobDeclaration("OBL1", "HBL1", "OCLU111110", "");
			JobDeclaration jobDeclaration4 = CreateJobDeclaration("OBL2", "", "OCLU111110", FormalEntryStatusList.Codes.EntryRejected);
			JobDeclaration jobDeclaration5 = CreateJobDeclaration("OBL3", "HBL1", "", FormalEntryStatusList.Codes.EntryRejected);
			JobDeclaration jobDeclaration6 = CreateJobDeclaration("OBL3", "HBL2", "OCLU222220", FormalEntryStatusList.Codes.DeliveryOrderReceived);
			Factory.Save();

			AssertEquals(FormalEntryStatusList.Descriptions.QueuedForSending, ((IPackLineStatus)packLineStatus).GetCustomsStatusDescription("OBL1", "HBL1", "OCLU111110"));
			AssertEquals(FormalEntryStatusList.Descriptions.QueuedForSending, ((IPackLineStatus)packLineStatus).GetCustomsStatusDescription("OBL1", "HBL1", ""));
			AssertEquals("", ((IPackLineStatus)packLineStatus).GetCustomsStatusDescription("OBL1", "", "OCLU111110"));
			AssertEquals(FormalEntryStatusList.Descriptions.EntryRejected, ((IPackLineStatus)packLineStatus).GetCustomsStatusDescription("OBL2", "", "OCLU111110"));
			AssertEquals("", ((IPackLineStatus)packLineStatus).GetCustomsStatusDescription("OBL3", "HBL1", "OCLU111110"));
			AssertEquals(FormalEntryStatusList.Descriptions.EntryRejected, ((IPackLineStatus)packLineStatus).GetCustomsStatusDescription("OBL3", "HBL1", ""));
			AssertEquals(FormalEntryStatusList.Descriptions.DeliveryOrderReceived, ((IPackLineStatus)packLineStatus).GetCustomsStatusDescription("OBL3", "HBL2", "OCLU222220"));
		}

		#region Implementation

		JobDeclaration CreateJobDeclaration(ZString masterBill, ZString houseBill, ZString container1, ZString entryStatus)
		{
			JobDeclaration result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			result.JE_MasterBill = masterBill;
			result.JE_HouseBill = houseBill;
			if (!container1.IsEmpty)
			{
				CusContainer container = result.CusContainers.AddNew();
				container.CO_ContainerNumber = container1;
			}
			result.JE_EntryStatus = entryStatus;
			return result;
		}

		#endregion

	}
}
