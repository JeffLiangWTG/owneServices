using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class MessageSendingActionValidationTest : TestCaseWithFactory
{
	public void TestCheckLRN()
	{
		var expectedMessage = "Customer Reference (LRN) is mandatory. If 'Customer Reference' is not enabled, check if the logon company has an EORI number.";
		CombineAssertions(() =>
		{
			messageSendingAction.Validation.ValidateLRN();
			AssertHasError("Departure - No LRN", messageSendingAction.LRNInfo, expectedMessage);
			commonMovement.BM_PaperlessInbondNum = "123LRN";
			messageSendingAction.Validation.ValidateLRN();
			AssertNoErrorContaining("Departure - LRN filled", messageSendingAction.LRNInfo, expectedMessage);
		});
	}

	public void TestCheckMRN()
	{
		var expectedMessage = "MRN is mandatory.";
		CombineAssertions(() =>
		{
			messageSendingAction.Validation.ValidateMRN();
			AssertNoErrorContaining("Departure - no validation for MRN", messageSendingAction.MRNInfo, expectedMessage);

			CreateNctsHeader(NctsMovementType.Codes.Arrival);
			messageSendingAction.Validation.ValidateMRN();
			AssertHasErrorContaining("Arrival - No MRN", messageSendingAction.MRNInfo, expectedMessage);
			nctsHeader.ArrivalMrnFromUser = "123MRN";
			messageSendingAction.Validation.ValidateMRN();
			AssertNoErrorContaining("Arrival - MRN filled", messageSendingAction.MRNInfo, expectedMessage);
		});
	}

	public void TestCheckAvailableBalance()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		CombineAssertions(() =>
		{
			CreateGuarantee(601m, 600m, "0000001");
			var errorMessage = "The available guarantee balance is ";
			var info = messageSendingAction.MessageTypeInfo;
			messageSendingAction.ShouldSend = true;
			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.Declaration;
			AssertHasErrorContaining(info, errorMessage);

			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.Amendment;
			AssertHasErrorContaining(info, errorMessage);

			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.ArrivalNotification;
			AssertNoErrorContaining("Entry type is not DEC or AMD", info, errorMessage);
			messageSendingAction.ShouldSend = false;
			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.Declaration;
			AssertNoMessageErrorContaining(info, errorMessage);

			CreateNctsHeader(NctsMovementType.Codes.Departure);
			CreateGuarantee(1200m, 600m, "0000002");
			messageSendingAction.ShouldSend = true;
			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.Declaration;
			AssertNoMessageErrorContaining(info, errorMessage);
		});
	}

	void CreateGuarantee(ZDecimal openingBalance, ZDecimal pendingBalance, ZString guaranteeReference)
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();

		var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
		guaranteeHeader.CPH_Number = guaranteeReference;
		guaranteeHeader.CPH_OH_PermitHolder = org.PK;

		var openingBalanceTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		openingBalanceTransaction.CPL_Reference = "Ref";
		openingBalanceTransaction.CPL_TransactionType = "OBL";
		openingBalanceTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
		openingBalanceTransaction.CPL_IsAggregated = true;
		openingBalanceTransaction.CPL_TranValue = openingBalance;

		guaranteeHeader.CPH_Balance = openingBalance - pendingBalance;

		nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;

		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = guaranteeReference;
		guarantee.PW_BondAmount = 10;

		Factory.Save();
	}

	protected override void SetUp()
	{
		base.SetUp();
		CreateNctsHeader(NctsMovementType.Codes.Departure);
	}

	void CreateNctsHeader(ZString movementType)
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);

		if (movementType == NctsMovementType.Codes.Departure)
		{
			commonMovement = nctsHeader.MovementHeader;
			commonMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
		}

		if (movementType == NctsMovementType.Codes.Arrival)
		{
			commonMovement = nctsHeader.ArrivalMovementHeader;
			commonMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		}
		messageSendingAction = new MessageSendingAction(commonMovement);
	}

	NctsHeader nctsHeader;
	NctsCommonMovementHeader commonMovement;
	MessageSendingAction messageSendingAction;
}
