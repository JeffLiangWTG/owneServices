using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.ReportingBook
{
	[TestedType(typeof(GLAccountSelectionAndEntryGuidRegistryDataType))]
	sealed class GLAccountSelectionAndEntryGuidRegistryDataTypeTest : RegistryDataTypeTestCase<GLAccountSelectionAndEntryGuidRegistryDataType>
	{
		protected override GLAccountSelectionAndEntryGuidRegistryDataType GetNewDataType()
		{
			return new GLAccountSelectionAndEntryGuidRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var chart = factory.NewWithValidTestData<AccAlternateChart>();
			factory.Save();

			var alternateGLAccount = factory.NewWithValidTestData<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AAC_AlternateChart = chart.PK;
			factory.Save();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Guid.Empty, Encoding.Unicode.GetBytes(Guid.Empty.ToString())),
				new ValidSampleAndBinaryValueInDB(chart.PK.ToGuid(), Encoding.Unicode.GetBytes(chart.PK.ToString()))
			};
		}

		public void TestValidateCore()
		{
			var factory = new BusinessObjectFactory();
			var chartWithAlternateGLAccount = factory.NewWithValidTestData<AccAlternateChart>();
			var chartWithoutAlternateGLAccount = factory.NewWithValidTestData<AccAlternateChart>();
			factory.Save();

			var alternateGLAccount = factory.NewWithValidTestData<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AAC_AlternateChart = chartWithAlternateGLAccount.PK;
			factory.Save();

			var gLAccountSelectionAndEntryGuidRegistryDataType = new GLAccountSelectionAndEntryGuidRegistryDataType();
			var registryItem = new GuidRegistryItem("GLAccountSelectionAndEntry", null, null, null, gLAccountSelectionAndEntryGuidRegistryDataType, RegistryStorageFlags.Company, Guid.Empty);
			var dataType = (GLAccountSelectionAndEntryGuidRegistryDataType)registryItem.DataType;
			AssertExceptionThrown<RegistryValidationException>("Please select a valid Alternate Chart.", () => dataType.Validate(registryItem, Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertExceptionThrown<RegistryValidationException>("The Alternate Chart of Account you selected does not have any Alternate GL Accounts. Please create Alternate GL Accounts for it first.", () => dataType.Validate(registryItem, chartWithoutAlternateGLAccount.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, chartWithAlternateGLAccount.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}
	}
}
