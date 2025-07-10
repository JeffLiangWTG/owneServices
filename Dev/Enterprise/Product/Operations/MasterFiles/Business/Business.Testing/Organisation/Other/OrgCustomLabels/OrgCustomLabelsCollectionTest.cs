using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCustomLabelsCollection))]
	public class OrgCustomLabelsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new OrgCustomLabelsCollection(org, Factory);
		}

		public void TestFindByFieldName()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader org = factory.New<OrgHeader>();
			OrgCustomLabelsCollection collection = new OrgCustomLabelsCollection(org, factory);

			OrgCustomLabels newLabel;
			newLabel = collection.AddNew();
			newLabel.OT_FieldName = "Field1";
			newLabel = collection.AddNew();
			newLabel.OT_FieldName = "Field2";

			AssertEquals("Field1", collection.FindByFieldName("Field1").OT_FieldName);
			AssertEquals("Field2", collection.FindByFieldName("Field2").OT_FieldName);
		}

		public void TestHasLabelType()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			AssertEquals(false, organisation.CustomLabels.HasLabelType("AAA"));

			OrgCustomLabels label = organisation.CustomLabels.AddNew();
			label.OT_Type = "AAA";
			AssertEquals(true, organisation.CustomLabels.HasLabelType("AAA"));

			label.OT_Type = "BBB";
			AssertEquals(false, organisation.CustomLabels.HasLabelType("AAA"));
		}

		public void TestFindByFieldNameAndType()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCustomLabels label = organisation.CustomLabels.AddNew();
			label.OT_Type = "AAA";
			label.OT_FieldName = "MarkUpPercentage";

			OrgCustomLabels label2 = organisation.CustomLabels.AddNew();
			label2.OT_Type = "BBB";
			label2.OT_FieldName = "1";

			OrgCustomLabels label3 = organisation.CustomLabels.AddNew();
			label3.OT_Type = "BBB";
			label3.OT_FieldName = "2";

			OrgCustomLabels label4 = organisation.CustomLabels.AddNew();
			label4.OT_FieldName = "3";

			AssertEquals(label, organisation.CustomLabels.FindByFieldNameAndType("MarkupPercentage", "AAA"));
			AssertEquals(label3, organisation.CustomLabels.FindByFieldNameAndType("2", ""));
			AssertEquals(label4, organisation.CustomLabels.FindByFieldNameAndType("3", ""));
			AssertNull(organisation.CustomLabels.FindByFieldNameAndType("3", "AAA"));
		}
	}
}
