using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxExpenseRecoveryChargeCodeGuidRegistryDataType))]
	sealed class TaxExpenseRecoveryChargeCodeGuidRegistryDataTypeTest : RegistryDataTypeTestCase<TaxExpenseRecoveryChargeCodeGuidRegistryDataType>
	{
		protected override TaxExpenseRecoveryChargeCodeGuidRegistryDataType GetNewDataType()
		{
			return new TaxExpenseRecoveryChargeCodeGuidRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Guid newGuid = Guid.NewGuid();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Guid.Empty, Encoding.Unicode.GetBytes(Guid.Empty.ToString())),
				new ValidSampleAndBinaryValueInDB(newGuid, Encoding.Unicode.GetBytes(newGuid.ToString()))
			};
		}

		public void TestValidateRevenueTaxExpenseRecoveryChargeCode()
		{
			var factory = new BusinessObjectFactory();
			var nonCurrentCompany = factory.NewWithValidTestData<GlbCompany>();
			var taxSystem1 = new AccountingTestObjectCreator(factory).CreateTaxSystem("TAXSYS1", includeInInvoiceTotal: false, taxSuperType: TaxSuperTypeList.TurnoverTax.Code);
			var taxSystem2 = new AccountingTestObjectCreator(factory).CreateTaxSystem("TAXSYS2", includeInInvoiceTotal: true, taxSuperType: TaxSuperTypeList.TurnoverTax.Code);
			
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.GetTaxSystem(taxSystem1.Code, It.IsAny<BusinessObjectFactory>())).Returns(taxSystem1);
			mockITaxFrameworkConfigurationHelper.Setup(x => x.GetTaxSystem(taxSystem2.Code, It.IsAny<BusinessObjectFactory>())).Returns(taxSystem2);
			mockITaxFrameworkConfigurationHelper.WithGetTaxSystems(taxSystem1);
			mockITaxFrameworkConfigurationHelper.WithGetTaxSystems(taxSystem2);

			var taxConfiguration = factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = nonCurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_RN_NKCountry = taxSystem1.Country;
			taxConfiguration.ETC_TaxSystemCode = taxSystem1.Code;

			var taxConfiguration1 = factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration1.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration1.ETC_RN_NKCountry = taxSystem1.Country;
			taxConfiguration1.ETC_TaxSystemCode = taxSystem1.Code;

			var taxConfiguration2 = factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration2.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration2.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration2.ETC_RN_NKCountry = taxSystem2.Country;
			taxConfiguration2.ETC_TaxSystemCode = taxSystem2.Code;

			var taxConfigList = new AccTaxConfigurationCollection(factory);
			mockITaxFrameworkConfigurationHelper.WithGetCompanyTaxConfigurations(taxConfigList, factory, GlbCompany.CurrentCompany, new ZQuery());

			var taxOverrideGroup = factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroup1 = factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroup2 = factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroupTaxConfigurationPivot = factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			var taxOverrideGroupTaxConfigurationPivot1 = factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			var taxOverrideGroupTaxConfigurationPivot2 = factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();

			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_AX_TaxOverrideGroup = taxOverrideGroup2.PK;

			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration2.PK;

			var chargeCode = factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode1 = factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TAX1";
			chargeCode1.AC_Code = "TAX2";
			chargeCode2.AC_Code = "TAX3";

			var taxOverrideGroupChargeCodePivot = factory.New<AccTaxOverrideGroupChargeCodePivot>();
			var taxOverrideGroupChargeCodePivot1 = factory.New<AccTaxOverrideGroupChargeCodePivot>();
			var taxOverrideGroupChargeCodePivot2 = factory.New<AccTaxOverrideGroupChargeCodePivot>();
			taxOverrideGroupChargeCodePivot.ACP_AC_ChargeCode = chargeCode.PK;
			taxOverrideGroupChargeCodePivot1.ACP_AC_ChargeCode = chargeCode1.PK;
			taxOverrideGroupChargeCodePivot2.ACP_AC_ChargeCode = chargeCode2.PK;
			taxOverrideGroupChargeCodePivot.ACP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			taxOverrideGroupChargeCodePivot1.ACP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;
			taxOverrideGroupChargeCodePivot2.ACP_AX_TaxOverrideGroup = taxOverrideGroup2.PK;

			factory.Save();

			var taxExpenseRecoveryChargeCodeGuidRegistryDataType = new TaxExpenseRecoveryChargeCodeGuidRegistryDataType();
			var registryItem = new GuidRegistryItem("TaxExpenseChargeCode", null, null, null, taxExpenseRecoveryChargeCodeGuidRegistryDataType, RegistryStorageFlags.Company, Guid.Empty);
			var exceptionMessage = "You cannot set this charge code because this charge code already attached to one of the Tax Override Group for TRX EXPENSE Tax Configuration.";
			var dataType = (TaxExpenseRecoveryChargeCodeGuidRegistryDataType)registryItem.DataType;
			AssertExceptionThrown<RegistryValidationException>(exceptionMessage, () => dataType.Validate(registryItem, chargeCode1.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, chargeCode2.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, chargeCode.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}
	}
}
