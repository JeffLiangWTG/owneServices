using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOuterPackage))]
	class HVLVOuterPackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadList()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();

			outerPackage.HVO_HVL_LoadList = loadList.PK;

			AssertEquals(loadList, outerPackage.LoadList);
		}

		public void TestOuterPackageItems()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var activeItem1 = Factory.NewWithValidTestData<HVLVItem>();
			var activeItem2 = Factory.NewWithValidTestData<HVLVItem>();
			var inactiveItem1 = Factory.NewWithValidTestData<HVLVItem>();
			var inactiveItem2 = Factory.NewWithValidTestData<HVLVItem>();
			inactiveItem1.HVI_IsActive = false;
			inactiveItem2.HVI_IsActive = false;

			outerPackage.Items.Add(activeItem1);
			outerPackage.Items.Add(activeItem2);
			outerPackage.Items.Add(inactiveItem1);
			outerPackage.Items.Add(inactiveItem2);

			AssertEquals("There should be a total of four items", 4, outerPackage.Items.Count);
			AssertEquals("There should be two active items", 2, outerPackage.ActiveItems.Count());
		}

		public void TestIDocumentSupportable()
		{
			var documentSupportable = Factory.NewWithValidTestData<HVLVOuterPackage>() as IDocumentSupportable;
			AssertNotNull(documentSupportable);
			AssertType(typeof(HVLVOuterPackageDocumentSupporter), documentSupportable.DocumentSupporter);
		}

		public void TestHVO_Status()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			AssertEquals("OPN", outerPackage.HVO_Status);
			string[] possibleStatuses = { "CLS", "LDG", "CON", "OPN" };
			foreach (var status in possibleStatuses)
			{
				AssertNoExceptionThrown(() =>
				{
					outerPackage.HVO_Status = status;
					Factory.Save();
				});
			}

			outerPackage.HVO_Status = "IAN";
			try
			{
				Factory.Save();
			}
			catch (Exception e)
			{
				CombineAssertions("should not be able to save when HVO_Status is not part of schema constraints", () =>
				{
					Assert(e.Message.Contains("Error Saving Record"));
					Assert(e.Message.Contains("Tablename: HVLVOuterPackage"));
				});
			}
		}

		[UseSnapshotProtection(true)]
		public void TestOuterPackageBarcodeFallBack()
		{
			var factory = new BusinessObjectFactory();
			var orgCurrentCompany = factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);

			var outerPackage = factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_PackageBarcode = "testbarcode";
			factory.Save();

			AssertEquals("barcode is user entered", "testbarcode", outerPackage.HVO_PackageBarcode);

			HVLVTestHelper.SetGS1FountainOnOrg(orgCurrentCompany, "9999999");
			factory.Save();

			outerPackage = factory.NewWithValidTestData<HVLVOuterPackage>();
			factory.Save();

			AssertEquals("fallback to gs1", "099999990000000010", outerPackage.HVO_PackageBarcode);

			orgCurrentCompany.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(cusCode => cusCode.OK_CodeType == OrgCusCode.CodeTypes.GS1).Delete();
			factory.Save();

			outerPackage = factory.NewWithValidTestData<HVLVOuterPackage>();
			factory.Save();

			AssertEquals("fallback to number fountain", "HVO000000000000001", outerPackage.HVO_PackageBarcode);
		}

		public void TestUnitOfMeasureProperties_WhenSetValueLowerCase_GetIsUpperCase()
		{
			var outerPackage = Factory.New<HVLVOuterPackage>();
			outerPackage.HVO_VolumeUQ = "m3";
			outerPackage.HVO_WeightUQ = "kg";
			outerPackage.HVO_UnitOfDimension = "m";

			AssertEquals("M3", outerPackage.HVO_VolumeUQ);
			AssertEquals("KG", outerPackage.HVO_WeightUQ);
			AssertEquals("M", outerPackage.HVO_UnitOfDimension);
		}

		public void TestNoAuditLog()
		{
			var newFactoryForLoading = new BusinessObjectFactory();
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var query = new ZQuery(StmALogSchema.SL_Parent, outerPackage.PK);
			Factory.Save();
			Assert("Not expecting Add event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			outerPackage.HVO_Height = 20m;
			Factory.Save();
			Assert("Not expecting Edit event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			outerPackage.Delete();
			Factory.Save();
			Assert("Not expecting Delete event.", !newFactoryForLoading.Exists(typeof(StmALog), query));
		}

		#region IWorkflowProvider Tests

		[TestedType(typeof(HVLVOuterPackage))]
		public class HVLVOuterPackageWorkflowProviderTest : WorkflowProviderTest<HVLVOuterPackage, HVLVOuterPackageProcessTaskCollection>
		{
			public void TestGetWorkflowInformationProvider()
			{
				AssertNull(((IWorkflowProvider)OuterPackage).GetWorkflowInformationProvider());
			}

			public void TestWorkflowItems()
			{
				var provider = OuterPackage as IWorkflowProvider;
				AssertNotNull(provider);
				AssertNotNull(provider.WorkflowItems);
				Assert("IsRegisteredEditableChildObject", OuterPackage.IsRegisteredEditableChildObject(provider.WorkflowItems));
			}

			public void TestGetTemplateSelectionCriteria()
			{
				AssertNotNull(((IWorkflowProvider)OuterPackage).GetTemplateSelectionCriteria());
				Assert("is ColumnValueRanker", ((IWorkflowProvider)OuterPackage).GetTemplateSelectionCriteria() is ColumnValueRanker);
			}

			#region Implementation

			protected override CargoWise.Types.ZString ExpectedWorkflowType => WorkflowDescriptors.HVLVOuterPackageWorkflowDescriptorCode;

			HVLVOuterPackage OuterPackage => BusinessObject;

			#endregion
		}

		#endregion
	}
}
