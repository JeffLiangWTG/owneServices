namespace Enterprise.Freight.Business.Testing
{
	sealed class MostInterestingTransportBindingCollectionTest : BaseFreightTest
	{
		#region TestSelectOnCreate

		public void TestSelectOnCreate()
		{
			Transport transport1 = Consol.Transports[0];
			Transport transport2 = Consol.Transports.AddNew();
			Transport transport3 = Consol.Transports.AddNew();

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort2;

			transport2.JW_RL_NKDiscPort = HomePort;
			transport2.JW_RL_NKLoadPort = AlternateHomePort;
			transport2.JW_LegOrder = 1;

			transport3.JW_RL_NKLoadPort = AlternateHomePort;
			transport3.JW_RL_NKDiscPort = OverseasPort;
			transport3.JW_LegOrder = 2;

			transport1.JW_RL_NKLoadPort = OverseasPort;
			transport1.JW_RL_NKDiscPort = OverseasPort2;
			transport1.JW_LegOrder = 3;

			collection = null;
			AssertSelectedTransport("Should have selected transport2", transport2);
		}

		#endregion

		#region TestUpdateOnAdd

		public void TestUpdateOnAdd()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport1 = Consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
			AssertSelectedTransport("precondition: There is only one transport to choose", transport1);

			Transport transport2 = Consol.Transports.AddNew();
			AssertSelectedTransport("A transport with matching transport mode takes precidence", transport2);
		}

		#endregion

		#region TestUpdateOnRemove

		public void TestUpdateOnRemove()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport1 = Consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;

			AssertSelectedTransport("precondition: should have selected transport2", transport2);

			Consol.Transports.Remove(transport2);
			AssertSelectedTransport("Should have updated selection", transport1);
		}

		#endregion

		#region TestUpdateOnDelete

		public void TestUpdateOnDelete()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport1 = Consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;

			AssertSelectedTransport("precondition: should have selected transport2", transport2);

			transport2.Delete();
			AssertSelectedTransport("Should have updated selection", transport1);
		}

		#endregion

		#region TestUpdateOnUpdateConsol

		public void TestUpdateOnUpdateConsol()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			Consol.JK_RL_NKLoadPort = OverseasPort;
			Consol.JK_RL_NKDischargePort = OverseasPort2;

			Transport transport0 = Consol.Transports[0];
			transport0.JW_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport1 = Consol.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Road;

			Transport transport3 = Consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;

			AssertSelectedTransport("Should select the first road transport (transport1)", transport1);

			Consol.JK_RL_NKDischargePort = AlternateHomePort;
			AssertSelectedTransport("Should select the last road transport (transport2)", transport2);

			Consol.JK_RL_NKLoadPort = HomePort;
			AssertSelectedTransport("Should select the first road transport (transport1)", transport1);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertSelectedTransport("Should select first transport (transport0)", transport0);

			Consol.JK_RL_NKLoadPort = OverseasPort;
			AssertSelectedTransport("Should select the last transport (transport3)", transport3);
		}

		#endregion

		#region TestUpdateOnUpdateTransport

		public void TestUpdateOnUpdateTransport()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport1 = Consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;

			AssertSelectedTransport("precondition: should select transport2", transport2);

			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertSelectedTransport("should select transport1 now that it is sea", transport1);

			transport1.JW_LegOrder = 5;
			AssertSelectedTransport("transport1 is no longer the first leg so select transport2", transport2);
		}

		#endregion

		#region Implementation

		#region Consol

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
				}
				return consol;
			}
		}

		CommonConsol consol;

		#endregion

		#region Collection

		MostInterestingTransportBindingCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new MostInterestingTransportBindingCollection(Consol);
				}

				return collection;
			}
		}

		MostInterestingTransportBindingCollection collection;

		#endregion

		#region AssertSelectedTransport

		void AssertSelectedTransport(string message, Transport expectedTransport)
		{
			AssertEquals(message + ": Count", 1, Collection.Count);
			AssertEquals(message, expectedTransport.PK, Collection[0].PK);
		}

		#endregion

		#endregion
	}
}
