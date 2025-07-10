using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GenRegCertAccredMaintList))]
	sealed class GenRegCertAccredMaintListTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUKStaffHandlingSecuredCargoCertificates()
		{
			var staff = Factory.New<GlbStaff>();

			var cert1 = staff.Certificates.AddNew();
			cert1.XZ_Type = "DTA";
			Assert("DTA is not a UK Staff Handling Secured Cargo Certificate", !cert1.IsUKStaffHandlingSecureCargoCertificate);
			AssertEquals("Country is not set for DTA", "", cert1.XZ_RN_NKCountryOfIssuance);

			var cert2 = staff.Certificates.AddNew();
			cert2.XZ_Type = "CO1";
			Assert("CO1 is a UK Staff Handling Secured Cargo Certificate", cert2.IsUKStaffHandlingSecureCargoCertificate);
			AssertEquals("Country is set to GB for CO1", "GB", cert2.XZ_RN_NKCountryOfIssuance);

			var cert3 = staff.Certificates.AddNew();
			cert3.XZ_Type = "CM1";
			Assert("CM1 is a UK Staff Handling Secured Cargo Certificate", cert3.IsUKStaffHandlingSecureCargoCertificate);
			AssertEquals("Country is set to GB for CM1", "GB", cert3.XZ_RN_NKCountryOfIssuance);
		}

		public void TestXZ_Type()
		{
			var staff = Factory.New<GlbStaff>();

			var cert1 = staff.Certificates.AddNew();
			cert1.XZ_Type = "BRK";
			Assert("Country is not set for BRK", cert1.XZ_RN_NKCountryOfIssuance.IsEmpty);

			var cert2 = staff.Certificates.AddNew();
			cert2.XZ_Type = "CNO";
			AssertEquals("Country is set to CN for CNO", "CN", cert2.XZ_RN_NKCountryOfIssuance);
		}

		public void TestMasterParent()
		{
			GenRegCertAccredMaintList certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			AssertNull(certificate.MasterParent);

			DummyObject masterParent = Factory.New<DummyObject>();
			certificate.MasterParent = masterParent;
			AssertEquals(masterParent, certificate.MasterParent);
		}

		public void TestValidation()
		{
			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			AssertNull(certificate.MasterParent);
			AssertType(typeof(GenRegCertAccredMaintListValidation), certificate.Validation);

			var masterParent = Factory.New<DummyObject>();
			certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			certificate.MasterParent = masterParent;
			AssertNotNull(certificate.MasterParent);
			AssertType(typeof(DummyGenRegCertAccredMaintListValidation), certificate.Validation);
		}

		public void TestOnSavingForMexico()
		{
			var staff = Factory.New<GlbStaff>();
			staff.NumberRangeMatchingDetails.DeleteAll();
			staff.Fountains.DeleteAll();

			var stmNums1 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<StaffViewStmNums>(staff, "1111111", type: OrgConstants.NumberFountains.Code.PatentNumber);
			var item = StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(staff.Factory, staff.PK, OrgConstants.NumberFountains.Code.PatentNumber, "1111111", ownerTable: GlbStaffSchema.Constants.Prefix);
			item.PatentNumber = "1234";
			Factory.Save();

			var cert1 = staff.Certificates.AddNew();
			cert1.XZ_Type = "BRK";
			cert1.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;
			cert1.XZ_RefNumber = ZString.Empty;

			cert1.Factory.Save();
			AssertEquals("Should be empty", ZString.Empty, item.PatentNumber);

			var cert2 = staff.Certificates.AddNew();
			cert2.XZ_Type = "BRK";
			cert2.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Mexico;
			cert2.XZ_RefNumber = "1234";

			cert2.Factory.Save();
			AssertEquals("Should be 1234, when XZ_RefNumber change", "1234", item.PatentNumber);
		}

		#region Helper Classes

		class DummyObject : DummyEnterpriseBusinessObject, ICertificatesProvider, ICertificatesValidationProvider
		{
			public DummyObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ICertificatesProvider Members

			public GenRegCertAccredMaintListCollection Certificates
			{
				get { return null; }
			}

			public ICodeDescriptionPairList GetCertificateTypeList()
			{
				return null;
			}

			public ICodeDescriptionPairList GetActiveCertificateTypeList()
			{
				return null;
			}

			public ZString GetDefaultDescription(ZString code)
			{
				return ZString.Empty;
			}

			public GenRegCertAccredMaintListValidation GetValidation(GenRegCertAccredMaintList parent)
			{
				return new DummyGenRegCertAccredMaintListValidation(parent);
			}

			#endregion
		}

		class DummyGenRegCertAccredMaintListValidation : GenRegCertAccredMaintListValidation
		{
			public DummyGenRegCertAccredMaintListValidation(AutoGenRegCertAccredMaintList parent)
				: base(parent)
			{
			}
		}

		#endregion
	}
}
