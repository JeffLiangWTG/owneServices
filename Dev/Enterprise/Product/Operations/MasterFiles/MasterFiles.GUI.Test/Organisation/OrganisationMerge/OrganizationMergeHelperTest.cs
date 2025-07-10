using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrganizationMergeHelperTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMergingOrganisation_RemovesOldOrganisation_FromRecentItems()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();

			newOrg.OH_Code = "NEWORG";

			Factory.Save();

			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();

			oldOrg.OH_FullName = "TOLL PTY";
			oldOrg.OH_RL_NKClosestPort = "AUSYD";
			oldOrg.MainAddress.OA_Address1 = "Test Address 1";
			oldOrg.MainAddress.OA_PostCode = "2015";
			oldOrg.MainAddress.OA_City = "SYDNEY";
			oldOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			oldOrg.MainAddress.OA_State = "NSW";

			using (var form = new FormForTest(oldOrg))
			{
				form.SaveForm();
			}

			var stmLinks = Factory.Load<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, oldOrg.PK));

			AssertEquals("Favorites should have link to oldOrg on (Favorites & Recent Items)", 2, stmLinks.Length);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			var mergeData = new MergeOrgHeader(Factory, oldOrg, newOrg);
			var result = OrganizationMergeHelper.Merge(mergeData, out var elapsedMilliseconds);

			AssertEquals(MergeResult.Success, result);
			Assert("Old Organisation should have been deleted", oldOrg.IsDeleted);
			AssertEquals(0, Factory.Load<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, oldOrg.PK)).Length);
		}

		[ExpectNoExceptions]
		public void TestMergingOrganizationFailedWhenOldOrganizationIsDeleted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrg = CreatePTOrgHeader("RETAINED ORG", "TEST ADDRESS", "PT", "RETORG");
				var dissolvedOrg = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG");

				//OK to merge if IVA# are the same
				AddIVACustomsCode(retainedOrg, "111");
				AddIVACustomsCode(dissolvedOrg, "111");

				CreatePostedTransaction(retainedOrg, "INV01", company);
				CreatePostedTransaction(dissolvedOrg, "INV01", company);

				Factory.Save();

				AssertEquals("Precondition merge is allowed", true, OrgHeaderMergingChecker.IsAllowedToMergeOrgs(retainedOrg.PK, dissolvedOrg.PK, out string reasons));
				AssertEquals("Reasons", string.Empty, reasons);

				var mergeOrgHeader = new MergeOrgHeader(Factory, dissolvedOrg, retainedOrg);
				dissolvedOrg.Delete();

				var result = OrganizationMergeHelper.Merge(mergeOrgHeader, out var elapsedMilliseconds);
				AssertEquals("Merge result", MergeResult.Failed, result);
			}
		}

		[ExpectNoExceptions]
		public void TestShouldStopMergeWhenExceptionOccurFromMoveFkReferences()
		{
			var warehousePK = ZGuid.NewZGuid();
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			var whsAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var createWareHouseCommand = ((IDbConnected)Factory).Connection.Command($@"
INSERT INTO dbo.WhsWarehouse 
(WW_PK, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_WarehouseName, WW_GB_RelatedCompanyBranch, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES 
('{warehousePK}', '{whsAddress.PK}', 'W1', 'TEST', '{GlbBranch.CurrentBranch.PK}', 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			createWareHouseCommand.ExecuteNonQuery();

			var createJobStorageCommand = ((IDbConnected)Factory).Connection.Command($@"
INSERT INTO dbo.JobStorage
(ET_PK, ET_StorageJobNumber, ET_StorageType, ET_BillingDate, ET_StorageFromDate, ET_StorageToDate, ET_OH_Client, ET_WW, ET_SystemCreateTimeUtc, ET_SystemCreateUser, ET_SystemLastEditTimeUtc, ET_SystemLastEditUser) VALUES
(NEWID(), '1', 'WHS', '{ZDateTime.BrettsBirthday}','{ZDateTime.BrettsBirthday}','{ZDateTime.BrettsBirthday.AddDays(1)}', '{orgA.PK}', '{warehousePK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.JobStorage
(ET_PK, ET_StorageJobNumber, ET_StorageType, ET_BillingDate, ET_StorageFromDate, ET_StorageToDate, ET_OH_Client, ET_WW, ET_SystemCreateTimeUtc, ET_SystemCreateUser, ET_SystemLastEditTimeUtc, ET_SystemLastEditUser) VALUES
(NEWID(), '2', 'WHS', '{ZDateTime.BrettsBirthday}','{ZDateTime.BrettsBirthday}','{ZDateTime.BrettsBirthday.AddDays(1)}', '{orgB.PK}', '{warehousePK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
");
			createJobStorageCommand.ExecuteNonQuery();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var mergeOrgHeader = new MergeOrgHeader(Factory, orgA, orgB);
			var result = OrganizationMergeHelper.Merge(mergeOrgHeader, out var elapsedMilliseconds);

			AssertEquals(MergeResult.Failed, result);
			AssertNotNull(mergeOrgHeader.AddOverlappingDatesException);
			AssertContains("It was not possible to complete the merge operation you requested because the organizations have periodic invoices with overlapping dates.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("It is possible to deactivate one of the organizations, however these two organizations cannot be merged.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestNotThrowExceptionWhenNewOrgIsDeleted()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			oldOrg.OH_Code = "OLDORG";
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_Code = "NEWORG";
			Factory.Save();

			var mergeData = new MergeOrgHeader(Factory, oldOrg, newOrg);
			newOrg.Delete();
			ErrorReporter.Clear();

			CombineAssertions(() =>
			{
				AssertNoErrors(mergeData.NewOrganisationPkInfo);
				var mergeResult = OrganizationMergeHelper.Merge(mergeData, out var elapsedMilliseconds);
				AssertEquals(MergeResult.Failed, mergeResult);
				AssertHasErrors(mergeData.NewOrganisationPkInfo);
				AssertContains("There are errors that need to be corrected before this record can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			});
		}

		public void TestDuplicateProducts()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TESTORG2";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PROD1";
			product1.OP_Desc = "MERGE-TEST";
			product1.RelatedOrganisations.AddOwner(org1);

			var product1dup = Factory.New<OrgSupplierPart>();
			product1dup.OP_PartNum = "PROD1";
			product1dup.OP_Desc = "MERGE-TEST";
			product1dup.RelatedOrganisations.AddOwner(org2);

			Factory.Save();

			var mergeData = new MergeOrgHeader(Factory, org2, org1);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // confirm merge

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			AssertEquals(MergeResult.Cancelled, OrganizationMergeHelper.Merge(mergeData, out var elapsedMilliseconds));
			AssertEquals(typeof(ProductOrgMergerForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // confirm merge

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				if (form is ProductOrgMergerForm mergerForm)
				{
					((ProductOrgMerger)mergerForm.BusinessEntity).DeactivateSingleRelationDuplicates();
				}
			});

			AssertEquals(MergeResult.Success, OrganizationMergeHelper.Merge(mergeData, out elapsedMilliseconds));
			AssertEquals(typeof(ProductOrgMergerForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
		}

		#region TestIsAllowedToMergeOrgs

		public void TestIsAllowedToMergeOrgs_OneToOne_StopMergeIfMergeIsNotAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrg = CreatePTOrgHeader("RETAINED ORG", "TEST ADDRESS", "PT", "RETORG");
				var dissolvedOrg = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG");

				//Different IVA# should disallow the merger
				AddIVACustomsCode(retainedOrg, "111");
				AddIVACustomsCode(dissolvedOrg, "222");

				CreatePostedTransaction(retainedOrg, "INV01", company);
				CreatePostedTransaction(dissolvedOrg, "INV01", company);

				Factory.Save();

				AssertEquals("Precondition merge is not allowed", false, OrgHeaderMergingChecker.IsAllowedToMergeOrgs(retainedOrg.PK, dissolvedOrg.PK, out string reasons));
				AssertNotNullOrEmpty("Reasons", reasons);

				var mergeOrgHeader = new MergeOrgHeader(Factory, dissolvedOrg, retainedOrg);
				var result = OrganizationMergeHelper.Merge(mergeOrgHeader, out var elapsedMilliseconds);

				AssertEquals("Merge result", MergeResult.Failed, result);

				CombineAssertions(() =>
				{
					AssertEquals("Retained Org should not be deleted after merge", false, retainedOrg.IsDeleted);
					AssertEquals("Dissolved Org should not be deleted after merge", false, dissolvedOrg.IsDeleted);
				});

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				var loadedRetainedOrg = newFactory.Load<OrgHeader>(retainedOrg.PK);
				var loadedDissolvedOrg = newFactory.Load<OrgHeader>(dissolvedOrg.PK);

				CombineAssertions(() =>
				{
					AssertNotNull("Retained Org should exist in the database after merge", loadedRetainedOrg);
					AssertNotNull("Dissolved Org should exist in the database after merge", loadedDissolvedOrg);
				});
			}
		}

		public void TestIsAllowedToMergeOrgs_OneToOne_CanProceedToMergeIfMergeIsAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrg = CreatePTOrgHeader("RETAINED ORG", "TEST ADDRESS", "PT", "RETORG");
				var dissolvedOrg = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG");

				//OK to merge if IVA# are the same
				AddIVACustomsCode(retainedOrg, "111");
				AddIVACustomsCode(dissolvedOrg, "111");

				CreatePostedTransaction(retainedOrg, "INV01", company);
				CreatePostedTransaction(dissolvedOrg, "INV01", company);

				Factory.Save();

				AssertEquals("Precondition merge is allowed", true, OrgHeaderMergingChecker.IsAllowedToMergeOrgs(retainedOrg.PK, dissolvedOrg.PK, out string reasons));
				AssertEquals("Reasons", string.Empty, reasons);

				var mergeOrgHeader = new MergeOrgHeader(Factory, dissolvedOrg, retainedOrg);
				var result = OrganizationMergeHelper.Merge(mergeOrgHeader, out var elapsedMilliseconds);

				AssertEquals("Merge result", MergeResult.Success, result);

				CombineAssertions(() =>
				{
					AssertEquals("Retained Org should not be deleted after merge", false, retainedOrg.IsDeleted);
					AssertEquals("Dissolved Org should be deleted after merge", true, dissolvedOrg.IsDeleted);
				});

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				var loadedRetainedOrg = newFactory.Load<OrgHeader>(retainedOrg.PK);
				var loadedDissolvedOrg = newFactory.Load<OrgHeader>(dissolvedOrg.PK);

				CombineAssertions(() =>
				{
					AssertNotNull("Retained Org should exist in the database after merge", loadedRetainedOrg);
					AssertNull("Dissolved Org should not exist in the database after merge", loadedDissolvedOrg);
				});
			}
		}

		#region Implementation

		OrgHeader CreatePTOrgHeader(ZString orgName, ZString orgAddress, ZString unloco, ZString orgCode)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsConsignee = true;
			orgHeader.OH_FullName = orgName;
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_RL_NKClosestPort = unloco;
			orgHeader.MainAddress.OA_Address1 = orgAddress;
			orgHeader.MainAddress.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return orgHeader;
		}

		OrgCusCode AddIVACustomsCode(OrgHeader header, string customsRegNo)
		{
			var retainedOrgCusCode = header.CustomsCodes.AddNew();
			retainedOrgCusCode.OK_OH = header.PK;
			retainedOrgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			retainedOrgCusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			retainedOrgCusCode.OK_CustomsRegNo = customsRegNo;
			var newAddress = header.Addresses.AddNew();
			newAddress.OA_Address1 = "newAddress";
			retainedOrgCusCode.OK_OA_PremisesAddress = newAddress.PK;

			return retainedOrgCusCode;
		}

		void CreatePostedTransaction(OrgHeader header, string transactionNum, GlbCompany company)
		{
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_OH = header.PK;
			transactionHeader.AH_GC = company.PK;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_OH = header.PK;
			transactionLine.AL_GC = company.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
		}

		#endregion

		#endregion

		#region Implementation

		class FormForTest : ZForm
		{
			public FormForTest(object bizO)
				: base(bizO)
			{
				ControllerID = ControllerIDs.Organisation;
			}

			public void SaveForm()
			{
				DisplayMode = ODisplayMode.New;
				base.OnPostButtonClick(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
