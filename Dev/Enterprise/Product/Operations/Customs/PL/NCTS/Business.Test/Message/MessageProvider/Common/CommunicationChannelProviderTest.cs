using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CommunicationChannelProviderTest : Customs.Business.Testing.DataProviderTestCase<CommunicationChannelProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null GlbStaff", "Value cannot be null.\r\nParameter name: glbStaff", () => new CommunicationChannelProvider(null));
		});
	}

	public void TestEmailChannelAddress()
	{
		using (PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new PLDefaultCommunicationChannelNCTSP5()))
		{
			AssertNull(GetProvider().EmailChannelAddress);
		}
	}

	public void TestEmailChannelAddressWithDefaultCommunicationChannelNCTSP5IsEmail()
	{
		CombineAssertions(() =>
		{
			using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test"))
			using (PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new PLDefaultCommunicationChannelNCTSP5()))
			{
				AssertEquals("Empty staff communication channel, should use Registry data", "Test", GetProvider().EmailChannelAddress);

				communicationChannel.GP_MailBoxID = "something@email.com";

				AssertEquals("Staff Communication Channel have priority over Registry data", "something@email.com", GetProvider().EmailChannelAddress);
			}
		});
	}

	public void TestSingleEntryAccessPointId()
	{
		CombineAssertions(() =>
		{
			seapId.GP_UserID = "";
			AssertEquals(null, GetProvider().SingleEntryAccessPointId);

			var gpUserID = "UVCBBKTWTPE00129-SWT";
			seapId.GP_UserID = gpUserID;
			AssertEquals(gpUserID, GetProvider().SingleEntryAccessPointId);

			using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test"))
			using (PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new PLDefaultCommunicationChannelNCTSP5()))
			{
				seapId.GP_UserID = gpUserID;
				AssertEquals(null, GetProvider().SingleEntryAccessPointId);
			}
		});
	}

	public void TestWebServiceUrl()
	{
		AssertNull(Provider.WebServiceUrl);
	}

	public void TestElectronicPlatformChannel()
	{
		AssertNull(Provider.ElectronicPlatformChannel);
	}

	protected override CommunicationChannelProvider GetProvider() => new CommunicationChannelProvider(GlbStaff.CurrentUser);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.CustomsOffices.RemoveAll();

		seapId = GlbStaff.CurrentUser.Factory.New<SeapId>();
		seapId.GP_PasswordType = PasswordTypesList.Codes.PLN;
		seapId.GP_Name = "SeapId";
		seapId.GP_GS = GlbStaff.CurrentUser.PK;
		seapId.GP_GC = GlbCompany.CurrentCompany.PK;

		communicationChannel = GlbStaff.CurrentUser.Factory.New<CommunicationChannel>();
		communicationChannel.GP_PasswordType = PasswordTypesList.Codes.PLC;
		communicationChannel.GP_Name = "CommunicationChannel";
		communicationChannel.GP_GS = GlbStaff.CurrentUser.PK;
		communicationChannel.GP_GC = GlbCompany.CurrentCompany.PK;
	}

	NctsHeader nctsHeader;

	SeapId seapId;
	CommunicationChannel communicationChannel;
}
