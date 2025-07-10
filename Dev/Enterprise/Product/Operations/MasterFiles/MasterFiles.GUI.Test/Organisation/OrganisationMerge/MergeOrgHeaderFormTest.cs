using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MergeOrgHeaderForm))]
	public class MergeOrgHeaderFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MergeOrgHeaderForm(new MergeOrgHeader(Factory, Factory.New<OrgHeader>()));
		}

		[RequiresSTA]
		public void TestGridsRemoveAction()
		{
			var merge = new MergeOrgHeader(Factory, null);
			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				AssertEquals(RemoveAction.NoRemovePossible, form.MergeAddressesZGrid.RemoveAction);
				AssertEquals(RemoveAction.NoRemovePossible, form.MergeContactsZGrid.RemoveAction);
			}
		}

		public void TestHandleConcurrencyErrorAfterMerge()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~TESTORG1~";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "~TESTORG2~";
			Factory.Save();
			var org1PK = org1.PK;
			var factory = new BusinessObjectFactory();
			factory.Saving += delegate
			{
				Factory.Load<OrgHeader>(org1PK).OH_IsActive = true;
				Factory.Load<OrgHeader>(org1PK).OH_Code = "~MODIFIED~";
				Factory.Save();
			};
			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(Factory, org1);
			merge.NewOrganisationPk = org2.PK;
			var merger = merge.SaveFactories[0] as OrganisationMergerForTest;
			merger.factoryForDeletingOldOrg = factory;
			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();

				factory = new BusinessObjectFactory();

				org1 = factory.Load<OrgHeader>(org1PK);
				org2 = factory.Load<OrgHeader>(org2.PK);

				AssertNotNull("Should not be deleted", org1);
				AssertNotNull("Should not be deleted", org2);

				AssertNotNull(merge.ConcurrencyException);
				ErrorReporter.Clear();
			}
		}

		[RequiresSTA]
		public void TestProcess()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~TESTORG1~";
			org1.MainAddress.OA_Code = "111";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "~TESTORG2~";
			org2.MainAddress.OA_Code = "111";
			Factory.Save();
			ZGuid org1PK = org1.PK;

			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(Factory, org1);
			merge.NewOrganisationPk = org2.PK;
			using (MergeOrgHeaderForm form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				processButton.PerformClick();

				org1.Reload();
				org2.Reload();

				Assert("Should not be deleted", !org1.IsDeleted);
				Assert("Should not be deleted", !org2.IsDeleted);

				Assert("Should be active", org1.OH_IsActive);
				Assert("Should be active", org2.OH_IsActive);

				processButton.Focus();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();

				BusinessObjectFactory factory = new BusinessObjectFactory();
				org1 = factory.Load<OrgHeader>(org1PK);
				org2 = factory.Load<OrgHeader>(org2.PK);

				AssertNull("Should be deleted", org1);
				AssertNotNull("Should not be deleted", org2);
				Assert("Should not be deleted", !org2.IsDeleted);
				Assert("Should be active", org2.OH_IsActive);
				AssertEquals(org2.Addresses.Count, 1);
			}
		}

		public void TestProcess_NoCustomsCodesPermission()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~TESTORG1~";
			org1.MainAddress.OA_Code = "111";
			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "~OrgMergeTestCusCode01";
			cusCode1.OK_CodeType = "~01";
			cusCode1.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString();
			org1.CustomsCodes.Add(cusCode1);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "~TESTORG2~";
			org2.MainAddress.OA_Code = "111";
			var cusCode1n = org2.CustomsCodes.AddNew();
			cusCode1n.OK_CustomsRegNo = "~OrgMergeTestCusCode01n";
			cusCode1n.OK_CodeType = "~01"; //Code type has to conflict with the old org cuscode
			cusCode1n.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString();
			org2.CustomsCodes.Add(cusCode1n);

			Factory.Save();
			var org1PK = org1.PK;

			Env.Security.OrgConfigModifyFinancialRegistrationNumbers.IsAllowed = false; //Failing RemoveAndDelete Customs Codes Checkpoint
			Env.Security.OrgConfigModifyNonFinancialRegistrationNumbers.IsAllowed = false;

			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(Factory, org1);
			merge.NewOrganisationPk = org2.PK;

			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				processButton.PerformClick();

				org1.Reload();
				org2.Reload();

				Assert("Should not be deleted", !org1.IsDeleted);
				Assert("Should not be deleted", !org2.IsDeleted);

				Assert("Should be active", org1.OH_IsActive);
				Assert("Should be active", org2.OH_IsActive);

				processButton.Focus();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();

				var factory = new BusinessObjectFactory();
				org1 = factory.Load<OrgHeader>(org1PK);
				org2 = factory.Load<OrgHeader>(org2.PK);

				Assert("Rollback occurred in debug. Assume form to show error was shown during a DB transaction in release.", !UnitTestUserNotification.Instance.LastMessage.Text.Contains("The DELETE statement conflicted"));
				AssertNull("Should be deleted", org1);
				AssertNotNull("Should not be deleted", org2);
				Assert("Should not be deleted", !org2.IsDeleted);
				Assert("Should be active", org2.OH_IsActive);
				AssertEquals(org2.Addresses.Count, 1);
			}
		}

		public void TestAllowOpenedFormWhenMerging()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~TESTORG1~";
			org1.MainAddress.OA_Code = "111";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "~TESTORG2~";
			org2.MainAddress.OA_Code = "111";
			Factory.Save();

			var mergeHeader = new MergeOrgHeader(Factory, org1);
			mergeHeader.NewOrganisationPk = org2.PK;
			var cacheForm = new ZForm();
			OpenedFormCache.GetInstance().Add(Guid.NewGuid(), cacheForm, "test");
			using (MergeOrgHeaderForm form = new MergeOrgHeaderForm(mergeHeader))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessages();
				processButton.PerformClick();

				AssertNotEquals("Please close open forms before you use this feature.", UnitTestUserNotification.Instance.LastMessage.Text);
				cacheForm.Close();
			}
		}

		[ExpectNoExceptions]
		public void TestProcessButton_DisplayInformation_WhenRetainedOrgIsInvalid()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "~TESTORG9~";
			org.MainAddress.OA_Code = "283";

			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(Factory, org);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();

				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				AssertNotNull(processButton);

				processButton.PerformClick();
				AssertEquals("You must specify a retained organization", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should not be deleted", !org.IsDeleted);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var organizationGroupBox = (ZGuidFindBox)form.Controls.Find("NewOrgGuidFindBox", true)[0];
				organizationGroupBox.CodeBox.Text = "NoneExistOrg";
				AssertNotNull(organizationGroupBox);

				processButton.PerformClick();
				AssertEquals("You must specify a retained organization", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should not be deleted", !org.IsDeleted);
			}
		}

		public void TestProcessButton_CloseFormWhenFailedWithCriticalErrorOccurred()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();

			newOrg.OH_Code = "RETAINEDORG";
			newOrg.OH_FullName = "RETAINED ORG";

			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();

			oldOrg.OH_Code = "DISSOLVEDORG";
			oldOrg.OH_FullName = "DISSOLVED ORG";

			Factory.Save();

			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(Factory, oldOrg);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			//Use "MergeOrgHeaderForm_WithCriticalFailMergeResult_ForTest" to force the merge result to 'FailedWithCriticalError'
			using (var form = new MergeOrgHeaderForm_WithCriticalFailMergeResult_ForTest(merge))
			{
				form.Show();

				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				AssertNotNull(processButton);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var organizationGroupBox = (ZGuidFindBox)form.Controls.Find("NewOrgGuidFindBox", true)[0];
				organizationGroupBox.CodeBox.Text = newOrg.OH_Code;
				AssertNotNull(organizationGroupBox);

				processButton.PerformClick();

				var mergeFormIsOpen = Application.OpenForms.OfType<MergeOrgHeaderForm_WithCriticalFailMergeResult_ForTest>().Any();

				CombineAssertions(() =>
				{
					AssertEquals(false, mergeFormIsOpen);
				});
			}
		}

		[RequiresSTA]
		public void TestMergeAndAddContactAtOneTime()
		{
			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var retainedOrgContact = retainedOrg.Contacts.AddNew();
			retainedOrgContact.OC_ContactName = "Retained Contact";
			retainedOrgContact.OC_Email = "RetainedContact@qq.com";

			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrgContactForAdd = dissolvedOrg.Contacts.AddNew();
			dissolvedOrgContactForAdd.OC_ContactName = "Dissolved Contact 1";
			dissolvedOrgContactForAdd.OC_Email = "DissolvedContact1@qq.com";

			var dissolvedOrgContactForMerge = dissolvedOrg.Contacts.AddNew();
			dissolvedOrgContactForMerge.OC_ContactName = "Dissolved Contact 2";
			dissolvedOrgContactForMerge.OC_Email = "DissolvedContact2@qq.com";

			var contactItem = dissolvedOrgContactForMerge.ContactItems.AddNew();
			contactItem.OI_OC = dissolvedOrgContactForMerge.PK;
			contactItem.OI_ContactItemType = "PHN";
			contactItem.OI_Description = "MOB2";
			contactItem.OI_Address = "15905112345";

			Factory.Save();

			var dissolvedMergeHeader = new MergeOrgHeader(Factory, dissolvedOrg);
			using (var form = new MergeOrgHeaderForm(dissolvedMergeHeader))
			{
				form.Show();

				CombineAssertions("PreCondition: Retained organization only have one contact and no contact items.", () =>
				{
					AssertEquals(0, retainedOrg.Contacts.Cast<OrgContact>().Single().ContactItems.Count);
				});

				var contactCollectionWithoutDummyContact = dissolvedMergeHeader.ContactCollectionWithoutDummyContact;
				var dissolvedContact1 = (IMergeOrgElement)contactCollectionWithoutDummyContact.Cast<MergeOrgContact>().Single(x => x.OldContactPK == dissolvedOrgContactForAdd.PK);
				var dissolvedContact2 = (IMergeOrgElement)contactCollectionWithoutDummyContact.Cast<MergeOrgContact>().Single(x => x.OldContactPK == dissolvedOrgContactForMerge.PK);

				CombineAssertions("NewObjectsCollection is not loaded", () =>
				{
					AssertEquals(2, dissolvedMergeHeader.ContactCollectionWithoutDummyContact.Count);
					AssertEquals(MergeOrgContact.ActionAdd, dissolvedContact1.Action);
					AssertEquals(MergeOrgContact.ActionAdd, dissolvedContact2.Action);
					AssertEquals(0, dissolvedContact1.NewObjectsCollection.Count());
					AssertEquals(0, dissolvedContact2.NewObjectsCollection.Count());
				});

				dissolvedMergeHeader.NewOrganisationPk = retainedOrg.PK;

				CombineAssertions("NewObjectsCollection is loaded for each dissolved contact after setting retained organization", () =>
				{
					AssertEquals(1, dissolvedContact1.NewObjectsCollection.Count());
					AssertEquals(1, dissolvedContact2.NewObjectsCollection.Count());
				});

				dissolvedContact2.Action = MergeOrgContact.ActionMerge;
				dissolvedContact2.NewObjectPK = retainedOrgContact.PK;

				UnitTestUserNotification.Instance.ClearMessages();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				processButton.PerformClick();

				retainedOrg = new BusinessObjectFactory().Load<OrgHeader>(retainedOrg.PK);
				var retainedContact = retainedOrg.Contacts.Cast<OrgContact>().Single(x => x.PK == retainedOrgContact.PK);
				var addedContact = retainedOrg.Contacts.Cast<OrgContact>().Single(x => x.PK == dissolvedOrgContactForAdd.PK);

				CombineAssertions("Merge succeed", () =>
				{
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Organization transferred successfully."));

					AssertEquals(true, dissolvedOrg.IsDeleted);
					AssertEquals(2, retainedOrg.Contacts.Count);

					AssertNotNull(addedContact);
					AssertEquals("Dissolved Contact 1", addedContact.OC_ContactName);
					AssertEquals("DissolvedContact1@qq.com", addedContact.OC_Email);

					AssertNotNull(retainedContact);
					AssertEquals("Retained Contact", retainedContact.OC_ContactName);
					AssertEquals("RetainedContact@qq.com", retainedContact.OC_Email);
					AssertEquals("PHN", retainedContact.ContactItems.Single().OI_ContactItemType);
					AssertEquals("MOB2", retainedContact.ContactItems.Single().OI_Description);
					AssertEquals("15905112345", retainedContact.ContactItems.Single().OI_Address);
				});
			}
		}

		[RequiresSTA]
		public void TestSuppressedDocumentsShouldBeMerged()
		{
			var orgRetained = Factory.NewWithValidTestData<OrgHeader>();
			var orgDissolved = Factory.NewWithValidTestData<OrgHeader>();

			orgRetained.OH_Code = "ORGRET";
			orgDissolved.OH_Code = "ORGDIS";

			var dummyContactDocument = orgDissolved.SuppressedDocuments.AddNew();
			dummyContactDocument.OD_DocumentGroup = ContactType.Receivables.ToString();
			dummyContactDocument.OD_DefaultContact = true;

			dummyContactDocument = orgDissolved.SuppressedDocuments.AddNew();
			dummyContactDocument.OD_DocumentGroup = ContactType.Miscellaneous.ToString();
			dummyContactDocument.OD_DefaultContact = true;

			Factory.Save();

			var orgRetainedContactCollection = new MergeOrgContactCollection(Factory, orgRetained, null);
			AssertEquals("PreCondition: Retained org does not have any contact", false, orgRetainedContactCollection.Any());

			MergeOrgHeader merge = new MergeOrgHeader(Factory, orgDissolved)
			{
				HasToLoadSimilarOrgs = true,
				NewOrganisationPk = orgRetained.PK
			};

			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();

				var contactGrid = (ZGrid)form.Controls.Find("MergeContactsZGrid", true)[0];

				AssertEquals("PreCondition: Grid BindTo", "ContactCollectionWithoutDummyContact", contactGrid.BindTo);

				var mergeOrgHeader = (MergeOrgHeader)contactGrid.DataSource;
				AssertNotNull("PreCondition: MergeOrgHeader should not be null", mergeOrgHeader);
				AssertNotNull("PreCondition: MergeOrgHeader.ContactCollectionWithoutDummyContact should not be null", mergeOrgHeader.OldOrgContactCollection);

				//Do Merge Process
				UnitTestUserNotification.Instance.ClearMessages();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				processButton.PerformClick();

				Assert("Merge result message", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Organization transferred successfully."));

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var newOrgRetained = newFactory.Load<OrgHeader>(orgRetained.PK);

				AssertNotNull("Retained Org should exist in the database after merge", orgRetained);
				AssertEquals("Number of suppressed documents of retained org should have", 2, newOrgRetained.SuppressedDocuments.Count);
			}
		}

		#region Billing Transaction Tests

		[TestDate(2021, 09, 16, 13, 08, 57, 100)]
		[RequiresSTA]
		public void TestBillingCreated_MergeSuccess()
		{
			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var merge = new MergeOrgHeader(Factory, dissolvedOrg, retainedOrg);
			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Dissolved Org 1 should be deleted after merge", true, dissolvedOrg.IsDeleted);
					var billingTransaction = BillingTransactionTestHelper.GetBillingTransaction(TestConnection);
					Assert(billingTransaction != null);
					BillingTransactionTestHelper.AssertOMGBillingTransaction(billingTransaction, new DateTime(2021, 09, 16, 13, 08, 57, 100), $"ACT=OMS|SRC=GR1", retainedOrg.PK.ToString(), dissolvedOrg.PK.ToString());
					Assert(Regex.IsMatch(billingTransaction.Reference4, @"^[\d]+ms$"));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				});
			}
		}

		[TestDate(2021, 09, 16, 13, 08, 57, 100)]
		public void TestBillingCreated_MergeCancelled()
		{
			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var merge = new MergeOrgHeader(Factory, dissolvedOrg, retainedOrg);
			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				processButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Dissolved Org 1 should not be deleted after merge", false, dissolvedOrg.IsDeleted);
					var billingTransaction = BillingTransactionTestHelper.GetBillingTransaction(TestConnection);
					Assert(billingTransaction != null);
					BillingTransactionTestHelper.AssertOMGBillingTransaction(billingTransaction, new DateTime(2021, 09, 16, 13, 08, 57, 100), $"ACT=OMC|SRC=GR1", retainedOrg.PK.ToString(), dissolvedOrg.PK.ToString());
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				});
			}
		}

		[TestDate(2021, 09, 16, 13, 08, 57, 100)]
		[RequiresSTA]
		public void TestBillingCreated_MergeFailed()
		{
			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			retainedOrg.OH_IsActive = false;
			Factory.Save();

			var merge = new MergeOrgHeader(Factory, dissolvedOrg, retainedOrg);
			using (var form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Dissolved Org 1 should not be deleted after merge", false, dissolvedOrg.IsDeleted);
					var billingTransaction = BillingTransactionTestHelper.GetBillingTransaction(TestConnection);
					Assert(billingTransaction != null);
					BillingTransactionTestHelper.AssertOMGBillingTransaction(billingTransaction, new DateTime(2021, 09, 16, 13, 08, 57, 100), $"ACT=OMF|SRC=GR1", retainedOrg.PK.ToString(), dissolvedOrg.PK.ToString(), "Failed");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				});
			}
		}
		#endregion

		#region Implementation

		class MergeOrgHeaderForm_WithCriticalFailMergeResult_ForTest : MergeOrgHeaderForm
		{
			public MergeOrgHeaderForm_WithCriticalFailMergeResult_ForTest(MergeOrgHeader businessEntity)
				: base(businessEntity)
			{
			}

			protected override MergeResult GetMergeResult(out long elapsedMilliseconds)
			{
				elapsedMilliseconds = 0;
				return MergeResult.FailedWithCriticalError;
			}
		}

		#endregion
	}

	[UseSnapshotProtection]
	public class MergeOrgHeaderFormTest_NonTransactional : TestCase
	{
		public void TestProcessWithException()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			OrgHeader org1 = factory1.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = factory1.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~temp1~";
			org2.OH_Code = "~temp2~";
			org1.MainAddress.OA_Code = "111";
			org2.MainAddress.OA_Code = "111";

			factory1.Save();
			ZGuid org1PK = org1.PK;
			ZGuid org2PK = org2.PK;

			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(factory1, org1);
			merge.ThrowExceptionOnOrgDelete = true;
			merge.NewOrganisationPk = org2.PK;
			using (MergeOrgHeaderForm form = new MergeOrgHeaderForm(merge))
			{
				form.Show();
				ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();

				BusinessObjectFactory factory = new BusinessObjectFactory();
				org1 = factory.Load<OrgHeader>(org1PK);
				org2 = factory.Load<OrgHeader>(org2.PK);

				AssertNotNull("Should not be deleted", org1);
				AssertNotNull("Should not be deleted", org2);
				Assert("First org should be de-activated", !org1.OH_IsActive);
				Assert("First org should be non-deleted", !org1.IsDeleted);
				Assert("Second org should be activate", org2.OH_IsActive);
				Assert("Second org should be non-deleted", !org2.IsDeleted);
				AssertEquals("should have initial address", 1, org1.Addresses.Count);
				AssertEquals("should have initial address", 1, org2.Addresses.Count);
			}
		}
	}
}
