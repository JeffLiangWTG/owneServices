using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class HouseIssuedByTest : TestCaseWithFactory
	{
		public void TestBillIssuedByAirline()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "IT";

			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "NHK";

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.MiscServ.OM_RM_Airline = airline.PK;
			var ivaCC = org1.CustomsCodes.AddNew();
			ivaCC.OK_RN_NKCodeCountry = "IT";
			ivaCC.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			ivaCC.OK_CustomsRegNo = "10987654321";

			airline.RM_AddressLine1 = "Line Addr1";
			airline.RM_AddressLine2 = "Line Addr2";
			airline.RM_AirlineName1 = "Line Name";
			airline.RM_AirlineCity = "ITMIL";
			airline.RM_AirlineCountry = "IT";
			airline.RM_AirlineState = "NSW";
			airline.RM_AirlinePostalCode = "9999";

			BillIssuedByAirline issuedBy = new BillIssuedByAirline(airline);

			AssertEquals("Line Name", issuedBy.Name);
			AssertEquals("Line Addr1", issuedBy.Address1);
			AssertEquals("Line Addr2", issuedBy.Address2);
			AssertEquals("ITMIL", issuedBy.City);
			AssertEquals("IT", issuedBy.CountryName);
			AssertEquals("NSW", issuedBy.State);
			AssertEquals("9999", issuedBy.PostCode);
			AssertEquals("10987654321", issuedBy.ItalianIVA);
		}

		RefUNLOCO GetLocoOfNewBranch()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_FullName = "BranchFullName";
			org.MainAddress.OA_Address1 = "BranchAddress1";
			org.MainAddress.OA_Address2 = "BranchAddress2";
			org.MainAddress.OA_City = "BranchCity";
			org.MainAddress.OA_PostCode = "BranchPC";
			org.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			GlbBranch branch = GlbBranch.CurrentBranch;
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "CPT";
			branch.GB_RL_NKHomePort = "ZACPT";
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();

			return branch.HomePort;
		}

		void SetUpChineseMainAddressWithEnglishTranslation()
		{
			var mainAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress;
			mainAddress.OA_Language = "ZH-CN";

			var englishAddress = mainAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = "EN";
			englishAddress.OTA_Address1 = "CompanyAddress1InEnglish";
			englishAddress.OTA_Address2 = "CompanyAddress2InEnglish";
			englishAddress.OTA_City = "CompanyCityInEnglish";
			englishAddress.OTA_PostCode = "CompanyPCInEnglish";
			englishAddress.OTA_State = "CompanyStateInEnglish";
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbBranch branch = GlbBranch.CurrentBranch;
			fOriginalGB_GC = branch.GB_GC;
			fOriginalGB_Code = branch.GB_Code;
			fOriginalGB_RL_NKHomePort = branch.GB_RL_NKHomePort;
			fOriginalGB_OH = branch.GB_OH_OrgProxy;
		}

		ZGuid fOriginalGB_GC;
		ZString fOriginalGB_Code;
		ZString fOriginalGB_RL_NKHomePort;
		ZGuid fOriginalGB_OH;

		protected override void TearDown()
		{
			// Need to restore current branch information as this is NOT rolled-back as part of the DB rollback
			base.TearDown();
			GlbBranch branch = GlbBranch.CurrentBranch;
			branch.GB_GC = fOriginalGB_GC;
			branch.GB_Code = fOriginalGB_Code;
			branch.GB_RL_NKHomePort = fOriginalGB_RL_NKHomePort;
			branch.GB_OH_OrgProxy = fOriginalGB_OH;
		}

		public void TestName()
		{
			BillIssuedBy billIssuedBy = new BillIssuedBy((RefUNLOCO)null);
			GlbCompany.CurrentCompany.OrgProxy.OH_FullName = "CompanyFullName";
			AssertEquals("COMPANYFULLNAME", billIssuedBy.Name);

			billIssuedBy = new BillIssuedBy(GetLocoOfNewBranch());
			AssertEquals("BRANCHFULLNAME", billIssuedBy.Name);
		}

		public void TestAddress1()
		{
			var billIssuedBy = new BillIssuedBy((RefUNLOCO)null);
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address1 = "CompanyAddress1";
			AssertEquals("COMPANYADDRESS1", billIssuedBy.Address1);

			billIssuedBy = new BillIssuedBy(GetLocoOfNewBranch());
			AssertEquals("BRANCHADDRESS1", billIssuedBy.Address1);

			SetUpChineseMainAddressWithEnglishTranslation();
			billIssuedBy = new BillIssuedBy((RefUNLOCO)null, true);
			AssertEquals("COMPANYADDRESS1INENGLISH", billIssuedBy.Address1);
		}

		public void TestAddress2()
		{
			var billIssuedBy = new BillIssuedBy((RefUNLOCO)null);
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address2 = "CompanyAddress2";
			AssertEquals("COMPANYADDRESS2", billIssuedBy.Address2);

			billIssuedBy = new BillIssuedBy(GetLocoOfNewBranch());
			AssertEquals("BRANCHADDRESS2", billIssuedBy.Address2);

			SetUpChineseMainAddressWithEnglishTranslation();
			billIssuedBy = new BillIssuedBy((RefUNLOCO)null, true);
			AssertEquals("COMPANYADDRESS2INENGLISH", billIssuedBy.Address2);
		}

		public void TestCity()
		{
			var billIssuedBy = new BillIssuedBy((RefUNLOCO)null);
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_City = "CompanyCity";
			AssertEquals("COMPANYCITY", billIssuedBy.City);

			billIssuedBy = new BillIssuedBy(GetLocoOfNewBranch());
			AssertEquals("BRANCHCITY", billIssuedBy.City);

			SetUpChineseMainAddressWithEnglishTranslation();
			billIssuedBy = new BillIssuedBy((RefUNLOCO)null, true);
			AssertEquals("COMPANYCITYINENGLISH", billIssuedBy.City);
		}

		public void TestPostCode()
		{
			var billIssuedBy = new BillIssuedBy((RefUNLOCO)null);
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_PostCode = "CompanyPC";
			AssertEquals("COMPANYPC", billIssuedBy.PostCode);

			billIssuedBy = new BillIssuedBy(GetLocoOfNewBranch());
			AssertEquals("BRANCHPC", billIssuedBy.PostCode);

			SetUpChineseMainAddressWithEnglishTranslation();
			billIssuedBy = new BillIssuedBy((RefUNLOCO)null, true);
			AssertEquals("COMPANYPCINENGLISH", billIssuedBy.PostCode);
		}

		public void TestUNLOCOAndState()
		{
			var billIssuedBy = new BillIssuedBy((RefUNLOCO)null);
			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "HKHKG";
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_State = "CompanyState";
			AssertEquals("HKHKG", billIssuedBy.UNLOCO.Code);
			AssertEquals("HONG KONG", billIssuedBy.UNLOCO.Country.Description.ToUpper());
			AssertEquals("COMPANYSTATE", billIssuedBy.State);

			billIssuedBy = new BillIssuedBy(GetLocoOfNewBranch());
			AssertEquals("AUSYD", billIssuedBy.UNLOCO.Code);
			AssertEquals("AUSTRALIA", billIssuedBy.UNLOCO.Country.Description.ToUpper());
			AssertEquals("NSW", billIssuedBy.State);

			SetUpChineseMainAddressWithEnglishTranslation();
			billIssuedBy = new BillIssuedBy((RefUNLOCO)null, true);
			AssertEquals("COMPANYSTATEINENGLISH", billIssuedBy.State);
		}

		public void TestCountryNameIsNotTranslated()
		{
			var billIssuedBy = new BillIssuedBy((RefUNLOCO)null);
			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "HKHKG";
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("Hong Kong", billIssuedBy.CountryName);
			}
		}

		public void TestFirstBranchForUnLoco()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Global";
			org.OH_Code = "GLOBAL";

			var currentOrganization1 = Factory.NewWithValidTestData<OrgHeader>();
			currentOrganization1.OH_Code = "GLOCON1";

			var currentOrganization2 = Factory.NewWithValidTestData<OrgHeader>();
			currentOrganization2.OH_Code = "CUSAFO2";

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			company.GC_Code = "~";
			company.GC_OH_OrgProxy = org.PK;

			var inactiveBranch = company.Branches.AddNew();
			inactiveBranch.GB_Code = "BR1";
			inactiveBranch.GB_OH_OrgProxy = currentOrganization1.PK;
			inactiveBranch.GB_RL_NKHomePort = "AUBNE";
			inactiveBranch.GB_IsActive = false;

			var activeBranch = company.Branches.AddNew();
			activeBranch.GB_Code = "BR2";
			activeBranch.GB_OH_OrgProxy = currentOrganization2.PK;
			activeBranch.GB_RL_NKHomePort = "AUPAL";
			activeBranch.GB_IsActive = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, activeBranch.PK.ToGuid(), Guid.Empty))
			{
				var billIssuedBy = new BillIssuedBy(activeBranch.HomePort);
				var fieldInfo = typeof(BillIssuedBy).GetField("issuedByOrg", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertNotNull(fieldInfo);

				var orgHeader = fieldInfo.GetValue(billIssuedBy) as OrgHeader;
				AssertEquals("CUSAFO2", orgHeader.OH_Code);

				var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
				billIssuedBy = new BillIssuedBy(unloco);
				orgHeader = fieldInfo.GetValue(billIssuedBy) as OrgHeader;
				AssertEquals("GLOBAL", orgHeader.OH_Code);
			}
		}
	}
}
