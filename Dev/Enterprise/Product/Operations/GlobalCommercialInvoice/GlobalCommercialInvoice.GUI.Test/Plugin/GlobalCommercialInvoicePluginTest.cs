using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.GlobalCommercialInvoice.Business.Test;
using Enterprise.Integration.Freight;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.GlobalCommercialInvoice.GUI.Test
{
	public class GlobalCommercialInvoicePluginTest : TestCaseWithFactory
	{
		public void TestGlobalCommercialInvoicePluginType_ZPlugIn()
		{
			using var plugin = new GlobalCommercialInvoicePlugin((IBusiness)Factory.CreateNewShipment());
			AssertEquals(typeof(ZPlugIn), plugin.GetType().BaseType);
		}

		public void TestGlobalCommercialInvoicePluginType_ZUserControl()
		{
			using var plugin = new GlobalCommercialInvoicePluginForTest((IBusiness)Factory.CreateNewShipment());
			AssertEquals(typeof(ZUserControl), plugin.UserControl.GetType().BaseType);
		}

		public void TestGlobalCommercialInvoicePluginType_DockStyleFill()
		{
			using var plugin = new GlobalCommercialInvoicePluginForTest((IBusiness)Factory.CreateNewShipment());
			AssertEquals(DockStyle.Fill, plugin.UserControl.Dock);
		}

		public void TestGlobalCommercialInvoicePlugin_Name()
		{
			using var plugin = new GlobalCommercialInvoicePluginForTest((IBusiness)Factory.CreateNewShipment());
			AssertEquals(Integration.Constants.PluginName, plugin.Name);
		}

		public void TestGlobalCommercialInvoicePlugin_LicenceCheckPoint()
		{
			using var plugin = new GlobalCommercialInvoicePluginForTest((IBusiness)Factory.CreateNewShipment());
			AssertEquals(Env.Licence.Core, plugin.LicenceCheckPoint);
		}

		public void TestBookingSpotQuote_WhenPassedIntoPlugin_NoExceptionThrown()
		{
			var bookingSpotQuote = GlobalCommercialInvoiceHelperTest.CreateNewBookingSpotQuote(Factory);
			var booking = (BusinessObject)Factory.New<ICommonShipment>();
			booking[JobShipmentSchema.JS_IsForwardRegistered] = false;
			booking[JobShipmentSchema.JS_IsCFSRegistered] = false;
			booking[JobShipmentSchema.JS_IsShipping] = false;
			booking[JobShipmentSchema.JS_IsBooking] = true;

			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				using var form = new ZForm(bookingSpotQuote);
				using var plugin = new GlobalCommercialInvoicePluginForTest((BusinessObject)(bookingSpotQuote));
				var userControl = plugin.GetNewUserControl();
				form.Controls.Add(userControl);
				form.Show();
				form.Close();
			});
		}

		class GlobalCommercialInvoicePluginForTest(IBusiness hostBusinessEntity) : GlobalCommercialInvoicePlugin(hostBusinessEntity)
		{
			public new LicenceCheckpoint LicenceCheckPoint => base.LicenceCheckPoint;
			public new Control GetNewUserControl() => base.GetNewUserControl();
		}
	}
}
