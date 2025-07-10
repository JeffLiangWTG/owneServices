using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using StatusCodes = Enterprise.Core.Constants.Customs.CusPollingTransactionStatus.Codes;
using Type = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

public static class FactoryExtensions
{
	public static GlbStaff CreateStaffAndGlbExternalPasswordWithBranch(this BusinessObjectFactory factory, ZString login, ZBool isActive, ZString mailBox, ZString password, ZGuid? branchPK = null)
	{
		if (!branchPK.HasValue)
		{
			var plBranch = factory.NewCompanyAndBranchWith(companyCode: "PL1", branchCode: "PL1", countryCode: Core.Constants.CountryCodes.Poland);
			branchPK = plBranch.PK;
		}

		var staff = factory.New<GlbStaff>();
		staff.GS_IsActive = isActive;
		staff.GS_LoginName = login;
		staff.GS_GB_HomeBranch = branchPK.Value;
		TestHelper.GenerateGlbExternalPasswordPL(staff, mailBox, password, branchPK);

		return staff;
	}

	public static CusPollingTransaction CreatePollingTransactionForStaff(this BusinessObjectFactory factory,
		GlbStaff staff,
		string status = StatusCodes.OPN,
		string type = Type.PLC,
		ZDateTime? earliestTimeOfNextAttemptUtc = null)
	{
		var cusPollingTransaction = factory.New<CusPollingTransaction>();
		cusPollingTransaction.CPT_ApplicationCode = ApplicationCodes.PLCustoms;
		cusPollingTransaction.CPT_Type = type;
		cusPollingTransaction.CPT_Status = status;
		cusPollingTransaction.CPT_ParentTableCode = GlbExternalPasswordSchema.Constants.Prefix;
		var externalPassword = GlbStaffWrapper.Get(staff).GlbExternalPassword;
		cusPollingTransaction.CPT_ParentID = externalPassword.PK;
		cusPollingTransaction.CPT_TransactionID = externalPassword.PK.ToString();
		cusPollingTransaction.CPT_NumberOfAttempts = 0 ;

		if (earliestTimeOfNextAttemptUtc != null)
		{
			var dateTime = earliestTimeOfNextAttemptUtc.Value;
			cusPollingTransaction.CPT_StatusTimeUtc = dateTime.AddHours(-1).TrimSeconds();
			cusPollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = dateTime.TrimSeconds();
		}

		return cusPollingTransaction;
	}

	const string TestMessageText =
@"<ns1:IsztarHistoryRequest xmlns:ns1=""http://www.mf.gov.pl/schematy/isztar/ecipSeapInpParams/2017/01"">
   <ns1:akceptuje_zobowiazanie>true</ns1:akceptuje_zobowiazanie>
   <ns1:startDate>2022-06-23T00:00:00</ns1:startDate>
   <ns1:endDate>2022-06-23T23:59:59</ns1:endDate>
</ns1:IsztarHistoryRequest>";

	internal static EDIMessage CreateCoreMessage(
		this BusinessObjectFactory factory,
		string messageText = TestMessageText,
		string applicationCode = ApplicationCodes.PLCustoms,
		string messageType = EUJobMessageTypeList.Codes.Import,
		string direction = EDIInterchange.Direction.Transmit,
		string status = EDIMessageStatusList.Codes.Queued,
		BusinessObject linkedObject = null,
		BusinessObject password = null,
		bool isTestMessage = true)
	{
		var message = factory.New<EDIMessage>();
		message.EM_IsTestMessage = isTestMessage;
		message.EM_ApplicationCode = applicationCode;
		message.EM_GB = GlbBranch.CurrentBranch.PK;
		message.EM_MessageText = messageText;
		message.EM_MessageType = messageType;
		message.EM_ReceiveTransmit = direction;
		message.EM_Status = status;
		message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
		message.EM_LinkedObject = linkedObject;
		message.EM_GP = password?.PK ?? ZGuid.Empty;
		return message;
	}
}
