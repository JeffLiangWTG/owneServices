using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Testing.Helpers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRCustomsDataRegistryActionHandler))]
	sealed class TRCustomsDataRegistryActionHandlerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructor() => new TRCustomsDataRegistryActionHandler();

		public void TestExportUnionCredentialMessagesAddOrUpdate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ABC";
			Factory.Save();

			var exportUnionFtpSettings = new FTPSettings();
			exportUnionFtpSettings.ExportUnionUserCode = "ExportUnionUser";
			exportUnionFtpSettings.ExportUnionUserPassword = "Password";
			RegistryActionHandler.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportUnionFtpSettings);
			RegistryActionHandler.OnAllValuesSaved();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "CustomsConfiguration");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var interchange = Factory.LoadTop1<EDIInterchange>(zQuery);

			const string expectedXml = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TREUTUnion"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"" Status=""VAL"">
      <FTP>
        <Server>ftpistanbul.ebirlik.net</Server>
        <UserName>ExportUnionUser</UserName>
        <Password>Password</Password>
        <SendFolder>outbox</SendFolder>
        <ReceiveFolder>inbox</ReceiveFolder>
        <Port>21</Port>
      </FTP>
    </Group>
  </Group>
</Configuration>"
			;

			CombineAssertions(() =>
			{
				AssertNotNull("One message should be generated", interchange);
				AssertEquals("The XML should be properly generated without credential", XmlHelper.IgnoreXmlnsAttrOrder(expectedXml), Regex.Replace(interchange.EI_BodyText, "<Password>.*</Password>", "<Password>Password</Password>"));
				AssertEquals("EI_ApplicationCode", "CFG", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", "EUT", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_From", "EDIEDIDAT", interchange.EI_From);
				AssertEquals("EI_To", "CustomsConfiguration", interchange.EI_To);
			});

			exportUnionFtpSettings.ExportUnionUserCode = "UpdateForTest";
			exportUnionFtpSettings.ExportUnionUserPassword = "Password";

			RegistryActionHandler.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportUnionFtpSettings);
			RegistryActionHandler.OnAllValuesSaved();

			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			interchange = Factory.LoadTop1<EDIInterchange>(zQuery);

			const string expectedXmlUpdate = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TREUTUnion"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"" Status=""VAL"">
      <FTP>
        <Server>ftpistanbul.ebirlik.net</Server>
        <UserName>UpdateForTest</UserName>
        <Password>Password</Password>
        <SendFolder>outbox</SendFolder>
        <ReceiveFolder>inbox</ReceiveFolder>
        <Port>21</Port>
      </FTP>
    </Group>
  </Group>
</Configuration>"
			;

			CombineAssertions(() =>
			{
				AssertNotNull("One message should be generated", interchange);
				AssertEquals("The XML should be properly generated without credential", XmlHelper.IgnoreXmlnsAttrOrder(expectedXmlUpdate), Regex.Replace(interchange.EI_BodyText, "<Password>.*</Password>", "<Password>Password</Password>"));
				AssertEquals("EI_ApplicationCode", "CFG", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", "EUT", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_From", "EDIEDIDAT", interchange.EI_From);
				AssertEquals("EI_To", "CustomsConfiguration", interchange.EI_To);
			});
		}

		public void TestExportUnionCredentialMessagesRemoved()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ABC";
			Factory.Save();

			var exportUnionFtpSettings = new FTPSettings();
			RegistryActionHandler.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportUnionFtpSettings);
			RegistryActionHandler.OnAllValuesSaved();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "CustomsConfiguration");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var interchange = Factory.LoadTop1<EDIInterchange>(zQuery);

			const string expectedXml = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TREUTUnion"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"" Status=""INV"">
      <FTP />
    </Group>
  </Group>
</Configuration>";

			CombineAssertions(() =>
			{
				AssertNotNull("One message should be generated", interchange);
				AssertEquals("The XML should be properly generated without credential", XmlHelper.IgnoreXmlnsAttrOrder(expectedXml), interchange.EI_BodyText);
				AssertEquals("EI_ApplicationCode", "CFG", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", "EUT", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_From", "EDIEDIDAT", interchange.EI_From);
				AssertEquals("EI_To", "CustomsConfiguration", interchange.EI_To);
			});
		}

		ITRCustomsDataRegistryActionHandler RegistryActionHandler => registryActionHandler ??= new TRCustomsDataRegistryActionHandler();
		ITRCustomsDataRegistryActionHandler registryActionHandler;
	}
}
