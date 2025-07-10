using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ProductOrgMergerForm))]
	sealed class OrgProductMergeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mergeOrgHeader = new MergeOrgHeader(Factory, null, null);
			return new ProductOrgMergerForm(new ProductOrgMerger(mergeOrgHeader));
		}

		[RequiresSTA]
		public void TestDeactivateSingleAndMultiRelation()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TESTORG2";

			var org3 = Factory.New<OrgHeader>();
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
			product2dup.RelatedOrganisations.AddOwner(org2);
			product2dup.RelatedOrganisations.AddOwner(org3);

			Factory.Save();

			var mergeData = new MergeOrgHeader(Factory, org2, org1);
			var productMerger = new ProductOrgMerger(mergeData);
			using (var form = new ProductOrgMergerForm(productMerger))
			{
				form.Show();

				AssertEquals(true, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(true, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(true, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(false, form.ProceedButton.Enabled);
				AssertEquals(2, productMerger.TotalDuplicateCount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.DeactivateSingleRelationButton.PerformClick();

				AssertEquals(false, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(true, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(true, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(false, form.ProceedButton.Enabled);
				AssertEquals(1, productMerger.TotalDuplicateCount);

				var factory2 = NewFactory();
				AssertEquals(false, factory2.Load<OrgSupplierPart>(product1dup.PK).OP_IsActive);
				AssertEquals(true, factory2.Load<OrgSupplierPart>(product2dup.PK).OP_IsActive);
				AssertEquals(1, factory2.Load<OrgSupplierPart>(product1dup.PK).RelatedOrganisations.Count);
				AssertEquals(2, factory2.Load<OrgSupplierPart>(product2dup.PK).RelatedOrganisations.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.DeactivateMultiRelationButton.PerformClick();

				AssertEquals(false, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(false, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(false, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(true, form.ProceedButton.Enabled);
				AssertEquals(0, productMerger.TotalDuplicateCount);

				var factory3 = NewFactory();
				AssertEquals(false, factory3.Load<OrgSupplierPart>(product1dup.PK).OP_IsActive);
				AssertEquals(false, factory3.Load<OrgSupplierPart>(product2dup.PK).OP_IsActive);
				AssertEquals(1, factory3.Load<OrgSupplierPart>(product1dup.PK).RelatedOrganisations.Count);
				AssertEquals(2, factory3.Load<OrgSupplierPart>(product2dup.PK).RelatedOrganisations.Count);
			}
		}

		public void TestDeactivateSingleRelation()
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
			var productMerger = new ProductOrgMerger(mergeData);
			using (var form = new ProductOrgMergerForm(productMerger))
			{
				form.Show();

				AssertEquals(true, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(false, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(false, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(false, form.ProceedButton.Enabled);
				AssertEquals(1, productMerger.TotalDuplicateCount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.DeactivateSingleRelationButton.PerformClick();

				AssertEquals(false, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(false, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(false, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(true, form.ProceedButton.Enabled);
				AssertEquals(0, productMerger.TotalDuplicateCount);

				var factory2 = NewFactory();
				AssertEquals(false, factory2.Load<OrgSupplierPart>(product1dup.PK).OP_IsActive);
				AssertEquals(1, factory2.Load<OrgSupplierPart>(product1dup.PK).RelatedOrganisations.Count);
			}
		}

		[RequiresSTA]
		public void TestDeactivateMultiRelation()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TESTORG2";

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "TESTORG3";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PROD1";
			product1.OP_Desc = "MERGE-TEST";
			product1.RelatedOrganisations.AddOwner(org1);

			var product1dup = Factory.New<OrgSupplierPart>();
			product1dup.OP_PartNum = "PROD1";
			product1dup.OP_Desc = "MERGE-TEST";
			product1dup.RelatedOrganisations.AddOwner(org2);
			product1dup.RelatedOrganisations.AddOwner(org3);

			Factory.Save();

			var mergeData = new MergeOrgHeader(Factory, org2, org1);
			var productMerger = new ProductOrgMerger(mergeData);
			using (var form = new ProductOrgMergerForm(productMerger))
			{
				form.Show();

				AssertEquals(false, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(true, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(true, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(false, form.ProceedButton.Enabled);
				AssertEquals(1, productMerger.TotalDuplicateCount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.DeactivateMultiRelationButton.PerformClick();

				AssertEquals(false, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(false, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(false, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(true, form.ProceedButton.Enabled);
				AssertEquals(0, productMerger.TotalDuplicateCount);

				var factory2 = NewFactory();
				AssertEquals(false, factory2.Load<OrgSupplierPart>(product1dup.PK).OP_IsActive);
				AssertEquals(2, factory2.Load<OrgSupplierPart>(product1dup.PK).RelatedOrganisations.Count);
			}
		}

		[RequiresSTA]
		public void TestRemoveMultiRelation()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TESTORG2";

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "TESTORG3";

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PROD1";
			product1.OP_Desc = "MERGE-TEST";
			product1.RelatedOrganisations.AddOwner(org1);

			var product1dup = Factory.New<OrgSupplierPart>();
			product1dup.OP_PartNum = "PROD1";
			product1dup.OP_Desc = "MERGE-TEST";
			product1dup.RelatedOrganisations.AddOwner(org2);
			product1dup.RelatedOrganisations.AddOwner(org3);

			Factory.Save();

			var mergeData = new MergeOrgHeader(Factory, org2, org1);
			var productMerger = new ProductOrgMerger(mergeData);
			using (var form = new ProductOrgMergerForm(productMerger))
			{
				form.Show();

				AssertEquals(false, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(true, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(true, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(false, form.ProceedButton.Enabled);
				AssertEquals(1, productMerger.TotalDuplicateCount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.RemoveMultiRelationButton.PerformClick();

				AssertEquals(false, form.DeactivateSingleRelationButton.Enabled);
				AssertEquals(false, form.DeactivateMultiRelationButton.Enabled);
				AssertEquals(false, form.RemoveMultiRelationButton.Enabled);
				AssertEquals(true, form.ProceedButton.Enabled);
				AssertEquals(0, productMerger.TotalDuplicateCount);

				var factory2 = NewFactory();
				AssertEquals(true, factory2.Load<OrgSupplierPart>(product1dup.PK).OP_IsActive);
				AssertEquals(1, factory2.Load<OrgSupplierPart>(product1dup.PK).RelatedOrganisations.Count);
			}
		}
	}
}
