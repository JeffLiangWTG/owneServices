using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionLineDissectionAttributeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAttributeValueList()
		{
			var accTransactionLineDissectionAttribute = Factory.NewWithValidTestData<AccTransactionLineDissectionAttribute>();
			var lookups = new AccTransactionLineDissectionAttributeLookups(accTransactionLineDissectionAttribute);

			accTransactionLineDissectionAttribute.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			AssertEquals(lookups.AttributeValueList.Count, AccountingMasterFilesConstants.OCGList.Count);
			AssertContainsExactElementsInAnyOrder(lookups.AttributeValueList, AccountingMasterFilesConstants.OCGList);
			AssertEquals(3, AccountingMasterFilesConstants.OCGList.Count);
			Assert(AccountingMasterFilesConstants.OCGList.ContainsCode(AccountingMasterFilesConstants.NAV.Code));

			accTransactionLineDissectionAttribute.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC;
			AssertEquals(lookups.AttributeValueList.Count, AccountingMasterFilesConstants.TICList.Count);
			AssertContainsExactElementsInAnyOrder(lookups.AttributeValueList, AccountingMasterFilesConstants.TICList);
			AssertEquals(3, AccountingMasterFilesConstants.TICList.Count);
			Assert(AccountingMasterFilesConstants.TICList.ContainsCode(AccountingMasterFilesConstants.NAV.Code));

			accTransactionLineDissectionAttribute.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE;
			AssertEquals(lookups.AttributeValueList.Count, AccountingMasterFilesConstants.LFEList.Count);
			AssertContainsExactElementsInAnyOrder(lookups.AttributeValueList, AccountingMasterFilesConstants.LFEList);
			AssertEquals(4, AccountingMasterFilesConstants.LFEList.Count);
			Assert(AccountingMasterFilesConstants.LFEList.ContainsCode(AccountingMasterFilesConstants.NAV.Code));

			accTransactionLineDissectionAttribute.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
			AssertEquals(lookups.AttributeValueList.Count, AccountingMasterFilesConstants.LFOList.Count);
			AssertContainsExactElementsInAnyOrder(lookups.AttributeValueList, AccountingMasterFilesConstants.LFOList);
			AssertEquals(3, AccountingMasterFilesConstants.LFOList.Count);
			Assert(AccountingMasterFilesConstants.LFOList.ContainsCode(AccountingMasterFilesConstants.NAV.Code));

			accTransactionLineDissectionAttribute.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;
			AssertEquals(lookups.AttributeValueList.Count, AccountingMasterFilesConstants.SPRList.Count);
			AssertContainsExactElementsInAnyOrder(lookups.AttributeValueList, AccountingMasterFilesConstants.SPRList);
			AssertEquals(3, AccountingMasterFilesConstants.SPRList.Count);
			Assert(AccountingMasterFilesConstants.SPRList.ContainsCode(AccountingMasterFilesConstants.NAV.Code));
		}

		public void TestAttributeValueIDCollection()
		{
			var aRControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			aRControlAccount.AG_AccountNum = "10.00.1010";
			aRControlAccount.AG_AccountType = "BSH";
			aRControlAccount.AG_DebitCredit = "DR";
			aRControlAccount.AG_Column = "OV";
			aRControlAccount.AG_CashFlowType = "XXX";
			aRControlAccount.AG_StatisticalUnits = "KG";

			var aPControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			aPControlAccount.AG_AccountNum = "20.00.1010";
			aPControlAccount.AG_AccountType = "BSH";
			aPControlAccount.AG_DebitCredit = "CR";
			aPControlAccount.AG_Column = "OV";
			aPControlAccount.AG_CashFlowType = "XXX";
			aPControlAccount.AG_StatisticalUnits = "KG";
			Factory.Save();

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ARControlAccount).Returns(aRControlAccount.PK.ToGuid());
			mock.Setup(m => m.APControlAccount).Returns(aPControlAccount.PK.ToGuid());

			using (ObjectFactory.Substitute(mock.Object))
			{
				var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				var accTransactionLineDissectionAttribute = Factory.NewWithValidTestData<AccTransactionLineDissectionAttribute>();
				var lookups = new AccTransactionLineDissectionAttributeLookups(accTransactionLineDissectionAttribute);

				accTransactionLineDissectionAttribute.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG;
				accTransactionLineDissectionAttribute.ALD_AL_TransactionLine = transactionLine.PK;

				transactionLine.AL_AG = aRControlAccount.PK;
				AssertEquals(GetDebtorCreditorOrgCount(true), lookups.AttributeValueIDCollection.Count);

				transactionLine.AL_AG = aPControlAccount.PK;
				AssertEquals(GetDebtorCreditorOrgCount(false), lookups.AttributeValueIDCollection.Count);

				var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
				transactionLine.AL_AG = glHeader.PK;
				AssertEquals(0, lookups.AttributeValueIDCollection.Count);
			}
		}

		int GetDebtorCreditorOrgCount(bool isDebtor)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			var fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			fromAccountFilter.AddToFilter(isDebtor ? OrgCompanyDataSchema.OB_IsDebtor : OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(fromAccountFilter);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return Factory.Load<OrgHeader>(query).Length;
		}

		public void TestAlternateGLAccountAttributeCodeWithID()
		{
			var accTransactionLineDissectionAttribute = Factory.NewWithValidTestData<AccTransactionLineDissectionAttribute>();
			var lookups = new AccTransactionLineDissectionAttributeLookups(accTransactionLineDissectionAttribute);

			AssertEquals(1, lookups.AlternateGLAccountAttributeCodeWithID.Length);
			AssertEquals(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, lookups.AlternateGLAccountAttributeCodeWithID[0]);
		}
	}
}
