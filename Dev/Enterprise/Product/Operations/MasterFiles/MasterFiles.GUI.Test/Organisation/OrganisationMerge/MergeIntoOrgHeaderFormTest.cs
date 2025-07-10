namespace Enterprise.MasterFiles.GUI.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using CargoWise.Application;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Billing.Integration;
	using Enterprise.Integration.Licensing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Favorites;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(MergeIntoOrgHeaderForm))]
	public class MergeIntoOrgHeaderFormTest : ZFormBasherTest
	{
		public void TestRemoveMergedOrganizationsFromRecentItems()
		{
			var factory = new BusinessObjectFactory();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TOLL PTY";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.MainAddress.OA_Address1 = "Test Address 1";
			org1.MainAddress.OA_PostCode = "2015";
			org1.MainAddress.OA_City = "SYDNEY";
			org1.MainAddress.OA_RN_NKCountryCode = "AU";
			org1.MainAddress.OA_State = "NSW";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(factory, org1, org2);
			merge.HasToLoadSimilarOrgs = true;
			merge.OldOrgsCollectionByName.Add(org1);
			merge.OldOrganisation = org1;

			using (var organizationForm1 = new FormForTest(org1))
			using (var form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				organizationForm1.SaveForm();
				form.Show();

				var shortcut = new LinkWrapper(merge.Factory.LoadTop1<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, org1.PK)));
				AssertEquals("Should be added to Recent Items of Current Module", true, RecentItemManager.Instance.IsInRecentItems(shortcut.ModuleName, shortcut));
				AssertEquals("Should be added to Recent Items of Main Form", true, RecentItemManager.Instance.IsInRecentItems(string.Empty, shortcut));

				var processButton = form.Controls.Find("ProcessButton", true)[0] as ZButton;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();
				AssertEquals("Should be removed from Recent Items of Current Module", false, RecentItemManager.Instance.IsInRecentItems(shortcut.ModuleName, shortcut));
				AssertEquals("Should be removed from Recent Items of Main Form", false, RecentItemManager.Instance.IsInRecentItems(string.Empty, shortcut));
			}
		}

		protected override Form GetFormToBashCore()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "~dont~find~anything~";
			MergeOrgHeader merge = new MergeOrgHeader(Factory, null, org);
			merge.HasChanges = false;
			return new MergeIntoOrgHeaderForm(merge);
		}

		[RequiresSTA]
		public void TestGridsRemoveAction()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var merge = new MergeOrgHeader(Factory, org, org);
			using (var form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				AssertEquals(RemoveAction.NoRemovePossible, form.MergeAddressesZGrid.RemoveAction);
				AssertEquals(RemoveAction.NoRemovePossible, form.MergeContactsZGrid.RemoveAction);
			}
		}

		public void TestProcess()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~test1~";
			org2.OH_Code = "~test2~";
			org3.OH_Code = "~test3~";
			org1.MainAddress.OA_Address1 = "address 1";
			org2.MainAddress.OA_Address1 = "address 2";
			org3.MainAddress.OA_Address1 = "address 3";
			Factory.Save();
			ZGuid pk1 = org1.PK;
			ZGuid pk3 = org3.PK;

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = org2.PK;
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(org1);
			merge.OldOrgsCollectionByName.Add(org3);
			using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				processButton.PerformClick();

				org1.Reload();
				org2.Reload();
				org3.Reload();

				Assert("Should not be deleted", !org1.IsDeleted);
				Assert("Should not be deleted", !org2.IsDeleted);
				Assert("Should not be deleted", !org3.IsDeleted);

				Assert("Should be active", org1.OH_IsActive);
				Assert("Should be active", org2.OH_IsActive);
				Assert("Should be active", org3.OH_IsActive);

				processButton.Focus();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();

				BusinessObjectFactory factory = new BusinessObjectFactory();
				org1 = factory.Load<OrgHeader>(pk1);
				org2 = factory.Load<OrgHeader>(org2.PK);
				org3 = factory.Load<OrgHeader>(pk3);

				AssertNull("Should be deleted", org1);
				AssertNull("Should be deleted", org3);
				AssertNotNull("Should not be deleted", org2);
				Assert("Should not be deleted", !org2.IsDeleted);
				Assert("Should be active", org2.OH_IsActive);
			}
		}

		public void TestAddToList()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~test1~";
			org2.OH_Code = "~test2~";
			org3.OH_Code = "~test3~";
			Factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = org2.PK;
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(org1);
			merge.OldOrgsCollectionByName.Add(org3);
			using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				ZButton addButton = (ZButton)form.Controls.Find("AddAnotherOldOrgButton", true)[0];
				ZGuidFindBox orgFind = (ZGuidFindBox)form.Controls.Find("OldOrgGuidFindBox", true)[0];
				AssertNotNull(addButton);
				AssertNotNull(orgFind);

				OrgHeader otherOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
				AssertNotNull(otherOrg);
				orgFind.CurrentCode = otherOrg.OH_Code;
				merge.OrganisationPkForBinding = otherOrg.PK;
				AssertEquals(otherOrg.PK, merge.OrganisationPkForBinding);
				AssertContainsPK(form.CurrentCollection, otherOrg.PK, false);

				int count = form.CurrentCollection.Count;

				addButton.PerformClick();
				AssertContainsPK(form.CurrentCollection, otherOrg.PK, true);
				AssertEquals(count + 1, form.CurrentCollection.Count);

				addButton.PerformClick(); // check its not added for the 2nd time
				AssertContainsPK(form.CurrentCollection, otherOrg.PK, true);
				AssertEquals(count + 1, form.CurrentCollection.Count);
			}
		}

		public void TestDeletingOrganizationClearsAddressesAndContacts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "My Test 1";
			org1.MainAddress.OA_Address1 = "aaa";
			org1.MainAddress.OA_PostCode = "123";
			org1.MainAddress.OA_City = "zzz";
			org1.MainAddress.OA_State = "xxx";

			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Bob";
			contact1.OC_OH = org1.PK;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "My Test 2";
			org2.MainAddress.OA_Address1 = "bbb";
			org2.MainAddress.OA_PostCode = "123";
			org2.MainAddress.OA_City = "zzz";
			org2.MainAddress.OA_State = "xxx";
			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Bill";
			contact2.OC_OH = org2.PK;

			org1.OH_Code = "~test1~";
			org2.OH_Code = "~test2~";
			Factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = org1.PK;
			merge.OldOrgsCollectionByName.RemoveAll();

			using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				ZButton addButton = (ZButton)form.Controls.Find("AddAnotherOldOrgButton", true)[0];
				ZGuidFindBox orgFind = (ZGuidFindBox)form.Controls.Find("OldOrgGuidFindBox", true)[0];
				AssertNotNull(addButton);
				AssertNotNull(orgFind);

				OrgHeader otherOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "~test2~"));
				AssertNotNull(otherOrg);
				orgFind.CurrentCode = otherOrg.OH_Code;
				merge.OrganisationPkForBinding = otherOrg.PK;
				AssertEquals(otherOrg.PK, merge.OrganisationPkForBinding);
				AssertContainsPK(form.CurrentCollection, otherOrg.PK, false);
				AssertEquals(0, form.CurrentOrgAddressCollection.Count);

				int count = form.CurrentCollection.Count;
				addButton.PerformClick();
				AssertContainsPK(form.CurrentCollection, otherOrg.PK, true);
				AssertEquals(count + 1, form.CurrentCollection.Count);
				AssertNotEquals(0, form.CurrentOrgAddressCollection.Count);
				AssertNotEquals(0, form.CurrentOrgContactCollection.Count);
				otherOrg.Delete();
				Factory.Save();
				AssertEquals(count, form.CurrentCollection.Count);
				AssertEquals(0, form.CurrentOrgAddressCollection.Count);
				AssertEquals(0, form.CurrentOrgContactCollection.Count);
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

			var mergeHeader = new MergeOrgHeader(Factory, null);
			mergeHeader.NewOrganisationPk = org2.PK;
			var cacheForm = new ZForm();
			OpenedFormCache.GetInstance().Add(Guid.NewGuid(), cacheForm, "test");
			using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(mergeHeader))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessages();
				processButton.PerformClick();

				AssertNotEquals("Please close open forms before you use this feature.", UnitTestUserNotification.Instance.LastMessage.Text);
				cacheForm.Close();
			}
		}

		void AssertContainsPK(OrgHeaderCollection col, ZGuid otherOrgPK, bool expected)
		{
			bool added = false;
			Array.ForEach(col.ToArray<OrgHeader>(), delegate(OrgHeader org)
			{ added |= org.PK == otherOrgPK; });
			AssertEquals(expected, added);
		}

		public void TestDuplicateProducts_Resolve()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var org4 = Factory.New<OrgHeader>();

			org1.OH_Code = "TESTORG1";
			org2.OH_Code = "TESTORG2";
			org3.OH_Code = "TESTORG3";
			org4.OH_Code = "TESTORG4";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PROD1";
			product1.OP_Desc = "MERGE-TEST";
			product1.RelatedOrganisations.AddOwner(org1);

			var product1dup = Factory.New<OrgSupplierPart>();
			product1dup.OP_PartNum = "PROD1";
			product1dup.OP_Desc = "MERGE-TEST";
			product1dup.RelatedOrganisations.AddOwner(org2);

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PROD2";
			product2.OP_Desc = "MERGE-TEST";
			product2.RelatedOrganisations.AddOwner(org1);

			var product2dup = Factory.New<OrgSupplierPart>();
			product2dup.OP_PartNum = "PROD2";
			product2dup.OP_Desc = "MERGE-TEST";
			product2dup.RelatedOrganisations.AddOwner(org3);

			var product3 = Factory.New<OrgSupplierPart>();
			product3.OP_PartNum = "PROD3";
			product3.OP_Desc = "MERGE-TEST";
			product3.RelatedOrganisations.AddOwner(org4);

			Factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = org1.PK;
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(org2);
			merge.OldOrgsCollectionByName.Add(org3);
			merge.OldOrgsCollectionByName.Add(org4);

			using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

				var actualResolutions = new List<string>();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
				{
					if (formOrDialog is ProductOrgMergerForm productMergerForm)
					{
						var productMerger = (ProductOrgMerger)productMergerForm.BusinessEntity;
						actualResolutions.Add($"{productMerger.OldOrganization?.OH_Code}->{productMerger.NewOrganization?.OH_Code}");
						productMerger.DeactivateSingleRelationDuplicates();
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				processButton.PerformClick();

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();

				CombineAssertions(() =>
				{
					AssertEquals
					(
						"resolution dialogs",
						// TESTORG4 does not have duplicates, and is not present in this list
						string.Join("\r\n", new string[] { "TESTORG2->TESTORG1", "TESTORG3->TESTORG1" }),
						string.Join("\r\n", actualResolutions.OrderBy(t => t))
					);
					var factory2 = NewFactory();
					AssertNotNull("org1 should remain", factory2.Load<OrgHeader>(org1.PK));
					AssertNull("org2 should be deleted", factory2.Load<OrgHeader>(org2.PK));
					AssertNull("org3 should be deleted", factory2.Load<OrgHeader>(org3.PK));
					AssertNull("org4 should be deleted", factory2.Load<OrgHeader>(org4.PK));
					var products = factory2.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_Desc, "MERGE-TEST"));
					AssertEquals("there are 5 products in this test", 5, products.Length);
					var activeProducts = new Dictionary<string, int>();
					foreach (var product in products)
					{
						activeProducts.TryGetValue(product.OP_PartNum, out var activeProductCount);
						if (product.OP_IsActive)
						{
							activeProductCount++;
						}
						activeProducts[product.OP_PartNum] = activeProductCount;

						foreach (var rel in product.RelatedOrganisations.Cast<OrgPartRelation>())
						{
							AssertEquals($"relation '{product.OP_PartNum}'/'{rel.OU_Relationship}' should reference org1", org1.PK, rel.OU_OH);
						}
					}
					foreach (var pair in activeProducts)
					{
						AssertEquals($"should be 1 active product with code '{pair.Key}', but was {pair.Value}", 1, pair.Value);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestDuplicateProducts_Cancel()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			org1.OH_Code = "TESTORG1";
			org2.OH_Code = "TESTORG2";
			org3.OH_Code = "TESTORG3";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PROD1";
			product1.OP_Desc = "MERGE-TEST";
			product1.RelatedOrganisations.AddOwner(org1);

			var product1dup = Factory.New<OrgSupplierPart>();
			product1dup.OP_PartNum = "PROD1";
			product1dup.OP_Desc = "MERGE-TEST";
			product1dup.RelatedOrganisations.AddOwner(org2);

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "PROD2";
			product2.OP_Desc = "MERGE-TEST";
			product2.RelatedOrganisations.AddOwner(org1);

			var product2dup = Factory.New<OrgSupplierPart>();
			product2dup.OP_PartNum = "PROD2";
			product2dup.OP_Desc = "MERGE-TEST";
			product2dup.RelatedOrganisations.AddOwner(org3);

			Factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = org1.PK;
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(org2);
			merge.OldOrgsCollectionByName.Add(org3);

			using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

				int actualResolutionCount = 0;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
				{
					if (formOrDialog is ProductOrgMergerForm productMergerForm)
					{
						var productMerger = (ProductOrgMerger)productMergerForm.BusinessEntity;
						productMerger.DeactivateSingleRelationDuplicates();
						actualResolutionCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				processButton.PerformClick();

				CombineAssertions(() =>
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();

					AssertEquals("should stop after first cancel, even if conflict is resolved", 1, actualResolutionCount);
					var factory2 = NewFactory();
					AssertNotNull("org1 should remain", factory2.Load<OrgHeader>(org1.PK));
					AssertNotNull("org2 should remain", factory2.Load<OrgHeader>(org2.PK));
					AssertNotNull("org3 should remain", factory2.Load<OrgHeader>(org3.PK));
				});
			}
		}

		[RequiresSTA]
		public void TestGridBindingForControls()
		{
			using (MergeIntoOrgHeaderForm form = (MergeIntoOrgHeaderForm)GetFormToBash())
			{
				form.Show();
				ZRadioButton b1 = (ZRadioButton)form.Controls.Find("FindByName", true)[0];
				ZRadioButton b2 = (ZRadioButton)form.Controls.Find("FindByCode", true)[0];
				ZRadioButton b3 = (ZRadioButton)form.Controls.Find("FindByPattern", true)[0];
				ZButton b4 = (ZButton)form.Controls.Find("FindButton", true)[0];
				ZGrid gridOrg = (ZGrid)form.Controls.Find("OldOrganisationsGrid", true)[0];
				ZGrid gridAdr = (ZGrid)form.Controls.Find("MergeAddressesZGrid", true)[0];
				ZGrid gridCnt = (ZGrid)form.Controls.Find("MergeContactsZGrid", true)[0];
				ZDropEdit drop = (ZDropEdit)form.Controls.Find("MatchThreshold", true)[0];
				ZCalcEdit calc = (ZCalcEdit)form.Controls.Find("MaxResults", true)[0];

				AssertEquals("Precondition:", "OldOrgsCollectionByName", gridOrg.DataMember);
				AssertEquals("Precondition:", "OldOrgAddressCollectionForSimilarOrgsByName", gridAdr.DataMember);
				AssertEquals("Precondition:", "OldOrgContactCollectionForSimilarOrgsByName", gridCnt.DataMember);
				AssertEquals("Precondition:", "CurrentMatchThresholdCode", drop.BindTo);
				AssertEquals("Precondition:", "MaxResults", calc.BindTo);

				b2.PerformClick();
				AssertEquals("switching does not fire search", "OldOrgsCollectionByName", gridOrg.DataMember);
				AssertEquals("switching does not fire search", "OldOrgAddressCollectionForSimilarOrgsByName", gridAdr.DataMember);
				AssertEquals("switching does not fire search", "OldOrgContactCollectionForSimilarOrgsByName", gridCnt.DataMember);
				AssertEquals("switching does not fire search", "CurrentMatchThresholdCode", drop.BindTo);
				AssertEquals("switching does not fire search", "MaxResults", calc.BindTo);

				b4.PerformClick();
				AssertEquals("should be by code", "OldOrgsCollectionByCode", gridOrg.DataMember);
				AssertEquals("should be by code", "OldOrgAddressCollectionForSimilarOrgsByCode", gridAdr.DataMember);
				AssertEquals("should be by code", "OldOrgContactCollectionForSimilarOrgsByCode", gridCnt.DataMember);
				AssertEquals("not changed", "CurrentMatchThresholdCode", drop.BindTo);
				AssertEquals("not changed", "MaxResults", calc.BindTo);

				b1.PerformClick();
				b4.PerformClick();
				AssertEquals("should be by name", "OldOrgsCollectionByName", gridOrg.DataMember);
				AssertEquals("should be by name", "OldOrgAddressCollectionForSimilarOrgsByName", gridAdr.DataMember);
				AssertEquals("should be by name", "OldOrgContactCollectionForSimilarOrgsByName", gridCnt.DataMember);
				AssertEquals("not changed", "CurrentMatchThresholdCode", drop.BindTo);
				AssertEquals("not changed", "MaxResults", calc.BindTo);

				b3.PerformClick();
				b4.PerformClick();
				AssertEquals("should be by pattern", "OldOrgsCollectionByPattern", gridOrg.DataMember);
				AssertEquals("should be by pattern", "OldOrgAddressCollectionForSimilarOrgsByPattern", gridAdr.DataMember);
				AssertEquals("should be by pattern", "OldOrgContactCollectionForSimilarOrgsByPattern", gridCnt.DataMember);
				AssertEquals("not changed", "CurrentMatchThresholdCode", drop.BindTo);
				AssertEquals("not changed", "MaxResults", calc.BindTo);
			}
		}

		#region TestIsAllowedToMergeOrgs 

		public void TestIsAllowedToMergeOrgs_ManyToOne_StopMergeIfAllMergesAreNotAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrg = CreatePTOrgHeader("RETAINED ORG", "TEST ADDRESS", "PT", "RETORG");

				var dissolvedOrg1 = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG1");
				var dissolvedOrg2 = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG2");

				var dissolvedOrg1PK = dissolvedOrg1.PK;
				var dissolvedOrg2PK = dissolvedOrg2.PK;

				//Setup all organizations have same IVA#
				AddIVACustomsCode(retainedOrg, "111");
				AddIVACustomsCode(dissolvedOrg1, "222");
				AddIVACustomsCode(dissolvedOrg2, "333");

				CreatePostedTransaction(retainedOrg, "INV01", company);
				CreatePostedTransaction(dissolvedOrg1, "INV02", company);
				CreatePostedTransaction(dissolvedOrg2, "INV02", company);

				Factory.Save();

				var merge = new MergeOrgHeader(Factory, null)
				{
					HasToLoadSimilarOrgs = true,
					NewOrganisationPk = retainedOrg.PK
				};
				merge.OldOrgsCollectionByName.RemoveAll();
				merge.OldOrgsCollectionByName.Add(dissolvedOrg1);
				merge.OldOrgsCollectionByName.Add(dissolvedOrg2);

				using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
				{
					form.Show();
					ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					processButton.PerformClick();

					CombineAssertions(() =>
					{
						AssertEquals("Dissolved Org 1 should not be deleted after merge", false, dissolvedOrg1.IsDeleted);
						AssertEquals("Dissolved Org 2 should not be deleted after merge", false, dissolvedOrg2.IsDeleted);
					});

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

					dissolvedOrg1 = newFactory.Load<OrgHeader>(dissolvedOrg1PK);
					dissolvedOrg2 = newFactory.Load<OrgHeader>(dissolvedOrg2PK);

					CombineAssertions(() =>
					{
						AssertNotNull("Dissolved Org 1 should exist in the database after merge", dissolvedOrg1);
						AssertNotNull("Dissolved Org 2 should exist in the database after merge", dissolvedOrg2);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestIsAllowedToMergeOrgs_ManyToOne_CanProceedToMergeIfAllMergesAreAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrg = CreatePTOrgHeader("RETAINED ORG", "TEST ADDRESS", "PT", "RETORG");

				var dissolvedOrg1 = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG1");
				var dissolvedOrg2 = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG2");

				var dissolvedOrg1PK = dissolvedOrg1.PK;
				var dissolvedOrg2PK = dissolvedOrg2.PK;

				//Setup all organizations have same IVA#
				AddIVACustomsCode(retainedOrg, "111");
				AddIVACustomsCode(dissolvedOrg1, "111");
				AddIVACustomsCode(dissolvedOrg2, "111");

				CreatePostedTransaction(retainedOrg, "INV01", company);
				CreatePostedTransaction(dissolvedOrg1, "INV01", company);
				CreatePostedTransaction(dissolvedOrg2, "INV01", company);

				Factory.Save();

				var merge = new MergeOrgHeader(Factory, null)
				{
					HasToLoadSimilarOrgs = true,
					NewOrganisationPk = retainedOrg.PK
				};
				merge.OldOrgsCollectionByName.RemoveAll();
				merge.OldOrgsCollectionByName.Add(dissolvedOrg1);
				merge.OldOrgsCollectionByName.Add(dissolvedOrg2);

				using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
				{
					form.Show();
					ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					processButton.PerformClick();

					CombineAssertions(() =>
					{
						AssertEquals("Dissolved Org 1 should be deleted after merge", true, dissolvedOrg1.IsDeleted);
						AssertEquals("Dissolved Org 2 should be deleted after merge", true, dissolvedOrg2.IsDeleted);
					});

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

					dissolvedOrg1 = newFactory.Load<OrgHeader>(dissolvedOrg1PK);
					dissolvedOrg2 = newFactory.Load<OrgHeader>(dissolvedOrg2PK);

					CombineAssertions(() =>
					{
						AssertNull("Dissolved Org 1 should not exist in the database after merge", dissolvedOrg1);
						AssertNull("Dissolved Org 2 should not exist in the database after merge", dissolvedOrg2);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestIsAllowedToMergeOrgs_ManyToOne_CanProceedToMergeIfSomeMergesAreAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var retainedOrg = CreatePTOrgHeader("RETAINED ORG", "TEST ADDRESS", "PT", "RETORG");

				var dissolvedOrg1 = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG1");
				var dissolvedOrg2 = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG2");

				var dissolvedOrg1PK = dissolvedOrg1.PK;
				var dissolvedOrg2PK = dissolvedOrg2.PK;

				//Setup some organizations have same IVA# and different IVA#
				AddIVACustomsCode(retainedOrg, "111");
				AddIVACustomsCode(dissolvedOrg1, "111");
				AddIVACustomsCode(dissolvedOrg2, "222");

				CreatePostedTransaction(retainedOrg, "INV01", company);
				CreatePostedTransaction(dissolvedOrg1, "INV01", company);
				CreatePostedTransaction(dissolvedOrg2, "INV01", company);

				Factory.Save();

				var merge = new MergeOrgHeader(Factory, null)
				{
					HasToLoadSimilarOrgs = true,
					NewOrganisationPk = retainedOrg.PK
				};
				merge.OldOrgsCollectionByName.RemoveAll();
				merge.OldOrgsCollectionByName.Add(dissolvedOrg1);
				merge.OldOrgsCollectionByName.Add(dissolvedOrg2);

				AssertEquals("Precondition number of organizations to be processed", 2, merge.OldOrgsCollectionByName.Count);

				using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
				{
					form.Show();
					ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					processButton.PerformClick();

					CombineAssertions(() =>
					{
						AssertEquals("Dissolved Org 1 should be deleted after merge", true, dissolvedOrg1.IsDeleted);
						AssertEquals("Dissolved Org 2 should not be deleted after merge", false, dissolvedOrg2.IsDeleted);
					});

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

					dissolvedOrg1 = newFactory.Load<OrgHeader>(dissolvedOrg1PK);
					dissolvedOrg2 = newFactory.Load<OrgHeader>(dissolvedOrg2PK);

					CombineAssertions(() =>
					{
						AssertNull("Dissolved Org 1 should not exist in the database after merge", dissolvedOrg1);

						AssertNotNull("Dissolved Org 2 should exist in the database after merge", dissolvedOrg2);
					});
				}
			}
		}

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

		[RequiresSTA]
		public void TestSuppressedDocumentsShouldBeMerged()
		{
			var orgRetained = Factory.NewWithValidTestData<OrgHeader>();
			var orgDissolved1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgDissolved2 = Factory.NewWithValidTestData<OrgHeader>();

			orgRetained.OH_Code = "ORGRET";
			orgDissolved1.OH_Code = "ORGDIS1";
			orgDissolved2.OH_Code = "ORGDIS2";

			orgRetained.OH_FullName = "ORGMERGE";
			orgDissolved1.OH_FullName = "ORGMERGE";
			orgDissolved2.OH_FullName = "ORGMERGE";

			orgRetained.MainAddress.OA_Address1 = "TEST ADDRESS";
			orgDissolved1.MainAddress.OA_Address1 = "TEST ADDRESS";
			orgDissolved2.MainAddress.OA_Address1 = "TEST ADDRESS";

			var org1SuppressedDocs = orgDissolved1.SuppressedDocuments.AddNew();
			org1SuppressedDocs.OD_DocumentGroup = ContactType.Receivables.ToString();
			org1SuppressedDocs.OD_DefaultContact = true;

			var org2SuppressedDocs = orgDissolved2.SuppressedDocuments.AddNew();
			org2SuppressedDocs.OD_DocumentGroup = ContactType.Miscellaneous.ToString();
			org2SuppressedDocs.OD_DefaultContact = true;

			Factory.Save();

			var orgRetainedContactCollection = new MergeOrgContactCollection(Factory, orgRetained, null);
			AssertEquals("PreCondition: Retained org does not have any contact", false, orgRetainedContactCollection.Any());

			var merge = new MergeOrgHeader(Factory, null)
			{
				HasToLoadSimilarOrgs = true,
				NewOrganisationPk = orgRetained.PK
			};

			using (var form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();

				//By default findByName is true
				var findButton = (ZButton)form.Controls.Find("FindButton", true)[0];
				var contactGrid = (ZGrid)form.Controls.Find("MergeContactsZGrid", true)[0];

				AssertEquals("PreCondition: Grid BindTo", "OldOrgContactCollectionForSimilarOrgsByName", contactGrid.BindTo);

				var mergeOrgHeader = (MergeOrgHeader)contactGrid.DataSource;
				AssertNotNull("PreCondition: MergeOrgHeader should not be null", mergeOrgHeader);
				AssertNotNull("PreCondition: MergeOrgHeader.OldOrgContactCollectionForSimilarOrgsByName should be not null", mergeOrgHeader.OldOrgContactCollectionForSimilarOrgsByName);

				findButton.PerformClick();

				//Do Merge Process
				UnitTestUserNotification.Instance.ClearMessages();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				processButton.PerformClick();

				Assert("Merge result message", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Organization transferred successfully."));

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var newOrgRetained = newFactory.Load<OrgHeader>(orgRetained.PK);

				AssertNotNull("Retained org should exist in the database after merge", orgRetained);
				AssertEquals("Number of suppressed documents of retained org should have", 2, newOrgRetained.SuppressedDocuments.Count);
			}
		}

		[TestDate(2021, 09, 15, 10, 38, 57, 100)]
		public void TestBillingCreated_MergeSuccess()
		{
			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var merge = new MergeOrgHeader(Factory, null)
			{
				HasToLoadSimilarOrgs = true,
				NewOrganisationPk = retainedOrg.PK
			};
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(dissolvedOrg);

			using (var form = new MergeIntoOrgHeaderFormForTest(merge))
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
					BillingTransactionTestHelper.AssertOMGBillingTransaction(billingTransaction, new DateTime(2021, 09, 15, 10, 38, 57, 100), $"ACT=OMS|SRC=GRD", retainedOrg.PK.ToString(), "ALL");
					Assert(Regex.IsMatch(billingTransaction.Reference4, @"^[\d]+ms$"));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				});
			}
		}

		[TestDate(2021, 09, 15, 10, 38, 57, 100)]
		public void TestBillingCreated_MergeFailed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;
				var retainedOrg = CreatePTOrgHeader("RETAINED ORG", "TEST ADDRESS", "PT", "RETORG");
				var dissolvedOrg = CreatePTOrgHeader("DISSOLVED ORG", "TEST ADDRESS", "PT", "DISSORG1");

				AddIVACustomsCode(retainedOrg, "111");
				CreatePostedTransaction(retainedOrg, "INV01", company);
				AddIVACustomsCode(dissolvedOrg, "111");
				CreatePostedTransaction(dissolvedOrg, "INV01", company);
				retainedOrg.OH_IsActive = false;
				Factory.Save();

				var merge = new MergeOrgHeader(Factory, null)
				{
					HasToLoadSimilarOrgs = true,
					NewOrganisationPk = retainedOrg.PK
				};
				merge.OldOrgsCollectionByName.RemoveAll();
				merge.OldOrgsCollectionByName.Add(dissolvedOrg);

				using (var form = new MergeIntoOrgHeaderFormForTest(merge))
				{
					form.Show();
					var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					processButton.PerformClick();
					CombineAssertions(() =>
					{
						AssertEquals("Dissolved Org 1 should not be deleted after merge", false, dissolvedOrg.IsDeleted);
						AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
						var billingTransaction = BillingTransactionTestHelper.GetBillingTransaction(TestConnection);
						Assert(billingTransaction != null);
						BillingTransactionTestHelper.AssertOMGBillingTransaction(billingTransaction, new DateTime(2021, 09, 15, 10, 38, 57, 100), $"ACT=OMF|SRC=GRD", retainedOrg.PK.ToString(), "ALL", "Failed");
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					});
				}
			}
		}

		[TestDate(2021, 09, 15, 10, 38, 57, 100)]
		public void TestBillingCreated_MergeCanncelled()
		{
			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PROD1";
			product1.OP_Desc = "MERGE-TEST";
			product1.RelatedOrganisations.AddOwner(retainedOrg);

			var product1dup = Factory.New<OrgSupplierPart>();
			product1dup.OP_PartNum = "PROD1";
			product1dup.OP_Desc = "MERGE-TEST";
			product1dup.RelatedOrganisations.AddOwner(dissolvedOrg);

			var merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = retainedOrg.PK;
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(dissolvedOrg);
			Factory.Save();

			using (var form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var actualResolutionCount = 0;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(formOrDialog =>
				{
					if (formOrDialog is ProductOrgMergerForm productMergerForm)
					{
						var productMerger = (ProductOrgMerger)productMergerForm.BusinessEntity;
						productMerger.DeactivateSingleRelationDuplicates();
						actualResolutionCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				processButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Product org merge has been cancelled", 1, actualResolutionCount);
					var billingTransaction = BillingTransactionTestHelper.GetBillingTransaction(TestConnection);
					Assert(billingTransaction != null);
					BillingTransactionTestHelper.AssertOMGBillingTransaction(billingTransaction, new DateTime(2021, 09, 15, 10, 38, 57, 100), $"ACT=OMC|SRC=GRD", retainedOrg.PK.ToString(), "ALL");
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				});
			}
		}

		[TestDate(2021, 09, 15, 10, 38, 57, 100)]
		[RequiresSTA]
		public void TestBillingCreated_MergeALLNotAllowed()
		{
			var retainedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity.P8_OH = dissolvedOrg.PK;
			var orgCommissionAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			orgCommissionAgreement.CA0_P8 = orgOpportunity.PK;
			orgCommissionAgreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;

			var merge = new MergeOrgHeader(Factory, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = retainedOrg.PK;
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(dissolvedOrg);
			Factory.Save();

			using (var form = new MergeIntoOrgHeaderFormForTest(merge))
			{
				form.Show();
				var processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				processButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertContains("Merge Organizations is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
					var billingTransaction = BillingTransactionTestHelper.GetBillingTransaction(TestConnection);
					Assert(billingTransaction != null);
					BillingTransactionTestHelper.AssertOMGBillingTransaction(billingTransaction, new DateTime(2021, 09, 15, 10, 38, 57, 100), $"ACT=OMF|SRC=GRD", retainedOrg.PK.ToString(), "ALL", "Failed");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				});
			}
		}
	}

	public class BillingTransactionTestHelper : TestCase
	{
		public static BillingTransaction GetBillingTransaction(DbConnection connection)
		{
			var billingTransactions = new List<BillingTransaction>();
			using (var cmd = connection.Command("SELECT SUD_Data FROM dbo.StmUsageData"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					billingTransactions.Add(BillingManager.DecryptTransaction(reader[0].ToString(), BillingManager.CurrentSchemaVersion));
				}
			}
			AssertEquals(1, billingTransactions.Count);
			return billingTransactions.Single();
		}

		public static void AssertOMGBillingTransaction(BillingTransaction billing, DateTime expectedServiceOccuredUTC, string expectedReference1, params string[] otherExpectedReferences)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			AssertEquals("MDM", billing.Category);
			AssertEquals("OMG", billing.PriceItemCode);
			AssertEquals(1, billing.BillableCount);
			AssertEquals("CW1", billing.ReportingSource);
			AssertEquals(expectedServiceOccuredUTC, billing.ServiceOccuredUTC);
			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, billing.ClientID);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, billing.Branch);
			AssertEquals($"{registrationKey.SystemId}.{GlbCompany.CurrentCompany.GC_Code}", billing.ClientNumber);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, billing.ClientStaffCode);
			AssertEquals(1, billing.Version);
			AssertEquals(expectedReference1, billing.Reference1);
			var otherReferenceInBilling = new Queue<string>(new[] { billing.Reference2, billing.Reference3, billing.Reference4, billing.Reference5 });
			if (otherExpectedReferences.Any())
			{
				var lengthOfParams = otherExpectedReferences.Length;
				var safeMaxLength = lengthOfParams > 4 ? 4 : lengthOfParams;
				for (var i = 0; i < safeMaxLength; i++)
				{
					AssertEquals(otherReferenceInBilling.Dequeue(), otherExpectedReferences[i]);
				}
			}
		}
	}

	[UseSnapshotProtection]
	public class MergeIntoOrgHeaderFormTest_NonTransactional : TestCase
	{
		[RequiresSTA]
		public void TestProcessWithException()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			OrgHeader org1 = factory1.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = factory1.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = factory1.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~temp1~";
			org2.OH_Code = "~temp2~";
			org3.OH_Code = "~temp3~";
			org1.MainAddress.OA_Address1 = "address 1";
			org2.MainAddress.OA_Address1 = "address 2";
			org3.MainAddress.OA_Address1 = "address 3";

			factory1.Save();
			ZGuid org1PK = org1.PK;
			ZGuid org2PK = org2.PK;
			ZGuid org3PK = org3.PK;

			var merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(factory1, null);
			merge.HasToLoadSimilarOrgs = true;
			merge.NewOrganisationPk = org2.PK;
			merge.OldOrgsCollectionByName.RemoveAll();
			merge.OldOrgsCollectionByName.Add(org1);
			merge.OldOrgsCollectionByName.Add(org3);

			try
			{
				merge.ThrowExceptionOnOrgDelete = true;
				using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
				{
					form.ThrowExceptionOnOrgDelete = true;
					form.Show();
					ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					processButton.PerformClick();

					BusinessObjectFactory factory = new BusinessObjectFactory();
					org1 = factory.Load<OrgHeader>(org1PK);
					org2 = factory.Load<OrgHeader>(org2PK);
					org3 = factory.Load<OrgHeader>(org3PK);

					AssertNotNull("Should not be deleted", org1);
					AssertNotNull("Should not be deleted", org2);
					AssertNotNull("Should not be deleted", org3);
					AssertEquals("should have initial address - the whole transaction is rolled back", 1, org1.Addresses.Count);
					AssertEquals("should have initial address - the whole transaction is rolled back", 1, org2.Addresses.Count);
					AssertEquals("should have initial address - the whole transaction is rolled back", 1, org3.Addresses.Count);
				}
			}
			finally
			{
				//now have to delete everything
				merge = new MergeOrgHeaderTest.MergeOrgHeaderForTest(factory1, null);
				merge.HasToLoadSimilarOrgs = true;
				merge.NewOrganisationPk = org2PK;
				merge.OldOrgsCollectionByName.RemoveAll();
				merge.OldOrgsCollectionByName.Add(org1);
				merge.OldOrgsCollectionByName.Add(org3);
				using (MergeIntoOrgHeaderFormForTest form = new MergeIntoOrgHeaderFormForTest(merge))
				{
					form.Show();
					ZButton processButton = (ZButton)form.Controls.Find("ProcessButton", true)[0];

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					processButton.PerformClick();

					BusinessObjectFactory factory = new BusinessObjectFactory();
					org1 = factory.Load<OrgHeader>(org1PK);
					org2 = factory.Load<OrgHeader>(org2PK);
					org3 = factory.Load<OrgHeader>(org3PK);

					AssertNull("Should be deleted", org1);
					AssertNull("Should be deleted", org3);
					AssertNotNull("Should not be deleted", org2);

					org2.Delete();
					factory.Save();

					factory = new BusinessObjectFactory();
					org2 = factory.Load<OrgHeader>(org2PK);
					AssertNull("Should be deleted", org2);
				}
			}
		}
	}

	class MergeIntoOrgHeaderFormForTest : MergeIntoOrgHeaderForm
	{
		public MergeIntoOrgHeaderFormForTest(MergeOrgHeader mergeOrg)
			: base(mergeOrg)
		{ }

		internal CurrentQueryMode Mode
		{
			get
			{
				return CurrentMode;
			}
		}

		public OrgHeaderCollection CurrentCollection
		{
			get
			{
				return (BusinessEntity as MergeOrgHeader).CurrentOrgHeaderCollection;
			}
		}

		public MergeOrgAddressCollection CurrentOrgAddressCollection
		{
			get
			{
				return (BusinessEntity as MergeOrgHeader).CurrentOrgAddressCollection;
			}
		}

		public MergeOrgContactCollection CurrentOrgContactCollection
		{
			get
			{
				return (BusinessEntity as MergeOrgHeader).CurrentOrgContactCollection;
			}
		}

		public bool ThrowExceptionOnOrgDelete { get; set; }
		protected override MergeOrgHeader GetLocalMergeOrgHeader(OrgHeader org)
		{
			var result = new MergeOrgHeaderTest.MergeOrgHeaderForTest(new BusinessObjectFactory(), org);
			result.ThrowExceptionOnOrgDelete = ThrowExceptionOnOrgDelete;

			return result;
		}
	}
}
