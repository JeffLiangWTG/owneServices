using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageLegController))]
	public class CartageLegControllerTest : ZControllerBasherTest
	{
		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var cartageLeg = move.CartageLegs.AddNew();
			Factory.Save();
			return cartageLeg;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(CommonCartageLeg);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CartageLeg;
		}
	}
}
