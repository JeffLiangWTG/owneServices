using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRCustomsTRKXmlCredentialConfigurationHandlerTest : Customs.Business.XmlCredential.Testing.ConfigurationHandlerTestCase
	{
		public void TestUserStatusbecameInvalidAndNotificationEMailIsPrepared()
		{
			var userID = "12345678901";
			var staffCode = "GRL";
			var companyQuery = new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.Equal, "DTR");
			var company = Factory.LoadTop1<GlbCompany>(companyQuery);
			if (company == null)
			{
				company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Code = "DTR";
			}

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_EmailAddress = "test@test.com";

			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
			externalPassword.GP_UserID = userID;
			externalPassword.GP_PasswordType = PasswordTypesList.Codes.TRK;
			externalPassword.GP_GC = company.PK;
			externalPassword.GP_GS = staff.PK;
			externalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			externalPassword.GP_StatusReason = "";
			externalPassword.GP_CurrentPassword = "SomeBigText";

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.eHub;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.EHubRegistryUpdate;
			interchange.EI_BodyText = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TRCustomsCredential"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
    <Group Type=""System"" Reference=""HYECMT"">
        <Group Type=""Company"" Reference=""DTR"">
              <Group Type=""Staff"" Reference=""GRL"">
                <Group Type=""TRK"" Status=""INV"">
                  <Annotations>
                        <Item name=""Reason"">3100 - The password has expired</Item>
                    </Annotations>
                    <Credential Name=""Current"">
                        <UserName>12345678901</UserName>
                    </Credential> 
                </Group>
            </Group>
        </Group>
    </Group>
</Configuration>";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(PasswordStatusList.Codes.PasswordOK, externalPassword.GP_PasswordStatus);
			AssertEquals("", externalPassword.GP_StatusReason);
			AssertEquals("Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			Configuration configuration;
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}
			var logger = new LoggingInformation();
			var handler = new TRCustomsTRKXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);

			var query = new ZQuery(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.Equal, userID);
			var password = Factory.LoadTop1<GlbExternalPassword>(query);
			AssertNotNull(password);
			AssertEquals(PasswordStatusList.Codes.Invalid, externalPassword.GP_PasswordStatus);
			AssertEquals(PasswordTypesList.Codes.TRK + ": 3100 - The password has expired", externalPassword.GP_StatusReason);
			AssertEquals("Emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmail(Env.OutgoingMailManager.EmailsCreated[0], "Password Status Has Been Updated to 'Invalid (INV)'", @"Password Type: 'Turkish Customs Bilge System (TRK)'
Owner: Company = 'DTR', Staff = 'GRL'
Password Status: 'Invalid (INV)'
Status Reason:
TRK: 3100 - The password has expired",
new[] { staff.GS_EmailAddress });
		}
	}
}
