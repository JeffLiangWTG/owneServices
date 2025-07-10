using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDecRefsCollection))]
	sealed class JobDecRefsCollectionTest : ActiveBusinessObjectCollectionTestCase<JobDecRefsCollection>
	{
		public void TestSetValueForJobDecRefsAndDeleteDuplicateWhenEmpty()
		{
			var declarationRefs = Declaration.DeclarationRefs;
			AssertEquals("declarationRefs.Count", 0, declarationRefs.Count);
			var descriptionCount = 0;
			var descriptionInfo_ValueChanged = new EventHandler((object sender, EventArgs e) => descriptionCount++);
			try
			{
				Declaration.JE_GoodsDescriptionInfo.ValueChanged += descriptionInfo_ValueChanged;
				var refNo = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", null, "BUILDER", Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo.J3_ReferenceType", "BOB", refNo.J3_ReferenceType);
				AssertEquals("refNo.J3_ReferenceNumber", "BUILDER", refNo.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 1, descriptionCount);
				AssertEquals("declarationRefs.Count", 1, declarationRefs.Count);
				var refNo1 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", refNo, "DESTROYER", Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo", refNo, refNo1);
				AssertEquals("refNo.J3_ReferenceType", "BOB", refNo.J3_ReferenceType);
				AssertEquals("refNo.J3_ReferenceNumber", "DESTROYER", refNo.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 2, descriptionCount);
				AssertEquals("declarationRefs.Count", 1, declarationRefs.Count);
				var refNo2 = declarationRefs.AddNew("BOB", "BUILDER");
				if (refNo1.PK > refNo2.PK)
				{
					refNo1 = refNo2;
					refNo2 = refNo;
				}
				var refNo3 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", refNo1, "THE", Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J3_ReferenceType", "BOB", refNo1.J3_ReferenceType);
				AssertEquals("refNo1.J3_ReferenceNumber", "THE", refNo1.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 3, descriptionCount);
				AssertEquals("declarationRefs.Count", 2, declarationRefs.Count);
				refNo3 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", refNo2, "THE", Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo2", refNo2, refNo3);
				AssertEquals("refNo2.J3_ReferenceType", "BOB", refNo2.J3_ReferenceType);
				AssertEquals("refNo2.J3_ReferenceNumber", "THE", refNo2.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 4, descriptionCount);
				AssertEquals("declarationRefs.Count", 2, declarationRefs.Count);
				refNo3 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", null, "D", Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J3_ReferenceType", "BOB", refNo1.J3_ReferenceType);
				AssertEquals("refNo1.J3_ReferenceNumber", "D", refNo1.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 5, descriptionCount);
				AssertEquals("declarationRefs.Count", 2, declarationRefs.Count);
				refNo3 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", null, ZString.Empty, Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J3_ReferenceType", "BOB", refNo1.J3_ReferenceType);
				AssertEquals("refNo1.J3_ReferenceNumber", ZString.Empty, refNo1.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 6, descriptionCount);
				AssertEquals("declarationRefs.Count", 1, declarationRefs.Count);
				AssertEquals("refNo2.IsDeleted", true, refNo2.IsDeleted);
				refNo2 = declarationRefs.AddNew("BOB", "BUILDER");
				refNo3 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", refNo2, ZString.Empty, Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo2", refNo2, refNo3);
				AssertEquals("refNo2.J3_ReferenceType", "BOB", refNo2.J3_ReferenceType);
				AssertEquals("refNo2.J3_ReferenceNumber", ZString.Empty, refNo2.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 7, descriptionCount);
				AssertEquals("declarationRefs.Count", 1, declarationRefs.Count);
				AssertEquals("refNo1.IsDeleted", true, refNo1.IsDeleted);
				refNo1 = declarationRefs.AddNew("BOB", "BUILDER");
				refNo3 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("JOE", refNo1, ZString.Empty, Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J3_ReferenceType", "JOE", refNo1.J3_ReferenceType);
				AssertEquals("refNo1.J3_ReferenceNumber", ZString.Empty, refNo1.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 8, descriptionCount);
				AssertEquals("declarationRefs.Count", 2, declarationRefs.Count);
				AssertCollectionContains("refNo1", refNo1, declarationRefs);
				AssertCollectionContains("refNo2", refNo2, declarationRefs);
				refNo3 = declarationRefs.SetValueForJobDecRefsAndDeleteDuplicateWhenEmpty("BOB", refNo1, "D", Declaration.JE_GoodsDescriptionInfo);
				AssertEquals("refNo1", refNo1, refNo3);
				AssertEquals("refNo1.J3_ReferenceType", "BOB", refNo1.J3_ReferenceType);
				AssertEquals("refNo1.J3_ReferenceNumber", "D", refNo1.J3_ReferenceNumber);
				AssertEquals("descriptionCount", 9, descriptionCount);
				AssertEquals("declarationRefs.Count", 2, declarationRefs.Count);
				AssertCollectionContains("refNo1", refNo1, declarationRefs);
				AssertCollectionContains("refNo2", refNo2, declarationRefs);
			}
			finally
			{
				Declaration.JE_GoodsDescriptionInfo.ValueChanged -= descriptionInfo_ValueChanged;
			}
		}

		public void TestGetFirstJobDecRefs()
		{
			var declarationRefs = Declaration.DeclarationRefs;
			var refNo1 = declarationRefs.AddNew("BOB", "BUILDER");
			var refNo2 = declarationRefs.AddNew("BOB", "DESTROYER");
			if (refNo1.PK > refNo2.PK)
			{
				var refNo = refNo1;
				refNo1 = refNo2;
				refNo2 = refNo;
			}
			AssertEquals(refNo1, declarationRefs.GetFirstJobDecRefs("BOB"));
			declarationRefs.ApplySort(JobDecRefs.Schema.J3_ReferenceNumber, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals(refNo1, declarationRefs.GetFirstJobDecRefs("BOB"));
			declarationRefs.ApplySort(JobDecRefs.Schema.J3_ReferenceNumber, System.ComponentModel.ListSortDirection.Descending);
			AssertEquals(refNo1, declarationRefs.GetFirstJobDecRefs("BOB"));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var lineRefs = Factory.New<JobDecRefs>();
			lineRefs.J3_JE = Declaration.PK;
			return lineRefs;
		}

		protected override JobDecRefsCollection GetCollectionToTest()
		{
			return new JobDecRefsCollection(Declaration);
		}

		BaseJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<BaseJobDeclarationForTest>()); }
		}
		BaseJobDeclaration declaration;

		public class BaseJobDeclarationForTest : BaseJobDeclaration
		{
			public BaseJobDeclarationForTest(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{ }

			protected internal override bool SupportDeclarationRefs
			{
				get { return true; }
			}
		}
	}
}
