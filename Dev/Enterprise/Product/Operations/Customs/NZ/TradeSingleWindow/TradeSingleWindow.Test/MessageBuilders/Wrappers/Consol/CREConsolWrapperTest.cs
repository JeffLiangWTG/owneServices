using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	public class CREConsolWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new CREConsolWrapper(null, null);
		}

		public void TestCREConsolWrapper()
		{
			var testConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrappedConsol = new CREConsolWrapper(testConsol, null);
			AssertNotNull("CREConsolWrapper", wrappedConsol);
		}

		#region Implementation
		protected void CreateExportSeaConsol()
		{
			SetUpShipmentsAndCommonDataForExport();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_UniqueConsignRef = "SIS00039215";
			Consol.JK_MasterBillNum = "OB528742";
			transport.JW_VoyageFlight = "175E";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_Code = "TESTVESL";
			vessel.RV_LloydsNumber = "1234567";
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_ATD = new ZDateTime(2013, 07, 18);
			transport.JW_ATA = new ZDateTime(2013, 07, 25);
			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "FLMU4928475";
		}

		protected void SetUpShipmentsAndCommonDataForExport()
		{
			shipment = Consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Ship1212";
			shipment.JS_UniqueConsignRef = "SI0004927";
			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(shippingLine);
			Consol.JK_RL_NKLoadPort = "NZAKL";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			transport = Consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2013, 07, 18);
		}

		protected ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
				}

				return consol;
			}
		}

		ForwardingConsol consol;
		CommonShipment shipment;
		Transport transport;
		CommonContainer container;
		#endregion
	}
}
