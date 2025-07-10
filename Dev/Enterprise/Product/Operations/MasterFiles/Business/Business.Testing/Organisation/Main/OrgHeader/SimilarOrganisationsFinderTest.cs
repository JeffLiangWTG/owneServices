using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SimilarOrganisationsFinderTest : TestCaseWithFactory
	{
		#region ISimilarOrganisationsFinder

		public void TestCreatingNewOrgHeaderShouldNOTMatchToUnmatched()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_FullName = "Goats and Monkeys";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "Address line 1";
			testHeader.MainAddress.OA_City = "City";
			testHeader.MainAddress.OA_State = "NSW";
			testHeader.MainAddress.OA_PostCode = "1221";

			Assert("Organisation will have a likely duplicate becasue default method called to include unmatched", testHeader.IsLikelyDuplicate());
			AssertEquals("Likely duplicate is UNMATCHED", "UNMATCHED", testHeader.SimilarOrgMatches[0].OH_Code);

			Assert("Organisation will NOT have a likely duplicate - UNAMTCHED is excluded from list", !testHeader.IsLikelyDuplicate(false));
		}

		public void TestForceRegeneratePatternMatch()
		{
			company.PatternMatchesForThisOrg.RemoveAndDeleteAll();
			AssertEquals(0, company.PatternMatchesForThisOrg.Count);

			company.SimilarOrgFinder.ForceRegeneratePatternMatch();
			Assert(company.PatternMatchesForThisOrg.Count > 0);
		}

		public void TestSimilarOrgMatchesReadOnly()
		{
			Assert("Similar organisations should be read only", company.SimilarOrgMatches.ReadOnly);
		}

		public void TestMatchesFiltering()
		{
			company.OH_FullName = "12345";
			company.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(company);
			company.PatternMatchesForThisOrg.Load();
			Assert("Pattern Matches Exist", company.PatternMatchesForThisOrg.Count > 0);

			foreach (OrgPatternMatch match in company.PatternMatchesForThisOrg)
			{
				Assert("Similar organisations should not contain the new organisation being added", !company.SimilarOrgMatches.Contains(match));
			}
		}

		public void TestGetFilter_ShouldNotGetDatabaseCountOnEmptyQueries()
		{
			//Arrange
			var countOfRecords = Factory.GetDatabaseCount(typeof(OrgPatternMatch));
			var org = Factory.New<OrgHeader>();
			var similarOrgsFinder = (SimilarOrganisationsFinder)(org.SimilarOrgFinder);

			//Act
			var matches = similarOrgsFinder.GetApproximateDatabaseCount(false);

			//Assert
			AssertEquals(0, matches);
		}

		public void TestLikelyMatchFound()
		{
			SetMatchToUNMATCHEDRegistryItem(false);
			company = BasicCompanyForTest;
			ISimilarOrganisationsFinder similarOrgsFinder = company.SimilarOrgFinder;
			company.OH_Code = "CODE 1";
			company.MainAddress.OA_City = "City";
			company.MainAddress.OA_Address1 = "Address";
			company.OH_FullName = "TEST ORG";
			company.OH_RL_NKClosestPort = "AUSYD";
			company.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(company);

			OrgHeader nonMatch = Factory.New<OrgHeader>();
			nonMatch.OH_Code = "BLAHCODE";
			nonMatch.MainAddress.OA_City = "Totally";
			nonMatch.MainAddress.OA_Address1 = "Different";
			nonMatch.OH_FullName = "Totally Different";
			nonMatch.OH_RL_NKClosestPort = "INBOM";
			nonMatch.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(nonMatch);

			similarOrgsFinder.FindSimilarOrganisations();
			Assert("No likely matches should be found", !similarOrgsFinder.LikelyMatchFound);

			OrgHeader matchingOrg = (OrgHeader)company.Clone();
			company.OH_Code = "CODE 2";
			matchingOrg.MainAddress.OA_City = "City";
			matchingOrg.MainAddress.OA_Address1 = "Address";
			matchingOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(matchingOrg);

			similarOrgsFinder.FindSimilarOrganisations();
			Assert("A likely match should be found", similarOrgsFinder.LikelyMatchFound);
		}

		public void TestLikelyMatchFoundForGermanOrg()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			OrgHeader company = Factory.New<OrgHeader>();
			company.OH_FullName = "MULLER LЬDENSCHEIDT GMBH CO.";
			company.MainAddress.OA_City = "Bremen";
			company.MainAddress.OA_Address1 = "Grьner Schorbachstrasse Weg 6";
			company.MainAddress.OA_Language = Constants.Languages.German;
			company.OH_RL_NKClosestPort = "DEBRE";
			company.OH_Language = Constants.Languages.German;
			company.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(company);

			ISimilarOrganisationsFinder similarOrgsFinder = company.SimilarOrgFinder;

			OrgHeader nonMatch = Factory.New<OrgHeader>();
			nonMatch.OH_Code = "BLAHCODE";
			nonMatch.MainAddress.OA_City = "Totally";
			nonMatch.MainAddress.OA_Address1 = "Different";
			nonMatch.MainAddress.OA_Language = Constants.Languages.German;
			nonMatch.OH_FullName = "Totally Different";
			nonMatch.OH_RL_NKClosestPort = "INBOM";
			nonMatch.OH_Language = Constants.Languages.German;
			nonMatch.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(nonMatch);

			similarOrgsFinder.FindSimilarOrganisations();
			Assert("No likely matches should be found", !similarOrgsFinder.LikelyMatchFound);

			OrgHeader matchingOrg = Factory.New<OrgHeader>();
			matchingOrg.OH_FullName = "MUELLER DIE LUDENSCHEIDT GMBH CO.";
			matchingOrg.MainAddress.OA_City = "Bremen";
			matchingOrg.MainAddress.OA_Address1 = "Grunerr Shorbachstrasse Weg 6";
			matchingOrg.MainAddress.OA_Language = Constants.Languages.German;
			matchingOrg.OH_Language = Constants.Languages.German;
			matchingOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(matchingOrg);

			similarOrgsFinder.FindSimilarOrganisations();
			Assert("A likely match should be found", similarOrgsFinder.LikelyMatchFound);
		}

		public void TestLikelyMatchFoundForNonSupportedLanguageOrgChinese()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			OrgHeader company = Factory.New<OrgHeader>();
			company.OH_FullName = OrgHeaderUnicodeTestConstants.ChineseCompanyName1;
			company.MainAddress.OA_City = OrgHeaderUnicodeTestConstants.ShanghaiCityName;
			company.MainAddress.OA_Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			company.MainAddress.OA_Language = Constants.Languages.ChineseSimplified;
			company.OH_RL_NKClosestPort = "CNSHA";
			company.OH_Language = Constants.Languages.ChineseSimplified;
			company.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(company);
			company.OH_Code = "ZGZPGZ";

			ISimilarOrganisationsFinder similarOrgsFinder = company.SimilarOrgFinder;

			OrgHeader nonMatch = Factory.New<OrgHeader>();
			nonMatch.MainAddress.OA_City = OrgHeaderUnicodeTestConstants.NanjingCityName;
			nonMatch.MainAddress.OA_Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress2;
			nonMatch.MainAddress.OA_Language = Constants.Languages.ChineseSimplified;
			nonMatch.OH_FullName = OrgHeaderUnicodeTestConstants.ChineseCompanyName2;
			nonMatch.OH_RL_NKClosestPort = "CNNJG";
			nonMatch.OH_Language = Constants.Languages.ChineseSimplified;
			nonMatch.OH_Code = "ZGZPGZ2";
			nonMatch.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(nonMatch);

			similarOrgsFinder.FindSimilarOrganisations();
			Assert("No likely matches should be found", !similarOrgsFinder.LikelyMatchFound);

			OrgHeader matchingOrg = Factory.New<OrgHeader>();
			matchingOrg.OH_FullName = OrgHeaderUnicodeTestConstants.ChineseCompanyName3;
			matchingOrg.MainAddress.OA_City = OrgHeaderUnicodeTestConstants.ShanghaiCityNameShort;
			matchingOrg.MainAddress.OA_Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress3;
			matchingOrg.MainAddress.OA_Language = Constants.Languages.ChineseSimplified;
			matchingOrg.OH_Language = Constants.Languages.ChineseSimplified;
			matchingOrg.OH_Code = "ZGZPGZ1";
			matchingOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(matchingOrg);

			Factory.Save();

			similarOrgsFinder.FindSimilarOrganisations();
			Assert("A likely match should be found", similarOrgsFinder.LikelyMatchFound);
		}

		public void TestLikelyMatchFoundInLocalFactory()
		{
			SetMatchToUNMATCHEDRegistryItem(false);
			company = BasicCompanyForTest;
			ISimilarOrganisationsFinder similarOrgsFinder = company.SimilarOrgFinder;
			company.OH_Code = "FIREMEL";
			company.MainAddress.OA_Address1 = "104 MAIN ROAD";
			company.MainAddress.OA_City = "ROMSEY";
			company.MainAddress.OA_State = "VIC";
			company.MainAddress.OA_PostCode = "1234";
			company.OH_FullName = "FIREWORKS";
			company.OH_RL_NKClosestPort = "AUMEL";
			OrgAddress adr1 = company.Addresses.AddNew();

			adr1.OA_Address1 = "C- SADLEIRS INTERNATIONAL";
			adr1.OA_City = "EAGLE FARM";
			adr1.OA_State = "QLD";
			adr1.OA_PostCode = "4009";

			OrgAddress adr2 = company.Addresses.AddNew();
			adr2.OA_Address1 = "UNIT 1 A/3 HELEN ST";
			adr2.OA_City = "BEAUDESERT";
			adr2.OA_State = "QLD";
			adr2.OA_PostCode = "4285";
			adr2.OA_RL_NKRelatedPortCode = "AUBNE";

			company.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(company);

			OrgHeader matchingOrg = (OrgHeader)company.Clone();
			company.OH_Code = "FIREMEL2";
			matchingOrg.MainAddress.OA_City = "Melbourne";
			matchingOrg.MainAddress.OA_Address1 = "104 MAIN ROAD";
			matchingOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(matchingOrg);

			similarOrgsFinder.FindSimilarOrganisations();
			Assert("A likely match should be found", similarOrgsFinder.LikelyMatchFound);
		}

		public void TestFindSimilarOrganisationsFallBack()
		{
			TestCaseHelper.ClearTable("OrgPatternMatch");
			SetMatchToUNMATCHEDRegistryItem(false);

			OrgHeader fakeHeader = OrgHeader.New(Factory);
			fakeHeader.OH_Code = "IBM123";
			fakeHeader.OH_FullName = "IBM Global Utilities Pty Ltd";
			fakeHeader.OH_RL_NKClosestPort = "FRVIE";
			fakeHeader.MainAddress.OA_Address1 = "Stanley Street";
			fakeHeader.MainAddress.OA_City = "Ole";
			fakeHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(fakeHeader);

			OrgHeader similarOrg = OrgHeader.New(Factory);
			similarOrg.OH_Code = "IBM321";
			similarOrg.OH_FullName = "IBM Global Utilities SIMILAR Pty Ltd";
			similarOrg.OH_RL_NKClosestPort = "FRPAR";
			similarOrg.MainAddress.OA_Address1 = "123 Rue Francois Mitterand";
			similarOrg.MainAddress.OA_City = "Paris";
			similarOrg.PatternMatchesForThisOrg.MatchThresholdOverride = OrgMatchThresholds.Codes.Low;

			Factory.Save();

			// Case where no matches found initially - falls back to less restrictive matching
			similarOrg.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There should be one match - FakeHeader", 1, similarOrg.SimilarOrgMatches.Count);
			AssertEquals("The match should be - FakeHeader", fakeHeader.PK, similarOrg.SimilarOrgMatches[0].OS_OH);

			// Create Identical Org
			OrgHeader realHeader = OrgHeader.New(Factory);
			realHeader.OH_Code = "IBM123";
			realHeader.OH_FullName = "IBM";
			realHeader.OH_RL_NKClosestPort = "FRPAR";
			realHeader.MainAddress.OA_Address1 = "123 Rue Francois Mitterand";
			realHeader.MainAddress.OA_City = "Paris";
			realHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(realHeader);

			Factory.Save();

			// Case where restrictive search succeeds - no fallback
			similarOrg.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There should be one match - RealHeader", 1, similarOrg.SimilarOrgMatches.Count);
			AssertEquals("The match should be - RealHeader", realHeader.PK, similarOrg.SimilarOrgMatches[0].OS_OH);

			// Case where too many matches
			InsertMaxThresholdPatternMatchRecordsForOrg(realHeader, Enterprise.MasterFiles.Business.Testing.OrgHeaderTest.OrgHeaderForTest.MaximumResultsToSearchExposed + 1);
			similarOrg.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There should always be matches returned", 1, similarOrg.SimilarOrgMatches.Count);
		}

		void InsertMaxThresholdPatternMatchRecordsForOrg(OrgHeader realHeader, int numberOfRecordsToInsert)
		{
			string sql =
				@"while (select count(*) From dbo.OrgPatternMatch where OS_OH = '{0}') < {1}
				begin
					insert into dbo.OrgPatternMatch (OS_OH, OS_OA, OS_FullCompanyName, OS_CompanyName1, OS_CompanyName2, OS_CompanyName3, OS_CompanyName4, OS_Address1, OS_Address2, OS_Address3, OS_Address4, OS_PostCode, OS_City, OS_UNLOCO)
					select top 1 OS_OH, OS_OA, OS_FullCompanyName, OS_CompanyName1, OS_CompanyName2, OS_CompanyName3, OS_CompanyName4, OS_Address1, OS_Address2, OS_Address3, OS_Address4, OS_PostCode, OS_City, OS_UNLOCO
					from dbo.OrgPatternMatch where OS_OH = '{0}'
				end";
			Db.Connection.ExecuteNonQuery(String.Format(sql, realHeader.PK.ToString(), numberOfRecordsToInsert)); // Doing this using BusinessObjects would take too long.
		}

		OrgHeader company;

		protected override void SetUp()
		{
			base.SetUp();
			company = Factory.New<OrgHeader>();
			company.OH_FullName = "Test Organisation Pty Limited";
			company.OH_RL_NKClosestPort = "AUSYD";
			company.OH_Code = "DUUUUH";
			company.MainAddress.OA_Address1 = "This should have been updated";
			company.MainAddress.OA_City = "A city";
			company.MainAddress.OA_CompanyNameOverride = "Test Organisation Different Name";
		}

		void SetMatchToUNMATCHEDRegistryItem(bool allowUnmatched)
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = allowUnmatched;
			Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
		}

		OrgHeader BasicCompanyForTest
		{
			get
			{
				company = Factory.New<OrgHeader>();
				company.OH_FullName = "Test Organisation Pty Limited";
				company.OH_Code = "DUUUUH";
				company.MainAddress.OA_Address1 = "This should have been updated";
				company.MainAddress.OA_City = "A city";

				return company;
			}
		}

		#endregion
	}
}
