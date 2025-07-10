using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class OrganisationFinderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestInitialiseOrganisationFinder()
		{
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), Factory);
			HtmlAssertNotNull("OrganisationFinder Created", finder);
		}

		public void TestInitialiseUnknownOrganisation()
		{
			var arg = new UnknownOrganisationCodeEventArgs();
			arg.Code = "ASALOW";
			arg.City = "Sydney";
			arg.Name = "Test Company";
			arg.PostCode = "10000";
			arg.RegistrationNo = "BN3423";
			arg.Street = "Wickham Terrace";
			var expectedHeader = GenerateOrganisation("ASALOW", "Wickham Terrace", ZString.Empty, "Sydney", "AU", "10000", "Test Company", "BN3423");
			var finder = new OrganisationFinder(arg, Factory);
			AssertOrganisation(expectedHeader, finder.UnknownOrg);
		}

		public void TestAllowEmptyAddresses()
		{
			var arg = new UnknownOrganisationCodeEventArgs();
			arg.Code = "0001";
			var finder = new OrganisationFinder(arg, Factory);
			AssertEquals("OH_Code should be", "0001", finder.UnknownOrg.OH_Code);
			AssertEquals("AllowEmptyAddresses should be", false, finder.UnknownOrg.AllowEmptyAddresses);
		}

		public void TestFindSimilarOrganisations()
		{
			OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgMatchThresholds.Codes.Low);

			WriteOrganisationsToDB();

			var arg = new UnknownOrganisationCodeEventArgs();
			arg.Code = "blabla";
			arg.Name = "Test Organisation1 251004";
			arg.PostCode = "3000";
			arg.Street = "Sudirman 1";
			arg.Street2 = "Sudirman 2";
			arg.Country = "AU";
			var finder = new OrganisationFinder(arg, Factory);
			finder.FindSimilarOrganisations();

			AssertEquals(3, finder.UnknownOrg.SimilarOrgMatches.Count);
			AssertSimilarMatchesContains("Test Organisation1 251004", finder.UnknownOrg.SimilarOrgMatches);
			AssertSimilarMatchesContains("Test Organisation2 251004", finder.UnknownOrg.SimilarOrgMatches);
			AssertSimilarMatchesContains("Test Organisation3 251004", finder.UnknownOrg.SimilarOrgMatches);
		}

		void AssertSimilarMatchesContains(ZString orgName, OrgPatternMatchCollection patternMatches)
		{
			var found = false;

			foreach (OrgPatternMatch match in patternMatches)
			{
				if (match.OH_FullName == orgName)
				{
					found = true;
					break;
				}
			}

			AssertEquals("Pattern Match found - " + orgName, true, found);
		}

		OrgHeader GenerateOrganisation(ZString code, ZString street1, ZString street2, ZString city, ZString country, ZString postCode, ZString name, ZString registrationNo)
		{
			var expectedOrg = Factory.New<OrgHeader>();
			expectedOrg.OH_Code = code;
			expectedOrg.MainAddress.OA_Address1 = street1;
			expectedOrg.MainAddress.OA_Address2 = street2;
			expectedOrg.MainAddress.OA_City = city;
			expectedOrg.MainAddress.OA_PostCode = postCode;
			expectedOrg.OH_RL_NKClosestPort = country;
			expectedOrg.OH_FullName = name;
			expectedOrg.PrimaryRegistrationNumber.Number = registrationNo;
			return expectedOrg;
		}

		void AssertOrganisation(OrgHeader expectedOrg, OrgHeader generatedOrg)
		{
			AssertEquals(expectedOrg.MainAddress.OA_Address1, generatedOrg.MainAddress.OA_Address1);
			AssertEquals(expectedOrg.MainAddress.OA_Address2, generatedOrg.MainAddress.OA_Address2);
			AssertEquals(expectedOrg.MainAddress.OA_City, generatedOrg.MainAddress.OA_City);
			AssertEquals(expectedOrg.MainAddress.OA_PostCode, generatedOrg.MainAddress.OA_PostCode);
			AssertEquals(expectedOrg.OH_FullName, generatedOrg.OH_FullName);
			AssertEquals(expectedOrg.PrimaryRegistrationNumber.Number, generatedOrg.PrimaryRegistrationNumber.Number);
			AssertEquals(expectedOrg.OH_RL_NKClosestPort, generatedOrg.OH_RL_NKClosestPort);
		}

		void WriteOrganisationsToDB()
		{
			Db.Connection.ExecuteNonQuery("DELETE from dbo.OrgPatternMatch");
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_FullName = "Test Organisation1 251004";
			organisation1.MainAddress.OA_Address1 = "Sudirman 1";
			organisation1.MainAddress.OA_Address2 = "Sudirman 2";
			organisation1.MainAddress.OA_City = "Melbourne";
			organisation1.MainAddress.OA_PostCode = "3000";
			organisation1.OH_IsConsignor = ZBool.True;
			organisation1.OH_RL_NKClosestPort = "AUSYD";
			organisation1.PrimaryRegistrationNumber.Number = "ABN101";

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_FullName = "Test Organisation2 251004";
			organisation2.MainAddress.OA_Address1 = "Sudirman 1";
			organisation2.MainAddress.OA_Address2 = "Sudirman 2";
			organisation2.MainAddress.OA_City = "Melbourne";
			organisation2.MainAddress.OA_PostCode = "3000";
			organisation2.OH_RL_NKClosestPort = "AUSYD";
			organisation2.PrimaryRegistrationNumber.Number = "ABN102";
			organisation2.OH_IsConsignor = ZBool.True;

			var organisation3 = Factory.New<OrgHeader>();
			organisation3.OH_FullName = "Test Organisation3 251004";
			organisation3.MainAddress.OA_Address1 = "Sudirman 1";
			organisation3.MainAddress.OA_Address2 = "asfasdf";
			organisation3.MainAddress.OA_City = "Sydney";
			organisation3.MainAddress.OA_PostCode = "3000";
			organisation3.OH_RL_NKClosestPort = "AUSYD";
			organisation3.PrimaryRegistrationNumber.Number = "ABN103";
			organisation3.OH_IsConsignor = ZBool.True;

			var organisation4 = Factory.New<OrgHeader>();
			organisation4.OH_FullName = "Test Organisation4 251004";
			organisation4.MainAddress.OA_Address1 = "Mulholland Drive";
			organisation4.MainAddress.OA_Address2 = "Bourke Road";
			organisation4.MainAddress.OA_City = "sydney";
			organisation4.MainAddress.OA_PostCode = "3000";
			organisation4.OH_RL_NKClosestPort = "AUSYD";
			organisation4.PrimaryRegistrationNumber.Number = "ABN104";
			organisation4.OH_IsConsignor = ZBool.True;

			Factory.Save();
		}
	}
}
