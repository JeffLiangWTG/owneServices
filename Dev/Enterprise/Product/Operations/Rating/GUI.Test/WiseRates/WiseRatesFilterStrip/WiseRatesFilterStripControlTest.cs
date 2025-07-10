using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;

namespace Enterprise.Rating.GUI.Testing
{
	public class WiseRatesFilterStripControlTest : ZFilterStripControlTest
	{
		public void TestValidation()
		{
			var clientFactory = new Mock<IWiseRatesClientFactory>();
			var logger = new ElementaryLogger();
			var provider = new WiseRatesProvider(Factory, clientFactory.Object, logger);
			var headerView = new WiseRatingHeaderView(provider, Factory, logger);

			using (var form = new WiseRatesForm(headerView))
			{
				form.Show();
				var filterControl = form.FilterControl;

				var dateFilter = filterControl.FilterBusinessObject.FilterStrips.Cast<FilterStrip>().SingleOrDefault(x => x.IsSingleDateFilter);
				filterControl.AddFilterStrip(dateFilter);
				filterControl.AddFilterStrip(dateFilter);

				form.FilterControl.Find();

				var errorMessage = "An effective date filter can be only selected once.";

				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestValidation_SeaLSE_ShowPopUpError() => AssertTransportAndContainerModeValidation("SEA", "LSE");

		public void TestValidation_AirLCL() => AssertTransportAndContainerModeValidation("AIR", "LCL");

		public void TestValidation_AirFCL() => AssertTransportAndContainerModeValidation("AIR", "FCL");

		void AssertTransportAndContainerModeValidation(string transportMode, string containerMode)
		{
			var clientFactory = new Mock<IWiseRatesClientFactory>();
			var logger = new ElementaryLogger();
			var provider = new WiseRatesProvider(Factory, clientFactory.Object, logger);
			var headerView = new WiseRatingHeaderView(provider, Factory, logger);

			using (var form = new WiseRatesForm(headerView))
			{
				form.Show();
				var filterControl = form.FilterControl.FilterBusinessObject;

				var effectiveDateStrip = (ModuleSingleDateFilter)filterControl[RateEntryFilterUtility.Constants.Codes.EffectiveOn];
				effectiveDateStrip.IsActive = true;
				effectiveDateStrip.Property1 = new ZDateTime(2021, 8, 20);

				var originDestStrip1 = (ModuleLocationFilter)filterControl[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				originDestStrip1.IsActive = true;
				originDestStrip1.Property1 = "AUSYD";
				originDestStrip1.Property2 = "USLAX";

				var containerModeStrip1 = (ModuleTextFilter)filterControl[RateEntryFilterUtility.Constants.Codes.ContainerMode];
				containerModeStrip1.IsActive = true;
				containerModeStrip1.Property = containerMode;

				var transportModeStrip1 = (ModuleTextFilter)filterControl[RateEntryFilterUtility.Constants.Codes.TransportMode];
				transportModeStrip1.IsActive = true;
				transportModeStrip1.Property = transportMode;

				form.FilterControl.Find();

				string expectedMessage = string.Format("Container mode of {0} can not be selected with {1} transport mode.", containerMode, transportMode);
				var notifiedMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, notifiedMessage);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}
	}
}