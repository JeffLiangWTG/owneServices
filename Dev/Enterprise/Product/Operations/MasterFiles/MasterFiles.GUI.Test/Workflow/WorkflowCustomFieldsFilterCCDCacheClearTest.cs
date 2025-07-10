using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class WorkflowCustomFieldsFilterCCDCacheClearTest : TestCaseWithFactory
	{
		#region TestCCDCacheClearedByOverridableTestListener

		public void TestCCDCacheClearedByOverridableTestListener()
		{
			var filterCollection = new ModuleFilterCollection();
			AssertNull(filterCollection["C1"]);
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "XXX";
			var def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "C1";
			def1.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C1"].GetType());
			template1.Delete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var filterCollection2 = new ModuleFilterCollection();
			AssertNull(filterCollection2["C1"]);
			var template2 = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "XXX";
			var def2 = template2.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "C1";
			def2.XC_Type = AddOnColumnDataType.Codes.Integer;
			newFactory.Save();

			var listener = new OverridableTestListener();
			listener.AfterEachTest(DateTime.Now);

			filterCollection2.AddWorkflowCustomFieldsFilters(newFactory, "XXX", typeof(DummyBusinessObject));
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection2["C1"].GetType());
		}

		#endregion

	}
}
