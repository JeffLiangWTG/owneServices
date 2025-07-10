using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class SingleCommercialInvoiceWrapperTest : CommercialInvoiceWrapperTest
	{
		public override void TestTransportation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var transport1 = invoice1.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			transport1.JW_RL_NKLoadPort = "TWKEL";
			transport1.JW_RL_NKDiscPort = "USLAX";

			CombineAssertions(() =>
			{
				var wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be empty if transport type is Flight2", ZString.Empty, wrapper1.Transportation);

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be AIR FREIGHT if transport type is Flight1", "AIR FREIGHT", wrapper1.Transportation);

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight3;
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be empty if transport type is Flight3", ZString.Empty, wrapper1.Transportation);

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be empty if transport type is Other", ZString.Empty, wrapper1.Transportation);
			});

			var invoice2 = declaration.Invoices.AddNew();
			var transport2 = invoice2.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "TWKEL";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_VesselForBinding = "Trailer123";

			CombineAssertions(() =>
			{
				var wrapper2 = GetCommercialInvoiceWrapper(invoice2, Factory);
				AssertEquals("When transport mode is SEA, should be empty if transport type is MainVessel", "Trailer123", wrapper2.Transportation);

				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				wrapper2 = GetCommercialInvoiceWrapper(invoice2, Factory);
				AssertEquals("When transport mode is SEA, should be empty if transport type is Other", ZString.Empty, wrapper2.Transportation);
			});

			var invoice3 = declaration.Invoices.AddNew();
			var transport3 = invoice3.Transports.AddNew();
			transport3.JW_LegOrder = 1;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport3.JW_RL_NKLoadPort = "TWKEL";
			transport3.JW_RL_NKDiscPort = "USLAX";
			transport3.JW_VesselForBinding = "Trailer123";
			var transport4 = invoice3.Transports.AddNew();
			transport4.JW_LegOrder = 1;
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport4.JW_RL_NKLoadPort = "TWKEL";
			transport4.JW_RL_NKDiscPort = "USLAX";
			transport4.JW_VesselForBinding = "Trailer123";
			var wrapper3 = GetCommercialInvoiceWrapper(invoice3, Factory);
			AssertEquals("Air transport and Sea transport at the same time.", ZString.Empty, wrapper3.Transportation);
		}

		public override void TestPortOfOriginName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var transport1 = invoice1.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			transport1.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport1.JW_RL_NKDiscPortForBinding = "USLAX";

			CombineAssertions(() =>
			{
				var wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be empty if transport type is Flight2", ZString.Empty, wrapper1.PortOfOriginName);

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be Taiwan - Keelung (Chilung) if transport type is Flight1", "Taiwan - Keelung (Chilung)", wrapper1.PortOfOriginName);

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight3;
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be empty if transport type is Flight3", ZString.Empty, wrapper1.PortOfOriginName);

				transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be empty if transport type is Other", ZString.Empty, wrapper1.PortOfOriginName);
			});

			var invoice2 = declaration.Invoices.AddNew();
			var transport2 = invoice2.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport2.JW_RL_NKDiscPortForBinding = "USLAX";

			CombineAssertions(() =>
			{
				var wrapper2 = GetCommercialInvoiceWrapper(invoice2, Factory);
				AssertEquals("When transport mode is SEA, should be Taiwan - Keelung(Chilung) if transport type is MainVessel", "Taiwan - Keelung (Chilung)", wrapper2.PortOfOriginName);

				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				wrapper2 = GetCommercialInvoiceWrapper(invoice2, Factory);
				AssertEquals("When transport mode is SEA, should be empty if transport type is Other", ZString.Empty, wrapper2.PortOfOriginName);
			});
		}

		public override void TestFinalDestinationName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var transport1 = invoice1.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport1.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport1.JW_RL_NKDiscPortForBinding = "USLAX";

			var transport2 = invoice1.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			transport2.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport2.JW_RL_NKDiscPortForBinding = "USABB";

			var transport3 = invoice1.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.Flight3;
			transport3.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport3.JW_RL_NKDiscPortForBinding = "USUAB";

			var transport4 = invoice1.Transports.AddNew();
			transport4.JW_LegOrder = 4;
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport4.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport4.JW_RL_NKDiscPortForBinding = "USABI";

			CombineAssertions("When transport mode is Air", () =>
			{
				var wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should get data from the transport which type is Other", "United States - Abilene", wrapper1.FinalDestinationName);

				invoice1.Transports.RemoveAndDelete(transport4);
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should get data from the transport which type is Flight3", "United States - Abbeville", wrapper1.FinalDestinationName);

				invoice1.Transports.RemoveAndDelete(transport3);
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should get data from the transport which type is Flight2", "United States - Abbeville", wrapper1.FinalDestinationName);

				invoice1.Transports.RemoveAndDelete(transport2);
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should get data from the transport which type is Flight1", "United States - Los Angeles", wrapper1.FinalDestinationName);

				invoice1.Transports.RemoveAndDelete(transport1);
				wrapper1 = GetCommercialInvoiceWrapper(invoice1, Factory);
				AssertEquals("When transport mode is AIR, should be empty is no transport", ZString.Empty, wrapper1.FinalDestinationName);
			});

			var invoice2 = declaration.Invoices.AddNew();
			var transport5 = invoice2.Transports.AddNew();
			transport5.JW_LegOrder = 1;
			transport5.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport5.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport5.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport5.JW_RL_NKDiscPortForBinding = "CNSHA";

			var transport6 = invoice2.Transports.AddNew();
			transport6.JW_LegOrder = 2;
			transport6.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport6.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport6.JW_RL_NKLoadPortForBinding = "TWKEL";
			transport6.JW_RL_NKDiscPortForBinding = "CNSZX";

			CombineAssertions("When transport mode is Sea", () =>
			{
				var wrapper2 = GetCommercialInvoiceWrapper(invoice2, Factory);
				AssertEquals("When transport mode is SEA, should get data from the transport which type is Other", "China - Shenzhen Baoan International Apt", wrapper2.FinalDestinationName);

				invoice2.Transports.RemoveAndDelete(transport6);
				wrapper2 = GetCommercialInvoiceWrapper(invoice2, Factory);
				AssertEquals("When transport mode is SEA, should get data from the transport which type is MainVessel", "China - Shanghai Hongqiao International Apt", wrapper2.FinalDestinationName);

				invoice2.Transports.RemoveAndDelete(transport5);
				wrapper2 = GetCommercialInvoiceWrapper(invoice2, Factory);
				AssertEquals("When transport mode is SEA, should be empty is no transport", ZString.Empty, wrapper2.FinalDestinationName);
			});

			var invoice3 = declaration.Invoices.AddNew();
			var transport7 = invoice3.Transports.AddNew();
			transport7.JW_LegOrder = 1;
			transport7.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport7.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport7.JW_RL_NKLoadPort = "TWKEL";
			transport7.JW_RL_NKDiscPort = "USLAX";
			transport7.JW_VesselForBinding = "Trailer123";
			var transport8 = invoice3.Transports.AddNew();
			transport8.JW_LegOrder = 1;
			transport8.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport8.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport8.JW_RL_NKLoadPort = "TWKEL";
			transport8.JW_RL_NKDiscPort = "USLAX";
			transport8.JW_VesselForBinding = "Trailer123";
			var wrapper3 = GetCommercialInvoiceWrapper(invoice3, Factory);
			AssertEquals("Air transport and Sea transport at the same time.", ZString.Empty, wrapper3.FinalDestinationName);
		}

		protected override CommercialInvoiceWrapper GetCommercialInvoiceWrapper(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factory)
		{
			return SingleCommercialInvoiceWrapper.New(jobComInvoiceHeader, factory);
		}
	}
}
