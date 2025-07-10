using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class ExportAWBHeader_StandaloneTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues()
		{
			var header = Factory.New<ExportAWBHeader>();
			AssertEquals("EH_ParentID", header.PK, header.EH_ParentID);
			AssertEquals("EH_Table", ExportAWBHeader.Schema.TableName, header.EH_Table);
			AssertEquals("EH_AWBType", AWBTypeList.Codes.AgentMaster, header.EH_AWBType);
			AssertEquals("EH_AWBStatus", AWBMessagingStatusList.Codes.NotSent, header.EH_AWBStatus);
			AssertEquals("EH_GB_UserBranch", GlbBranch.CurrentBranch.PK, header.EH_GB_UserBranch);
		}

		public void TestHasBeenSent()
		{
			var header = Factory.New<ExportAWBHeader>();
			AssertEquals("Default Value", false, header.HasBeenSent);

			header.EH_AWBStatus = AWBMessagingStatusList.Codes.ErrorRejected;
			AssertEquals("When set to 'ERR'", true, header.HasBeenSent);

			header.EH_AWBStatus = AWBMessagingStatusList.Codes.SentToAirlines;
			AssertEquals("When set to 'SNT'", true, header.HasBeenSent);
		}

		public void TestAWBMessagingStatusDescription()
		{
			var header = Factory.New<ExportAWBHeader>();
			AssertEquals("Default Value", AWBMessagingStatusList.Descriptions.NotSent, header.AWBMessagingStatusDescription);

			header.EH_AWBStatus = AWBMessagingStatusList.Codes.ErrorRejected;
			AssertEquals("When set to valid value 'ERR'", AWBMessagingStatusList.Descriptions.ErrorRejected, header.AWBMessagingStatusDescription);

			header.EH_AWBStatus = ":-)";
			AssertEquals("When set to invalid value ':-)'", ZString.Empty, header.AWBMessagingStatusDescription);
		}

		public void TestYouCanCreateAndSaveMoreThanOneParentlessRow()
		{
			var header1 = Factory.New<ExportAWBHeader>();
			header1.EH_WayBillNumber = "081-11111111";

			var header2 = Factory.New<ExportAWBHeader>();
			header2.EH_WayBillNumber = "081-11111122";

			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			var reloadingFactory = new BusinessObjectFactory();

			var reloadedHeader1 = Factory.Load<ExportAWBHeader>(header1.PK);
			reloadedHeader1.EH_WayBillNumber = "081-11111111";

			var reloadedHeader2 = Factory.Load<ExportAWBHeader>(header2.PK);
			reloadedHeader2.EH_WayBillNumber = "081-11111122";
		}

		public void TestMessagesCollection()
		{
			var header = Factory.New<ExportAWBHeader>();
			var message = header.Messages.AddNew();
			message.EM_ApplicationReference = "1234567888";
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedHeader = reloadingFactory.Load<ExportAWBHeader>(header.PK);
			AssertEquals("reloadedHeader.Messages.Count", 1, reloadedHeader.Messages.Count);
			var reloadedMessage = reloadedHeader.Messages[0];
			AssertEquals("reloadedMessage.EM_LinkUniqueID", reloadedHeader.PK, reloadedMessage.EM_LinkUniqueID);
			AssertEquals("reloadedMessage.EM_LinkTable", ExportAWBHeaderSchema.Constants.TableName, reloadedMessage.EM_LinkTable);
			AssertEquals("reloadedMessage.EM_ApplicationReference", "1234567888", reloadedMessage.EM_ApplicationReference);
		}

		public void TestChildBillsCollections()
		{
			var header = Factory.New<ExportAWBHeader>();
			header.EH_WayBillNumber = "081-11111111";
			var child = header.ChildBills.AddNew();
			child.EH_WayBillNumber = "HB34289239";
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedHeader = reloadingFactory.Load<ExportAWBHeader>(header.PK);
			AssertEquals("reloadedHeader.EH_WayBillNumber", "081-11111111", reloadedHeader.EH_WayBillNumber);
			AssertEquals("reloadedHeader.ChildBills.Count", 1, reloadedHeader.ChildBills.Count);
			var reloadedChild = reloadedHeader.ChildBills[0];
			AssertEquals("reloadedChild.EH_WayBillNumber", "HB34289239", reloadedChild.EH_WayBillNumber);
		}

		public void TestParent()
		{
			var header = Factory.New<ExportAWBHeader>();
			AssertEquals("header.ParentBill", null, header.ParentBill);
			var child = header.ChildBills.AddNew();
			AssertEquals("child.ParentBill", header, child.ParentBill);
		}
	}
}
