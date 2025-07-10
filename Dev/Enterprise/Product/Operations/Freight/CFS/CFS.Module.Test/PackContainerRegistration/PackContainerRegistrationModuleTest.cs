using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(PackContainerRegistrationModule))]
	sealed class PackContainerRegistrationModuleTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (PackContainerRegistrationModule module = new PackContainerRegistrationModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("CFSContainerRego business context should be returned", BusinessContext.CFSContainerRego, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestSetGuiProviders()
		{
			using (PackContainerRegistrationModuleForTest module = new PackContainerRegistrationModuleForTest())
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
			return ModuleIDs.PackContainerRegistration;
		}

		protected override void SetUp()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.JC_OH_CFSClient = (Factory.LoadTop1<OrgHeader>(new ZQuery())).PK;
			Factory.Save();
			base.SetUp();
		}

		#region Implementation

		class PackContainerRegistrationModuleForTest : PackContainerRegistrationModule
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

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = (CFSContainer)factory.NewWithValidTestData(businessObjectType);
			bizo.JC_ContainerNum = "MODULE BASHER";

			return bizo;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);

			var filter = (ModuleTextFilter)filterBusinessObject["Container #"];
			filter.IsActive = true;
			filter.Property = "MODULE BASHER";
		}

		#endregion
	}
}
