using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EBondRequestMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateMessage()
		{
			DeclarationTestHelper.SetProcessingDistrictPortCode("SQMZ");
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetBRecordOfficeCode("HZ");

			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			AssertEquals("Precondition", 0, entryHeader.Messages.Count);

			var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
			var extPassword = companyWrapper.PasswordCollection.AddNew();
			extPassword.GP_MailBoxID = "AAA";
			extPassword.GP_UserID = "TST";
			extPassword.CurrentDecryptedCertificatePassphrase = "012345H23";

			var senderId = GlbCompany.CurrentCompany.LicenceKeyIdentifier;

			CombineAssertions(() =>
			{
				var builder = new EBondRequestMessageBuilder(declaration, entryHeader);
				var expectedMessageText = builder.GetMessageTextFromShipment();
				var expectedBodyText = GetExpectedBodyText(senderId, expectedMessageText, "AAA", "TST");

				builder.Generate();
				Factory.Save();

				AssertEquals(InsuranceDispositionCodeList.Codes.SentToSurety, declaration.US_InsuranceDisposition);

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USeBond);

				var messageFromEntryHeader = entryHeader.Messages.Find(query);
				Assert("Should not load in the entry header's messages", !(messageFromEntryHeader.Length > 0));

				var message = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(query);

				AssertEquals("EM_MessageType", message.EM_MessageType, EDIInterchangeTypeList.Codes.XDC);
				AssertEquals("EM_MessageSubType", message.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
				AssertEquals("EM_ReceiveTransmit", message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
				AssertEquals("EM_Status", message.EM_Status, EDIInterchangeStatusList.Codes.Sent);
				AssertEquals("EM_GB", message.EM_GB, declaration.JE_GB);
				AssertEquals("EM_GE", message.EM_GE, GlbDepartment.CurrentDepartment.PK);
				AssertEquals("EM_LinkTable", message.EM_LinkTable, CusEntryHeaderSchema.Constants.TableName);
				AssertXMLEquals("EM_MessageText", message.EM_MessageText, expectedMessageText);

				var interchange = message.Interchange;

				AssertEquals("EI_ApplicationCode", interchange.EI_ApplicationCode, ApplicationCodeList.Codes.USeBond);
				AssertEquals("EI_InterchangeType", interchange.EI_InterchangeType, EDIInterchangeTypeList.Codes.XDC);
				AssertEquals("EI_ReceiveTransmit", interchange.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
				AssertEquals("EI_From", interchange.EI_From, senderId);
				AssertEquals("EI_To", interchange.EI_To, "USCustomsEBond");
				AssertEquals("EI_Status", interchange.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued);
				AssertEquals("EI_GB", interchange.EI_GB, declaration.JE_GB);
				AssertXMLEquals("EI_BodyText", Regex.Replace(interchange.EI_BodyText,
						@"          <Name>Password</Name>
          <Type>Base64Binary</Type>
          <Data>.*</Data>",
						@"          <Name>Password</Name>
          <Type>Base64Binary</Type>
          <Data>password</Data>"), expectedBodyText);

				AssertEquals("EI_HeaderNText", "", interchange.EI_HeaderNText);
			});
		}

		public void TestBondDesignationCodeIsAlwaysB()
		{
			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.SubstitutionBond;
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EBondRequestMessageBuilder(declaration, entryHeader);
			builder.Generate();

			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USeBond);

			var messageFromEntryHeader = entryHeader.Messages.Find(query);
			Assert("Should not load in the entry header's messages", !(messageFromEntryHeader.Length > 0));

			var message = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(query);

			AssertContains("<Key>BondDesignationCode</Key>\r\n        <Value>B</Value>", message.EM_MessageText);
			AssertNotContains("<Key>BondDesignationCode</Key>\r\n        <Value>U</Value>", message.EM_MessageText);
		}

		public void TestExceptionThrownWhenGenerateMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
			var extPassword = companyWrapper.PasswordCollection.AddNew();
			extPassword.GP_MailBoxID = "AAA";
			extPassword.GP_UserID = "TST";
			extPassword.CurrentDecryptedCertificatePassphrase = "012345H23";

			try
			{
				var builder = new EBondRequestMessageBuilderForExceptionTest(declaration, entryHeader);
				AssertEquals("No message created", false, builder.Generate());

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USeBond);
				var message = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(query);
				AssertNull("No message created", message);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGenerateMessage_MeetingSaveConcurrencyException()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
			var extPassword = companyWrapper.PasswordCollection.AddNew();
			extPassword.GP_MailBoxID = "AAA";
			extPassword.GP_UserID = "TST";
			extPassword.CurrentDecryptedCertificatePassphrase = "012345H23";

			var builder = new EBondRequestMessageBuilderForSaveExceptionTest(declaration, entryHeader);
			AssertEquals("Message created", true, builder.Generate());

			AssertExceptionThrown<ZSaveException>(() => declaration.Factory.Save());
			AssertEquals(ZString.Empty, declaration.US_InsuranceDisposition);

			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USeBond);
			var message = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(query);
			AssertNull("No message created", message);
		}

		string GetExpectedBodyText(string senderId, string messageText, string insuranceAgent, string userName)
		{
			return $@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>{senderId}</SenderID>
    <RecipientID>USCustomsEBond</RecipientID>
    <DeliveryMetadata>
      <ValueCollection>
        <Value>
          <Name>InsuranceAgent</Name>
          <Type>String</Type>
          <Data>{insuranceAgent}</Data>
        </Value>
        <Value>
          <Name>UserName</Name>
          <Type>String</Type>
          <Data>{userName}</Data>
        </Value>
        <Value>
          <Name>Password</Name>
          <Type>Base64Binary</Type>
          <Data>password</Data>
        </Value>
      </ValueCollection>
    </DeliveryMetadata>
  </Header>
  <Body>
    {messageText}
  </Body>
</UniversalInterchange>";
		}

		sealed class EBondRequestMessageBuilderForExceptionTest : EBondRequestMessageBuilder
		{
			public EBondRequestMessageBuilderForExceptionTest(JobDeclaration declaration, CusEntryHeader entryHeader)
				: base(declaration, entryHeader)
			{
			}

			public override ZString GetMessageTextFromShipment() => throw new InvalidDataException("Test Invalid Data");
		}

		sealed class EBondRequestMessageBuilderForSaveExceptionTest : EBondRequestMessageBuilder
		{
			public EBondRequestMessageBuilderForSaveExceptionTest(JobDeclaration declaration, CusEntryHeader entryHeader)
				: base(declaration, entryHeader)
			{
			}

			protected override void Message_Saving(Enterprise.Messaging.Business.EDIMessage message)
			{
				base.Message_Saving(message);

				var table = new DataTable("Bala");
				var column = new DataColumn("PK", typeof(System.Guid));
				table.Columns.Add(column);
				table.PrimaryKey = new[] { column };
				var dataRow = table.NewRow();
				throw new ZSaveException(new ZDataException(new System.Exception(), dataRow, CargoWise.Data.Db.Connection), new BusinessObjectFactory());
			}
		}
	}
}
