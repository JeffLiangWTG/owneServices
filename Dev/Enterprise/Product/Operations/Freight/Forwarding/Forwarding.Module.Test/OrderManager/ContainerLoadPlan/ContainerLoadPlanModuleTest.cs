using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ContainerLoadPlanModule))]
	public class ContainerLoadPlanModuleTest : GlowOnlyModuleTest<ContainerLoadPlanModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(ContainerLoadPlanFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(ContainerLoadPlanFilterControl);

		protected override Type ExpectedCollectionType => typeof(ContainerLoadPlanCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.OrderManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.ContainerLoadPlan;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ContainerLoadPlan;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var containerLoadPlanHeader = factory.NewWithValidTestData<CFSContainerLoadList>();
			return containerLoadPlanHeader;
		}

		#endregion

		[RequiresSTA]
		public void TestHandleOpenInWebPortal()
		{
			var header = Factory.NewWithValidTestData<CFSContainerLoadList>();
			header.CLH_LoadListId = "CLP01";

			Factory.Save();

			var glowSingleSignOnTokenProvider = BuildMockForGlowSingleSignOnTokenProvider();
			var baseURL = "https://www.aaa.com";

			using (var module = new ContainerLoadPlanModule())
			using (var form = new ZForm())
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, baseURL))
			using (ObjectFactory.Substitute(glowSingleSignOnTokenProvider.Object))
			{
				var filterControl = (ContainerLoadPlanFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();

				var moduleForTesting = (IFilterModuleInternalsForTesting)module;
				moduleForTesting.FilterBusinessObject.ResetToDefaultValues();
				moduleForTesting.PerformSearch();
				Application.DoEvents();

				var filteredGrid = module.DisplayGrid;
				AssertEquals(1, filteredGrid.VisibleRowCount);

				filteredGrid.SelectSingleElementByPK(header.PK);
				module.HandleOpenInWebPortal(null, EventArgs.Empty);
				AssertEquals($"{baseURL}/goto/ContainerLoadPlan?sso_otp=SSOOTPTOKEN&entityPK={header.PK}", WebUrlLauncher.LastUrlLaunched);
				filterControl.Dispose();
			}
		}

		static Mock<IGlowSingleSignOnTokenProvider> BuildMockForGlowSingleSignOnTokenProvider()
		{
			var glowSingleSignOnTokenProvider = new Mock<IGlowSingleSignOnTokenProvider>();
			glowSingleSignOnTokenProvider.Setup(h => h.CreateLimitedToken(Env.CurrentUserPK, "GS", It.IsAny<GlowSingleSignOnTokenOptions>())).Returns("SSOOTPTOKEN");
			return glowSingleSignOnTokenProvider;
		}
	}
}
