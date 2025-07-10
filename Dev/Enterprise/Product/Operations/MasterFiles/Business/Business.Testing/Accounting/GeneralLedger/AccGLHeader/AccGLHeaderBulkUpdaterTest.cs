using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using SectionTypes = Enterprise.MasterFiles.Business.AccGLHeader.Constants.SectionTypes;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLHeaderBulkUpdater))]
	sealed class AccGLHeaderBulkUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AccGLHeaderCollection allHeaders = new AccGLHeaderCollection(Factory);
			allHeaders.Load();

			int totalNumberOfHeaders = allHeaders.Count;

			List<AccGLHeaderCollection> lookups = new List<AccGLHeaderCollection>();

			lookups.Add(Sections.GLHeadersTSStart);
			lookups.Add(Sections.GLHeadersTSEnd);
			lookups.Add(Sections.GLHeadersOVStart);
			lookups.Add(Sections.GLHeadersOVEnd);
			lookups.Add(Sections.GLHeadersAPStart);
			lookups.Add(Sections.GLHeadersAPEnd);
			lookups.Add(Sections.GLHeadersOEStart);
			lookups.Add(Sections.GLHeadersOEEnd);
			lookups.Add(Sections.GLHeadersASStart);
			lookups.Add(Sections.GLHeadersASEnd);
			lookups.Add(Sections.GLHeadersLIStart);
			lookups.Add(Sections.GLHeadersLIEnd);

			foreach (AccGLHeaderCollection collection in lookups)
			{
				collection.Load();
				AssertEquals(totalNumberOfHeaders, collection.Count);
			}
		}

		public void TestValidationForAtLeastOneOfEachSectionIsDefined()
		{
			ZString pNLError = AccGLHeaderBulkUpdater.ErrorMessageForAtLeastOneProfitAndLossSection;
			ZString bSError = AccGLHeaderBulkUpdater.ErrorMessageForAtLeastOneBalanceSheetSection;

			SetValues(Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty);

			AssertHasError(Sections.AG_TradingStatementStartAccountInfo, pNLError);
			AssertHasError(Sections.AG_OverheadsStartAccountInfo, pNLError);
			AssertHasError(Sections.AG_ProfitAndLossAppropriationStartAccountInfo, pNLError);
			AssertHasError(Sections.AG_OwnersEquityStartAccountInfo, bSError);
			AssertHasError(Sections.AG_AssetsStartAccountInfo, bSError);
			AssertHasError(Sections.AG_LiabilitiesStartAccountInfo, bSError);

			SetValues(Acc1000.PK, Acc4999.PK, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Acc5000.PK, Acc9999.PK);

			AssertNoError(Sections.AG_TradingStatementStartAccountInfo, pNLError);
			AssertNoError(Sections.AG_OverheadsStartAccountInfo, pNLError);
			AssertNoError(Sections.AG_ProfitAndLossAppropriationStartAccountInfo, pNLError);
			AssertNoError(Sections.AG_OwnersEquityStartAccountInfo, bSError);
			AssertNoError(Sections.AG_AssetsStartAccountInfo, bSError);
			AssertNoError(Sections.AG_LiabilitiesStartAccountInfo, bSError);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);

			AssertNoError(Sections.AG_TradingStatementStartAccountInfo, pNLError);
			AssertNoError(Sections.AG_OverheadsStartAccountInfo, pNLError);
			AssertNoError(Sections.AG_ProfitAndLossAppropriationStartAccountInfo, pNLError);
			AssertNoError(Sections.AG_OwnersEquityStartAccountInfo, bSError);
			AssertNoError(Sections.AG_AssetsStartAccountInfo, bSError);
			AssertNoError(Sections.AG_LiabilitiesStartAccountInfo, bSError);
		}

		public void TestValidationWhenThereIsAnOverlapBetweenSections()
		{
			ZString error = AccGLHeaderBulkUpdater.ErrorMessageForOverlappingSections;

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);
			AssertNoErrorForAllInfos(error);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc6000.PK, Acc9999.PK);
			AssertHasErrorForStartInfos(error);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);
			AssertNoErrorForAllInfos(error);
		}

		public void TestValidationWhenSectionsDoNotCoverAllGLHeaders()
		{
			ZString error = AccGLHeaderBulkUpdater.ErrorMessageForSectionsDontCoverAllHeaders;

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);
			AssertNoErrorForAllInfos(error);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4900.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc8000.PK);
			AssertHasErrorForStartInfos(error);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);
			AssertNoErrorForAllInfos(error);
		}

		public void TestValidationWhenEndAccountIsSmallerThanStartAccount()
		{
			ZString error = AccGLHeaderBulkUpdater.ErrorMessageForEndDateGreaterThanStartDate;

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);

			AssertNoErrorForAllInfos(error);

			SetValues(Acc1900.PK, Acc1000.PK, Acc4899.PK, Acc2010.PK, Acc4999.PK, Acc4900.PK,
				Acc5900.PK, Acc5000.PK, Acc7999.PK, Acc6000.PK, Acc9999.PK, Acc8000.PK);

			AssertHasError(Sections.AG_TradingStatementEndAccountInfo, error);
			AssertHasError(Sections.AG_OverheadsEndAccountInfo, error);
			AssertHasError(Sections.AG_ProfitAndLossAppropriationEndAccountInfo, error);
			AssertHasError(Sections.AG_OwnersEquityEndAccountInfo, error);
			AssertHasError(Sections.AG_AssetsEndAccountInfo, error);
			AssertHasError(Sections.AG_LiabilitiesEndAccountInfo, error);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);

			AssertNoErrorForAllInfos(error);
		}

		public void TestValidationWhenStartAccountIsNotSpecified()
		{
			ZString error = AccGLHeaderBulkUpdater.ErrorMessageForStartAndEndAccountsAreMandatory;

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);

			AssertNoErrorForStartInfos(error);

			SetValues(Empty, Acc1900.PK, Empty, Acc4899.PK, Empty, Acc4999.PK,
				Empty, Acc5900.PK, Empty, Acc7999.PK, Empty, Acc9999.PK);

			AssertHasErrorForStartInfos(error);

			SetValues(Empty, Empty, Empty, Empty, Empty, Empty,
				Empty, Empty, Empty, Empty, Empty, Empty);

			AssertNoErrorForStartInfos(error);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);

			AssertNoErrorForStartInfos(error);
		}

		public void TestValidationWhenEndAccountIsNotSpecified()
		{
			ZString error = AccGLHeaderBulkUpdater.ErrorMessageForStartAndEndAccountsAreMandatory;

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);

			AssertNoErrorForEndInfos(error);

			SetValues(Acc1000.PK, Empty, Acc2010.PK, Empty, Acc4900.PK, Empty,
			Acc5000.PK, Empty, Acc6000.PK, Empty, Acc8000.PK, Empty);

			AssertHasErrorForEndInfos(error);

			SetValues(Empty, Empty, Empty, Empty, Empty, Empty,
				Empty, Empty, Empty, Empty, Empty, Empty);

			AssertNoErrorForEndInfos(error);

			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);

			AssertNoErrorForEndInfos(error);
		}

		public void TestSavingAndDefaultValues()
		{
			SetupAG_ColumnToEmptyForAllGLHeaders();

			BusinessObjectFactory sectionsFactory = new BusinessObjectFactory();
			Sections = new AccGLHeaderBulkUpdater(sectionsFactory);
			SetValues(Acc1000.PK, Acc1900.PK, Acc2010.PK, Acc4899.PK, Acc4900.PK, Acc4999.PK,
				Acc5000.PK, Acc5900.PK, Acc6000.PK, Acc7999.PK, Acc8000.PK, Acc9999.PK);
			sectionsFactory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(newFactory);

			ZString sql = @"SELECT {0}, MIN({1}) as Min, MAX({1}) as Max
							FROM dbo.AccGLHeader
							GROUP BY {0}
							ORDER BY MIN({1})";

			ZString aG_Column = AccGLHeaderSchema.Constants.AG_Column;
			ZString aG_AccountNum = AccGLHeaderSchema.Constants.AG_AccountNum;
			collection.Load(String.Format(sql, aG_Column, aG_AccountNum));
			AssertEquals("Number of Sections", 6, collection.Count);

			AssertMinAndMaxForSection(collection[0], SectionTypes.Codes.TradingStatement, Acc1000, Acc1900);
			AssertMinAndMaxForSection(collection[1], SectionTypes.Codes.Overheads, Acc2010, Acc4899);
			AssertMinAndMaxForSection(collection[2], SectionTypes.Codes.ProfitAndLossAppropriation, Acc4900, Acc4999);
			AssertMinAndMaxForSection(collection[3], SectionTypes.Codes.OwnersEquity, Acc5000, Acc5900);
			AssertMinAndMaxForSection(collection[4], SectionTypes.Codes.Assets, Acc6000, Acc7999);
			AssertMinAndMaxForSection(collection[5], SectionTypes.Codes.Liabilities, Acc8000, Acc9999);

			BusinessObjectFactory factoryForNewBizObj = new BusinessObjectFactory();

			AccGLHeaderBulkUpdater newBizObj = new AccGLHeaderBulkUpdater(factoryForNewBizObj);

			AssertEquals("TradingStatementStartAccount", newBizObj.AG_TradingStatementStartAccount, Acc1000.PK);
			AssertEquals("TradingStatementEndAccount", newBizObj.AG_TradingStatementEndAccount, Acc1900.PK);

			AssertEquals("OverheadsStartAccount", newBizObj.AG_OverheadsStartAccount, Acc2010.PK);
			AssertEquals("OverheadsEndAccount", newBizObj.AG_OverheadsEndAccount, Acc4899.PK);

			AssertEquals("ProfitAndLossAppropriationStartAccount", newBizObj.AG_ProfitAndLossAppropriationStartAccount, Acc4900.PK);
			AssertEquals("ProfitAndLossAppropriationEndAccount", newBizObj.AG_ProfitAndLossAppropriationEndAccount, Acc4999.PK);

			AssertEquals("OwnersEquityStartAccount", newBizObj.AG_OwnersEquityStartAccount, Acc5000.PK);
			AssertEquals("OwnersEquityEndAccount", newBizObj.AG_OwnersEquityEndAccount, Acc5900.PK);

			AssertEquals("AssetsStartAccount", newBizObj.AG_AssetsStartAccount, Acc6000.PK);
			AssertEquals("AssetsEndAccount", newBizObj.AG_AssetsEndAccount, Acc7999.PK);

			AssertEquals("LiabilitiesStartAccount", newBizObj.AG_LiabilitiesStartAccount, Acc8000.PK);
			AssertEquals("LiabilitiesEndAccount", newBizObj.AG_LiabilitiesEndAccount, Acc9999.PK);

			AssertEquals("HasChanges", false, newBizObj.HasChanges);
		}

		void AssertMinAndMaxForSection(DynamicBusinessObject bizObj, ZString sectionCode,
			AccGLHeader minAccount, AccGLHeader maxAccount)
		{
			AssertEquals("Section", bizObj[AccGLHeaderSchema.Constants.AG_Column], sectionCode);
			AssertEquals("Min", bizObj["Min"], minAccount.AG_AccountNum);
			AssertEquals("Max", bizObj["Max"], maxAccount.AG_AccountNum);
		}

		void SetupAG_ColumnToEmptyForAllGLHeaders()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccGLHeaderCollection gLHeaders = new AccGLHeaderCollection(newFactory);
			gLHeaders.Load();

			foreach (AccGLHeader header in gLHeaders)
			{
				header.AG_Column = ZString.Empty;
			}

			newFactory.Save();
		}

		#region Implementation

		#region Has Error Assertions

		void AssertHasErrorForStartInfos(ZString error)
		{
			AssertHasError(Sections.AG_TradingStatementStartAccountInfo, error);
			AssertHasError(Sections.AG_OverheadsStartAccountInfo, error);
			AssertHasError(Sections.AG_ProfitAndLossAppropriationStartAccountInfo, error);
			AssertHasError(Sections.AG_OwnersEquityStartAccountInfo, error);
			AssertHasError(Sections.AG_AssetsStartAccountInfo, error);
			AssertHasError(Sections.AG_LiabilitiesStartAccountInfo, error);
		}

		void AssertHasErrorForEndInfos(ZString error)
		{
			AssertHasError(Sections.AG_TradingStatementEndAccountInfo, error);
			AssertHasError(Sections.AG_OverheadsEndAccountInfo, error);
			AssertHasError(Sections.AG_ProfitAndLossAppropriationEndAccountInfo, error);
			AssertHasError(Sections.AG_OwnersEquityEndAccountInfo, error);
			AssertHasError(Sections.AG_AssetsEndAccountInfo, error);
			AssertHasError(Sections.AG_LiabilitiesEndAccountInfo, error);
		}

		#endregion

		#region No Error Assertions

		void AssertNoErrorForAllInfos(ZString error)
		{
			AssertNoErrorForProfitAndLossInfos(error);
			AssertNoErrorForBalanceSheetInfos(error);
		}

		void AssertNoErrorForProfitAndLossInfos(ZString error)
		{
			AssertNoError(Sections.AG_TradingStatementStartAccountInfo, error);
			AssertNoError(Sections.AG_TradingStatementEndAccountInfo, error);
			AssertNoError(Sections.AG_OverheadsStartAccountInfo, error);
			AssertNoError(Sections.AG_OverheadsEndAccountInfo, error);
			AssertNoError(Sections.AG_ProfitAndLossAppropriationStartAccountInfo, error);
			AssertNoError(Sections.AG_ProfitAndLossAppropriationEndAccountInfo, error);
		}

		void AssertNoErrorForBalanceSheetInfos(ZString error)
		{
			AssertNoError(Sections.AG_OwnersEquityStartAccountInfo, error);
			AssertNoError(Sections.AG_OwnersEquityEndAccountInfo, error);
			AssertNoError(Sections.AG_AssetsStartAccountInfo, error);
			AssertNoError(Sections.AG_AssetsEndAccountInfo, error);
			AssertNoError(Sections.AG_LiabilitiesStartAccountInfo, error);
			AssertNoError(Sections.AG_LiabilitiesEndAccountInfo, error);
		}

		void AssertNoErrorForStartInfos(ZString error)
		{
			AssertNoError(Sections.AG_TradingStatementStartAccountInfo, error);
			AssertNoError(Sections.AG_OverheadsStartAccountInfo, error);
			AssertNoError(Sections.AG_ProfitAndLossAppropriationStartAccountInfo, error);
			AssertNoError(Sections.AG_OwnersEquityStartAccountInfo, error);
			AssertNoError(Sections.AG_AssetsStartAccountInfo, error);
			AssertNoError(Sections.AG_LiabilitiesStartAccountInfo, error);
		}

		void AssertNoErrorForEndInfos(ZString error)
		{
			AssertNoError(Sections.AG_TradingStatementEndAccountInfo, error);
			AssertNoError(Sections.AG_OverheadsEndAccountInfo, error);
			AssertNoError(Sections.AG_ProfitAndLossAppropriationEndAccountInfo, error);
			AssertNoError(Sections.AG_OwnersEquityEndAccountInfo, error);
			AssertNoError(Sections.AG_AssetsEndAccountInfo, error);
			AssertNoError(Sections.AG_LiabilitiesEndAccountInfo, error);
		}

		#endregion

		void SetValues(ZGuid tSStart, ZGuid tSEnd, ZGuid oVStart, ZGuid oVEnd, ZGuid aPStart, ZGuid aPEnd, ZGuid oEStart, ZGuid oEEnd, ZGuid aSStart, ZGuid aSEnd, ZGuid lIStart, ZGuid lIEnd)
		{
			Sections.AG_TradingStatementStartAccount = tSStart;
			Sections.AG_TradingStatementEndAccount = tSEnd;

			Sections.AG_OverheadsStartAccount = oVStart;
			Sections.AG_OverheadsEndAccount = oVEnd;

			Sections.AG_ProfitAndLossAppropriationStartAccount = aPStart;
			Sections.AG_ProfitAndLossAppropriationEndAccount = aPEnd;

			Sections.AG_OwnersEquityStartAccount = oEStart;
			Sections.AG_OwnersEquityEndAccount = oEEnd;

			Sections.AG_AssetsStartAccount = aSStart;
			Sections.AG_AssetsEndAccount = aSEnd;

			Sections.AG_LiabilitiesStartAccount = lIStart;
			Sections.AG_LiabilitiesEndAccount = lIEnd;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Sections = new AccGLHeaderBulkUpdater(Factory);

			Acc1000 = LoadExistingOrCreateNewForTest("1000.00.00");
			Acc1900 = LoadExistingOrCreateNewForTest("1900.00.00");
			Acc2010 = LoadExistingOrCreateNewForTest("2010.00.00");
			Acc4899 = LoadExistingOrCreateNewForTest("4899.00.00");
			Acc4900 = LoadExistingOrCreateNewForTest("4900.00.00");
			Acc4999 = LoadExistingOrCreateNewForTest("4999.00.00");
			Acc5000 = LoadExistingOrCreateNewForTest("5000.00.00");
			Acc5900 = LoadExistingOrCreateNewForTest("5900.00.00");
			Acc6000 = LoadExistingOrCreateNewForTest("6000.00.00");
			Acc7999 = LoadExistingOrCreateNewForTest("7999.00.00");
			Acc8000 = LoadExistingOrCreateNewForTest("8000.00.00");
			Acc9999 = LoadExistingOrCreateNewForTest("9999.00.00");
		}

		AccGLHeader LoadExistingOrCreateNewForTest(ZString accountNumber)
		{
			BusinessObjectFactory factoryForCreation = new BusinessObjectFactory();

			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountNum, accountNumber);
			AccGLHeader header = factoryForCreation.LoadTop1<AccGLHeader>(query);

			if (header == null)
			{
				header = factoryForCreation.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = accountNumber;
				factoryForCreation.Save();
			}

			return header;
		}

		AccGLHeaderBulkUpdater Sections;

		AccGLHeader Acc1000;
		AccGLHeader Acc1900;
		AccGLHeader Acc2010;
		AccGLHeader Acc4899;
		AccGLHeader Acc4900;
		AccGLHeader Acc4999;
		AccGLHeader Acc5000;
		AccGLHeader Acc5900;
		AccGLHeader Acc6000;
		AccGLHeader Acc7999;
		AccGLHeader Acc8000;
		AccGLHeader Acc9999;

		ZGuid Empty
		{
			get { return ZGuid.Empty; }
		}

		#endregion
	}
}
