using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(GlbCompanyDataImportLogSubscriber))]
	public class GlbCompanyDataImportLogSubscriberTest : LogSubscriberTest<GlbCompanyDataImportLogSubscriber>
	{
		public void TestProcessQueuedLogs()
		{
			var taxRates = new AccTaxRateCollection(Factory, Constants.CountryCodes.NewZealand);
			taxRates.Load();
			taxRates.RemoveAndDeleteAll();
			Factory.Save();
			taxRates.Load();
			AssertEquals("Precondition: there are no tax rates in DB.", 0, taxRates.Count);

			var manager = new ImportHandler(new AncillaryImportServices());

			using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(sourceXML)))
			{
				manager.Import(inputStream);
			}

			var newCompany = Factory.LoadFromUniqueKey<GlbCompany>(GlbCompanySchema.GC_Code, new ZString("XYZ"));
			AssertNotNull("Company from XML should be added", newCompany);

			taxRates.Load();
			AssertEquals("Should be zero tax rates.", 0, taxRates.Count);

			RunLogWalkerCycleForTest();

			taxRates.Reload(true);
			AssertNotEquals("Tax rates are added in DB for brand new Company if it is GST registered.", 0, taxRates.Count);

			int expectedCompanyChargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, DemoCompany.PK)).Length;
			int newCompanyChargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, newCompany.PK)).Length;
			Assert("precondition there are charge codes in demo company", expectedCompanyChargeCodes > 0);
			AssertEquals("New Company should have the same amount of ChargeCodes as Expected Company", expectedCompanyChargeCodes, newCompanyChargeCode);

			using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(sourceXML)))
			{
				manager.Import(inputStream);
			}

			RunLogWalkerCycleForTest();

			newCompanyChargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, newCompany.PK)).Length;
			AssertEquals("Charged Codes should be the same after importing the same company twice", expectedCompanyChargeCodes, newCompanyChargeCode);
		}

		GlbCompany DemoCompany
		{
			get
			{
				return Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, GlbCompany.DemoCompanyCode);
			}
		}

		const string sourceXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <Company version=""2.0"">
      <GlbCompany Action=""MERGE"">
        <PK>b7ffa5cb-b600-4531-9a59-53708e894284</PK>
        <Code>XYZ</Code>
        <BusinessRegNo>103888638</BusinessRegNo>
        <BusinessRegNo2></BusinessRegNo2>
        <CustomsRegistrationNo></CustomsRegistrationNo>
        <Address1>ABCDEF Building</Address1>
        <Address2>George Bolt Memorial Drive</Address2>
        <City>AUCKLAND</City>
        <Phone>+6491234567</Phone>
        <PostCode>2150</PostCode>
        <State>AUK</State>
        <Fax></Fax>
        <NoOfAccountingPeriods>12</NoOfAccountingPeriods>
        <PeriodFormat>CAL</PeriodFormat>
        <StartDate></StartDate>
        <PeriodEndWeekDay>FRI</PeriodEndWeekDay>
        <GLCurrentPeriod>200001</GLCurrentPeriod>
        <ARAPCurrentPeriod>200001</ARAPCurrentPeriod>
        <GLClosedPeriod>200001</GLClosedPeriod>
        <ARAPClosedPeriod>200001</ARAPClosedPeriod>
        <ExRateDisplayMode>IUL</ExRateDisplayMode>
        <ExRateDecimals>4</ExRateDecimals>
        <LocalDocLanguage>DEF</LocalDocLanguage>
        <IsActive>true</IsActive>
        <Email></Email>
        <Name>XYZ Solutions</Name>
        <WebAddress></WebAddress>
        <IsGSTRegistered>true</IsGSTRegistered>
        <IsGSTCashBasis>false</IsGSTCashBasis>
        <IsWHTRegistered>false</IsWHTRegistered>
        <IsWHTCashBasis>false</IsWHTCashBasis>
        <IsReciprocal>false</IsReciprocal>
        <AddressMap></AddressMap>

        <ValidationStatus>NYV</ValidationStatus>
        <GlbBranchCollection>
          <GlbBranch Action=""MERGE"">
            <PK>c4e56a8c-acac-4a30-98bd-b3c9d5a2dce5</PK>
            <Code>XYZ</Code>
            <BranchName>XYZ Solutions</BranchName>
            <Address1>ABCDEF Building</Address1>
            <Address2>George Bolt Memorial Drive</Address2>
            <City>AUCKLAND</City>
            <State>AUK</State>
            <PostCode>2150</PostCode>
            <Phone>+6491234567</Phone>
            <Fax></Fax>
            <InternalExtension></InternalExtension>
            <WebAddress></WebAddress>
            <LocalDocLanguage>DEF</LocalDocLanguage>
            <IsActive>true</IsActive>
            <Email></Email>
            <AccountingGroupCode></AccountingGroupCode>
            <AddressMap></AddressMap>

            <ValidationStatus>NYV</ValidationStatus>
            <HomePort TableName=""RefUNLOCO"">
              <Code>NZAKL</Code>
              <PK>8f9895f5-8bc5-488e-8c1f-4fde8f2b9833</PK>
            </HomePort>
            <OrgProxy TableName=""OrgHeader"" />
            <AddressProxy TableName=""OrgAddress"" />
            <CountryCode TableName=""RefCountry"">
              <Code>NZ</Code>
              <PK>7a546412-b30c-46bb-ac7a-e60fc6120d1f</PK>
            </CountryCode>
          </GlbBranch>
        </GlbBranchCollection>
        <OrgProxy TableName=""OrgHeader"" />
        <LocalCurrency TableName=""RefCurrency"">
          <Code>NZD</Code>
          <PK>c3d01660-010f-47f2-a8dd-5030cfdfb9f8</PK>
        </LocalCurrency>
        <CountryCode TableName=""RefCountry"">
          <Code>NZ</Code>
          <PK>7a546412-b30c-46bb-ac7a-e60fc6120d1f</PK>
        </CountryCode>
      </GlbCompany>
    </Company>
  </Body>
</Native>";
	}
}
