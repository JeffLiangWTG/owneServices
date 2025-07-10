using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class SGCustomsSGAXmlCredentialConfigurationHandlerTest : ConfigurationHandlerTestCase
	{
		public void TestProcess()
		{
			var message = @"<Configuration Name=""SGCustomsConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""WTLSGC"">
		<Group Type=""Company"" Reference=""DSG"">
			<Annotations>
				<Item Name=""Info"">Password is changed</Item>
			</Annotations>
			<Group Type=""Staff"" Reference=""AAA"">
				<Group Type=""SGCustomsAccount"" Status=""VAL"">
					<Credential Name=""Current"">
						<UserName>VWGT002</UserName>
						<Password>{ENCRYPTEDPASSWORD}</Password>
					</Credential>
				</Group>
			</Group>
		</Group>
	</Group>
</Configuration>";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DSG";
			var glbGroup = Factory.New<GlbGroup>();
			glbGroup.GG_Code = "GP1";
			glbGroup.GG_Desc = "TEST GROUP";
			glbGroup.GG_GC = company.PK;
			glbGroup.GG_IsSales = true;
			var staff1 = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff1.GS_Code = "AAA";
			staff1.GS_EmailAddress = "bob@where.com";
			glbGroup.Staff.Add(staff1);
			var externalPassword = Factory.New<GlbExternalPassword>();
			externalPassword.GP_GS = staff1.PK;
			externalPassword.GP_GC = company.PK;
			externalPassword.GP_PasswordType = PasswordTypesList.Codes.SGA;
			externalPassword.GP_UserID = "VWGT002";
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.eHub;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.Configuration;
			interchange.EI_BodyText = message.Replace("{ENCRYPTEDPASSWORD}", "by90WWdvbmt5Mzd1WmdiRGpTZDJObDRSZ2hPdzg3RnVZbjJISk9GdXI2N2VNRmtnQll2TWorSEJZSDVZb2lqNEdMUmdUUC9yU3JjTFNsdllseGRrNFJJTElwT1BlTzUvWXV1RURzaXlRYkphd0VLU1lrcjhjcjNMUi8wQjNZSUlnUHJZbVYwWEJ2bXZibHJpMEhqQlJTb1pCdnM2N3JYdzhIUFkvNGhydlZRPQ==");
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Configuration configuration;
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			var logger = new LoggingInformation();
			var handler = new SGCustomsSGAXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var externalPasswordQuery = new ZQuery();
			externalPasswordQuery.AddToFilter(GlbExternalPasswordSchema.GP_UserID, "VWGT002");
			externalPasswordQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.SGA);
			var newExternalPassword = newFactory.LoadTop1<GlbExternalPassword>(externalPasswordQuery);
			AssertEquals("Password should have been changed.", "nnn", newExternalPassword.CurrentDecryptedPassword);
			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("Should have created eMail.", 1, emails.Count);
			var email = emails[0];
			AssertEquals("eMail Subject", "Password Has Been Updated", email.Subject);
			AssertEquals("eMail Body", @"Password Type: 'SG ACCESS (SGA)'
Owner: Company = 'DSG', Staff = 'AAA'
New Password: nnn", email.Body);
		}
	}
}
