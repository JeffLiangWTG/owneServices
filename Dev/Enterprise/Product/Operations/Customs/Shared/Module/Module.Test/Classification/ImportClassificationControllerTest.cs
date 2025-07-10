using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class ImportClassificationControllerTest : ZControllerBasherTest
	{
		public void TestCheckPointForView()
		{
			AssertEquals(Env.Security.ImportClassification, Controller.CheckPointForViewExposedForTest);
		}

		public void TestCheckPointForEdit()
		{
			AssertEquals(Env.Security.ImportClassificationModify, Controller.CheckPointForEditExposedForTest);
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.ImportClassificationModify, Controller.CheckPointForNewExposedForTest);
		}

		public void TestCheckPointForDelete()
		{
			AssertEquals(Env.Security.ImportClassificationDelete, Controller.CheckPointForDeleteExposedForTest);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.ImportClassification, Controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(BaseCusClassification);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.ImportClassification;
		}

		protected new ImportClassificationController Controller
		{
			get
			{
				return (ImportClassificationController)base.Controller;
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BaseCusClassification testClass = Factory.New<BaseCusClassification>();
			testClass.CC_Description = "TestDescription";
			testClass.CC_LookupCode = "TestTest";
			testClass.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			Factory.Save();
			return testClass;
		}
	}
}
