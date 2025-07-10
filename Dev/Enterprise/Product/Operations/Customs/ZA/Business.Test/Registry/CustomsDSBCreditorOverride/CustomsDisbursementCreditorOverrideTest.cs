using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(CustomsDSBCreditorOverride))]
	sealed class CustomsDisbursementCreditorOverrideTest : RegistryBusinessObjectTemplateTestCase<CustomsDSBCreditorOverride>
	{
		public void TestValidateDistrictOfficeCode()
		{
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			Factory.Save();
			var coll = new CustomsDSBCreditorOverrideCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.DistrictOfficeCode = ZString.Empty;
			AssertHasErrorContaining(mapping.DistrictOfficeCodeInfo, MandatoryValidation.MustBeEntered);
			mapping.DistrictOfficeCode = "~~~";
			AssertNoErrorContaining(mapping.DistrictOfficeCodeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(mapping.DistrictOfficeCodeInfo, ListValidation.InvalidCodeError);
			mapping.DistrictOfficeCode = "BFN";
			AssertNoErrorContaining(mapping.DistrictOfficeCodeInfo, ListValidation.InvalidCodeError);
			mapping = coll.AddNew();
			mapping.DistrictOfficeCode = "BFN";
			AssertHasError(mapping.DistrictOfficeCodeInfo, CustomsDSBCreditorOverride.DuplicateDistrictOfficeCode);
			mapping.DistrictOfficeCode = "BBR";
			AssertNoError(mapping.DistrictOfficeCodeInfo, CustomsDSBCreditorOverride.DuplicateDistrictOfficeCode);
		}

		public void TestValidateCreditorPK()
		{
			var orgheader = Factory.NewWithValidTestData<OrgHeader>();
			orgheader.CompanyData.OB_IsCreditor = true;
			var orgheader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader2.CompanyData.OB_IsCreditor = false;
			Factory.Save();
			var coll = new CustomsDSBCreditorOverrideCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.CreditorPK = ZGuid.Empty;
			AssertHasErrorContaining(mapping.CreditorPKInfo, MandatoryValidation.MustBeEntered);
			mapping.CreditorPK = orgheader2.PK;
			AssertNoErrorContaining(mapping.CreditorPKInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(mapping.CreditorPKInfo, ListValidation.InvalidCodeError);
			mapping.CreditorPK = orgheader.PK;
			AssertNoErrorContaining(mapping.CreditorPKInfo, ListValidation.InvalidCodeError);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new CustomsDSBCreditorOverrideCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override CustomsDSBCreditorOverride GetBusinessObjectToClone()
		{
			return (CustomsDSBCreditorOverride)GetNewBusinessObject();
		}

		protected override CustomsDSBCreditorOverride GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
