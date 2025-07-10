using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(MessagePreSendingValidation))]
sealed class MessagePreSendingValidationTest : MessagePreSendingValidationBaseTest<MessagePreSendingValidation>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("NctsHeader is null", () => new MessagePreSendingValidation(null));
		AssertNoExceptionThrown("NctsHeader passed", () => new MessagePreSendingValidation(Factory.New<NctsHeader>()));
	}

	public void TestValidate_MissingPrincipalOrRepresentative()
	{
		const string expectedErrorMessage = "EORI number of the Representative or Principal is required for generation of the valid LRN.";
		SetCredentials();

		var validation = GetInstanceForTest();
		var orgHeaderWithEori = Factory.New<OrgHeader>();

		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		orgHeader.CustomsCodes.RemoveAll();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "123", "PL");
		orgHeaderWithEori.OH_Code = "Second";
		orgHeaderWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", "PL");
		var principal = nctsHeader.Principal;
		principal.OrganisationPK = orgHeader.PK;

		var notifications = new NotificationCollection();

		CombineAssertions(() =>
		{
			validation.Validate(notifications);
			Assert(!notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			var representative = nctsHeader.MovementHeader.Representative;
			validation.Validate(notifications);
			Assert(notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			representative.OrganisationPK = orgHeaderWithEori.PK;
			validation.Validate(notifications);
			Assert(!notifications.Contains(expectedErrorMessage));
		});
	}

	public void TestValidate_MissingEmail()
	{
		const string expectedErrorMessage = "You have not entered email address for Communication Channel Email. Please enter the Email - either in Credentials tab of Staff record or in Registry Form > Edit System Registry > Customs > Country of Region Specific > Poland > Communication Channel Email.";
		SetCredentials();

		var communicationChannel = GlbStaff.CurrentUser.Factory.New<CommunicationChannel>();
		communicationChannel.GP_PasswordType = PasswordTypesList.Codes.PLC;
		communicationChannel.GP_Name = "CommunicationChannel";
		communicationChannel.GP_GS = GlbStaff.CurrentUser.PK;
		communicationChannel.GP_GC = GlbCompany.CurrentCompany.PK;

		var validation = GetInstanceForTest();
		var notifications = new NotificationCollection();

		PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.Value.IsSeapID = false;
		CombineAssertions(() =>
		{
			validation.Validate(notifications);
			Assert("Email is null", notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			communicationChannel.GP_MailBoxID = "abnc";
			validation.Validate(notifications);
			Assert("Staff Email entered", !notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			communicationChannel.GP_MailBoxID = ZString.Empty;
			using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test"))
			{
				validation.Validate(notifications);
				Assert("Registry Email entered", !notifications.Contains(expectedErrorMessage));
			}
		});
	}

	public void TestValidate_MissingSEAPID()
	{
		const string expectedErrorMessage = "You have not entered email address for Communication Channel Email. Please enter the Email - either in Credentials tab of Staff record or in Registry Form > Edit System Registry > Customs > Country of Region Specific > Poland > Communication Channel Email.";
		SetCredentials();

		var seapId = GlbStaff.CurrentUser.Factory.New<SeapId>();
		seapId.GP_PasswordType = PasswordTypesList.Codes.PLN;
		seapId.GP_Name = "SeapId";
		seapId.GP_GS = GlbStaff.CurrentUser.PK;
		seapId.GP_GC = GlbCompany.CurrentCompany.PK;

		var validation = GetInstanceForTest();
		var notifications = new NotificationCollection();

		PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.Value.IsSeapID = true;
		CombineAssertions(() =>
		{
			validation.Validate(notifications);
			Assert("Registry SeapId is set: Email is null but SeapId is not null", notifications.Contains(expectedErrorMessage));

			notifications.Clear();
			seapId.GP_UserID = "test";
			validation.Validate(notifications);
			Assert("Registry SeapId is set: Email is null but SeapId is not null", !notifications.Contains(expectedErrorMessage));
		});
	}

	protected override MessagePreSendingValidation GetInstanceForTest() => new(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		orgHeader = Factory.New<OrgHeader>();

		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		orgHeader.OH_Code = "Header";
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", "PL");
		nctsHeader.Principal.OrganisationPK = orgHeader.PK;
	}

	NctsHeader nctsHeader;
	OrgHeader orgHeader;
}
