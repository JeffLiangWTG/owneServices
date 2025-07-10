using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(ExportClassificationController))]
	public abstract class ExportClassificationControllerTest : ZControllerBasherTest
	{
		public void TestCheckPointForView()
		{
			AssertEquals(Env.Security.ExportClassification, Controller.CheckPointForViewExposedForTest);
		}

		public void TestCheckPointForEdit()
		{
			AssertEquals(Env.Security.ExportClassificationModify, Controller.CheckPointForEditExposedForTest);
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.ExportClassificationModify, Controller.CheckPointForNewExposedForTest);
		}

		public void TestCheckPointForDelete()
		{
			AssertEquals(Env.Security.ExportClassificationDelete, Controller.CheckPointForDeleteExposedForTest);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.ExportClassification, Controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(BaseCusClassification);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.ExportClassification;
		}

		protected new ExportClassificationController Controller
		{
			get
			{
				return (ExportClassificationController)base.Controller;
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BaseCusClassification testClass = Factory.New<BaseCusClassification>();
			testClass.CC_Description = "TestDescription";
			testClass.CC_LookupCode = "TestTest";
			testClass.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			Factory.Save();
			return testClass;
		}
	}
}
