using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCollectionCall))]
	sealed class OrgCollectionCallTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestCallsBaseSetDefaultValues()
		{
			Assert("SetDefaultValues is never called - this BizObj is based on a view.", true);
		}

		public override void TestBizObjectFields()
		{
			Assert("This BizObj is based on a view, these BusinessObject fields are always readonly and will never be saved.", true);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("You can't save this and you can't delete it - its based on a view.", true);
		}

		#region Properties

		public void TestGetCollectionNotes()
		{
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("CollectionCall CollectionNotes should contain CollectionNote.", CollectionNote, collectionCalls[0].Header.CollectionNotes[0]);
		}

		public void TestGetHeader()
		{
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("Header should be the same as the one used for initalization.", Org.PK, collectionCalls[0].Header.PK);
		}

		public void TestAdditionalCompanyName()
		{
			var expectedAdditionalCompanyName = OrgHeaderUnicodeTestConstants.ChineseCompanyName1;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				Org.OH_IsDebtor = true;
				Org.Addresses.RemoveAndDeleteAll();

				var orgAddress = Org.Addresses.AddNew();
				orgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
				orgAddress.Language = SharedConstants.Languages.ChineseSimplified;
				orgAddress.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
				orgAddress.CompanyName = expectedAdditionalCompanyName;
				orgAddress.AddressCapability.SetCapabilityEnabled("ARM");
				orgAddress.AddressCapability.SetIsMainAddress("ARM");

				Factory.Save();

				OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
				collectionCalls.Load();

				AssertEquals("The additional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, collectionCalls[0].AdditionalCompanyName);
			}
		}

		[TestDate(2024, 3, 1)]
		public void TestCC_AvgDueDate()
		{
			AssertEquals("PreCondition", new ZDateTime(2024, 3, 1), ZDateTime.Now);
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CompanyData.OB_IsDebtor = true;
			organisation.CompanyData.OB_AROnCreditHold = true;

			Factory.Save();

			PostTransaction("1000014", new DateTime(2024, 2, 27));
			PostTransaction("1000015", new DateTime(2024, 2, 20));

			AssertEquals("CC_AvgDueDate", new ZDateTime(2024, 02, 23, 12, 00, 00), LoadCollectionCall().CC_AvgDueDate);
			System.Threading.Tasks.Task.Delay(20000).Wait();
			AssertEquals("CC_AvgDueDate", new ZDateTime(2024, 02, 23, 12, 00, 00), LoadCollectionCall().CC_AvgDueDate);
			System.Threading.Tasks.Task.Delay(11000).Wait();
			AssertEquals("CC_AvgDueDate", new ZDateTime(2024, 02, 23, 12, 00, 00), LoadCollectionCall().CC_AvgDueDate);

			OrgCollectionCall LoadCollectionCall()
			{
				var query = new ZDBOnlyQuery(typeof(OrgCollectionCall));
				query.AddToFilter(new ZQuery(vw_OrgCollectionCallSchema.CC_GC, GlbBranch.CurrentBranch.GB_GC));
				query.AddToFilter(new ZQuery(vw_OrgCollectionCallSchema.CC_OH, organisation.PK));
				return Factory.LoadTop1<OrgCollectionCall>(query);
			}

			void PostTransaction(string transactionNum, ZDateTime dueDate)
			{
				TestHelper.PostTransaction("AR", "INV", transactionNum, 100m, 50m, 200m, GlbBranch.CurrentBranch.PK.ToGuid(), ZDateTime.Now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), dueDate, GlbDepartment.CurrentDepartment.PK.ToGuid(), ZDateTime.Empty);
			}
		}

		#region TestCC_AvgDaysOverdue

		[TestDate(2024, 3, 1)]
		public void TestCC_AvgDaysOverdue()
		{
			GlbBranch branch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			OrgDebtorGroup debtorGroup = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader organisation = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch, debtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg2");
			OrgContact contact = TestHelper.GetNewContact(organisation, "Official", "123");
			OrgDocument document = TestHelper.GetNewOrgDocument(contact.PK, ZBool.True, "A/R");

			ZDateTime now = ZDateTime.Now;

			TestHelper.AddCollectionNote(organisation, contact.PK, now.Date.AddDays(-5), now.Date.AddDays(-5), CollectionNoteDispositionList.Codes.PaymentReceived, "Again. Very good call.");

			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			TestHelper.PostTransaction("AR", "INV", "1000014", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-10), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "INV", "1000015", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-20), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "INV", "1000016", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-40), department.PK.ToGuid(), ZDateTime.Empty);

			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			ZInt index = (collectionCalls[0].PK == organisation.CompanyData.PK) ? 0 : 1;
			OrgCollectionCall collectionCall = collectionCalls[index];

			Assert("collectionCall.CC_AvgDaysOverdue = 23, was " + collectionCall.CC_AvgDaysOverdue.ToString(), collectionCall.CC_AvgDaysOverdue == 23);
		}

		[TestDate(2078, 06, 06)]
		public void TestCC_AvgDaysOverdueArithmeticOverflowError()
		{
			GlbBranch branch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			OrgDebtorGroup debtorGroup = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader organisation = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch, debtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg2");
			OrgContact contact = TestHelper.GetNewContact(organisation, "Official", "123");
			OrgDocument document = TestHelper.GetNewOrgDocument(contact.PK, ZBool.True, "A/R");

			ZDateTime now = ZDateTime.Now;

			TestHelper.AddCollectionNote(organisation, contact.PK, now.Date.AddDays(-5), now.Date.AddDays(-5), CollectionNoteDispositionList.Codes.PaymentReceived, "Again. Very good call.");

			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();
			for (int i = 0; i < 100; i++)
			{
				var transactionNum = 1000014 + i;
				TestHelper.PostTransaction("AR", "INV", transactionNum.ToString(), 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), ZDateTime.MinSmallDateTimeValue, department.PK.ToGuid(), ZDateTime.Empty);
			}

			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			AssertNoExceptionThrown("Should not have ArithmeticOverflowError error.", () => collectionCalls.Load());
		}

		#endregion

		#region TestCC_MaxDaysOverdue

		[TestDate(2024, 3, 1)]
		public void TestCC_MaxDaysOverdue()
		{
			GlbBranch branch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			OrgDebtorGroup debtorGroup = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader organisation = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch, debtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg2");
			OrgContact contact = TestHelper.GetNewContact(organisation, "Official", "123");
			OrgDocument document = TestHelper.GetNewOrgDocument(contact.PK, ZBool.True, "A/R");

			ZDateTime now = ZDateTime.Now;

			TestHelper.AddCollectionNote(organisation, contact.PK, now.Date.AddDays(-5), now.Date.AddDays(-5), CollectionNoteDispositionList.Codes.PaymentReceived, "Again. Very good call.");

			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			TestHelper.PostTransaction("AR", "INV", "1000014", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-3), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "INV", "1000015", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-10), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "INV", "1000016", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-50), department.PK.ToGuid(), ZDateTime.Empty);

			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			ZInt index = (collectionCalls[0].PK == organisation.CompanyData.PK) ? 0 : 1;
			OrgCollectionCall collectionCall = collectionCalls[index];

			Assert("collectionCall.CC_MaxDaysOverdue = 50, was " + collectionCall.CC_MaxDaysOverdue.ToString(), collectionCall.CC_MaxDaysOverdue == 50);
		}

		#endregion

		#endregion

		public void TestZDecimalsHaveCorrectDecimalPlacesOrgCollection()
		{
			var calls = Factory.New<OrgCollectionCall>();

			var localList = new List<string>
			{
				nameof(calls.CC_AvailableCredit),
				nameof(calls.CC_CreditLimit),
				nameof(calls.CC_DisbursementOutstandingAmount),
				nameof(calls.CC_DisbursementOverdueAmount),
				nameof(calls.CC_LastReceiptAmount),
				nameof(calls.CC_StandardOutstandingAmount),
				nameof(calls.CC_StandardOverdueAmount),
				nameof(calls.CC_TotalOutstandingAmount),
				nameof(calls.CC_TotalOverdueAmount)
			};

			var tester = new DecimalPlacesAttributeTester(calls);
			tester.CheckLocalCurrency(localList, nameof(calls.DecimalPlaces));
		}

		public void TestDecimalPlaces()
		{
			ZInt defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				TestHelper.AddCollectionNote(Org, Org.Contacts[0].PK, ZDateTime.Now.Date.AddDays(5), ZDateTime.Now.AddDays(15), CollectionNoteDispositionList.Codes.PaymentReceived, "Very Good Call");

				OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
				Factory.Save();
				collectionCalls.Load();

				AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				AssertEquals("Decimals should be 0", 0, collectionCalls[0].DecimalPlaces);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				AssertEquals("Decimals should be 2", 2, collectionCalls[0].DecimalPlaces);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		#region Test Related Objects

		public void TestOrgHeaderIsLoadedFromCollectionCall()
		{
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("Header should be the same as the one used for initalization.", true, collectionCalls[0].Header.IsLoadedFromCollectionCall);
		}

		public void TestDependentContacts()
		{
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("DependentContacts collection should equal to the collection from the Header.", Org.Contacts, collectionCalls[0].Lookups.Contacts);
		}

		public void TestRelatedOrgHeader()
		{
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("Related OrgHeader should equal to the OrgHeader used for creation of Collection Call", Org, collectionCalls[0].Header);
		}

		#endregion

		#region Tests for the view vw_OrgCollectionCall

		public void TestViewGeneralParameters()
		{
			TestHelper.AddCollectionNote(Org, Org.Contacts[0].PK, ZDateTime.Now.Date.AddDays(5), ZDateTime.Now.AddDays(15), CollectionNoteDispositionList.Codes.PaymentReceived, "Very Good Call");
			TestHelper.AddCompanyARTerms(Org);

			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			Factory.Save();
			collectionCalls.Load();

			AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
			AssertEquals(Org.CompanyData.PK, collectionCalls[0].PK);
			AssertEquals(Org.PK, collectionCalls[0].CC_OH);
			AssertEquals(Org.OH_FullName, collectionCalls[0].CC_OH_FullName);
			AssertEquals(Org.CompanyData.OB_GC, collectionCalls[0].CC_GC);
			AssertEquals(Org.OH_Code, collectionCalls[0].CC_DebtorCode);
			AssertEquals(TestOfficialContact.PK, collectionCalls[0].CC_OC);
			AssertEquals(TestOfficialContact.OC_Phone, collectionCalls[0].CC_OC_Phone);
			AssertEquals(Org.CompanyData.OB_ARCreditLimit, collectionCalls[0].CC_CreditLimit);
			AssertEquals(Org.CompanyData.OB_ARCreditLimit, collectionCalls[0].CC_AvailableCredit);
			AssertEquals(0m, collectionCalls[0].CC_TotalOutstandingAmount);
			AssertEquals(0m, collectionCalls[0].CC_TotalOverdueAmount);
			AssertEquals(0, collectionCalls[0].CC_MaxDaysOverdue);
			AssertEquals(0, collectionCalls[0].CC_AvgDaysOverdue);
			AssertEquals(CollectionNote.PN_SystemCreateTimeUtc.Date, collectionCalls[0].CC_LastCallDate.Date);
			AssertEquals(ZDateTime.Now.Date.AddDays(5), collectionCalls[0].CC_NextCallDate);
			AssertEquals(CollectionNote.PN_CallBackDate, collectionCalls[0].CC_NextFollowUpDate);
			AssertEquals(ZDateTime.Empty, collectionCalls[0].CC_LastReceiptDate);
			AssertEquals(0m, collectionCalls[0].CC_LastReceiptAmount);
			AssertEquals(TestDebtorGroup.PK, collectionCalls[0].CC_OB_OJ_ARDebtorGroup);
			AssertEquals(DefaultBranch.PK, collectionCalls[0].CC_OB_GB_ControllingBranch);
			AssertEquals("AAA", collectionCalls[0].CC_OB_ARCategory);
			AssertEquals("BBB", collectionCalls[0].CC_OB_ARConsolidatedAccountingCategory);
			AssertEquals("CCC", collectionCalls[0].CC_OB_ARCreditRating);
			AssertEquals(ZBool.True, collectionCalls[0].CC_OB_AROnCreditHold);

			Org.CompanyData.OB_AROnCreditHold = false;
			Org.CompanyData.OB_ARCreditApproved = false;
			Factory.Save();
			collectionCalls = new OrgCollectionCallCollection(new BusinessObjectFactory());
			collectionCalls.Load();
			AssertEquals("COD", collectionCalls[0].CC_OB_ARInvoiceTerms);
			AssertEquals("COD", collectionCalls[0].CC_OB_ARDisbursementInvoiceTerms);

			Org.CompanyData.OB_ARCreditApproved = true;
			Factory.Save();
			collectionCalls = new OrgCollectionCallCollection(new BusinessObjectFactory());
			collectionCalls.Load();
			AssertEquals("10/INV", collectionCalls[0].CC_OB_ARInvoiceTerms);
			AssertEquals("10/INV, (DSB)->10/INV", collectionCalls[0].CC_OB_ARDisbursementInvoiceTerms);
		}

		public void TestCreditLimit()
		{
			// This test doesn't use a TestDate attribute because we are testing date logic within SQL Server, and
			// setting that attribute doesn't change the date in SQL Server.

			var branch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			var debtorGroup = TestHelper.GetNewOrgDebtorGroup();
			var organisation = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch, debtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg2");
			var now = DateTime.Now;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			TestHelper.PostTransaction("AR", "INV", "1000016", 100m, 10m, 20m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code,
				"CSH", Org.PK.ToGuid(), now.AddDays(-1), department.PK.ToGuid(), ZDateTime.Empty);

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(Org.CompanyData, 99m, 0m);
			Factory.Save();
			var collectionCalls = new OrgCollectionCallCollection(new BusinessObjectFactory());
			collectionCalls.Load();
			AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
			AssertEquals(99m, collectionCalls[0].CC_CreditLimit);
			AssertEquals(79m, collectionCalls[0].CC_AvailableCredit);
			AssertEquals(20m, collectionCalls[0].CC_TotalOutstandingAmount);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(Org.CompanyData, 99m, 3m);
			Factory.Save();
			collectionCalls = new OrgCollectionCallCollection(new BusinessObjectFactory());
			collectionCalls.Load();
			AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
			AssertEquals(102m, collectionCalls[0].CC_CreditLimit);
			AssertEquals(82m, collectionCalls[0].CC_AvailableCredit);
			AssertEquals(20m, collectionCalls[0].CC_TotalOutstandingAmount);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(Org.CompanyData, 0m, 17m);
			Factory.Save();
			collectionCalls = new OrgCollectionCallCollection(new BusinessObjectFactory());
			collectionCalls.Load();
			AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
			AssertEquals(17m, collectionCalls[0].CC_CreditLimit);
			AssertEquals(-3m, collectionCalls[0].CC_AvailableCredit);
			AssertEquals(20m, collectionCalls[0].CC_TotalOutstandingAmount);
		}

		[TestDate(2024, 3, 1)]
		public void TestViewAmounts()
		{
			GlbBranch branch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			OrgDebtorGroup debtorGroup = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader organisation = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch, debtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg2");
			OrgContact contact = TestHelper.GetNewContact(organisation, "Official", "123");
			OrgDocument document = TestHelper.GetNewOrgDocument(contact.PK, ZBool.True, "A/R");

			ZDateTime now = ZDateTime.Now;

			TestHelper.AddCollectionNote(organisation, contact.PK, now.Date.AddDays(-5), now.Date.AddDays(-5), CollectionNoteDispositionList.Codes.PaymentReceived, "Again. Very good call.");

			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			ZGuid receiptPK = TestHelper.PostTransaction("AR", "REC", "1000014", -100m, -50m, -200m, branch.PK.ToGuid(), now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.Date, department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AP", "REC", "1000015", -100m, -50m, -200m, branch.PK.ToGuid(), now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-1), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "INV", "1000016", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-1), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "INV", "1000017", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddMinutes(5), department.PK.ToGuid(), now);
			TestHelper.PostTransaction("AR", "INV", "1000018", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, true, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-1), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "CRD", "1000019", -100m, -50m, -200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-5), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "ADJ", "1000020", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-1), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "JNL", "1000021", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-1), department.PK.ToGuid(), ZDateTime.Empty);
			// Invoice Batches should be ignored in the database query
			TestHelper.PostTransaction("AR", "INB", "1000022", 100m, 50m, 200m, branch.PK.ToGuid(), now, Guid.Empty, true, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(-1), department.PK.ToGuid(), ZDateTime.Empty);

			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("There should be 2 Collection Calls in the collection.", 2, collectionCalls.Count);

			ZInt index = (collectionCalls[0].PK == organisation.CompanyData.PK) ? 0 : 1;
			OrgCollectionCall collectionCall = collectionCalls[index];
			AssertEquals("collectionCall.CC_AvailableCredit", -300m, collectionCall.CC_AvailableCredit);
			AssertEquals("collectionCall.CC_TotalOutstandingAmount", 400m, collectionCall.CC_TotalOutstandingAmount);
			AssertEquals("collectionCall.CC_TotalOverdueAmo	unt", 400m, collectionCall.CC_TotalOverdueAmount);

			Assert("collectionCall.CC_MaxDaysOverdue = 5 or 6, was " + collectionCall.CC_MaxDaysOverdue.ToString(), collectionCall.CC_MaxDaysOverdue == 5 || collectionCall.CC_MaxDaysOverdue == 6);
			Assert("collectionCall.CC_AvgDaysOverdue = 1 or 2, was " + collectionCall.CC_AvgDaysOverdue.ToString(), collectionCall.CC_AvgDaysOverdue == 1 || collectionCall.CC_AvgDaysOverdue == 2);

			AssertEquals("collectionCall.CC_LastCallDate", now.Date.AddDays(-5), collectionCall.CC_LastCallDate);
			AssertEquals("collectionCall.CC_NextCallDate", ZDateTime.Empty, collectionCall.CC_NextCallDate);
			AssertEquals("collectionCall.CC_NextFollowUpDate", ZDateTime.Empty, collectionCall.CC_NextFollowUpDate);

			AccTransactionHeader receipt = Factory.Load<AccTransactionHeader>(receiptPK);
			AssertEquals("collectionCall.CC_LastReceiptDate", receipt.AH_InvoiceDate, collectionCall.CC_LastReceiptDate);
			AssertEquals("collectionCall.CC_LastReceiptAmount", 150m, collectionCall.CC_LastReceiptAmount);
		}

		public void TestAllPercentOverDues_NoArithmeticOverflowException_WhenTransactionCategoryIsNotDBT()
		{
			var branch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			var debtorGroup = TestHelper.GetNewOrgDebtorGroup();
			var organisation = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch, debtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg2");

			var now = ZDateTime.Now;
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			const decimal biggestValueOfSQLMoneyType = 922337203685477.5807m;
			const decimal smallestValueOfSQLMoneyType = 0.0001m;
			var outstandingAmountForInvoice = biggestValueOfSQLMoneyType;
			var outstandingAmountForCreditNote = smallestValueOfSQLMoneyType - biggestValueOfSQLMoneyType;
			AssertEquals("Precondition: total outstanding amount", smallestValueOfSQLMoneyType, outstandingAmountForInvoice + outstandingAmountForCreditNote);

			TestHelper.PostTransaction("AR", "INV", "1000018", 100m, 50m, outstandingAmountForInvoice, branch.PK.ToGuid(), now.Date.AddDays(-6), Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.Date.AddDays(-5), department.PK.ToGuid(), ZDateTime.Empty);
			TestHelper.PostTransaction("AR", "CRD", "1000019", -100m, -50m, outstandingAmountForCreditNote, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(50), department.PK.ToGuid(), ZDateTime.Empty);

			var collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();

			AssertEquals("There should be 2 Collection Calls in the collection.", 2, collectionCalls.Count);

			var result = collectionCalls.Cast<OrgCollectionCall>().First(x => x.CC_PercentOverdue != 0);
			AssertEquals("Total Percent OverDue", 922337203685477580700m, result.CC_PercentOverdue);
			AssertEquals("Standard Percent OverDue", 922337203685477580700m, result.CC_StandardPercentOverdue);
			AssertEquals("Disbursement Percent OverDue", 0m, result.CC_DisbursementPercentOverdue);
		}

		public void TestAllPercentOverDues_NoArithmeticOverflowException_WhenTransactionCategoryIsDBT()
		{
			var branch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			var debtorGroup = TestHelper.GetNewOrgDebtorGroup();
			var organisation = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch, debtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg2");

			var now = ZDateTime.Now;
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			const decimal biggestValueOfSQLMoneyType = 922337203685477.5807m;
			const decimal smallestValueOfSQLMoneyType = 0.0001m;
			var outstandingAmountForInvoice = biggestValueOfSQLMoneyType;
			var outstandingAmountForCreditNote = smallestValueOfSQLMoneyType - biggestValueOfSQLMoneyType;
			AssertEquals("Precondition: total outstanding amount", smallestValueOfSQLMoneyType, outstandingAmountForInvoice + outstandingAmountForCreditNote);

			TestHelper.PostTransaction("AR", "INV", "1000018", 100m, 50m, outstandingAmountForInvoice, branch.PK.ToGuid(), now.Date.AddDays(-6), Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.Date.AddDays(-5), department.PK.ToGuid(), ZDateTime.Empty, "DBT");
			TestHelper.PostTransaction("AR", "CRD", "1000019", -100m, -50m, outstandingAmountForCreditNote, branch.PK.ToGuid(), now, Guid.Empty, false, TestCurrency.RX_Code, "CSH", organisation.PK.ToGuid(), now.AddDays(50), department.PK.ToGuid(), ZDateTime.Empty, "DBT");

			var collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();

			AssertEquals("There should be 2 Collection Calls in the collection.", 2, collectionCalls.Count);

			var result = collectionCalls.Cast<OrgCollectionCall>().First(x => x.CC_PercentOverdue != 0);
			AssertEquals("Total Percent OverDue", 922337203685477580700m, result.CC_PercentOverdue);
			AssertEquals("Standard Percent OverDue", 0m, result.CC_StandardPercentOverdue);
			AssertEquals("Disbursement Percent OverDue", 922337203685477580700m, result.CC_DisbursementPercentOverdue);
		}

		public void TestViewDebtorWithNoOverdueAmounts()
		{
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(Org.CompanyData, 100m, 0m);
			Factory.Save();
			var collectionCalls = new OrgCollectionCallCollection(new BusinessObjectFactory());
			collectionCalls.Load();
			AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
			AssertEquals("Credit limit value", 100m, collectionCalls[0].CC_CreditLimit);
			AssertEquals("Available credit value", 100m, collectionCalls[0].CC_AvailableCredit);
			AssertEquals("Outstanding amount value", 0m, collectionCalls[0].CC_TotalOutstandingAmount);
		}

		public void TestViewOrganisationNotDebtorAndDoesntHaveNotesAndTransactions()
		{
			GlbBranch branch3 = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			OrgDebtorGroup debtorGroup3 = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader org3 = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch3, debtorGroup3, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg3");
			OrgContact officialContact3 = TestHelper.GetNewContact(org3, "Official", "123");
			OrgDocument document2 = TestHelper.GetNewOrgDocument(officialContact3.PK, ZBool.True, "A/R");

			Factory.Save();
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
		}

		public void TestViewDebtorOrganisationIsPresentInResults()
		{
			GlbBranch branch4 = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			OrgDebtorGroup debtorGroup4 = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader org4 = TestHelper.GetNewOrganisationWithCompanyData(true, DefaultGlbCompany.PK, branch4, debtorGroup4, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg4");
			OrgContact officialContact4 = TestHelper.GetNewContact(org4, "Official", "123");
			OrgDocument document3 = TestHelper.GetNewOrgDocument(officialContact4.PK, ZBool.True, "A/R");

			Factory.Save();
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("There should be 2 Collection Calls in the collection", 2, collectionCalls.Count);
		}

		public void TestViewOrganisationFromAnotherGlbCompanyCouldntBeSeen()
		{
			GlbCompany testGlbCompany = TestHelper.GetNewGlbCompany("Test Company");
			GlbBranch branch5 = TestHelper.GetNewBranch(testGlbCompany.PK);
			OrgDebtorGroup debtorGroup5 = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader org5 = TestHelper.GetNewOrganisationWithCompanyData(true, testGlbCompany.PK, branch5, debtorGroup5, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg5");
			OrgContact officialContact5 = TestHelper.GetNewContact(org5, "Official", "123");
			OrgDocument document4 = TestHelper.GetNewOrgDocument(officialContact5.PK, ZBool.True, "A/R");

			Factory.Save();
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			collectionCalls.Load();
			AssertEquals("There should be 1 Collection Call in the collection", 1, collectionCalls.Count);
		}

		public void TestViewOrganisationWithAtLeast1ReceiptWillBeInResults()
		{
			GlbBranch branch6 = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			OrgDebtorGroup debtorGroup6 = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader org6 = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, branch6, debtorGroup6, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg6");
			OrgContact officialContact6 = TestHelper.GetNewContact(org6, "Official", "123");
			OrgDocument document5 = TestHelper.GetNewOrgDocument(officialContact6.PK, ZBool.True, "A/R");
			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();
			OrgCollectionCallCollection collectionCalls = new OrgCollectionCallCollection(Factory);
			TestHelper.PostTransaction("AR", "JNL", "1000022", 100, 50, 200, branch6.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", org6.PK.ToGuid(), ZDateTime.Now.Date.AddDays(-1), department.PK.ToGuid());

			collectionCalls.Load();
			AssertEquals("There should be 2 Collection Calls in the collection", 2, collectionCalls.Count);
		}

		#endregion

		#region TestHumanReadableShortcutName

		public void TestHumanReadableShortcutName()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = true;
			header.OH_FullName = "Happy Company";
			header.OH_Code = "HPL";
			var query = new ZQuery(vw_OrgCollectionCallSchema.CC_OH, header.PK);
			query.AddToFilter(vw_OrgCollectionCallSchema.CC_GC, GlbCompany.CurrentCompany.PK);

			Factory.Save();

			var orgCollectionCall = Factory.Load<OrgCollectionCall>(query)[0];

			AssertEquals("HumanReadableShortcutName", "HPL - Happy Company", orgCollectionCall.HumanReadableShortcutName);

			header.OH_RL_NKClosestPort = "HAPPY";

			AssertEquals("HumanReadableShortcutName", "HPL - Happy Company (HAPPY)", orgCollectionCall.HumanReadableShortcutName);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader header = newFactory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = true;
			ZQuery query = new ZQuery(vw_OrgCollectionCallSchema.CC_OH, header.PK);
			query.AddToFilter(vw_OrgCollectionCallSchema.CC_GC, GlbCompany.CurrentCompany.PK);
			newFactory.Save();
			return Factory.Load<OrgCollectionCall>(query)[0];
		}

		protected override void SetUp()
		{
			DefaultGlbCompany = TestHelper.GetNewGlbCompany("New GlbCompany");
			DefaultBranch = TestHelper.GetNewBranch(DefaultGlbCompany.PK);
			DefaultBranch.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Factory.Save();
			userContextChange = Env.SetTemporaryUserContext(Env.CurrentUser.PK, DefaultBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

			base.SetUp();

			TestCurrency = TestHelper.GetNewCurrency("CFT", "Currency For Test");
			TestGLHeader = TestHelper.GetNewGLHeader("XXXX.XX.XX", "1111.11.11", "New Gl Header");
			TestBank = TestHelper.GetNewBankAccount("TES", "12345", "1234", TestGLHeader.PK, TestCurrency.RX_Code, DefaultGlbCompany.PK);
			TestDebtorGroup = TestHelper.GetNewOrgDebtorGroup();
			Org = TestHelper.GetNewOrganisationWithCompanyData(false, DefaultGlbCompany.PK, DefaultBranch, TestDebtorGroup, "AAA", "BBB", "CCC", ZBool.True, "SomeOrg1");
			TestOfficialContact = TestHelper.GetNewContact(Org, "Official", "123");
			OrgDocument document = TestHelper.GetNewOrgDocument(TestOfficialContact.PK, ZBool.True, "A/R");

			CollectionNote = TestHelper.GetNewCollectionNote(Org, TestOfficialContact.PK, ZDateTime.Now.Date, ZDateTime.Now.AddDays(10).Date, CollectionNoteDispositionList.Codes.PaymentReceived, "That was a good call!");
			Factory.Save();
		}

		protected override void TearDown()
		{
			userContextChange.Dispose();
			base.TearDown();
		}

		OrgCollectionNote CollectionNote;
		OrgHeader Org;
		AccBankAccount TestBank;
		AccGLHeader TestGLHeader;
		RefCurrency TestCurrency;
		OrgContact TestOfficialContact;
		GlbBranch DefaultBranch;
		GlbCompany DefaultGlbCompany;
		OrgDebtorGroup TestDebtorGroup;
		IDisposable userContextChange;

		MasterFilesTestHelper fTestHelper;
		MasterFilesTestHelper TestHelper
		{
			get
			{
				if (fTestHelper == null)
				{
					fTestHelper = new MasterFilesTestHelper(Factory);
				}
				return fTestHelper;
			}
		}

		#endregion
	}
}
