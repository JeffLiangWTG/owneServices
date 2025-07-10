using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDebtorGroupBankDefault))]
	sealed class OrgDebtorGroupBankDefaultTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			OrgDebtorGroupBankDefault test = Factory.New<OrgDebtorGroupBankDefault>();
			AssertEquals("GC is set to current company", GlbCompany.CurrentCompany.PK, test.P6_GC);
		}

		public void TestOnSaving()
		{
			OrgDebtorGroupBankDefault test = Factory.NewWithValidTestData<OrgDebtorGroupBankDefault>();
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();
			test.P6_AB = account.PK;
			Factory.Save();
			Assert("Object must not be deleted", !test.IsDeleted);

			test.P6_AB = Guid.Empty;
			Factory.Save();
			Assert("Object must be deleted", test.IsDeleted);
		}

		public void TestUniqueIndexErrorHandling()
		{
			BusinessObjectFactory creationFactory = new BusinessObjectFactory();
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			creationFactory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			OrgDebtorGroup group = creationFactory.New<OrgDebtorGroup>();
			group.OJ_Code = "dfg";
			group.OJ_Desc = "dfg desc";
			AccBankAccount bankAccount1 = creationFactory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bankAccount2 = creationFactory.NewWithValidTestData<AccBankAccount>();
			creationFactory.Save();

			OrgDebtorGroup group1 = factory1.Load<OrgDebtorGroup>(group.PK);
			OrgDebtorGroup group2 = factory2.Load<OrgDebtorGroup>(group.PK);

			group1.DefaultBankAccountPK = bankAccount1.PK;
			group2.DefaultBankAccountPK = bankAccount2.PK;

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("Factory 2 save should not have succeeded");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);

				Assert("User should have been notified of the problem.", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("User should have been notified of the problem.", "While you were working with this Debtor Group, another user has changed the bank account. Press the Save button to try and save your changes again",
					UnitTestUserNotification.Instance.LastMessage.Text);
				factory2.Save();
			}

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();
			OrgDebtorGroup reloadedGroup = newFactoryForLoad.Load<OrgDebtorGroup>(group.PK);
			AssertEquals("Group BankAccount PK should be set correctly", bankAccount2.PK, reloadedGroup.DefaultBankAccountPK);
		}
	}
}
