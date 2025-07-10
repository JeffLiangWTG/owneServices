using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateRecordFindboxListProviderTest : TestCaseWithFactory
	{
		public void TestCodeFromPrimaryKey()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "DU1";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "DU2";
			Factory.Save();

			var templateFactory = new TemplateRecordBusinessObjectFactory();
			var dummyTemplate = templateFactory.New<DummyTemplateRecordProvider>();
			templateFactory.TemplateRecordProvider = dummyTemplate;
			dummyTemplate.Z0_Code = "DU3";
			dummyTemplate.Z0_Description = "ABC";
			var templateRecord = templateFactory.TemplateRecordFactory.New<StmTemplateRecord>();
			((ITemplateRecordProvider)dummyTemplate).TemplateRecord = templateRecord;
			((ITemplateRecordProvider)dummyTemplate).IsTemplateRecord = true;
			templateRecord.STR_ModuleID = nameof(ModuleId.Dummy);
			templateRecord.STR_ReferenceId = "TR00001000";

			var otherTemplateRecord1 = templateFactory.TemplateRecordFactory.New<StmTemplateRecord>();
			otherTemplateRecord1.STR_ModuleID = nameof(ModuleId.Dummy);
			otherTemplateRecord1.STR_ReferenceId = "TR00001001";

			var otherTemplateRecord2 = templateFactory.TemplateRecordFactory.New<StmTemplateRecord>();
			otherTemplateRecord2.STR_ModuleID = "OtherModule";
			otherTemplateRecord2.STR_ReferenceId = "TR00001002";

			templateFactory.Save();

			var collection = new DummyBizoModuleCollection(Factory);
			var findBoxListProvider = new TemplateRecordFindboxListProvider(collection);

			AssertEquals("DU1", findBoxListProvider.CodeFromPrimaryKey(dummy1.PK));
			AssertEquals("DU2", findBoxListProvider.CodeFromPrimaryKey(dummy2.PK));
			Assert("This record should not exists in db", string.IsNullOrEmpty(findBoxListProvider.CodeFromPrimaryKey(dummyTemplate.PK)));
			AssertEquals("TR00001000", findBoxListProvider.CodeFromPrimaryKey(templateRecord.PK));
			AssertEquals("TR00001001", findBoxListProvider.CodeFromPrimaryKey(otherTemplateRecord1.PK));
			Assert("This template record should not be found because of different module id", string.IsNullOrEmpty(findBoxListProvider.CodeFromPrimaryKey(otherTemplateRecord2.PK)));
		}

		public void TestDescriptionFromPrimaryKey()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "Dummy1";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "Dummy2";
			Factory.Save();

			var templateFactory = new TemplateRecordBusinessObjectFactory();
			var dummyTemplate = templateFactory.New<DummyTemplateRecordProvider>();
			templateFactory.TemplateRecordProvider = dummyTemplate;
			dummyTemplate.Z0_Code = "DU3";
			dummyTemplate.Z0_Description = "ABC";
			var templateRecord = templateFactory.TemplateRecordFactory.New<StmTemplateRecord>();
			((ITemplateRecordProvider)dummyTemplate).TemplateRecord = templateRecord;
			((ITemplateRecordProvider)dummyTemplate).IsTemplateRecord = true;
			templateRecord.STR_ModuleID = nameof(ModuleId.Dummy);
			templateRecord.STR_ReferenceId = "TR00001000";

			var otherTemplateRecord1 = templateFactory.TemplateRecordFactory.New<StmTemplateRecord>();
			otherTemplateRecord1.STR_ModuleID = nameof(ModuleId.Dummy);
			otherTemplateRecord1.STR_ReferenceId = "TR00001001";

			var otherTemplateRecord2 = templateFactory.TemplateRecordFactory.New<StmTemplateRecord>();
			otherTemplateRecord2.STR_ModuleID = "OtherModule";
			otherTemplateRecord2.STR_ReferenceId = "TR00001002";

			templateFactory.Save();

			var collection = new DummyBizoModuleCollection(Factory);
			var findBoxListProvider = new TemplateRecordFindboxListProvider(collection);

			AssertEquals("Dummy1", findBoxListProvider.DescriptionFromPrimaryKey(dummy1.PK));
			AssertEquals("Dummy2", findBoxListProvider.DescriptionFromPrimaryKey(dummy2.PK));
			Assert("This record should not exists in db", string.IsNullOrEmpty(findBoxListProvider.DescriptionFromPrimaryKey(dummyTemplate.PK)));
			AssertEquals("Dummy TR00001000", findBoxListProvider.DescriptionFromPrimaryKey(templateRecord.PK));
			AssertEquals("Dummy TR00001001", findBoxListProvider.DescriptionFromPrimaryKey(otherTemplateRecord1.PK));
			Assert("This template record should not be found because of different module id", string.IsNullOrEmpty(findBoxListProvider.DescriptionFromPrimaryKey(otherTemplateRecord2.PK)));
		}

		#region Test classes

		[ModuleID(ModuleId.Dummy)]
		class DummyBizoModuleCollection : DummyBusinessObjectCollection
		{
			public DummyBizoModuleCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion
	}
}
