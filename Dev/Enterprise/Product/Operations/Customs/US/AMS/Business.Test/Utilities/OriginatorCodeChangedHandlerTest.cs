using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.US.USAMS;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class OriginatorCodeChangedHandlerTest : TestCaseWithFactory
	{
		public void TestOriginatorCodeChanged()
		{
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Test Address 1";
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "0000001", Core.Constants.CountryCodes.UnitedStates);
			var orgAddress2 = org1.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 2";
			orgAddress2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "0000002", Core.Constants.CountryCodes.UnitedStates);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			var orgAddress3 = org2.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Test Address 3";
			orgAddress3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "0000001", Core.Constants.CountryCodes.UnitedStates);
			var orgAddress4 = org2.Addresses.AddNew();
			orgAddress4.OA_Address1 = "Test Address 4";
			orgAddress4.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "0000003", Core.Constants.CountryCodes.UnitedStates);

			USBranch1.GB_OH_OrgProxy = org1.PK;
			USBranch2.GB_OH_OrgProxy = org1.PK;
			AUBranch1.GB_OH_OrgProxy = org1.PK;
			AUBranch2.GB_OH_OrgProxy = org2.PK;
			CABranch1.GB_OH_OrgProxy = org2.PK;
			Factory.Save();

			IOriginatorCodeChangedHandler handler = new OriginatorCodeChangedHandler(Factory, org1.PK.ToGuid());
			handler.OnUpdateAction();

			var expectedXml =
$@"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""US Customs Registry"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""{USCompany.GC_Code}"">
      <Group Type=""AMA"" Status=""VAL"">
        <Credential Name=""Current"">
          <Password>{{password}}</Password>
        </Credential>
      </Group>
    </Group>
    <Group Type=""Company"" Reference=""{AUCompany.GC_Code}"">
      <Group Type=""AMA"" Status=""VAL"">
        <Credential Name=""Current"">
          <Password>{{password}}</Password>
        </Credential>
      </Group>
    </Group>
  </Group>
</Configuration>";

			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			interchangeQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotNull("Should have created an EDIInterchange.", createdInterchange);
			AssertEquals("Created EDIInterchange should have correct content.", FormatXmlToTest(expectedXml), FormatXmlToTest(createdInterchange.EI_BodyText));
		}

		public void TestOriginatorCodeCleared()
		{
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG1";

			USBranch1.GB_OH_OrgProxy = org.PK;
			USBranch2.GB_OH_OrgProxy = org.PK;
			AUBranch1.GB_OH_OrgProxy = org.PK;
			Factory.Save();

			IOriginatorCodeChangedHandler handler = new OriginatorCodeChangedHandler(Factory, org.PK.ToGuid());
			handler.OnUpdateAction();

			var expectedXml =
$@"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""US Customs Registry"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""{USCompany.GC_Code}"">
      <Group Type=""AMA"" Status=""VAL"" />
    </Group>
    <Group Type=""Company"" Reference=""{AUCompany.GC_Code}"">
      <Group Type=""AMA"" Status=""VAL"" />
    </Group>
  </Group>
</Configuration>";

			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			interchangeQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotNull("Should have created an EDIInterchange.", createdInterchange);
			AssertEquals("Created EDIInterchange should have correct content.", FormatXmlToTest(expectedXml), FormatXmlToTest(createdInterchange.EI_BodyText));
		}

		string FormatXmlToTest(string input)
		{
			var nbspIgnored = input.Replace(" ", string.Empty);
			var xmlnsIgnored = Regex.Replace(nbspIgnored, @"xmlns(:\w+)?\s?=\s?""[^""]+""", string.Empty);
			var passwordReplaced = Regex.Replace(xmlnsIgnored, @"\<Password\>[^\<]+\</Password\>", "<Password>{password}</Password>");
			var breakingsAndIndentsRemoved = Regex.Replace(passwordReplaced, @"[\r\n]+\s*", " ");
			var continuousBlankRemoved = Regex.Replace(breakingsAndIndentsRemoved, @"\s{2,}", " ");
			return continuousBlankRemoved;
		}

		GlbCompany USCompany { get; set; }
		GlbCompany AUCompany { get; set; }
		GlbCompany CACompany { get; set; }

		GlbBranch USBranch1 { get; set; }
		GlbBranch USBranch2 { get; set; }
		GlbBranch AUBranch1 { get; set; }
		GlbBranch AUBranch2 { get; set; }
		GlbBranch CABranch1 { get; set; }
		GlbBranch CABranch2 { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			if (USCompany == null)
			{
				USCompany = Factory.NewWithValidTestData<GlbCompany>();
				USCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				USCompany.GC_Code = "US1";

				USBranch1 = Factory.NewWithValidTestData<GlbBranch>();
				USBranch1.GB_GC = USCompany.PK.ToGuid();
				USBranch1.GB_Code = "CH1";

				USBranch2 = Factory.NewWithValidTestData<GlbBranch>();
				USBranch2.GB_GC = USCompany.PK.ToGuid();
				USBranch2.GB_Code = "CH2";
			}

			if (AUCompany == null)
			{
				AUCompany = Factory.NewWithValidTestData<GlbCompany>();
				AUCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				AUCompany.GC_Code = "AU1";

				AUBranch1 = Factory.NewWithValidTestData<GlbBranch>();
				AUBranch1.GB_GC = AUCompany.PK.ToGuid();
				AUBranch1.GB_Code = "AA1";

				AUBranch2 = Factory.NewWithValidTestData<GlbBranch>();
				AUBranch2.GB_GC = AUCompany.PK.ToGuid();
				AUBranch2.GB_Code = "AA2";
			}

			if (CACompany == null)
			{
				CACompany = Factory.NewWithValidTestData<GlbCompany>();
				CACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				CACompany.GC_Code = "CA1";

				CABranch1 = Factory.NewWithValidTestData<GlbBranch>();
				CABranch1.GB_GC = CACompany.PK.ToGuid();
				CABranch1.GB_Code = "BL1";

				CABranch2 = Factory.NewWithValidTestData<GlbBranch>();
				CABranch2.GB_GC = CACompany.PK.ToGuid();
				CABranch2.GB_Code = "BL2";
			}

			Factory.Save();
		}
	}
}
