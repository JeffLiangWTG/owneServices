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
	[TestedType(typeof(FinancialAccountNumberPortMap))]
	sealed class FinancialAccountNumberPortMapTest : RegistryBusinessObjectTemplateTestCase<FinancialAccountNumberPortMap>
	{
		public void TestVatDefermentAmount()
		{
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			AssertEquals(FinancialAccountNumberPortMap.DefaultVatDefermentAmount, mapping.VatDefermentAmount);
		}

		public void TestAccountStartDay()
		{
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.AccountStartDay = 0;
			AssertHasError(mapping.AccountStartDayInfo, FinancialAccountNumberPortMap.MustBeAValidCalendarDay);
			mapping.AccountStartDay = 32;
			AssertHasError(mapping.AccountStartDayInfo, FinancialAccountNumberPortMap.MustBeAValidCalendarDay);
			mapping.AccountStartDay = 1;
			AssertNoError(mapping.AccountStartDayInfo, FinancialAccountNumberPortMap.MustBeAValidCalendarDay);
		}

		public void TestValidateCash()
		{
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.Cash = true;
			AssertNoError(mapping.CashInfo, FinancialAccountNumberPortMap.OnlyOneCashFANPerOrg);
			var mapping2 = coll.AddNew();
			mapping2.Cash = true;
			AssertHasError(mapping2.CashInfo, FinancialAccountNumberPortMap.OnlyOneCashFANPerOrg);
		}

		public void TestImporterPaysOrCreditorPKIsCleared()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = true;
			Factory.Save();
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.ImporterPays = ZBool.False;
			AssertEquals("mapping.CreditorPKInfo.ReadOnly", false, mapping.CreditorPKInfo.ReadOnly);
			mapping.CreditorPK = org.PK;
			mapping.ImporterPays = ZBool.True;
			AssertEquals("mapping.CreditorPKInfo.ReadOnly", true, mapping.CreditorPKInfo.ReadOnly);
			AssertEquals("mapping.CreditorPK", ZGuid.Empty, mapping.CreditorPK);
			mapping.CreditorPK = org.PK;
			AssertEquals("mapping.ImporterPays", ZBool.False, mapping.ImporterPays);
		}

		public void TestImporterPaysOrAutoAllocationAllowed()
		{
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.ImporterPays = false;
			mapping.AutoAllocationAllowed = false;
			AssertEquals("Importer Pays is false", false, mapping.ImporterPays);
			AssertEquals("Auto Allocation Allowed is false", false, mapping.AutoAllocationAllowed);
			mapping.ImporterPays = true;
			AssertEquals("Importer Pays is true", true, mapping.ImporterPays);
			AssertEquals("Auto Allocation Allowed is false", false, mapping.AutoAllocationAllowed);
			mapping.AutoAllocationAllowed = true;
			AssertEquals("Importer Pays is false", false, mapping.ImporterPays);
			AssertEquals("Auto Allocation Allowed is true", true, mapping.AutoAllocationAllowed);
			mapping.ImporterPays = true;
			AssertEquals("Importer Pays is true", true, mapping.ImporterPays);
			AssertEquals("Auto Allocation Allowed is false", false, mapping.AutoAllocationAllowed);
		}

		public void TestValidateOrganizationPK()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, ZString.Empty, Core.Constants.CountryCodes.SouthAfrica);
			var org2 = Factory.New<OrgHeader>();
			org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AST11", Core.Constants.CountryCodes.SouthAfrica);
			var org3 = Factory.New<OrgHeader>();
			org3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AST11", Core.Constants.CountryCodes.NewCaledonia);
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.OrganizationPK = ZGuid.Empty;
			var mustBeEnteredMessage = MandatoryValidation.MustBeEnteredMessage("Organization");
			AssertHasError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
			AssertNoErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
			AssertNoError(mapping.OrganizationPKInfo, FinancialAccountNumberPortMap.ValidOrganizationIsRequired);
			mapping.OrganizationPK = ZGuid.Invalid;
			AssertNoError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
			AssertHasErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
			AssertNoError(mapping.OrganizationPKInfo, FinancialAccountNumberPortMap.ValidOrganizationIsRequired);
			mapping.OrganizationPK = org1.PK;
			AssertNoError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
			AssertNoErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
			AssertHasError(mapping.OrganizationPKInfo, FinancialAccountNumberPortMap.ValidOrganizationIsRequired);
			mapping.OrganizationPK = org3.PK;
			AssertNoError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
			AssertNoErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
			AssertHasError(mapping.OrganizationPKInfo, FinancialAccountNumberPortMap.ValidOrganizationIsRequired);
			mapping.OrganizationPK = org2.PK;
			AssertNoError(mapping.OrganizationPKInfo, mustBeEnteredMessage);
			AssertNoErrorContaining(mapping.OrganizationPKInfo, ListValidation.InvalidCodeError);
			AssertNoError(mapping.OrganizationPKInfo, FinancialAccountNumberPortMap.ValidOrganizationIsRequired);
		}

		public void TestValidateCustomsOfficeCode()
		{
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			Factory.Save();
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.CustomsOfficeCode = ZString.Empty;
			var mustBeEnteredMessage = MandatoryValidation.MustBeEnteredMessage("Customs Office Code");
			AssertHasError(mapping.CustomsOfficeCodeInfo, mustBeEnteredMessage);
			mapping.CustomsOfficeCode = "~~~";
			AssertNoError(mapping.CustomsOfficeCodeInfo, mustBeEnteredMessage);
			AssertHasErrorContaining(mapping.CustomsOfficeCodeInfo, ListValidation.InvalidCodeError);
			mapping.CustomsOfficeCode = "BFN";
			AssertNoErrorContaining(mapping.CustomsOfficeCodeInfo, ListValidation.InvalidCodeError);
			mapping = coll.AddNew();
			mapping.CustomsOfficeCode = "BFN";
			AssertHasError(mapping.CustomsOfficeCodeInfo, FinancialAccountNumberPortMap.DuplicateCustomsOfficeCode);
			mapping.CustomsOfficeCode = "BBR";
			AssertNoError(mapping.CustomsOfficeCodeInfo, FinancialAccountNumberPortMap.DuplicateCustomsOfficeCode);
			AssertNoError(mapping.CustomsOfficeCodeInfo, FinancialAccountNumberPortMap.CustomsOfficeMustBeBlankForCash);
			mapping.Cash = true;
			mapping.RunPreSaveValidation();
			AssertHasError(mapping.CustomsOfficeCodeInfo, FinancialAccountNumberPortMap.CustomsOfficeMustBeBlankForCash);
		}

		public void ValidateFinancialAccountNumber()
		{
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.FinancialAccountNumber = ZString.Empty;
			AssertHasError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberIsRequired);
			AssertNoError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberMustBeValid);
			mapping.FinancialAccountNumber = "23";
			AssertNoError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberIsRequired);
			AssertHasError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberMustBeValid);
			mapping.FinancialAccountNumber = "123456789A";
			AssertNoError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberIsRequired);
			AssertHasError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberMustBeValid);
			mapping.FinancialAccountNumber = "12345678901";
			AssertNoError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberIsRequired);
			AssertHasError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberMustBeValid);
			mapping.FinancialAccountNumber = "1234567890";
			AssertNoError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberIsRequired);
			AssertNoError(mapping.FinancialAccountNumberInfo, FinancialAccountNumberPortMap.FinancialAccountNumberMustBeValid);
		}

		public void TestValidateCreditorPK()
		{
			var orgheader = Factory.NewWithValidTestData<OrgHeader>();
			orgheader.CompanyData.OB_IsCreditor = true;
			var orgheader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader2.CompanyData.OB_IsCreditor = false;
			Factory.Save();
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.CreditorPK = ZGuid.Empty;
			AssertHasError(mapping.CreditorPKInfo, FinancialAccountNumberPortMap.EitherCreditorOrImporterPaysIsNeeded);
			AssertNoErrorContaining(mapping.CreditorPKInfo, ListValidation.InvalidCodeError);
			mapping.CreditorPK = orgheader2.PK;
			AssertNoError(mapping.CreditorPKInfo, FinancialAccountNumberPortMap.EitherCreditorOrImporterPaysIsNeeded);
			AssertHasErrorContaining(mapping.CreditorPKInfo, ListValidation.InvalidCodeError);
			mapping.CreditorPK = orgheader.PK;
			AssertNoError(mapping.CreditorPKInfo, FinancialAccountNumberPortMap.EitherCreditorOrImporterPaysIsNeeded);
			AssertNoErrorContaining(mapping.CreditorPKInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidatePaymentDay()
		{
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var mapping = coll.AddNew();
			mapping.PaymentDay = 0;
			AssertNoError(mapping.PaymentDayInfo, FinancialAccountNumberPortMap.PaymentDayMustBeAValidCalendarDay);
			mapping.PaymentDay = -1;
			AssertHasError(mapping.PaymentDayInfo, FinancialAccountNumberPortMap.PaymentDayMustBeAValidCalendarDay);
			mapping.PaymentDay = 32;
			AssertHasError(mapping.PaymentDayInfo, FinancialAccountNumberPortMap.PaymentDayMustBeAValidCalendarDay);
			mapping.PaymentDay = 1;
			AssertNoError(mapping.PaymentDayInfo, FinancialAccountNumberPortMap.PaymentDayMustBeAValidCalendarDay);
			mapping.PaymentDay = 31;
			AssertNoError(mapping.PaymentDayInfo, FinancialAccountNumberPortMap.PaymentDayMustBeAValidCalendarDay);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override FinancialAccountNumberPortMap GetBusinessObjectToClone()
		{
			return (FinancialAccountNumberPortMap)GetNewBusinessObject();
		}

		protected override FinancialAccountNumberPortMap GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
