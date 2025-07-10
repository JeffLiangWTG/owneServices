using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolWithPrintedMAWBAttachDetachCheckTest : TestCaseWithFactory
	{
		public void TestIsAllowedToAddNewShipment_FinalMasterIsPreinted_ReturnFalse()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.UpdateAWBPrinted();

			var check = new ConsolWithPrintedMAWBAttachDetachCheck();
			var consols = new List<CommonConsol> { consol };
			var shipments = new List<CommonShipment>();
			string message;

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
			message = check.Check(AttachDetachAction.New, shipments, consols, consol, DefaultListFormatter);
			AssertEquals("Error message", string.Empty, message);

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			message = check.Check(AttachDetachAction.New, shipments, consols, consol, DefaultListFormatter);
			AssertNotEquals("Error message", string.Empty, message);
		}

		public void TestIsAllowedToAttachShipment_FinalMasterIsPreinted_ReturnFalse()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.UpdateAWBPrinted();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "MCLAREN";

			var check = new ConsolWithPrintedMAWBAttachDetachCheck();
			var consols = new List<CommonConsol> { consol };
			var shipments = new List<CommonShipment>() { shipment };
			string message;

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
			message = check.Check(AttachDetachAction.New, shipments, consols, consol, DefaultListFormatter);
			AssertEquals("Error message", string.Empty, message);

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			message = check.Check(AttachDetachAction.New, shipments, consols, consol, DefaultListFormatter);
			AssertNotEquals("Error message", string.Empty, message);
		}

		public void TestIsAllowedToAttachConsol_FinalMasterIsPreinted_ReturnFalse()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.UpdateAWBPrinted();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "MCLAREN";

			var check = new ConsolWithPrintedMAWBAttachDetachCheck();
			var consols = new List<CommonConsol> { consol };
			var shipments = new List<CommonShipment>() { shipment };
			string message;

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
			message = check.Check(AttachDetachAction.New, shipments, consols, shipment, DefaultListFormatter);
			AssertEquals("Error message", string.Empty, message);

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			message = check.Check(AttachDetachAction.New, shipments, consols, shipment, DefaultListFormatter);
			AssertNotEquals("Error message", string.Empty, message);
		}

		public void TestIsAllowedToDetachShipments_FinalMasterIsPreinted_ReturnFalse()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.UpdateAWBPrinted();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "MCLAREN";

			var check = new ConsolWithPrintedMAWBAttachDetachCheck();
			var consols = new List<CommonConsol> { consol };
			var shipments = new List<CommonShipment>() { shipment };
			string message;

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
			message = check.Check(AttachDetachAction.New, shipments, consols, consol, DefaultListFormatter);
			AssertEquals("Restricted message", string.Empty, message);

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			message = check.Check(AttachDetachAction.New, shipments, consols, consol, DefaultListFormatter);
			AssertNotEquals("Restricted message", string.Empty, message);
		}

		public void TestIsAllowedToDetachConsols_FinalMasterIsPreinted_ReturnFalse()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.UpdateAWBPrinted();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "MCLAREN";

			var check = new ConsolWithPrintedMAWBAttachDetachCheck();
			var consols = new List<CommonConsol> { consol };
			var shipments = new List<CommonShipment>() { shipment };
			string message;

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
			message = check.Check(AttachDetachAction.New, shipments, consols, shipment, DefaultListFormatter);
			AssertEquals("Restricted message", string.Empty, message);

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			message = check.Check(AttachDetachAction.New, shipments, consols, shipment, DefaultListFormatter);
			AssertNotEquals("Restricted message", string.Empty, message);
		}

		string DefaultListFormatter(IEnumerable<BusinessObject> list, int itemsToList = 1)
		{
			return "McLaren";
		}
	}
}
