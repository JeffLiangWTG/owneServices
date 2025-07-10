using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeMessageSenderHelperTest : TestCaseWithFactory
	{
		public void TestGetSequenceForMessage()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YK";
			staff.GS_LoginName = "Yusuf";
			staff.GS_EmailAddress = "yusuf.kamhi@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalTREMessage = Factory.New<ETradeEDIMessage>();
			originalTREMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalTREMessage.EM_MessageType = TRMessageTypes.Codes.TRE;
			originalTREMessage.EM_MessageSubType = "XXX";
			originalTREMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalTREMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			originalTREMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalTREMessage.EM_IsTestMessage = true;
			originalTREMessage.EM_MessageText = "TRE test message";
			originalTREMessage.EM_SystemCreateUser = "YK";
			originalTREMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalTREMessage.EM_LinkUniqueID = header.PK;

			var originalTRSMessage = Factory.New<ETradeEDIMessage>();
			originalTRSMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalTRSMessage.EM_MessageType = TRMessageTypes.Codes.TRS;
			originalTRSMessage.EM_MessageSubType = "XXX";
			originalTRSMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalTRSMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			originalTRSMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalTRSMessage.EM_IsTestMessage = true;
			originalTRSMessage.EM_MessageText = "TRS test message";
			originalTRSMessage.EM_SystemCreateUser = "YK";
			originalTRSMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalTRSMessage.EM_LinkUniqueID = header.PK;

			var originalTRDMessage = Factory.New<ETradeEDIMessage>();
			originalTRDMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalTRDMessage.EM_MessageType = TRMessageTypes.Codes.TRD;
			originalTRDMessage.EM_MessageSubType = "XXX";
			originalTRDMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalTRDMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			originalTRDMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalTRDMessage.EM_IsTestMessage = true;
			originalTRDMessage.EM_MessageText = "TRD test message";
			originalTRDMessage.EM_SystemCreateUser = "YK";
			originalTRDMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalTRDMessage.EM_LinkUniqueID = header.PK;

			var originalTCDMessage = Factory.New<ETradeEDIMessage>();
			originalTCDMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalTCDMessage.EM_MessageType = TRMessageTypes.Codes.TCD;
			originalTCDMessage.EM_MessageSubType = "XXX";
			originalTCDMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalTCDMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			originalTCDMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalTCDMessage.EM_IsTestMessage = true;
			originalTCDMessage.EM_MessageText = "TCD test message";
			originalTCDMessage.EM_SystemCreateUser = "YK";
			originalTCDMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalTCDMessage.EM_LinkUniqueID = header.PK;

			Factory.Save();

			var provider = new ETradeAutoReceiveResponseMessageProvider(header);
			CombineAssertions("Get Sequence For Message", () =>
			{
				AssertEquals("Count", 4, header.Messages.Count);
				AssertEquals("Sequence", "ETG0000001|20201224104|5", ETradeMessageSenderHelper.CreateIncrementedReferenceId(provider, user.GP_UserID));
			});
		}
	}
}
