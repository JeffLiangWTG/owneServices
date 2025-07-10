using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class JobShipmentPreplanningEventModelTest : TestCaseWithFactory
	{
		#region Origin

		public void TestOriginGetter_ReturnOriginPortFromShipment()
		{
			var shipment = GetShipmentInstance();
			shipment.EF_RL_NKPortLoad = "UAIEV";

			var model = new JobShipmentPreplanningEventModel(shipment);

			AssertEquals("Property value", "UAIEV", model.Origin);
		}

		#endregion

		#region Destination

		public void TestDestinationGetter_ReturnDestinationPortFromShipment()
		{
			var shipment = GetShipmentInstance();
			shipment.EF_RL_NKPortDisch = "UAIEV";

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Property value", "UAIEV", model.Destination);
		}

		#endregion

		#region FirstLeg

		public void TestFirstLegGetter_SomeLegsExist_ReturnModelForShipmentFirstLeg()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			var transport1 = shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.PreAdviceTransports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.PreAdviceTransports.New(from: "USNYC", to: "UAIEV");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", transport1, model.FirstLeg.Parent_DebugOnly);
		}

		public void TestFirstLegGetter_NoLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			shipment.PreAdviceTransports.RemoveAndDeleteAll();

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Model", null, model.FirstLeg);
		}

		#endregion

		#region SecondLeg

		public void TestSecondLegGetter_MoreThanTwoLegsExist_ReturnModelForShipmentSecondLeg()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			var transport1 = shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.PreAdviceTransports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.PreAdviceTransports.New(from: "USNYC", to: "UAIEV");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", transport2, model.SecondLeg.Parent_DebugOnly);
		}

		public void TestSecondLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", null, model.SecondLeg);
		}

		#endregion

		#region ThirdLeg

		public void TestThirdLegGetter_MoreThanThreeLegsExist_ReturnModelForShipmentThirdLeg()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			var transport1 = shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.PreAdviceTransports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.PreAdviceTransports.New(from: "USNYC", to: "UAIEV");
			var transport4 = shipment.PreAdviceTransports.New(from: "UAIEV", to: "AUMEL");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", transport3, model.ThirdLeg.Parent_DebugOnly);
		}

		public void TestThirdLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			shipment.PreAdviceTransports.New(from: "USLAX", to: "USNYC");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", null, model.ThirdLeg);
		}

		#endregion

		#region FourthLeg

		public void TestFourthLegGetter_MoreThanFourLegsExist_ReturnModelForShipmentFourthLeg()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			var transport1 = shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.PreAdviceTransports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.PreAdviceTransports.New(from: "USNYC", to: "UAIEV");
			var transport4 = shipment.PreAdviceTransports.New(from: "UAIEV", to: "AUMEL");
			var transport5 = shipment.PreAdviceTransports.New(from: "AUMEL", to: "NZAKL");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", transport4, model.FourthLeg.Parent_DebugOnly);
		}

		public void TestFourthLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			shipment.PreAdviceTransports.New(from: "USLAX", to: "USNYC");
			shipment.PreAdviceTransports.New(from: "USNYC", to: "UAIEV");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", null, model.FourthLeg);
		}

		#endregion

		#region LastLeg

		public void TestLastLegGetter_SomeLegsExist_ReturnModelForShipmentLastLeg()
		{
			var shipment = GetShipmentInstance();
			var defaultLeg = shipment.PreAdviceTransports.FirstOrDefault();
			var transport1 = shipment.PreAdviceTransports.New(from: "AUSYD", to: "USLAX");
			var transport2 = shipment.PreAdviceTransports.New(from: "USLAX", to: "USNYC");
			var transport3 = shipment.PreAdviceTransports.New(from: "USNYC", to: "UAIEV");
			DeleteIfExist(defaultLeg);

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Wrapped transport", transport3, model.LastLeg.Parent_DebugOnly);
		}

		public void TestLastLegGetter_NoLegsExist_ReturnNull()
		{
			var shipment = GetShipmentInstance();
			shipment.PreAdviceTransports.RemoveAndDeleteAll();

			var model = new JobShipmentPreplanningEventModel(shipment);
			AssertEquals("Model", null, model.LastLeg);
		}

		#endregion

		JobShipmentPreplanning GetShipmentInstance()
		{
			var shipment = Factory.New<JobShipmentPreplanning>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			shipment.BuyerPK = orgHeader.PK;

			return shipment;
		}

		static void DeleteIfExist(BusinessObject businessObject)
		{
			businessObject.Delete();
		}
	}
}
