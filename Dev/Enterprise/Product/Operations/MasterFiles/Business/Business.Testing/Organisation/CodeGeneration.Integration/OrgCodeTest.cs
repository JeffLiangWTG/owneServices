using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgCodeTest : TestCaseWithFactory
	{
		OrgHeader organization;

		OrgHeader Organization
		{
			get { return organization ?? (organization = Factory.New<OrgHeader>()); }
		}

		void AssertHasChanged(IOrgCode orgCode, params string[] codesWithNoChanges)
		{
			string[] codes =
			{
				"",
				"ABC_ZZ",
				"ABCXYZ",
				"ABCXYZ123",
				"ABCXYZ0123",
				"ABCXYZ1234",
				"ABCXYZ_YY",
				"ABCXYZ_ZZ",
				"ABCXYZ123_ZZ",
				"ABCXYZ_ZZ2",
				"ABC/XYZ",
				"ABC/XYZ2",
				"ABCDEFG_GLB1",
				"ABCDEFG_GLB",
				"ABCDEFGH_GLB",
				"QWERTYBOB",
				"QWERTYBOB000",
				"QWERTYBOB001",
				"QWERTYBOB1",
				"QWERTYBOB123",
				"QWERTYBO1234"
			};

			foreach (string code in codes)
			{
				AssertEquals("HasChanged(\"" + code + "\")", Array.IndexOf(codesWithNoChanges, code) == -1, orgCode.HasChanged(code));
			}
		}

		OrgCode OrgCode(OrgHeader info, string header, string tail, IOrgCodeElement uniqueNumberElement)
		{
			return new OrgCode(new OrgHeaderOrgCodeInfo(info), header, tail, uniqueNumberElement, 12, new OrgCodeDbProxy(Factory));
		}

		public void TestGetProposedAndFinalCode_WithEmptyValues()
		{
			OrgCode code = OrgCode(null, "", "", null);
			AssertEquals("GetProposedCode()", "", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "", code.GetFinalCode());

			code = OrgCode(null, null, null, null);
			AssertEquals("GetProposedCode()", "", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "", code.GetFinalCode());
		}

		public void TestGetCode_WithoutIndexError()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);
			var orgHeader1000 = Factory.New<OrgHeader>();
			orgHeader1000.OH_Code = "0001000000";
			Factory.Save();

			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 1000);

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			try
			{
				OrgCode code = OrgCode(Organization, "000", "000", globallyUniqueNumberElement);

				AssertNotNullOrEmpty(code.GetProposedCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestEditingExistingOrgWithTail()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "BCR Freight";
			org1.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL", org1.OH_Code);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "BCR Freight";
			org2.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL1", org2.OH_Code);

			var org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = "BCR Freight";
			org3.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL2", org3.OH_Code);

			org2.OH_FullName = "BCR Freighty";
			Factory.Save();
			AssertEquals("BCRFREMEL1", org2.OH_Code);

			org1.Delete();
			Factory.Save();

			org2.GenerateProposedCode();

			AssertEquals("BCRFREMEL", org2.OH_Code);
			Factory.Save();

			org3.GenerateProposedCode();
			AssertEquals("BCRFREMEL1", org3.OH_Code);
		}

		public void TestLowestAvailableValueIsTaken()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "BCR Freight";
			org1.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL", org1.OH_Code);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "BCR Freight";
			org2.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL1", org2.OH_Code);

			var org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = "BCR Freight";
			org3.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL2", org3.OH_Code);

			var org4 = Factory.New<OrgHeader>();
			org4.OH_FullName = "BCR Freight";
			org4.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL3", org4.OH_Code);

			var org5 = Factory.New<OrgHeader>();
			org5.OH_FullName = "BCR Freight";
			org5.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			AssertEquals("BCRFREMEL4", org5.OH_Code);

			var anotherOrg1 = Factory.New<OrgHeader>();
			anotherOrg1.OH_FullName = "XYZ Cargo";
			anotherOrg1.OH_RL_NKClosestPort = "UAIEV";
			Factory.Save();
			AssertEquals("XYZCARIEV", anotherOrg1.OH_Code);

			var anotherOrg2 = Factory.New<OrgHeader>();
			anotherOrg2.OH_FullName = "XYZ Cargo";
			anotherOrg2.OH_RL_NKClosestPort = "UAIEV";
			Factory.Save();
			AssertEquals("XYZCARIEV1", anotherOrg2.OH_Code);

			var anotherOrg3 = Factory.New<OrgHeader>();
			anotherOrg3.OH_FullName = "XYZ Cargo";
			anotherOrg3.OH_RL_NKClosestPort = "UAIEV";
			Factory.Save();
			AssertEquals("XYZCARIEV2", anotherOrg3.OH_Code);

			org3.Delete();
			org4.Delete();
			Factory.Save();
			org5.GenerateProposedCode();
			AssertEquals("BCRFREMEL2", org5.OH_Code);

			anotherOrg1.Delete();
			Factory.Save();
			anotherOrg3.GenerateProposedCode();
			AssertEquals("XYZCARIEV", anotherOrg3.OH_Code);
		}

		public void TestGetProposedAndFinalCode_WithNoTailOrUniqueNumber()
		{
			Organization.OH_Code = "BOBJANE";
			OrgCode code = OrgCode(Organization, "BOBJANE", null, null);
			AssertEquals("GetProposedCode()", "BOBJANE", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "BOBJANE", code.GetFinalCode());

			OrgHeader anotherOrganization = Factory.New<OrgHeader>();
			code = OrgCode(anotherOrganization, "BOBJANE", null, null);
			AssertEquals("GetProposedCode()", "BOBJANE1", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "BOBJANE1", code.GetFinalCode());
			anotherOrganization.OH_Code = "BOBJANE1";

			OrgHeader yetAnotherOrganization = Factory.New<OrgHeader>();
			code = OrgCode(yetAnotherOrganization, "BOBJANE", null, null);
			AssertEquals("GetProposedCode()", "BOBJANE2", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "BOBJANE2", code.GetFinalCode());
		}

		public void TestGetProposedAndFinalCode_WithTail()
		{
			Organization.OH_Code = "ABCDEFGH_XYZ";
			OrgCode code = OrgCode(Organization, "ABCDEFGH", "_XYZ", null);
			AssertEquals("GetProposedCode()", "ABCDEFGH_XYZ", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "ABCDEFGH_XYZ", code.GetFinalCode());

			OrgHeader anotherOrganization = Factory.New<OrgHeader>();
			code = OrgCode(anotherOrganization, "ABCDEFGH", "_XYZ", null);
			AssertEquals("GetProposedCode()", "ABCDEFG_XYZ1", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "ABCDEFG_XYZ1", code.GetFinalCode());

			anotherOrganization.OH_Code = "ABCDEFG_XYZ1";
			OrgHeader yetAnotherOrganization = Factory.New<OrgHeader>();
			code = OrgCode(yetAnotherOrganization, "ABCDEFGH", "_XYZ", null);
			AssertEquals("GetProposedCode()", "ABCDEFG_XYZ2", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "ABCDEFG_XYZ2", code.GetFinalCode());

			code = OrgCode(yetAnotherOrganization, "QRSTUVWABC", "_XYZ", null);
			AssertEquals("GetProposedCode()", "QRSTUVWA_XYZ", code.GetProposedCode());
			AssertEquals("GetFinalCode()", "QRSTUVWA_XYZ", code.GetFinalCode());
		}

		public void TestPaddingOfGloballyUniqueNumber()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 6);
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 1000);
			var code = OrgCode(Organization, "", null, globallyUniqueNumberElement);
			AssertEquals("code.GetProposedCode()", "001000", code.GetProposedCode());
		}

		public void TestRetrieveWithSpecificUniqueNumberWith8DigitsResolvesToMaxValuePlusOne()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 8);
			var orgHeader1000 = Factory.New<OrgHeader>();
			orgHeader1000.OH_Code = "00001000";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "12345678";
			Factory.Save();
			Assert(orgHeader.OH_Code == "12345678");    // sanity check
			Assert(orgHeader1000.OH_Code == "00001000");    // sanity check

			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 1000);

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			try
			{
				OrgCode code = OrgCode(Organization, "", null, globallyUniqueNumberElement);
				AssertEquals("code.GetProposedCode()", "12345679", code.GetProposedCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestRetrieveWithSpecificUniqueNumberWith3DigitsResolvesToMaxValuePlusOne()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 3);
			var orgHeader1000 = Factory.New<OrgHeader>();
			orgHeader1000.OH_Code = "350";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "12345678";
			Factory.Save();
			Assert(orgHeader.OH_Code == "12345678");    // sanity check
			Assert(orgHeader1000.OH_Code == "350"); // sanity check

			OrgCode code = OrgCode(Organization, "", null, globallyUniqueNumberElement);
			AssertEquals("code.GetProposedCode()", "351", code.GetProposedCode());
		}

		public void TestRetrieveWithSpecificUniqueNumberWithCharactersAndDigitsResolvesToMaxValuePlusOne()
		{
			var globallySpecificNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 3);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "org350";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "org666";
			Factory.Save();
			Assert(orgHeader1.OH_Code == "org350");    // sanity check
			Assert(orgHeader2.OH_Code == "org666"); // sanity check

			var code = OrgCode(Organization, "org", null, globallySpecificNumberElement);
			AssertEquals("code.GetProposedCode()", "org667", code.GetProposedCode());
		}

		public void TestRetrieveWithGloballyUniqueNumberWith8DigitsResolvesToNextUnusedValue()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 8);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "67000001";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "67000002";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "67000003";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "67000004";
			var orgHeader5 = Factory.New<OrgHeader>();
			orgHeader5.OH_Code = "67000010";
			var orgHeader6 = Factory.New<OrgHeader>();
			orgHeader6.OH_Code = "67000011";
			Factory.Save();
			Assert(orgHeader1.OH_Code == "67000001");
			Assert(orgHeader2.OH_Code == "67000002");
			Assert(orgHeader3.OH_Code == "67000003");
			Assert(orgHeader4.OH_Code == "67000004");
			Assert(orgHeader5.OH_Code == "67000010");
			Assert(orgHeader6.OH_Code == "67000011");

			AssertCodeIsCorrectWhenUsingNumberFountain(67000001, "67000005", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000002, "67000005", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000003, "67000005", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000004, "67000005", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000005, "67000005", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000006, "67000006", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000007, "67000007", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000008, "67000008", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000009, "67000009", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000010, "67000012", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(67000011, "67000012", globallyUniqueNumberElement);
		}

		public void TestRetrieveWithGloballyUniqueNumberWith3DigitsResolvesToNextUnusedValue()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "100";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "101";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "102";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "104";
			var orgHeader5 = Factory.New<OrgHeader>();
			orgHeader5.OH_Code = "106";
			var orgHeader6 = Factory.New<OrgHeader>();
			orgHeader6.OH_Code = "108";
			Factory.Save();
			Assert(orgHeader1.OH_Code == "100");
			Assert(orgHeader2.OH_Code == "101");
			Assert(orgHeader3.OH_Code == "102");
			Assert(orgHeader4.OH_Code == "104");
			Assert(orgHeader5.OH_Code == "106");
			Assert(orgHeader6.OH_Code == "108");

			AssertCodeIsCorrectWhenUsingNumberFountain(100, "103", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(101, "103", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(102, "103", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(103, "103", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(104, "105", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(105, "105", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(106, "107", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(107, "107", globallyUniqueNumberElement);
			AssertCodeIsCorrectWhenUsingNumberFountain(108, "109", globallyUniqueNumberElement);
		}

		public void TestRetrieveWithGloballyUniqueNumberWithCharactersAndDigitsResolvesToNextUnusedValue()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "aa001";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "aa002";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "aa005";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "bb001";
			var orgHeader5 = Factory.New<OrgHeader>();
			orgHeader5.OH_Code = "bb002";
			var orgHeader6 = Factory.New<OrgHeader>();
			orgHeader6.OH_Code = "bb003";
			var orgHeader7 = Factory.New<OrgHeader>();
			orgHeader7.OH_Code = "bb006";
			Factory.Save();
			Assert(orgHeader1.OH_Code == "aa001");
			Assert(orgHeader2.OH_Code == "aa002");
			Assert(orgHeader3.OH_Code == "aa005");
			Assert(orgHeader4.OH_Code == "bb001");
			Assert(orgHeader5.OH_Code == "bb002");
			Assert(orgHeader6.OH_Code == "bb003");
			Assert(orgHeader7.OH_Code == "bb006");

			AssertCodeIsCorrectWhenUsingNumberFountain(1, "aa003", globallyUniqueNumberElement, "aa");
			AssertCodeIsCorrectWhenUsingNumberFountain(2, "aa003", globallyUniqueNumberElement, "aa");
			AssertCodeIsCorrectWhenUsingNumberFountain(3, "aa003", globallyUniqueNumberElement, "aa");
			AssertCodeIsCorrectWhenUsingNumberFountain(4, "aa004", globallyUniqueNumberElement, "aa");
			AssertCodeIsCorrectWhenUsingNumberFountain(5, "aa006", globallyUniqueNumberElement, "aa");

			AssertCodeIsCorrectWhenUsingNumberFountain(1, "bb004", globallyUniqueNumberElement, "bb");
			AssertCodeIsCorrectWhenUsingNumberFountain(2, "bb004", globallyUniqueNumberElement, "bb");
			AssertCodeIsCorrectWhenUsingNumberFountain(3, "bb004", globallyUniqueNumberElement, "bb");
			AssertCodeIsCorrectWhenUsingNumberFountain(4, "bb004", globallyUniqueNumberElement, "bb");
			AssertCodeIsCorrectWhenUsingNumberFountain(5, "bb005", globallyUniqueNumberElement, "bb");
			AssertCodeIsCorrectWhenUsingNumberFountain(6, "bb007", globallyUniqueNumberElement, "bb");
		}

		public void TestNotResetNumberFountainWhenCurrentValueIsUsedInCacheProposedCode()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "100";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "102";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "104";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "900";

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 100);

			try
			{
				var code1 = OrgCode(orgHeader1, "", null, globallyUniqueNumberElement);
				orgHeader1.OH_Code = code1.GetProposedCode();
				AssertEquals("code.GetProposedCode()", "100", orgHeader1.OH_Code);
				var code2 = OrgCode(orgHeader2, "", null, globallyUniqueNumberElement);
				orgHeader2.OH_Code = code2.GetProposedCode();
				AssertEquals("code.GetProposedCode()", "101", orgHeader2.OH_Code);
				var code3 = OrgCode(orgHeader3, "", null, globallyUniqueNumberElement);
				orgHeader3.OH_Code = code3.GetProposedCode();
				AssertEquals("code.GetProposedCode()", "102", orgHeader3.OH_Code);
				var code4 = OrgCode(orgHeader4, "", null, globallyUniqueNumberElement);
				orgHeader4.OH_Code = code4.GetProposedCode();
				AssertEquals("code.GetProposedCode()", "103", orgHeader4.OH_Code);

				AssertEquals("NumberFountain Should not be reset", 100, Env.NumberFountains.OrgCodeNumberFountain.PeekPreliminary(Db.Connection));
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestResetNumberFountainWhenCurrentValueIsUsedNotInCacheFinalCode()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "100";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "102";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "104";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "900";
			Factory.Save();

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 100);
			try
			{
				var code = OrgCode(Organization, "", null, globallyUniqueNumberElement);
				AssertEquals("code.GetFinalCode()", "101", code.GetFinalCode());
				AssertEquals("NumberFountain Should be reset", 102, Env.NumberFountains.OrgCodeNumberFountain.PeekPreliminary(Db.Connection));
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestNotResetNumberFountainWhenCurrentValueIsUsedInCacheFinalCode()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "100";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "101";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "102";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "900";

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 100);
			try
			{
				var code = OrgCode(Organization, "", null, globallyUniqueNumberElement);
				AssertEquals("code.GetFinalCode()", "103", code.GetFinalCode());
				AssertEquals("NumberFountain Should not be reset", 101, Env.NumberFountains.OrgCodeNumberFountain.PeekPreliminary(Db.Connection));
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestShouldUseCorrectValueWith8DigitsWhenConflictInCacheButNotInDb()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 8);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "00000001";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "90000000";
			Factory.Save();

			var orgHeaderInCache = Factory.New<OrgHeader>();
			orgHeaderInCache.OH_Code = "00000002";

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 2);
			try
			{
				var code = OrgCode(Organization, "", null, globallyUniqueNumberElement);
				AssertEquals("Should use next unused value", "00000003", code.GetProposedCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestShouldUseCorrectValueWithDigitsMixedCharactersWhenConflictInCacheButNotInDb()
		{
			var globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TES001";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TES900";
			Factory.Save();

			var orgHeaderInCache = Factory.New<OrgHeader>();
			orgHeaderInCache.OH_Code = "TES002";

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 2);
			try
			{
				var code = OrgCode(Organization, "TES", null, globallyUniqueNumberElement);
				AssertEquals("Should use next unused value", "TES003", code.GetProposedCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestResetNumberFountainWhenGenerateCodeAndSingleSaveWithFactory()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "100";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "103";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "104";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "900";
			Factory.Save();

			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 100);

			try
			{
				var newOrgHeader = Factory.New<OrgHeader>();
				newOrgHeader.OH_FullName = "Test Company One";
				newOrgHeader.OH_RL_NKClosestPort = "ADALV";
				Factory.Save();

				AssertEquals("101", newOrgHeader.OH_Code);
				AssertEquals("NumberFountain Should be reset",102, Env.NumberFountains.OrgCodeNumberFountain.PeekPreliminary(Db.Connection));
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestResetNumberFountainWhenGenerateCodeAndBatchSaveWithFactory()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "100";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "103";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "104";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "900";
			Factory.Save();

			var algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, 100);

			try
			{
				var newOrgHeader1 = Factory.New<OrgHeader>();
				newOrgHeader1.OH_FullName = "Test Company One";
				newOrgHeader1.OH_RL_NKClosestPort = "ADALV";

				var newOrgHeader2 = Factory.New<OrgHeader>();
				newOrgHeader2.OH_FullName = "Test Company Two";
				newOrgHeader2.OH_RL_NKClosestPort = "ADALV";

				var newOrgHeader3 = Factory.New<OrgHeader>();
				newOrgHeader3.OH_FullName = "Test Company Three";
				newOrgHeader3.OH_RL_NKClosestPort = "ADALV";

				Factory.Save();

				AssertEquals("101", newOrgHeader1.OH_Code);
				AssertEquals("102", newOrgHeader2.OH_Code);
				AssertEquals("105", newOrgHeader3.OH_Code);
				AssertEquals("NumberFountain Should be reset", 106, Env.NumberFountains.OrgCodeNumberFountain.PeekPreliminary(Db.Connection));
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestGetProposedAndFinalCode_WithUniqueNumber()
		{
			OrgCodeElement codeSpecificUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 5);
			OrgCodeElement globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 3);

			OrgCode code = OrgCode(Organization, "GOOBER", null, codeSpecificUniqueNumberElement);
			OrgHeader anotherOrganization = Factory.New<OrgHeader>();
			OrgCode anotherCode = OrgCode(anotherOrganization, "GOOBER", null, codeSpecificUniqueNumberElement);

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.

			try
			{
				AssertEquals("code.GetProposedCode()", "GOOBER00001", code.GetProposedCode());
				AssertEquals("anotherCode.GetProposedCode()", "GOOBER00001", anotherCode.GetProposedCode());
				AssertEquals("code.GetFinalCode()", "GOOBER00001", code.GetFinalCode());
				AssertEquals("No Org saved - anotherCode.GetFinalCode()", "GOOBER00001", anotherCode.GetFinalCode());

				code = OrgCode(Organization, "GOOBERX", null, codeSpecificUniqueNumberElement);
				AssertEquals("code.GetProposedCode()", "GOOBERX00001", code.GetProposedCode());
				AssertEquals("code.GetFinalCode()", "GOOBERX00001", code.GetFinalCode());

				Organization.OH_Code = "GOOBER0001";
				anotherOrganization.OH_Code = "GOOBER10000";
				Factory.Save();

				code = OrgCode(Organization, "GOOBER", null, codeSpecificUniqueNumberElement);
				AssertEquals("code.GetProposedCode()", "GOOBER10001", code.GetProposedCode());
				AssertEquals("code.GetFinalCode()", "GOOBER10001", code.GetFinalCode());

				Env.NumberFountains.OrgCodeNumberFountain.SetNext(Factory, 1);
				code = OrgCode(Organization, "GOOBER", null, globallyUniqueNumberElement);
				anotherCode = OrgCode(anotherOrganization, "GOOBERY", null, globallyUniqueNumberElement);
				AssertEquals("code.GetProposedCode()", "GOOBER001", code.GetProposedCode());
				AssertEquals("anotherCode.GetProposedCode()", "GOOBERY001", anotherCode.GetProposedCode());
				AssertEquals("code.GetFinalCode()", "GOOBER001", code.GetFinalCode());
				AssertEquals("anotherCode.GetFinalCode()", "GOOBERY002", anotherCode.GetFinalCode());

				Env.NumberFountains.OrgCodeNumberFountain.SetNext(Factory, 1000);
				AssertEquals("code.GetProposedCode()", "GOOBER1000", code.GetProposedCode());
				AssertEquals("anotherCode.GetProposedCode()", "GOOBERY1000", anotherCode.GetProposedCode());
				AssertEquals("code.GetFinalCode()", "GOOBER1000", code.GetFinalCode());
				AssertEquals("anotherCode.GetFinalCode()", "GOOBERY1001", anotherCode.GetFinalCode());

				code = OrgCode(Organization, null, null, globallyUniqueNumberElement);
				AssertEquals("code.GetProposedCode()", "1002", code.GetProposedCode());
				AssertEquals("code.GetFinalCode()", "1002", code.GetFinalCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		public void TestGetProposedAndFinalCode_WithUniqueNumberAndNumericHeader()
		{
			OrgCodeElement codeSpecificUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 9);

			OrgCode code = OrgCode(Organization, "888", null, codeSpecificUniqueNumberElement);

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.

			try
			{
				using (DbCommand cmd = Db.Connection.Command("insert into dbo.orgheader (oh_pk, oh_code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values (newid(), '888000000001', GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
				{
					cmd.ExecuteNonQuery();
				}

				AssertEquals("anotherCode.GetFinalCode()", "888000000002", code.GetFinalCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}

			Organization.OH_IsNationalAccount = true;
			code = OrgCode(Organization, "888", "_AU", codeSpecificUniqueNumberElement);

			AssertEquals("Should work with national account.", "000000002_AU", code.GetFinalCode());
		}

		public void TestGetProposedAndFinalCode_WithUniqueNumberAndNeedToSubstractCharacter()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.RegenerateOrgCodeOnChanges = true;
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 2;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 8;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "OCEAN WORLD LINES, INC. - MIAMI";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.OH_IsNationalAccount = true;
			Factory.Save();
			AssertEquals("O00000001_AU", header1.OH_Code);

			Env.Registry.CanUserEditOrganisationCode = true;
			OrgHeader header2 = Factory.New<OrgHeader>();
			header2.OH_FullName = "ABC DEF ASD, INC. - MIAMI";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.OH_IsNationalAccount = true;
			Factory.Save();
			AssertEquals("A00000001_AU", header2.OH_Code);

			header2.OH_FullName = "BCD DEF ASD, INC. - MIAMI";
			header2.OH_IsNationalAccount = false;
			AssertEquals("BC00000001", header2.OH_Code);

			header2.OH_IsNationalAccount = true;
			AssertEquals("B00000001_AU", header2.OH_Code);

			OrgHeader header3 = Factory.New<OrgHeader>();
			header3.OH_FullName = "OCEAN WORLD LINES, INC2. - MIAMI";
			header3.OH_RL_NKClosestPort = "AUSYD";
			header3.OH_IsNationalAccount = true;
			Factory.Save();
			AssertEquals("O00000002_AU", header3.OH_Code);

			OrgHeader header4 = Factory.New<OrgHeader>();
			header4.OH_FullName = "00 WORLD LINES, INC2. - MIAMI";
			header4.OH_RL_NKClosestPort = "AUSYD";
			header4.OH_IsNationalAccount = true;
			Factory.Save();
			AssertEquals("000000001_AU", header4.OH_Code);

			OrgHeader header5 = Factory.New<OrgHeader>();
			header5.OH_FullName = "0A WORLD LINES, INC2. - MIAMI";
			header5.OH_RL_NKClosestPort = "AUSYD";
			header5.OH_IsNationalAccount = true;
			Factory.Save();
			AssertEquals("000000002_AU", header5.OH_Code);

			OrgHeader header6 = Factory.New<OrgHeader>();
			header6.OH_FullName = "A0 WORLD LINES, INC2. - MIAMI";
			header6.OH_RL_NKClosestPort = "AUSYD";
			header6.OH_IsNationalAccount = true;
			Factory.Save();
			AssertEquals("A00000001_AU", header6.OH_Code);
		}

		public void TestGetProposedAndFinalCode_WithUniqueNumberAndShouldNotAddCountToTail()
		{
			OrgCodeElement codeSpecificUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 5);

			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgCode code1 = OrgCode(orgHeader1, "GOOBER", null, codeSpecificUniqueNumberElement);
			orgHeader1.OH_Code = code1.GetProposedCode();

			OrgHeader orgHeader2 = Factory.New<OrgHeader>();
			OrgCode code2 = OrgCode(orgHeader2, "GOOBER", null, codeSpecificUniqueNumberElement);
			orgHeader2.OH_Code = code2.GetProposedCode();

			OrgHeader orgHeader3 = Factory.New<OrgHeader>();
			OrgCode code3 = OrgCode(orgHeader3, "GOOBER", null, codeSpecificUniqueNumberElement);
			orgHeader3.OH_Code = code3.GetProposedCode();

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.

			try
			{
				AssertEquals("orgHeader1.OH_code", "GOOBER00001", orgHeader1.OH_Code);
				AssertEquals("orgHeader2.OH_code", "GOOBER00002", orgHeader2.OH_Code);
				AssertEquals("orgHeader3.OH_code", "GOOBER00003", orgHeader3.OH_Code);

				AssertEquals("orgHeader1.OH_code", "GOOBER00001", code1.GetFinalCode());
				AssertEquals("orgHeader2.OH_code", "GOOBER00002", code2.GetFinalCode());
				AssertEquals("orgHeader3.OH_code", "GOOBER00003", code3.GetFinalCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		#region TestHasChanged

		public void TestHasChanged()
		{
			OrgCode code = OrgCode(Organization, "", "", null);
			AssertHasChanged(code, "");

			code = OrgCode(Organization, "ABCXYZ", "_ZZ", null);
			AssertHasChanged(code, "ABCXYZ_ZZ");

			code = OrgCode(Organization, "ABCXYZ1234", null, null);
			AssertHasChanged(code, "ABCXYZ", "ABCXYZ123", "ABCXYZ0123", "ABCXYZ1234");

			code = OrgCode(Organization, "ABC/XYZ", null, null);
			AssertHasChanged(code, "ABC/XYZ");

			code = OrgCode(Organization, "ABCDEFGHIJKL", "_GLB1", null);
			AssertHasChanged(code, "ABCDEFG_GLB1", "ABCDEFG_GLB");
		}

		[ExpectNoExceptions]
		public void TestHasChanged_EditableCodeIsOnlyNumbersWithGloballyUniqueNumbers()
		{
			Env.Registry.CanUserEditOrganisationCode = true;
			OrgCodeElement globallyUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.GloballyUniqueNumber, 4);
			var dbProxy = new OrgCodeDbProxy(Factory);

			dbProxy.NumberFountainSetNext(10);
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgCode code1 = OrgCode(org1, "", "", globallyUniqueNumberElement);
			org1.OH_Code = "0009";
			AssertEquals("Code edited by the user has changed. HasChanged assumes all numbers before 10 have been used", false, code1.HasChanged(org1.OH_Code));

			dbProxy.NumberFountainSetNext(98765);
			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgCode code2 = new OrgCode(new OrgHeaderOrgCodeInfo(org2), "", "", globallyUniqueNumberElement, 12, dbProxy);
			org2.OH_Code = code2.GetProposedCode();
			AssertEquals("Code that was generated has changed", false, code2.HasChanged(org2.OH_Code));

			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "64040";
			OrgCode code3 = new OrgCode(new OrgHeaderOrgCodeInfo(org3), "", "", globallyUniqueNumberElement, 12, dbProxy);
			AssertEquals("Code edited by the user has changed. HasChanged assumes all numbers before 98765 have been used", false, code3.HasChanged(org3.OH_Code));

			OrgHeader org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "640404";
			OrgCode code4 = new OrgCode(new OrgHeaderOrgCodeInfo(org4), "", "", globallyUniqueNumberElement, 12, dbProxy);
			AssertEquals("Code edited by the user has changed", true, code4.HasChanged(org4.OH_Code));

			OrgHeader org5 = Factory.New<OrgHeader>();
			org5.OH_Code = "6404040322"; // out of range of an int; actual OH_Code used by client
			OrgCode code5 = new OrgCode(new OrgHeaderOrgCodeInfo(org5), "", "", globallyUniqueNumberElement, 12, dbProxy);
			AssertEquals("Code edited by the user has changed with no exception", true, code5.HasChanged(org5.OH_Code));
		}

		public void TestHasChanged_EditableCodeIsOnlyNumbersWithCodeSpecificUniqueNumbers()
		{
			Env.Registry.CanUserEditOrganisationCode = true;
			OrgCodeElement uniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 4);
			var dbProxy = new OrgCodeDbProxy(Factory);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC0001";
			OrgCode code1 = OrgCode(org1, "XYZ", "", uniqueNumberElement);
			AssertEquals(true, code1.HasChanged(org1.OH_Code));

			code1 = OrgCode(org1, "ABC", "", uniqueNumberElement);
			AssertEquals(false, code1.HasChanged(org1.OH_Code));
		}

		[ExpectNoExceptions]
		public void TestHasChanged_WithCodeSpecificUniqueNumber_WhenDbProxyHasClosedInternalConnection()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgCodeElement uniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 4);
			var dbProxy = new OrgCodeDbProxy(Factory);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC0001";

			using (var command = dbProxy.CreateCommand())
			{
				var connection = command.Connection;
				connection.Close();

				AssertEquals("PreCondition: connection state", ConnectionState.Closed, connection.State);

				var code1 = new OrgCode(new OrgHeaderOrgCodeInfo(org1), "ABC", "", uniqueNumberElement, 12, dbProxy);
				code1.HasChanged(org1.OH_Code);
			}
		}

		#endregion

		public void TestHasUniqueNumber()
		{
			OrgCode code = OrgCode(null, "", "", null);
			AssertEquals("HasUniqueNumber", false, code.HasUniqueNumber);
			code = OrgCode(null, "", "", new OrgCodeElement());
			AssertEquals("HasUniqueNumber", true, code.HasUniqueNumber);
		}

		public void TestWhenCodeIsOnlyNumbers()
		{
			OrgHeader duplicateOrg = Factory.New<OrgHeader>();
			duplicateOrg.OH_FullName = "Test Duplicated Code Org";
			duplicateOrg.OH_Code = "1111100001";
			Factory.Save();

			OrgCodeElement codeSpecificUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 5);
			Organization.OH_Code = "11111";
			OrgCode code = OrgCode(Organization, "11111", "", codeSpecificUniqueNumberElement);
			AssertEquals("GetProposedCode()", "1111100002", code.GetProposedCode());
		}

		public void TestWhenThereAreExistingCodeTakenInFactoryDoesNotCauseInfiniteLoop()
		{
			OrgHeader duplicateOrgInDB = Factory.New<OrgHeader>();
			duplicateOrgInDB.OH_FullName = "Test Duplicated Code Org3";
			duplicateOrgInDB.OH_Code = "00001";
			Factory.Save();

			OrgHeader duplicateOrg = Factory.New<OrgHeader>();
			duplicateOrg.OH_FullName = "Test Duplicated Code Org1";
			duplicateOrg.OH_Code = "00002";

			OrgHeader duplicateOrg2 = Factory.New<OrgHeader>();
			duplicateOrg2.OH_FullName = "Test Duplicated Code Org2";
			duplicateOrg2.OH_Code = "00003";

			OrgCodeElement codeSpecificUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 5);

			Organization.OH_Code = "";
			OrgCode code = OrgCode(Organization, "", "", codeSpecificUniqueNumberElement);

			AssertEquals("GetProposedCode()", "00004", code.GetProposedCode());
		}

		#region GetNextTailNumberWithoutDuplicate

		public void TestGetNextTailNumberWithoutDuplicate()
		{
			OrgHeader header0 = Factory.New<OrgHeader>();
			header0.OH_Code = "abc";

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "abc1";

			OrgHeader header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "abc2.";

			OrgHeader header3 = Factory.New<OrgHeader>();
			header3.OH_Code = "abc3";

			Factory.Save();

			OrgCode code = OrgCode(Factory.New<OrgHeader>(), "abc", "", new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 1));

			AssertEquals("abc4", code.GetProposedCode());
		}

		public void TestGetNextTailNumberWithoutDuplicateNoUniqueNumber()
		{
			OrgHeader header0 = Factory.New<OrgHeader>();
			header0.OH_Code = "abc";

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "abc1";

			OrgHeader header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "abc2.";

			OrgHeader header3 = Factory.New<OrgHeader>();
			header3.OH_Code = "abc3";

			Factory.Save();

			OrgCode code = OrgCode(Factory.New<OrgHeader>(), "abc", "", null);

			AssertEquals("abc2", code.GetProposedCode());
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestNoInfiniteLoopWhenGeneratingOrgCodeWithTail()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 4;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Length = 4;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 4;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "OCEAWORLMIAM";

			OrgHeader header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "OCEAWORLMIA1";
			Factory.Save();

			OrgHeader header3 = Factory.New<OrgHeader>();
			header3.OH_FullName = "OCEAN WORLD LINES, INC. - MIAMI"; //Will cause code regen

			AssertEquals("OCEAWORLMIA2", header3.OH_Code);
		}

		public void TestElementsEndingInNumbersWithoutUniqueNumbers()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stormcloaks";
			org1.OH_RL_NKClosestPort = "JPMI3";
			Factory.Save();

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Stormcloaks";
			org2.OH_RL_NKClosestPort = "JPMI3";
			Factory.Save();

			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = "Stormcloaks";
			org3.OH_RL_NKClosestPort = "JPMI3";
			org3.OH_Code = "STORMCMI32";
			Factory.Save();
			org3 = Factory.Load<OrgHeader>(org3.PK); // Load() is called here because Save() can re-generate the code

			OrgHeader org4 = Factory.New<OrgHeader>();
			org4.OH_FullName = "Stormcloaks";
			org4.OH_RL_NKClosestPort = "JPMI3";
			Factory.Save();

			AssertEquals("STORMCMI3", org1.OH_Code);
			AssertEquals("STORMCMI31", org2.OH_Code);
			AssertEquals("STORMCMI32", org3.OH_Code);
			AssertEquals("STORMCMI33", org4.OH_Code);
		}

		public void TestElementsEndingInNumbersWithUniqueMaxNumbers()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgCodeAlgorithm oca = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			oca.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			oca.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oca);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stormcloaks";
			org1.OH_RL_NKClosestPort = "JPMI3";
			org1.OH_Code = "STORMCMI3000";
			Factory.Save();
			org1 = Factory.Load<OrgHeader>(org1.PK); // Load() is called here because Save() can re-generate the code

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Stormcloaks";
			org2.OH_RL_NKClosestPort = "JPMI3";
			org2.OH_Code = "STORMCMI3001";
			Factory.Save();
			org2 = Factory.Load<OrgHeader>(org2.PK);

			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = "Stormcloaks";
			org3.OH_RL_NKClosestPort = "JPMI3";
			org3.OH_Code = "STORMCMI3999";
			Factory.Save();
			org3 = Factory.Load<OrgHeader>(org3.PK);

			OrgHeader org4 = Factory.New<OrgHeader>();
			org4.OH_FullName = "Stormcloaks";
			org4.OH_RL_NKClosestPort = "JPMI3";
			Factory.Save();
			org4 = Factory.Load<OrgHeader>(org4.PK);

			AssertEquals("STORMCMI3000", org1.OH_Code);
			AssertEquals("STORMCMI3001", org2.OH_Code);
			AssertEquals("STORMCMI3999", org3.OH_Code);
			AssertEquals("STORMCMI1000", org4.OH_Code);
		}

		public void TestElementsEndingInNumbersWithUniqueNumbers()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stormcloaks";
			org1.OH_RL_NKClosestPort = "JPMI3";
			org1.OH_Code = "STORMCMI3001";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Stormcloaks";
			org2.OH_RL_NKClosestPort = "JPMI3";
			org2.OH_Code = "STORMCMI3002";

			OrgCodeElement codeSpecificUniqueNumberElement = new OrgCodeElement(OrgCodeElementDescription.CodeSpecificUniqueNumber, 3);
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = "Stormcloaks";
			org3.OH_RL_NKClosestPort = "JPMI3";
			OrgCode code = OrgCode(org3, "STORMCMI3", "", codeSpecificUniqueNumberElement);
			AssertEquals("STORMCMI3003", code.GetProposedCode());
		}

		public void TestDoesntGoInfiniteLoopWhenAllPossibleUniqueNumberAreTaken()
		{
			Factory.New<OrgHeader>().OH_Code = "CAREDIAUSYD";
			for (int i = 0; i < 10; i++)
			{
				Factory.New<OrgHeader>().OH_Code = "CAREDIAUSYD" + i;
			}
			Factory.Save();

			var algorithm = new OrgCodeAlgorithm { AlgorithmType = OrgCodeAlgorithmType.Default, RegenerateOrgCodeOnChanges = true };
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.UnlocoCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.UnlocoCode].Length = 5;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 4;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 1;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Factory, 1);

			var factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			header.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header.MainAddress.OA_City = "Alexandria";

			AssertNoExceptionThrown(() => factory.Save());
			AssertEquals("Algorithm in registry is ignored and code is generated disregarding unique number rule", "CAREDIAUSY10", header.OH_Code);
		}

		public void TestDoesntGoInfiniteLoopWhenAllPossibleUniqueNumberAreTaken_2()
		{
			var oldHeader = Factory.New<OrgHeader>();
			oldHeader.OH_Code = "MAR000001";
			oldHeader = Factory.New<OrgHeader>();
			oldHeader.OH_Code = "MAR999999_JP";
			Factory.Save();

			Env.Registry.CanUserEditOrganisationCode = false;

			var algorithm = new OrgCodeAlgorithm { AlgorithmType = OrgCodeAlgorithmType.Default, RegenerateOrgCodeOnChanges = true };
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 6;
			/*algorithm.Elements[OrgCodeElementDescription.CountryCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CountryCode].Length = 3;*/

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Factory, 1);

			var factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			header.OH_FullName = "MARgoWise edi Australia Pty Ltd";
			header.OH_RL_NKClosestPort = "JPTAJ";
			header.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header.MainAddress.OA_City = "Alexandria";
			factory.Save();
			Assert("Doesn't go infinite loop", true);
			AssertEquals("MAR000002", header.OH_Code);
		}

		#region Implementation

		void AssertCodeIsCorrectWhenUsingNumberFountain(long nextNumberFountainValue, string expectedCode, OrgCodeElement globallyUniqueNumberElement, string codeHeader = "")
		{
			Env.NumberFountains.OrgCodeNumberFountain.SetNext(Db.Connection, nextNumberFountainValue);

			Db.Connection.BeginTransaction(); // Number fountain testing requires transaction level 2.
			try
			{
				var code = OrgCode(Organization, codeHeader, null, globallyUniqueNumberElement);
				AssertEquals("code.GetProposedCode()", expectedCode, code.GetProposedCode());
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // Number fountain testing requires transaction level 2.
			}
		}

		#endregion
	}
}
