using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(ContainersModule))]
	sealed class ContainersModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (ContainersModule module = new ContainersModule())
			{
				AssertEquals(ModuleIDs.Containers, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (ContainersModule module = new ContainersModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Consol business context should be returned", BusinessContext.CusContainer, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestSetGuiProviders()
		{
			using (ContainersModuleForTest module = new ContainersModuleForTest())
			{
				module.GetNewGridCollection();

				var servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);

				module.PerformSearch();

				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Containers;
		}

		#region Implementation

		class ContainersModuleForTest : ContainersModule
		{
			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

			public void PerformSearch()
			{
				base.PerformSearch();
			}
		}

		#endregion
	}
}
