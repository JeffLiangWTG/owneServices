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
	[TestedType(typeof(ContainerLoadListModule))]
	public class ContainerLoadListModuleTest : GlowOnlyModuleTest<ContainerLoadListModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(ContainerLoadListFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(ContainerLoadListFilterControl);

		protected override Type ExpectedCollectionType => typeof(ContainerLoadListCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.OrderManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.ContainerLoadList;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ContainerLoadList;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var containerLoadListHeader = factory.NewWithValidTestData<CYContainerLoadList>();
			return containerLoadListHeader;
		}

		#endregion

		[RequiresSTA]
		public void TestHandleOpenInWebPortal()
		{
			var header = Factory.NewWithValidTestData<CYContainerLoadList>();
			header.CLH_LoadListId = "CLH01";

			Factory.Save();

			var glowSingleSignOnTokenProvider = BuildMockForGlowSingleSignOnTokenProvider();
			var baseURL = "https://www.xxx.com";

			using (var module = new ContainerLoadListModule())
			using (var form = new ZForm())
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, baseURL))
			using (ObjectFactory.Substitute(glowSingleSignOnTokenProvider.Object))
			{
				var filterControl = (ContainerLoadListFilterControl)module.EmbeddedControl;
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
				AssertEquals($"{baseURL}/goto/ContainerLoadList?sso_otp=SSOOTPTOKEN&entityPK={header.PK}", WebUrlLauncher.LastUrlLaunched);
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
