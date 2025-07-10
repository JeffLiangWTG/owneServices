using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPatternMatchCollection))]
	sealed class OrgPatternMatchCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSimilarOrgsParameterGeneratorCatersForNVarchar()
		{
			SimilarOrgsParameterGenerator generator = new SimilarOrgsParameterGenerator(new Hashtable());
			ZSqlParameter param = generator.GetParameterForValue(OrgHeaderSchema.OH_FullName, "testname");
			AssertEquals("ParameterValueTextSql", "N'testname'", param.ParameterValueTextSql);
		}

		#region Results Order

		public void TestMultipleMatches_SameDetails()
		{
			OrgHeader checkHeader = GenerateCheckHeader("NOOBSRUS CONSOLIDATORS", "AUSYD", "Main", "Alexandria");
			OrgPatternMatchCollection testCollection = LoadSimilarPatternMatches(checkHeader, 4);

			AssertFoundPatternMatchHasTheseDetails(testCollection[0], Header1, Header1.OH_FullName, Header1.MainAddress, "Result 1");
			AssertFoundPatternMatchHasTheseDetails(testCollection[1], Header1, Header1.OH_FullName, Header1Address, "Result 2");
			AssertFoundPatternMatchHasTheseDetails(testCollection[2], Header2, Header2.OH_FullName, Header2.MainAddress, "Result 3");
			AssertFoundPatternMatchHasTheseDetails(testCollection[3], Header2, Header2.OH_FullName, Header2Address, "Result 4");
		}

		public void TestMultipleMatches_AlternateAddress()
		{
			OrgHeader checkHeader = GenerateCheckHeader("NOOBSRUS CONSOLIDATORS", "AUSYD", "Pickup", "Alexandria");
			OrgPatternMatchCollection testCollection = LoadSimilarPatternMatches(checkHeader, 4);

			AssertFoundPatternMatchHasTheseDetails(testCollection[0], Header1, Header1.OH_FullName, Header1Address, "Result 1");
			AssertFoundPatternMatchHasTheseDetails(testCollection[1], Header1, Header1.OH_FullName, Header1.MainAddress, "Result 2");
			AssertFoundPatternMatchHasTheseDetails(testCollection[2], Header2, Header2.OH_FullName, Header2.MainAddress, "Result 3");
			AssertFoundPatternMatchHasTheseDetails(testCollection[3], Header2, Header2.OH_FullName, Header2Address, "Result 4");
		}

		public void TestMultipleMatches_DifferentName()
		{
			OrgHeader checkHeader = GenerateCheckHeader("NOOBSRUS CONSOLIDATORS ALTERN", "AUSYD", "Main", "Alexandria");
			OrgPatternMatchCollection testCollection = LoadSimilarPatternMatches(checkHeader, 4);

			AssertFoundPatternMatchHasTheseDetails(testCollection[0], Header2, Header2.OH_FullName, Header2.MainAddress, "Result 1");
			AssertFoundPatternMatchHasTheseDetails(testCollection[1], Header1, Header1.OH_FullName, Header1.MainAddress, "Result 2");
			AssertFoundPatternMatchHasTheseDetails(testCollection[2], Header2, Header2.OH_FullName, Header2Address, "Result 3");
			AssertFoundPatternMatchHasTheseDetails(testCollection[3], Header1, Header1.OH_FullName, Header1Address, "Result 4");
		}

		#endregion

		#region Scoring

		public void TestFindSimilarOrganisationsMatchCountDoesNothingIfTheresNoPatterns()
		{
			TestCaseHelper.ClearTable(OrgPatternMatchSchema.PK.TableName);
			SetupMatches(100);
			ZInt totalMatchCount;

			FirstOrg.PatternMatchesForThisOrg.RemoveAll();
			TestCollection.LoadSimilarOrganisations(FirstOrg.PatternMatchesForThisOrg, null, out totalMatchCount, true, false);

			AssertEquals("Count should match", 0, totalMatchCount);
		}

		public void TestFindSimilarOrganisationsMatchCount()
		{
			TestCaseHelper.ClearTable(OrgPatternMatchSchema.PK.TableName);
			SetupMatches(100);
			ZInt totalMatchCount;
			TestCollection.LoadSimilarOrganisations(FirstOrg.PatternMatchesForThisOrg, null, out totalMatchCount, true, false);

			AssertEquals("Count should match", 100, totalMatchCount);
		}

		public void TestLikelyMatchesExist()
		{
			TestCollection = new OrgPatternMatchCollection(Factory2);
			Assert("No objects are in the collection, so no likely matches should exit", !TestCollection.LikelyMatchesExist);

			OrgPatternMatch orgMatch = Factory2.New<OrgPatternMatch>();
			orgMatch.OS_Score = TestCollection.LikelyMatchScore - 1;
			TestCollection.Add(orgMatch);

			OrgPatternMatch secondMatch = (OrgPatternMatch)orgMatch.Clone();
			secondMatch.OS_Score = 0;
			TestCollection.Add(secondMatch);

			Assert("Score is not sufficient for a likely match", !TestCollection.LikelyMatchesExist);

			orgMatch.OS_Score = 0;
			Assert("Score is not sufficient for a likely match", !TestCollection.LikelyMatchesExist);

			orgMatch.OS_Score = TestCollection.LikelyMatchScore;
			Assert("Score is sufficient for a likely match", TestCollection.LikelyMatchesExist);

			orgMatch.OS_Score = 0;
			secondMatch.OS_Score = TestCollection.LikelyMatchScore;
			Assert("Score is sufficient for a likely match", TestCollection.LikelyMatchesExist);

			secondMatch.OS_Score = TestCollection.LikelyMatchScore + 1;
			Assert("Score is sufficient for a likely match", TestCollection.LikelyMatchesExist);
		}

		#endregion

		#region Specific Matching Cases

		public void TestUNLOCOMatchesOnCountry()
		{
			OrgHeader johnSmithApples1 = OrgHeader.New(Factory);
			johnSmithApples1.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples1.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples1.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples1.MainAddress.OA_City = "Appleville";

			Factory.Save(); // Gen Pattern Matches etc.

			OrgHeader johnSmithApples2 = OrgHeader.New(Factory);
			johnSmithApples2.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples2.OH_RL_NKClosestPort = "AU";
			johnSmithApples2.MainAddress.OA_Address1 = "Ernest Hemmingway Drive";
			johnSmithApples2.MainAddress.OA_City = "Melbourne";

			johnSmithApples2.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There is one similar org", 1, johnSmithApples2.SimilarOrgMatches.Count);
			AssertEquals("The Match is JohnSmithApples2", johnSmithApples2.SimilarOrgMatches[0].Header.PK, johnSmithApples1.PK);
		}

		public void TestUNLOCOFilterWithSpecialCharacters()
		{
			OrgHeader johnSmithApples1 = OrgHeader.New(Factory);
			johnSmithApples1.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples1.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples1.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples1.MainAddress.OA_City = "Appleville";

			Factory.Save();

			OrgHeader johnSmithApples2 = OrgHeader.New(Factory);
			johnSmithApples2.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples2.OH_RL_NKClosestPort = "AU[";
			johnSmithApples2.MainAddress.OA_Address1 = "Ernest Hemmingway Drive";
			johnSmithApples2.MainAddress.OA_City = "Appleville";

			johnSmithApples2.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("No similar organization was found for UNLOCO 'AU['", 1, johnSmithApples2.SimilarOrgMatches.Count);
			AssertEquals("Only one match should exist when using UNLOCO 'AU['", johnSmithApples2.SimilarOrgMatches[0].Header.PK, johnSmithApples1.PK);

			johnSmithApples2.OH_RL_NKClosestPort = "AU[\\\\";
			johnSmithApples2.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("No similar organization was found for UNLOCO 'AU[\\\\'", 1, johnSmithApples2.SimilarOrgMatches.Count);
			AssertEquals("Only one match should exist when using UNLOCO 'AU[\\\\'", johnSmithApples2.SimilarOrgMatches[0].Header.PK, johnSmithApples1.PK);
		}

		public void TestExcludeOrgsWithDifferentUNLOCO()
		{
			OrgHeader johnSmithApples1 = OrgHeader.New(Factory);
			johnSmithApples1.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples1.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples1.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples1.MainAddress.OA_City = "Appleville";
			johnSmithApples1.MainAddress.OA_Email = "Email@test.com";

			OrgHeader johnSmithApples2 = OrgHeader.New(Factory);
			johnSmithApples2.OH_FullName = "JAHN SMYTHE APLES";
			johnSmithApples2.OH_RL_NKClosestPort = "SGSIN";
			johnSmithApples2.MainAddress.OA_Address1 = "111 Some Road";
			johnSmithApples2.MainAddress.OA_City = "Appleville";

			Factory.Save(); // Gen Pattern Matches etc.

			// EVERYTHING exact match except UNLOCO
			OrgHeader johnSmithApples3 = OrgHeader.New(Factory);
			johnSmithApples3.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples3.OH_RL_NKClosestPort = "SGSIN";
			johnSmithApples3.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples3.MainAddress.OA_City = "Appleville";
			johnSmithApples3.MainAddress.OA_Email = "Email@test.com";

			johnSmithApples3.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There is one similar org", 1, johnSmithApples3.SimilarOrgMatches.Count);
			AssertEquals("The Match is JSA2 because JSA1 has UNLOCO in different Country", johnSmithApples3.SimilarOrgMatches[0].Header.PK, johnSmithApples2.PK);
		}

		public void TestExcludeOrgsWithDifferentBusinessRegNo()
		{
			OrgHeader johnSmithApples1 = OrgHeader.New(Factory);
			johnSmithApples1.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples1.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples1.PrimaryRegistrationNumber.Number = "123 123 123 12";
			johnSmithApples1.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples1.MainAddress.OA_City = "Appleville";
			johnSmithApples1.MainAddress.OA_Email = "Email@test.com";

			OrgHeader johnSmithApples2 = OrgHeader.New(Factory);
			johnSmithApples2.OH_FullName = "JAHN SMYTHE APLES";
			johnSmithApples2.PrimaryRegistrationNumber.Number = "9";
			johnSmithApples2.OH_RL_NKClosestPort = "AUMEL";
			johnSmithApples2.MainAddress.OA_Address1 = "111 Some Road";
			johnSmithApples2.MainAddress.OA_City = "Aplevile";

			Factory.Save(); // Gen Pattern Matches etc.

			// EVERYTHING exact match except Business Reg No
			OrgHeader johnSmithApples3 = OrgHeader.New(Factory);
			johnSmithApples3.OH_FullName = "JOHN SMITH APPLES";
			johnSmithApples3.OH_RL_NKClosestPort = "AUSYD";
			johnSmithApples3.PrimaryRegistrationNumber.Number = "9";
			johnSmithApples3.MainAddress.OA_Address1 = "111 Constitution Road";
			johnSmithApples3.MainAddress.OA_City = "Appleville";
			johnSmithApples3.MainAddress.OA_Email = "Email@test.com";

			johnSmithApples3.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There is one similar org", 1, johnSmithApples3.SimilarOrgMatches.Count);
			AssertEquals("The Match is JSA2 because JSA1 has different Business Reg No", johnSmithApples3.SimilarOrgMatches[0].Header.PK, johnSmithApples2.PK);
		}

		public void TestDontLoadDuplicatePatternMatchForSameOrgAndSameAddress()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Gaint planet";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "AAAAA Jupiter st.";
			org.MainAddress.OA_City = "Jupiter";
			factory.Save();

			var anotherPatternMatch = org.PatternMatchesForThisOrg[0].Clone();
			factory.Save();
			org.PatternMatchesForThisOrg.Load();

			var org2 = OrgHeader.New(Factory);
			org2.OH_FullName = org.OH_FullName;
			org2.OH_RL_NKClosestPort = org.OH_RL_NKClosestPort;
			org2.MainAddress.OA_Address1 = org.MainAddress.OA_Address1;
			org2.MainAddress.OA_City = org.MainAddress.OA_City;
			org2.SimilarOrgFinder.FindSimilarOrganisations();

			AssertEquals("Should only have one match since the two pattern match are for same org and same address", 1, org2.SimilarOrgMatches.Count);
		}

		#endregion

		#region Testing Match Threshold (Low/Medium/High/NeverMatch)

		const bool ExpectMatch = true;
		const bool ExpectNoMatch = false;

		public void TestMatchThresholdOverride()
		{
			string oldValue = OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value;

			TestCollection = new OrgPatternMatchCollection(Factory2);

			// Set it to not override.
			AssertMatchThreshold(OrgMatchThresholds.Codes.Low, "", OrgPatternMatchCollection.LowMatchThreshold, OrgPatternMatchCollection.MediumMatchThreshold, OrgPatternMatchCollection.HighMatchThreshold, OrgPatternMatchCollection.ExtremeMatchThreshold);

			// Set override to low
			AssertMatchThreshold(OrgMatchThresholds.Codes.High, OrgMatchThresholds.Codes.Low, OrgPatternMatchCollection.LowMatchThreshold, OrgPatternMatchCollection.LowMatchThreshold, OrgPatternMatchCollection.LowMatchThreshold, OrgPatternMatchCollection.LowMatchThreshold);

			// Set override to medium
			AssertMatchThreshold(OrgMatchThresholds.Codes.Low, OrgMatchThresholds.Codes.Medium, OrgPatternMatchCollection.MediumMatchThreshold, OrgPatternMatchCollection.MediumMatchThreshold, OrgPatternMatchCollection.MediumMatchThreshold, OrgPatternMatchCollection.MediumMatchThreshold);

			// Set override to High
			AssertMatchThreshold(OrgMatchThresholds.Codes.Low, OrgMatchThresholds.Codes.High, OrgPatternMatchCollection.HighMatchThreshold, OrgPatternMatchCollection.HighMatchThreshold, OrgPatternMatchCollection.HighMatchThreshold, OrgPatternMatchCollection.HighMatchThreshold);

			// Set override to Extreme
			AssertMatchThreshold(OrgMatchThresholds.Codes.Low, OrgMatchThresholds.Codes.Extreme, OrgPatternMatchCollection.ExtremeMatchThreshold, OrgPatternMatchCollection.ExtremeMatchThreshold, OrgPatternMatchCollection.ExtremeMatchThreshold, OrgPatternMatchCollection.ExtremeMatchThreshold);

			OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);
		}

		void AssertMatchThreshold(string registryThresholdSetting, string thresholdOverrideCode, int expectedThresholdWhenLow, int expectedThresholdWhenMedium, int expectedThresholdWhenHigh, int expectedThresholdWhenExtreme)
		{
			TestCollection.MatchThresholdOverride = thresholdOverrideCode;
			using (ThresholdRegistrySetter.SetThreshold(registryThresholdSetting))
			{
				AssertEquals("Threshold should be low", expectedThresholdWhenLow, TestCollection.LikelyMatchScore);
			}
			using (ThresholdRegistrySetter.SetThreshold(OrgMatchThresholds.Codes.Medium))
			{
				AssertEquals("Threshold should be medium", expectedThresholdWhenMedium, TestCollection.LikelyMatchScore);
			}
			using (ThresholdRegistrySetter.SetThreshold(OrgMatchThresholds.Codes.High))
			{
				AssertEquals("Threshold should be high", expectedThresholdWhenHigh, TestCollection.LikelyMatchScore);
			}
			using (ThresholdRegistrySetter.SetThreshold(OrgMatchThresholds.Codes.Extreme))
			{
				AssertEquals("Threshold should be Extreme", expectedThresholdWhenExtreme, TestCollection.LikelyMatchScore);
			}
		}

		public void TestMatchThresholdLow()
		{
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.Low, ExpectNoMatch, "Just below threshold",
				"Brian Fulling", "splaty place", "Sydney", "QLD", "9999");
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.Low, ExpectMatch, "Just after threshold",
				"FullName", "Address9", "Sydney", "QLD", "9999");
		}

		public void TestMatchThresholdMedium()
		{
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.Medium, ExpectNoMatch, "Just below threshold",
				"FullName", "splaty place", "Sydney", "QLD", "9999");
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.Medium, ExpectMatch, "Just after threshold",
				"FullName", "Address9", "Sydney", "QLD", "9999");
		}

		public void TestMatchThresholdHigh()
		{
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.High, ExpectNoMatch, "Just below threshold",
				"Patonga", "Wadonga", "Brisbane", "QLD", "9999");
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.High, ExpectMatch, "Just after threshold",
				"FullName", "Address1", "Sydney", "QLD", "9999");
		}

		public void TestMatchThresholdExtreme()
		{
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.Extreme, ExpectNoMatch, "Just below threshold",
				"FullName ltd", "Wadonga", "Sydney", "QLD", "9999");
			AssertMatchesForThresholdLevel(
				OrgMatchThresholds.Codes.Extreme, ExpectMatch, "Just after threshold",
				"FullName ltd", "Address1 AAA", "Sydney", "QLD", "9999");
		}

		void AssertMatchesForThresholdLevel(
			string thresholdCode, bool expectMatch, string failureMessage,
			string fullName, string address1, string city, string state, string postcode)
		{
			TestCaseHelper.ClearTable(OrgPatternMatch.Schema.TableName);
			BusinessObjectFactory factory = new BusinessObjectFactory();

			using (ThresholdRegistrySetter.SetThreshold(thresholdCode))
			{
				OrgHeader organisation = factory.NewWithValidTestData<OrgHeader>();
				organisation.SetDefaultValuesForTemporaryOrganisation();
				OrgHeader similarOrgToMatch = factory.NewWithValidTestData<OrgHeader>();

				similarOrgToMatch.OH_FullName = fullName;
				similarOrgToMatch.OH_RL_NKClosestPort = "AUSYD";
				similarOrgToMatch.MainAddress.OA_Address1 = address1;
				similarOrgToMatch.MainAddress.OA_City = city;
				similarOrgToMatch.MainAddress.OA_State = state;
				similarOrgToMatch.MainAddress.OA_PostCode = postcode;

				factory.Save();

				organisation.OH_FullName = "FullName ltd";
				organisation.OH_RL_NKClosestPort = "AUSYD";
				organisation.MainAddress.OA_Address1 = "Address1";
				organisation.MainAddress.OA_City = "Sydney";
				organisation.MainAddress.OA_State = "QLD";
				organisation.MainAddress.OA_PostCode = "9999";
				organisation.PatternMatchRequiresRegen = true;

				factory.Save();

				AssertSimilarOrganisationFound("For threshold " + thresholdCode + ": " + failureMessage, organisation, similarOrgToMatch, expectMatch);
			}
		}

		void AssertSimilarOrganisationFound(string failureMessage, OrgHeader organisation, OrgHeader expectedSimilarOrgMatch, bool expectMatch)
		{
			organisation.SimilarOrgFinder.FindSimilarOrganisations();
			if (expectMatch)
			{
				AssertEquals(failureMessage, 1, organisation.SimilarOrgMatches.Count);
				AssertEquals("Expected the found organisation to be the one we set up above for this test", expectedSimilarOrgMatch.PK, organisation.SimilarOrgMatches[0].OS_OH);
			}
			else
			{
				AssertEquals(failureMessage, 0, organisation.SimilarOrgMatches.Count);
			}
		}

		class ThresholdRegistrySetter : IDisposable
		{
			readonly string OldThreshold;

			protected ThresholdRegistrySetter(string oldThreshold)
			{
				this.OldThreshold = oldThreshold;
			}

			public static ThresholdRegistrySetter SetThreshold(string threshold)
			{
				ThresholdRegistrySetter result = new ThresholdRegistrySetter(OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value);
				OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, threshold);
				return result;
			}

			public void Dispose()
			{
				OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OldThreshold);
			}
		}

		#endregion

		#region Matching Inactive Organisations

		public void TestMatchInactiveOrganisation()
		{
			TestCaseHelper.ClearTable(OrgPatternMatch.Schema.TableName);

			OrgHeader header1 = OrgHeader.New(Factory);
			header1.OH_FullName = "ABCXYZ International";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.PrimaryRegistrationNumber.Number = "123 123 123 12";
			header1.MainAddress.OA_Address1 = "Address 1";

			Factory.Save();
			Assert("Pre: Is in Database for Org Matching", header1.IsInDatabase);

			OrgHeader header2 = OrgHeader.New(Factory);
			header2.OH_FullName = "ABCXYZ International";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.PrimaryRegistrationNumber.Number = "123 123 123 12";
			header2.MainAddress.OA_Address1 = "Address 1";

			header2.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There should be 1 match result returned", 1, header2.SimilarOrgMatches.Count);

			header1.OH_IsActive = ZBool.False;
			Factory.Save();

			header2.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("There should be NO matches found ", 0, header2.SimilarOrgMatches.Count);
		}

		#endregion

		#region Match UNMATCHED Organisation
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "TotalMatchCount is reAssigned in the function of resultList.LoadSimilarOrganisations.")]
		ZInt TotalMatchCount;

		public void TestMatchToUnmatchedOrganisation_NoSimilarOrgsFound()
		{
			EnableUseUnmatchedOrganisationForMatching();

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "QJKTC";
			testHeader.OH_FullName = "Some Completely Random Name";
			testHeader.OH_RL_NKClosestPort = "BTPHU";
			testHeader.MainAddress.OA_Address1 = "Hallabalooza";
			testHeader.MainAddress.OA_City = "Gobbeldygook";

			testHeader.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("One Match Found", 1, testHeader.SimilarOrgMatches.Count);
			AssertEquals("The Match is the UNMATCHED Org", "UNMATCHED", testHeader.SimilarOrgMatches[0].Header.OH_Code);

			OrgPatternMatchCollection resultList = new OrgPatternMatchCollection(Factory);
			resultList.LoadSimilarOrganisations(testHeader.PatternMatchesForThisOrg, null, out TotalMatchCount, true, false);
			AssertEquals("No Items in collection as we exclude the unmatched org", 0, resultList.Count);
		}

		[StressTest()]
		public void TestMatchToUnmatchedOrganisation_NotEnoughDetailForMatching()
		{
			EnableUseUnmatchedOrganisationForMatching();

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "Q";
			testHeader.OH_FullName = "A";

			testHeader.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("One Match Found", 1, testHeader.SimilarOrgMatches.Count);
			AssertEquals("The Match is the UNMATCHED Org", "UNMATCHED", testHeader.SimilarOrgMatches[0].Header.OH_Code);

			OrgPatternMatchCollection resultList = new OrgPatternMatchCollection(Factory);
			resultList.LoadSimilarOrganisations(testHeader.PatternMatchesForThisOrg, null, out TotalMatchCount, true, false);
			AssertEquals("No Items in collection as we exclude the unmatched org", 0, resultList.Count);
		}

		public void TestMatchToUnmatchedOrganisation_SimilarOrgsFound()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "DEMORG";
			testHeader.OH_FullName = "Demo Organisation";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "111 Demo St";
			testHeader.MainAddress.OA_City = "Demoville";

			testHeader.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("One Match Found", 1, testHeader.SimilarOrgMatches.Count); // UNMATCHED Org should not be in the list.
			AssertEquals("The Match is the Demo Org", "DEMORG", testHeader.SimilarOrgMatches[0].Header.OH_Code);
		}

		#endregion

		#region Limit Count

		public void TestLimitCount()
		{
			TestCaseHelper.ClearTable("OrgPatternMatch"); // Speed up test
			TestCollection = new OrgPatternMatchCollection(Factory);
			SetupMatches(TestCollection.MaximumResultsToShow + 20);
			AssertEquals("Count should be truncated at maximum count", TestCollection.MaximumResultsToShow, TestCollection.Count);
			AssertEquals("High ranking match should be first", HighRankingMatch, TestCollection[0]);

			for (int i = 1; i < TestCollection.MaximumResultsToShow; i++)
			{
				Assert("Following ranking objects should be the low ranking objects", LowRankingMatches.Contains(TestCollection[i]));
			}

			SetupMatches(TestCollection.MaximumResultsToShow - 1);
			AssertEquals("Count", TestCollection.MaximumResultsToShow - 1, TestCollection.Count);

			SetupMatches(TestCollection.MaximumResultsToShow);
			AssertEquals("Count", TestCollection.MaximumResultsToShow, TestCollection.Count);

			Factory2 = new BusinessObjectFactory();
			TestCollection = new OrgPatternMatchCollection(Factory2);
			OrgHeader firstOrg = Factory2.New<OrgHeader>();
			firstOrg.MainAddress.OA_Email = "XX";
			firstOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(firstOrg);
			firstOrg.PatternMatchesForThisOrg.Load();

			TestCollection.LoadSimilarOrganisations(firstOrg.PatternMatchesForThisOrg, null, out TotalMatchCount, true, true);
			AssertEquals("No results should be found", 0, TestCollection.Count);
		}

		#endregion

		#region GetFilter

		public void TestGetFilter()
		{
			var testHeader = Factory.New<OrgHeader>();
			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			var expectedFilter = string.Empty;
			AssertEquals("Filter is Empty (No Header PK Exclusions)", expectedFilter, (testHeader.PatternMatchesForThisOrg.GetFilter() as List<ZQuery>)[0].LiteralTextADO);

			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.OH_FullName = "XXX Industries Pty Limited";
			testHeader.MainAddress.OA_Address1 = "XXX Address";
			testHeader.MainAddress.OA_City = "XXX Address";
			var address2 = CreateAddress(testHeader, "You Got Sprung Address", "You Got Sprung Industries Pty Limited");
			testHeader.BrandsOrRelatedNames.AddNew("How Could You Industries Pty Limited");

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);

			expectedFilter = "(((((OS_CompanyName1 = 'X000' or OS_CompanyName2 = 'I532') or (OS_Address1 = 'X000' or OS_Address2 = 'A362')) or ((OS_CompanyName1 = 'X000' or OS_CompanyName2 = 'I532') or OS_FullCompanyName = 'XXX INDUSTRIES')) or (((OS_CompanyName1 = 'X000' or OS_CompanyName2 = 'I532') or (OS_Address1 = 'Y000' or OS_Address2 = 'G300' or OS_Address3 = 'S165' or OS_Address4 = 'A362')) or ((OS_CompanyName1 = 'X000' or OS_CompanyName2 = 'I532') or OS_FullCompanyName = 'XXX INDUSTRIES')) or (((OS_CompanyName1 = 'Y000' or OS_CompanyName2 = 'G300' or OS_CompanyName3 = 'S165' or OS_CompanyName4 = 'I532') or (OS_Address1 = 'Y000' or OS_Address2 = 'G300' or OS_Address3 = 'S165' or OS_Address4 = 'A362')) or ((OS_CompanyName1 = 'Y000' or OS_CompanyName2 = 'G300' or OS_CompanyName3 = 'S165' or OS_CompanyName4 = 'I532') or OS_FullCompanyName = 'YOU GOT SPRUNG INDUSTRIES')) or (((OS_CompanyName1 = 'H000' or OS_CompanyName2 = 'C430' or OS_CompanyName3 = 'Y000' or OS_CompanyName4 = 'I532') or (OS_Address1 = 'X000' or OS_Address2 = 'A362')) or ((OS_CompanyName1 = 'H000' or OS_CompanyName2 = 'C430' or OS_CompanyName3 = 'Y000' or OS_CompanyName4 = 'I532') or OS_FullCompanyName = 'HOW COULD YOU INDUSTRIES')) or (((OS_CompanyName1 = 'H000' or OS_CompanyName2 = 'C430' or OS_CompanyName3 = 'Y000' or OS_CompanyName4 = 'I532') or (OS_Address1 = 'Y000' or OS_Address2 = 'G300' or OS_Address3 = 'S165' or OS_Address4 = 'A362')) or ((OS_CompanyName1 = 'H000' or OS_CompanyName2 = 'C430' or OS_CompanyName3 = 'Y000' or OS_CompanyName4 = 'I532') or OS_FullCompanyName = 'HOW COULD YOU INDUSTRIES'))) and (OS_UNLOCO = 'AUSYD' or OS_UNLOCO = '' or OS_UNLOCO like 'AU%')) and OS_OH <> CONVERT('{0}', 'System.Guid')";

			AssertMultilineASCIIEquals("Filter is as expected",
				String.Format(expectedFilter, testHeader.PK.ToString()),
				((List<ZQuery>)testHeader.PatternMatchesForThisOrg.GetFilter())[0].LiteralTextADO);
			AssertEquals("The query is using the RECOMPILE option for performance improvements", true, ((List<ZQuery>)testHeader.PatternMatchesForThisOrg.GetFilter())[0].AddOptionRecompileConditionally);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Testing")]
		public void TestGetFilterWithLotsOfParameters()
		{
			Type dataAdapterType = ObjectFactory.GetType<Enterprise.Integration.Customs.DataTransfer.IDeclarationValueObjectDataAdapter>();
			MethodInfo newMethod = dataAdapterType.GetMethod("New", BindingFlags.Public | BindingFlags.Static, null, Array.Empty<Type>(), null);
			object dataAdapter = newMethod.Invoke(null, null);

			Type dataImporterType = ObjectFactory.GetType<Enterprise.Integration.Customs.DataTransfer.IDeclarationXmlDataImporter>();
			object dataImporter = Activator.CreateInstance(dataImporterType, dataAdapter);
			MethodInfo importMethod = dataImporterType.GetMethods().Single(m =>
				m.Name == "ImportDataToFactory" &&
				m.GetParameters().Length == 5
			);

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var testFilePath = resourceRetriever.SaveResourceToFile("SASH_S00234111_20070917124526.xml", "SASH_S00234111_20070917124526.xml");
				using (var reader = File.OpenText(testFilePath))
				{
					AssertNoExceptionThrown(() => importMethod.Invoke(dataImporter, new object[] { reader, "SASH_S00234111_20070917124526.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo, null }));
				}
			}

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			for (int i = 0; i < 12; i++)
			{
				string iString = i.ToString();
				OrgPatternMatch match = Collection.AddNew();
				match.OS_CompanyName1 = "C" + iString;
				match.OS_Address1 = "A" + iString;
				match.OS_OH = org1.PK;
			}

			IEnumerable<ZQuery> filterParts = Collection.GetFilter();
			ZQuery filter = new ZQuery();
			foreach (ZQuery part in filterParts)
			{
				filter.AddToFilter(part, JoinCondition.Or);
			}

			foreach (OrgPatternMatch match in Collection)
			{
				match.OS_OH = org2.PK;
			}

			Collection.RemoveAll();
			Collection.Load(filter);
			AssertEquals("Count", 12, Collection.Count);
		}

		#endregion

		#region Contains Pattern Match

		public void TestContainsBasedOnMatchDetails()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.OH_FullName = "XXX Industries Pty Limited";
			testHeader.PrimaryRegistrationNumber.Number = "123456";
			testHeader.MainAddress.OA_Address1 = "XXX Address";
			testHeader.MainAddress.OA_City = "City";

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			AssertEquals("Header should have one pattern match", 1, testHeader.PatternMatchesForThisOrg.Count);

			OrgPatternMatch testMatch = (OrgPatternMatch)testHeader.PatternMatchesForThisOrg[0].Clone();
			Assert("Match should be found in the collection", testHeader.PatternMatchesForThisOrg.ContainsBasedOnMatchDetails(testMatch));

			// Changing these should return no search results
			AssertChangeMatchAndSearch(testMatch.OS_FullCompanyNameInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", false);
			AssertChangeMatchAndSearch(testMatch.OS_Address1Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", false);
			AssertChangeMatchAndSearch(testMatch.OS_Address2Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", false);
			AssertChangeMatchAndSearch(testMatch.OS_Address3Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", false);
			AssertChangeMatchAndSearch(testMatch.OS_Address4Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", false);
			AssertChangeMatchAndSearch(testMatch.OS_Address4Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", false);
			AssertChangeMatchAndSearch(testMatch.OS_BusinessRegNoInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", false);

			// Changing these should not affect the search results
			AssertChangeMatchAndSearch(testMatch.OS_CompanyName1Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_CompanyName2Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_CompanyName3Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_CompanyName4Info, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_IsCorporationInfo, testHeader.PatternMatchesForThisOrg, ZBool.True, true);
			AssertChangeMatchAndSearch(testMatch.OS_StreetNumberInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_IsPOBoxInfo, testHeader.PatternMatchesForThisOrg, ZBool.True, true);
			AssertChangeMatchAndSearch(testMatch.OS_POBoxNumberInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_PostCodeInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_CityInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_StateInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_PhoneInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_FaxNumInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_UNLOCOInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_EmailInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
			AssertChangeMatchAndSearch(testMatch.OS_DomainInfo, testHeader.PatternMatchesForThisOrg, (ZString)"AAAA", true);
		}

		void AssertChangeMatchAndSearch(ZPropertyInfo patternMatchPropertyToChange, OrgPatternMatchCollection collectionToSearch, IZType newValue, bool shouldBeFoundInCollection)
		{
			OrgPatternMatch match = (OrgPatternMatch)patternMatchPropertyToChange.BizObj;

			object originalValue = patternMatchPropertyToChange.Value;
			patternMatchPropertyToChange.Value = newValue;
			string message = patternMatchPropertyToChange.Name + (shouldBeFoundInCollection ? " Should Be Found In Collection but was not" : " Should NOT Be Found In Collection but was");
			Assert(message, shouldBeFoundInCollection == collectionToSearch.ContainsBasedOnMatchDetails(match));

			patternMatchPropertyToChange.Value = (IZType)originalValue;
		}

		#endregion

		#region Generate Pattern Matches

		public void TestGeneratePatternMatchesFromOrg()
		{
			string companyName1 = "XXX Industries Pty Limited";
			string companyName2 = "You Got Sprung Industries pty Limited";
			string companyName3 = "How Could You Industries Pty Limited";
			string companyName4 = "I Thought You Loved Me Industries Pty Limited";

			string companyAddress1 = "XXX Address";
			string companyAddress2 = "You Got Sprung Address";
			string companyAddress3 = "Original Name Address";
			string companyAddress4 = "Name Address Duplicated";

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_FullName = companyName1;
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.BrandsOrRelatedNames.AddNew(companyName3);
			testHeader.BrandsOrRelatedNames.AddNew(companyName4);

			testHeader.MainAddress.OA_Address1 = companyAddress1;
			testHeader.MainAddress.OA_City = "City";
			OrgAddress address2 = CreateAddress(testHeader, companyAddress2, companyName2); // Address has Company Override
			OrgAddress address3 = CreateAddress(testHeader, companyAddress3, "");
			OrgAddress address4 = CreateAddress(testHeader, companyAddress4, "");
			OrgAddress address5 = CreateAddress(testHeader, companyAddress4, "");

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);

			#region expectedPatternMatches

			const string expectedPatternMatches = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [N500]
OS_Address2               - [A362]
OS_Address3               - [D142]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [O625]
OS_Address2               - [N500]
OS_Address3               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [N500]
OS_Address2               - [A362]
OS_Address3               - [D142]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [I000]
OS_CompanyName2           - [T230]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [L130]
OS_FullCompanyName        - [I THOUGHT YOU LOVED ME INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [O625]
OS_Address2               - [N500]
OS_Address3               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [I000]
OS_CompanyName2           - [T230]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [L130]
OS_FullCompanyName        - [I THOUGHT YOU LOVED ME INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [I000]
OS_CompanyName2           - [T230]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [L130]
OS_FullCompanyName        - [I THOUGHT YOU LOVED ME INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [I000]
OS_CompanyName2           - [T230]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [L130]
OS_FullCompanyName        - [I THOUGHT YOU LOVED ME INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [N500]
OS_Address2               - [A362]
OS_Address3               - [D142]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [O625]
OS_Address2               - [N500]
OS_Address3               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]";

			#endregion

			AssertPatternMatches("1 pattern match per name (4) per address (5), 1 address doesn't have enough info to generate, 1 name is only valid for one address", 13, expectedPatternMatches, testHeader);
		}

		public void TestGeneratePatternMatchesFromOrgWithLocalBusinessNumber()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_FullName = "Company1";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "Address";
			testHeader.MainAddress.OA_City = "City";

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			AssertEquals("Generated with Business Number (but none specified)", 1, testHeader.PatternMatchesForThisOrg.Count);
			AssertEquals("The Business Registration Number should be blank for Match 1", "", testHeader.PatternMatchesForThisOrg[0].OS_BusinessRegNo);

			testHeader.PrimaryRegistrationNumber.Number = "123456";
			OrgCusCode vATCode = testHeader.CustomsCodes.AddNew();
			vATCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vATCode.OK_CustomsRegNo = "abc123";

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			AssertEquals("There are two pattern matches for this org", 2, testHeader.PatternMatchesForThisOrg.Count);
			if (testHeader.PatternMatchesForThisOrg[0].OS_BusinessRegNo == "123456")
			{
				AssertEquals("The Business Registration Number should be the VATCode for Match 2", "abc123", testHeader.PatternMatchesForThisOrg[1].OS_BusinessRegNo);
			}
			else
			{
				AssertEquals("The Business Registration Number should be the ABN for Match 1", "abc123", testHeader.PatternMatchesForThisOrg[0].OS_BusinessRegNo);
				AssertEquals("The Business Registration Number should be the VATCode for Match 2", "123456", testHeader.PatternMatchesForThisOrg[1].OS_BusinessRegNo);
			}
		}

		[TestDate(2011, 5, 5, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestGeneratePatterns_NoFullRegen()
		{
			#region Setup and check initial match generation

			var companyName1 = "XXX INDUSTRIES PTY LIMITED";
			var companyName2 = "YOU GOT SPRUNG INDUSTRIES PTY LIMITED";
			var companyName3 = "HOW COULD YOU INDUSTRIES PTY LIMITED";
			var companyName4 = "I THOUGHT YOU LOVED ME INDUSTRIES PTY LIMITED";
			var companyName5 = "GET LOST INDUSTRIES PTY LIMITED";

			var companyAddress1 = "XXX ADDRESS";
			var companyAddress2 = "YOU GOT SPRUNG ADDRESS";
			var companyAddress3 = "HOW COULD YOU ADDRESS";
			var companyAddress4 = "I THOUGHT YOU LOVED ME ADDRESS";
			var companyAddress5 = "GET LOST ADDRESS";

			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader organisation = factory.New<OrgHeader>();
			organisation.OH_Code = "~=TESTING=~";
			organisation.OH_FullName = companyName1;
			organisation.OH_RL_NKClosestPort = "AUSYD";

			var address1 = SetupAddress(organisation.MainAddress, companyAddress1, "");
			var address2 = CreateAddress(organisation, companyAddress2, companyName2);
			var address3 = CreateAddress(organisation, companyAddress3, companyName3);
			var address4 = CreateAddress(organisation, companyAddress4, companyName4);

			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatches

			string expectedPatternMatches = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [I000]
OS_CompanyName2           - [T230]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [L130]
OS_FullCompanyName        - [I THOUGHT YOU LOVED ME INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]";

			#endregion
			CombineAssertions(delegate
			{
				AssertEquals("4 addresses, 3 with name overrides.", 7, organisation.PatternMatchesForThisOrg.Count);
				AssertPatternMatches("4 addresses, 3 with name overrides.", 7, expectedPatternMatches, organisation);
			});

			#endregion

			#region Change of company name

			factory.Save();
			address4.OA_CompanyNameOverride = companyName4 = "NEW OVERRIDE 123";
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatches

			expectedPatternMatches = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [G300]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]";

			#endregion
			CombineAssertions(delegate
			{
				int newAddressCompanyNameOverrideMatchCount = organisation.PatternMatchesForThisOrg.Cast<OrgPatternMatch>().Count(m => m.OS_FullCompanyName == companyName4);
				AssertEquals("newAddressCompanyNameOverrideMatchCount", 1, newAddressCompanyNameOverrideMatchCount);
				AssertPatternMatches("After change of Address Company Name Override.", 7, expectedPatternMatches, organisation);
			});

			#endregion

			#region Change of address

			address2.OA_Address1 = "YOU CAN SPRING ADDRESS";
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatches

			expectedPatternMatches = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]";

			#endregion
			AssertPatternMatches("After change of Address Number 2.", 7, expectedPatternMatches, organisation);

			#endregion

			#region Addition of new address

			OrgAddress address5 = CreateAddress(organisation, companyAddress5, companyName5);
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatchesIncludingNewAddress

			var expectedPatternMatchesIncludingNewAddress = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [G300]
OS_Address2               - [L230]
OS_Address3               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [G300]
OS_CompanyName2           - [L230]
OS_CompanyName3           - [I532]
OS_FullCompanyName        - [GET LOST INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [G300]
OS_Address2               - [L230]
OS_Address3               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [AUSYD]";

			#endregion
			AssertPatternMatches("After addition of new address with name override.", 9, expectedPatternMatchesIncludingNewAddress, organisation);

			#endregion

			#region Removal of address

			organisation.Addresses.RemoveAndDelete(address5);
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			AssertPatternMatches("After removing address we just added should go back to as before.", 7, expectedPatternMatches, organisation);

			#endregion

			#region Change on main unloco

			organisation.OH_RL_NKClosestPort = "UAIEV";
			address2.OA_RL_NKRelatedPortCode = "GBLON";
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatches

			expectedPatternMatches = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [GBLON]";

			#endregion
			CombineAssertions(delegate
			{
				var matches = organisation.PatternMatchesForThisOrg.ToArray<OrgPatternMatch>();
				AssertEquals("Match Count with UNLOCO of GBLON", 2, matches.Count(m => m.OS_UNLOCO.ToUpper() == "GBLON"));
				AssertEquals("Match Count with UNLOCO of UAIEV", 5, matches.Count(m => m.OS_UNLOCO.ToUpper() == "UAIEV"));
				AssertPatternMatches("After changing UNLOCOs including main.", 7, expectedPatternMatches, organisation);
			});

			#endregion

			#region Addition of applicable and non applicable cus code

			factory.Save();
			OrgCusCode code = organisation.CustomsCodes.AddNew();
			code.OK_CustomsRegNo = "41 065 894 724";
			code.OK_CodeType = "GST";
			code = organisation.CustomsCodes.AddNew();
			code.OK_CustomsRegNo = "1234";
			code.OK_CodeType = "CID";
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatchesWithBusinessRegNo

			string expectedPatternMatchesWithBusinessRegNo = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [GBLON]";

			#endregion
			CombineAssertions(delegate
			{
				AssertEquals("All matches should be regenerated with new applicable Cus Code.", 7, organisation.PatternMatchesForThisOrg.Cast<OrgPatternMatch>().Count(m => m.OS_BusinessRegNo == "41065894724"));
				AssertPatternMatches("After adding one matchable and one non applicable Cus Code.", 7, expectedPatternMatchesWithBusinessRegNo, organisation);
			});

			#endregion

			#region Removal of not applicable cus code

			expectedPatternMatchesWithBusinessRegNo = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]";
			factory.Save();
			organisation.CustomsCodes.RemoveAndDelete(code);
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			CombineAssertions(delegate
			{
				AssertEquals("All matches should still have applicable Cus Code.", 7, organisation.PatternMatchesForThisOrg.Cast<OrgPatternMatch>().Count(m => m.OS_BusinessRegNo == "41065894724"));
				AssertPatternMatches("After removing non applicable Cus Code.", 7, expectedPatternMatchesWithBusinessRegNo, organisation);
			});
			#endregion

			#region Addition of another applicable code

			factory.Save();
			code = organisation.CustomsCodes.AddNew();
			code.OK_CustomsRegNo = "23 112 936 991";
			code.OK_CodeType = "GCR";
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatchesWithSecondBusinessRegNo

			var expectedPatternMatchesWithSecondBusinessRegNo = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [23112936991]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [23112936991]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [23112936991]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [23112936991]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [23112936991]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [23112936991]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [23112936991]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [41065894724]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]";

			#endregion
			CombineAssertions(delegate
			{
				var matches = organisation.PatternMatchesForThisOrg.ToArray<OrgPatternMatch>();
				AssertEquals("Match Count with BusinessNo of 41065894724", 7, matches.Count(m => m.OS_BusinessRegNo.ToUpper() == "41065894724"));
				AssertEquals("Match Count with BusinessNo of 23112936991", 7, matches.Count(m => m.OS_BusinessRegNo.ToUpper() == "23112936991"));
				AssertPatternMatches("After adding one more matchable Cus Code.", 14, expectedPatternMatchesWithSecondBusinessRegNo, organisation);
			});

			#endregion

			#region Removal of second applicable cus code

			factory.Save();
			organisation.CustomsCodes.RemoveAndDelete(code);
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			CombineAssertions(delegate
			{
				AssertEquals("All matches should only have the one applicable Cus Code.", 7, organisation.PatternMatchesForThisOrg.Cast<OrgPatternMatch>().Count(m => m.OS_BusinessRegNo == "41065894724"));
				AssertPatternMatches("After removing the second applicable Cus Code.", 7, expectedPatternMatchesWithBusinessRegNo, organisation);
			});

			#endregion

			#region Removal of all applicable cus codes

			expectedPatternMatches = @"---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]";
			factory.Save();
			code = organisation.CustomsCodes.AddNew();
			code.OK_CustomsRegNo = "23 112 936 991";
			code.OK_CodeType = "GCR";
			factory.Save();
			organisation.CustomsCodes.RemoveAndDeleteAll();
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			CombineAssertions(delegate
			{
				AssertEquals("All matches should have no Business Reg No.", 7, organisation.PatternMatchesForThisOrg.Cast<OrgPatternMatch>().Count(m => m.OS_BusinessRegNo.IsEmpty));
				AssertPatternMatches("After removing all applicable Cus Codes.", 7, expectedPatternMatches, organisation);
			});

			#endregion

			#region Addition of brand

			factory.Save();
			var brand = organisation.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "AWESOMENESS UNLIMITED";
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			#region expectedPatternMatchesWithBrand

			var expectedPatternMatchesWithBrand = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [A255]
OS_CompanyName2           - [U545]
OS_FullCompanyName        - [AWESOMENESS UNLIMITED]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [A255]
OS_CompanyName2           - [U545]
OS_FullCompanyName        - [AWESOMENESS UNLIMITED]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [A255]
OS_CompanyName2           - [U545]
OS_FullCompanyName        - [AWESOMENESS UNLIMITED]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [A255]
OS_CompanyName2           - [U545]
OS_FullCompanyName        - [AWESOMENESS UNLIMITED]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [H000]
OS_CompanyName2           - [C430]
OS_CompanyName3           - [Y000]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [HOW COULD YOU INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [N000]
OS_CompanyName2           - [O163]
OS_CompanyName3           - [1000]
OS_FullCompanyName        - [NEW OVERRIDE 123]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [H000]
OS_Address2               - [C430]
OS_Address3               - [Y000]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [I000]
OS_Address2               - [T230]
OS_Address3               - [Y000]
OS_Address4               - [L130]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [X000]
OS_Address2               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_State                  - [N200]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [UAIEV]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [X000]
OS_CompanyName2           - [I532]
OS_FullCompanyName        - [XXX INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [Y000]
OS_Address2               - [C500]
OS_Address3               - [S165]
OS_Address4               - [A362]
OS_AddressLanguage        - [EN]
OS_City                   - [C300]
OS_CompanyName1           - [Y000]
OS_CompanyName2           - [G300]
OS_CompanyName3           - [S165]
OS_CompanyName4           - [I532]
OS_FullCompanyName        - [YOU GOT SPRUNG INDUSTRIES]
OS_IsCorporation          - [Y]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [GBLON]";

			#endregion
			CombineAssertions(delegate
			{
				AssertEquals("Maches with Brand name.", 4, organisation.PatternMatchesForThisOrg.Cast<OrgPatternMatch>().Count(m => m.OS_FullCompanyName == "AWESOMENESS UNLIMITED"));
				AssertPatternMatches("After adding a brand.", 11, expectedPatternMatchesWithBrand, organisation);
			});

			#endregion

			#region Removal of brand

			factory.Save();
			organisation.BrandsOrRelatedNames.RemoveAndDelete(brand);
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);

			CombineAssertions(delegate
			{
				AssertEquals("Maches with Brand name after deleting the brand.", 0, organisation.PatternMatchesForThisOrg.Cast<OrgPatternMatch>().Count(m => m.OS_FullCompanyName == "AWESOMENESS UNLIMITED"));
				AssertPatternMatches("After removing the brand should go back to original.", 7, expectedPatternMatches, organisation);
			});

			#endregion
		}

		public void TestWithOverridenAddedAddressesAndNames()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader org = factory.New<OrgHeader>();
			org.OH_Code = "~=testing=~";
			org.OH_FullName = "~=testing=~=org~=";
			org.OH_RL_NKClosestPort = "AUSYD";

			org.MainAddress.OA_Address1 = "address1";
			org.MainAddress.OA_City = "City";

			org.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(org);
			AssertEquals("1 pattern match per name per address", 1, org.PatternMatchesForThisOrg.Count);

			factory.Save();

			org.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(org);
			AssertEquals("still one", 1, org.PatternMatchesForThisOrg.Count);

			List<IMatchingAddress> addressesOverride = new List<IMatchingAddress>();
			OrgAddress adr1 = factory.NewWithValidTestData<OrgAddress>();
			adr1.OA_OH = org.PK;
			adr1.OA_Address1 = "xxx";

			OrgAddress adr2 = factory.NewWithValidTestData<OrgAddress>();
			adr2.OA_OH = org.PK;
			adr2.OA_Address1 = "yyy";

			addressesOverride.Add(adr1);
			addressesOverride.Add(adr2);

			var names = new List<OrganisationName>();
			names.Add(new OrganisationName("other name", Constants.Languages.English));
			names.Add(new OrganisationName("other name 2", Constants.Languages.English));

			factory.Save();

			org.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(new BusinessObjectFactory().Load<OrgHeader>(org.PK), addressesOverride, names);
			AssertEquals("should generate patterns from overrides thou org wanst changed", 9, org.PatternMatchesForThisOrg.Count);
		}

		public void TestGenerateMatchForBusinessRegistrationNumbers()
		{
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(OrgCusCode.CodeTypes.GSTCode, true);
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, true);
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(OrgCusCode.CodeTypes.GovBusinessCode, true);
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(OrgCusCode.CodeTypes.CorporationCode, true);
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(OrgCusCode.CodeTypes.VATCode, true);
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfBusiness, true);
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany, true);

			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(OrgCusCode.CodeTypes.CarrierCode, false);
			AssertPatternMatchesGeneratedForBusinessRegistrationNumber(OrgCusCode.CodeTypes.DeliveranceCode, false);
		}

		void AssertPatternMatchesGeneratedForBusinessRegistrationNumber(string codeType, bool matchShouldContainRegNo)
		{
			string codeValue = "abc123";

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_FullName = "Company1";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "Address";
			testHeader.MainAddress.OA_City = "City";
			OrgCusCode customsCode = testHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = codeType;
			customsCode.OK_CustomsRegNo = codeValue;

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			AssertEquals("Pattern Match Generated with Code Type " + codeType, 1, testHeader.PatternMatchesForThisOrg.Count);
			AssertEquals("The Business Reg No should " + (!matchShouldContainRegNo ? "NOT " : "") + "contain the Reg No", matchShouldContainRegNo ? codeValue : "", testHeader.PatternMatchesForThisOrg[0].OS_BusinessRegNo);
		}

		public void TestGeneratingMatchDoesNotCreateMainAddress()
		{
			HeaderForTest testHeader = Factory.New<HeaderForTest>();
			testHeader.AddressesForTest = new OrgAddressDependentCollection(testHeader);

			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_FullName = "Company1";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.AddressesForTest.RemoveAndDeleteAllIncludingMainAddress();
			OrgAddress dummy = testHeader.AddressesNoAutoCreate.AddNew();
			dummy.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			dummy.OA_Address1 = "Dummy";
			dummy.OA_City = "Dummy";

			AssertEquals("There should be only 1 address", 1, testHeader.AddressesNoAutoCreate.Count);

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			AssertEquals("There should be only 1 address", 1, testHeader.AddressesNoAutoCreate.Count);
			AssertEquals("Address should be Pickup", ZBool.True, testHeader.AddressesNoAutoCreate[0].AddressCapability.GetCapabilityEnabled(OrgAddressType.Pickup));
		}

		class HeaderForTest : OrgHeader
		{
			public HeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public OrgAddressDependentCollection AddressesForTest
			{
				get { return base.AddressesInternal; }
				set { base.AddressesInternal = value; }
			}
		}

		#endregion

		#region Helpers

		static OrgAddress CreateAddress(OrgHeader header, string address1, string companyOverride)
		{
			return SetupAddress(header.Addresses.AddNew(), address1, companyOverride);
		}

		static OrgAddress SetupAddress(OrgAddress address, string address1, string companyOverride)
		{
			address.OA_Address1 = address1;
			address.OA_City = "CITY";
			address.OA_CompanyNameOverride = companyOverride;
			return address;
		}

		OrgPatternMatchCollection LoadSimilarPatternMatches(OrgHeader headerToMatch, int expectedResults)
		{
			OrgPatternMatchCollection result = new OrgPatternMatchCollection(Factory);
			ZInt totalMatchCount;
			result.LoadSimilarOrganisations(headerToMatch.PatternMatchesForThisOrg, null, out totalMatchCount, true, true);
			AssertEquals(expectedResults + " Results returned", expectedResults, result.Count);

			return result;
		}

		OrgHeader GenerateCheckHeader(string companyName, string uNLOCO, string address1, string city)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = companyName;
			result.OH_RL_NKClosestPort = uNLOCO;
			result.MainAddress.OA_Address1 = address1;
			result.MainAddress.OA_City = city;

			return result;
		}

		// This test only is concerned of checking against a few fields to determine uniqueness - Matching on all fields is not being tested here.
		void AssertFoundPatternMatchHasTheseDetails(OrgPatternMatch matchToCheck, OrgHeader headerThatShouldHaveBeenMatched, string companyNameThatShouldHaveBeenMatched, OrgAddress addressThatShouldHaveBeenMatched, string message)
		{
			AssertEquals("Checking " + message + " CompanyName matches expected", companyNameThatShouldHaveBeenMatched, matchToCheck.OH_FullName);
			AssertEquals("Checking " + message + " Header matches expected", matchToCheck.OS_OH, headerThatShouldHaveBeenMatched.PK);
			AssertEquals("Checking " + message + " Address matches expected", matchToCheck.OS_OA, addressThatShouldHaveBeenMatched.PK);
		}

		void SetupRealisticOrgsForTest()
		{
			int originalDBCount = Factory.GetDatabaseCount(typeof(OrgPatternMatch));

			Header1 = Factory.New<OrgHeader>();
			Header1.OH_FullName = "NOOBSRUS CONSOLIDATORS";
			Header1.OH_RL_NKClosestPort = "AUSYD";
			Header1.MainAddress.OA_Address1 = "Main";
			Header1.MainAddress.OA_City = "Alexandria";
			Header1.MainAddress.OA_State = "NSW";
			Header1Address = Header1.Addresses.AddNew();
			Header1Address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			Header1Address.OA_Address1 = "Pickup";
			Header1Address.OA_City = "Mascot";

			Header2 = Factory.New<OrgHeader>();
			Header2.OH_FullName = "NOOBSRUS CONSOLIDATORS ALTERNATIVE";
			Header2.OH_RL_NKClosestPort = "AUSYD";
			Header2.MainAddress.OA_Address1 = "Main";
			Header2.MainAddress.OA_City = "Alexandria";
			Header2.MainAddress.OA_State = "NSW";
			Header2Address = Header2.Addresses.AddNew();
			Header2Address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			Header2Address.OA_Address1 = "Delivery";
			Header2Address.OA_City = "Botany";

			Factory.Save();
			AssertEquals("There should be 4 new pattern match records in the DB", originalDBCount + 4, Factory.GetDatabaseCount(typeof(OrgPatternMatch)));
		}

		#endregion

		#region Implementation

		OrgPatternMatchCollection TestCollection;
		ArrayList LowRankingMatches;
		OrgPatternMatch HighRankingMatch;
		OrgHeader FirstOrg;
		BusinessObjectFactory Factory2;
		static int OrgNameGeneratorCount;
		OrgHeader Header1;
		OrgHeader Header2;
		OrgAddress Header1Address;
		OrgAddress Header2Address;

		protected override void SetUp()
		{
			base.SetUp();
			Factory2 = new BusinessObjectFactory();
			Header1 = null;
			Header2 = null;
			Header1Address = null;
			Header2Address = null;
			SetupRealisticOrgsForTest();

			OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgMatchThresholds.Codes.Low);
		}

		new OrgPatternMatchCollection Collection => (OrgPatternMatchCollection)base.Collection;

		void SetupMatches(int possibleMatches)
		{
			OrgNameGeneratorCount++;

			Factory2 = new BusinessObjectFactory();
			FirstOrg = Factory2.New<OrgHeader>();
			FirstOrg.OH_FullName = "FullName";
			FirstOrg.OH_Code = "XXXYYY" + OrgNameGeneratorCount.ToString();
			FirstOrg.OH_RL_NKClosestPort = "AUSYD";
			FirstOrg.PrimaryRegistrationNumber.Number = "ZZ";
			FirstOrg.MainAddress.OA_Address1 = "Address";
			FirstOrg.MainAddress.OA_City = "City";
			FirstOrg.MainAddress.OA_Email = "XX";
			FirstOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(FirstOrg);
			FirstOrg.PatternMatchesForThisOrg.Load();

			LowRankingMatches = CreateOrgMatches(possibleMatches - 1, "ZZ", "JJ");
			HighRankingMatch = (OrgPatternMatch)CreateOrgMatches(1, "ZZ", "XX")[0];
			TestCollection = new OrgPatternMatchCollection(Factory2);
			TestCollection.LoadSimilarOrganisations(FirstOrg.PatternMatchesForThisOrg, null, out TotalMatchCount, true, true);
		}

		ArrayList CreateOrgMatches(int count, string businessRegNo, string email)
		{
			ArrayList orgMatches = new ArrayList();
			for (int i = 0; i < count; i++)
			{
				OrgNameGeneratorCount++;
				OrgHeader org = Factory2.New<OrgHeader>();
				org.OH_FullName = "FullName";
				org.OH_Code = "XXXYYY" + OrgNameGeneratorCount.ToString();
				org.OH_RL_NKClosestPort = "AUSYD";
				org.PrimaryRegistrationNumber.Number = businessRegNo;
				org.MainAddress.OA_Address1 = "Address";
				org.MainAddress.OA_City = "City";
				org.MainAddress.OA_Email = email;
				org.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(org);
				org.PatternMatchesForThisOrg.Load();

				foreach (OrgPatternMatch match in org.PatternMatchesForThisOrg)
				{
					orgMatches.Add(match);
				}
			}

			return orgMatches;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgPatternMatchCollection(Factory);
		}

		void EnableUseUnmatchedOrganisationForMatching()
		{
			UnmatchedOrganisation unmatchedOrg = new UnmatchedOrganisation();
			unmatchedOrg.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrg);
		}

		static void AssertPatternMatches(string message, int expectedPatternMatchCount, string expectedPatternMatches, OrgHeader organisation)
		{
			var query = new ZQuery(OrgPatternMatchSchema.OS_OH, organisation.PK);
			AssertEquals(message, expectedPatternMatchCount, organisation.Factory.Load<OrgPatternMatch>(query).Length);

			var sortOrder = OrgPatternMatchSchema.OS_FullCompanyName.Name + ", " + OrgPatternMatchSchema.OS_Address1.Name + ", " + OrgPatternMatchSchema.OS_BusinessRegNo.Name;
			var actualPatternMatches = organisation.Factory.SerialiseForTesting<OrgPatternMatch>(query, sortOrder);
			AssertMultilineASCIIEquals(message, expectedPatternMatches, actualPatternMatches);
		}

		#endregion
	}
}
