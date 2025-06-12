using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.CDS.BT.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Tests.Helpers
{
	[TestClass]
	public class CredentialsHelpersTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CredentialsHelpers_GetAccessToken()
		{
			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>
			{
				new eHubClientSystemRegistration
				{
					CD_Qualifier = "CURRENT",
					CD_Code = "CCC123456",
					CD_Flag1 = 0,
					CD_Attr1 = "1234567890",
					eHubClientSystem = new eHubClientSystem {EH_ID = "AAABBB"},
					eHubRegistrationType = new eHubRegistrationType {RT_ID = "GBCustomsAccessToken"}
				}
			};
			var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockeHubTransactions.Stub(x => x.eHubClientSystemRegistrations).Return(eHubClientSystemRegistrations);
			CredentialsHelpers.ContextFactory = () => mockeHubTransactions;

			var result = CredentialsHelpers.GetAccessToken("AAABBB.CCC123456");

			Assert.AreEqual("1234567890", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CredentialsHelpers_GetEoriBadge()
		{
			var result = CredentialsHelpers.GetEoriBadge("HYEMIK.GB896458895023.D01");
			Assert.AreEqual("GB896458895023.D01", result);

			result = CredentialsHelpers.GetEoriBadge("HYE123MIK.GB896458895023.D01");
			Assert.AreEqual("GB896458895023.D01", result);

			result = CredentialsHelpers.GetEoriBadge("GB896458895023");
			Assert.AreEqual("GB896458895023", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CredentialsHelpers_RequestAccessTokenRefresh()
		{
			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>
			{
				new eHubClientSystemRegistration
				{
					CD_Qualifier = "CURRENT",
					CD_Code = "CCC123456",
					CD_Flag1 = 0,
					CD_Attr1 = "1234567890",
					eHubClientSystem = new eHubClientSystem {EH_ID = "AAABBB"},
					eHubRegistrationType = new eHubRegistrationType {RT_ID = "GBCustomsAccessToken"}
				}
			};
			var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockeHubTransactions.Stub(x => x.eHubClientSystemRegistrations).Return(eHubClientSystemRegistrations);
			CredentialsHelpers.ContextFactory = () => mockeHubTransactions;

			CredentialsHelpers.RequestAccessTokenRefresh("AAABBB.CCC123456", "1234567890");

			byte expectedFlag1 = 1;
			Assert.AreEqual(expectedFlag1, eHubClientSystemRegistrations.First().CD_Flag1);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CredentialsHelpers_GetSqlXmlWhenAccessTokenIsFailedToBeObtained()
		{
			var guids = new Queue<Guid>(new Guid[]
				{new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"), new Guid("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB")});
			CredentialsHelpers.GuidFactory = guids.Dequeue;

			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>
			{
				new eHubClientSystemRegistration
				{
					CD_Qualifier = "CURRENT",
					CD_Code = "GB123456789000.ABC",
					CD_Flag1 = 0,
					CD_Attr1 = "1234567890",
					eHubClientSystem = new eHubClientSystem {EH_ID = "HYECMT"},
					eHubRegistrationType = new eHubRegistrationType {RT_ID = "GBCustomsAccessToken"},
					CD_ExpiryUTC = DateTime.UtcNow.AddHours(-1)
				}
			};

			var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockeHubTransactions.Stub(x => x.eHubClientSystemRegistrations).Return(eHubClientSystemRegistrations);
			CredentialsHelpers.ContextFactory = () => mockeHubTransactions;

			var result = CredentialsHelpers.GetSqlXmlWhenAccessTokenIsFailedToBeObtained("HYECMT.GB123456789000.ABC", "500 Internal Server Error",
				"33333333-3333-3333-3333-333333333333", "AAAAAAABlkI=", "00000000-0000-0000-0000-000000000000",
				"11111111-1111-1111-1111-111111111111", "99999999-9999-9999-9999-999999999999");

			Assert.AreEqual(
@"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</EI_PK>
        <EI_MessageTrackingID>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID></EI_EnvelopeTrackingID>
        <EI_CC_Sender>00000000-0000-0000-0000-000000000000</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://www.wisetechglobal.com/Schemas/Configuration#Configuration</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>HUB</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>H4sIAAAAAAAEAHWQz07CQBDGX2WyZ8MWFf+lNKELwR7gYCuJx6UM0NjuktmphWfz4CP5Cm6rqJh42sz3zfeb2Xl/fQuVNetiU5PmwhqY6wqHYhqr2rGtnBqnAhZIzntD0e8FAvZVadxQbJl3d1I2TdNrCoeM+XZT2qUue7mtZJpvsdJOnsBFFE7J1jvIDjs/JD04xkrAA66R0OReun+aqFn2p6/bIWXNtR+bzBfeHhljuWO6KEw85WvvzD6jaVMiGuU5OhfK1j3pmXlZb3yHsnW5Ag8Cu2RdGNBdBLiFnMEgCCAxjGR0CSnSCxJMiCwdmfK/LWa6KGO7T8Yimsb984vLwdX1zW0QBMekIlyh4cKDPxOqJn8B9j97dEitFo1iFcrvKpQ/GV905/n1npw5+gBFrQTC1gEAAA==</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</EX_EI_Inbox>
        <EX_XmlContent>&lt;Configuration Name=""GBCustomsCDS"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""&gt;&lt;Group Type=""System"" Reference=""HYECMT""&gt;&lt;Group Type=""CDS"" Status=""INV""&gt;&lt;Annotations&gt;&lt;Item Name=""TokenType""&gt;Access&lt;/Item&gt;&lt;Item Name=""Message""&gt;Could not obtain access token, 500 Internal Server Error&lt;/Item&gt;&lt;/Annotations&gt;&lt;Item Name=""MailBoxID""&gt;GB123456789000&lt;/Item&gt;&lt;Credential Name=""Current""&gt;&lt;UserName&gt;ABC&lt;/UserName&gt;&lt;/Credential&gt;&lt;/Group&gt;&lt;/Group&gt;&lt;/Configuration&gt;</EX_XmlContent>
        <EX_DT_Source>99999999-9999-9999-9999-999999999999</EX_DT_Source>
        <EX_UncompressedLength>470</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb</OI_PK>
        <OI_MessageTrackingID>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</OI_MessageTrackingID>
        <OI_EI_InboxPK>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</OI_EI_InboxPK>
        <OI_CC_Sender>00000000-0000-0000-0000-000000000000</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>99999999-9999-9999-9999-999999999999</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>H4sIAAAAAAAEAHWQz07CQBDGX2WyZ8MWFf+lNKELwR7gYCuJx6UM0NjuktmphWfz4CP5Cm6rqJh42sz3zfeb2Xl/fQuVNetiU5PmwhqY6wqHYhqr2rGtnBqnAhZIzntD0e8FAvZVadxQbJl3d1I2TdNrCoeM+XZT2qUue7mtZJpvsdJOnsBFFE7J1jvIDjs/JD04xkrAA66R0OReun+aqFn2p6/bIWXNtR+bzBfeHhljuWO6KEw85WvvzD6jaVMiGuU5OhfK1j3pmXlZb3yHsnW5Ag8Cu2RdGNBdBLiFnMEgCCAxjGR0CSnSCxJMiCwdmfK/LWa6KGO7T8Yimsb984vLwdX1zW0QBMekIlyh4cKDPxOqJn8B9j97dEitFo1iFcrvKpQ/GV905/n1npw5+gBFrQTC1gEAAA==</OI_Content>
        <OI_XmlContent>&lt;Configuration Name=""GBCustomsCDS"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""&gt;&lt;Group Type=""System"" Reference=""HYECMT""&gt;&lt;Group Type=""CDS"" Status=""INV""&gt;&lt;Annotations&gt;&lt;Item Name=""TokenType""&gt;Access&lt;/Item&gt;&lt;Item Name=""Message""&gt;Could not obtain access token, 500 Internal Server Error&lt;/Item&gt;&lt;/Annotations&gt;&lt;Item Name=""MailBoxID""&gt;GB123456789000&lt;/Item&gt;&lt;Credential Name=""Current""&gt;&lt;UserName&gt;ABC&lt;/UserName&gt;&lt;/Credential&gt;&lt;/Group&gt;&lt;/Group&gt;&lt;/Configuration&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
  <Update xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration"">
    <Rows>
      <RowPair>
        <After>
          <CD_Flag1 xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">255</CD_Flag1>
        </After>
        <Before>
          <CD_PK xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">33333333-3333-3333-3333-333333333333</CD_PK>
          <CD_RV xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">AAAAAAABlkI=</CD_RV>
        </Before>
      </RowPair>
    </Rows>
  </Update>
</CompositeOperation>", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CredentialsHelpers_GetSqlXmlWhenAccessTokenIsFailedToBeObtainedButStillValid()
		{
			var guids = new Queue<Guid>(new Guid[]
				{new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"), new Guid("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB")});
			CredentialsHelpers.GuidFactory = guids.Dequeue;

			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>
			{
				new eHubClientSystemRegistration
				{
					CD_Qualifier = "CURRENT",
					CD_Code = "GB123456789000.ABC",
					CD_Flag1 = 1,
					CD_Attr1 = "1234567890",
					eHubClientSystem = new eHubClientSystem {EH_ID = "HYECMT"},
					eHubRegistrationType = new eHubRegistrationType {RT_ID = "GBCustomsAccessToken"},
					CD_ExpiryUTC = DateTime.UtcNow.AddHours(1)
				}
			};

			var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockeHubTransactions.Stub(x => x.eHubClientSystemRegistrations).Return(eHubClientSystemRegistrations);
			CredentialsHelpers.ContextFactory = () => mockeHubTransactions;

			var result = CredentialsHelpers.GetSqlXmlWhenAccessTokenIsFailedToBeObtained("HYECMT.GB123456789000.ABC", "500 Internal Server Error",
				"33333333-3333-3333-3333-333333333333", "AAAAAAABlkI=", "00000000-0000-0000-0000-000000000000",
				"11111111-1111-1111-1111-111111111111", "99999999-9999-9999-9999-999999999999");

			Assert.AreEqual(
@"<CompositeOperation>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
    <Rows>
      <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EI_PK>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</EI_PK>
        <EI_MessageTrackingID>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</EI_MessageTrackingID>
        <EI_EnvelopeTrackingID></EI_EnvelopeTrackingID>
        <EI_CC_Sender>00000000-0000-0000-0000-000000000000</EI_CC_Sender>
        <EI_CC_Recipient>11111111-1111-1111-1111-111111111111</EI_CC_Recipient>
        <EI_MessageType>http://www.wisetechglobal.com/Schemas/Configuration#Configuration</EI_MessageType>
        <EI_IsFlatFile>false</EI_IsFlatFile>
        <EI_ApplicationCode>HUB</EI_ApplicationCode>
        <EI_Status>2</EI_Status>
        <EI_Content>H4sIAAAAAAAEAHVRy27CMBD8lZXPFQ5t6UshUmIQRSo9NBSpRxMWYtWxkb0h8G099JP6C3XSJ5UqWbJ2dmZ2vH57eY2FNWu1qZ0kZQ3cywqHbJKJ2pOtvBjlDBbofOgNWb8XMdhX2vghK4m2N5w3TdNrlEfCotxou5S6V9iK50WJlfT8yJwl8cTZegvzwzYMyQ+esGLwgGt0aIoA3T6NxWz+h9dlyElSHcYu0rvQTo2x1Hn6JJ4Gl8/cc/uMplWxJC0K9D7mbfeIMwuw3ASGsLVeQTACh2uHvgQqEWSnA2qdTmBZE6hwPHhSWsNOarXqwSCKYGoInZEacnQ7dDB2zrqvefy/hDOpdGb30xFLJln/9Ox8cHF5dR1F0ZdSOFyhIRWMPxSidmE7FF796NG1WJJmIubfVcx/NKHoVvfrPvqC5B1hqTfD8gEAAA==</EI_Content>
      </eHubInboxMessage>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent"">
    <Rows>
      <eHubInboxXmlContent xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <EX_EI_Inbox>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</EX_EI_Inbox>
        <EX_XmlContent>&lt;Configuration Name=""GBCustomsCDS"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""&gt;&lt;Group Type=""System"" Reference=""HYECMT""&gt;&lt;Group Type=""CDS"" Status=""VAL""&gt;&lt;Annotations&gt;&lt;Item Name=""TokenType""&gt;Access&lt;/Item&gt;&lt;Item Name=""Message""&gt;Could not refresh the access token, but it is still valid. 500 Internal Server Error&lt;/Item&gt;&lt;/Annotations&gt;&lt;Item Name=""MailBoxID""&gt;GB123456789000&lt;/Item&gt;&lt;Credential Name=""Current""&gt;&lt;UserName&gt;ABC&lt;/UserName&gt;&lt;/Credential&gt;&lt;/Group&gt;&lt;/Group&gt;&lt;/Configuration&gt;</EX_XmlContent>
        <EX_DT_Source>99999999-9999-9999-9999-999999999999</EX_DT_Source>
        <EX_UncompressedLength>498</EX_UncompressedLength>
      </eHubInboxXmlContent>
    </Rows>
  </Insert>
  <Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage"">
    <Rows>
      <eHubOutboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
        <OI_PK>bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb</OI_PK>
        <OI_MessageTrackingID>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</OI_MessageTrackingID>
        <OI_EI_InboxPK>aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa</OI_EI_InboxPK>
        <OI_CC_Sender>00000000-0000-0000-0000-000000000000</OI_CC_Sender>
        <OI_CC_Recipient>11111111-1111-1111-1111-111111111111</OI_CC_Recipient>
        <OI_DT_Target>99999999-9999-9999-9999-999999999999</OI_DT_Target>
        <OI_Status>0</OI_Status>
        <OI_Content>H4sIAAAAAAAEAHVRy27CMBD8lZXPFQ5t6UshUmIQRSo9NBSpRxMWYtWxkb0h8G099JP6C3XSJ5UqWbJ2dmZ2vH57eY2FNWu1qZ0kZQ3cywqHbJKJ2pOtvBjlDBbofOgNWb8XMdhX2vghK4m2N5w3TdNrlEfCotxou5S6V9iK50WJlfT8yJwl8cTZegvzwzYMyQ+esGLwgGt0aIoA3T6NxWz+h9dlyElSHcYu0rvQTo2x1Hn6JJ4Gl8/cc/uMplWxJC0K9D7mbfeIMwuw3ASGsLVeQTACh2uHvgQqEWSnA2qdTmBZE6hwPHhSWsNOarXqwSCKYGoInZEacnQ7dDB2zrqvefy/hDOpdGb30xFLJln/9Ox8cHF5dR1F0ZdSOFyhIRWMPxSidmE7FF796NG1WJJmIubfVcx/NKHoVvfrPvqC5B1hqTfD8gEAAA==</OI_Content>
        <OI_XmlContent>&lt;Configuration Name=""GBCustomsCDS"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration""&gt;&lt;Group Type=""System"" Reference=""HYECMT""&gt;&lt;Group Type=""CDS"" Status=""VAL""&gt;&lt;Annotations&gt;&lt;Item Name=""TokenType""&gt;Access&lt;/Item&gt;&lt;Item Name=""Message""&gt;Could not refresh the access token, but it is still valid. 500 Internal Server Error&lt;/Item&gt;&lt;/Annotations&gt;&lt;Item Name=""MailBoxID""&gt;GB123456789000&lt;/Item&gt;&lt;Credential Name=""Current""&gt;&lt;UserName&gt;ABC&lt;/UserName&gt;&lt;/Credential&gt;&lt;/Group&gt;&lt;/Group&gt;&lt;/Configuration&gt;</OI_XmlContent>
      </eHubOutboxMessage>
    </Rows>
  </Insert>
  <Update xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration"">
    <Rows>
      <RowPair>
        <After>
          <CD_Flag1 xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">0</CD_Flag1>
        </After>
        <Before>
          <CD_PK xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">33333333-3333-3333-3333-333333333333</CD_PK>
          <CD_RV xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">AAAAAAABlkI=</CD_RV>
        </Before>
      </RowPair>
    </Rows>
  </Update>
</CompositeOperation>", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CredentialsHelpers_GetSqlXmlWhenAccessTokenIsFailedToBeObtainedNoRecipient()
		{
			var guids = new Queue<Guid>(
				new Guid[]
				{
					new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
					new Guid("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
					new Guid("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC")
				});
			CredentialsHelpers.GuidFactory = guids.Dequeue;

			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>
			{
				new eHubClientSystemRegistration
				{
					CD_Qualifier = "CURRENT",
					CD_Code = "GB123456789000.ABC",
					CD_Flag1 = 1,
					CD_Attr1 = "1234567890",
					eHubClientSystem = new eHubClientSystem {EH_ID = "HYECMT"},
					eHubRegistrationType = new eHubRegistrationType {RT_ID = "GBCustomsAccessToken"},
					CD_ExpiryUTC = DateTime.UtcNow.AddHours(1)
				}
			};

			var eHubClients = new TestDbSet<eHubClient>
			{
				new eHubClient
				{
					CC_PK = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
					CC_ID = "GBCustoms"
				}
			};

			var mockeHubTransactions = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockeHubTransactions.Stub(x => x.eHubClientSystemRegistrations).Return(eHubClientSystemRegistrations);
			mockeHubTransactions.Stub(x => x.eHubClients).Return(eHubClients);
			CredentialsHelpers.ContextFactory = () => mockeHubTransactions;
			CredentialsHelpers.UTCNow = () => new DateTime(2022, 01, 01, 13, 05, 15);

			var result = CredentialsHelpers.GetSqlXmlWhenAccessTokenIsFailedToBeObtained("HYECMT.GB123456789000.ABC", "500 Internal Server Error",
				"33333333-3333-3333-3333-333333333333", "AAAAAAABlkI=", "00000000-0000-0000-0000-000000000000",
				null, "99999999-9999-9999-9999-999999999999");

			Assert.AreEqual(
@"<CompositeOperation>
  <Update xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration"">
    <Rows>
      <RowPair>
        <After>
          <CD_Flag1 xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">0</CD_Flag1>
        </After>
        <Before>
          <CD_PK xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">33333333-3333-3333-3333-333333333333</CD_PK>
          <CD_RV xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">AAAAAAABlkI=</CD_RV>
        </Before>
      </RowPair>
    </Rows>
  </Update>
</CompositeOperation>", result);
		}
	}
}
