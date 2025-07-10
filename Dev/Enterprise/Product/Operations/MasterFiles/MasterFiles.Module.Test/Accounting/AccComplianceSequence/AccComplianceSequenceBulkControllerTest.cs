using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccComplianceSequnceBulkController))]
	sealed class AccComplianceSequenceBulkControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccComplianceSequenceBulk;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new AccComplianceSequenceBulkCreator(Factory);
		}

		protected override Type GetBusinessObjectType() => typeof(AccComplianceSequenceBulkCreator);

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AccComplianceSequenceBulkCreator), TestController.TypeOfTopLevelBusinessObject);
		}

		public void TestGetForm()
		{
			using (var testForm = TestController.ShowNewForm())
			{
				AssertEquals(typeof(AccComplianceSequenceBulkForm), testForm.GetType());
			}
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.ComplianceSequencesActionsBulkCreate, TestController.CheckPointForNewExposedForTest);
		}

		[RequiresSTA]
		public void TestDisplayModeForNew()
		{
			using (IZForm testForm = TestController.ShowNewForm())
			{
				AssertEquals(ODisplayMode.NewSaved, testForm.DisplayMode);
			}
		}

		public void TestModuleID()
		{
			AssertNull(TestController.ModuleID);
		}

		AccComplianceSequnceBulkController TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new AccComplianceSequnceBulkController();
				}
				return fTestController;
			}
		}

		AccComplianceSequnceBulkController fTestController;
	}
}
