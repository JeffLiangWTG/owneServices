using System;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(FTPSettingsRegistryActionHandler))]
sealed class FTPSettingsRegistryActionHandlerTest : TestCaseWithFactory
{
	public void TestFTPSettingsCustomRegistry_OnUpdate_ConfigurationMessageIsGenerated()
	{
		var expectedXmlWithCredentials =
#if NETFRAMEWORK
			$"""
<Configuration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Name="NOCTietoEvry" Version="1.0" xmlns="http://www.wisetechglobal.com/Schemas/Configuration">
  <Group Type="System" Reference="{EnterpriseCode}">
    <Group Type="Company" Reference="ABC" Status="VAL">
      <FTP>
        <Server>myurl.someserver.com</Server>
        <Port>80</Port>
        <UserName>admin@someserver</UserName>
        <Password>password</Password>
        <SendFolder>myhome/</SendFolder>
        <ReceiveFolder>inbox</ReceiveFolder>
      </FTP>
    </Group>
  </Group>
</Configuration>
""";
#else
			$"""
<Configuration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Name="NOCTietoEvry" Version="1.0" xmlns="http://www.wisetechglobal.com/Schemas/Configuration">
  <Group Type="System" Reference="{EnterpriseCode}">
    <Group Type="Company" Reference="ABC" Status="VAL">
      <FTP>
        <Server>myurl.someserver.com</Server>
        <Port>80</Port>
        <UserName>admin@someserver</UserName>
        <Password>password</Password>
        <SendFolder>myhome/</SendFolder>
        <ReceiveFolder>inbox</ReceiveFolder>
      </FTP>
    </Group>
  </Group>
</Configuration>
""";
#endif

		var customsRegistry = new FTPSettingsCustomsRegistry
		{
			Url = "myurl.someserver.com",
			SendToCustomFolder = "myhome/",
			ReceiveFromCustomFolder = "inbox",
			Port = "80",
			Username = "admin@someserver",
			Password = "password"
		};

		RegistryActionHandler.UpdateFtpCustomsSettings(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, customsRegistry);
		RegistryActionHandler.Save();

		CombineAssertions(() =>
		{
			var xml = Factory.LoadTop1<EDIInterchange>(InterchangeQuery);
			AssertNotNull("EDIInterchange for CFG message should be present", xml);
			AssertEquals("The XML should be properly generated", expectedXmlWithCredentials, ReplaceEncryptedPasswordStringWithExpectedPassword(xml.EI_BodyText, "password"));
		});

		static string ReplaceEncryptedPasswordStringWithExpectedPassword(ZString source, string value)
			=> Regex.Replace(source, "<Password>.*</Password>", $"<Password>{value}</Password>");
	}

	ZString EnterpriseCode => enterpriseCodeLazy.Value;
	readonly Lazy<ZString> enterpriseCodeLazy = new(() =>
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
		return $"{registrationKey.EnterpriseCode}{registrationKey.ServerCode}";
	});

	IFTPSettingsRegistryActionHandler RegistryActionHandler => registryActionHandler ??= new FTPSettingsRegistryActionHandler();
	IFTPSettingsRegistryActionHandler registryActionHandler;

	GlbCompany Company => company ??= CreateNewCompany();
	GlbCompany company;

	ZQuery InterchangeQuery => interchangeQuery ??= GenerateInterchangeSearchQuery();
	ZQuery interchangeQuery;

	GlbCompany CreateNewCompany()
	{
		var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
		glbCompany.GC_Code = "ABC";
		Factory.Save();
		return glbCompany;
	}

	static ZQuery GenerateInterchangeSearchQuery()
	{
		var zQuery = new ZQuery(EDIInterchangeSchema.EI_TransportType, "XTT");
		_ = zQuery.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_To, "CustomsConfiguration"));
		_ = zQuery.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, "NOC"));
		zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
		return zQuery;
	}
}
