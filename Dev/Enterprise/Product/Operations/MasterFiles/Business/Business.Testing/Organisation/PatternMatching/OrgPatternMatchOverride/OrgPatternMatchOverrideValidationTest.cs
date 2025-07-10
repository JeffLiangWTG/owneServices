using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPatternMatchOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateLocalCode()
		{
			var packageType = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "PCE"));
			AssertNotNull("Precondition: PackageType", packageType);

			var match = Factory.New<OrgPatternMatchOverride>();
			match.OO_LocalCode = packageType.F3_Code;
			Assert("Local code should have no errors", !match.OO_LocalCodeInfo.HasErrors());

			match.OO_ForeignCode = "ABC";
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.PackageType;
			match.OO_LocalCode = string.Empty;
			AssertHasError(match.OO_LocalCodeInfo, "Please enter a Package Type.");

			match.OO_LocalCode = "~BLAH";
			AssertHasError(match.OO_LocalCodeInfo, "Enter a valid Package Type.");

			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
			match.OO_LocalCode = "~BLAH";
			AssertHasError(match.OO_LocalCodeInfo, "Enter a valid Incoterm.");

			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.EventCode;
			match.OO_LocalCode = "~BLAH";
			AssertHasError(match.OO_LocalCodeInfo, "Enter a valid Event Code.");

			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			match.OO_LocalCode = "~BLAH";
			AssertHasError(match.OO_LocalCodeInfo, "Enter a valid Charge Code.");

			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel;
			match.OO_LocalCode = "~BLAH";
			AssertHasError(match.OO_LocalCodeInfo, "Enter a valid Carrier Service Level.");
		}

		public void TestValidateLocalCodeAllowsAnyChargeCodeInAnyCompany()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode1.AC_Code = "AAA";

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_GC = ZGuid.NewZGuid();
			chargeCode2.AC_Code = "BBB";

			var match = Factory.New<OrgPatternMatchOverride>();
			match.OO_ForeignCode = "ABC";
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			match.OO_LocalCode = "AAA";
			AssertNoErrors("No error for code in current company", match.OO_LocalCodeInfo);
			AssertNoWarnings("No warning for code in current company", match.OO_LocalCodeInfo);

			match.OO_LocalCode = "BBB";
			AssertNoErrors("No error for code in other company", match.OO_LocalCodeInfo);
			AssertHasWarning("Warning for code in other company", match.OO_LocalCodeInfo, "This Charge Code does not belong to current company.");

			match.OO_LocalCode = "CCC";
			AssertHasError("Error for non-existing anywhere code", match.OO_LocalCodeInfo, "Enter a valid Charge Code.");
			AssertNoWarnings("No warning when there is an error", match.OO_LocalCodeInfo);
		}

		public void TestValidateForeignCode()
		{
			OrgPatternMatchOverride match = Factory.New<OrgPatternMatchOverride>();
			match.OO_ForeignCode = "";
			Assert("Foreign code should have errors", match.OO_ForeignCodeInfo.HasErrors());

			match.OO_ForeignCode = "ABC";
			Assert("Foreign code should not have errors", !match.OO_ForeignCodeInfo.HasErrors());

			match.OO_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			match.OO_ForeignCode = "ABC";
			Assert("Foreign code should not have errors because Org is not a ComPay org", !match.OO_ForeignCodeInfo.HasErrors());

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsENettOrganisation(It.IsAny<ZGuid>())).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				match.OO_ForeignCode = "ABC";
			}
			Assert("Foreign code should now have some errors because Org is a ComPay org and the code is not on the list", match.OO_ForeignCodeInfo.HasErrors());
		}

		public void TestForeignCodeAndRelationshipAndLocalCodeAndContextIsUnique()
		{
			var localGuid1 = OrgHeader.New(Factory).PK.ToString();
			var localGuid2 = OrgHeader.New(Factory).PK.ToString();
			var testHeader = OrgHeader.New(Factory);
			var match1 = testHeader.CreatePatternMatchOverrideForTest();
			match1.OO_ForeignCode = "ABC";
			match1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			match1.OrgCoNameorGuid = localGuid1;
			match1.OO_Context = "CTA";

			var match2 = testHeader.CreatePatternMatchOverrideForTest();
			match2.OO_ForeignCode = "DEF";
			match2.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match2.OrgCoNameorGuid = localGuid2;
			match2.OO_Context = "CTB";

			AssertNoErrors("Match 1 is unique", match1.OO_ForeignCodeInfo);
			AssertNoErrors("Match 2 is unique", match2.OO_ForeignCodeInfo);

			match2.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			AssertNoErrors("Match 1 is unique", match1.OO_ForeignCodeInfo);
			AssertNoErrors("Match 2 is unique", match2.OO_ForeignCodeInfo);

			match2.OO_ForeignCode = "ABC";
			AssertHasErrorContaining(match2.OO_ForeignCodeInfo, "A code mapping already exists between the code");

			match2.OrgCoNameorGuid = localGuid1;
			AssertNoErrors("Match 2 is unique, due to different Context", match2.OO_ForeignCodeInfo);

			match2.OO_Context = "CTA";
			AssertHasErrorContaining(match2.OO_ForeignCodeInfo, "A code mapping already exists between the code");

			var testHeader2 = OrgHeader.New(Factory);
			var match3 = testHeader2.CreatePatternMatchOverrideForTest();
			match3.OO_ForeignCode = "ABC";
			match3.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			match3.OrgCoNameorGuid = localGuid1;
			match3.OO_Context = "CTA";
			AssertNoErrors("No errors (Match3 is on a different org)", match3.OO_ForeignCodeInfo);
		}

		public void TestForeignCodeUniqueForOceanCarrierMessage()
		{
			var testHeader = OrgHeader.New(Factory);
			var match1 = testHeader.CreatePatternMatchOverrideForTest();
			match1.OO_ForeignCode = "ABC";
			match1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match1.OO_Context = Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;

			var match2 = testHeader.CreatePatternMatchOverrideForTest();
			match2.OO_ForeignCode = "DEF";
			match2.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match2.OO_Context = Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;

			AssertNoErrors("Match 1 is unique for foreign code", match1.OO_ForeignCodeInfo);
			AssertNoErrors("Match 2 is unique for foreign code", match2.OO_ForeignCodeInfo);

			match2.OO_ForeignCode = "ABC";
			AssertHasErrorContaining(match2.OO_ForeignCodeInfo, "A mapping already exists for foreign port code ABC with the context OCM.");
		}

		public void TestLocalGuidForOceanCarrierMessage()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var cnshaUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CNSHA");

			var testHeader = OrgHeader.New(Factory);
			var match1 = testHeader.CreatePatternMatchOverrideForTest();
			match1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match1.OO_Context = Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			match1.OO_LocalGuid = sydneyUNLOCO.PK;

			var match2 = testHeader.CreatePatternMatchOverrideForTest();
			match2.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match2.OO_Context = Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			match2.OO_LocalGuid = cnshaUNLOCO.PK;

			AssertNoErrors("Match 1 is unique for local code", match1.OO_LocalGuidInfo);
			AssertNoErrors("Match 2 is unique for local code", match2.OO_LocalGuidInfo);

			match2.OO_LocalGuid = sydneyUNLOCO.PK;
			AssertHasErrorContaining(match2.OO_LocalGuidInfo, "A mapping already exists for local port code AUSYD with the context OCM.");
		}

		public void TestValidateLocalCodeAndGuid()
		{
			OrgPatternMatchOverride match = Factory.New<OrgPatternMatchOverride>();

			FieldInfo[] fields = typeof(Constants.OrgPatternMatchOverrideRelationships).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				match.OO_Relationship = (string)field.GetValue(null);
				Assert(String.Format("{0}: should have error because it is a code relationship", match.OO_Relationship), match.IsNotCode || match.OO_LocalCodeInfo.HasErrors());
				Assert(String.Format("{0}: should have error because it is a guid relationship", match.OO_Relationship), match.IsNotGuid || match.OO_LocalGuidInfo.HasErrors());
			}
		}

		public void TestCheckOO_Relationship()
		{
			var match = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;

			AssertNoErrors("Has a valid code", match.OO_RelationshipInfo);

			match.OO_Relationship = "XXX";

			AssertHasErrors("Has a valid code", match.OO_RelationshipInfo);

			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;

			AssertNoErrors("Has a valid code", match.OO_RelationshipInfo);

			match.OO_Relationship = ZString.Empty;

			AssertNoErrors("Empty is fine", match.OO_RelationshipInfo);
		}

		public void TestCheckOO_Context()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var match = parent.CreatePatternMatchOverrideForTest();
			var match2 = parent.CreatePatternMatchOverrideForTest();

			CombineAssertions(() =>
			{
				AssertNoErrors("Precondition - Context can be empty by default", match.OO_ContextInfo);
				AssertNoErrors("Precondition - Context can be empty by default", match2.OO_ContextInfo);
			});

			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("AAA");
			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				match.OO_Context = "AAA";
				match2.OO_Context = "AAA";

				CombineAssertions(() =>
				{
					AssertNoErrors("Valid code", match.OO_ContextInfo);
					AssertNoErrors("Should be able to have two of the same contexts in the collection wiithout errors", match2.OO_ContextInfo);
				});

				match.OO_Context = "AZA";

				AssertHasError(match.OO_ContextInfo, "Enter a valid Context.");
			}
		}
	}
}
