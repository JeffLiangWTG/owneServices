using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(DrawbackModule))]
	sealed class DrawbackModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckpoint()
		{
			using (var module = new DrawbackModule())
			{
				AssertEquals(Env.Security.USDrawback, module.SecurityCheckpoint);
			}
		}

		public void TestDrawbackModuleAllows()
		{
			using (var module = new DrawbackModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
			}
		}

		public void TestToolBarButtons()
		{
			using (var module = new DrawbackModule())
			{
				AssertEquals("Module should have 7 standard buttons", 7, module.ToolBarButtons.Length);
				AssertEquals("View", "View", module.ToolBarButtons[0].Text);
				AssertEquals("New", "New", module.ToolBarButtons[1].Text);
				AssertEquals("Edit", "Edit", module.ToolBarButtons[2].Text);
				AssertEquals("Copy", "Copy", module.ToolBarButtons[3].Text);
				AssertEquals("Delete", "Delete", module.ToolBarButtons[4].Text);
				AssertEquals("Actions", "Actions", module.ToolBarButtons[5].Text);
				AssertEquals("Hide/Show Filters", "Hide/Show Filters", module.ToolBarButtons[6].Text);
			}
		}

		public void TestCorrectFormIsOpened()
		{
			JobDeclarationModuleTest.AssertCorrectFormIsOpened<DrawbackModule>();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Drawback;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = factory.NewWithValidTestData<JobDeclaration>();
			result.SetDefaultValuesForDrawback();
			return result;
		}
	}
}
