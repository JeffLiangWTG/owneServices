using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSHeader))]
	class SPTSHeaderTests : EnterpriseBusinessObjectTestCase
	{
		public void TestIsInPhase5TransitionPeriod()
		{
			var header = Factory.New<SPTSHeader>();
			Assert(!header.IsInPhase5TransitionPeriod);
		}

		public void TestICusInBondContainerTypeSupporter()
		{
			var header = Factory.New<SPTSHeader>();
			var supporter = header as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(SPTSContainer), supporter.ContainerType);
		}

		public void TestLoadCorrectType()
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondSPTSHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.TRSPTS;

			var message = Factory.New<SPTSMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_ApplicationReference = "2020/00084/5";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_LinkTable = CusInBondHeader.Schema.TableName;
			message.EM_LinkUniqueID = header.PK;
			message.EM_MessageText = "Test";
			message.EM_MessageType = TRMessageTypes.Codes.TSP;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var newFactory = NewFactory();
			var reLoadEdiMessage = newFactory.Load<SPTSMessage>(message.PK);
			var sptsHeader = reLoadEdiMessage.EM_LinkedObject;
			AssertEquals(typeof(SPTSHeader), sptsHeader.GetType());
		}

		public void TestJobReference()
		{
			var header = Factory.New<SPTSHeader>();
			Factory.Save();

			CombineAssertions("JobReference", () =>
			{
				Assert(header.BH_JobReference.StartsWith("SPTS"));
				Assert(header.HumanReadableName.Contains("SPTS Simplified Procedure Transit System SPTS"));
			});
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<SPTSHeader>();
			AssertEquals(Common.CusInBondApplicationCodeList.Codes.TRSPTS, header.BH_ApplicationCode);
		}

		public void TestRegistrationNumber()
		{
			var header = Factory.New<SPTSHeader>();
			header.RegistrationNumber = "AB1234CD";
			Factory.Save();
			AssertEquals("Not Empty", "AB1234CD", header.RegistrationNumber);

			header.RegistrationNumber = ZString.Empty;
			Factory.Save();
			AssertEquals("Empty", ZString.Empty, header.RegistrationNumber);
		}

		public void TestRegistrationDate()
		{
			var header = Factory.New<SPTSHeader>();
			header.RegistrationDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals("Not Empty", ZDateTime.Today, header.RegistrationDate);

			header.RegistrationDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("Empty", ZDateTime.Empty, header.RegistrationDate);
		}

		public void TestBH_VoyageNumber()
		{
			var header = Factory.New<SPTSHeader>();
			header.BH_VoyageNumber = "AB1234CD";
			Factory.Save();
			AssertEquals(header.BH_VoyageNumber, "AB1234CD");

			header.BH_VoyageNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(header.BH_VoyageNumber, "");
		}

		public void TestBH_SailingDate()
		{
			var header = Factory.New<SPTSHeader>();
			header.BH_SailingDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals(header.BH_SailingDate, ZDateTime.Today);

			header.BH_SailingDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals(header.BH_SailingDate, ZDateTime.Empty);
		}

		public void TestChangingAmendSPTS()
		{
			var header = Factory.New<SPTSHeader>();
			header.BH_JobReference = "C123456";
			header.RegistrationNumber = "22340300IM12345678";
			header.RegistrationDate = new ZDateTime(2022, 1, 1);
			var entryNum = CusEntryNumber.Load(header, "SPT", header.CountryCode);
			CombineAssertions("Registered SPTS", () =>
			{
				AssertEquals("BH_JobReference", "C123456", header.BH_JobReference);
				AssertEquals("BH_MessageStatus", ZString.Empty, header.BH_MessageStatus);
				AssertEquals("CE_EntryNum", "22340300IM12345678", entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2022, 1, 1), entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", ZString.Empty, entryNum.CE_ExpiryDate.ToString());
				AssertEquals("CE_EntryLineReference", ZString.Empty, entryNum.CE_EntryLineReference);
			});

			var changeDate = ZDateTime.Now;
			header.ChangeToAmmendSPTS();
			entryNum = CusEntryNumber.Load(header, "SPT", header.CountryCode);
			CombineAssertions("Changing Amend SPTS", () =>
			{
				AssertEquals("BH_JobReference", "C123456-1", header.BH_JobReference);
				AssertEquals("BH_MessageStatus", ZString.Empty, header.BH_MessageStatus);
				AssertEquals("CE_EntryNum", ZString.Empty, entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", ZDateTime.Empty, entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", changeDate.ToString(), entryNum.CE_ExpiryDate.ToString());
				AssertEquals("CE_EntryLineReference", "22340300IM12345678", entryNum.CE_EntryLineReference);
			});

			header.RegistrationNumber = "22340300IM12345679";
			header.RegistrationDate = new ZDateTime(2022, 2, 2);
			entryNum = CusEntryNumber.Load(header, "SPT", header.CountryCode);
			CombineAssertions("New Registered SPTS", () =>
			{
				AssertEquals("BH_JobReference", "C123456-1", header.BH_JobReference);
				AssertEquals("BH_MessageStatus", ZString.Empty, header.BH_MessageStatus);
				AssertEquals("CE_EntryNum", "22340300IM12345679", entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2022, 2, 2), entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", changeDate.ToString(), entryNum.CE_ExpiryDate.ToString());
				AssertEquals("CE_EntryLineReference", "22340300IM12345678", entryNum.CE_EntryLineReference);
			});

			changeDate = ZDateTime.Now;
			header.ChangeToAmmendSPTS();
			entryNum = CusEntryNumber.Load(header, "SPT", header.CountryCode);
			CombineAssertions("Changing Amend SPTS", () =>
			{
				AssertEquals("BH_JobReference", "C123456-2", header.BH_JobReference);
				AssertEquals("BH_MessageStatus", ZString.Empty, header.BH_MessageStatus);
				AssertEquals("CE_EntryNum", ZString.Empty, entryNum.CE_EntryNum);
				AssertEquals("CE_IssueDate", ZDateTime.Empty, entryNum.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", changeDate.ToString(), entryNum.CE_ExpiryDate.ToString());
				AssertEquals("CE_EntryLineReference", "22340300IM12345679", entryNum.CE_EntryLineReference);
			});
		}

		public void TestHumanReadableNameCore()
		{
			var header = Factory.New<SPTSHeader>();
			header.BH_JobReference = "C123456";
			CombineAssertions("HumanReadableNameCore", () =>
			{
				header.RegistrationNumber = "22340300IM12345678";
				AssertEquals("Real Record", "SPTS Simplified Procedure Transit System C123456", header.HumanReadableName);
				header.ChangeToAmmendSPTS();
				AssertEquals("First Ammend", "SPTS Simplified Procedure Transit System C123456-1", header.HumanReadableName);
				header.RegistrationNumber = "22340300IM12345679";
				header.ChangeToAmmendSPTS();
				AssertEquals("Second Ammend", "SPTS Simplified Procedure Transit System C123456-2", header.HumanReadableName);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.New<SPTSHeader>();
	}
}
