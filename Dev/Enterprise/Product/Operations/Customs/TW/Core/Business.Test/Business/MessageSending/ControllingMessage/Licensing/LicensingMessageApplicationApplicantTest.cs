using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageApplicationApplicant))]
	sealed class LicensingMessageApplicationApplicantTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var reservedFields = declaration.ReservedFields;
			var reservedField1 = reservedFields.AddNew();

			var reservedField2 = reservedFields.AddNew();
			reservedField1.CY_Code = "1";
			reservedField1.CY_Data = "data1";
			reservedField2.CY_Code = "2";
			reservedField2.CY_Data = "data2";
			var applicantDocAddress = header.ApplicantDocumentaryAddress;
			var org = new TestTWCreator(Factory).CreateOrganization();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			applicantDocAddress.OrganisationPK = org.PK;
			applicantDocAddress.E2_OA_Address = org.MainAddress.PK;
			applicantDocAddress.ContactPK = org.Contacts.FirstOrDefault().PK;

			var applicant = new LicensingMessageApplicationApplicant(header.ApplicantDocumentaryAddress, declaration);
			CombineAssertions("Not Override", () =>
			{
				NUnit.Framework.Assert.That(applicant.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison), "ChineseName");
				NUnit.Framework.Assert.That(applicant.Name, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison), "Name");
				NUnit.Framework.Assert.That(applicant.OwnerName, NUnit.Framework.Is.EqualTo(ZString.Empty), "OwnerName");
				NUnit.Framework.Assert.That(applicant.ID, NUnit.Framework.Is.EqualTo("VAT001").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(applicant.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "TypeCode");
				NUnit.Framework.Assert.That(applicant.UndertakeCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "UndertakeCode");
				NUnit.Framework.Assert.That(applicant.Address.Line, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN").Using(CustomComparers.TypeComparison), "Address.Line");
				NUnit.Framework.Assert.That(applicant.Address.ChineseLine, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "Address.ChineseLine");
				var communications = applicant.Communications;
				NUnit.Framework.Assert.That(communications.Count(), NUnit.Framework.Is.EqualTo(2), "Communications count");
				NUnit.Framework.Assert.That(communications.Any(x => x.TypeID == "TE" && x.ID == "13925568211"), NUnit.Framework.Is.True, "Communication (Phone)");
				NUnit.Framework.Assert.That(communications.Any(x => x.TypeID == "MA" && x.ID == "123@456.com"), NUnit.Framework.Is.True, "Communication (EMail)");
				NUnit.Framework.Assert.That(applicant.ContactName, NUnit.Framework.Is.EqualTo("Contact1").Using(CustomComparers.TypeComparison), "ContactName");

				var zhTWtranslatedAddress = org.MainAddress.TranslatedAddresses.FirstOrDefault(c => c.OTA_Language == Core.SharedConstants.Languages.ChineseTraditional);
				zhTWtranslatedAddress.OTA_Address1 = "";
				applicant = new LicensingMessageApplicationApplicant(header.ApplicantDocumentaryAddress, declaration);
				NUnit.Framework.Assert.That(applicant.Address.ChineseLine, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Address.ChineseLine");
				var additionalInformations = applicant.AdditionalInformations;
				NUnit.Framework.Assert.That(additionalInformations.Count(), NUnit.Framework.Is.EqualTo(2), "additionalInformations Count");
				var additionalInformation1 = applicant.AdditionalInformations.ElementAt(0);
				NUnit.Framework.Assert.That(additionalInformation1.StatementCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "additionalInformation1.StatementCode");
				NUnit.Framework.Assert.That(additionalInformation1.StatementDescription, NUnit.Framework.Is.EqualTo("data1").Using(CustomComparers.TypeComparison), "additionalInformation1.StatementDescription");
				var additionalInformation2 = applicant.AdditionalInformations.ElementAt(1);
				NUnit.Framework.Assert.That(additionalInformation2.StatementCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "additionalInformation2.StatementCode");
				NUnit.Framework.Assert.That(additionalInformation2.StatementDescription, NUnit.Framework.Is.EqualTo("data2").Using(CustomComparers.TypeComparison), "additionalInformation2.StatementDescription");
			});

			applicantDocAddress.E2_AddressOverride = true;
			applicantDocAddress.E2_CompanyName = "ABC Company";
			applicantDocAddress.State = "TAO";
			applicantDocAddress.City = "Zhongli City";
			applicantDocAddress.Address1 = "No. 88, Yuandong Road";
			applicantDocAddress.Postcode = "32073";
			var localAddress = (TWJobDocAddress)applicantDocAddress.LocalAddress;
			localAddress.E2_CompanyName = "ABC 公司";
			applicantDocAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			applicantDocAddress.IDCode = "00695437";
			localAddress.State = "TAO";
			localAddress.City = "中壢市";
			localAddress.Address1 = "遠東路88號";
			localAddress.Postcode = "32073";
			applicantDocAddress.E2_Phone = "(03)3322100";
			applicant = new LicensingMessageApplicationApplicant(header.ApplicantDocumentaryAddress, declaration);
			CombineAssertions("Override", () =>
			{
				NUnit.Framework.Assert.That(applicant.ChineseName, NUnit.Framework.Is.EqualTo("ABC 公司").Using(CustomComparers.TypeComparison), "ChineseName");
				NUnit.Framework.Assert.That(applicant.Name, NUnit.Framework.Is.EqualTo("ABC Company").Using(CustomComparers.TypeComparison), "Name");
				NUnit.Framework.Assert.That(applicant.OwnerName, NUnit.Framework.Is.EqualTo(ZString.Empty), "OwnerName");
				NUnit.Framework.Assert.That(applicant.ID, NUnit.Framework.Is.EqualTo("00695437").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(applicant.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "TypeCode");
				NUnit.Framework.Assert.That(applicant.UndertakeCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "UndertakeCode");
				NUnit.Framework.Assert.That(applicant.Address.Line, NUnit.Framework.Is.EqualTo("NO. 88, YUANDONG ROAD ORANGE DISTRICT ZHONGLI CITY 32073 TAIWAN").Using(CustomComparers.TypeComparison), "Address.Line");
				NUnit.Framework.Assert.That(applicant.Address.ChineseLine, NUnit.Framework.Is.EqualTo("32073中壢市遠東路88號").Using(CustomComparers.TypeComparison), "Address.ChineseLine");
				var communications = applicant.Communications;
				NUnit.Framework.Assert.That(communications.Count(), NUnit.Framework.Is.EqualTo(2), "Communications count");
				NUnit.Framework.Assert.That(communications.Any(x => x.TypeID == "TE" && x.ID == "(03)3322100"), NUnit.Framework.Is.True, "Communication (Phone)");
				NUnit.Framework.Assert.That(communications.Any(x => x.TypeID == "MA" && x.ID == "123@456.com"), NUnit.Framework.Is.True, "Communication (EMail)");
				NUnit.Framework.Assert.That(applicant.ContactName, NUnit.Framework.Is.EqualTo("Contact1").Using(CustomComparers.TypeComparison), "ContactName");

				localAddress.E2_Address1 = "";
				applicant = new LicensingMessageApplicationApplicant(header.ApplicantDocumentaryAddress, declaration);
				NUnit.Framework.Assert.That(applicant.Address.ChineseLine, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Address.ChineseLine");
				var additionalInformations = applicant.AdditionalInformations;
				NUnit.Framework.Assert.That(additionalInformations.Count(), NUnit.Framework.Is.EqualTo(2), "additionalInformations Count");
				var additionalInformation1 = applicant.AdditionalInformations.ElementAt(0);
				NUnit.Framework.Assert.That(additionalInformation1.StatementCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "additionalInformation1.StatementCode");
				NUnit.Framework.Assert.That(additionalInformation1.StatementDescription, NUnit.Framework.Is.EqualTo("data1").Using(CustomComparers.TypeComparison), "additionalInformation1.StatementDescription");
				var additionalInformation2 = applicant.AdditionalInformations.ElementAt(1);
				NUnit.Framework.Assert.That(additionalInformation2.StatementCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "additionalInformation2.StatementCode");
				NUnit.Framework.Assert.That(additionalInformation2.StatementDescription, NUnit.Framework.Is.EqualTo("data2").Using(CustomComparers.TypeComparison), "additionalInformation2.StatementDescription");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
	}
}
