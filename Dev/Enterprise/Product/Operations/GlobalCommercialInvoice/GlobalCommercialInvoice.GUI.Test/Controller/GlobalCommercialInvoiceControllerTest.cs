using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.GlobalCommercialInvoice.Business.Test;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.GUI.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceController))]
	public class GlobalCommercialInvoiceControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlobalCommercialInvoicePlugin;
		}

		public void TestPlugin()
		{
			using var plugIn = new GlobalCommercialInvoiceControllerForTest().GetPlugin((IBusiness)Factory.CreateNewShipment());
			AssertEquals(typeof(GlobalCommercialInvoicePlugin), plugIn.GetType());
		}

		public void TestPluginTabPageCaption()
		{
			var controller = new GlobalCommercialInvoiceControllerForTest();
			AssertEquals("Commercial Invoice", controller.PluginTabPageCaption.Caption);
		}

		public void TestInitialSecurityCheckPoints()
		{
			var controller = new GlobalCommercialInvoiceControllerForTest();
			AssertEquals(Env.Security.None, controller.CheckPointForViewForTest);
			AssertEquals(Env.Security.None, controller.CheckPointForNewForTest);
			AssertEquals(Env.Security.None, controller.CheckPointForEditForTest);
			AssertEquals(Env.Security.None, controller.CheckPointForDeleteForTest);
		}

		public void TestGetCheckPointForEdit()
		{
			var controller = new GlobalCommercialInvoiceControllerForTest();

			AssertCheckPoint((BusinessObject)Factory.CreateNewShipment(), Env.Security.ShipmentsCommercialInvoiceEdit);
			AssertCheckPoint((BusinessObject)Factory.CreateNewBookingSpotQuote(), Env.Security.BookingsCommercialInvoiceEdit);
			AssertCheckPoint((BusinessObject)Factory.CreateNewBookingWithQuote(), Env.Security.BookingsCommercialInvoiceEdit);
			AssertCheckPoint(Factory.New<DummyBusinessObject>(), Env.Security.None);

			void AssertCheckPoint(BusinessObject hostBizObject, SecurityCheckpoint expectedCheckpoint)
			{
				AssertEquals($"{hostBizObject.HumanReadableName}: Should return checkpoint: {expectedCheckpoint}", expectedCheckpoint, controller.GetCheckPointForEdit(hostBizObject));
			}
		}

		public void TestGetCheckPointForEdit_WhenSecurityCommercialInvoiceIsFalse()
		{
			var controller = new GlobalCommercialInvoiceControllerForTest();
			var securityCore = Factory.CreateSecurityCore();
			securityCore.ShipmentsCommercialInvoice.IsAllowed = false;
			securityCore.BookingsCommercialInvoice.IsAllowed = false;

			AssertCheckPoint((BusinessObject)Factory.CreateNewShipment(), securityCore.ShipmentsCommercialInvoice);
			AssertCheckPoint((BusinessObject)Factory.CreateNewBookingSpotQuote(), securityCore.BookingsCommercialInvoice);

			void AssertCheckPoint(BusinessObject hostBizObject, SecurityCheckpoint expectedCheckpoint)
			{
				using (Env.SetTemporarySecurityInstanceForTest(securityCore))
				{
					AssertEquals($"{hostBizObject.HumanReadableName}: Should return checkpoint: {expectedCheckpoint}", expectedCheckpoint, controller.GetCheckPointForEdit(hostBizObject));
				}
			}
		}

		public void TestGetCheckPointForView()
		{
			var controller = new GlobalCommercialInvoiceControllerForTest();

			AssertCheckPoint((BusinessObject)Factory.CreateNewShipment(), Env.Security.ShipmentsCommercialInvoice);
			AssertCheckPoint((BusinessObject)Factory.CreateNewBookingSpotQuote(), Env.Security.BookingsCommercialInvoice);
			AssertCheckPoint((BusinessObject)Factory.CreateNewBookingWithQuote(), Env.Security.BookingsCommercialInvoice);
			AssertCheckPoint(Factory.New<DummyBusinessObject>(), Env.Security.None);

			void AssertCheckPoint(BusinessObject hostBizObject, SecurityCheckpoint expectedCheckpoint)
			{
				AssertEquals($"{hostBizObject.HumanReadableName}: Should return checkpoint: {expectedCheckpoint}", expectedCheckpoint, controller.GetCheckPointForView(hostBizObject));
			}
		}

		class GlobalCommercialInvoiceControllerForTest : GlobalCommercialInvoiceController
		{
			public ZPlugIn GetPlugin(IBusiness businessEntity) => GetPlugIn(businessEntity);

			public SecurityCheckpoint CheckPointForViewForTest => CheckPointForView;

			public SecurityCheckpoint CheckPointForNewForTest => CheckPointForNew;

			public SecurityCheckpoint CheckPointForEditForTest => CheckPointForEdit;

			public SecurityCheckpoint CheckPointForDeleteForTest => CheckPointForDelete;
		}
	}
}
