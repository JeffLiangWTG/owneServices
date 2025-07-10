using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomLabelInfoListTest : TestCaseWithFactory
	{
		public void TestHasEnabledFields()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.New<OrgHeader>();
			var list = new CustomLabelInfoList(typeof(DummyBusinessObject), org, (NoResString)"", Factory);
			list.Add("F1", AutoDummyBizo.Schema.Z0_Code, (NoResString)"x");
			AssertEquals(false, list.HasEnabledFields());

			OrgCustomLabels label = org.CustomLabels.AddNew();
			label.OT_FieldName = "F2";
			list.Add("F2", AutoDummyBizo.Schema.Z0_Code, (NoResString)"x");
			AssertEquals(false, list.HasEnabledFields());

			OrgCustomLabels label2 = org.CustomLabels.AddNew();
			label2.OT_FieldName = "F3";
			label2.OT_Caption = "x";
			list.Add("F3", AutoDummyBizo.Schema.Z0_Code, (NoResString)"x");
			AssertEquals(true, list.HasEnabledFields());
		}

		[ExpectNoExceptions]
		public void TestOrgNull()
		{
			new CustomLabelInfoList(typeof(DummyBusinessObject), null, (NoResString)"", Factory);
		}

		// Make sure the custom labels are transferred from a source factory to a target factory so that when the user
		// edits the label on the org form, they are propagated to the OrdersForm.
		public void TestChangingCustomLabelsInOtherFactory()
		{
			var emptyFilter = new ZQuery();
			var sourceFactory = new BusinessObjectFactory();
			var targetFactory = new BusinessObjectFactory();
			var sourceOrg = sourceFactory.LoadTop1<OrgHeader>(emptyFilter);

			sourceOrg.CustomLabels.RemoveAndDeleteAll();
			sourceFactory.Save();

			var targetOrg = targetFactory.LoadTop1<OrgHeader>(emptyFilter);
			OrgCustomLabelsCollection customLabelsCollectionInTargetFactory = targetOrg.CustomLabels;

			OrgCustomLabels newSourceLabel = sourceOrg.CustomLabels.AddNew();
			newSourceLabel.OT_FieldName = "xxx";
			newSourceLabel.OT_Caption = "xxx";

			// Factory.StartManagingCollectionForAddingBusinessObjects should be called here
			var someOtherFieldInfo = new CustomLabelInfo("xxx", "xxx", typeof(string), (NoResString)"caption", (NoResString)"hint",
				CustomLabelStyles.None, targetOrg, Factory);
			var customLabelInfo = new CustomLabelInfo("xxx", "xxx", typeof(string), (NoResString)"caption", (NoResString)"hint", CustomLabelStyles.None, targetOrg, Factory);

			AssertEquals("CustomLabelsCollection count before test", 0, customLabelsCollectionInTargetFactory.Count);
			sourceFactory.Save();
			AssertEquals("CustomLabelsCollection count after test", 1, customLabelsCollectionInTargetFactory.Count);
		}

		public void TestSortOrgCustomLabels()
		{
			var org = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery());
			org.CustomLabels.RemoveAndDeleteAll();

			OrgCustomLabels label1 = org.CustomLabels.AddNew();
			label1.OT_Caption = "c1";
			label1.OT_FieldName = "f1";
			label1.OT_Position = 1;

			OrgCustomLabels label2 = org.CustomLabels.AddNew();
			label2.OT_Caption = "c2";
			label2.OT_FieldName = "f2";
			label2.OT_Position = 3;

			OrgCustomLabels label3 = org.CustomLabels.AddNew();
			label3.OT_Caption = "c3";
			label3.OT_FieldName = "f3";
			label3.OT_Position = 2;

			org.Factory.Save();

			CustomLabelInfoList list = new CustomLabelInfoList(typeof(OrgHeader), org, (NoResString)"", Factory);
			CustomLabelInfo labelInfo1 = new CustomLabelInfo("f1", "p1", typeof(string), (NoResString)"", (NoResString)"", CustomLabelStyles.None, org, Factory);
			CustomLabelInfo labelInfo2 = new CustomLabelInfo("f2", "p2", typeof(string), (NoResString)"", (NoResString)"", CustomLabelStyles.None, org, Factory);
			CustomLabelInfo labelInfo3 = new CustomLabelInfo("f3", "p3", typeof(string), (NoResString)"", (NoResString)"", CustomLabelStyles.None, org, Factory);

			list.Add(labelInfo1);
			list.Add(labelInfo2);
			list.Add(labelInfo3);

			AssertEquals(labelInfo1, list[0]);
			AssertEquals(labelInfo2, list[1]);
			AssertEquals(labelInfo3, list[2]);

			list.SortByPosition();

			AssertEquals(labelInfo1, list[0]);
			AssertEquals(labelInfo2, list[2]);
			AssertEquals(labelInfo3, list[1]);
		}

		[ExpectNoExceptions]
		public void TestSortPartCustomLabelInfo()
		{
			var org = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery());
			org.Factory.Save();

			CustomLabelInfoList list = new CustomLabelInfoList(typeof(OrgHeader), org, (NoResString)"", Factory);
			PartCustomLabelInfo partInfo1 = new PartCustomLabelInfo("Part Info 1", "", "", null, (NoResString)"Caption 1", GlbCompany.CurrentCompany.OrgProxy, Factory);
			PartCustomLabelInfo partInfo2 = new PartCustomLabelInfo("Part Info 2", "", "", null, (NoResString)"Caption 2", GlbCompany.CurrentCompany.OrgProxy, Factory);
			PartCustomLabelInfo partInfo3 = new PartCustomLabelInfo("Part Info 3", "", "", null, (NoResString)"Caption 3", GlbCompany.CurrentCompany.OrgProxy, Factory);
			list.Add(partInfo1);
			list.Add(partInfo2);
			list.Add(partInfo3);

			list.SortByPosition();
		}
	}
}
