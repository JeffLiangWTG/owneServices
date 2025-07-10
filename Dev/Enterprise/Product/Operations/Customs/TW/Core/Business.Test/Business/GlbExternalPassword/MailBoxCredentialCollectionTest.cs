using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(MailBoxCredentialCollection))]
	sealed class MailBoxCredentialCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			return new MailBoxCredentialCollection(cusInBondHeader, cusInBondHeader.Company);
		}

		[ExpectNoExceptions]
		public void TestRelationshipFilter()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "XXX";
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "GS1";
			AddgenRegCertAccredMaintList(staff1, "BRK");
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "GS2";
			AddgenRegCertAccredMaintList(staff2, "BRK");
			var extPassword1 = AddMailBoxCredential(cusInBondHeader.Company, staff1, "00000000-2", PasswordTypesList.Codes.TVA);
			var extPassword2 = AddMailBoxCredential(cusInBondHeader.Company, staff1, "00000000-1", PasswordTypesList.Codes.UVC);
			var extPassword3 = AddMailBoxCredential(company, staff1, "00000000-3", PasswordTypesList.Codes.TVA);
			var extPassword4 = AddMailBoxCredential(cusInBondHeader.Company, staff2, "00000000-4", PasswordTypesList.Codes.TVA);
			Factory.Save();
			cusInBondHeader.BH_GS_NKCusAgent = "GS1";
			var collection = new MailBoxCredentialCollection(cusInBondHeader, cusInBondHeader.Company);
			collection.Load();
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword1.PK), NUnit.Framework.Is.Not.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should have extPassword1 - should not be [null]");
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword2.PK), NUnit.Framework.Is.Not.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should have extPassword2 - should not be [null]");
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword3.PK), NUnit.Framework.Is.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should not have extPassword3 - should be [null]");
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword4.PK), NUnit.Framework.Is.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should not have extPassword4 - should be [null]");
			cusInBondHeader.BH_GS_NKCusAgent = "GS2";
			collection.Load();
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword1.PK), NUnit.Framework.Is.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should not have extPassword1 - should be [null]");
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword2.PK), NUnit.Framework.Is.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should not have extPassword2 - should be [null]");
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword3.PK), NUnit.Framework.Is.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should not have extPassword3 - should be [null]");
			NUnit.Framework.Assert.That(collection.FindByPK(extPassword4.PK), NUnit.Framework.Is.Not.EqualTo(default(CargoWise.EntityFramework.BusinessObject)), "Collection should have extPassword4 - should not be [null]");
		}

		GlbExternalPassword AddMailBoxCredential(GlbCompany company, GlbStaff staff, ZString mailBoxID, ZString passwordType)
		{
			var extPassword2 = Factory.New<GlbExternalPassword>();
			extPassword2.GP_PasswordType = passwordType;
			extPassword2.GP_MailBoxID = mailBoxID;
			if (company != null)
			{
				extPassword2.GP_GC = company.PK;
			}

			if (staff != null)
			{
				extPassword2.GP_GS = staff.PK;
			}

			return extPassword2;
		}

		static GenRegCertAccredMaintList AddgenRegCertAccredMaintList(GlbStaff staff, ZString type)
		{
			var genRegCertAccredMaintList = staff.Certificates.AddNew();
			genRegCertAccredMaintList.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			genRegCertAccredMaintList.XZ_Type = type;
			return genRegCertAccredMaintList;
		}
	}
}
