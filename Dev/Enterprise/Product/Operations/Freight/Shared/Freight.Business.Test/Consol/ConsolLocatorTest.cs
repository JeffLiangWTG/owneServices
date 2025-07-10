using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	public class ConsolLocatorTest : BaseFreightTest
	{
		public void TestFindWithAir_Exact()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Air);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Air, OverseasPort, HomePort, new ZDateTime(2005, 2, 1));
			AssertEquals("Should find the consol correctly", Consol.PK, found.PK);
		}

		public void TestFindWithAir_DateJustUnderAMonth()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Air);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Air, OverseasPort, HomePort, new ZDateTime(2005, 2, 28));
			AssertEquals("Should find the consol correctly", Consol.PK, found.PK);
		}

		public void TestFindWithAir_NoDate()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Air);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Air, OverseasPort, HomePort, ZDateTime.Invalid);
			AssertEquals("Should find the consol correctly", Consol.PK, found.PK);
		}

		public void TestFindWithAir_DateOutOfRange()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Air);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Air, "", "", new ZDateTime(2005, 3, 1));
			AssertEquals("Should not find the consol", null, found);
		}

		public void TestFindWithSea_Exact()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Sea);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Sea, OverseasPort, HomePort, new ZDateTime(2005, 2, 1));
			AssertEquals("Should find the consol correctly", Consol.PK, found.PK);
		}

		public void TestFindWithSea_DateNotExact()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Sea);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Sea, "", "", new ZDateTime(2005, 2, 2));
			AssertEquals("Should not find the consol", null, found);
		}

		public void TestFindWithSea_NoDate()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Sea);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Sea, OverseasPort, HomePort, ZDateTime.Invalid);
			AssertEquals("Should find the consol correctly", Consol.PK, found.PK);
		}

		public void TestFind_InvalidVessel()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Sea);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Sea, OverseasPort2, HomePort, ZDateTime.Invalid);
			AssertEquals("Should not find the consol", null, found);
		}

		public void TestFind_InvalidVoyage()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Sea);
			CommonConsol found = Locator.Find(Factory, "masterbill", "", Constants.TransportModes.Sea, OverseasPort, AlternateHomePort, ZDateTime.Invalid);
			AssertEquals("Should not find the consol", null, found);
		}

		public void TestFind_NoMasterBill()
		{
			SetTransportModeOnConsols(Constants.TransportModes.Sea);
			CommonConsol found = Locator.Find(Factory, "", "", Constants.TransportModes.Sea, OverseasPort, HomePort, ZDateTime.Invalid);
			AssertEquals("Should not find the consol", null, found);
		}

		public void TestFind_MasterBillWithAgentsReference()
		{
			// delete all the consols in the developer's machine for the test so we can ensure we're not matching on 1 'transport mode sea'.
			CommonConsol[] consolsToDelete = (CommonConsol[])Factory.Load(typeof(CommonConsol), new ZQuery());
			foreach (CommonConsol current in consolsToDelete)
			{
				current.Delete();
			}

			CommonConsol consol = GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2005, 2, 1);
			transport.JW_Vessel = "vessel";
			transport.JW_VoyageFlight = "voyage";

			consol.JK_AgentsReference = "splaty";
			CommonConsol notFound = Locator.Find(Factory, "", "agent", Constants.TransportModes.Sea, "", "", ZDateTime.Invalid);
			AssertEquals("Should not find the consol correctly as the agent doesnt exist", null, notFound);

			consol.JK_MasterBillNum = "master";
			consol.JK_AgentsReference = "agent";
			CommonConsol found = Locator.Find(Factory, "", "agent", Constants.TransportModes.Sea, "", "", ZDateTime.Invalid);
			AssertEquals("Should find the consol correctly", consol.PK, found.PK);

			notFound = Locator.Find(Factory, "othermaster", "agent", Constants.TransportModes.Sea, "", "", ZDateTime.Invalid);
			AssertEquals("Should not find the consol correctly as the agent exist but master bill is not empty", null, notFound);
		}

		public void TestFind_MasterBillOnly()
		{
			CommonConsol consol = GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "forme";

			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2005, 2, 1);
			transport.JW_Vessel = "vessel";
			transport.JW_VoyageFlight = "voyage";

			CommonConsol found = Locator.Find(Factory, "forme", "", "", "", "", ZDateTime.Invalid);
			AssertEquals("Should find the consol correctly", consol.PK, found.PK);
		}

		public void TestFind_AgentsReferenceOnly()
		{
			// delete all the consols in the developer's machine for the test so we can ensure we're not matching on 1 'transport mode sea'.
			CommonConsol[] consolsToDelete = Factory.Load<CommonConsol>(new ZQuery());
			foreach (CommonConsol current in consolsToDelete)
			{
				current.Delete();
			}

			CommonConsol consol = GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2005, 2, 1);
			transport.JW_Vessel = "vessel";
			transport.JW_VoyageFlight = "voyage";

			consol.JK_AgentsReference = "agent";
			CommonConsol found = Locator.Find(Factory, "othermaster", "agent", Constants.TransportModes.Sea, "", "", ZDateTime.Invalid);
			AssertEquals("Should find the consol correctly", consol.PK, found.PK);
		}

		public void TestFind_MawbRecycleDateRange()
		{
			var consol = GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(1 - FreightDataRegistry.Instance.MAWBRecyclePeriod.Value);
			consol.JK_MasterBillNum = "master";
			consol.JK_AgentsReference = "agent";

			var found = Locator.Find(Factory, "", "agent", Constants.TransportModes.Air, "", "", ZDateTime.Invalid);
			AssertEquals("Should find the consol correctly", consol.PK, found.PK);

			consol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-1 - FreightDataRegistry.Instance.MAWBRecyclePeriod.Value);

			var notFound = Locator.Find(Factory, "", "agent", Constants.TransportModes.Air, "", "", ZDateTime.Invalid);
			AssertEquals("Should not find the consol", null, notFound);
		}

		public void TestEnsureGetConsolsReturnsCorrectBizOType()
		{
			_ = (CommonConsol)Factory.New(ExpectedConsolType);
			var locator = new ConsolLoacatorForTest();
			var consols = locator.GetConsols(Factory, new ZQuery());

			AssertGreaterThan("Consols should have at least one consol", consols.Length, 1);

			foreach (var consol in consols)
			{
				AssertEquals("Consols' element type should be ExpectedConsolType", ExpectedConsolType, consol.GetType());
			}
		}

		CommonConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = GetNewConsol();
				}
				return fConsol;
			}
		}
		CommonConsol fConsol;

		ConsolLocator<CommonConsol> Locator
		{
			get
			{
				if (fLocator == null)
				{
					fLocator = GetNewConsolLocator();
				}
				return fLocator;
			}
		}
		ConsolLocator<CommonConsol> fLocator;

		CommonConsol GetNewConsol()
		{
			return (CommonConsol)Factory.New(ExpectedConsolType);
		}

		protected virtual ConsolLocator<CommonConsol> GetNewConsolLocator()
		{
			return new ConsolLocator<CommonConsol>();
		}

		protected override void SetUp()
		{
			base.SetUp();

			((IBusinessObjectInternals)this.Consol).IsCopying = true;

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = "masterbill";
			Consol.JK_RL_NKLoadPort = OverseasPort;
			Consol.JK_RL_NKDischargePort = HomePort;

			Transport transport = Consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2005, 2, 1);
			transport.JW_Vessel = "vessel";
			transport.JW_VoyageFlight = "voyage";

			CommonConsol decoyConsol1 = GetNewConsol();
			((IBusinessObjectInternals)decoyConsol1).IsCopying = true;
			decoyConsol1.JK_TransportMode = Constants.TransportModes.Sea;
			decoyConsol1.JK_MasterBillNum = "";
			decoyConsol1.JK_RL_NKLoadPort = OverseasPort2;
			decoyConsol1.JK_RL_NKDischargePort = AlternateHomePort;

			Transport decoyTransport1 = decoyConsol1.Transports[0];
			decoyTransport1.JW_ETD = new ZDateTime(2003, 2, 1);
			decoyTransport1.JW_Vessel = "splaty";
			decoyTransport1.JW_VoyageFlight = "splaty";

			CommonConsol decoyConsol2 = GetNewConsol();
			((IBusinessObjectInternals)decoyConsol2).IsCopying = true;

			CommonConsol decoyConsol3 = GetNewConsol();
			((IBusinessObjectInternals)decoyConsol3).IsCopying = true;
			decoyConsol1.JK_MasterBillNum = "splaty";

			CommonConsol decoyConsol4 = GetNewConsol();
			((IBusinessObjectInternals)decoyConsol4).IsCopying = true;
			decoyConsol1.JK_MasterBillNum = "masterbill";
		}

		void SetTransportModeOnConsols(ZString transportMode)
		{
			CommonConsol[] consols = (CommonConsol[])Factory.Load(typeof(CommonConsol), new ZQuery());
			foreach (CommonConsol next in consols)
			{
				string vessel = next.JK_JX_JV_NKVessel;
				string voyage = next.JK_JX_JV_VoyageFlight;
				string masterBill = next.JK_MasterBillNum;
				next.JK_TransportMode = transportMode;
				next.JK_MasterBillNum = masterBill;

				Transport transport = next.Transports[0];
				transport.JW_Vessel = vessel;
				transport.JW_VoyageFlight = voyage;
			}
		}

		protected virtual Type ExpectedConsolType
		{
			get { return typeof(CommonConsol); }
		}
	}
}
