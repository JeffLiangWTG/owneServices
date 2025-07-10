using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccGLHeaderFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestGetBizObjFromCode()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;

			AccGLHeader glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader1.AG_AccountNum = "4101.02.00";
			AccGLAccountDescriptor descriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor1.AJ_Language = GlbStaff.CurrentUser.GS_WorkingLanguage;
			descriptor1.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			descriptor1.AJ_LocalAccountNumber = "41010200";
			descriptor1.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			AccGLDescriptorPivot descriptorPivot1 = Factory.NewWithValidTestData<AccGLDescriptorPivot>();
			descriptorPivot1.YJ_AJ = descriptor1.PK;
			descriptorPivot1.YJ_AG = glHeader1.PK;

			AccGLHeader glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader2.AG_AccountNum = "4101.01.00";
			AccGLAccountDescriptor descriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor2.AJ_Language = Core.SharedConstants.Languages.EnglishAmerican;
			descriptor2.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			descriptor2.AJ_LocalAccountNumber = "41010100";
			descriptor2.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			AccGLDescriptorPivot descriptorPivot2 = Factory.NewWithValidTestData<AccGLDescriptorPivot>();
			descriptorPivot2.YJ_AJ = descriptor2.PK;
			descriptorPivot2.YJ_AG = glHeader2.PK;

			Factory.Save();

			AccGLHeaderCollection collection = new AccGLHeaderCollection(Factory);
			AccGLHeaderFindBoxListProvider listProvider = new AccGLHeaderFindBoxListProvider(collection);

			ZGuid headerPK = listProvider.PrimaryKeyFromCode("4101.02.00");
			AssertEquals(glHeader1.PK, headerPK);

			headerPK = listProvider.PrimaryKeyFromCode("41010200");
			AssertEquals(glHeader1.PK, headerPK);

			headerPK = listProvider.PrimaryKeyFromCode("4101.01.00");
			AssertEquals(glHeader2.PK, headerPK);

			headerPK = listProvider.PrimaryKeyFromCode("41010100");
			AssertNotEquals(glHeader2.PK, headerPK);
		}

		public void TestGetGLHeaderFromAlternateGLAccountWhenTransactionLineIsNull()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "1111", accountType: Core.Constants.AccountType.Header);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 1, "");

			Factory.Save();

			var collection = new AccGLHeaderCollection(Factory, (AccTransactionLines)null);
			var listProvider = new AccGLHeaderFindBoxListProvider(collection, null);

			var gLHeaderPK = listProvider.GetGLHeaderFromAlternateGLAccount(Chart.PK, alternateGLAccount.AGA_AccountNum);

			AssertEquals(GLHeader.PK, gLHeaderPK);
		}

		public void TestGetGLHeaderFromAlternateGLAccountWithoutAttribute()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "1111", accountType: Core.Constants.AccountType.Header);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 1, "");

			Factory.Save();

			var transactionHeaderPK = CreateTransactionHeader();
			AssertGLHeaderIsValidFromAlternateGLAccount(transactionHeaderPK,alternateGLAccount.AGA_AccountNum, string.Empty, string.Empty);
		}

		public void TestGetGLHeaderFromAlternateGLAccountWithSingleAttribute()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "2222", accountType: Core.Constants.AccountType.Header);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 1, AlternateGLAccountAttributeCode.LFO, LFOCodes.LOC);

			Factory.Save();

			var transactionHeaderPK = CreateTransactionHeader();
			AssertGLHeaderIsValidFromAlternateGLAccount(transactionHeaderPK, alternateGLAccount.AGA_AccountNum, AlternateGLAccountAttributeCode.LFO, LFOCodes.LOC, withAttribute: true);
		}

		public void TestGetGLHeaderFromAlternateGLAccountWithMultipleAttributes()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "2222", accountType: Core.Constants.AccountType.Header);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 1, AlternateGLAccountAttributeCode.LFO, LFOCodes.LOC);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 2, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC);

			Factory.Save();

			var transactionHeaderPK = CreateTransactionHeader();
			AssertGLHeaderIsValidFromAlternateGLAccount(transactionHeaderPK, alternateGLAccount.AGA_AccountNum, AlternateGLAccountAttributeCode.LFO, LFOCodes.LOC, withAttribute: true);
			AssertGLHeaderIsValidFromAlternateGLAccount(transactionHeaderPK, alternateGLAccount.AGA_AccountNum, AlternateGLAccountAttributeCode.LFE, LFECodes.LOC, withAttribute: true);
		}

		public void TestPrimaryKeyFromCode_GLAccountSelectionAndEntry()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "1111", accountType: Core.Constants.AccountType.Header);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 1, "");

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertGLAccountSelectionAndEntry(isRegistryEnabled: false, Guid.Empty, GLHeader.AG_AccountNum);
				AssertGLAccountSelectionAndEntry(isRegistryEnabled: true, Chart.PK.ToGuid(), alternateGLAccount.AGA_AccountNum);
			});
		}

		public void TestPrimaryKeyFromCode_InvalidGLAccountShouldReturnInvalidGuid()
		{
			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader1.AG_AccountNum = "4101.02.00";
			glHeader1.AG_IsActive = false;

			Factory.Save();

			var collection = new AccGLHeaderCollection(Factory);
			var listProvider = new AccGLHeaderFindBoxListProvider(collection);

			var headerPK = listProvider.PrimaryKeyFromCode("4101.02.00");
			AssertEquals(ZGuid.Invalid, headerPK);
		}

		void AssertGLAccountSelectionAndEntry(bool isRegistryEnabled, Guid chartPK, string code)
		{
			using (AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chartPK))
			{
				var transactionHeaderPK = CreateTransactionHeader();
				var listProvider = InitializeListProvider(transactionHeaderPK);
				var gLHeaderPK = listProvider.PrimaryKeyFromCode(code);

				AssertEquals($"Registry GLAccountSelectionAndEntry Enabled: {isRegistryEnabled}", GLHeader.PK, gLHeaderPK);
			}
		}

		void AssertGLHeaderIsValidFromAlternateGLAccount(ZGuid transactionHeaderPK, ZString alternateAccountNumber, ZString attribute, ZString attributeValue, bool withAttribute = false)
		{
			var listProvider = InitializeListProvider(transactionHeaderPK);

			var gLHeaderPK = listProvider.GetGLHeaderFromAlternateGLAccount(Chart.PK, alternateAccountNumber);

			AssertEquals(GLHeader.PK, gLHeaderPK);

			if (withAttribute)
			{
				var expectedAttribute = listProvider.TransactionLines.AccTransactionLineDissectionAttributes
					.Where(x => x.ALD_Attribute == attribute && x.ALD_AttributeValue == attributeValue);

				AssertEquals(expectedAttribute.Count(), 1);
			}
			else
			{
				AssertEquals(listProvider.TransactionLines.AccTransactionLineDissectionAttributes.Count, 0);
			}
		}

		public void TestGetGLHeaderFromAlternateGLAccountWithMultipleAlternateGLAccountAndSameParentGLAccount()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "2222", accountType: Core.Constants.AccountType.Header);
			var alternateGLAccount2 = Creator.CreateAccAlternateGlAccount(Chart.PK, "3333", accountType: Core.Constants.AccountType.Header);
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader.PK, 1, "");
			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount2, GLHeader.PK, 1);

			Factory.Save();

			var transactionHeaderPK = CreateTransactionHeader();
			AssertGLHeaderIsValidFromAlternateGLAccount(transactionHeaderPK, alternateGLAccount.AGA_AccountNum, string.Empty, string.Empty);
		}

		public void TestGetGLHeaderFromAlternateGLAccount()
		{
			var glAccount = Factory.NewWithValidTestData<AccGLHeader>();
			var collection = new AccGLHeaderCollectionForTest(Factory, null, (collection, glHeaderList) => { glHeaderList.Add(glAccount); });
			var provider = collection.FindBoxListProvider_ForTest;
			var creator = new AccountingTestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("TRR");
			Factory.Save();

			var alternateGLAccount = creator.CreateAccAlternateGlAccount(chart.PK, "111");
			var glHeader = creator.CreateAccGLHeader("3333.33.33");
			creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK);
			Factory.Save();
			AssertEquals(ZGuid.Invalid, provider.PrimaryKeyFromCode("111"));

			using (AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid()))
			{
				AssertEquals(glHeader.PK, provider.PrimaryKeyFromCode("111"));

				var glHeader2 = creator.CreateAccGLHeader("4444.33.33");
				creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader2.PK);
				Factory.Save();
				AssertEquals(glAccount.PK, provider.PrimaryKeyFromCode("111"));
			}
		}

		AccGLHeaderFindBoxListProvider InitializeListProvider(ZGuid transactionHeaderPK)
		{
			var transactionLines = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLines.AL_AH = transactionHeaderPK;
			transactionLines.TransactionHeader.AH_TransactionType = TransactionTypes.GLStandardJournal;

			var collection = new AccGLHeaderCollection(Factory, transactionLines);
			var listProvider = new AccGLHeaderFindBoxListProvider(collection, transactionLines);

			return listProvider;
		}

		ZGuid CreateTransactionHeader()
		{
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();

			return transactionHeader.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Chart = Creator.CreateAlternateChart("TST");
			Creator.CreateAccAlternateChartFormat(Chart, 1, "9999");
			GLHeader = Creator.CreateAccGLHeader("1333.33.33", Core.Constants.AccountType.Note);

			Factory.Save();
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
		AccAlternateChart Chart;
		AccGLHeader GLHeader;
	}
}
