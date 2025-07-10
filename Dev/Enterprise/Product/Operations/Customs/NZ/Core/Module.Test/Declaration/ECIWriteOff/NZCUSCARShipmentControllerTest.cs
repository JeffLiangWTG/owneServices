using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.Testing
{
	[TestedType(typeof(NZCUSCARShipmentController))]
	sealed class NZCUSCARShipmentControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get
			{
				return "NZ";
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.NZ.CUSCARPluggedIntoShipment;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			return declaration;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}
	}
}
