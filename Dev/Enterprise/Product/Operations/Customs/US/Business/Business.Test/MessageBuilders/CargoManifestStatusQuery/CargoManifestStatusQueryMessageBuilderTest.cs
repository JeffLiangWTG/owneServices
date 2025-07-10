using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CargoManifestStatusQueryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestQueryWithMessageTypeIN()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "10000657";

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;
			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);

			var builder = new CargoManifestStatusQueryMessageBuilder(declaration, sendingObj);
			var result = builder.GenerateMessages();

			var message = result.EM_MessageText;
			AssertEquals(true, message.Contains("R1    XJ5 10000657"));
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatus, result.EM_MessageType);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;
			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);
			sendingObj.LimitOutputOption = LimitOutputCodeList.Codes._1Last5Results;

			builder = new CargoManifestStatusQueryMessageBuilder(declaration, sendingObj);
			result = builder.GenerateMessages();

			message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1    XJ5 10000657                                                     1"));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
		}

		public void TestQueryForHAWB()
		{
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0018210009999";
			bill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HB123999999999998";
			houseBill.US_UI_NKBillIssuerSCAC = "RE";

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.HAWB;

			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, houseBill);
			sendingObj.RequestForRelatedBOL = true;
			sendingObj.LimitOutputOption = LimitOutputCodeList.Codes._2AllAvailableResults;
			var builder = new CargoManifestStatusQueryMessageBuilder(houseBill.Declaration, sendingObj);
			var result = builder.GenerateMessages();

			var message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1                                            00182100099HB1239999999  2       "));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, declaration.Messages.Contains(result));
			AssertEquals(false, houseBill.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestHAWBQuery, result.EM_MessageSubType);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.HAWB;
			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, houseBill);
			sendingObj.LimitOutputOption = LimitOutputCodeList.Codes._2AllAvailableResults;

			result = builder.GenerateMessages();
			message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1                                            00182100099HB1239999999  2       "));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, declaration.Messages.Contains(result));
			AssertEquals(false, houseBill.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestHAWBQuery, result.EM_MessageSubType);

			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, houseBill);
			sendingObj.LimitOutputOption = LimitOutputCodeList.Codes._2AllAvailableResults;
			builder = new CargoManifestStatusQueryMessageBuilder(houseBill.Declaration, sendingObj);
			result = builder.GenerateMessages();
			message = result.EM_MessageText;
			AssertContains("WR1                                            HB123999999HB1239999999  2       ", message);
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestAirQuery, result.EM_MessageSubType);
		}

		public void TestQueryForEntry()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "10000657";

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;

			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);
			var builder = new CargoManifestStatusQueryMessageBuilder(entry.Declaration, sendingObj);
			var result = builder.GenerateMessages();

			var message = result.EM_MessageText;
			AssertEquals(true, message.Contains("R1    XJ5 10000657"));//entry number should be right-justified
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatus, result.EM_MessageType);
			AssertEquals(true, entry.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestEntryQuery, result.EM_MessageSubType);
		}

		public void TestQueryForRemoteLocationFilingEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_SchDEntry = "2501";
			declaration.US_PreparerDistrictPort = "1101";
			declaration.JE_TransportMode = "SEA";

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1101");

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "10000657";

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;

			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);
			var builder = new CargoManifestStatusQueryMessageBuilder(declaration, sendingObj);
			var result = builder.GenerateMessages().EM_MessageText;

			Assert("B block should have a processing port code from the registry where a broker is registered, not the entry port of a job", result.Substring(0, 80).Contains("1101"));
			Assert("B block should have a processing port code from the registry where a broker is registered, not the entry port of a job", !result.Substring(0, 80).Contains("2501"));

			declaration.JE_MasterBill = "234089234";
			declaration.JE_PrimaryITNumber = "1234567890";
			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);

			builder = new CargoManifestStatusQueryMessageBuilder(declaration, sendingObj);
			result = builder.GenerateMessages().EM_MessageText;
			Assert("B block should have a processing port code from the registry where a broker is registered, not the entry port of a job", result.Substring(0, 80).Contains("1101"));
			Assert("B block should have a processing port code from the registry where a broker is registered, not the entry port of a job", !result.Substring(0, 80).Contains("2501"));

			var itNo = new ITNumber(declaration.PrimaryMasterBill.ITAndSplitDetails[0]);
			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);
			builder = new CargoManifestStatusQueryMessageBuilder(declaration, sendingObj);
			result = builder.GenerateMessages().EM_MessageText;
			Assert("B block should have a processing port code from the registry where a broker is registered, not the entry port of a job", result.Substring(0, 80).Contains("1101"));
			Assert("B block should have a processing port code from the registry where a broker is registered, not the entry port of a job", !result.Substring(0, 80).Contains("2501"));
		}

		public void TestInBondQuery()
		{
			entry.CH_MessageType = "INB";
			entry.EntryNumber = "0000000229999";

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;

			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);
			sendingObj.LimitOutputOption = ZString.Empty;
			var builder = new CargoManifestStatusQueryMessageBuilder(entry.Declaration, sendingObj);
			var result = builder.GenerateMessages();

			var message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1                000000022999                                                 "));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, entry.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestInBondQuery, result.EM_MessageSubType);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);

			builder = new CargoManifestStatusQueryMessageBuilder(entry.Declaration, sendingObj);
			result = builder.GenerateMessages();
			message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1                000000022999                                         2       "));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, entry.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestInBondQuery, result.EM_MessageSubType);
		}

		public void TestOceanRailTruckBill()
		{
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0018210009999999999";
			bill.US_UI_NKBillIssuerSCAC = "APLU";

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;

			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, bill);
			sendingObj.RequestForRelatedBOL = true;
			sendingObj.LimitOutputOption = ZString.Empty;
			var builder = new CargoManifestStatusQueryMessageBuilder(bill.Declaration, sendingObj);
			var result = builder.GenerateMessages();

			var message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1                            APLU001821000999                       Y         "));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, declaration.Messages.Contains(result));
			AssertEquals(false, bill.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery, result.EM_MessageSubType);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, bill);
			sendingObj.RequestForRelatedBOL = true;
			sendingObj.LimitOutputOption = ZString.Empty;
			builder = new CargoManifestStatusQueryMessageBuilder(bill.Declaration, sendingObj);

			result = builder.GenerateMessages();
			message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1                            APLU001821000999                       Y         "));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, declaration.Messages.Contains(result));
			AssertEquals(false, bill.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery, result.EM_MessageSubType);
		}

		public void TestACEQuery()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "10000657";

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;

			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, entry);
			var builder = new CargoManifestStatusQueryMessageBuilder(entry.Declaration, sendingObj);
			var result = builder.GenerateMessages();

			var message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1    XJ5 10000657"));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, entry.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestEntryQuery, result.EM_MessageSubType);

			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0018210009999999999";
			bill.US_UI_NKBillIssuerSCAC = "APLU";

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			USCustomsDataRegistry.Instance.RequestForBillAndEntryData.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);

			sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;

			sendingObj = new CargoManifestQuerySendingObject(sendingHeader, bill);
			sendingObj.RequestForRelatedBOL = true;
			builder = new CargoManifestStatusQueryMessageBuilder(bill.Declaration, sendingObj);
			result = builder.GenerateMessages();

			message = result.EM_MessageText;
			AssertEquals(true, message.Contains("WR1                            APLU001821000999                       Y 2       Y      XJ5CQ"));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result.EM_MessageType);
			AssertEquals(true, declaration.Messages.Contains(result));
			AssertEquals(false, bill.Messages.Contains(result));
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery, result.EM_MessageSubType);
		}

		public void TestQueryInBondStatus_MultipleSendingObjects()
		{
			entry.CH_MessageType = "INB";
			entry.EntryNumber = "0000000229999";

			var sendingHeader = new CargoManifestQueryHeader(Factory);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
			var sendingObj1 = sendingHeader.SendingObjects.AddNew();
			sendingObj1.InBondNumber = "1234001";

			var builder = new CargoManifestStatusQueryMessageBuilder(Factory, sendingHeader.ActionCode, sendingHeader.SendingObjects.OfType<ICargoManifestQuerySendingObject>(), "USPH1", "USPH2");
			var result = builder.GenerateMessages();

			var message = result.EM_MessageText;
			Assert(message.Contains("WR1                1234001                                              2       "));

			var sendingObj2 = sendingHeader.SendingObjects.AddNew();
			sendingObj2.InBondNumber = "1234002";
			var sendingObj3 = sendingHeader.SendingObjects.AddNew();
			sendingObj3.InBondNumber = "1234003";
			builder = new CargoManifestStatusQueryMessageBuilder(Factory, sendingHeader.ActionCode, sendingHeader.SendingObjects.OfType<ICargoManifestQuerySendingObject>(), "USPH1", "USPH2");
			result = builder.GenerateMessages();
			message = result.EM_MessageText;
			Assert(message.Contains(
"WR1                1234001                                              2       " +
"WR1                1234002                                              2       " +
"WR1                1234003                                              2       "));
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			bill = declaration.Bills.AddNew();
			entry = declaration.CustomsEntryHeaders.AddNew();
		}
		JobDeclaration declaration;
		Bill bill;
		CusEntryHeader entry;
	}
}
