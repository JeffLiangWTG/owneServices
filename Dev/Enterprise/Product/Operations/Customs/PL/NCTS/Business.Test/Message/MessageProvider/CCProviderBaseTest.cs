using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CCProviderBaseTest : Customs.Business.Testing.DataProviderTestCase<CCProviderBase>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsCommonMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CCProviderBaseForTests(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsCommonMovementHeader.Header", () => new CCProviderBaseForTests(Factory.New<NctsDepartureMovementHeader>(), null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsCommonMovementHeader.Header", () => new CCProviderBaseForTests(Factory.New<NctsArrivalMovementHeader>(), null));
			AssertExceptionThrown<ArgumentNullException>("Null Message Type", "Value cannot be null.\r\nParameter name: messageType", () => new CCProviderBaseForTests(movementHeader, null));
		});
	}

	public void TestAuthorisation()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(Provider.Authorisation);
			AssertEquals(GetMessage(0), 0, GetProvider().Authorisation.Count);

			var cuaNew1 = movementHeader.CusAuthorizationUsages.AddNew();
			cuaNew1.AGC_Code = CusPermitHeaderTypesList.Codes.ACR;
			cuaNew1.AGC_Number = ZGuid.NewZGuid().ToString().Substring(35);
			AssertEquals(GetMessage(1), 1, GetProvider().Authorisation.Count);

			for (var i = 2; i <= 9; i++)
			{
				var cuaNew9 = movementHeader.CusAuthorizationUsages.AddNew();
				cuaNew9.AGC_Code = CusPermitHeaderTypesList.Codes.SSE;
				cuaNew9.AGC_Number = ZGuid.NewZGuid().ToString().Substring(35);
			}
			AssertEquals(GetMessage(9), 9, GetProvider().Authorisation.Count);

			var cuaNew10 = movementHeader.CusAuthorizationUsages.AddNew();
			cuaNew10.AGC_Code = CusPermitHeaderTypesList.Codes.TRD;
			cuaNew10.AGC_Number = ZGuid.NewZGuid().ToString().Substring(35);
			AssertEquals(10, movementHeader.CusAuthorizationUsages.Count);
			AssertEquals(GetMessage(10), 10, GetProvider().Authorisation.Count);
		});
		string GetMessage(int countIn) => $"{nameof(movementHeader.CusAuthorizationUsages)}.Count = {countIn}";
	}

	public void TestCorrelationIdentifier() => AssertNull(Provider.CorrelationIdentifier);

	public void TestMessageIdentification() => AssertEquals("_PL_MSGNO_PLACEHOLDER_", Provider.MessageIdentification);

	public void TestMessageRecipient() => AssertEquals(Constants.MessageRecipient, Provider.MessageRecipient);

	public void TestMessageSender()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Eori", string.Empty, Provider.MessageSender);

			var companyOrg = GlbCompany.CurrentCompany.OrgProxy;
			companyOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("GlbCompany Eori exists", "PL111", GetProvider().MessageSender);

			companyOrg.CustomsCodes.RemoveAll();
			var branchOrg = GlbBranch.CurrentBranch.OrgProxy;
			branchOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "222", CountryCodes.Poland);
			AssertEquals("GlbBranch Eori exists", "PL222", GetProvider().MessageSender);
		});
	}

	public void TestMessageType() => AssertEquals(testMessageType, Provider.MessageType);

	public void TestPreparationDateAndTime()
	{
		var plDateTime1 = GetPLTimeNow();
		var preparationTime = ProviderWithPLBranch().PreparationDateAndTime;
		var plDateTime2 = GetPLTimeNow();

		Assert($"PreparationDateAndTime {preparationTime} should be been between {plDateTime1} and {plDateTime2}", preparationTime >= plDateTime1 && preparationTime <= plDateTime2);
		DateTime GetPLTimeNow() => EnvProxy.Instance.Time.GetUnlocoDateTime("PLWAW");
	}

	protected override CCProviderBase GetProvider() => new CCProviderBaseForTests(movementHeader, testMessageType);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}

	const string testMessageType = "IETSTG";
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;

	CCProviderBase ProviderWithPLBranch()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		company.GC_RN_NKCountryCode = CountryCodes.Poland;
		var branch = company.Branches.AddNew();
		branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
		var header = Factory.New<NctsHeader>();
		header.BH_GB = branch.PK;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		return new CCProviderBaseForTests(header.MovementHeader, testMessageType);
	}

	sealed class CCProviderBaseForTests : CCProviderBase
	{
		public CCProviderBaseForTests(NctsCommonMovementHeader movementHeader, string messageType) : base(movementHeader, messageType)
		{ }
	}
}
