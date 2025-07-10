using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching.Testing
{
	sealed class PatternMatchGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2011, 5, 5, 1, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestMatchGenerationOnlyGeneratesForAddressLevelCompanyNameOverrides()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "BENNY BANANA STRIKES AGAIN";
			organisation.OH_RL_NKClosestPort = "AUBNE";

			var address1 = organisation.MainAddress;
			address1.OA_CompanyNameOverride = "WAYNE BENNETT WANTS TO COACH TOADS";
			address1.OA_Address1 = "243 BANANANANANANANANA PLACE";
			address1.OA_City = "SKINTIGHT SACKVILLE";
			address1.OA_PostCode = "4378";
			address1.OA_State = "QLD";
			address1.OA_Phone = "61 8 8423 1231";
			address1.OA_Mobile = "61 421 565 787";
			address1.OA_Fax = "61 8 8423 1245";
			address1.OA_Email = "bananabender@doohdoohdhduhder.com.au";
			address1.OA_RL_NKRelatedPortCode = "AUBNE";

			var address2 = organisation.Addresses.AddNew();
			address2.OA_CompanyNameOverride = "TOMMY RAUDONIKIS COACHED COCKROACHES";
			address2.OA_Address1 = "123 UNDER FRIDGE ROAD";
			address2.OA_City = "SAGGY BROWTOWN";
			address2.OA_PostCode = "2545";
			address2.OA_State = "NSW";
			address2.OA_Phone = "61 2 8423 1231";
			address2.OA_Mobile = "61 478 565 787";
			address2.OA_Fax = "61 2 8423 1245";
			address2.OA_Email = "cockroach@ahgsplesheerplserheer.com.au";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";

			organisation.PrimaryRegistrationNumber.Number = "61124598796";
			organisation.CustomsCodes.AddNew("GBR", "GNADS");

			Factory.Save();

			var query = new ZQuery(OrgPatternMatchSchema.OS_OH, organisation.PK);
			var patternMatches = Factory.Load<OrgPatternMatch>(query);

			CombineAssertions(delegate
			{
				AssertEquals("Total count of Pattern Match rows", 8, patternMatches.Length);
				AssertEquals("Pattern Match rows for BENNY BANANA (Org Name)", 4, patternMatches.Count(m => m.OS_FullCompanyName == "BENNY BANANA STRIKES AGAIN"));
				AssertEquals("Pattern Match rows for WAYNE BENNETT (Add 1 Ovr)", 2, patternMatches.Count(m => m.OS_FullCompanyName == "WAYNE BENNETT WANTS TO COACH TOADS"));
				AssertEquals("Pattern Match rows for TOMMY RAUDONIKIS (Add 2 Ovr)", 2, patternMatches.Count(m => m.OS_FullCompanyName == "TOMMY RAUDONIKIS COACHED COCKROACHES"));
			});

			#region expectedPatternMatches

			const string expectedPatternMatches = @"
---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [B555]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [61124598796]
OS_City                   - [S532]
OS_CompanyName1           - [B500]
OS_CompanyName2           - [B550]
OS_CompanyName3           - [S362]
OS_CompanyName4           - [A250]
OS_Domain                 - [DOOHDOOHDHDUHDER.COM.AU]
OS_Email                  - [BANANABENDER]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [BENNY BANANA STRIKES AGAIN]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [4378]
OS_State                  - [Q430]
OS_StreetNumber           - [243]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUBNE]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [U536]
OS_Address2               - [F632]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [61124598796]
OS_City                   - [S216]
OS_CompanyName1           - [B500]
OS_CompanyName2           - [B550]
OS_CompanyName3           - [S362]
OS_CompanyName4           - [A250]
OS_Domain                 - [AHGSPLESHEERPLSERHEER.COM.AU]
OS_Email                  - [COCKROACH]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [BENNY BANANA STRIKES AGAIN]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [2545]
OS_State                  - [N200]
OS_StreetNumber           - [123]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [B555]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [GNADS]
OS_City                   - [S532]
OS_CompanyName1           - [B500]
OS_CompanyName2           - [B550]
OS_CompanyName3           - [S362]
OS_CompanyName4           - [A250]
OS_Domain                 - [DOOHDOOHDHDUHDER.COM.AU]
OS_Email                  - [BANANABENDER]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [BENNY BANANA STRIKES AGAIN]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [4378]
OS_State                  - [Q430]
OS_StreetNumber           - [243]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUBNE]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [U536]
OS_Address2               - [F632]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [GNADS]
OS_City                   - [S216]
OS_CompanyName1           - [B500]
OS_CompanyName2           - [B550]
OS_CompanyName3           - [S362]
OS_CompanyName4           - [A250]
OS_Domain                 - [AHGSPLESHEERPLSERHEER.COM.AU]
OS_Email                  - [COCKROACH]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [BENNY BANANA STRIKES AGAIN]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [2545]
OS_State                  - [N200]
OS_StreetNumber           - [123]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [U536]
OS_Address2               - [F632]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [61124598796]
OS_City                   - [S216]
OS_CompanyName1           - [T500]
OS_CompanyName2           - [R352]
OS_CompanyName3           - [C230]
OS_CompanyName4           - [C262]
OS_Domain                 - [AHGSPLESHEERPLSERHEER.COM.AU]
OS_Email                  - [COCKROACH]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [TOMMY RAUDONIKIS COACHED COCKROACHES]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [2545]
OS_State                  - [N200]
OS_StreetNumber           - [123]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [U536]
OS_Address2               - [F632]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [GNADS]
OS_City                   - [S216]
OS_CompanyName1           - [T500]
OS_CompanyName2           - [R352]
OS_CompanyName3           - [C230]
OS_CompanyName4           - [C262]
OS_Domain                 - [AHGSPLESHEERPLSERHEER.COM.AU]
OS_Email                  - [COCKROACH]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [TOMMY RAUDONIKIS COACHED COCKROACHES]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [2545]
OS_State                  - [N200]
OS_StreetNumber           - [123]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUSYD]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [B555]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [61124598796]
OS_City                   - [S532]
OS_CompanyName1           - [W500]
OS_CompanyName2           - [B530]
OS_CompanyName3           - [W532]
OS_CompanyName4           - [T000]
OS_Domain                 - [DOOHDOOHDHDUHDER.COM.AU]
OS_Email                  - [BANANABENDER]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [WAYNE BENNETT WANTS TO COACH TOADS]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [4378]
OS_State                  - [Q430]
OS_StreetNumber           - [243]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUBNE]

---- OrgPatternMatch -----------------------------------------------------------
OS_Address1               - [B555]
OS_AddressLanguage        - [EN]
OS_BusinessRegNo          - [GNADS]
OS_City                   - [S532]
OS_CompanyName1           - [W500]
OS_CompanyName2           - [B530]
OS_CompanyName3           - [W532]
OS_CompanyName4           - [T000]
OS_Domain                 - [DOOHDOOHDHDUHDER.COM.AU]
OS_Email                  - [BANANABENDER]
OS_FaxNum                 - [4231245]
OS_FullCompanyName        - [WAYNE BENNETT WANTS TO COACH TOADS]
OS_IsCorporation          - [N]
OS_IsPOBox                - [N]
OS_OA                     - [GUID Hidden]
OS_OH                     - [GUID Hidden]
OS_Phone                  - [4231231]
OS_PostCode               - [4378]
OS_State                  - [Q430]
OS_StreetNumber           - [243]
OS_SystemCreateTimeUtc    - [05-May-11 01:01:01]
OS_SystemCreateUser       - [E]
OS_SystemLastEditTimeUtc  - [05-May-11 01:01:01]
OS_SystemLastEditUser     - [E]
OS_UNLOCO                 - [AUBNE]";
			#endregion

			var actualPatternMatches = Factory.SerialiseForTesting<OrgPatternMatch>(query, OrgPatternMatchSchema.OS_FullCompanyName.Name + ", " + OrgPatternMatchSchema.OS_BusinessRegNo.Name);
			AssertMultilineASCIIEquals("All Pattern Matches", expectedPatternMatches, actualPatternMatches);
		}

		public void TestConstructorChecksForNulls()
		{
			var mockDataManager = new Mock<IPatternMatchDataManager>();

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new PatternMatchReBuilder(null); });
			AssertNoExceptionThrown(delegate
			{ new PatternMatchReBuilder(mockDataManager.Object); });
		}
	}
}
