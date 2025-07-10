using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EnableNewOsOutstandingAmountDataType))]
	sealed class EnableNewOsOutstandingAmountDataTypeTest : BooleanRegistryDataTypeTest
	{
		public void TestValidateCoreWithoutNewOSAmountData()
		{
			var registryItem = CreateRegistryItemForTest();
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, false, Guid.Empty, Guid.Empty, Guid.Empty));

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			registryItem.SetValue(currentCompanyPK, Guid.Empty, Guid.Empty, true);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, currentCompanyPK, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, false, currentCompanyPK, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, false, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidateCoreWithNewOSAmountData()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new AccountingTestObjectCreator(factory);
			var header = factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_IsOSOutstandingAmountApplicable = true;
			factory.Save();

			var registryItem = CreateRegistryItemForTest();
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, false, Guid.Empty, Guid.Empty, Guid.Empty));

			var nonHeaderCompanyPK = testHelper.NonCurrentCompany.PK.ToGuid();
			var headerCompanyPK = header.Company.PK.ToGuid();
			registryItem.SetValue(nonHeaderCompanyPK, Guid.Empty, Guid.Empty, true);
			registryItem.SetValue(headerCompanyPK, Guid.Empty, Guid.Empty, true);

			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, nonHeaderCompanyPK, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, false, nonHeaderCompanyPK, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, headerCompanyPK, Guid.Empty, Guid.Empty));
			var ex = AssertExceptionThrown<RegistryValidationException>(() => DataType.Validate(registryItem, false, headerCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals("The registry cannot be turned off once it's set to 'Yes'.", ex.Message);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertNoExceptionThrown(() => DataType.Validate(registryItem, true, Guid.Empty, Guid.Empty, Guid.Empty));
			ex = AssertExceptionThrown<RegistryValidationException>(() => DataType.Validate(registryItem, false, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("The registry cannot be turned off once it's set to 'Yes'.", ex.Message);
		}

		protected override BooleanRegistryDataType GetNewDataType()
		{
			return new EnableNewOsOutstandingAmountDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(true, Encoding.Unicode.GetBytes("True")),
				new ValidSampleAndBinaryValueInDB(false, Encoding.Unicode.GetBytes("False")),
			};
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		BooleanRegistryItem CreateRegistryItemForTest() => new BooleanRegistryItem("Test Registry Name", (NoResString)"Test Category", (NoResString)"Test Caption", (NoResString)"Test Hint", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false, new EnableNewOsOutstandingAmountDataType());
	}
}
