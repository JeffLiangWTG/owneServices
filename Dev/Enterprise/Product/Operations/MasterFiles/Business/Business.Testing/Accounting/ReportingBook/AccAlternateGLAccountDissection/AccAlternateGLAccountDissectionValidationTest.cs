using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Moq;
using static Enterprise.MasterFiles.Business.AccAlternateGLAccountDissectionLookups;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccAlternateGLAccountDissectionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckADC_AAC_AlternateChart()
		{
			var factory = Factory.CreateNewFactory();
			var chart = factory.NewWithValidTestData<AccAlternateChart>();
			factory.Save();

			AlternateGLAccountDissection.ADC_AAC_AlternateChart = ZGuid.Empty;
			AssertHasError(AlternateGLAccountDissection.ADC_AAC_AlternateChartInfo, "Please enter a value.");

			AlternateGLAccountDissection.ADC_AAC_AlternateChart = chart.PK;
			AssertNoError(AlternateGLAccountDissection.ADC_AAC_AlternateChartInfo, "Please enter a value.");
		}

		public void TestCheckADC_AAC_AlternateChart_HasLinkedAccount()
		{
			LinkChartToAccount(AlternateGLAccountDissection);
			Factory.Save();

			var anotherDissection = GLHeader.AlternateGLAccountDissections.AddNew();
			anotherDissection.ADC_AAC_AlternateChart = AlternateGLAccountDissection.ADC_AAC_AlternateChart;
			AssertHasError(anotherDissection.ADC_AAC_AlternateChartInfo, "You cannot add a new attribute to this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<" + GLHeader.AccountNum + ">, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.");

			var noLinkedChart = AccountingTestObjectCreator.CreateAlternateChart("CC2");
			AlternateGLAccountDissection.ADC_AAC_AlternateChart = noLinkedChart.PK;
			AssertHasError(AlternateGLAccountDissection.ADC_AAC_AlternateChartInfo, "You cannot change the chart to this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<" + GLHeader.AccountNum + ">, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.");
		}

		void LinkChartToAccount(AccAlternateGLAccountDissection dissection)
		{
			if (dissection.ADC_AAC_AlternateChart.IsEmpty)
			{
				var chart = AccountingTestObjectCreator.CreateAlternateChart("CC1");
				dissection.ADC_AAC_AlternateChart = chart.PK;
			}

			if (dissection.ADC_Attribute.IsEmpty)
			{
				dissection.ADC_Attribute = AlternateGLAccountAttributeCode.SPR;
			}
			Factory.Save();

			var account = AccountingTestObjectCreator.CreateAccAlternateGlAccount(dissection.ADC_AAC_AlternateChart,dissection.GLHeader.AccountNum);
			AccountingTestObjectCreator.CreateAccAlternateGlAccountAttribute(account,dissection.ADC_AG_GLHeader,attribute: dissection.ADC_Attribute);
		}

		public void TestCheckADC_Attribute()
		{
			AlternateGLAccountDissection.ADC_Attribute = ZString.Empty;
			AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo, "Please enter an Attribute.");

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_IsGlobal = true;
			AlternateGLAccountDissection.ADC_AAC_AlternateChart = chart.PK;
			AlternateGLAccountDissection.ADC_Attribute = NonGlobalAttributeCode.ORG;
			AssertNoError(AlternateGLAccountDissection.ADC_AttributeInfo, "Please enter an Attribute.");
			AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo, "This is a company-specific dissection attribute that cannot be selected for a Global chart.");

			var newAlternateGLAccountDissections = GLHeader.AlternateGLAccountDissections.AddNew();
			newAlternateGLAccountDissections.ADC_AAC_AlternateChart = chart.PK;
			newAlternateGLAccountDissections.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;
			AlternateGLAccountDissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;
			AssertNoError(AlternateGLAccountDissection.ADC_AttributeInfo, "This is a company-specific dissection attribute that cannot be selected for a Global chart.");
			AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo, "Chart and Attribute values combination should not be duplicated.");

			AlternateGLAccountDissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE;
			AssertNoErrors(AlternateGLAccountDissection.ADC_AttributeInfo);
		}

		public void TestCheckADC_Attribute_HasLinkedAccount()
		{
			AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.SPR;
			LinkChartToAccount(AlternateGLAccountDissection);
			Factory.Save();

			AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.LFE;
			AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo,  "You cannot change the attribute to this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<" + GLHeader.AccountNum + ">, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.");
		}

		public void TestCheckORGAttribute()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_IsGlobal = false;
			AlternateGLAccountDissection.ADC_AAC_AlternateChart = chart.PK;
			AlternateGLAccountDissection.ADC_Attribute = NonGlobalAttributeCode.ORG;
			AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo,
				"The 'ORG' attribute can only be used for GL Accounts specified in Accounting > General Ledger Defaults > Control Account > AR Control Account OR AP Control Account.");

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ARControlAccount)
				.Returns(GLHeader.PK.ToGuid());
			using (ObjectFactory.Substitute(mock.Object))
			{
				AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.LFE;

				var dissection = GLHeader.AlternateGLAccountDissections.AddNew();
				dissection.ADC_Attribute = NonGlobalAttributeCode.ORG;

				AssertNoError(dissection.ADC_AttributeInfo, "The 'ORG' attribute can only be used for GL Accounts specified in Accounting > General Ledger Defaults > Control Account > AR Control Account OR AP Control Account.");
				AssertNoError(dissection.ADC_AttributeInfo, "The 'ORG' attribute cannot be used in combination with other attributes.");

				dissection.ADC_AAC_AlternateChart = chart.PK;
				dissection.ADC_Attribute = NonGlobalAttributeCode.ORG;
				AssertHasError(dissection.ADC_AttributeInfo, "The 'ORG' attribute cannot be used in combination with other attributes.");
			}
		}

		public void TestCheckTICAttribute()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_IsGlobal = false;
			AlternateGLAccountDissection.ADC_AAC_AlternateChart = chart.PK;
			AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.TIC;
			AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo,
				"The 'TIC' attribute can only be used for GL Account specified in Accounting > General Ledger Defaults > Control Account > Pending Tax Input Control Account or Pending Tax Output Control Account or Reportable Tax Input Control Account or Reportable Tax Output Control Account.");

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.PendingGSTInputControlAccount)
				.Returns(GLHeader.PK.ToGuid());
			using (ObjectFactory.Substitute(mock.Object))
			{
				AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.TIC;
				AssertNoError(AlternateGLAccountDissection.ADC_AttributeInfo, "The 'TIC' attribute can only be used for GL Account specified in Accounting > General Ledger Defaults > Control Account > Pending Tax Input Control Account or Pending Tax Output Control Account or Reportable Tax Input Control Account or Reportable Tax Output Control Account.");

				var dissection = GLHeader.AlternateGLAccountDissections.AddNew();
				dissection.ADC_Attribute = AlternateGLAccountAttributeCode.LFE;
				AssertNoErrors(dissection.ADC_AttributeInfo);
			}
		}

		public void TestCheckSPRAttirbute()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_IsGlobal = false;
			AlternateGLAccountDissection.ADC_AAC_AlternateChart = chart.PK;
			AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.SPR;
			AssertNoError(AlternateGLAccountDissection.ADC_AttributeInfo, "The 'SPR' attribute can not be used for GL Accounts specified in Accounting > General Ledger Defaults > Control Account > AR Control Account OR AP Control Account.");

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ARControlAccount)
				.Returns(GLHeader.PK.ToGuid());
			using (ObjectFactory.Substitute(mock.Object))
			{
				AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.SPR;
				AlternateGLAccountDissection.Validation.ValidateADC_Attribute();

				AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo, "The 'SPR' attribute can not be used for GL Accounts specified in Accounting > General Ledger Defaults > Control Account > AR Control Account OR AP Control Account.");
			}

			mock = new Mock<IAccounting>();
			mock.Setup(m => m.APControlAccount)
				.Returns(GLHeader.PK.ToGuid());
			using (ObjectFactory.Substitute(mock.Object))
			{
				AlternateGLAccountDissection.ADC_Attribute = AlternateGLAccountAttributeCode.SPR;
				AlternateGLAccountDissection.Validation.ValidateADC_Attribute();

				AssertHasError(AlternateGLAccountDissection.ADC_AttributeInfo, "The 'SPR' attribute can not be used for GL Accounts specified in Accounting > General Ledger Defaults > Control Account > AR Control Account OR AP Control Account.");
			}
		}

		public void TestCheckADC_SeparateNumbering()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_IsGlobal = true;
			AlternateGLAccountDissection.ADC_AAC_AlternateChart = chart.PK;
			AlternateGLAccountDissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;

			var newAlternateGLAccountDissections = GLHeader.AlternateGLAccountDissections.AddNew();
			newAlternateGLAccountDissections.ADC_AAC_AlternateChart = chart.PK;
			newAlternateGLAccountDissections.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
			newAlternateGLAccountDissections.ADC_SeparateNumbering = true;

			AlternateGLAccountDissection.ADC_SeparateNumbering = false;
			AssertHasError(AlternateGLAccountDissection.ADC_SeparateNumberingInfo, "The \"Separate Numbering\" setting for all attributes of an alternate chart must be the same.");

			AlternateGLAccountDissection.ADC_SeparateNumbering = true;
			AssertNoError(AlternateGLAccountDissection.ADC_SeparateNumberingInfo, "The \"Separate Numbering\" setting for all attributes of an alternate chart must be the same.");
		}

		public void TestCheckADC_SeparateNumbering_DisallowedControlAccounts()
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsNotAllowedForSeparateNumbering(GLHeader.PK))
				.Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var chart = Factory.NewWithValidTestData<AccAlternateChart>();
				AlternateGLAccountDissection.ADC_AAC_AlternateChart = chart.PK;
				AlternateGLAccountDissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
				AlternateGLAccountDissection.ADC_SeparateNumbering = ZBool.True;
				AssertHasError(AlternateGLAccountDissection.ADC_SeparateNumberingInfo, "The \"Separate Numbering\" setting is not allowed in this GL Account.");
			}
		}

		public void TestCheckADC_SeparateNumbering_HasLinkedAccount_Untick()
		{
			AlternateGLAccountDissection.ADC_SeparateNumbering = true;
			LinkChartToAccount(AlternateGLAccountDissection);
			Factory.Save();

			AlternateGLAccountDissection.ADC_SeparateNumbering = false;
			AssertHasError(AlternateGLAccountDissection.ADC_SeparateNumberingInfo,  "You cannot untick 'Separate Numbering' check box of this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<" + GLHeader.AccountNum + ">, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.");
		}

		public void TestCheckADC_SeparateNumbering_HasLinkedAccount_Tick()
		{
			AlternateGLAccountDissection.ADC_SeparateNumbering = false;
			LinkChartToAccount(AlternateGLAccountDissection);
			Factory.Save();

			AlternateGLAccountDissection.ADC_SeparateNumbering = true;
			AssertHasError(AlternateGLAccountDissection.ADC_SeparateNumberingInfo,  "You cannot tick 'Separate Numbering' check box of this Chart because there is one Alternate Account relating to the GL Account.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<" + GLHeader.AccountNum + ">, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.");
		}

		#region Implementation

		AccAlternateGLAccountDissection AlternateGLAccountDissection;
		AccGLHeader GLHeader;

		protected override void SetUp()
		{
			base.SetUp();
			GLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			AlternateGLAccountDissection = GLHeader.AlternateGLAccountDissections.AddNew();
		}

		#endregion

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ??= new AccountingTestObjectCreator(Factory);

		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
