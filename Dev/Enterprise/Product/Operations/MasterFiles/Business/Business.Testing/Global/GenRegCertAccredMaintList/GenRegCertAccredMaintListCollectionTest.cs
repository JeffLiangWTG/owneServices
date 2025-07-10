using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GenRegCertAccredMaintListCollection))]
	sealed class GenRegCertAccredMaintListCollectionTest : ActiveBusinessObjectCollectionTestCase<GenRegCertAccredMaintListCollection>
	{
		public void TestOnLoadedIntoCollection()
		{
			Staff.GS_Code = "SS";
			Staff.GS_FullName = "TT";

			GenRegCertAccredMaintList certificate1 = Factory.New<GenRegCertAccredMaintList>();
			certificate1.XZ_ParentID = Staff.PK;
			certificate1.XZ_ParentTableCode = GlbStaffSchema.Constants.Prefix;

			GenRegCertAccredMaintList certificate2 = Factory.New<GenRegCertAccredMaintList>();
			certificate2.XZ_ParentID = Staff.PK;
			certificate2.XZ_ParentTableCode = "XX";

			GenRegCertAccredMaintList certificate3 = Factory.New<GenRegCertAccredMaintList>();
			certificate3.XZ_ParentID = ZGuid.NewZGuid();
			certificate3.XZ_ParentTableCode = GlbStaffSchema.Constants.Prefix;

			GenRegCertAccredMaintList certificate4 = Factory.New<GenRegCertAccredMaintList>();
			certificate4.XZ_ParentID = Staff.PK;
			certificate4.XZ_ParentTableCode = GlbStaffSchema.Constants.Prefix;

			Factory.Save();

			AssertEquals(2, Staff.Certificates.Count);

			bool b1 = false;
			bool b2 = false;
			bool b3 = false;
			bool b4 = false;
			foreach (GenRegCertAccredMaintList certificate in Staff.Certificates)
			{
				if (certificate.PK == certificate1.PK)
				{
					b1 = true;
				}
				if (certificate.PK == certificate2.PK)
				{
					b2 = true;
				}
				if (certificate.PK == certificate3.PK)
				{
					b3 = true;
				}
				if (certificate.PK == certificate4.PK)
				{
					b4 = true;
				}
			}
			Assert("Correct certificates set should be loaded.", b1 && !b2 && !b3 && b4);
		}

		public void TestSetDefaultsForNewElement()
		{
			AssertEquals(GlbStaffSchema.Constants.Prefix, Staff.Certificates.AddNew().XZ_ParentTableCode);

			GenRegCertAccredMaintList certificate = Factory.New<GenRegCertAccredMaintList>();
			AssertNotEquals(GlbStaffSchema.Constants.Prefix, certificate.XZ_ParentTableCode);
			Staff.Certificates.Add(certificate);
			AssertEquals(GlbStaffSchema.Constants.Prefix, certificate.XZ_ParentTableCode);
		}

		public void TestGetFirstCertificateNumber()
		{
			ZDateTime today = ZDateTime.Today;

			GenRegCertAccredMaintList cert1 = Collection.AddNew();
			cert1.XZ_Type = CertificateTypePairList.Codes.BR1;
			cert1.XZ_RefNumber = "111";
			cert1.XZ_ExpiryOrDueDate = ZDateTime.Empty;

			GenRegCertAccredMaintList cert2 = Collection.AddNew();
			cert2.XZ_Type = CertificateTypePairList.Codes.CA1;
			cert2.XZ_RefNumber = "222";
			cert2.XZ_ExpiryOrDueDate = today.AddDays(5);

			GenRegCertAccredMaintList cert3 = Collection.AddNew();
			cert3.XZ_Type = CertificateTypePairList.Codes.DG1;
			cert3.XZ_RefNumber = "333";
			cert3.XZ_ExpiryOrDueDate = today.AddDays(7);

			GenRegCertAccredMaintList cert4 = Collection.AddNew();
			cert4.XZ_Type = CertificateTypePairList.Codes.CA1;
			cert4.XZ_RefNumber = "444";
			cert4.XZ_ExpiryOrDueDate = today.AddDays(9);

			AssertEquals("CAR", "444", Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.CA1));
			AssertEquals("DGN", "333", Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.DG1));
			AssertEquals("MMM", ZString.Empty, Collection.GetFirstCertificateNumber("MMM"));

			AssertEquals("BRK", "111", Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.BR1, today));
			AssertEquals("CAR", "444", Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.CA1, ZDateTime.Empty));
			AssertEquals("CAR", "444", Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.CA1, today.AddDays(4)));
			AssertEquals("CAR", "444", Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.CA1, today.AddDays(6)));
			AssertEquals("DGN", "333", Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.DG1, today.AddDays(7)));
			AssertEquals("DGN", ZString.Empty, Collection.GetFirstCertificateNumber(CertificateTypePairList.Codes.DG1, today.AddDays(8)));
		}

		public void TestGetFirstCertificate()
		{
			ZDateTime today = ZDateTime.Today;

			GenRegCertAccredMaintList cert1 = Collection.AddNew();
			cert1.XZ_Type = CertificateTypePairList.Codes.BR1;
			cert1.XZ_RefNumber = "111";
			cert1.XZ_ExpiryOrDueDate = ZDateTime.Empty;

			GenRegCertAccredMaintList cert2 = Collection.AddNew();
			cert2.XZ_Type = CertificateTypePairList.Codes.CA1;
			cert2.XZ_RefNumber = "222";
			cert2.XZ_ExpiryOrDueDate = today.AddDays(5);

			GenRegCertAccredMaintList cert3 = Collection.AddNew();
			cert3.XZ_Type = CertificateTypePairList.Codes.DG1;
			cert3.XZ_RefNumber = "333";
			cert3.XZ_ExpiryOrDueDate = today.AddDays(7);

			GenRegCertAccredMaintList cert4 = Collection.AddNew();
			cert4.XZ_Type = CertificateTypePairList.Codes.CA1;
			cert4.XZ_RefNumber = "444";
			cert4.XZ_ExpiryOrDueDate = today.AddDays(9);

			AssertEquals("CAR", "444", Collection.GetFirstCertificate(CertificateTypePairList.Codes.CA1).XZ_RefNumber);
			AssertEquals("DGN", "333", Collection.GetFirstCertificate(CertificateTypePairList.Codes.DG1).XZ_RefNumber);
			AssertNull("MMM", Collection.GetFirstCertificate("MMM"));

			AssertEquals("BRK", "111", Collection.GetFirstCertificate(CertificateTypePairList.Codes.BR1, today).XZ_RefNumber);
			AssertEquals("CAR", "444", Collection.GetFirstCertificate(CertificateTypePairList.Codes.CA1, ZDateTime.Empty).XZ_RefNumber);
			AssertEquals("CAR", "444", Collection.GetFirstCertificate(CertificateTypePairList.Codes.CA1, today.AddDays(4)).XZ_RefNumber);
			AssertEquals("CAR", "444", Collection.GetFirstCertificate(CertificateTypePairList.Codes.CA1, today.AddDays(6)).XZ_RefNumber);
			AssertEquals("DGN", "333", Collection.GetFirstCertificate(CertificateTypePairList.Codes.DG1, today.AddDays(7)).XZ_RefNumber);
			AssertNull("DGN", Collection.GetFirstCertificate(CertificateTypePairList.Codes.DG1, today.AddDays(8)));
		}

		public void TestIsDuplicated()
		{
			GenRegCertAccredMaintList cert1 = Collection.AddNew();
			cert1.XZ_Type = CertificateTypePairList.Codes.CA1;

			GenRegCertAccredMaintList cert2 = Collection.AddNew();
			cert2.XZ_Type = CertificateTypePairList.Codes.DG1;

			GenRegCertAccredMaintList cert3 = Collection.AddNew();
			cert3.XZ_Type = CertificateTypePairList.Codes.CA1;

			AssertEquals("CAR", true, Collection.IsDuplicated(CertificateTypePairList.Codes.CA1));
			AssertEquals("DGN", false, Collection.IsDuplicated(CertificateTypePairList.Codes.DG1));
		}

		#region Implementation

		protected override GenRegCertAccredMaintListCollection GetCollectionToTest()
		{
			return new GenRegCertAccredMaintListCollection(Staff);
		}

		GlbStaff Staff
		{
			get { return staff ?? (staff = Factory.NewWithValidTestData<GlbStaff>()); }
		}

		GlbStaff staff;

		#endregion
	}
}
