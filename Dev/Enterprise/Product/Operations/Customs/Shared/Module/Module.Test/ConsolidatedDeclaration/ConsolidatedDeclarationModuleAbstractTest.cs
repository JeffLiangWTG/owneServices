using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class ConsolidatedDeclarationModuleAbstractTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var declaration = factory.NewWithValidTestData<BaseJobDeclaration>();
			var consolidatedDeclaration = (ConsolidatedDeclaration)factory.NewWithValidTestData(ConsolidatedDeclarationType);
			consolidatedDeclaration.CRD_JE_LeadDeclaration = declaration.PK;
			return consolidatedDeclaration;
		}

		public void TestElementType()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				var elementType = module.GetElementType();
				CombineAssertions(() =>
				{
					AssertEquals("Job must be of type ConsolidateDeclaration.", true, typeof(ConsolidatedDeclaration).IsAssignableFrom(elementType));
					AssertEquals("BizO Application Code", GetExpectedApplicationCode(), (Factory.New(elementType) as ConsolidatedDeclaration).CRD_ApplicationCode);
				});
			}
		}

		public void TestBusinessContexts()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("The 'ConsolidatedEntry' business context should be returned", BusinessContext.ConsolidatedEntry, module.BusinessContexts[0]);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ConsolidatedDeclaration;

		protected abstract string GetExpectedApplicationCode();

		protected abstract Type ConsolidatedDeclarationType { get; }
	}
}
