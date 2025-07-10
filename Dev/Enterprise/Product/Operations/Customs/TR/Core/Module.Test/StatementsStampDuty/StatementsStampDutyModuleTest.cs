using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(StatementsStampDutyModule))]
	public class StatementsStampDutyModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.TR.StatementsStampDuty;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Turkey; }
		}

		public void TestAllowNewDeleteEditAndWorkflowTypeAndSupportsWorkflow()
		{
			using (var module = new StatementsStampDutyModule())
			{
				AssertEquals(false, module.SupportsWorkflow);
				AssertEquals(true, module.AllowDelete);
				AssertEquals(true, module.AllowEdit);
				AssertEquals(true, module.AllowNew);
				AssertEquals(false, module.AllowCopyFilterGridHyperlinkToClipboard);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (CusStatementHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			return result;
		}

		public void TestActionMenu()
		{
			using (StatementsStampDutyModule module = new StatementsStampDutyModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("&Actions"));
			}
		}
	}
}
