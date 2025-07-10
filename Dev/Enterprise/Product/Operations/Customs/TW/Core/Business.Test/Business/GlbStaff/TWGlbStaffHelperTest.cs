using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWGlbStaffHelperTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestGetValidTWBrokerCertificateNumber()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS2";
			var cert = staff.Certificates.AddNew();
			cert.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert.XZ_IssueDate = new ZDate(2019, 08, 02);
			cert.XZ_ExpiryOrDueDate = new ZDate(2020, 08, 02);
			cert.XZ_RefNumber = "123";
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 08, 02)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 10, 02)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2020, 08, 02)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 08, 01)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2020, 08, 03)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			cert.XZ_RefNumber = ZString.Empty;
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 08, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 10, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2020, 08, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			cert.XZ_RefNumber = "123";
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.DTA;
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 08, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 10, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2020, 08, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			cert.XZ_RefNumber = "123";
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Albania;
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 08, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 10, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2020, 08, 02)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			cert = staff.Certificates.AddNew();
			cert.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert.XZ_IssueDate = new ZDate(2019, 08, 02);
			cert.XZ_ExpiryOrDueDate = new ZDate(2020, 08, 02);
			cert.XZ_RefNumber = "123";
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 08, 02)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 10, 02)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2020, 08, 02)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2019, 08, 01)), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(new ZDate(2020, 08, 03)), NUnit.Framework.Is.EqualTo(ZString.Empty));
			cert.XZ_ExpiryOrDueDate = ZDate.Empty;
			NUnit.Framework.Assert.That(staff.GetValidTWBrokerCertificateNumber(ZDateTime.MaxSmallDateTimeValue), NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetTWBrkCertificate()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();

			var cert = staff.Certificates.AddNew();
			cert.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);

			var cert2 = staff.Certificates.AddNew();
			cert2.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert2.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert2.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);

			var cert3 = staff.Certificates.AddNew();
			cert3.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.China;
			cert3.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert3.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(2);

			var cert4 = staff.Certificates.AddNew();
			cert4.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert4.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.APP;
			cert4.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(3);

			NUnit.Framework.Assert.That(staff.GetTWBrkCertificate().PK, NUnit.Framework.Is.EqualTo(cert2.PK));
		}
	}
}
