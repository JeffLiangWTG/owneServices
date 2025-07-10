using System;
using System.Windows.Forms;
using CargoWise.Application;
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
	[TestedType(typeof(JobSupplierBookingModule))]
	public class JobSupplierBookingModuleTest : GlowOnlyModuleTest<JobSupplierBookingModule>
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.SupplierBooking;

		protected override Type ExpectedFilterBusinessObjectType => typeof(JobSupplierBookingFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(JobSupplierBookingFilterControl);

		protected override Type ExpectedCollectionType => typeof(JobSupplierBookingCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.OrderManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.SupplierBooking;

		protected override bool ExpectedSupportsWorkflow => true;

		[RequiresSTA]
		public void TestHandleOpenInWebPortal()
		{
			var booking1 = Factory.NewWithValidTestData<JobSupplierBooking>();
			Factory.Save();
			var booking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			Factory.Save();

			var glowSingleSignOnTokenProvider = BuildMockForGlowSingleSignOnTokenProvider();
			var baseURL = "https://www.xxx.com";

			using (var module = new JobSupplierBookingModule())
			using (var form = new ZForm())
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, baseURL))
			using (ObjectFactory.Substitute(glowSingleSignOnTokenProvider.Object))
			{
				var filterControl = (JobSupplierBookingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();

				var moduleForTesting = (IFilterModuleInternalsForTesting)module;
				moduleForTesting.FilterBusinessObject.ResetToDefaultValues();
				moduleForTesting.PerformSearch();
				Application.DoEvents();

				var filteredGrid = module.DisplayGrid;
				AssertEquals(2, filteredGrid.VisibleRowCount);

				filteredGrid.SelectSingleElementByPK(booking1.PK);
				module.HandleOpenInWebPortal(null, EventArgs.Empty);
				AssertEquals($"{baseURL}/goto/JobSupplierBooking?sso_otp=SSOOTPTOKEN&entityPK={booking1.PK}", WebUrlLauncher.LastUrlLaunched);

				filteredGrid.SelectSingleElementByPK(booking2.PK);
				module.HandleOpenInWebPortal(null, EventArgs.Empty);
				AssertEquals($"{baseURL}/goto/JobSupplierBooking?sso_otp=SSOOTPTOKEN&entityPK={booking2.PK}", WebUrlLauncher.LastUrlLaunched);

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
