using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MostInterestingTransportBindingCollectionContractsValidationTest : BaseFreightTest
	{
		CommonConsol consol;

		void AssertActionTriggersContractsValidation(string message, Action<CommonConsol> fn)
		{
			var factory = new BusinessObjectFactory();
			var reloadedConsol = factory.Load<CommonConsol>(consol.PK);

			var consolMarkedForValidation = false;
			var containerMarkedForValidation = false;

			factory.MarkedAsNeedingValidation += (BusinessObject obj) =>
			{
				if (obj is CommonConsol)
				{
					consolMarkedForValidation = true;
				}
				else if (obj is CommonContainer)
				{
					containerMarkedForValidation = true;
				}
			};

			AssertNotNull(reloadedConsol.MostInterestingTransportForBinding[0]);
			// AssertNotNull(reloadedConsol.Containers);
			fn(reloadedConsol);
			var bothMarkedForValidation = containerMarkedForValidation && consolMarkedForValidation;
			AssertEquals($"container/consol marked for validation: {message}", true, bothMarkedForValidation);
		}

		public void TestConsolAndChildrenAreMarkedAsNeedingValidation()
		{
			CombineAssertions(() =>
			{
				AssertActionTriggersContractsValidation("JW_Vessel", consol => consol.Transports[0].JW_Vessel = "changed");
				AssertActionTriggersContractsValidation("JW_VoyageFlight", consol => consol.Transports[0].JW_VoyageFlight = "newFlight");
				AssertActionTriggersContractsValidation("JW_ETD", consol => consol.Transports[0].JW_ETD = new ZDateTime(2000, 1, 1));
				AssertActionTriggersContractsValidation("JW_RL_NKLoadPort", consol => consol.Transports[0].JW_RL_NKLoadPort = "NZAKL");
				AssertActionTriggersContractsValidation("JW_RL_NKDiscPort", consol => consol.Transports[0].JW_RL_NKDiscPort = "NZAKL");
				AssertActionTriggersContractsValidation("most interesting transport deleted", consol => consol.Transports[0].Delete());
				AssertActionTriggersContractsValidation("other transport becomes most interesting", consol => consol.Transports[1].JW_LegOrder = 1);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.Containers.AddNew();

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKDiscPort = HomePort;
			transport1.JW_RL_NKLoadPort = AlternateHomePort;
			transport1.JW_LegOrder = 2;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKDiscPort = HomePort;
			transport2.JW_RL_NKLoadPort = AlternateHomePort2;
			transport2.JW_LegOrder = 3;

			AssertEquals(consol.MostInterestingTransportForBinding[0], transport1);
			Factory.Save();
		}
	}
}
