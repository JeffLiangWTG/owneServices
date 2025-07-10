using CargoWise.ComponentModel;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolTransportCollectionTest : BaseFreightTest
	{
		#region TestLastTransportWithTransportModeAndImportVessel

		public void TestFirstTransportWithTransportModeAndExportVessel()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;
			Transport transport1 = Consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort2;
			transport1.JW_Vessel = TestVessel1.RV_FK;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("No export transports", null, Consol.Transports.FirstTransportWithTransportModeAndExportVessel(Core.Constants.TransportModes.Sea));
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Only one transport", transport1, Consol.Transports.FirstTransportWithTransportModeAndExportVessel(Core.Constants.TransportModes.Sea));

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = OverseasPort2;
			transport2.JW_RL_NKDiscPort = AlternateHomePort;
			transport2.JW_Vessel = TestVessel1.RV_FK;
			AssertEquals("Transport1 should be returned", transport1, Consol.Transports.FirstTransportWithTransportModeAndExportVessel(Core.Constants.TransportModes.Sea));

			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport2 should be returned", transport2, Consol.Transports.FirstTransportWithTransportModeAndExportVessel(Core.Constants.TransportModes.Sea));
		}

		#endregion

		#region TestLastTransportWithTransportModeAndImportVessel

		public void TestLastTransportWithTransportModeAndImportVessel()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = OverseasPort;
			Consol.JK_RL_NKDischargePort = HomePort;
			Transport transport1 = Consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = OverseasPort;
			transport1.JW_RL_NKDiscPort = OverseasPort2;
			transport1.JW_Vessel = TestVessel1.RV_FK;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("No import transports", null, Consol.Transports.LastTransportWithTransportModeAndImportVessel(Core.Constants.TransportModes.Sea));
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Only one transport", transport1, Consol.Transports.LastTransportWithTransportModeAndImportVessel(Core.Constants.TransportModes.Sea));
			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = OverseasPort2;
			transport2.JW_RL_NKDiscPort = AlternateHomePort;
			transport2.JW_Vessel = TestVessel1.RV_FK;
			AssertEquals("There is only one import transport to return", transport2, Consol.Transports.LastTransportWithTransportModeAndImportVessel(Core.Constants.TransportModes.Sea));
			Transport transport3 = Consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_RL_NKLoadPort = AlternateHomePort;
			transport3.JW_RL_NKDiscPort = AlternateHomePort2;
			transport3.JW_Vessel = TestVessel1.RV_FK;
			AssertEquals("Transport3 should be returned", transport3, Consol.Transports.LastTransportWithTransportModeAndImportVessel(Core.Constants.TransportModes.Sea));
			Transport transport4 = Consol.Transports.AddNew();
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = AlternateHomePort2;
			transport4.JW_RL_NKDiscPort = HomePort;
			transport4.JW_Vessel = TestVessel2.RV_FK;
			AssertEquals("Different vessel so transport3 should still be returned", transport3, Consol.Transports.LastTransportWithTransportModeAndImportVessel(Core.Constants.TransportModes.Sea));
		}

		#endregion

		#region TestMostInterestingTransport

		public void TestMostInterestingTransport()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;
			Transport transport1 = Consol.Transports[0];
			AssertEquals("there is only one transport to return", transport1, Consol.Transports.MostInterestingTransport);
			AssertEquals("Expected Description", "Voyage", Consol.MostInterestingTransportDescription);

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_LegOrder = 5;
			AssertEquals("non-import consols should select the departure leg when no transports match the consol transport mode", transport2, Consol.Transports.MostInterestingTransport);
			AssertEquals("Expected Description", "Flight (Departure Leg)", Consol.MostInterestingTransportDescription);

			Consol.JK_RL_NKDischargePort = HomePort;
			Consol.JK_RL_NKLoadPort = OverseasPort;
			AssertEquals("import consols should select the arrival leg when no transports match the consol transport mode", transport1, Consol.Transports.MostInterestingTransport);
			AssertEquals("Expected Description", "Flight (Arrival Leg)", Consol.MostInterestingTransportDescription);

			Transport transport3 = Consol.Transports.AddNew();
			Transport transport4 = Consol.Transports.AddNew();
			AssertEquals("import consols should select the last leg with a matching transport mode", transport4, Consol.Transports.MostInterestingTransport);
			AssertEquals("Expected Description", "Voyage (Last 'SEA' Leg)", Consol.MostInterestingTransportDescription);

			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;
			AssertEquals("non-import consols should select the first leg with a matching transport mode", transport3, Consol.Transports.MostInterestingTransport);
			AssertEquals("Expected Description", "Voyage (First 'SEA' Leg)", Consol.MostInterestingTransportDescription);
		}

		#endregion

		#region TestArrivalDepartureTransport

		public void TestArrivalDepartureTransport()
		{
			Transport transport1 = Consol.Transports[0];

			AssertEquals(transport1, Consol.Transports.DepartureTransport);
			AssertEquals(transport1, Consol.Transports.ArrivalTransport);

			Transport transport2 = Consol.Transports.AddNew();
			AssertEquals(transport1, Consol.Transports.DepartureTransport);
			AssertEquals(transport2, Consol.Transports.ArrivalTransport);

			transport1.JW_LegOrder = 3;
			AssertEquals(transport2, Consol.Transports.DepartureTransport);
			AssertEquals(transport1, Consol.Transports.ArrivalTransport);

			transport1.Delete();
			AssertEquals(transport2, Consol.Transports.DepartureTransport);
			AssertEquals(transport2, Consol.Transports.ArrivalTransport);
		}

		#endregion

		#region TestImportExportTransport

		public void TestImportExportTransport()
		{
			Transport transport1 = Consol.Transports[0];
			Transport transport2 = Consol.Transports.AddNew();

			AssertNull(Consol.Transports.ExportTransport);
			AssertNull(Consol.Transports.ImportTransport);

			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort;
			transport2.JW_RL_NKLoadPort = OverseasPort;
			transport2.JW_RL_NKDiscPort = OverseasPort2;
			AssertEquals(transport1, Consol.Transports.ExportTransport);
			AssertNull(Consol.Transports.ImportTransport);

			transport1.JW_RL_NKLoadPort = OverseasPort3;
			transport2.JW_RL_NKDiscPort = HomePort;
			AssertNull(Consol.Transports.ExportTransport);
			AssertEquals(transport2, Consol.Transports.ImportTransport);

			transport1.JW_RL_NKLoadPort = AlternateHomePort;
			AssertEquals(transport1, Consol.Transports.ExportTransport);
			AssertEquals(transport2, Consol.Transports.ImportTransport);

			transport1.Delete();
			AssertNull(Consol.Transports.ExportTransport);
			AssertEquals(transport2, Consol.Transports.ImportTransport);

			transport2.Delete();
			AssertNull(Consol.Transports.ExportTransport);
			AssertNull(Consol.Transports.ImportTransport);
		}

		#endregion

		#region TestInitialTransportShouldNotBeValidated

		public void TestInitialTransportShouldNotBeValidatedExceptWhenEmptyETDAndETA()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals(2, consol.Transports[0].Notifications.GetWarnings().Count());
			AssertHasWarning(consol.Transports[0].JW_ETDInfo, "You have not entered an " + consol.Transports[0].JW_ETDInfo.Description + ".");
			AssertHasWarning(consol.Transports[0].JW_ETAInfo, "You have not entered an " + consol.Transports[0].JW_ETAInfo.Description + ".");
		}

		#endregion

		#region TestRemovingLastTransportAddsNewPrimary

		public void TestRemovingLastTransportAddsNewLinked()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("should have a transport already", 1, Consol.Transports.Count);

			Transport transport1;
			Transport transport2;

			transport1 = Consol.Transports[0];
			transport2 = Consol.Transports.AddNew();
			AssertEquals("Should have 2 transports", 2, Consol.Transports.Count);

			Consol.Transports.Remove(transport1);
			AssertEquals("Should have 1 transport", 1, Consol.Transports.Count);

			Consol.Transports.Remove(transport2);
			AssertEquals("Should have 1 transport", 1, Consol.Transports.Count);
			AssertEquals("The only remaining element should be linked", true, Consol.Transports[0].JW_IsLinked);

			transport1 = Consol.Transports[0];
			transport2 = Consol.Transports.AddNew();
			AssertEquals("Should have 2 transports", 2, Consol.Transports.Count);

			Consol.Transports.RemoveAll();
			AssertEquals("Should have 1 transport", 1, Consol.Transports.Count);
			AssertEquals("The only remaining element should be linked", true, Consol.Transports[0].JW_IsLinked);
		}

		#endregion

		#region TestDeletingLastTransportAddsNewPrimary

		public void TestDeletingLastTransportAddsNewPrimary()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("should have a transport already", 1, Consol.Transports.Count);

			Transport transport1;
			Transport transport2;

			transport1 = Consol.Transports[0];
			transport2 = Consol.Transports.AddNew();
			AssertEquals("Should have 2 transports", 2, Consol.Transports.Count);

			Consol.Transports.RemoveAndDelete(transport1);
			AssertEquals("Should have 1 transport", 1, Consol.Transports.Count);

			Consol.Transports.RemoveAndDelete(transport2);
			AssertEquals("Should have 1 transport", 1, Consol.Transports.Count);
			AssertEquals("The only remaining element should be the linked", true, Consol.Transports[0].JW_IsLinked);

			transport1 = Consol.Transports[0];
			transport2 = Consol.Transports.AddNew();
			AssertEquals("Should have 2 transports", 2, Consol.Transports.Count);

			Consol.Transports.RemoveAndDeleteAll();
			AssertEquals("Should have 1 transport", 1, Consol.Transports.Count);
			AssertEquals("The only remaining element should be the linked", true, Consol.Transports[0].JW_IsLinked);
		}

		#endregion

		#region TestSetDefaultsForNewChild

		public void TestSetDefaultsForNewChild_Air()
		{
			GenericSetDefaultsForNewChildTest(Constants.TransportModes.Air);
		}

		public void TestSetDefaultsForNewChild_Sea()
		{
			GenericSetDefaultsForNewChildTest(Constants.TransportModes.Sea);
		}

		void GenericSetDefaultsForNewChildTest(string transportMode)
		{
			Consol.JK_TransportMode = transportMode;
			Consol.Transports.RemoveAndDeleteAll();

			Transport transport1 = Consol.Transports[0];
			AssertEquals(true, transport1.JW_IsLinked);
			AssertEquals(transportMode, transport1.JW_TransportMode);

			Transport transport2 = Consol.Transports.AddNew();
			AssertEquals(false, transport2.JW_IsLinked);
			AssertEquals(transportMode, transport2.JW_TransportMode);
		}

		#endregion

		#region TestDontAddNewRowWhenParentIsBeingDeleted

		public void TestDontAddNewRowWhenParentIsBeingDeleted()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			AssertEquals("precondition: ", 1, consol.Transports.Count);
			consol.Delete();
			AssertEquals(0, consol.Transports.Count);
		}

		#endregion

		#region TestDefaultingLegOrder

		public void TestDefaultingLegOrder()
		{
			Transport transportA = Consol.Transports[0];
			AssertEquals("the first number issued should be 1", 1, (int)transportA.JW_LegOrder);

			Transport transportB = Consol.Transports.AddNew();
			AssertEquals("the second number issued should be 2", 2, (int)transportB.JW_LegOrder);

			Transport transportC = Consol.Transports.AddNew();
			AssertEquals("the third number issued should be 3", 3, (int)transportC.JW_LegOrder);

			transportB.Delete();
			transportB = Consol.Transports.AddNew();
			AssertEquals("dont fill gap's the new leg should always have the heighest value", 4, (int)transportB.JW_LegOrder);

			transportC.Delete();
			transportB.Delete();
			transportB = Consol.Transports.AddNew();
			AssertEquals("use the smallest value greater than all the rest, dont leave unnessisary gaps", 2, (int)transportB.JW_LegOrder);
		}

		#endregion

		#region FindByLoadPort / FindByDischargePort

		public void TestFindByLoadPort()
		{
			Transport transport1 = Consol.Transports.AddNew();
			Transport transport2 = Consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKLoadPort = "MYPKG";
			AssertEquals("FindByLoadPort", transport2, Consol.Transports.FindByLoadPort("MYPKG"));
		}

		public void TestFindByDischargePort()
		{
			Transport transport1 = Consol.Transports.AddNew();
			Transport transport2 = Consol.Transports.AddNew();
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "MYPKG";
			AssertEquals("FindByLoadPort", transport2, Consol.Transports.FindByDischargePort("MYPKG"));
		}

		#endregion

		public void TestCourierConsolTransportIsNotLinked()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_AgentType = Constants.AgentType.Courier;
			Consol.Transports.RemoveAndDeleteAll();

			var consolTransportCollection = new ConsolTransportCollection(Consol);
			consolTransportCollection.Load();

			AssertEquals(1, consolTransportCollection.Count);
			Assert(!consolTransportCollection[0].JW_IsLinked);
		}

		public void TestTransportOrderHelperNotCached()
		{
			var consolTransportCollection = new ConsolTransportCollection(Consol);
			var propertyGetter = consolTransportCollection.GetType().GetProperty("OrderHelper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetGetMethod(nonPublic: true);
			var orderHelper1 = propertyGetter.Invoke(consolTransportCollection, null);
			var orderHelper2 = propertyGetter.Invoke(consolTransportCollection, null);

			AssertNotEquals("TransportOrderHelper should not be cached", orderHelper1, orderHelper2);
		}

		#region Implementation

		CommonConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<CommonConsol>();
				}

				return fConsol;
			}
		}

		CommonConsol fConsol;

		#endregion
	}
}
