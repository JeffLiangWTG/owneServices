using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class ConsolDataCalculatorTest : TestCaseWithFactory
	{
		public void TestFirstUSPortOfDischarge()
		{
			AssertNull(calculator.FirstCountryPortOfDischarge);
			consol.JK_RL_NKPortOfFirstArrival = PortInTheCountry1.RL_Code;
			AssertEquals(PortInTheCountry1, calculator.FirstCountryPortOfDischarge);
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2011, 4, 10);
			AssertEquals(PortInTheCountry1, calculator.FirstCountryPortOfDischarge);

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = PortNotInTheCountry2.RL_Code;
			transport1.JW_RL_NKDiscPort = PortInTheCountry2.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("Consol's PortOfFirstArrival is earlier", PortInTheCountry1, calculator.FirstCountryPortOfDischarge);

			transport1.JW_ATA = new ZDateTime(2011, 4, 8);
			AssertEquals(PortInTheCountry2, calculator.FirstCountryPortOfDischarge);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = PortNotInTheCountry3.RL_Code;
			transport2.JW_RL_NKDiscPort = PortInTheCountry3.RL_Code;
			transport2.JW_ATA = new ZDateTime(2011, 4, 7);

			AssertEquals("First Leg", PortInTheCountry2, calculator.FirstCountryPortOfDischarge);

			transport1.JW_RL_NKDiscPort = PortNotInTheCountry3.RL_Code;
			AssertEquals("First Leg is not US Bound", PortInTheCountry3, calculator.FirstCountryPortOfDischarge);
		}

		public void TestGetInfosAffectingFirstCountryPortOfDischarge()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingFirstCountryPortOfDischarge());
			AssertCollectionContains(consol.JK_DatePortOfFirstArrivalInfo, list);
			AssertCollectionContains(consol.JK_RL_NKPortOfFirstArrivalInfo, list);

			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport1.JW_ETAInfo, list);
			AssertCollectionContains(transport1.JW_ATAInfo, list);

			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport2.JW_ETAInfo, list);
			AssertCollectionContains(transport2.JW_ATAInfo, list);
		}

		public void TestFirstCountryDischargeDate()
		{
			AssertEquals(ZDateTime.Empty, calculator.FirstCountryDischargeDate);
			consol.JK_RL_NKPortOfFirstArrival = PortInTheCountry1.RL_Code;
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2011, 4, 10);
			AssertEquals(new ZDateTime(2011, 4, 10), calculator.FirstCountryDischargeDate);

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = PortNotInTheCountry2.RL_Code;
			transport1.JW_RL_NKDiscPort = PortInTheCountry2.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("Consol's PortOfFirstArrival is earlier", new ZDateTime(2011, 4, 10), calculator.FirstCountryDischargeDate);

			transport1.JW_ATA = new ZDateTime(2011, 4, 8);
			AssertEquals(new ZDateTime(2011, 4, 8), calculator.FirstCountryDischargeDate);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = PortNotInTheCountry3.RL_Code;
			transport2.JW_RL_NKDiscPort = PortInTheCountry3.RL_Code;
			transport2.JW_ATA = new ZDateTime(2011, 4, 7);

			AssertEquals("First Leg", new ZDateTime(2011, 4, 8), calculator.FirstCountryDischargeDate);

			transport1.JW_RL_NKDiscPort = PortNotInTheCountry3.RL_Code;
			AssertEquals("First Leg is not US Bound", new ZDateTime(2011, 4, 7), calculator.FirstCountryDischargeDate);
		}

		public void TestGetInfosAffectingFirstCountryDischargeDate()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingFirstCountryDischargeDate());
			AssertCollectionContains(consol.JK_DatePortOfFirstArrivalInfo, list);
			AssertCollectionContains(consol.JK_RL_NKPortOfFirstArrivalInfo, list);

			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport1.JW_ETAInfo, list);
			AssertCollectionContains(transport1.JW_ATAInfo, list);

			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport2.JW_ETAInfo, list);
			AssertCollectionContains(transport2.JW_ATAInfo, list);
		}

		public void TestFirstCountryBoundTransportOrFirstTransportWithTransportMode()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = PortNotInTheCountry1.RL_Code;
			transport1.JW_RL_NKDiscPort = PortNotInTheCountry3.RL_Code;
			AssertEquals(transport1, calculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode);
			AssertEquals(null, calculator.FirstCountryBoundTransport);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = PortNotInTheCountry3.RL_Code;
			transport2.JW_RL_NKDiscPort = PortInTheCountry1.RL_Code;
			AssertEquals(transport2, calculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode);
			AssertEquals(transport2, calculator.FirstCountryBoundTransport);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_RL_NKLoadPort = PortInTheCountry1.RL_Code;
			transport3.JW_RL_NKDiscPort = PortInTheCountry3.RL_Code;
			AssertEquals(transport2, calculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode);
			AssertEquals(transport2, calculator.FirstCountryBoundTransport);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = PortInTheCountry3.RL_Code;
			transport4.JW_RL_NKDiscPort = PortNotInTheCountry2.RL_Code;
			AssertEquals(transport2, calculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode);
			AssertEquals(transport2, calculator.FirstCountryBoundTransport);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(transport1, calculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode);
			AssertEquals(null, calculator.FirstCountryBoundTransport);

			transport3.JW_RL_NKLoadPort = PortNotInTheCountry3.RL_Code;
			AssertEquals(transport3, calculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode);
			AssertEquals(transport3, calculator.FirstCountryBoundTransport);

			transport1.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transport1.JW_RL_NKLoadPort = PortNotInTheCountry1.RL_Code;
			transport1.JW_RL_NKDiscPort = PortNotInTheCountry2.RL_Code;

			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = PortNotInTheCountry2.RL_Code;
			transport2.JW_RL_NKDiscPort = PortNotInTheCountry3.RL_Code;

			transport3.JW_RL_NKDiscPort = PortNotInTheCountry3.RL_Code;

			AssertEquals(transport2, calculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode);
			AssertEquals(null, calculator.FirstCountryBoundTransport);
		}

		public void TestGetInfosAffectingFirstCountryBoundTransportOrFirstTransportWithTransportMode()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingTransportsOrder());

			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);

			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
		}

		public void TestGetTransportsInfos()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			var list = new List<ZPropertyInfo>(calculator.GetTransportsInfos(Transport.Schema.JW_ETD));
			AssertEquals(2, list.Count);

			AssertCollectionContains(transport1.JW_ETDInfo, list);
			AssertCollectionContains(transport2.JW_ETDInfo, list);

			list = new List<ZPropertyInfo>(calculator.GetTransportsInfos(Transport.Schema.JW_ETD, Transport.Schema.JW_ETA, Transport.Schema.JW_ATD));
			AssertEquals(6, list.Count);

			AssertCollectionContains(transport1.JW_ETDInfo, list);
			AssertCollectionContains(transport2.JW_ETDInfo, list);
			AssertCollectionContains(transport1.JW_ETAInfo, list);
			AssertCollectionContains(transport2.JW_ETAInfo, list);
			AssertCollectionContains(transport1.JW_ATDInfo, list);
			AssertCollectionContains(transport2.JW_ATDInfo, list);
		}

		protected abstract RefUNLOCO CreatePortNotInTheCountry1();
		protected abstract RefUNLOCO CreatePortNotInTheCountry2();
		protected abstract RefUNLOCO CreatePortNotInTheCountry3();
		protected abstract RefUNLOCO CreatePortInTheCountry1();
		protected abstract RefUNLOCO CreatePortInTheCountry2();
		protected abstract RefUNLOCO CreatePortInTheCountry3();

		protected RefUNLOCO PortNotInTheCountry1
		{
			get { return fPortNotInTheCountry1 ?? (fPortNotInTheCountry1 = CreatePortNotInTheCountry1()); }
		}
		RefUNLOCO fPortNotInTheCountry1;

		protected RefUNLOCO PortNotInTheCountry2
		{
			get { return fPortNotInTheCountry2 ?? (fPortNotInTheCountry2 = CreatePortNotInTheCountry2()); }
		}
		RefUNLOCO fPortNotInTheCountry2;

		protected RefUNLOCO PortNotInTheCountry3
		{
			get { return fPortNotInTheCountry3 ?? (fPortNotInTheCountry3 = CreatePortNotInTheCountry3()); }
		}
		RefUNLOCO fPortNotInTheCountry3;

		protected RefUNLOCO PortInTheCountry1
		{
			get { return fPortInTheCountry1 ?? (fPortInTheCountry1 = CreatePortInTheCountry1()); }
		}
		RefUNLOCO fPortInTheCountry1;

		protected RefUNLOCO PortInTheCountry2
		{
			get { return fPortInTheCountry2 ?? (fPortInTheCountry2 = CreatePortInTheCountry2()); }
		}
		RefUNLOCO fPortInTheCountry2;

		protected RefUNLOCO PortInTheCountry3
		{
			get { return fPortInTheCountry3 ?? (fPortInTheCountry3 = CreatePortInTheCountry3()); }
		}
		RefUNLOCO fPortInTheCountry3;

		protected abstract ConsolDataCalculator CreateNewCalculator(ForwardingConsol consol);

		protected ConsolDataCalculator calculator;
		protected ForwardingConsol consol;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			calculator = CreateNewCalculator(consol);
		}

		protected override void TearDown()
		{
			calculator.Dispose();
			calculator = null;
			base.TearDown();
		}
	}
}
